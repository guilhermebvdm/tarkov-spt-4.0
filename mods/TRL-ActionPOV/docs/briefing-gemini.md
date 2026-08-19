---
title: TRL-ActionPOV — Briefing Técnico Completo
date: 2026-08-18
status: 🟢 Vivo
authors: Antigravity + Gemini + Guilherme
---

# TRL-ActionPOV — Briefing Técnico Completo
> Preparado pelo Antigravity com leitura direta dos fontes.
> Versão base: `TarkovIRL-SPT4.0-beta` → fork `TRL-ActionPOV`
> SPT 4.0.13 · EFT 0.16.9 · BepInEx + Harmony

---

## PARTE 1 — Arquitetura do mod (o que já existe)

### 1.1 Ciclo de vida dos patches (ordem de execução por frame)

```
LateUpdate (PrimeMover.cs)
  ├── PlayerMotionController.UpdateMovementMeasurementsInFDT(dt)  ← mede delta de rotação
  ├── NewDeadzoneController.Update(dt)                            ← suaviza delta para deadzone de cabeça
  ├── NewSwayController.UpdateLerp(dt)                            ← acumula sway procedural
  ├── HeadRotController.UpdateLerp(dt)                            ← inclinação de lean/parallax
  ├── DirectionalSwayController.UpdateDirectionalSwayLerp(dt)     ← sway por WASD
  ├── FootstepController.UpdateStep(dt)                           ← bob de passos
  ├── SwayController.UpdateLerp(dt)                               ← sway nativo modificado
  └── ParallaxController.Update(dt)                               ← parallax de rotação

Patch_PlayerRotate [Prefix em Player.Rotate()]
  └── FreeAimController.ApplyInput(ref deltaRotation)             ← intercepta mouse ANTES da câmera girar

Patch_LerpCamera_ForceUpdateSway [Postfix em PWA.LerpCamera()]
  ├── PlayerMotionController.UpdateMovementInformation(player)    ← snapshot de rotação do player
  └── PWA.UpdateSwayFactors()                                     ← força re-execução dos fatores de sway

Patch_UpdateSwayFactors [Postfix em PWA.UpdateSwayFactors()]
  └── SwayController.GetNewSway() → MotionReact.SwayFactors       ← substitui SwayFactors nativos

Patch_Look [Postfix em Player.Look()]
  └── HeadRotController.GetHeadRotThisFrame() → player.HeadRotation ← inclinação orgânica de cabeça

Patch_SetHeadRotation [Prefix em PWA.SetHeadRotation() — retorna false]
  ├── NewDeadzoneController.GetHeadRotWithDeadzone()              ← aplica lag de cabeça
  ├── player.HeadRotation = finalRot                              ← escreve na câmera dos olhos
  └── _headRotVecField.SetValue(pwa, finalRot)                    ← OBRIGATÓRIO: alinha arma com cabeça

Patch_CalculateCameraPosition_HandLayers [Postfix em PWA.CalculateCameraPosition()]
  ├── HandBreathController                                        ← respiração
  ├── HandPoseController                                          ← posição por agachamento + transição
  ├── HandShakeController                                         ← tremor de braço
  ├── HandMovWithRotController                                    ← movimento Z + arma abaixada
  ├── ParallaxController                                          ← desalinhamento de mira
  ├── FootstepController (side-to-side)                           ← bob lateral de passos
  ├── NewSwayController                                           ← sway procedural final
  ├── DirectionalSwayController                                   ← sway direcional
  ├── WeaponSelectionController                                   ← transições de arma
  ├── FreeAimController.GetOffsets()                              ← offset de free aim (só rotação atualmente)
  └── _shoulderFadeMultiplier Slerp                               ← fade em left-shoulder

Patch_LateUpdate_UpdateWpnStats [Postfix em Player.LateUpdate()]
  └── PlayerMotionController.UpdateMovementInformation()          ← snapshot adicional por frame
```

### 1.2 Classes de controle

