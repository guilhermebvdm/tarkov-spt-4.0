# 014 — Fix 01 · Sync aplica no ponto certo (ProcessEffectors, não ApplyComplexRotation)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Item raiz:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Asbuild:** [014-sync-stances-fika-05-asbuild.md](014-sync-stances-fika-05-asbuild.md)
**Criado:** 2026-06-22
**Disparado por:** validação in-game — após a entrega do 014, **nada** sincronizava (nem o braço que antes aparecia com o Spine3).

## Contexto

A entrega original do 014 trocou a aplicação remota de `PlayerBones.Spine3` (LateUpdate) para um Postfix em `ProceduralWeaponAnimation.ApplyComplexRotation`, escrevendo no `WeaponRootAnim`. In-game: a arma **não** acompanhou **e** o sync do braço (que antes aparecia, mesmo desalinhado) **sumiu**.

## Causa raiz (investigação por 2 sub-agents + leitura do Fika)

Cadeia real de render do jogador observado ([ObservedPlayer.cs:1851-1876](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1851)):
1. `ProcessEffectors(...)` roda — **internamente** chama `ApplyComplexRotation` (via `GClass912.ApplyTransformations`, que segue com `ApplyTacticalReload`/`AvoidObstacles`).
2. copia `WeaponRootAnim.localPosition/localRotation` → `PlayerBones.Offset/DeltaRotation`.
3. `ShiftWeaponRoot(ThirdPerson)` usa o `DeltaRotation` para posicionar a arma de 3ª pessoa (o braço segue por IK).

**O offset escrito no `ApplyComplexRotation` era sobrescrito** pelas etapas posteriores do `ProcessEffectors` (passo 1) antes da cópia (passo 2) — então não chegava ao `DeltaRotation` nem à render. O `Spine3` antigo "aparecia" porque era um `LateUpdate` independente (rodava após tudo), mas girava só o tronco e o `ShiftWeaponRoot` re-corrigia a arma → o desalinhamento relatado.

## Solução

Aplicar o offset num **Postfix de `ProceduralWeaponAnimation.ProcessEffectors`** (novo `ObservedStanceProcessPatch`), que roda **depois de todo** o processamento da PWA (offset não é sobrescrito) e **antes** da cópia para `PlayerBones` (passo 2). Assim o offset entra no `DeltaRotation` → o `ShiftWeaponRoot` o aplica na arma de 3ª pessoa; o braço acompanha por IK. É o mesmo ponto que o item 011 (`PassiveSwayPatch`) usa e que comprovadamente roda para observados/peers.

- Offset **aditivo** sobre `WeaponRootAnim.localPosition/localRotation` (coexiste com lean/ombro/mira, que já estão na pose).
- O jogador **local** volta a ser tratado só pelo `ApplyComplexRotationPatch` (1ª pessoa) — gate `IsYourPlayer` restaurado.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded-beta/Patches/ObservedStanceProcessPatch.cs` | **CRIADO** — Postfix em `ProcessEffectors`; resolve o player da PWA; só observados → `ObservedStanceAnimator.ApplyToObserved`. |
| `modded-beta/Networking/ObservedStanceAnimator.cs` | `ApplyToObserved(pwa)` aplica o offset **aditivo** em `WeaponRootAnim.localPosition/localRotation`. Log único `[ObservedStance] fix-01 ATIVO` para confirmar execução. |
| `modded-beta/Patches/ApplyComplexRotationPatch.cs` | Revertido o desvio observed; gate `!IsYourPlayer return` restaurado (local-only). `SpringLerp*` seguem públicos. |
| `modded-beta/Plugin.cs` | `SafeEnable("ObservedStanceProcessPatch")`. |

## Checklist de validação (2 clientes Fika)

- [x] Compila via `/compile-mod` sem erros
- [ ] **Log:** aparece `[ObservedStance] fix-01 ATIVO` no `LogOutput.log` do cliente que observa (confirma que o patch roda no observado)
- [ ] A **arma do outro player acompanha** a stance (não só o braço); pose remota ≈ local
- [ ] **Stance + lean** e **stance + troca de ombro** coexistem; vanilla intacto
- [ ] Prone/ADS/troca rápida/vários remotos/spawn-despawn-morte sem erro

> Se o log aparecer mas a arma **não** mover, o offset não chega à render 3ª pessoa por outro motivo (ex.: `ThirdPersonWeaponRootAuthority` baixo, ObservedPlayer.cs:1866) → próximo alvo seria `PlayerBones.Offset/DeltaRotation` direto ou `Weapon_Root_Third`.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Fix criado — aplicação movida para Postfix de `ProcessEffectors` (ponto que renderiza no observado). Compila 0 erros; aguarda validação in-game (2 clientes). |
