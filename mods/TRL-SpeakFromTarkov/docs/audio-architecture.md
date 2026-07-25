---
title: "SFT Audio Architecture & Jitter Mastery"
date: "2026-07-20"
status: "🟢 Vivo"
authors: ["AI Assistant"]
---

# TRL-SpeakFromTarkov: Arquitetura de Áudio e Jitter

Este documento registra a causa do problema de áudio "cabo com mal contato" (stuttering severo e dropouts) e a arquitetura adotada para garantir voz perfeitamente limpa e imune a quedas de FPS no SPT.

## O Problema: Por que a voz falhava?

O problema antigo nascia de dois gargalos arquiteturais trabalhando contra o jogador:

1. **Captura amarrada ao FPS (Update Loop)**
   Anteriormente, o `MicrophoneCapturer` drenava o buffer do microfone da Unity e acionava o encode (Opus + RNNoise) dentro do `Update()`.
   **O Efeito:** Como Tarkov tem frametimes instáveis (stutters visuais pesados), o `Update()` parava de rodar. O microfone do jogador parava de ser capturado e, 100ms depois, todos os pacotes eram encavalados e enviados de uma vez para a rede.

2. **Jitter Buffer Intolerante (RemoteSpeaker)**
   O ouvinte recebia os pacotes pela rede e tentava reproduzir. Se um único pacote atrasasse pela rede (ou pelo engasgo do *Sender* citado acima), a fila (`availableSamples`) chegava a zero.
   O código antigo ativava `isBuffering = true` e obrigava a placa de som a ficar muda por **150ms completos** até encher o buffer novamente.
   **O Efeito:** Qualquer atrasinho mínimo na rede gerava um buraco de silêncio de 150ms. A voz picotava como se o cabo do microfone estivesse com mal contato.

## A Solução (Segredo da Qualidade)

A arquitetura atual resolve isso atacando ambas as frentes:

### 1. Desacoplamento Thread-Safe (`CaptureLoop`)
A drenagem do microfone e aplicação de filtros foi isolada em uma `System.Threading.Thread` pura.
O loop dorme (`Thread.Sleep(2)`) e acorda constantemente para ler do DSP.
* **Benefício:** A voz é gravada e encodada a exatos 50 pacotes por segundo (20ms), de forma lisa, **independente se o FPS do Tarkov estiver a 120 ou travado a 2 FPS**.
* **Atenção:** Dentro de processos paralelos, não podemos usar `Time.deltaTime`. Foi substituído por constantes matemáticas exatas (`0.02f` = 20ms) no `VoipProcessor` para atualizar o VU meter e o VAD.

### 2. Jitter Buffer Elástico de Dois Estágios
No `RemoteSpeaker`, implementamos resiliência à rede imperfeita:
- **Jitter Inicial (`150ms`):** Acumulamos 150ms antes de começar a falar para garantir estabilidade inicial.
- **Jitter de Recuperação (`40ms`):** Se a rede falhar feio e der underrun (buffer secar no meio de uma fala), o sistema agora exige **apenas 2 frames (40ms)** para reiniciar.
* **Benefício:** Quedas de pacotes geram *micro-cortes* quase invisíveis na fala, ao invés de buracos gigantes de silêncio, passando despercebidos pelo ouvido humano num tiroteio.

### 3. Loopback 2D de Alta Fidelidade (Eco)
O "Teste de Eco" do F12 (UI) não pega apenas a voz crua. Ele joga os dados Pós-Opus para um `RemoteSpeaker` local com `spatialBlend = 0f` (Áudio 2D puro, sem direção).
* **Benefício:** O usuário se escuta **exatamente com a mesma fidelidade, gating e compressão** que os demais jogadores da raid, facilitando a calibração do microfone.
