# 017 — Spec técnica · F1 (waypoint Stance 0 + gate de aim-speed)

**Mod:** stancesAndCameraPositionSPT4.0.11 · **Sandbox:** `modded/` canônico
**Spec funcional:** [01-spec.md](017-transicao-ads-cirurgica-01-spec.md) (F1)
**Criado:** 2026-07-18 · **Design confirmado com o usuário.**

## Mecanismo (2 componentes, mesmo timer `X ms`)

Ao apertar mirar estando em Stance 1/2/3, por `X ms` (config F12):
1. **Waypoint de alvo** — o alvo da mola vira `Vector3.zero` (Stance 0). A arma assenta no neutro. (local + observado)
2. **Gate de aim-speed** — `_aimingSpeed` do PWA × 0.001 → a câmera/mira **não sobe**. (só local)

Passado `X ms`: gate libera (restaura `_aimingSpeed`), alvo volta ao de ADS, a arma sobe limpa. Ao sair do ADS:
`CurrentStance` **nunca mudou** → a pose volta sozinha.

## Ponto de plugue (confirmado via ilspycmd)

- **Gate:** escrever o **campo privado** `ProceduralWeaponAnimation._aimingSpeed` no **nosso postfix**
  (`ApplyComplexRotationPatch`, que já roda 1×/frame, tem o `__instance` PWA e a borda `_isAiming`). `LerpCamera`
  (câmera) roda **depois** do postfix → gate sem leak na câmera; `UpdateAimWeight`/scope rodam **antes** → no
  máx **1 frame** de leak no arranque (imperceptível num hold de X ms).
- ⚠️ **`_aimingSpeed` PERSISTE** (só recomputado em eventos de arma, não por frame) → **salvar na borda de
  entrada e RESTAURAR ao expirar/sair/interromper**, senão o aim quebra permanentemente.
- ⚠️ **Fator `× 0.001`, não `0`** — o EFT faz `SwayFalloff / _aimingSpeed` em `UpdateWeaponVariables`; zero =
  divisão por zero se um evento de arma disparar durante o gate.
- ⚠️ **Guardar o PWA gateado** — se a arma trocar durante o gate, NÃO restaurar o valor antigo no PWA novo (o
  equip já recomputou o `_aimingSpeed` da nova arma); só descartar o estado do gate.

## Arquivos

| Arquivo | Ação |
|---|---|
| `modded/AdsWaypoint.cs` | **criar** — helper por corpo: timer + borda de ADS + "alvo deve ir a Stance 0" |
| `modded/Patches/ApplyComplexRotationPatch.cs` | waypoint (local) + gate de aim-speed + pausar kick durante o waypoint + `ResetWaypoint()` |
| `modded/Networking/ObservedStanceAnimator.cs` | waypoint de alvo (SÓ — sem gate; a subida do observado é a animação nativa) |
| `modded/StanceManager.cs` | `ResetWaypoint()` no `ResetState` |
| `modded/Plugin.cs` | `ADS Waypoint` (bool, default true) + `ADS Waypoint Time` (int ms, default 120, 0–400) na seção `Stance Transition & Kick`; bump 2.6.0 → 2.7.0 |

## Decisões

- **Só ativa** com `_ResetOnADS == true`, `isInStance`, `!prone` e a flag `ADS Waypoint` on.
- **Kick de ADS-in:** pausar o `_adsKickTimer` enquanto o waypoint está ativo → o kick dispara ~depois do gate
  (acompanha a subida real), não no meio do waypoint.
- **Fika:** o observado passa pelo waypoint de alvo (paridade de pose); o gate é local (a mira/câmera é do jogador
  local). Pacote de rede inalterado.
- **Reset de raid:** restaura o `_aimingSpeed` se ainda gateado (try/catch — o PWA pode ter morrido) e zera o
  estado.

## Corner cases (a validar no code-review + in-game)

Soltar o ADS antes de X ms (restaura na hora) · trocar de arma durante o gate (não restaura no PWA novo) ·
snap-on-fire durante o waypoint · X ms = 0 (waypoint desligado de fato) · `_ResetOnADS=false` (waypoint off) ·
prone · morte/extração no meio (reset restaura) · scope de alto zoom · trocar de stance mirando.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Tech-spec F1 (g-autodev). Ponto de plugue do gate confirmado via ilspycmd. |
