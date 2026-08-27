# TRL-SpeakFromTarkov — Documentação Técnica

Sistema VOIP proprietário para FIKA/SPT 4.0 (EFT 0.16.9). Substitui o Dissonance/FIKA VOIP nativo por um pipeline completo de captura, filtragem neural, codificação Opus e reprodução 3D posicional em tempo real, com reatividade de bots à voz do jogador.

**Versão documentada:** `1.5.2` · **Escopo:** `modded-V3-audit/` · **Status:** 🟢 Vivo

---

## Índice de Documentos

| # | Documento | Descrição | Status |
|---|---|---|---|
| 01 | [Visão Geral e Arquitetura](./01-visao-geral-e-arquitetura.md) | Diagrama de componentes, ciclo de vida, módulos, dependências, estados de canal | 🟢 Vivo |
| 02 | [Pipeline de Áudio DSP](./02-pipeline-audio-dsp.md) | MicrophoneCapturer, AudioFilter (RNNoise/HPF+Gate), VoipProcessor (VAD/PTT/Open), RemoteSpeaker | 🟢 Vivo |
| 03 | [Camada de Rede e Protocolo](./03-rede-e-protocolo.md) | SftNetwork, SftAudioPacketV2 (envelope), V1 legado, SftChannelAnnouncementPacket, thread-safety, guards de recepção | 🟢 Vivo |
| 04 | [Reatividade de Bots e Áudio 3D](./04-bots-e-audio-3d.md) | BotVoiceBridge, interpolação de raio, BotEventHandler, ancoragem 3D, oclusão, patches GameSession | 🟢 Vivo |
| 05 | [Interface, HUDs e Configurações F12](./05-interface-huds-e-configuracoes.md) | InRaidVoipHUD, VoipHUD, VoiceCalibrationHUD, PlayerVolumeMixerHUD, MenuVoipHUD, catálogo de configs | 🟢 Vivo |
| 06 | [Relatório de Auditoria Técnica (Review 01)](./relatorio-auditoria-codigo-01.md) | 1ª rodada de auditoria estática em 6 dimensões (AUD-01-01 a AUD-01-09 aplicados) | 🟢 Vivo |
| 07 | [Relatório de Auditoria Técnica (Review 02)](./relatorio-auditoria-codigo-02.md) | 2ª rodada de auditoria pós-refatoração (AUD-02-01 a AUD-02-03) | 🟢 Vivo |

---

## Mapa de Código-Fonte (`modded-V3-audit/`)

| Arquivo | Namespace | Responsabilidade |
|---|---|---|
| [`VOIPPlugin.cs`](../modded-V3-audit/VOIPPlugin.cs) | `TRL_SpeakFromTarkov` | Entry point BepInEx: configs F12, patches, bootstrap |
| [`GameSessionPatcher.cs`](../modded-V3-audit/GameSessionPatcher.cs) | `TRL_SpeakFromTarkov` | 9 patches Harmony: ciclo de raid + silenciamento Dissonance |
| [`SftAudioPacket.cs`](../modded-V3-audit/SftAudioPacket.cs) | `TRL_SpeakFromTarkov` | Structs V1 (legado recepção) + V2 (envio com envelope) |
| [`Core/VoipController.cs`](../modded-V3-audit/Core/VoipController.cs) | `TRL_SpeakFromTarkov.Core` | Orquestrador central: wiring de todos os subsistemas |
| [`Audio/MicrophoneCapturer.cs`](../modded-V3-audit/Audio/MicrophoneCapturer.cs) | `TRL_SpeakFromTarkov.Audio` | Captura PCM: polling + ring buffer + thread de captura + Catmull-Rom resample |
| [`Audio/AudioFilter.cs`](../modded-V3-audit/Audio/AudioFilter.cs) | `TRL_SpeakFromTarkov.Audio` | DSP: RNNoise (P/Invoke) ou HPF+NoiseGate; AGC; Limiter; LPF |
| [`Audio/VoipProcessor.cs`](../modded-V3-audit/Audio/VoipProcessor.cs) | `TRL_SpeakFromTarkov.Audio` | Decisão VAD/PTT/Open; encoder Opus Concentus; evento OnOpusDataEncoded |
| [`Audio/BotVoiceBridge.cs`](../modded-V3-audit/Audio/BotVoiceBridge.cs) | `TRL_SpeakFromTarkov.Audio` | Janela 500ms; interpolação raio; BotEventHandler; ForceBotResponsesInRadius |
| [`Audio/RemoteSpeaker.cs`](../modded-V3-audit/Audio/RemoteSpeaker.cs) | `TRL_SpeakFromTarkov.Audio` | Decoder Opus; stream buffer; jitter buffer; pan; oclusão; rolloff manual |
| [`Network/SftNetwork.cs`](../modded-V3-audit/Network/SftNetwork.cs) | `TRL_SpeakFromTarkov.Network` | Registro de pacotes FIKA; fila ConcurrentQueue; DrainSendQueue; DispatchVoipPacket |
| [`Network/SftChannelAnnouncementPacket.cs`](../modded-V3-audit/Network/SftChannelAnnouncementPacket.cs) | `TRL_SpeakFromTarkov.Network` | Struct de anúncio de canal de menu (Announce/Join/Leave/Kick/Ban) |
| [`UI/VoipHUD.cs`](../modded-V3-audit/UI/VoipHUD.cs) | `TRL_SpeakFromTarkov.UI` | Painel IMGUI de diagnóstico e profiler |
| [`UI/InRaidVoipHUD.cs`](../modded-V3-audit/UI/InRaidVoipHUD.cs) | `TRL_SpeakFromTarkov.UI` | Barra tática 7px + enum HudVisibilityMode + ícones PNG |
| [`UI/VoiceCalibrationHUD.cs`](../modded-V3-audit/UI/VoiceCalibrationHUD.cs) | `TRL_SpeakFromTarkov.UI` | Wizard interativo de calibração de limiares |
| [`UI/PlayerVolumeMixerHUD.cs`](../modded-V3-audit/UI/PlayerVolumeMixerHUD.cs) | `TRL_SpeakFromTarkov.UI` | Modal per-player de volume em raid |
| [`UI/MenuVoipHUD.cs`](../modded-V3-audit/UI/MenuVoipHUD.cs) | `TRL_SpeakFromTarkov.UI` | Gerenciamento de canais de voz no menu principal |
| [`Properties/AssemblyInfo.cs`](../modded-V3-audit/Properties/AssemblyInfo.cs) | — | Metadados de assembly |

---

> Para configurar o ambiente de build, consulte [GEMINI.md](../../../GEMINI.md) e [AGENTS.md](../../../AGENTS.md).
> Para PROPRIEDADES.md e ROADMAP do mod: [PROPRIEDADES.md](../PROPRIEDADES.md) · [ROADMAP.md](../ROADMAP.md)
