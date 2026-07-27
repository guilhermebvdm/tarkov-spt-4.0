# 086 — Bugfix: Saque Rápido (080) só afetava o quickdraw-fast · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.12.1 → **0.13.0**

Conserto de bug de cobertura achado na **validação in-game** (report do usuário, Caçador: "não senti diferença no saque do coldre").

## O bug
O `HolsterDrawSpeedPatch` (080) fazia Postfix em `Player.FirearmController.GetWeaponDrawSpeedMultiplier`. Esse getter **está vivo**, mas é lido em apenas 2 call-sites, ambos do **quickdraw-fast** (double-tap com `FastSlotSelection`, arma do holster, fora de prone — Player.cs:10091 e :12648). O **saque NORMAL** (trocar de slot pela tecla) cai no ramo `else` de `SetAnimatorAndProceduralValues` (Player.cs:12661) e usa o campo push-based `BuffInfo.SwapSpeed` (→ float `SpeedDraw` do animator) — que o patch antigo **não tocava**. Mesma natureza do getter morto do 085. Ver [[reference_eft_reload_speed_getter_dead]].

## Conserto
`HolsterDrawSpeedPatch` migrado para o funil real (molde 084/085): Prefix escala `BuffInfo.SwapSpeed ÷ t` antes do push (arma+corpo em lockstep), Postfix restaura via `__state`. Gate: MainPlayer local (075) + classe (Hunter/Rifleman/Stealth) + a arma nas mãos veio do slot **Holster** (`Item.CurrentAddress.Container == Equipment[Holster]`). Cobre o saque comum; o quickdraw-fast já é intrinsecamente rápido (cobertura perdida é aceitável).

## Arquivos
| Ação | Path |
|---|---|
| MOD | `Patches/ClassWeaponPatches.cs` (HolsterDrawSpeedPatch: getter só-quickdraw → Prefix/Postfix em SetAnimatorAndProceduralValues) |

## Code-review (sub-agent adversarial) — 0 bloqueadores
| Sev | Achado | Nota |
|---|---|---|
| 🟢 | Gate de holster | `CurrentAddress.Container` ≡ `Parent.Container` (Item.cs:481); `ReferenceEquals` ≡ ao `.Equals` do vanilla (Slot não sobrescreve Equals); mais seguro que o vanilla (null-safe) |
| 🟢 | Coexistência 3 patches | 080 mexe em SwapSpeed, 084/085 em ReloadSpeed (campos disjuntos); Harmony isola `__state` por patch; ordem irrelevante |
| 🟢 | Timing | SetAnimatorAndProceduralValues do saque roda após HandsController setado e BuffInfo populado → sem gap |
| 🟢 | Borda | saque com braço danificado usa `SetSpeedParameters()` sem args (draw=1) → perk sem efeito ali, espelha o vanilla |

**Verificado limpo:** gate 075 (bots/peers barrados); sem vazamento (único leitor de SwapSpeed é o próprio método; Postfix restaura); null-safe; sem div/0 (t∈[0.3,1)).

## Pendências de validação in-game
- Saque da pistola do **holster** (troca normal pela tecla do slot) fica ~20% mais rápido como Caçador/Fuzileiro/Furtivo. (Teste exagerado: F12 `Quick Draw — Draw time mult` = 0.3.)

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Bugfix via g-autodev (report de validação in-game); getter só-quickdraw → funil push-based SwapSpeed; code-review 0 bloqueadores; 0.13.0 |
