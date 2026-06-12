# 004 — Apoiar arma / Mount · 06-fix-01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-11
**Status:** 🟡 Implementado — requer validação in-game

## Sintomas

1. **Mount passivo "grudava":** encostar a ponta da arma na parede deslocava completamente arma/braço (desalinhava a Stance 0), exigindo reset manual das mãos.
2. **Mount ativo inconsistente:** superfícies válidas para o passivo não permitiam o mount ativo (critérios diferentes).
3. Ao sair do mount ativo, a arma voltava à Stance 0 com offset residual.

## Causa raiz

- O **"grude"** (`MountingCollisionPatch`, Postfix em `AvoidObstacles`, desloca `WeaponRoot.localPosition`) rodava quando `!IsMounting` — ou seja, no **passivo**. Era o **inverso** do desejado.
- **Critérios diferentes:** passivo usava raycast custom; ativo usava o mount **nativo** do EFT (`MovementContext.IsInMountedState`) → conjuntos de superfície distintos.
- O `TurnAwayEffector` vanilla ficava **permanentemente** zerado; offsets de grude não eram zerados ao sair.

## Correção — sistema próprio unificado (decisão do usuário)

| Arquivo | Mudança |
|---|---|
| `modded/MountingManager.cs` | Estado explícito `EMountState { None, Passive, Active }` (substitui a dependência do mount nativo). `IsMounting => Active`, `IsBracing => Passive` (exclusivos, revisão #2). `SetMountState` central (anim `SetMounted`, Fika sync, force Stance 0 ao entrar, `ResetCollisionOffsets` ao sair). `DetectBracing(fc, player, ln)` chamado pelo patch de `method_11`. |
| `modded/Patches/WeaponMountingPatch.cs` | **Novo** `FirearmCollisionDetectPatch` — Prefix de leitura em `Player.FirearmController.method_11` (modelo Realism `CollisionPatch.cs:209`), alimenta a detecção com o `ln` real e throttle. `MountingCollisionPatch`: grude **só no Active**; `ResetCollisionOffsets()`; cacheia e **restaura** o `TurnAwayEffector` fora do ativo. Sway/recoil inalterados (semântica nova já cobre full/partial). |
| `modded/Patches/MountingInputPatch.cs` | **Reabilitado.** Mount ativo via `ECommand.WeaponMounting (140)` em `TranslateCommand` (modelo Realism `KeyInputPatch2`). Toggle Active↔None; suprime o mount nativo (`return false`) **exceto com bipé** (`IsBipodUsed`). |
| `modded/StanceManager.cs` | `TickStanceStamina`: suspende o drain enquanto `MountState != None` (revisão #5 — apoiar não drena; o vanilla regenera em hipfire). |
| `modded/Plugin.cs` | Registro de `FirearmCollisionDetectPatch` + `MountingInputPatch`. |

## APIs (validadas Assembly 0.16)

`Player.FirearmController.method_11(origin, ln, out overlapsWithPlayer, weaponUp)`; `ECommand.WeaponMounting (140)`; `ProceduralWeaponAnimation.{AvoidObstacles, ProcessEffectors, TurnAway, IsBipodUsed}`; `FirearmsAnimator.SetMounted`; `TurnAwayEffector.{_blendSpeed,_inSmoothTime,_outSmoothTime}`.

## Critérios de aceite

- [ ] Encostar a arma na parede **não** desloca/quebra a Stance 0 (passivo SEM grude).
- [ ] Passivo dá benefícios (recoil/sway reduzidos parcial) + ícone transparente; stamina de braço não drena.
- [ ] Ativar a tecla com superfície detectada → mount ativo (grude + ícone sólido).
- [ ] Toda superfície válida para passivo permite ativo (mesmo raycast).
- [ ] Ao desmontar o ativo → Stance 0 correta, sem offset residual.
- [ ] Sair da posição / correr cancela o mount; sem impacto em bots; sem resíduo entre raids.

## Ponto de referência do raycast (gap fechado)

Mantido **`WeaponRootAnim`** (ponta da arma) — ponto canônico de colisão de arma do EFT, respeita o comprimento real via `ln`. Confirmado pelo Realism `CollisionPatch`, que usa o mesmo método/ponto e **não** aplica grude (só flags + bônus) → valida "passivo sem grude".

## Premissas assumidas (decididas autonomamente — validar in-game)

1. **Suprimir o mount nativo** (cmd 140 → `return false`), exceto com bipé. Se conflitar, expor toggle `Suppress Native Mounting`.
2. **`method_23` (reset de pose) OMITIDO** — risco de conflito com o controle de springs do stance (revisão #8). Confia-se em `ResetCollisionOffsets` + force Stance 0. Se houver resíduo visual, reavaliar.
3. **Magnitudes do grude** mantidas (alvo de Stance 0); podem precisar de re-tuning agora que só atuam no ativo (antes "vazavam" no passivo).
4. **Cache do `TurnAwayEffector`** lê os defaults no 1º mount ativo do boot — assume que o effector é recriado com defaults a cada boot/raid.
5. **Fallback de tecla implementado:** a `Weapon Mounting Hotkey` (F12, default `Mouse3`) alterna o mount ativo no `Update` via `MountingManager.ToggleActiveMount()`, além da tecla nativa do EFT (cmd 140). Com bipé, deixa o nativo. (Antes a config estava órfã — não-lida.)

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-11 | Claude (autônomo) | 06-fix-01: sistema de mount próprio unificado (EMountState); grude só no ativo + reset; detecção via method_11; supressão do nativo; stamina suspensa. Não testado in-game. |
