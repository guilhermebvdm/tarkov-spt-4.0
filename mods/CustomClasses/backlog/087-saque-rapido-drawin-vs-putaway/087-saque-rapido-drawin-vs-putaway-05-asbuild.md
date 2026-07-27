# 087 — Bugfix: Saque Rápido acelerava o put-away, não o draw-in · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-27 · **Versão:** 0.13.0 → **0.13.2**

Segunda correção do "Saque Rápido" (080), achada na validação in-game (report do usuário: "acelerou a SAÍDA da pistola, não o saque"; config com Sidearm=tecla 3, quickdraw *Not set*).

## Modelo mecânico (diagnóstico do decompile)
Na troca de arma o EFT tem **dois controles de velocidade independentes**:

| Fase | Controlado por | Vanilla |
|---|---|---|
| **DRAW-IN** (sacar/trazer à mão) | `Animator.speed` GLOBAL, via arg `animationSpeed` de `FirearmController.Spawn` (Player.cs:13495 → estado SPAWN/GClass2055) | hardcoded `1f` — o saque NUNCA acelera por skill |
| **PUT-AWAY** (guardar) | float `SpeedDraw` (=`SwapSpeed`) via `SetAnimatorAndProceduralValues` | acelera por skill |

O 086 escalava `SwapSpeed` → alimentava `SpeedDraw` → acelerava só o **put-away**. Por isso o usuário viu a *saída* da pistola acelerar, não o saque. Prova BSG (observed-player): `GClass2949` (spawn) seta só `Animator.speed`; `GClass2944` (put-away) seta `SpeedDraw`. Não existe parâmetro de skill para o draw-in no vanilla — acelerar o saque é mecânica NOVA, via o `Animator.speed` do Spawn.

## Conserto
- `HolsterDrawSpeedPatch` → **Prefix em `FirearmController.Spawn(float animationSpeed, Action)`**, escala `animationSpeed /= t` (draw-in mais rápido) + seta flag `BoostedDraw`. Gate: MainPlayer local (075, HandsController já setado antes do Spawn) + classe (Hunter/Rifleman/Stealth) + a arma que ENTRA vem do slot Holster.
- `HolsterDrawResetPatch` → **Postfix em `SetAnimatorAndProceduralValues`**, restaura `FirearmsAnimator.SetAnimationSpeed(1f)` no 1º disparo pós-Spawn (`GClass2055.WeaponAppeared` = fim do draw-in). ⚠️ Obrigatório: `Animator.speed` é global e o vanilla não reseta — sem isso a pistola atiraria/recarregaria/idle acelerada até a próxima troca.
- Flag `BoostedDraw` (static) blindada no raid-start (RaidPerksNotificationPatch).

## Arquivos
| Ação | Path |
|---|---|
| MOD | `Patches/ClassWeaponPatches.cs` (HolsterDrawSpeedPatch → Spawn; +HolsterDrawResetPatch) |
| MOD | `Plugin.cs` (+`HolsterDrawResetPatch().Enable()`) · `Patches/RaidPerksNotificationPatch.cs` (+reset da flag) |

## Code-review (sub-agent adversarial) — 0 bloqueadores
| Sev | Achado | Nota |
|---|---|---|
| ✅ | Timing do reset | dispara em `WeaponAppeared` (fim do draw-in); nenhum `SetAnimatorAndProceduralValues` entre Spawn e WeaponAppeared no caminho normal; `SyncWithCharacterSkills` roda ANTES do Spawn (flag ainda false) |
| ✅ | Gate 075 | `SpawnController` seta `HandsController` antes de `Spawn` → gate passa p/ local, barra bots/peers |
| ✅ | Reset suficiente | caminho feliz sempre reseta; caminhos que pulam = arma sendo destruída (animator descartado); próximo Drop/Spawn passa 1f |
| 🟡 | Flag `BoostedDraw` static atravessa raids | auto-curável (reset só escreve 1f, idempotente) — **blindado** com reset no raid-start |
| 🟢 | 3 gatilhos assíncronos (dano de braço/skill/mastery) podem cortar o boost de um saque | raro e benigno (perde o speedup 1×, sem estado preso) |

## Pendências de validação in-game
- Apertar **Sidearm (tecla 3, saque normal)** puxa a pistola do holster mais rápido. Put-away (guardar ao trocar p/ primária) e demais armas inalterados. Teste exagerado: F12 `Quick Draw — Draw time mult` = 0.3.
- Confirmar que a pistola NÃO fica acelerada (tiro/reload) após o saque (o reset funcionou).

## Histórico
| Data | Evento |
|---|---|
| 2026-07-27 | Bugfix via g-autodev (2ª correção do 080); diagnóstico draw-in(Animator.speed)×put-away(SpeedDraw); Prefix no Spawn + reset; code-review 0 bloqueadores; 0.13.2 |
