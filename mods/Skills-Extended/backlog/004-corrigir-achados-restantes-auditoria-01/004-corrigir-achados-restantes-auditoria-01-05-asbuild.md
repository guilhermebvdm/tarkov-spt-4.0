# 004 — Corrigir achados restantes da auditoria 01 · As-Built

**Mod:** Skills-Extended
**Spec funcional:** [004-corrigir-achados-restantes-auditoria-01-01-spec.md](004-corrigir-achados-restantes-auditoria-01-01-spec.md)
**Spec técnica:** [004-corrigir-achados-restantes-auditoria-01-02-spec-tech.md](004-corrigir-achados-restantes-auditoria-01-02-spec-tech.md)
**Última review técnica:** [004-corrigir-achados-restantes-auditoria-01-03-spec-tech-review-01.md](004-corrigir-achados-restantes-auditoria-01-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-07

> Documentação pós-implementação. Reflete o estado real do código entregue pelo `/code-mod`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/SilentOps/Patches/MeleeSpeedPatch.cs` | Resolve o dono real do golpe via `GameWorld.AllAlivePlayersList`/`Player.HandsAnimator` em vez de sempre o MainPlayer (`AUD-01-16`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/LockPicking/LockPickingGame.cs` | Null-checks em `SetSweetSpotRange`/`SetTimeLimit` (`AUD-01-17`); `_onUnlocked = null` em `OnDisable` (`AUD-01-23`); reasserção de cursor/input movida de `Update()` pra `OnEnable()` (`AUD-01-28`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Helpers/ConsoleCommands.cs` | Guard `Singleton<GameWorld>.Instantiated` em `DoDamage`/`DoDie`/`DoFracture`; null-safety em `GetAllWeaponIDsInInventory` (`AUD-01-18`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs` | Cacheia o delegate de XP de bruços, recriado só quando o `Player` muda (`AUD-01-19`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffPatch.cs` | Remove o `.Clone()` — aplica o ajuste de duração/chance no `__result` real, não num clone descartado (`AUD-01-20`, bug funcional real — ver spec técnica §0) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffStringPatches.cs` | Mesma correção, aplicada ao `__instance` do tooltip (`AUD-01-20`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/LockPicking/WorldInteractionUtils.cs` | Memoiza `GetLockPicksInInventory().Any()` por `Time.frameCount`; handlers continuam frescos por chamada, vinculados ao `owner` correto (`AUD-01-21`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/LockPicking/Patches/KeyCardDoorActionPatch.cs` | Comentário apontando a assinatura desatualizada de `smethod_9` para a spec técnica (`AUD-01-22`) |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Core/UpdateChecker.cs` | `using var httpClient` com `Timeout` de 5s; `catch (Exception ex)` com log via `logger.Debug` (`AUD-01-24`, `AUD-01-25`, `AUD-01-26`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs` | `LockSkills` cacheia o `FieldInfo` de `"Locked"` em vez de resolver 8x por `SkillManager` construído (`AUD-01-27`) |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Skills/LockPicking/Actions/HackingActionHandler.cs` | Cacheia o `MethodInfo` de `"Unlock"` em vez de resolver a cada terminal hackeado (`AUD-01-27`) |
| MODIFICADO | `mods/Skills-Extended/modded/Common/SkillsExtendedInfo.cs` | Bump de versão 2.2.6 → 2.2.7 |
| MODIFICADO | `mods/Skills-Extended/modded/Plugin/Plugin.csproj` | Bump de versão 2.2.6 → 2.2.7 |
| MODIFICADO | `mods/Skills-Extended/modded/Server/Server.csproj` | Bump de versão 2.2.6 → 2.2.7 |

## PA-NN-MM resolvidos durante o build

> A review técnica 01 não levantou nenhum achado formal (0 pontos) — o único trabalho da rodada foi reforçar evidência do `AUD-01-20` (aplicado diretamente na spec técnica antes do build, não durante).

## Achado durante o build (não estava em nenhuma review)

Ao implementar `AUD-01-24`/`26` (`UpdateChecker.cs`), a spec técnica assumia que `ISptLogger<T>.Debug(string)` existia sem confirmar via Assembly (o método era usado só em prosa/stub). Antes de finalizar, usei reflection direta sobre a DLL real (`SPTarkov.Server.Core.dll`, via PowerShell `Assembly.LoadFrom` + `GetMethods()`) pra confirmar a assinatura exata: `Debug(string data, Exception ex = null)` — confirmado, `ex` é opcional. Documentando aqui porque foi verificação feita durante a implementação, não na spec técnica em si.

## Mudanças posteriores

### 2026-09-07 — `/apply-code-review` (rodada 01)

| ID | Resultado | Arquivo tocado |
| --- | --- | --- |
| CR-01-01 | ✅ Aplicado | `mods/Skills-Extended/modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs` (novo `ClearCachedAction()`); `mods/Skills-Extended/modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs` (chamada em `OnGameEndedPatch.Postfix`) |

## Pendências não-bloqueadoras para validação manual

- Validar in-game que o cursor/input do minigame de arrombamento continua correto após mover a reasserção de `Update()` pra `OnEnable()` (`AUD-01-28`) — se vazar, reverter e documentar como reasserção intencional.
- Validar in-game (prioridade alta) que o ajuste de duração/chance do Field Medicine em estimulantes não "vaza" entre usos — usar o mesmo tipo de estimulante duas vezes com níveis de skill diferentes (`AUD-01-20`).
- Compilação efetiva (`.dll`) ainda não realizada nesta rodada — fora do escopo do `/code-mod`.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Build concluído via `/code-mod` — 13 achados (`AUD-01-16` a `AUD-01-28`) implementados em `mods/Skills-Extended/modded/`; review técnica 01 não levantou achados formais |
| 2026-09-07 | Compilação Release (Plugin + Common + Server) via `dotnet build` redirecionado — 0 erros de código; nada instalado em `E:/Tarkov Red Line` |
| 2026-09-07 | `/code-review` rodada 01 — 1 achado (`CR-01-01`, retenção de `Player` entre raids no cache do `AUD-01-19`); `/apply-code-review` rodada 01 — CR-01-01 aplicado |
