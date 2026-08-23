# 010 — perf-spawn-pipeline-r2 · As-Built

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [010-perf-spawn-pipeline-r2-01-spec.md](010-perf-spawn-pipeline-r2-01-spec.md)
**Spec técnica:** [010-perf-spawn-pipeline-r2-02-spec-tech.md](010-perf-spawn-pipeline-r2-02-spec-tech.md)
**Última review técnica:** [010-perf-spawn-pipeline-r2-03-spec-tech-review-02.md](010-perf-spawn-pipeline-r2-03-spec-tech-review-02.md) (review 01: [aqui](010-perf-spawn-pipeline-r2-03-spec-tech-review-01.md))
**Build inicial:** 2026-08-23

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.
>
> **Escopo:** rodada 2 de performance (AUD-01-04/05/06/07/08 + PA-01-05 do item 009). **100% client-side.** Build de sanidade `dotnet build` Release: 0 erros (refs temporárias fora do repo, como no 009).

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-DynamicSpawn/Client/Patches/SpawnGatePatches.cs` | `ActivateBotsWithoutWavePatch` — prefix em `BotsController.ActivateBotsWithoutWave(int, IGetProfileData)` (`:536`): recusa `assault`/`cursedAssault` do spawner contínuo vanilla **antes** de criar/escolher perfil; `marksman` passa; guest/`IsGeneratingDynamicWave` → vanilla. `// ref: AUD-01-08` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Patches/Patches.cs` | `ChooseProfilePatch` reescrito: todos os papéis, exato → relaxado (Side+Role) → vanilla; PMC por Side **ou** Role (`PmcMatches(InfoClass, ProfileInfoSettingsClass, …)`), sem o fallback "qualquer perfil"; passada única sem LINQ; logs gated/Info. `// ref: AUD-01-04, AUD-01-07` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Patches/BotSpawnLoggerPatch.cs` | Gate antes de formatar; `LogInfo`. `// ref: AUD-01-07` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Patches/RaidLifecyclePatches.cs` | `BaseLocalGameStopPatch` (inerte) → `LocalGameStopPatch` (`LocalGame.cs:357`) + `CoopGameStopPatch` (soft, `AccessTools.TypeByName`, `CoopGame.cs:718`). `// ref: PA-01-03` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Helpers/RaidLifecycle.cs` | `OnRaidEnd` chama `DynamicSpawnManager.StopSpawnLoops()`. `// ref: AUD-01-06` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs` | Pré-carga inicial via `Settings.initialProfilePreload` (era 30/30/20); removida pré-carga fixa 10/10/10 por onda; `ClearSptQueue` 1×/raid (`_sptQueueClearedThisRaid`) após check de humano; pausa sem humano vivo interrompe `_activeWaveCoroutine`; `GetAliveHumanCount()`; `StopSpawnLoops()` (flags antes do early-return); gate em `Calculating Wave`/`[SPY]`/`Horde Breakdown`/`Batch profile pre-fetching`/`Profiles generated`/`SQUAD …`. `// ref: AUD-01-04/05/06/07` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Helpers/Settings.cs` | `ConfigEntry<int> initialProfilePreload` ("Profile Pool (Advanced)" / "Initial Profile Preload", 15, 0–30, Advanced) |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Plugin.cs` | Registra `LocalGameStopPatch`, `CoopGameStopPatch` (só com `TargetType != null`), `ActivateBotsWithoutWavePatch`; remove `BaseLocalGameStopPatch`. Versão inalterada (bump **minor** no compile: 3.3.0 → 3.4.0) |
| MODIFICADO | `mods/TRL-DynamicSpawn/PROPRIEDADES.md` | Seção `Profile Pool (Advanced)` |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-02-01 | C — Lógica · 🟢 | Comentário de `PmcMatches` cita `WildSpawnType.cs` real (sem `spt*`) |
| PA-02-02 | A — Gap · 🟢 | Código segue a semântica corrigida (1×/raid, `:160`, campos públicos) |
| PA-02-03 | A — Gap · 🟢 | Guard `TargetType != null` no `Plugin.cs`; expectativa `CoopGame.Stop` documentada no patch |
| PA-01-01..09 | (review 01) | Já aplicados na spec antes do build; o código reflete todos (tipo `ProfileInfoSettingsClass`, callers, hooks concretos, AC-X5, pausa interrompe onda, flags-first, 1×/raid, MoreBotsAPI não verificado) |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-23 | Build concluído via `/code-mod` (Fase 3 do `/optimize-mod-performance`, rodada 2) |
