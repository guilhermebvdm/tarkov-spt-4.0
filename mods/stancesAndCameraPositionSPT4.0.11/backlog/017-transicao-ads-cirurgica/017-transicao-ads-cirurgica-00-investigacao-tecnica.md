# 017 — Investigação técnica (pré-spec)

> **Data:** 2026-07-17<br>
> **Status:** ✅ Aprovado (fatos confirmados via `ilspycmd` na DLL real)<br>
> **Responsáveis:** Guilherme<br>

---

Fundamenta a spec do item 017. Dois sub-agents read-only investigaram o nosso pipeline (Problema A) e o
**Assembly real do EFT** (Problema B — via `ilspycmd` em `D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll`,
não o decompilado, que tem namespaces vazios — ver `reference_eft_decompile_incomplete`).

## Problema A — waypoint por Stance 0 antes do ADS

**Como o alvo troca hoje:** ao mirar (`_ResetOnADS=true`, default), `GetTargetRotation/Position(isAiming=true)`
deixam de retornar `_cachedStance{1,2,3}` e passam a retornar `_cachedADSRotation/Position`
([`StanceManager.cs:797,824`](../../modded/StanceManager.cs#L797)). **O Vector3 alvo salta em UM frame** da pose
de stance para a de ADS; a mola sub-amortecida (ζ≈0,49) persegue esse salto com velocidade acumulada → **overshoot**.
Consumo da mola: [`ApplyComplexRotationPatch.cs:275-276`](../../modded/Patches/ApplyComplexRotationPatch.cs#L275).

**Ponto de plugue (confirmado):** um helper por-corpo `Stance0AdsWaypoint` (espelho do `TransitionSpeedTracker`),
que reescreve o alvo entre o cálculo (`:264-265`) e o consumo (`:275-276`): retorna `Vector3.zero` (Stance 0
neutro) enquanto um timer curto roda, depois o alvo real de ADS. No handoff, opcionalmente zerar `_rotVelocity`
para o trecho final "partir do repouso".

- **Armar:** na borda "Entering ADS" **que já existe** em [`:212-219`](../../modded/Patches/ApplyComplexRotationPatch.cs#L212)
  (mesma do ADS-kick, reusa `_wasAiming`).
- **Precedente correto = o timer do ADS-kick** ([`:216-252`](../../modded/Patches/ApplyComplexRotationPatch.cs#L216)):
  perturbação de mola por timer, **sem tocar `CurrentStance`**. ⚠️ **NÃO** imitar o snap-on-fire nem o item 013 —
  ambos fazem `SetStance(Default)` (`StanceManager.cs:172,429`), que dispara `OnStanceChanged` → pacote Fika +
  stamina + speed-limit. O waypoint NÃO pode mexer em `CurrentStance`.

**Gates obrigatórios:**
- Só quando `_ResetOnADS == true`. Com `false`, a pose de stance **continua** em ADS (sem salto de alvo, sem
  overshoot) — o waypoint não faz sentido e introduziria movimento espúrio.
- Pular em **prone** (o kick também é pulado ali).
- **NÃO forjar** bordas de `isAiming`/`stance` para manipular o `TransitionSpeedTracker` — isso reenviaria Fika,
  re-dispararia o kick e re-aplicaria stamina.

**Coordenação com o kick:** o timer do waypoint (T) e o `_ADSKickDelay` (0,15s) coexistem. Se o waypoint zerar
velocidade no handoff e o kick já tiver injetado em `_posVelocity.y`, um apaga o outro — decidir a ordem (ideal:
zerar só a componente rotacional, ou escolher T e o delay do kick juntos).

**Fika (crítico):** o observado reusa `GetTargetRotation/Position(stance, isAiming)` e a mesma mola
([`ObservedStanceAnimator.cs:42-43,50-51`](../../modded/Networking/ObservedStanceAnimator.cs#L42)). Se o waypoint
for só local, **1ª e 3ª pessoa divergem** (você passeia por Stance 0; os peers te veem saltar direto com
overshoot). O helper precisa ser **compartilhado** (instância estática local + uma por observado, como o
`TransitionSpeedTracker`), armado na borda de `_isAiming` em `SetStance` do observado.

**Duplicação a cobrir:** `ApplySimpleRotationPatch` tem mola própria idêntica (`:177-178`) — o consumo precisa ser
replicado lá também (ou os dois consolidados) se o EFT rotear para o Simple.

## Problema B — atenuar o offset longitudinal por comprimento de arma

**Onde estão os IK markers (a suposição da spec estava errada):** NÃO no PWA/HandsContainer. Estão em `EFT.Player`,
campos privados: `_markers[2]` (`[0]`=mão esquerda), `_limbs[2]` (`LimbIK` do FinalIK, `[0]`=braço esquerdo),
`_gripReferences[2]`. Acesso só por reflection em `Player`. `LeftHandInteractionTarget` (`GripPose`) é público.

**O sinal a usar — `FirearmController.WeaponLn`** ✅ (confirmado via ilspycmd):
- `protected float WeaponLn;` — distância física base-do-cano → boca, computada em `FirearmController.method_10`.
- É **o número que o próprio EFT usa** para a colisão do cano (`WeaponOverlapping`) e o param `WEAPON_SIZE_MODIFIER`.
  Faixa ~0,5 (pistola) a ~1,4 (rifle longo).
- Acesso: `AccessTools.Field(typeof(FirearmController), "WeaponLn")` **cacheado** (recalculado só na troca de arma).
- ⚠️ **LIÇÃO DO FONTAINE (não repetir o erro dele):** ele tentou **reescrever** o `WeaponLn` por stance e
  **abandonou** com o aviso no código: *"Do NOT do this, weapon length determines origin of bullet."* → **só LER;
  escalar o nosso offset, nunca o `WeaponLn`.** O Fontaine já fazia "escalar offset por `WeaponLn`" no
  `CollisionOverride` (prova de conceito) e capturava o valor no `WeaponLengthPatch`.

**Sinal alternativo (refino opcional) — folga do braço esquerdo** ✅: `_limbs[0].solver.bone1/2/3` (públicos no
`IKSolverLimb`) dão para calcular `slack = armLength − reachDemand`; hiperestende quando `slack ≤ 0`. É o rig de
**1ª pessoa** (o braço que você vê quebrar). Fisicamente mais correto, porém mais complexo e com timing pré/pós-IK
a validar. **FinalIK não tem clamp nativo de "não alcança"** — estica reto = a origem do bug; a atenuação é 100%
nossa.

**Viabilidade:** ✅ viável. Começar por `WeaponLn` (1 `GetValue` cacheado, determinístico, **funciona igual em
Fika** por ser per-weapon local): escalar a componente longitudinal (Y local) do offset da transição por um fator
que cai com o comprimento — `push *= Mathf.Lerp(1f, kMin, InverseLerp(lenCurta, lenLonga, WeaponLn))`. Refino pela
folga real do braço só depois, se o `WeaponLn` sozinho não resolver.

**Relação com P-11.2** (braço G36 em High Ready ao mirar): provável **mesma causa** (offset longitudinal × alcance
do braço em arma longa). A tech-spec deve verificar se um único fix de atenuação cobre os dois.

## Refs confirmadas via ilspycmd (DLL real)

`EFT.Player._markers[2]` · `Player._limbs[2]` (LimbIK) · `Player._gripReferences[2]` ·
`Player.LeftHandInteractionTarget` (GripPose, público) · `PWA.HandsContainer` **é** `EFT.Animations.PlayerSpring`
(não tem markers) · `FirearmController.WeaponLn` (protected float) · `FirearmController.method_10` (calcula) ·
`RootMotion.FinalIK.LimbIK.solver` → `IKSolverLimb.bone1/2/3` (públicos).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-17 | Guilherme | Criação — 2 investigações read-only (pipeline + Assembly real via ilspycmd). |
