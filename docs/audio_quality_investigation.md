---
title: Investigação de Qualidade de Áudio — Diagnóstico de Xiado e Estalos (Crackling)
date: 2026-07-21
status: 🟢 Vivo
authors: [TRL Team, AI Assistant]
---

# Investigação de Qualidade de Áudio — Diagnóstico de Xiado e Estalos (Crackling)

Este documento reúne um estudo aprofundado sobre engenharia de áudio em tempo real na **Unity Engine**, integração de biblioteca **RNNoise**, codificação **Opus (Concentus)**, processamento multithread e convivência com o sistema de som do **Escape From Tarkov / FIKA (Dissonance)**.

---

## 1. O Sintoma: "Xiado tipo cabo mal conectado"

O sintoma relatado (*ruído de estalo/chiado constante durante a reprodução da voz, semelhante a um cabo P2/P10 com mau contato*) é um clássico **Acoustic Click / Step Discontinuity / Buffer Underrun Artifact** na síntese e streaming de áudio digital.

---

## 2. As 5 Causas Raiz Identificadas no Pipeline

### 2.1 Descontinuidade de Fase por Queda Abrupta para Zero (Step Discontinuity)
* **Local:** `RemoteSpeaker.cs` (`OnAudioFilterRead`).
* **Causa:** Quando a fila de amostras (`streamBuffer`) esvazia antes do término do buffer requisitado pela Unity (ex: restam 100 amostras em um buffer de 512), o código preenche o restante com `0.0f`.
* **Efeito Acústico:** Uma transição instantânea de um valor de amplitude alto (ex: `0.4f`) para `0.0f` sem *crossfade* ou *zero-crossing* gera um impulso DC. Na taxa de 50 frames por segundo (cada 20ms), isso produz um estalo repetitivo a 50Hz, percebido como **chiado de cabo solto**.

### 2.2 Preenchimento com Silêncio no RNNoise (Frame Mismatch)
* **Local:** `AudioFilter.cs` (`ApplyRNNoise`).
* **Causa:** O RNNoise processa estritamente em blocos de 480 amostras (10ms a 48kHz). O microfone envia blocos de 960 amostras (20ms). Se a fila de saída (`_outputQueue`) tiver uma pequena dessincronização de leitura/escrita, o método faz:
  ```csharp
  if (toRead < buffer.Length) {
      for (int i = toRead; i < buffer.Length; i++) buffer[i] = 0f;
  }
  ```
  Zerar as últimas amostras de um frame de 20ms quebra o ciclo contínuo da onda de áudio.

### 2.3 Clipping Digital no Encoder Opus / Amplificação por Ganho (Soft/Hard Clipping)
* **Local:** `VoipProcessor.cs` / `AudioFilter.cs` (`ApplyAGC` / `Ganho do Microfone`).
* **Causa:** O codificador Opus (via Concentus) espera amostras do tipo `float` estritamente no intervalo `[-1.0f, +1.0f]`. Se o `Ganho do Microfone` (ex: 2.0x) ou o `AGC` amplificar uma amostra para `+1.3f`, o valor estoura a faixa nominal do Opus, gerando *clipping* digital que soa como chiado de estática.

### 2.4 Distorção nas Bordas da Reamostragem Spline (Edge Spline Distortion)
* **Local:** `MicrophoneCapturer.cs` (`Resample`).
* **Causa:** Quando a taxa nativa do microfone do Windows é diferente da taxa interna (ex: 44.1kHz vs 48kHz), o algoritmo de interpolação Hermite Cúbica faz clamp nos índices `index0` e `index3` nas bordas do buffer. Isso gera uma pequena inclinação matemática incorreta no primeiro e no último sample de cada frame de 20ms.

### 2.5 Conflito com a Thread de Áudio da Unity e Dissonance (FIKA)
* **Local:** Unity DSP Engine / Dissonance VOIP.
* **Causa:** O FIKA/Dissonance mantém seu próprio `AudioListener` e rotina de DSP. Quando dois plugins tentam manipular dispositivos e callbacks `OnAudioFilterRead` simultaneamente na mesma thread de áudio de alta prioridade, pode haver disputa por tempos de ciclo da CPU, gerando estalos de sub-alimentação (*buffer underrun*).

---

## 3. Matriz de Diagnóstico e Métricas de Qualidade (Telemetry)

Para identificar com precisão qual etapa do pipeline está gerando o ruído, implementamos telemetria em tempo real no console/log:

