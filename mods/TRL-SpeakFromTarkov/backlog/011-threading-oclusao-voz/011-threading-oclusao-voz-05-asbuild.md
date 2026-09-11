# 011 — Threading e Oclusão de Voz · As-Built

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [011-threading-oclusao-voz-01-spec.md](011-threading-oclusao-voz-01-spec.md)
**Spec técnica:** [011-threading-oclusao-voz-02-spec-tech.md](011-threading-oclusao-voz-02-spec-tech.md)
**Última review técnica:** [011-threading-oclusao-voz-03-spec-tech-review-01.md](011-threading-oclusao-voz-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod` em `modded-V4/`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/RemoteSpeaker.cs` | Cache de `_cachedListenerPos`/`_cachedListenerRight` em `Update()` (main thread); `OnAudioFilterRead` (audio thread) só lê esses campos, zero chamada a `Camera.main`/`Singleton<GameWorld>` ali; corrigido o bitmask de oclusão (`1 << DoorLayer`, `InteractiveMask`). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/BotVoiceBridge.cs` | Mesma correção de bitmask de oclusão em `_botOcclusionMask`. |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/VoipProcessor.cs` | `RawLevel`, `DisplayLevel`, `PeakLevel`, `LastOpusBytes`, `IsTransmitting`, `IsMuted`, `IsPTTActive`, `CurrentMode` convertidos para backing field `volatile`. |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/AudioFilter.cs` | Propriedades de config soltas substituídas por `NoiseGateConfig` (snapshot imutável trocado via `Interlocked.Exchange`); `Apply()` corrigido para incluir `ApplyAGC`/`ApplyLimiter` (achado da review). |
| MODIFICADO | `mods/TRL-SpeakFromTarkov/modded-V4/Audio/MicrophoneCapturer.cs` | `Initialize()`/`Update()` passam a montar `AudioFilter.NoiseGateConfig` via helper `BuildNoiseGateConfig()` e chamar `UpdateConfig()`, em vez de escrever 9 propriedades campo a campo. `isThreadRunning` já era `volatile` no código existente — sem mudança necessária. |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que já vinham resolvidos na spec técnica antes do build (aplicados na sessão de `/review-technical-spec`, não durante este `/code-mod`).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 Bloqueador | Stub de `Apply()` corrigido antes do build para incluir `ApplyAGC`/`ApplyLimiter` — implementado corretamente no código real. |
| PA-01-02 | A — Gap · 🟢 Menor | Nota sobre `RNNoiseVADThreshold`/`RNNoiseGateHold` aparentemente inertes registrada na spec técnica; campos incluídos no `NoiseGateConfig` mesmo assim (correção de threading vale independente do consumo). |

## Mudanças posteriores

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded-V4/`. Testes em jogo e compilação (`/compile-mod`) ainda pendentes. |
