# 002 — Ciclo linear, hotkeys e snap fogo · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [002-ciclo-linear-hotkeys-snap-fogo-01-spec.md](002-ciclo-linear-hotkeys-snap-fogo-01-spec.md)
**Spec técnica:** [002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md](002-ciclo-linear-hotkeys-snap-fogo-02-spec-tech.md)
**Última review técnica:** [002-ciclo-linear-hotkeys-snap-fogo-03-spec-tech-review-04.md](002-ciclo-linear-hotkeys-snap-fogo-03-spec-tech-review-04.md)
**Code reviews:**

- [002-ciclo-linear-hotkeys-snap-fogo-04-code-review-01.md](002-ciclo-linear-hotkeys-snap-fogo-04-code-review-01.md)

**Fixes posteriores:**

- [002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md](002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md) — F4 patch target corrigido (operation-base → FirearmController.SetTriggerPressed) + swap Stance 2 ↔ Stance 3.
- [002-ciclo-linear-hotkeys-snap-fogo-06-fix-02.md](002-ciclo-linear-hotkeys-snap-fogo-06-fix-02.md) — Labels das hotkeys Stance 2/3 (residual do swap do 06-fix-01) + ordem F12 da seção `Stance 0 - Vanilla` (via Order bump em `BindStance`).

**Build inicial:** 2026-05-10

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.
>
> ⚠️ **Asbuild criado retroativamente** durante o `/apply-code-review` do CR-01 (2026-05-10). O `/code-mod` original (mesma data, mais cedo) foi executado **antes** de o `/code-mod` passar a gerar `05-asbuild.md` automaticamente.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs` | Section renames Stance 1/2/3 (Ready Up→High Ready, Ready Down→Custom, Custom→Low Ready); +`enum ScrollMode`; +`_stanceDefaults.SnapOnFire`; -`_UseOnlyStances`; +8 ConfigEntries Settings (Include Stance 0, ScrollMode, 4 Hotkeys, SnapFireThreshold, StartInLowReady); helpers F2 (CM cache + Browsable refresh + OnDestroy unsubscribe via named handler); helpers F4 (ResolveFirearmOperationBase abstract-first + GetBaseDefinition fallback; CurrentOperationGetter); registro condicional do SnapFireTriggerPatch |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded/StanceConfig.cs` | +`SnapToStance0OnFire` (nullable sentinel para Stance.Default) |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs` | F1 `IsStanceEnabled` reescrito (Include Stance 0 explícito); F2 `HandleLinearScroll` + branch por `ScrollMode`; F3 `HandleStanceHotkeys`/`TryHotkey` retornando bool (early-return priority); F4 estado snap (`_triggerDownTimeUnscaled`, `_snapInterceptActive`, 2 pares de pending resurrect/reset) + 6 helpers (TryInterceptTriggerDown, OnTriggerUpAfterIntercept, TryDispatchPendingResurrect, EvaluateSnapStaleTimeout, IsTriggerPressedNaturally, IsOperationStillCurrent); F5 `QueueInitialStance`/`TryApplyPendingInitialStance` com HideoutPlayer guard; `Update` reordenado (hotkeys antes da tecla V); `ResetState` estendido |
| CRIADO | `mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/SnapFireTriggerPatch.cs` | Prefix intercept-and-resurrect com `[ThreadStatic] _inSyntheticCall`; cache de `_trueArgs`/`_falseArgs`; cache lazy de `_cachedFcField`; `RaiseSyntheticTrigger(object, MethodBase, bool)` para o pulso de 2 frames |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/RaidLifecyclePatches.cs` | `GameWorldOnGameStartedPatch.Postfix` enfileira Stance3 via `QueueInitialStance` quando `_StartInLowReadyOnRaidBegin = true` |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md` | 89 props (de 79): −1 Use Only Stances, +11 novas, 3 seções renomeadas |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/backlog/mod-backlog.md` | 002 → 🟢 |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica (04) que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

Todos os 24 PAs das 4 rodadas de spec-tech-review foram aceitos pelo usuário e refletidos na spec técnica antes do build. Lista resumida (ver reviews para detalhes):

| Round | IDs aplicados | Tema |
| --- | --- | --- |
| 01 | PA-01-01..08 | Patches F4 por reflection da operation-base; race de timer; CM dependency; hotkey priority; snap state leak; nullability; checklist refinements |
| 02 | PA-02-01..06 | `[ThreadStatic]` reentry guard; resolução `IsAbstract` + `GetBaseDefinition`; defer 1-frame para resurrect; remove closure; Enable condicional; F5+F4 simultâneos AC |
| 03 | PA-03-01..05 | 2-frame pulse (synthetic false em N+2 para parar fullauto); validação `CurrentOperation` entre frames; AC ChangeFireMode mid-hold; SettingChanged unsubscribe; stub BuildStanceConfig |
| 04 | PA-04-01..05 | Order 59 collision fix (ScrollMode → 58); hotkeys antes de V no Update; natural-pressed guard no reset; HideoutPlayer guard em F5 |

