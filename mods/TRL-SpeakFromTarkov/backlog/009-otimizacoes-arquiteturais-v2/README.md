# 009 - Otimizações Arquiteturais e Performance V2 (Zero-Alloc, Spatial Culling, Oclusão e Segurança)

> **Mod:** TRL-SpeakFromTarkov  
> **Status:** 🟡 Em progresso  
> **Alvo:** `mods/TRL-SpeakFromTarkov/modded-V2-otimização`

---

## 📑 Lista de Tarefas Passo a Passo (Backlog de Amanhã)

| # | Passo | Componente / Arquivo | Descrição da Implementação | Status |
|---|---|---|---|---|
| **01** | **Opus DTX, VBR & PTT Hangover** | [`VoipProcessor.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/VoipProcessor.cs) | Ativar `encoder.UseDTX = true` e `encoder.UseVBR = true`. Adicionar timer `pttHoldTimer = 0.20f` no desacionamento da tecla PTT para evitar o corte da última sílaba. | ⚪ |
| **02** | **RMS Pre-Check no Filtro Neural** | [`AudioFilter.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/AudioFilter.cs) | Adicionar pré-checagem de amplitude (RMS < 0.001f) em `ApplyRNNoise()` para pular o P/Invoke da rede neural RNNoise durante silêncio, economizando CPU. | ⚪ |
| **03** | **Motor Zero-Alloc (`ArrayPool<byte>.Shared`)** | [`VoipProcessor.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/VoipProcessor.cs) & [`SftNetwork.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Network/SftNetwork.cs) | Refatorar `Transmit()` e `OnAudioPacketReceivedV2()` com `ArrayPool<byte>.Shared.Rent(payloadLength)`. Repassar `payloadLength` exato para decodificação e devolução com `Return()` em bloco `finally`. | ⚪ |
| **04** | **Decodificação Opus Assíncrona** | [`RemoteSpeaker.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/RemoteSpeaker.cs) | Mover a execução de `decoder.Decode(...)` do loop `Update()` (Main Thread) para a recepção na thread de rede, aliviando a CPU principal do jogo. | ⚪ |
| **05** | **Oclusão Físico-Acústica Zero-Alloc** | [`RemoteSpeaker.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/RemoteSpeaker.cs) | Adicionar checagem física por geometria a cada 200ms usando `Physics.LinecastNonAlloc` com `RaycastHit[1]` na camada `HighPolyWithRaycast`. Suavizar abafamento (-6dB e Low-Pass) via `Mathf.Lerp`. | ⚪ |
| **06** | **Spatial Culling Host-Side (40m)** | [`SftNetwork.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Network/SftNetwork.cs) | Interceptar pacotes de voz no Host via `RegisterPacket<SftAudioPacketV2, NetPeer>`. Para `Channel 0` (vivos), calcular distância quadrada (`sqrMagnitude <= 1600.0f`) e transmitir via `SendDataToPeer`. | ⚪ |
| **07** | **Blindagem de Segurança Vivo/Morto** | [`SftNetwork.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Network/SftNetwork.cs) | No Host, verificar a saúde do emissor via `HealthController.IsAlive`. Se o emissor estiver morto, retransmitir exclusivamente para o `Channel Ghost`, bloqueando 100% o envio para vivos. | ⚪ |
| **08** | **Otimização de IA de Bots** | [`BotVoiceBridge.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/BotVoiceBridge.cs) | Substituir `Vector3.Distance` por `(bot.Position - soundPos).sqrMagnitude <= power * power`. Adicionar checagem física de oclusão por paredes com `Physics.LinecastNonAlloc` antes de forçar `bot.BotTalk.Say()`. | ⚪ |
| **09** | **Compilação e Validação em Raid** | `.agents/scripts/compile-mod.sh` | Executar script de compilação garantindo **0 Erros e 0 Avisos**, validando integridade em raid. | ⚪ |