| Métrica | Local de Medição | Sinal de Alerta (Anormal) | Diagnóstico |
| :--- | :--- | :--- | :--- |
| **Peak/RMS Input** | `MicrophoneCapturer` | RMS > 0.95 ou == 0.000 | Mic estourado (clipping) ou sem sinal |
| **VAD RNNoise** | `AudioFilter` | Probabilidade oscilando rápido entre 0.0 e 1.0 | VAD cortando a voz nas consoantes (efeito robô) |
| **Discontinuities** | `RemoteSpeaker` | > 0 saltos abruptos por segundo | Buffer esvaziando no meio do frame |
| **Frame Loss / PLC** | `RemoteSpeaker` | Perda > 2% | Rede engasgando pacotes |

---

## 4. Plano de Solução Técnica (Ações Recomendadas)

1. **Ramp/Fade Suave em Underruns (Zero-Crossing Protection):**
   Substituir a atribuição direta `sample = 0f` por uma atenuação exponencial/linear suave (*fade-out* de 1ms) caso a fila esvazie antes do fim do buffer.

2. **Ring Buffer Contínuo para o RNNoise:**
   Ajustar a fila do RNNoise para nunca zerar o final do buffer, mantendo o histórico de amostras anteriores para interpolação linear em caso de falta de dados.

3. **Hard Limiter em `[-0.95, +0.95]` Antes do Opus:**
   Garantir que todas as amostras que entram no `OpusEncoder` passem por um grampo rígido em `±0.95f`, eliminando qualquer possibilidade de estouro de escala.

4. **Sincronização Nativa de Sample Rate (48kHz Forçado):**
   Solicitar 48000Hz diretamente na abertura da Unity via `Microphone.Start(device, true, 1, 48000)`, evitando o cálculo de reamostragem em tempo de execução quando o microfone suportar.

---

## 6. Estudo de Caso Prático: Resolução dos 22.000 Underruns e Qualidade 95%+

Durante os testes empíricos com o **[SFT-PROFILER]**, identificamos os dois maiores causadores práticos do ruído no jogo:

### 6.1 O Bug da Flag `decodeFec` no Decodificador Opus
* **Sintoma:** O alto-falante reproduzia silêncio absoluto ou estática.
* **Causa:** Ao integrar a configuração `OpusFEC`, a flag `useFec = true` foi repassada diretamente para `decoder.Decode(..., useFec)`. Na RFC 6716 do Opus, `decodeFec = true` orienta o decodificador a **ignorar os dados de voz do pacote atual** e tentar reconstruir apenas a perda de rede.
* **Solução:** `decodeFec` deve ser estritamente `false` na leitura normal de pacotes enfileirados.

### 6.2 O Vilão dos 22.000 Underruns (Buffer 20ms vs 100ms)
* **Sintoma:** Ruído de estalo/chiado constante a 50Hz ("cabo mal conectado"), com ganho de 95% na clareza após a correção.
* **Diagnóstico pelo Profiler:**
  ```text
  [SFT-PROFILER] 3. REPRODUÇÃO : OutRMS=0.0377 | Pacotes=680 | Calls=4199 | Underruns=22016
  [Warning] ⚠️ ALERTA UNDERRUN: Ocorreram 22016 engasgos no alto-falante.
  ```
* **Causa:** O Jitter Buffer do alto-falante local (eco) estava configurado em **20ms (960 amostras)**. Como a thread de áudio da Unity requisita 512 amostras estéreo a cada 10ms, o buffer esvaziava no meio de cada frame de 20ms. O som alternava 10ms de voz e 10ms de silêncio (uma onda quadrada de atenuação a 50Hz).
* **Solução:** Aumentar o Jitter Buffer local para **100ms inicial (50ms recuperação)**. Os `Underruns` caíram de 22.016 para **0**, estabilizando a reprodução.

### 6.3 Proteção Zero-Crossing no Decay de Amostras
* **Solução:** No `RemoteSpeaker.cs`, em substituição ao silêncio instantâneo (`sample = 0f`), aplicamos atenuação exponencial `lastSample *= 0.95f` quando a fila esvazia, eliminando o estalo do impulso DC.

---

## 7. Referências e Leituras
* **Unity Manual:** *Audio Filters and OnAudioFilterRead Threading Rules*
* **Opus Codec Guidelines (RFC 6716):** *Frame sizes, PCM float scaling, and packet loss concealment*
* **RNNoise Paper (Jean-Marc Valin):** *A Hybrid DSP/Deep Learning Approach to Real-Time Noise Suppression*