## Mudanças posteriores

### Rodada CR-01 (2026-05-10) — code review 01

6 achados aplicados via `/apply-code-review`:

| ID | Cat · Impacto | Resumo da aplicação | Arquivos tocados |
| --- | --- | --- | --- |
| CR-01-01 | B · 🟠 | Multiplayer (Fika) — guard `fc == MainPlayer.HandsController` no Prefix do SnapFireTriggerPatch | `Patches/SnapFireTriggerPatch.cs` |
| CR-01-02 | B · 🟡 | Anti-swap: `_interceptOperationInstance` cacheado no down, validado no up | `StanceManager.cs` |
| CR-01-03 | D · 🟢 | `TryInterceptTriggerDown(object operationInstance)` — parâmetro agora usado (anti-swap) | `StanceManager.cs`, `Patches/SnapFireTriggerPatch.cs` |
| CR-01-04 | D · 🟢 | Removido `IsHoldingFirearm()` redundante em `TryInterceptTriggerDown` (caller já validou via MainPlayer guard do CR-01-01) | `StanceManager.cs` |
| CR-01-05 | E · 🟢 | XMLDOC explícito do null sentinel em `SnapToStance0OnFire` | `StanceConfig.cs` |
| CR-01-06 | F · 🟢 | `Snap Stale Timeout (s)` exposto como ConfigEntry Advanced (90ª prop) | `Plugin.cs`, `PROPRIEDADES.md` |

### Rodada 06-fix-01 (2026-05-10) — F4 patch target + Stance 2 ↔ Stance 3 swap

Disparada por feedback in-raid do usuário: F4 não funcionou no item 002 original; usuário também pediu swap das identidades das stances 2 e 3.

| Eixo | Mudança | Arquivos tocados |
| --- | --- | --- |
| **F4 patch target** | Operation-base aninhada (Player.cs:3810) → `Player.FirearmController.SetTriggerPressed` (Player.cs:13668). Causa: virtual dispatch em C# bypassa Harmony quando override não chama `base` — só 1 de 14 overrides chamam (Player.cs:3184). Detalhes em 06-fix-01.md. | `Plugin.cs`, `Patches/SnapFireTriggerPatch.cs`, `StanceManager.cs` |
| **Stance 2 ↔ 3 swap** | Stance 2 = Low Ready (era Custom); Stance 3 = Custom (era Low Ready). Section constants, `_stanceDefaults`, hand rotation defaults, F5 target — todos swap. | `Plugin.cs` (constants L57-60, tuple L43-46, hand rotation defaults L735-840), `Patches/RaidLifecyclePatches.cs` (L29 F5 target → Stance.Stance2) |
| **PROPRIEDADES.md** | Section headers swap, 12 default values swap nas tabelas, 2º aviso de breaking change. | `PROPRIEDADES.md` |
| **Spec funcional** | 01-spec.md atualizada com novos nomes em Visão geral, ACs, F12 layout, corner cases. Histórico nova entrada. | `002-…-01-spec.md` |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-05-10 | Build concluído via `/code-mod` (item 002 entregue antes do `/code-mod` passar a gerar asbuild automático) |
| 2026-05-10 | Aplicação de 6 achados de code-review 01 via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03, CR-01-04, CR-01-05, CR-01-06. Asbuild criado retroativamente. |
| 2026-05-10 | **06-fix-01:** F4 patch target trocado de operation-base (Player.cs:3810) para `Player.FirearmController.SetTriggerPressed` (Player.cs:13668) — virtual dispatch bypassa Harmony quando override não chama base (só 1 de 14 chama). Stance 2 ↔ Stance 3 swap completo (rótulos, defaults de axis/stamina/speed/snap, F5 target). Detalhes em [`06-fix-01.md`](002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md). |
| 2026-05-10 | **06-fix-02:** residuais do swap do 06-fix-01 — labels das hotkeys `_Stance2Hotkey`/`_Stance3Hotkey` (string literals em `Config.Bind` que ficaram com nomes antigos "Custom"/"Low Ready"); tooltips dessas hotkeys; e seção `Stance 0 - Vanilla` na ordem errada do F12 (corrigido com Order bumped de 5 para 35 em `BindStance` para `Stance.Default`). Detalhes em [`06-fix-02.md`](002-ciclo-linear-hotkeys-snap-fogo-06-fix-02.md). |
