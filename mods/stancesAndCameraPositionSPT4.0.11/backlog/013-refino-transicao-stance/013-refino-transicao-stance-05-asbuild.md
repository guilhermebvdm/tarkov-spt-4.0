# 013 — Refinamentos de transição de stance · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-06-21
**Spec funcional:** [013-refino-transicao-stance-01-spec.md](013-refino-transicao-stance-01-spec.md)
**Spec técnica:** [013-refino-transicao-stance-02-spec-tech.md](013-refino-transicao-stance-02-spec-tech.md)
**Reviews:** [03-spec-tech-review-01.md](013-refino-transicao-stance-03-spec-tech-review-01.md) (0 🔴) · [04-code-review-01.md](013-refino-transicao-stance-04-code-review-01.md) (0 achados, revisor independente)

> Três refinamentos de transição de stance, sem novas configs F12 e sem patches Harmony novos. Compila 0 erros; **aguarda validação in-game**.

## Arquivos alterados

| Ação | Arquivo | Resumo |
|---|---|---|
| MODIFICADO | `modded-beta/StaminaController.cs` | `Tick` permite stationary no gate; `Resolve` → `ActiveStance0` quando `IsStationaryWeaponInHands` (arma montada = Mount Active). |
| MODIFICADO | `modded-beta/StanceManager.cs` | `Update`: detecta `isStationary` e o inclui na condição que força Stance 0; bloco de sprint chama `SnapToNeutral()` após `SetStance(Default)`. |
| MODIFICADO | `modded-beta/Patches/ApplyComplexRotationPatch.cs` | Novo `SnapToNeutral()` — zera offsets + velocidades do spring (snap instantâneo, sem animar pela Stance 0). |

## Ajustes entregues

1. **Arma montada → Mount Active:** `MovementContext.IsStationaryWeaponInHands` (MovementContext.cs:1446) detectado no `StaminaController` → cenário `Active Mount`. Detecção contínua (sai limpo).
2. **Força Stance 0 ao entrar em arma montada:** reusou a condição de força-Default do `StanceManager.Update` (que já cobria mount nativo/prone), incluindo `isStationary`.
3. **Sprint sem flash da Stance 0:** ao forçar Stance 0 no início do sprint (sem TacSprint), `SnapToNeutral()` pula a animação do spring `Stance1→0`; a corrida assume a pose nativa direto. TacSprint preservado.

## Pendências de validação in-game (antes de 🟢)

- **Arma montada:** debug de stamina mostra **`Active Mount`**; entrar de Stance 1/2/3 alinha a arma (vai para Stance 0); sair normaliza.
- **Sprint:** correr a partir de Stance 1/2/3 **não pisca** pela Stance 0; TacSprint (arma leve) intacto; arma pesada/grande sem clipping na corrida. _(Se restar salto, calibrar o snap — PA-01-01.)_
- **Lifecycle/Fika:** sair da arma montada / fim de raid sem estado preso; só jogador local.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Build via `/code-mod` — compila 0 erros. Code review 01 (revisor independente) sem achados. Status 🟡 (aguarda validação in-game). |
