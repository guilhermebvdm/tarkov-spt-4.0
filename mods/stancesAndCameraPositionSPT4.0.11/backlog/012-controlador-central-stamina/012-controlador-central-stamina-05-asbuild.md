# 012 — Controlador central de stamina · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-21
**Spec funcional:** [012-controlador-central-stamina-01-spec.md](012-controlador-central-stamina-01-spec.md)
**Spec técnica:** [012-controlador-central-stamina-02-spec-tech.md](012-controlador-central-stamina-02-spec-tech.md)
**Reviews técnicas:** [03-spec-tech-review-01.md](012-controlador-central-stamina-03-spec-tech-review-01.md) (0 🔴)

> `StaminaController` é a **autoridade única** da stamina de braço do MainPlayer: escreve `HandsStamina.Current` por cenário e neutraliza o `Process`/`Consume` vanilla (gate por instância de braço — a perna fica intacta). Absorve o `ArmStaminaCoordinator` (06-fix-01). Compila 0 erros; **aguarda validação in-game**.

## Arquivos alterados

| Ação | Arquivo | Resumo |
|---|---|---|
| CRIADO | `modded-beta/StaminaController.cs` | Enum `StaminaScenario` (15+Inactive), `Resolve`/`Tick` (escrita direta + eventos via reflection), `CurrentLabel`, `ControllingHands`, `Reset`. |
| CRIADO | `modded-beta/StaminaDebugUI.cs` | Overlay `OnGUI` (`STAMINA STATE` + `Current`) no GameObject do plugin, gated por toggle. |
| REMOVIDO | `modded-beta/ArmStaminaCoordinator.cs` | Absorvido pelo `StaminaController`. |
| MODIFICADO | `modded-beta/Patches/StanceStaminaRecoveryPatch.cs` | Os 3 patches do 06-fix-01 → `HandsStaminaNeutralizePatch` (Process) + `HandsConsumeNeutralizePatch` (Consume), gate `__instance == MainPlayer.HandsStamina`. |
| MODIFICADO | `modded-beta/Plugin.cs` | `Update`→`StaminaController.Tick`; `BindStaminaManagement()` (15 mults + toggle) antes de `HoldBreathSection`; removidas `*_Regen`/`_HoldBreathArmStaminaDrain`; `AddComponent<StaminaDebugUI>`; enables dos 2 patches. |
| MODIFICADO | `modded-beta/StanceManager.cs` | `CachedAimDrainRate` exposto; `TickStanceStamina` reduzido ao re-apply do speed-limit; `ApplyStaminaStance` sem `StaminaMultiplier`. |
| MODIFICADO | `modded-beta/StanceConfig.cs` | Campo `StaminaMultiplier` removido (migrou). |
| MODIFICADO | `modded-beta/StanceStaminaState.cs` | `Multiplier`/`ShouldApplyStamina` aposentados; sobra `IsSuspendedByProne`. |
| MODIFICADO | `modded-beta/Patches/PassiveMountDetectPatch.cs` | Corner case: passivo só em Stance 0 ou ADS. |
| MODIFICADO | `modded-beta/Patches/ApplyComplexRotationPatch.cs` | Removido o arm-drain do hold-breath (virou multiplicador); oxigênio mantido. |
| MODIFICADO | `modded-beta/Patches/RaidLifecyclePatches.cs` | `StaminaController.Reset()` no OnRaidEnd. |
| MODIFICADO | `PROPRIEDADES.md` | Seção `Stamina Management` (15 props); migração dos `Stance X Stamina Multiplier`; remoções. |

## Pontos da review tratados no build

| ID | Resumo | Como foi tratado |
|---|---|---|
| PA-01-01 | Ordem Tick × Process | Gate por flag `ControllingHands` (defasagem ≤1 frame segura); comentado no Prefix. |
| PA-01-02 | `FieldInfo` de evento null | Null-guard nos disparos + log único se faltarem. |
| PA-01-03 | Buffs/skills ignorados | Aceito (controle 100%); documentado em PROPRIEDADES ("não se somam"). |
| PA-01-04 | `StanceStaminaState.Multiplier` órfão | Removido (sobra `IsSuspendedByProne`). |

## Decisões de implementação (assunções)

- **`_PassiveStaminaSave`** reaproveitado como gate de captura de stamina do passivo (off = passivo não mexe na stamina).
- **Ordenação do grupo no F12** (assunção a validar): nome `Stamina Management` + Bind antes de `HoldBreathSection`, apostando em ordem de descoberta. Se no F12 não ficar acima de "9. Respiração", prefixar o nome (1 linha).
- **Mãos vazias / não-arma** → controlador cede ao vanilla.

## Pendências de validação in-game (antes de 🟢)

- F12: grupo `Stamina Management` acima de "9. Respiração"; Stance 3 abaixo de Stance 2.
- Debug: os 15 cenários batem com o estado; transição troca o label na hora (nunca dois ativos).
- Cada multiplicador manda (valor extremo responde só no cenário); sem oscilação/resíduo; sem mexer no F12 para destravar.
- **Stamina de perna intacta** (sprint normal — gate não vazou).
- Corner case (passivo só Stance 0/ADS); lifecycle (raid1→raid2, morte); Fika (só local).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Build concluído via `/code-mod` — compila 0 erros; status 🟡 (aguarda validação in-game) |
| 2026-06-21 | Code review 01 (2 revisores independentes) + aplicação: null-check `p` (Tick/Resolve), Reset no OnRaidStart, doc do toggle `_PassiveStaminaSave`. Recompila 0 erros. |
