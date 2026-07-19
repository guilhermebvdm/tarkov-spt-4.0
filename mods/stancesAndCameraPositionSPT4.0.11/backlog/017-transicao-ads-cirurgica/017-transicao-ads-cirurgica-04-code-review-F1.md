# 017 — Code Review · F1 (waypoint Stance 0 + gate de aim-speed)

**Mod:** stancesAndCameraPositionSPT4.0.11 · **Sandbox:** `modded/` (v2.7.0)
**Escopo:** `AdsWaypoint.cs` (novo) + integrações em `ApplyComplexRotationPatch`, `ObservedStanceAnimator`,
`StanceManager`, `Plugin`.
**Data:** 2026-07-18 · **Método:** 1 lente adversarial de runtime, com decompile do EFT para validar premissas.

## Resumo

> 🔴 0 · 🟡 2 · 🟢 3 — os 2 🟡 e o 🟢 relevante **aplicados**; 2 🟢 anotados.

**O review derrubou uma premissa minha:** eu assumi que trocar de arma cria um novo `ProceduralWeaponAnimation`.
**Falso** (confirmado por decompile): o PWA é **único por jogador e sobrevive à troca de arma**. Isso tornava o
guard `_gatedPwa == __instance` **inerte** — e abria um vazamento de aim-speed errado. Corrigido guardando a
identidade do **FirearmController** (que muda na troca de arma), não do PWA.

## Achados

| ID | Sev | Título | Status |
|---|---|---|---|
| CR-1 | 🟡 | Guard `_gatedPwa == __instance` inerte → troca de arma no gate herda a velocidade de mira da arma anterior | ✅ Corrigido |
| CR-2 | 🟡 | Restore ficava abaixo dos 5 early-returns → trocar p/ não-arma (granada/med) orfanizava o gate | ✅ Corrigido |
| CR-3 | 🟢 | Sem null-guard de `_aimingSpeedField` no hot-path (NRE/frame se o EFT renomear o campo) | ✅ Corrigido |
| CR-4 | 🟢 | Timer do waypoint observado pode decrementar >1×/frame se `ShiftWeaponRoot` rodar múltiplo | 📝 Anotado (só pose; baixa confiança) |
| CR-5 | 🟢 | Régua ancora no alvo forçado-zero durante o waypoint (rótulo de rota confuso com ADS≠0) | 📝 Anotado (cosmético; com ADS=0 não ocorre) |

**Correção CR-1 + CR-2 (combinada):** identidade guardada = `_gatedFc` (FirearmController). Novo método
`ReleaseGate(bool restore)`: `restore:false` na troca de arma (o equip da nova já recomputou o `_aimingSpeed`
dela — restaurar o valor salvo da antiga seria clobber). **Release CEDO**, antes dos early-returns:
`if (_gated && !ReferenceEquals(firearmController, _gatedFc)) ReleaseGate(false);` — pega tanto a troca de arma
quanto o "virou não-arma" (`firearmController null`). Expiração/soltar ADS com a mesma arma → `ReleaseGate(true)`.

## Verificado sem problema (cobertura da lente)

- Sem 🔴: nenhum caminho de freeze permanente do `_aimingSpeed` (o `×0.001` é sempre sobre `_savedAimingSpeed`,
  nunca `current × 0.001` → sem runaway; o re-equip recomputa; `ResetWaypoint` no raid end restaura).
- `×0.001` e não `0` — evita o `SwayFalloff / _aimingSpeed` (div-by-zero, confirmado no decompile do PWA).
- Kick pausado durante o waypoint (`_waitingForAdsKick && !waypointActive`) — o `_adsKickTimer` congela e retoma.
- `_wasAiming` do waypoint e o do kick são independentes e consistentes (ambos atrás dos mesmos early-returns).
- Observado (Fika): o `AdsWaypoint` do observado **nunca toca `_aimingSpeed`** (só a pose) — instância
  por-componente, morre com o peer, sem estado estático, sem leak.
- `ADS Waypoint Time = 0` → waypoint de fato desligado. `_ResetOnADS=false` ou fora de stance → não arma.
- Prone: passamos `isInStance && !player.IsInPronePose` ao Update.

## Notas para o gate humano (usuário)

- Calibrar `ADS Waypoint Time (ms)` (default 120) com `Debug Transition Metrics` on — curto demais ainda dá loop,
  longo demais fica lento. **Testar em especial:** trocar de arma no meio do ADS-in, granada/med durante o mirar,
  scope de alto zoom, e a paridade 1ª/3ª pessoa no Fika.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Code review F1 (adversarial + decompile). 2 🟡 + 1 🟢 corrigidos; premissa "troca de arma = novo PWA" derrubada. Build 0/0. |