| Classe | Propósito |
|---|---|
| `FreeAimController` | Zona morta da arma: acumula Offset baseado no delta do mouse, absorve parte do movimento antes de repassar à câmera |
| `NewDeadzoneController` | Lag de cabeça: a câmera atrasa levemente para acompanhar o giro, simulando o peso da cabeça |
| `NewSwayController` | Sway procedural completo com ring buffer de 30 frames (lagging sway), responde ao delta de rotação |
| `HeadRotController` | Inclinação de cabeça ao fazer lean + head tilt de parallax ADS |
| `PlayerMotionController` | Motor de medição: calcula HorizontalRotationDelta, VerticalRotationDelta, RawHorizontalSpeed, direção WASD, IsAiming, IsSprinting |
| `DirectionalSwayController` | Sway causado por movimentação WASD (não pelo mouse) |
| `FootstepController` | Bob de passos: reage ao evento Patch_PlayStepSound, lateral (side-to-side) + vertical |
| `ParallaxController` | Desalinhamento de mira: a arma gira na mão desacoplando a mira ao rotacionar |
| `ParallaxAdsController` | Reação de parallax por disparo em ADS com cheek weld |
| `HandPoseController` | Posição da arma dependente de agachamento + transição de postura |
| `HandBreathController` | Oscilação por respiração baseada em stamina |
| `HandShakeController` | Tremor adicional quando arm stamina cai |
| `HandMovWithRotController` | Movimento Z (puxar arma ao rotacionar) + arma abaixada sem coronha |
| `WeaponSelectionController` | Animações customizadas de transição (ombro, bandoleira, holster) |
| `EfficiencyController` | Stat sintético (0-10+) que agrega peso, stamina, lesões e sobrepeso |
| `SwayController` | Modificador leve dos SwayFactors nativos do PWA |
| `StanceController` | Detecta postura: None (vanilla), HighReady, LowReady, ShortStock, ActiveAiming |
| `AnimStateController` | Detecta estado do animator: blindfire, left-shoulder, estado atual do corpo/arma |

### 1.3 Variáveis críticas do PlayerMotionController

```csharp
// Pipeline do eixo horizontal:
num2 = Mathf.DeltaAngle(lastRot.x, newRot.x)         // ângulo bruto em graus — TEM SINAL
RawHorizontalSpeed = Mathf.Abs(num2 / dt)             // velocidade real em graus/s — SEM dt embutido
num4 = num2 * dt                                       // delta ponderado por dt (uma vez)
_horizontalRotationHistory += num4
_horizontalRotationValue = _horizontalRotationHistory * dt * RotationAverageDTMulti  // dt² embutido!
HorizontalRotationDelta = _horizontalRotationValue    // CONTÉM dt² — NÃO dividir por deltaTime novamente

// ATENÇÃO: não existe HorizontalRotationSign atualmente.
// RawHorizontalSpeed é sempre positivo (Abs).
// Precisamos adicionar essa propriedade (ver Parte 3, Correção 4).
```

---

## PARTE 2 — Mapa completo de funcionalidades e configurações F12

### Toggles (features on/off)

| Toggle | Padrão | O que faz |
|---|---|---|
| Enable Mod | true | Master switch — desativa tudo |
| Enable weapon deadzone | true | NewDeadzoneController: cabeça atrasa ao girar |
| Enable custom weapon sway | true | NewSwayController + SwayController: sway procedural completo |
| Enable breathing effect | true | HandBreathController: oscilação por stamina |
| Enable stance-dependent weapon position | true | HandPoseController: arma se aproxima ao agachar |
| Enable stance transition effect | true | HandPoseController: dip suave ao mudar postura |
| Enable extra arm stam shake | true | HandShakeController: tremor por arm stamina |
| Enable small visual effects | true | HandMovWithRotController: rotação puxa arma + arma abaixada + granadas + lean |
| Enable footstep effect | true | FootstepController: bob de passos |
| Enable aiming misalignment feature | true | ParallaxController: desalinhamento de mira ao rotacionar |
| Enable directional sway feature | true | DirectionalSwayController: sway por WASD |
| Enable ADS head tilt | true | HeadRotController + ParallaxAdsController: inclinação de cabeça em ADS |
| Enable Enhanced Weapon Transitions | true | WeaponSelectionController: transições customizadas de arma |
| Enable Shot Parallax | false | ParallaxAdsController: torção da arma por disparo em ADS |
| Enable True Free Aim | true | FreeAimController: zona morta hipfire |
| Enable Free Aim (ADS) | false | FreeAimController: zona morta em ADS |
| Enable Camera Auto-Center | false | Câmera gradualmente persegue o offset da arma (hipfire) |
| Enable Camera Auto-Center (ADS) | false | Idem para ADS |
| Enable efficiency indicator | true | HUD: dois pontos cuja distância indica eficiência atual |
| Invert Sway Direction in Vanilla | false | Arma lidera câmera (estilo Bodycam) em vez de atrasar |

### Parâmetros numéricos por seção

