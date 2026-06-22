# 013 — Refinamentos de transição de stance · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**As-built:** [013-refino-transicao-stance-05-asbuild.md](013-refino-transicao-stance-05-asbuild.md)
**Data:** 2026-06-21

> Revisão por **revisor independente** (sub-agent de contexto limpo — anti-viés). 12 pontos verificados.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 — **nenhum achado acionável**.

O revisor independente confirmou os 3 ajustes corretos, defensivos e sem regressões do item 012. A única ressalva é a herdada da review técnica (timing de 1 frame, imperceptível) + a validação visual do ajuste 3, ambas para o teste in-game.

## Verificações confirmadas (✅)

- **Ajuste 1 — stationary → Mount Active:** gate do `Tick` e `Resolve` reconhecem stationary; `MovementContext` null tratado em duas camadas; detecção **contínua** (ao sair, volta ao normal sem flag preso). `StaminaController.cs:56,98`. Sem regressão do item 012 (`ActiveStance0` reusado; stationary é pré-check de maior prioridade).
- **Ajuste 2 — força Stance 0:** `isStationary` na condição `if (isNativeMounting || isInProne || isStationary)`; no-op se já em Default; `return` não bloqueia nada indevido (sprint é bloqueado nativamente em stationary). `StanceManager.cs:176-186`.
- **Ajuste 3 — SnapToNeutral no sprint:** zera os 5 campos do spring corretamente; gate `!_isTacSprintActive && !CanDoTacSprint` **preserva o TacSprint**; restore `SetStance(_preSprintStance)` re-anima suave ao parar (não dá outro snap); só MainPlayer (AP-02). `StanceManager.cs:194-203` + `ApplyComplexRotationPatch.cs:SnapToNeutral`.
- **Corner cases da spec** (entrar já em Stance 0 = no-op; sprint tap; sair de stationary) confirmados sem inconsistência.

## Ressalva herdada (não-bloqueante)

- **PA-01-02** (review técnica): timing `Plugin.Update` × `ApplyComplexRotationPatch` Postfix = ≤1 frame — imperceptível, aceito.
- **PA-01-01** (review técnica): o ajuste 3 é visual — confirmar in-game que o snap remove o flash sem salto; calibrar se necessário.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Code review 01 (revisor independente, 12 pontos) — 0 achados acionáveis; liberado para validação in-game |
