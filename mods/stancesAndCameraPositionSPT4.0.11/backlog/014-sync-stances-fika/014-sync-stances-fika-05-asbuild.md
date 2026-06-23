# 014 — Corrigir sync visual de stances no Fika · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-22
**Spec funcional:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Spec técnica:** [014-sync-stances-fika-02-spec-tech.md](014-sync-stances-fika-02-spec-tech.md)
**Reviews técnicas:** [03-spec-tech-review-01.md](014-sync-stances-fika-03-spec-tech-review-01.md) (0 🔴)

> Corrige a aplicação remota: o offset de stance passa a ser escrito no `HandsContainer.WeaponRootAnim` do jogador observado (braço **e** arma juntos), aditivo sobre a pose nativa (lean/ombro/mira), em vez de girar o `PlayerBones.Spine3`. Networking inalterado. Compila 0 erros; **aguarda validação in-game (2 clientes Fika)**.

## Arquivos alterados

| Ação | Arquivo | Resumo |
|---|---|---|
| MODIFICADO | `modded-beta/Networking/ObservedStanceAnimator.cs` | Removido `LateUpdate`/Spine3. Virou state-holder por player + `ApplyTo(pwa, weaponPosition, weapRotation, dt)` que avança o spring e escreve o `WeaponRootAnim` (mesma fórmula do local). |
| MODIFICADO | `modded-beta/Patches/ApplyComplexRotationPatch.cs` | Gate `!IsYourPlayer` desviado para `ObservedStanceAnimator.ApplyTo` (com safeguards), antes do bloco MainPlayer. `SpringLerpAngle`/`SpringLerp` agora `public`. |
| MODIFICADO | `modded-beta/StanceManager.cs` | Overloads `GetTargetRotation(Stance,bool)` / `GetTargetPosition(Stance,bool)`; os existentes delegam com `CurrentStance`. |

> Networking (`FikaSyncManager`/`StanceSyncPacket`) **inalterado** — já enviava `ProfileId`+`Stance`+`IsAiming` corretamente.

## Pontos da review tratados no build

| ID | Resumo | Como foi tratado |
|---|---|---|
| PA-01-01 | Timing Postfix × cópia PlayerBones | Mantido (Postfix de `ApplyComplexRotation` dentro do `ProcessEffectors`, antes da cópia). **Validar in-game**; plano B documentado. |
| PA-01-02 | Coexistência lean/ombro | Offset aditivo sobre `weapRotation` (pose nativa). **Validar combinações in-game.** |
| PA-01-03 | Edição de `RaidLifecyclePatches` desnecessária | Removida do escopo (estado nos components). |

## Pendências de validação in-game (2 clientes Fika, antes de 🟢)

- O outro player vê a **arma acompanhar** a stance (não só o tronco); pose remota = pose local.
- **Stance + lean** (esq/dir) e **stance + troca de ombro** combinam sem conflito; sequências nos dois sentidos.
- Lean/ombro **vanilla** seguem funcionando.
- ADS + stance no remoto; troca rápida; vários remotos; spawn/despawn/morte sem órfão/erro.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Build concluído via `/code-mod` — compila 0 erros; status 🟡 (aguarda validação in-game) |
| 2026-06-22 | Code review 01 (2 revisores independentes) + aplicação: clamp do stance de rede (CR-01-01). Recompila 0 erros. |