**a - Mod Status:** Master Sensitivity Multiplier [0.1-5.0] padrão 1.0; Toggle Mod Key (F10)

**b - Adjust main feature values:**
- Deadzone multiplier [0-5] = 0.3
- Sway multiplier [0-2] = 0.5
- Aiming misalignment multiplier [1-100] = 16.0
- Directional Sway Final Modifier [0-5] = 0.12
- Weapon transition speed multiplier [0.1-5] = 1.3
- Main hand smoothing layer [1-20] = 1.0
- Fast Turn Threshold [0.1-500°/s] = 150.0 — velocidade que ativa atenuação
- Fast Turn Attenuation [0-1] = 0.8 — quanto sway/free aim reduzem
- Efficiency Indicator Position [400-600] = 550

**c - Sway Values (15+ parâmetros):**
- Minimum Weapon Sway = 0.3, Pistol Multiplier = 2.0
- Fixed Weight = 4.0, Fixed Ergo Norm = 0.5
- Return speed = 15.0, Position/Rotation multipliers e clamps
- Lagging sway norm = 0.5, multi = 2.0, clamp = 12.0 (ring buffer 30 frames)
- Weapon drop unstocked (valor + velocidade)
- Weapon aimpoint drop on rotation = 0.3
- Hyper-vertical effect (efeito de gravidade na arma)

**d - Parallax (8 parâmetros):** set size, Position X, % in ADS, Hard Stop, Shot parallax, lerp/smoothing speeds

**e - Efficiency (3 parâmetros):** change rate, overweight coef = 250.0, injury coef = 0.5

**f - Rotation engine (2 parâmetros — NÃO TOCAR):**
- RotationAverageDTMulti = 80.0 (escala do pipeline HorizontalRotationDelta)
- RotationHistoryClamp = 0.1

**g - Misc (12 parâmetros):** Breathing, ArmShake, Footstep, Throw, Side-to-side, LeanCounterRotate
- **Bodycam Delay Speed [1-20] = 7.0** — controla HeadRotController (JÁ é um efeito bodycam!)

**h - Directional Sway (6 parâmetros):** posição lateral/projetada, rotação, lerp speed, % during ADS = 0.25

**j - Deadzone (8 parâmetros):**
- % por postura: ADS=0.2, Vanilla=0.65, LowReady=0.75, HighReady=0.35, ShortStock=0.35, ActiveAim=0.5
- Fixed Weight = 4.0, Fixed Ergo = 0.5, Headfollow Speed Multi = 10.0
- Deadzone weighted for Efficiency (bool)

**z - Free Aim hipfire:** Bounds X=15°, Y=10°, Return Speed=5.0, Sensitivity=0.5, Auto-Center Speed=3.0
**z - Free Aim ADS:** Bounds X=5°, Y=3°, Return Speed=10.0, Sensitivity=0.5, Auto-Center Speed=1.0
**z - Debug Axes (6 sliders):** DebugPosX/Y/Z e DebugRotX/Y/Z — apenas para desenvolvimento

---

## PARTE 3 — Correções técnicas obrigatórias da proposta anterior

### Correção 1 — FreeAimController é static e tem 3 ramos obrigatórios no ApplyInput

