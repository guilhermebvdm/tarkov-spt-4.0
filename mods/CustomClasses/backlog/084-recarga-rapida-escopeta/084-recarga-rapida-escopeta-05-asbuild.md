# 084 — Recarga Rápida Escopeta · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.11.1

Perk NOVO (Tanque): recarga de escopeta de tubo (shell-a-shell) mais rápida.

## Spike — mecânica elite INVIÁVEL → fallback aprovado (−40% tempo)
A mecânica sonhada ("Mag Drills elite = carregar 2 cartuchos por vez") **NÃO existe no EFT 0.16.9**: não há skill/buff/getter que faça a escopeta tubular carregar 2 shells por ciclo. O número de shells é dirigido por EVENTOS de animação (`Int_0++`, 1 por evento), sem gate de skill nem getter interceptável. Forçar 2 exigiria mutar o campo `Int_0` com dessync de animação/HUD/rede. **Decisão do usuário (múltipla escolha, 2026-07-26): fallback = reduzir o TEMPO de recarga, intensidade −40%** (mult 0.6, ~1,7× mais rápido).

## Implementação
- `ShotgunReloadPatch` — Prefix+Postfix em `FirearmController.SetAnimatorAndProceduralValues()` (o funil push-based real do reload speed; lê o CAMPO `BuffInfo.ReloadSpeed` e o repassa a DOIS animators — arma + corpo — em lockstep).
  - **Prefix** escala `BuffInfo.ReloadSpeed /= t` ANTES do push → os dois animators recebem o valor acelerado, sem dessincronia, sem tocar draw/swap. **Postfix** RESTAURA o original (via `__state`) → não acumula entre syncs nem vaza p/ outros consumidores do `GClass2250`.
  - ⚠️ O getter `GetWeaponReloadAnimationSpeed` é **código morto** no 0.16.9 (ver [[reference_eft_reload_speed_getter_dead]]) — o molde do Adrenaline (Postfix no getter) NÃO serve.
- Gate: MainPlayer local (075) + Tank + `WeapClass=="shotgun"` + `Weapon.SupportsInternalReload`.
- Config `ShotgunReloadTime` (default 0.6, range [0.4,1.0], seção Tank) · catálogo grupo `shotgun_reload` · ByClass Tank +shotgun_reload · Plugin `Enable`.

## Arquivos
| Ação | Path |
|---|---|
| MOD | `Patches/ClassWeaponPatches.cs` (+ShotgunReloadPatch no fim) |
| MOD | `PerksConfig.cs` (+ShotgunReloadEnabled/Time) · `PerksCatalog.cs` (+shotgun_reload, ByClass Tank) · `Plugin.cs` (+Enable) |

## Code-review (sub-agent adversarial) — 1 bloqueador corrigido
| Sev | Achado | Resolução |
|---|---|---|
| 🔴 | `SupportsInternalReload` sozinho pega **bolt-action (Mosin), SKS, revólver, M32** (todos InternalMagazine) — o perk aceleraria a recarga deles | **Corrigido** — +`WeapClass=="shotgun"` no gate → exatamente as 8 escopetas de tubo (Saiga/bicano ficam de fora) |
| 🟡 | Postfix re-setava só o animator da ARMA → dessincronia com o do CORPO (o base atualiza os dois) | **Corrigido** — trocado p/ Prefix escala o campo (ambos animators em lockstep) + Postfix restaura |
| 🟢 | Repassar swap no Postfix clobrava o draw da branch quickdraw-fast | **Corrigido** pela mesma mudança (não toca mais em draw/swap) |

**Verificado limpo:** gate 075 canônico (bots/peers barrados); sem acumulação (restaura o original); null-safe (`Item`/`BuffInfo` guardados); div/0 coberto (t∈[0.4,1)).

## Pendências de validação in-game (feedback_spt_validation)
- Confirmar que a recarga da escopeta acelera de fato (~1,7×) e que mãos×corpo não dessincronizam.
- Confirmar que Saiga (carregador) NÃO é afetada e que bolt-action/SKS/revólver seguem normais.

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Build via g-autodev; spike (elite inviável→fallback tempo, −40% escolhido pelo user); code-review 🔴 (bolt-action) + 🟡 (dessync animators) corrigidos; 0.11.1 |
