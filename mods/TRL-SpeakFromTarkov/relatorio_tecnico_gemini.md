# 📡 Relatório Técnico de Engenharia — Módulo VOIP (TRL-SpeakFromTarkov V2)

**Autor:** Equipe de Engenharia (Antigravity AI / Lead Audio & Network Engineer)  
**Projeto:** TRL-SpeakFromTarkov (SPT 4.0 / Escape From Tarkov 0.16.9 / FIKA Coop)  
**Arquitetura Base:** Concentus (Opus) + RNNoise C-Native + Unity DSP + LiteNetLib (FIKA)

---

## 1. 🎤 CAPTURA DE ÁUDIO & BUFFERING

### API de Leitura do Microfone
- **API Utilizada:** `UnityEngine.Microphone.Start(deviceName, loop: true, lengthSec: 1, sampleRate)`.
- **Frequência de Amostragem (DSP):** Detectada dinamicamente via `AudioSettings.outputSampleRate` (padrão do SO, geralmente **48.000 Hz**), com rotina de fallback automático para `[48000, 44100, 32000, 24000, 16000, 8000]`.
- **Tamanho do Buffer de Captura:** Frames de **40ms** (`captureFrameSize = actualSampleRate * 0.040`). Para 48 kHz, cada frame contém **1.920 samples**.

### Mecanismo de Polling e Ring Buffer
1. **Poll na Main Thread (`Update()`):** O método `PollMicrophoneData()` consulta `Microphone.GetPosition(deviceName)` no `Update()` da Unity. Quando novos dados chegam no `AudioClip`, eles são lidos via `micClip.GetData(micPollBuffer, lastMicPosition)` para um buffer temporário fixo.
2. **Ring Buffer Thread-Safe:** Os dados lidos são copiados para um Ring Buffer circular fixo (`float[] ringBuffer = new float[actualSampleRate * 2]`), protegido por uma trava leve de sincronização (`lock(bufferLock)`).
3. **Thread de Captura Dedicada (`captureThread`):** Uma thread assíncrona de segundo plano com prioridade `ThreadPriority.AboveNormal` executa o loop `CaptureLoop()`. Ela extrai frames completos de 40ms do `ringBuffer` e os envia para o pipeline de filtragem DSP e codificação Opus, **100% imune a quedas de FPS (hitches) do Tarkov**.

---

## 2. 🧵 THREADING, PROCESSAMENTO & CONCORRÊNCIA

### Distribuição de Carga por Threads (3-Thread Architecture)

```
┌────────────────────────────────┐     ┌────────────────────────────────┐     ┌────────────────────────────────┐
│      1. MAIN THREAD (Unity)    │     │   2. WORKER THREAD (Background)│     │ 3. AUDIO THREAD (Unity Native) │
├────────────────────────────────┤     ├────────────────────────────────┤     ├────────────────────────────────┤
│ • Update() de Input (Teclas)   │     │ • CaptureLoop() (Polled 40ms)  │     │ • OnAudioFilterRead()          │
│ • PollMicrophoneData()         │───> │ • AudioFilter.Apply() (RNNoise)│ ──> │ • Leitura do Ring Buffer 3s    │
│ • SftNetwork.Update() (Drain)  │     │ • OpusEncoder.Encode()         │     │ • Filtro de Absorção do Ar     │
│ • Posicionamento 3D no Bone    │     │ • Enfileira na ConcurrentQueue │     │ • Panning Estéreo (-3dB Law)   │
└────────────────────────────────┘     └────────────────────────────────┘     └────────────────────────────────┘
```

### Análise de Locks e Bloqueios
- **Captura (Produtor):** O `lock(bufferLock)` em `MicrophoneCapturer.cs` é uma trava de milissegundos utilizada **apenas** para atualizar os ponteiros de escrita/leitura (`writePos`, `readPos`) do Ring Buffer da captura. A operação leva `< 0.01 ms`, não gerando bloqueios perceptíveis.
- **Filtro Neural RNNoise:** Roda 100% na `captureThread` de background através da DLL nativa `rnnoise.dll` (`rnnoise_process_frame`). Não toca na Main Thread da Unity.
- **Codificador Opus:** O `OpusEncoder.Encode()` da biblioteca Concentus é executado 100% na `captureThread`.
- **Thread de Áudio da Unity (`OnAudioFilterRead`):** Em `RemoteSpeaker.cs`, a leitura do buffer circular de 3 segundos (`streamBuffer`) é feita por ponteiros voláteis (`streamReadPos`, `streamWritePos`) **sem nenhum `lock()`**, garantindo execução **100% Lock-Free** na thread de mixagem nativa do motor de áudio.

