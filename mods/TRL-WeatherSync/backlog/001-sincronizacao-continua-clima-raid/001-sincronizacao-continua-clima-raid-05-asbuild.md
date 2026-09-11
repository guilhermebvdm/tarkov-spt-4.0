# 001 — Sincronização Contínua de Clima em Raid · As-Built

**Mod:** TRL-WeatherSync
**Spec funcional:** [001-sincronizacao-continua-clima-raid-01-spec.md](001-sincronizacao-continua-clima-raid-01-spec.md)
**Spec técnica:** [001-sincronizacao-continua-clima-raid-02-spec-tech.md](001-sincronizacao-continua-clima-raid-02-spec-tech.md)
**Última review técnica:** [001-sincronizacao-continua-clima-raid-03-spec-tech-review-02.md](001-sincronizacao-continua-clima-raid-03-spec-tech-review-02.md)
**Build inicial:** 2026-09-10

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-WeatherSync/modded/TRL-WeatherSync.csproj` | Projeto BepInEx 5 (.NET Standard 2.1), referenciando `Fika.Core` e as DLLs padrão do jogo. |
| CRIADO | `mods/TRL-WeatherSync/modded/Plugin.cs` | `BaseUnityPlugin` (`trl.weathersync`, depende de `com.fika.core`); registra as 2 `ConfigEntry`s, os 3 patches de lifecycle e assina o evento de criação do NetworkManager. |
| CRIADO | `mods/TRL-WeatherSync/modded/Networking/TrlWeatherSyncPacket.cs` | `struct : INetSerializable` com envelope de comprimento, campos de clima (`Cloudness`/`Wind`/`WindDirection`/`Rain`/`ScaterringFogDensity`/`Temperature`) e `ThunderEventTrigger`. |
| CRIADO | `mods/TRL-WeatherSync/modded/Networking/WeatherSyncNetworkHandler.cs` | Registro híbrido (evento `FikaNetworkManagerCreatedEvent` + polling) por papel (`Source`/`Receiver` no handler normal, `Relay` na variante com `NetPeer`); broadcast e callbacks com airbag. |
| CRIADO | `mods/TRL-WeatherSync/modded/Patches/RaidLifecyclePatches.cs` | 3 patches Postfix: `GameWorld.OnGameStarted`, `GameWorld.OnDestroy`, `CoopGame.Stop` — todos com try/catch. |
| CRIADO | `mods/TRL-WeatherSync/modded/WeatherSyncSession.cs` | `MonoBehaviour` raid-scoped: enum `WeatherRole` (`Source`/`Relay`/`Receiver`), resolução de papel desacoplada de `FikaBackendUtils.IsServer`, broadcast periódico, conversão de direção de vento, aplicação via `SetWeatherForce`/`HandleReconnect`. |
| CRIADO | `mods/TRL-WeatherSync/PROPRIEDADES.md` | Documenta as `ConfigEntry`s (`Enable Weather Sync`, `Sync Interval Seconds`; `Storm Check Cooldown Seconds` adicionada na rodada 01 de code-review). |
| MODIFICADO | `mods/TRL-WeatherSync/README.md` | Status atualizado; seção "Requisito de instalação" adicionada (mod exigido em todos os peers da raid). |

## PA-NN-MM resolvidos durante o build

> Todos os pontos das reviews 01 e 02 já haviam sido resolvidos na própria spec técnica antes deste build — nenhum ponto novo foi resolvido diretamente no código além do que a spec já continha. Lista de referência (já ✅ na spec):

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Erro de Lógica · 🔴 | Alvo do patch de fim de raid trocado de `BaseLocalGame<T>.Stop` para `CoopGame.Stop` (implementado em `RaidLifecyclePatches.cs`). |
| PA-01-02 | C — Erro de Lógica · 🔴 | Decisão: mod exigido em todos os peers da raid — documentado em `README.md`/`PROPRIEDADES.md`. |
| PA-01-03 | A — Gap · 🟡 | `try/catch` em todos os 3 Postfixes de `RaidLifecyclePatches.cs`. |
| PA-01-04 | A — Gap · 🟡 | Campo `AtmospherePressure` removido de `TrlWeatherSyncPacket.cs`. |
| PA-01-05 | A — Gap · 🟢 | `NearestWindDirectionIndex` implementado em `WeatherSyncSession.cs`. |
| PA-01-06 | B — Edge Case · 🟢 | Drift de relógio coberto pelo broadcast periódico (sem código adicional — já é o design). |
| PA-02-01 | C — Erro de Lógica · 🔴 | `Role.Source` registra o mesmo handler de recepção que `Role.Receiver` em `WeatherSyncNetworkHandler.cs` (evita `ParseException` no eco do Relay). |
| PA-02-02 | B — Edge Case · 🟢 | Comentário explicativo sobre a janela evento-antes-da-sessão em `WeatherSyncNetworkHandler.cs`. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

### Rodada 01 (code-review 01, 2026-09-10)

| ID | Resultado | Arquivo(s) |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `WeatherSyncSession.cs:106-113` — `Wind`/`Rain` convertidos de volta pra escala `[1,5]` (`Mathf.Lerp(1f, 5f, ...)`) antes de montar `TrlWeatherSyncPacket`, corrigindo o descompasso com `WeatherClass`/`WeatherCurve.method_4()` (que espera essa escala e desnormaliza via `InverseLerp(1f, 5f, ...)`). Sem essa correção, chuva e vento sincronizados sempre aplicavam como zero no lado que recebe. |
| CR-01-02 | ✅ Aplicado | `WeatherSyncSession.cs` (`RollForStorm`, `_stormCooldownRemaining`) + `Plugin.cs` (`ConfigEntry` `Storm Check Cooldown Seconds`) — política simples de início de tempestade: sorteia contra `IWeatherCurve.LightningThunderProbability` a cada ciclo, com cooldown configurável. Decisão do usuário: só o INÍCIO é sincronizado; o fim continua não-forçado (depende de P-2.1, ainda aberta). |
| CR-01-03 | ✅ Aplicado | `WeatherSyncSession.cs` (`NormalizedWindDirections`, `BuildNormalizedWindDirections`) — os 9 vetores de direção de vento são normalizados uma vez em vez de a cada chamada de `NearestWindDirectionIndex`. |

## Pendências conhecidas (não resolvidas neste build)

- **P-2.1 (memória do mod):** sincronizar o FIM de uma tempestade forçada continua sem solução — `HandleReconnect` não tem evidência de funcionar pra sair de `Storm` (`Class451`/`Class452` podem herdar um no-op), e descobriu-se que existe um segundo state machine (`RainController`/`ERainControllerStatus.WinterStorm`) que também precisaria ser revertido. **Decisão consciente do usuário:** o mod sincroniza só o INÍCIO da tempestade (`RollForStorm`, CR-01-02); o fim fica pelo tempo nativo de cada jogo, não sincronizado.
- **Testes in-game:** nenhum item do checklist §8 que exige raid real foi validado (raid Headless, raid Host normal, reconexão em tempestade, raid1→raid2, e agora também: início de tempestade sincronizado). Ver `/compile-mod` e testes manuais antes de considerar o item pronto para uso.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-10 | Build concluído via `/code-mod` — 6 arquivos de código criados, `PROPRIEDADES.md` criado, `README.md` atualizado. Compilação (`.dll`) e testes in-game ainda pendentes. |
| 2026-09-10 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — CR-01-01 (bug crítico de escala Rain/Wind) corrigido em `WeatherSyncSession.cs`. CR-01-02/CR-01-03 seguem pendentes. Recompilação (`/compile-mod`) ainda necessária antes de testar. |
| 2026-09-10 | Recompilado — v1.0.0 → 1.0.1 (fix CR-01-01). |
| 2026-09-10 | Aplicados CR-01-02 (política simples de início de tempestade) e CR-01-03 (micro-otimização) — rodada 01 de code-review fechada, 3/3 achados aplicados. Recompilado — v1.0.1 → 1.1.0 (feature nova visível: tempestade sincronizada, ainda que só o início). Nenhuma instalação automática no jogo em nenhuma das compilações desta rodada, a pedido do usuário — só `mods/TRL-WeatherSync/builds/`. |
