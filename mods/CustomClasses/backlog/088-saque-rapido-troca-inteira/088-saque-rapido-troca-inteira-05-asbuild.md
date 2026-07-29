# 088 — Saque Rápido: acelerar a troca inteira (put-away + transição) · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-27 · **Versão:** 0.13.2 → **0.14.0**

Extensão do Saque Rápido pedida na validação in-game: com só o draw-in (fase 3) acelerado, o começo da troca destoava.

## As 3 fases da troca (decomposição do usuário → mecânica)
| Fase | O que é | Mecânica EFT | Antes do 088 |
|---|---|---|---|
| 1 | Saída da arma anterior (put-away) | `FirearmController.Drop(animationSpeed,…)` → `Animator.speed` global do controller que SAI | lento (não tocado) |
| 2 | Transição (iniciar o saque) | encadeamento de callbacks — o `Spawn` só começa quando o put-away dispara `OnWeapOut`; SEM timer | lento (consequência da 1) |
| 3 | Saque (draw-in) | `FirearmController.Spawn(animationSpeed)` → `Animator.speed` do estado SPAWN | acelerado (087) |

**Causalidade (diagnóstico):** o callback do `Drop` dispara no evento de animação `OnWeapOut` do put-away → acelerar a fase 1 antecipa todo o encadeamento create→spawn, encurtando a fase 2 **de brinde**. Não há `WaitForSeconds` a remover.

## Conserto
`HolsterPutAwaySpeedPatch` — Prefix em `FirearmController.Drop(float animationSpeed, Action, bool, Item nextControllerItem)`, escala `animationSpeed /= t` (put-away mais rápido). **Gate INVERTIDO vs. a fase 3:** aqui `__instance.Item` é a arma que SAI (primária); quem está no Holster é o `nextControllerItem` (a arma que ENTRA). Gate: MainPlayer local (075, no Drop o HandsController ainda é a arma que sai) + classe + `nextControllerItem` vem do Holster. **SEM reset** (controller que sai vai pro pool; todo `Spawn` reescreve o speed incondicionalmente → resíduo zerado no próximo saque). Mesmo `QuickDrawTime` das duas fases → troca uniforme; o usuário pode afrouxar a fase 3 e distribuir o ganho.

## Arquivos
| Ação | Path |
|---|---|
| MOD | `Patches/ClassWeaponPatches.cs` (+HolsterPutAwaySpeedPatch) · `Plugin.cs` (+Enable) |

## Code-review (sub-agent adversarial) — 0 bloqueadores
| Sev | Achado | Nota |
|---|---|---|
| 🟢 | Gate invertido | `CurrentAddress.Container` ≡ `Parent.Container` (Item.cs:481), null-safe; primária→pistola passa, pistola→primária/faca/vazio não |
| 🟢 | Gate 075 | no Drop `__instance` É o HandsController (só nulado no callback/DestroyController); bots/peers barrados |
| 🟢 | Sem reset | controller vai pro POOL (não destruído) mas todo Spawn reescreve o speed → sem vazamento; comentário atualizado |
| 🟢 | Coexistência | Drop/Spawn/SetAnimator… são métodos distintos; fase 1 não seta BoostedDraw; sem cross-talk |
| 🟡 | `fastDrop` + boost | eixos independentes (fastDrop usa SpeedDraw, não animationSpeed) → sem double-dip no mesmo valor, mas swaps já-fast podem ficar > que o QuickDrawTime sugere — **validar o feel in-game** (balanceamento, não bug) |

## Pendências de validação in-game
- Trocar PARA a pistola do holster (Sidearm): a troca INTEIRA (guardar a primária + transição + saque) acelera de forma uniforme.
- Trocar DA pistola para a primária: inalterado (só acelero quando a arma que ENTRA vem do holster).
- Afrouxar a fase 3 (F12 `Quick Draw — Draw time mult` ↑) e conferir que o total fica bom.

## Histórico
| Data | Evento |
|---|---|
| 2026-07-27 | Extensão via g-autodev (feedback in-game); fase 1 (Drop) + fase 2 de brinde; gate invertido (nextControllerItem); code-review 0 bloqueadores; 0.14.0 |
