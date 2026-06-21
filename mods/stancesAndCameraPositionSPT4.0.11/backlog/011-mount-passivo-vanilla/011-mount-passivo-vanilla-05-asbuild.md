# 011 — Mount passivo sobre o vanilla · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-21
**Spec funcional:** [011-mount-passivo-vanilla-01-spec.md](011-mount-passivo-vanilla-01-spec.md)
**Spec técnica:** [011-mount-passivo-vanilla-02-spec-tech.md](011-mount-passivo-vanilla-02-spec-tech.md)
**Reviews técnicas:** [03-spec-tech-review-01.md](011-mount-passivo-vanilla-03-spec-tech-review-01.md)

> Mount **ativo = 100% vanilla** (o mod não patcha input/estado do mount). Implementado apenas o **passivo**. Compila com 0 erros; **aguarda validação in-game**.

## Arquivos alterados

| Ação | Arquivo | Resumo |
|---|---|---|
| CRIADO | `modded-beta/PassiveMountState.cs` | Estado estático do passivo: `IsBracing`, `Direction`, `LastDetectTick`, `SetBracing/ClearBracing/Reset`. |
| CRIADO | `modded-beta/Patches/PassiveMountDetectPatch.cs` | Postfix em `Player.FirearmController.method_11` (por assinatura) + 3 raycasts (Top/Left/Right) com `origin`/`ln`/`weaponUp` reais; gate `IsYourPlayer`; cede a montado/bipé/prone/sprint. |
| CRIADO | `modded-beta/Patches/PassiveMountBuffPatches.cs` | `PassiveRecoilPatch` (`NewRecoilShotEffect.AddRecoilForce`) + `PassiveSwayPatch` (`ProceduralWeaponAnimation.ProcessEffectors` → `Breath.Intensity`); só quando `IsBracing`. |
| CRIADO | `modded-beta/PassiveMountUI.cs` | Ícone direcional (canto inf. direito) no GameObject do plugin + `BattleUIScreenPatch` (`EftBattleUIScreen.Show`); reset por timeout. |
| MODIFICADO | `modded-beta/Patches/StanceStaminaRecoveryPatch.cs` | Passivo poupa stamina (regen `2.5` vs `5` do ativo), atrás de `Passive Stamina Save`; mount ativo/passivo movidos para antes do guard de multiplier. |
| MODIFICADO | `modded-beta/Plugin.cs` | 5 `ConfigEntry` novas (seção `Weapon Mount (Passive)`); `SafeEnable` dos 4 patches; `AddComponent<PassiveMountUI>()`. |
| MODIFICADO | `modded-beta/Patches/RaidLifecyclePatches.cs` | `PassiveMountState.Reset()` no `GameWorld.OnDestroy`. |
| MODIFICADO | `PROPRIEDADES.md` | Seção "Apoio Passivo de Arma" (5 props). |

## Pontos da review tratados no build

| ID | Resumo | Como foi tratado |
|---|---|---|
| PA-01-01 | Frequência do `method_11` | Fallback no `PassiveMountUI.Update` (timeout `LastDetectTick > 0.3s` solta o estado). Validar a taxa in-game. |
| PA-01-02 | "passivo < ativo" | Multiplicadores como pontos de partida (recoil `0.7`, sway `0.65`). **Calibrar in-game** medindo vs. o vanilla montado. |
| PA-01-03 | Estado preso sem arma | Reset por timeout no `Update` + `Reset()` no raid end. |
| PA-01-04 | Integração de stamina | Regen passivo `2.5` (metade do ativo `5`), atrás de `Passive Stamina Save`. |
| PA-01-05 | Efeito real de recoil/sway | Pontos de patch reusados do item 004 (validados em runtime). **Confirmar efeito in-game.** |
| PA-01-06 | Stub da UI | `PassiveMountUI` baseado no `MountingUI` antigo (`git ebc2312^`), adaptado ao GameObject do plugin. |

## Pendências de validação in-game (antes de 🟢)
- Mount **vanilla** intacto (sem regressão); passivo ativa ao encostar (recoil/sway/stamina + ícone direcional); **passivo < ativo** (calibrar); cede ao montar/bipé/prone/sprint; reset entre raids; Fika (só local); `[enable] OK` dos 4 patches no log.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Build concluído via `/code-mod` — compila 0 erros; status 🟡 (aguarda validação in-game) |
| 2026-06-21 | Code review 01 + aplicação: gate MainPlayer (AP-02) no `PassiveSwayPatch` + `try/catch` nos buffs. Recompila 0 erros. |