```csharp
// A declaração CORRETA da classe:
public static class FreeAimController { ... }

// ApplyInput TEM que manter estes 3 ramos:
public static void ApplyInput(ref Vector2 deltaRotation)
{
    bool isAiming = PlayerMotionController.IsAiming;

    // RAMO 1: Força retorno ao centro, NÃO acumula no TargetOffset
    // Ativado quando: mod off | stance ativa sem ADS | correndo | left-shoulder fade
    if (!PrimeMover.EnableMod.Value
        || !isAiming && StanceController.CurrentStance != EStance.None
        || PlayerMotionController.IsSprinting
        || Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay)
    {
        float returnSpeed = PrimeMover.FreeAimReturnSpeed.Value;
        TargetOffset = Vector2.Lerp(TargetOffset, Vector2.zero, Time.deltaTime * returnSpeed);
        // CurrentOffset segue via UpdateWeaponInertia() chamado no Patch_CalculateCameraPosition
        return;
    }

    // RAMO 2: Feature desativada no F12 para este estado
    if (!isAiming && !PrimeMover.EnableFreeAim.Value
        || isAiming && !PrimeMover.EnableFreeAimADS.Value)
    {
        float returnSpeed = isAiming ? PrimeMover.FreeAimReturnSpeedADS.Value : PrimeMover.FreeAimReturnSpeed.Value;
        TargetOffset = Vector2.Lerp(TargetOffset, Vector2.zero, Time.deltaTime * returnSpeed);
        return;
    }

    // RAMO 3: Modo ativo. Bounds com Lerp gradual (evita teleporte ao mirar):
    float lerpSpeed = isAiming ? 6f : 15f;
    _currentBoundsX = Mathf.Lerp(_currentBoundsX,
        isAiming ? PrimeMover.FreeAimBoundsXADS.Value : PrimeMover.FreeAimBoundsX.Value,
        Time.deltaTime * lerpSpeed);
    _currentBoundsY = Mathf.Lerp(_currentBoundsY,
        isAiming ? PrimeMover.FreeAimBoundsYADS.Value : PrimeMover.FreeAimBoundsY.Value,
        Time.deltaTime * lerpSpeed);

    // Fast-turn attenuation (manter):
    float rawSpeed = PlayerMotionController.RawHorizontalSpeed;
    float attenTarget = rawSpeed > PrimeMover.FastTurnThreshold.Value
        ? Mathf.Clamp01(1f - (rawSpeed - PrimeMover.FastTurnThreshold.Value) * PrimeMover.FastTurnAttenuation.Value * 0.005f)
        : 1f;
    _attenFactorLerp = attenTarget >= _attenFactorLerp
        ? Mathf.Lerp(_attenFactorLerp, attenTarget, Time.deltaTime * 10f)
        : attenTarget;

    // Sensibilidade usando o multiplicador global existente:
    float sensitivity = isAiming ? PrimeMover.FreeAimSensitivityADS.Value : PrimeMover.FreeAimSensitivity.Value;
    sensitivity *= PrimeMover.MasterSensitivityMultiplier.Value;
    if (sensitivity <= 0.001f) sensitivity = 0.001f;

    // Proposta do Gemini com matemática correta:
    Vector2 normalizedDelta = deltaRotation * sensitivity * _attenFactorLerp;
    Vector2 candidate = TargetOffset + normalizedDelta;
    Vector2 clamped = new Vector2(
        Mathf.Clamp(candidate.x, -_currentBoundsX, _currentBoundsX),
        Mathf.Clamp(candidate.y, -_currentBoundsY, _currentBoundsY));
    Vector2 consumedNormalized = clamped - TargetOffset;
    TargetOffset = clamped;
    deltaRotation -= consumedNormalized / sensitivity; // reconversão correta de escala

    // Autocenter (manter com TargetOffset):
    bool autoCenterEnabled = isAiming ? PrimeMover.EnableCameraAutoCenterADS.Value : PrimeMover.EnableCameraAutoCenter.Value;
    if (autoCenterEnabled)
    {
        float centerSpeed = isAiming ? PrimeMover.CameraAutoCenterSpeedADS.Value : PrimeMover.CameraAutoCenterSpeed.Value;
        _currentAutoCenterSpeed = Mathf.Lerp(_currentAutoCenterSpeed, centerSpeed, Time.deltaTime * 6f);
        Vector2 pullback = TargetOffset * Time.deltaTime * _currentAutoCenterSpeed;
        deltaRotation += pullback;
        TargetOffset -= pullback;
    }
}
```

### Correção 2 — Cache do Player em Patch_SetHeadRotation

O Harmony NÃO injeta Player automaticamente. O pattern abaixo DEVE ser mantido integralmente:

```csharp
protected override MethodBase GetTargetMethod()
{
    // Resolve os FieldInfos uma vez no boot via HarmonyLib AccessTools:
    _playerField     = AccessTools.Field(typeof(Player.FirearmController), "_player");
    _fcField         = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
    _headRotVecField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec");
    return typeof(ProceduralWeaponAnimation).GetMethod("SetHeadRotation", BindingFlags.Instance | BindingFlags.Public);
}

[PatchPrefix]
private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
{
    if (__instance == null) return true;

    // Cache lazy — TryGetValue + Add na primeira vez:
    Player player;
    if (!_playerCache.TryGetValue(__instance, out player))
    {
        var fc = (Player.FirearmController)_fcField.GetValue(__instance);
        if (fc != null)
        {
            player = (Player)_playerField.GetValue(fc);
            if (player != null) _playerCache.Add(__instance, player);
        }
    }
    if (player == null) return true;
    if (!player.IsYourPlayer
        || player.MovementContext.CurrentState.Name == EPlayerState.Stationary
        || !PrimeMover.EnableMod.Value
        || Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay)
        return true;

    // ... nova lógica de roll aqui ...

    // OBRIGATÓRIO — escrever em AMBOS:
    player.HeadRotation = finalRotation;
    _headRotVecField.SetValue(__instance, finalRotation); // alinha a arma com a cabeça
    return false;
}
```

