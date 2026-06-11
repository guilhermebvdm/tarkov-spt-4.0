# 008 — Stance para Recarga/Checagem · 06-fix-01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-11
**Status:** 🟡 Implementado — requer validação in-game

## Necessidade

O item original cobria recarga, checar munição/câmara, examinar arma e checar modo de fogo. Faltava o comando **"Esvaziar câmara"** (unload chamber): deve igualmente levantar a arma para Stance 0, executar a animação e retornar à stance anterior.

## Correção

| Arquivo | Mudança |
|---|---|
| `modded/Patches/ActionStancePatches.cs` | Nova classe `ActionStanceUnloadChamberPatch` — Prefix em `Player.FirearmController.GClass2046.Start()` (operação de unload-chamber, deriva de `GClass2013`) → `StanceManager.StartActionStance()`. Resolve o `FirearmController` via `FirearmController_0` com fallback no `MainPlayer`. Guard `ChamberAmmoCount > 0`. Log `[ActionStance] UnloadChamber`. |
| `modded/Plugin.cs` | Registro do patch junto às demais Action Stance. |

**Fim da animação:** reusa o `ActionStanceOnIdlePatch` (`method_45` / OnIdle) — sem delay fixo.

**Reusa** o toggle `_EnableActionStanceSwap` (sem nova config).

## APIs (validadas Assembly 0.16)

`GClass2046 : GClass2013` (ctor `(FirearmController)`, `Start()` parameterless + `RemoveAmmoFromChamber()`); `method_45` (OnIdle).

## Critérios de aceite

- [ ] Low Ready → esvaziar câmara → sobe p/ Stance 0 → executa → volta a Low Ready.
- [ ] Retorno só após o fim real da animação (OnIdle).
- [ ] Respeita o toggle; com a feature off, esvazia na postura atual.
- [ ] Não interfere em ADS/melee/granada; sem resíduo entre raids.

## Premissas assumidas (validar in-game)

1. **`GClass2046` dispara com câmara CHEIA** (esvaziar). O guard `ChamberAmmoCount > 0` mantém o item disjunto do **010** (manual chambering age em câmara VAZIA via `ECommand.ChamberUnload` antes da operação). O log `[ActionStance] UnloadChamber (ChamberAmmoCount=…)` confirma isso na 1ª execução; se também disparar com câmara vazia, o guard já evita o swap espúrio.
2. **Patchar a base `GClass2046` inclui `FixMalfunctionOperationClass`** (corrigir malfunção também levanta a arma) — aceito como desejável. Para separar, patchar só `GClass2047`/`GClass2049`.
3. **(code-review F5)** O fim da stance depende do `method_45` (OnIdle) disparar ao término do unload-chamber. Como `GClass2046.Start()` é parameterless (sem `Callback`), não há como envolver um callback de fim como no `ActionStanceUnloadMagPatch`. Se a operação não retornar ao idle, a arma pode ficar "levantada" até o jogador correr/montar (`EndActionStance(forceCancel)` no `Update` mitiga — sem softlock permanente). **Validar in-game.**
4. **(code-review F7)** O fallback ao `MainPlayer` (quando `FirearmController_0` não resolve) pode disparar o swap por uma operação de peer remoto em Fika/coop. Em SP puro, sem efeito.
5. **(code-review)** `.Enable()` do patch envolto em try/catch — se `GClass2046` não resolver em algum build 0.16, só esta feature é desabilitada (log `[008]`), o resto do mod carrega.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-11 | Claude (autônomo) | 06-fix-01: adiciona "esvaziar câmara" ao action-stance via GClass2046; guard de câmara cheia p/ disjunção com 010. Não testado in-game. |
