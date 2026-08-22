---
title: Relatório Técnico — Análise Arquitetural do TarkovIRL
date: 2026-08-18
status: 🟢 Vivo
authors: Antigravity + Gemini + Guilherme
---

# Relatório Técnico — TarkovIRL-SPT4.0-beta
> **Solicitado por:** Gemini (arquiteto de projeto)
> **Analisado por:** Antigravity (leitura direta dos fontes em `mods/TarkovIRL-SPT4.0-beta/`)

---

## 1 · Interceptação de Input — Onde e como o delta do mouse é capturado?

**Ponto de captura:** [`Patch_PlayerRotate.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_PlayerRotate.cs)

```csharp
// Patch_PlayerRotate.cs — PatchPrefix em Player.Rotate()
[PatchPrefix]
private static bool Prefix(Player __instance, ref Vector2 deltaRotation, bool ignoreClamp)
{
    if (__instance == null || !__instance.IsYourPlayer) return true;
    FreeAimController.ApplyInput(ref deltaRotation); // ← intercepção aqui
    return true; // continua execução nativa
}
```

**Resposta direta:**
- O patch é um **Prefix** em `Player.Rotate()` — método nativo que a engine do Tarkov chama a cada frame para aplicar o giro do mouse ao corpo/cabeça do player.
- O delta é interceptado **antes** de a engine processar qualquer rotação de cabeça. O `ref deltaRotation` permite modificar o valor antes de passá-lo adiante.
- O `return true` preserva a execução do método original; o Prefix apenas filtra/modifica o valor de entrada.

**Cadeia de medição do delta (para cálculo de sway/deadzone):**

O delta bruto de rotação é medido secundariamente em [`PlayerMotionController`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/PlayerMotionController.cs) via snapshot de `player.Rotation` em cada `LerpCamera` (veja ponto 2).

```csharp
// PlayerMotionController.cs — UpdateRotationEngine()
float num2 = Mathf.DeltaAngle(_playerRotLastFrame.x, newRot.x); // delta horizontal
float num3 = Mathf.DeltaAngle(_playerRotLastFrame.y, newRot.y); // delta vertical
// Exposto como: HorizontalRotationDelta, VerticalRotationDelta, RawHorizontalSpeed
```

---

## 2 · Pontos de Injeção — Quais métodos do jogo são patcheados?

| Patch | Tipo | Alvo no jogo | Propósito |
|---|---|---|---|
| [`Patch_PlayerRotate`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_PlayerRotate.cs) | **Prefix** | `Player.Rotate()` | Intercepta delta do mouse → Free Aim |
| [`Patch_SetHeadRotation`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_SetHeadRotation.cs) | **Prefix** (retorna `false`) | `ProceduralWeaponAnimation.SetHeadRotation()` | Substitui rotação da cabeça com deadzone aplicada + amplifica Y |
| [`Patch_CalculateCameraPosition_HandLayers`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_CalculateCameraPosition_HandLayers.cs) | **Postfix** | `ProceduralWeaponAnimation.CalculateCameraPosition()` | Ponto central: empilha todos os efeitos visuais no `HandsContainer.WeaponRoot` |
| [`Patch_LerpCamera_ForceUpdateSway`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_LerpCamera_ForceUpdateSway.cs) | **Postfix** | `ProceduralWeaponAnimation.LerpCamera()` | Atualiza `PlayerMotionController` + força `UpdateSwayFactors()` |
| [`Patch_UpdateSwayFactors`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_UpdateSwayFactors.cs) | **Postfix** | `ProceduralWeaponAnimation.UpdateSwayFactors()` | Sobrescreve `MotionReact.SwayFactors` com sway customizado |
| [`Patch_Look`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_Look.cs) | **Postfix** | `Player.Look()` | Adiciona inclinação de cabeça orgânica (lean counter-rotate, parallax ADS head tilt) |
| [`Patch_OnShot`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_OnShot.cs) | **Postfix** | `Player.OnMakingShot()` | Inicia efeito de parallax ADS a cada disparo |

**O mod não altera a rotação global do player** — trabalha exclusivamente no espaço local do `WeaponRoot` (HandsContainer) e na rotação da cabeça (`HeadRotation`). A câmera dos olhos reage via `SetHeadRotation` no PWA.

---

## 3 · Deadzone e Inércia — Como funciona?

### 3.1 Deadzone (NewDeadzoneController)

[`NewDeadzoneController.cs`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/NewDeadzoneController.cs) implementa uma **deadzone de rotação de cabeça** — a arma "atrasa" levemente para acompanhar a virada da câmera.

```csharp
// Acumula o delta horizontal e suaviza com dt
_rotDeltaHistory += horizontalDelta * 100f;
_rotDeltaHistory -= _rotDeltaSmoothed;
_rotDeltaSmoothed = _rotDeltaHistory * fdt * 9f;

// Aplica multiplicadores de peso/ergo/postura
float weight = WeightCurve.Evaluate(customWeight) * (1 - ErgoCurve.Evaluate(customErgo));
float deadzoneMulti = WeaponDeadzoneMulti * weight;
// Fator adicional por stance: ADS, HighReady, LowReady, etc.

// Smooth final com Lerp
_rotDeltaSmoothedInDeltaTime = Mathf.Lerp(_rotDeltaSmoothedInDeltaTime,
    _rotDeltaSmoothed * deadzoneMulti, fdt * DeadzoneHeadFollowSpeedMulti);
```

O resultado é injetado na rotação da cabeça em [`Patch_SetHeadRotation`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/Patch_SetHeadRotation.cs):
```csharp
// Prefix em PWA.SetHeadRotation() — retorna false (bloqueia original)
if (PrimeMover.IsWeaponDeadzone.Value)
    headRotInitial = NewDeadzoneController.GetHeadRotWithDeadzone(headRotInitial);
headRotInitial.y *= 1.5f; // amplifica eixo Y
player.HeadRotation = headRotInitial;
```

**Observação crítica:** O `return false` no Prefix **substitui completamente** o método nativo do PWA. O mod escreve diretamente em `player.HeadRotation` e no field privado `_headRotationVec` via reflection.

### 3.2 Free Aim (FreeAimController)

[`FreeAimController.cs`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/FreeAimController.cs) implementa a **zona morta da arma** (weapon lag / free aim).

**Lógica em `ApplyInput(ref Vector2 deltaRotation)`:**

```csharp
// 1. Acumula o delta no Offset (posição da arma relativa ao centro)
Vector2 candidate = Offset + (deltaRotation * sensitivity * attenFactor);

// 2. Clamp dentro dos bounds configuráveis (XY separados)
Vector2 clamped = new Vector2(
    Mathf.Clamp(candidate.x, -_currentBoundsX, _currentBoundsX),
    Mathf.Clamp(candidate.y, -_currentBoundsY, _currentBoundsY)
);

// 3. A diferença clamped-Offset é o quanto o mouse "consumiu" a deadzone
Vector2 consumed = clamped - Offset;
Offset = clamped;

// 4. Subtrai do delta original — o que restou move a câmera normalmente
deltaRotation = deltaRotation - consumed;
```

**Inércia (autocenter):** Quando `EnableCameraAutoCenter = true`, um `_currentAutoCenterSpeed` lerpa o `Offset` de volta ao zero enquanto simultaneamente devolve o delta para a câmera:
```csharp
Vector2 pullback = Offset * Time.deltaTime * autoCenterSpeed;
deltaRotation += pullback;
Offset -= pullback;
```

**Fast-turn attenuation:** Ao virar rápido (`RawHorizontalSpeed > FastTurnThreshold`), `_attenFactorLerp` cai, reduzindo quanto do mouse entra na deadzone → a câmera acompanha mais rapidamente.

**Aplicação ao WeaponRoot** (em `GetOffsets`):
```csharp
rotOffset = Quaternion.Euler(Offset.y, 0f, Offset.x);
// Aplicado via Patch_CalculateCameraPosition_HandLayers no weaponRoot.localRotation
```

---

## 4 · Tratamento de ADS

O código distingue `isAiming` em **três camadas**:

### 4.1 FreeAimController
```csharp
bool isAiming = PlayerMotionController.IsAiming;
// Bounds menores no ADS:
float boundsX = isAiming ? FreeAimBoundsXADS : FreeAimBoundsX;
// Retorno ao centro mais rápido:
float returnSpeed = isAiming ? FreeAimReturnSpeedADS : FreeAimReturnSpeed;
// Feature pode ser desativada independentemente:
if (isAiming && !EnableFreeAimADS)
    Offset = Lerp(Offset, zero, dt * FreeAimReturnSpeedADS);
```

### 4.2 NewSwayController
```csharp
if (isSprinting | isAiming)
{
    // ZERA todos os accumulators de sway imediatamente ao entrar em ADS
    _lerpPosHorizontal = _lerpPosVertical = _lerpRot = 0f;
    // ...
}
// No GetNewSwayPosition/Rotation:
float adsMulti = isAiming ? 8f : 1f; // Lerp 8x mais rápido no ADS → centraliza a arma
```

### 4.3 NewDeadzoneController
```csharp
if (PlayerMotionController.IsAiming)
    num3 *= PrimeMover.DeadzoneInADS.Value; // Multiplicador separado para ADS
```

**Resumo ADS:** A arma não tem uma "centralização rápida" explícita por transição — o sistema usa `_wasAimingLastFrame` para **resetar todos os accumulators de sway** ao detectar mudança de estado. O free aim tem bounds e velocidades de retorno configuráveis independentemente para ADS.

---

## 5 · Câmera e Reações Orgânicas

### 5.1 Onde vivem os efeitos

Todos os efeitos são **empilhados** em `Patch_CalculateCameraPosition_HandLayers.PatchPostfix()`:

```csharp
// Ordem de composição no WeaponRoot (postfix em CalculateCameraPosition):
Vector3 totalPos = Vector3.zero;
Quaternion totalRot = Quaternion.identity;

totalPos += HandBreathController.GetModifiedHandPosForBreath(player);   // respiração
totalPos += HandPoseController.GetModifiedHandPosWithPose(player);       // postura
totalPos += HandPoseController.GetModifiedHandPosWithPoseChange(player); // mudança postura
totalRot *= HandPoseController.GetModifiedHandRotWithPoseChange();
totalPos += HandShakeController.GetHandsShakePosition(player);           // tremor de mãos
totalPos += HandMovWithRotController.GetModifiedHandPosZMovement(player); // movimentação Z
totalPos += FootstepController.GetModifiedHandPosFootstep              // passadas
         + FootstepController.GetSideToSidePosition();
totalRot *= FootstepController.GetSideToSideRotation();
// Parallax (modifica localPosition/Rotation direto)
totalPos += NewSwayController.GetNewSwayPosition();                      // sway procedural
totalRot *= NewSwayController.GetNewSwayRotation();
// DirectionalSway
// WeaponSelection transitions
// FreeAim offset

// Aplica com fade de shoulder:
weaponRoot.localPosition += totalPos * _shoulderFadeMultiplier;
weaponRoot.localRotation *= totalRot^_shoulderFadeMultiplier; // via Slerp
```

### 5.2 Walk Bob (FootstepController)

[`FootstepController.cs`](file:///d:/Projetos/GITHUB TARKOV/tarkov-spt-4.0/mods/TarkovIRL-SPT4.0-beta/FootstepController.cs) — reage ao evento de som de passo patcheado em `Patch_PlayStepSound.cs`. A posição lateral (side-to-side) alterna a cada passo.

### 5.3 Recoil da câmera

Não há um shake separado de câmera implementado no mod — o recuo visual vem do sistema nativo do PWA (`MotionReact`), que o mod modifica indiretamente em `Patch_UpdateSwayFactors` sobrescrevendo `SwayFactors`.

O `ParallaxAdsController` adiciona uma reação de parallax por disparo apenas em ADS com cheek weld.

### 5.4 Conflitos com animações procedurais nativas

- **`SetHeadRotation`** é completamente substituído (return false) → elimina o processamento nativo de head bobbing do PWA.
- **`UpdateSwayFactors`** é sobrescrito via Postfix → `MotionReact.SwayFactors` é substituído, anulando o sway nativo do Tarkov.
- `CalculateCameraPosition` tem Postfix aditivo → não bloqueia o nativo, mas os offsets do mod se somam sobre o resultado nativo. Isso pode causar **double-sway** se o sway nativo não for neutralizado — mitigado pelo `Patch_UpdateSwayFactors` que zera/substitui `SwayFactors`.

---

## 6 · Free Aim e Sway — Resumo Conjunto

| Aspecto | Free Aim | Sway |
|---|---|---|
| **Classe** | `FreeAimController` | `NewSwayController` |
| **Entrada** | `deltaRotation` (mouse delta) via `Patch_PlayerRotate` (Prefix) | `HorizontalRotationDelta` / `VerticalRotationDelta` medidos em `PlayerMotionController` |
| **Espaço** | Modifica o delta antes de a engine girar a câmera | Opera no espaço local do `WeaponRoot` |
| **Suavização** | `Vector2.Lerp` no Offset; bounds por `Mathf.Clamp` | Cadeia de `Mathf.Lerp` com dois buffers (direto + lagging) |
| **Inércia lagging** | Autocenter configurable via `CameraAutoCenterSpeed` | Ring buffer de 30 frames (`_laggingSwayPoses/Rots`) — offset histórico que "atrasa" o sway |
| **Fast-turn** | `_attenFactorLerp` reduz absorção do mouse → câmera acompanha | Mesmo `attenFactor` espelhado — ambos recebem o mesmo sinal |
| **ADS** | Bounds menores + retorno mais rápido + feature separada | Accumulators zerados + Lerp 8x mais rápido |
| **Ponto de aplicação** | `GetOffsets()` → `rotOffset = Quaternion.Euler(y, 0, x)` no WeaponRoot | `GetNewSwayPosition()` + `GetNewSwayRotation()` → localPosition + localRotation no WeaponRoot |

### Trecho que precisa atenção para reescrita (deadzone vs. free aim)

O `Patch_SetHeadRotation` usa `return false` (bloqueia nativo) mas o `Patch_CalculateCameraPosition_HandLayers` usa Postfix (aditivo). Isso significa:

1. A **câmera dos olhos** (head rotation) é completamente controlada pelo mod.
2. A **arma** recebe offsets **em cima** do que o Tarkov calculou — se o Tarkov ainda aplica algum sway residual via `CalculateCameraPosition` antes do Postfix, isso se soma.

**Para reescrita do efeito Bodycam:** O ponto crítico é que o `FreeAimController.Offset` é aplicado como `Quaternion.Euler(y, 0, x)` — sem translação. Para um efeito mais orgânico (como Bodycam), faz sentido adicionar um componente de **translação lateral** ao `posOffset` proporcional ao `Offset.x`, simulando a câmera se deslocando fisicamente junto com a arma.
