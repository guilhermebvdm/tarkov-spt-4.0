# 009 — perf-config-cache-raid · As-Built

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [009-perf-config-cache-raid-01-spec.md](009-perf-config-cache-raid-01-spec.md)
**Spec técnica:** [009-perf-config-cache-raid-02-spec-tech.md](009-perf-config-cache-raid-02-spec-tech.md)
**Última review técnica:** [009-perf-config-cache-raid-03-spec-tech-review-01.md](009-perf-config-cache-raid-03-spec-tech-review-01.md)
**Build inicial:** 2026-08-22

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.
>
> **Escopo:** rodada 1 de performance (AUD-01-01/02/03). **100% client-side** — nenhuma mudança em `Server/`. Logs de debug existentes intocados (observabilidade da validação V1).

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Helpers/ServerConfigProvider.cs` | Cache com escopo de raid (fetch-on-miss, sem TTL); `GetConfigJson(bool bypassBackoff)`; retry mínimo de 30 s em falha (`_lastAttemptTime` avança em sucesso e falha); `ForceRefresh()` única invalidação. `// ref: AUD-01-01`, `AUD-01-03` |
| CRIADO | `mods/TRL-DynamicSpawn/Client/Helpers/RaidLifecycle.cs` | `OnRaidStart` (ignora hideout e guest Fika, idempotente) / `OnRaidEnd` (só para o poller) / `OnWorldDestroyed` (poller + invalida cache). `// ref: AUD-01-01/02` |
| CRIADO | `mods/TRL-DynamicSpawn/Client/Patches/RaidLifecyclePatches.cs` | `RaidStartPatch` (postfix `GameWorld.OnGameStarted` :2584), `GameWorldOnDestroyPatch` (prefix `GameWorld.OnDestroy` :2111), `BaseLocalGameStopPatch` (prefix `BaseLocalGame<EftGamePlayerOwner>.Stop` :1018); `try/catch` + `LogInfo` 1×/raid |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs` | `Start()` não inicia mais o loop; `StartLoop()`/`StopLoop()` estáticos idempotentes; `yield break` no topo do `DespawnLoop` se `GameWorld` não existe. Corpo do scan/teleporte intocado. `// ref: AUD-01-02` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs` | `FetchServerConfigAndStart` usa `ServerConfigProvider.GetConfigJson(bypassBackoff: true)` (cópia privada, 1 HTTP/raid); vazio → throw → fallback existente. `// ref: AUD-01-01` |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Helpers/Settings.cs` | `ConfigEntry<bool> reloadServerConfig` ("Server Config" / "Reload Server Config", default `false`) com handler `SettingChanged` que limpa o cache e se auto-reseta |
| MODIFICADO | `mods/TRL-DynamicSpawn/Client/Plugin.cs` | Registro dos 3 patches de lifecycle (versão inalterada — bump **minor** no `/compile-mod`) |
| MODIFICADO | `mods/TRL-DynamicSpawn/PROPRIEDADES.md` | Seção `Server Config` + cabeçalho de versão atualizado |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | B — Edge Case · 🟡 | `Stop` só para o poller; invalidação do cache exclusiva em `OnDestroy` (`RaidLifecycle.OnWorldDestroyed`) |
| PA-01-02 | B — Edge Case · 🟡 | `GetConfigJson(bypassBackoff: true)` no fetch one-shot do `DynamicSpawnManager` |
| PA-01-03 | B — Edge Case · 🟢 | `OnRaidStart` retorna cedo em guest Fika (`FikaHelper.IsClient()`) |
| PA-01-04 | A — Gap · 🟢 | Critério de bump (minor) registrado; número fica para o `/compile-mod`; cabeçalho do `PROPRIEDADES.md` corrigido |
| PA-01-05 | C — Lógica · 🟢 | `LogInfo` 1×/raid nos dois hooks de fim — a V1 confirma no log se `BaseLocalGame.Stop` dispara |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-22 | Build concluído via `/code-mod` (Fase 3 do `/optimize-mod-performance`, rodada 1) |
