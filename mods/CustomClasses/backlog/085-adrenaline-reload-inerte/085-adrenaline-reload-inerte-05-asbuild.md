# 085 — Bugfix: Adrenaline Reload estava inerte · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.11.1 → **0.12.1**

Conserto de bug latente achado durante o spike do 084.

## O bug
O `ReloadSpeedPatch` (perk "Adrenaline Reload" do Fuzileiro: −30% tempo de recarga na janela de combate) fazia Postfix em `Player.FirearmController.GetWeaponReloadAnimationSpeed` — **código morto** no EFT 0.16.9 (nada o chama; o reload speed virou push-based, escrito no float `SpeedReload` do animator por `SetAnimatorAndProceduralValues`). O Postfix **nunca disparava** → o perk não tinha efeito. O 078 recalibrou esse valor (0.80→0.7) sem saber. Ver [[reference_eft_reload_speed_getter_dead]]. Decisão do usuário (múltipla escolha): consertar agora.

## Conserto
- **`ReloadSpeedPatch`** migrado para o mesmo funil do 084: Prefix em `SetAnimatorAndProceduralValues` escala `BuffInfo.ReloadSpeed ÷ t` antes do push (arma+corpo em lockstep); Postfix restaura via `__state`. Gate MainPlayer local (075) + Rifleman + janela ativa. Vale p/ qualquer arma. Coexiste com o `ShotgunReloadPatch` (084) no mesmo método sem conflito (classes exclusivas Tank×Rifleman → só um escala).
- **`AdrenalineState` watcher:** abrir/fechar a janela NÃO gera sync sozinho → `EnsureReloadResync()` inicia um coroutine (1 por janela) que força `FirearmController.SetAnimatorAndProceduralValues()` na abertura e quando `IsActive` vira false → o patch re-avalia e aplica/restaura imediatamente.
- **`AdrenalineTriggerPatch`** chama `EnsureReloadResync()` após `Trigger()`.

## Arquivos
| Ação | Path |
|---|---|
| MOD | `Patches/ClassWeaponPatches.cs` (ReloadSpeedPatch reescrito: getter morto → Prefix/Postfix em SetAnimatorAndProceduralValues) |
| MOD | `AdrenalineState.cs` (+watcher: EnsureReloadResync/WatchWindow/ForceReloadResync, handle de Coroutine, StopCoroutine no Reset) |
| MOD | `Patches/AdrenalineTriggerPatch.cs` (+EnsureReloadResync após Trigger) |

## Code-review (sub-agent adversarial) — 0 bloqueadores
| Sev | Achado | Resolução |
|---|---|---|
| 🟡 | `EnsureReloadResync` rodava em TODO trigger, inclusive em cooldown → spam de SetAnimatorAndProceduralValues (2×/hit, pior com pellets/rajadas) | **Corrigido** — guard `if (!IsActive) return` (só arranca com a janela já aberta) |
| 🟡 | `_watching` (bool) era latch sem reset externo — coroutine órfão travaria o re-sync pela sessão | **Corrigido** — trocado por handle de `Coroutine` + `StopCoroutine` no `Reset()` |
| 🟢 | resync manual pode roubar o one-shot de fast-draw do holster | Mitigado pelo guard CR#1 (invocações despencam); janela estreitíssima |
| 🟢 | comentário superestimava "congelaria" | **Afinado** — o valor do watcher é a aplicação/restauração imediata nas transições |

**Verificado limpo:** gate 075 (bots/peers barrados; ForceReloadResync mira só MainPlayer); dois patches no mesmo método sem conflito de `__state` (Harmony isola por patch; classes exclusivas); fechamento restaura o speed normal (branch comum + SyncWithCharacterSkills reescreve); null-safe; máx. 1 coroutine.

## Pendências de validação in-game (feedback_spt_validation)
- Confirmar que a recarga do Fuzileiro acelera ~30% DURANTE a janela de Adrenalina e VOLTA ao normal ao expirar.
- Confirmar que fora da janela (e outras classes) a recarga é normal.

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Bugfix via g-autodev (achado no spike do 084); getter morto→funil push-based + watcher; code-review 2×🟡 corrigidos; 0.12.1 |