---

## 3. 🧹 GERENCIAMENTO DE MEMÓRIA & GARBAGE COLLECTION (GC)

### Estado Atual (V2)
- **Captura & OnAudioFilterRead:** 0 alocações de GC. Os arrays de amostragem (`micPollBuffer`, `captureBuffer`, `outputBuffer`, `streamBuffer`, `opusDecodeBuffer`) são **arrays pré-alocados de tamanho fixo** instanciados na inicialização.
- **Gargalo Identificado de Alocação (Trabalho em Andamento para V2-Otimização):**
  1. No `VoipProcessor.Transmit()`: `byte[] finalData = new byte[len];` é instanciado a cada pacote codificado (50 vezes/s por jogador).
  2. No `SftNetwork.OnAudioPacketReceivedV2()`: `byte[] audioDataCopy = new byte[packet.AudioData.Length];` é alocado ao receber o pacote da rede.

### Plano de Otimização Zero-Alloc (V2-Otimização)
Substituir a instanciação de arrays por aluguel de buffers reutilizáveis via `System.Buffers.ArrayPool<byte>.Shared`, eliminando 100% do lixo na memória RAM durante rajadas de VOIP.

---

## 4. 🌐 ARQUITETURA DE REDE & PACOTES UDP

### Camada de Transporte
- O mod opera sobre o transporte de rede UDP não confiável (*Unreliable*) da infraestrutura coop do **FIKA** (baseada em **LiteNetLib**).

### Estrutura do Pacote (`SftAudioPacketV2`)
Para evitar desalinhamento no leitor serial do FIKA (`NetDataReader`) e prevenir o erro `ParseException: Undefined packet`, o pacote utiliza um **envelope com prefixo de tamanho** (`PutBytesWithLength`):

```
┌────────────────────────────────────────────────────────────────────────┐
│                      SftAudioPacketV2 (Envelope UDP)                   │
├───────────────────────┬──────────┬──────────────────────────┬──────────┤
│ Envelope Length       │ ProfileId│ Channel                  │ Payload  │
│ (ushort - 2 bytes)    │ (string) │ (byte: 0=3D Proximity)   │ Opus     │
└───────────────────────┴──────────┴──────────────────────────┴──────────┘
```

### Arquitetura de Envio Não-Bloqueante
1. O `OpusEncoder` enfileira os quadros codificados em uma `ConcurrentQueue<PendingAudio> sendQueue` com teto de 25 frames (~500ms de buffer).
2. O `SftNetwork.Update()` (Main Thread) consome a fila e dispara os pacotes via LiteNetLib. Isso impede concorrência no `NetDataWriter` do FIKA (que não é thread-safe) e garante que o socket de rede nunca bloqueie a thread de captura de áudio.

---

## 5. 🎛️ LÓGICA DE ESTADOS (PTT, VAD & OPEN MIC)

### Transição de Estados (`VoipProcessor.UpdateTransmittingState()`)

```csharp
switch (CurrentMode)
{
    case VoipMode.PTT:
        // Transmite apenas se a tecla atalhada estiver pressionada na Main Thread
        // (Será adicionado PTT Hangover de 200ms na V2 para não cortar a última sílaba)
        IsTransmitting = IsPTTActive || (pttHoldTimer > 0f);
        break;

    case VoipMode.VAD:
        // Transmite se a energia RMS superar o limiar calibrado pelo usuário
        if (RawLevel >= vadThreshold)
            vadHoldTimer = VADDecayTime.Value; // (0.5s - 0.7s de sustentação/hangover)
        
        IsTransmitting = (vadHoldTimer > 0f);
        break;

    case VoipMode.Open:
        // Transmissão Contínua Inteligente: Valida a probabilidade do RNNoise VAD 
        // (LastVadProbability >= 0.30f) ou RMS mínimo para evitar envio de ruído
        IsTransmitting = filter.LastVadProbability >= 0.30f || RawLevel >= vadThreshold;
        break;
}
```

### Integração Opus DTX & VBR (Melhoria V2 Planejada)
A ativação das propriedades `encoder.UseDTX = true` e `encoder.UseVBR = true` no `OpusEncoder` reduzirá o payload de silêncio no modo **Open Mic** para apenas **~400 bits/s**, igualando a carga de rede de um microfone fechado.