### Correção 3 — Calcular sinal horizontal no PlayerMotionController

```csharp
// Em PlayerMotionController.cs, dentro de UpdateRotationEngine():
// (num2 já existe e tem sinal: Mathf.DeltaAngle(lastRot.x, newRot.x))
HorizontalRotationSign = num2 >= 0f ? 1f : -1f;

// Nova propriedade pública:
public static float HorizontalRotationSign { get; private set; } = 1f;

// No Patch_SetHeadRotation, calcular o roll assim:
float signedSpeed = PlayerMotionController.RawHorizontalSpeed * PlayerMotionController.HorizontalRotationSign;
float targetRoll  = Mathf.Clamp(-signedSpeed * rollIntensity, -maxRollAngle, maxRollAngle);
currentHeadRoll   = Mathf.SmoothDamp(currentHeadRoll, targetRoll, ref rollVelocity, rollRecoveryTime);
```

### Correção 4 — Reset no ADS via NewSwayController

```csharp
// NewSwayController.cs — bloco existente em UpdateLerp():
if (isAiming != _wasAimingLastFrame)
{
    _lerpPosHorizontal = 0f;
    // ... outros resets existentes ...
    FreeAimController.Reset(); // ADICIONAR esta linha — zera TargetOffset + CurrentOffset + currentVelocity
}
```

### Correção 5 — UpdateWeaponInertia() e GetOffsets() no Patch_CalculateCameraPosition

```csharp
// No início do PatchPostfix (antes de todos os controladores):
FreeAimController.UpdateWeaponInertia(); // SmoothDamp da inércia, roda mesmo com mouse parado

// Ao aplicar os offsets (substituir a chamada atual):
FreeAimController.GetOffsets(out Vector3 freeAimPos, out Quaternion freeAimRot);
vector3_1 += freeAimPos; // NOVO: translação lateral
quaternion1 *= freeAimRot; // já existia (era só rotação)
```

---

## PARTE 4 — Escopo definitivo (5 arquivos)

| Arquivo | Mudanças |
|---|---|
| `PlayerMotionController.cs` | +1 linha: `HorizontalRotationSign` em `UpdateRotationEngine` |
| `FreeAimController.cs` | Ramo 3 reescrito (SmoothDamp + translação); Ramos 1 e 2 mantidos; `Reset()` inclui `currentVelocity`; `GetOffsets()` retorna `posOffset` |
| `Patch_SetHeadRotation.cs` | Roll orgânico via `RawHorizontalSpeed * HorizontalRotationSign + SmoothDamp`; cache + `_headRotVecField` integralmente mantidos |
| `NewSwayController.cs` | +1 linha: `FreeAimController.Reset()` no bloco `_wasAimingLastFrame` |
| `Patch_CalculateCameraPosition_HandLayers.cs` | +`UpdateWeaponInertia()` no início; `GetOffsets()` retorna `posOffset` somado ao `totalPos` |

---

## PARTE 5 — Pontos para discussão de simplificação

1. **Três camadas de head motion:** `NewDeadzoneController` (deadzone), `HeadRotController` (lean tilt), `Patch_Look` (small movements). Podem ter redundância.

2. **SwayController vs. NewSwayController:** Nomes confusos. O `SwayController` modifica apenas `MotionReact.SwayFactors` (interface nativa PWA). O `NewSwayController` é o sway procedural real do mod. São diferentes, mas poderiam ser fundidos.

3. **Dois sistemas de Parallax:** `ParallaxController` (rotação geral) e `ParallaxAdsController` (por disparo ADS). Candidatos a unificação.

4. **Configs duplicadas de peso/ergo:** `SwayCustomWeight/Ergo` e `DeadzoneCustomWeight/Ergo` têm os mesmos valores padrão (4.0 / 0.5). Poderiam ser um único par compartilhado.

5. **Debug Axes (6 configs):** Sliders de desenvolvimento que não deveriam estar na build de release.

6. **MasterSensitivityMultiplier:** Existe na config mas o `FreeAimController` atual **não o aplica** ao `sensitivity`. A nova versão deve garantir `sensitivity *= MasterSensitivityMultiplier.Value`.
