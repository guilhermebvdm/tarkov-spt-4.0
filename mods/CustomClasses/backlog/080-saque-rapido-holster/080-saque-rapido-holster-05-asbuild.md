# 080 — Saque Rápido (deploy do holster) · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.7.1 → **0.8.0**

Perk NOVO (Caçador + Fuzileiro + Furtivo): sacar a arma do slot **HOLSTER** mais rápido.

## Ponto de patch (via sub-agent no assembly)
- **`Player.FirearmController.GetWeaponDrawSpeedMultiplier(Weapon, bool)`** (Player.cs:12591) — análogo do `GetWeaponReloadAnimationSpeed`; retorna a VELOCIDADE do parâmetro `draw` do animator (maior = mais rápido). Postfix `__result /= tempo` (0.8 → speed ×1.25).
- Gate Holster: `weapon.CurrentAddress.Container == MainPlayer.Inventory.Equipment.GetSlot(EquipmentSlot.Holster)` (padrão canônico do EFT, Player.cs:12637).

## Arquivos
| Ação | Path | Resumo |
|---|---|---|
| MOD | `PerksConfig.cs` | +`QuickDrawEnabled`/`QuickDrawTime` (0.8; compartilhado, SecHunter) |
| MOD | `Patches/ClassWeaponPatches.cs` | +`HolsterDrawSpeedPatch` (Postfix, gate MainPlayer+classe+Holster) |
| MOD | `PerksCatalog.cs` | +grupo `quick_draw`; ByClass Hunter/Rifleman/Stealth +quick_draw |
| MOD | `Plugin.cs` | +`new HolsterDrawSpeedPatch().Enable()` |

## Auto-revisão (perk simples, espelha ReloadSpeedPatch)
- **Gate/075:** `ReferenceEquals(__instance, MainPlayer.HandsController)` + `IsLocalClass` + Holster → só o player local, não vaza p/ bot/peer. ✅
- **Semântica:** `__result` é SPEED (maior=rápido); `/= 0.8` = ×1.25. ✅
- **Corner:** `t < 1f` evita no-op quando o perk está "desligado" via valor 1. Fail-safe no catch.
- ⚠️ **Risco (validar in-game):** o gate por Holster assume que a arma ainda está no slot quando `GetWeaponDrawSpeedMultiplier` é chamado. Se o EFT já moveu a arma, o gate falha e o perk não aplica (sem crash). Se in-game não acelerar, remover o gate explícito de Holster e confiar no gate implícito (o método já roda no contexto de quickdraw/deploy da secundária).
- **Assunção:** config compartilhada entre as 3 classes (mesmo valor 0.8). Nome EN "Quick Draw".

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Build via g-autodev; ponto de patch localizado por sub-agent; 0.8.0, build 0/0 |
