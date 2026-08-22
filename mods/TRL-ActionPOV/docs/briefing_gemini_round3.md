---
title: Briefing Técnico para o Gemini — TRL-ActionPOV (Round 3)
date: 2026-08-18
status: 🟢 Vivo
authors: Antigravity + Gemini + Guilherme
---

# Briefing Técnico para o Gemini — TRL-ActionPOV
> Preparado pelo Antigravity após análise dos fontes reais do mod.
> Objetivo: corrigir 4 problemas na proposta anterior e fornecer contexto arquitetural que o Gemini não tem visibilidade.

---

## Contexto geral que o Gemini não viu

### 1. O FreeAimController atual é uma classe `static` com namespace próprio

```csharp
namespace TarkovIRL;
public static class FreeAimController { ... }
```

A proposta do Gemini o declarou como `public class` (não-static). Isso muda a assinatura de todos os métodos. Manter `static` é obrigatório — outros patches chamam `FreeAimController.ApplyInput(...)` e `FreeAimController.GetOffsets(...)` diretamente como métodos estáticos.

---

## Problema 1 — Guards de estado ausentes na proposta

A proposta do Gemini para `ApplyInput()` trata o `TargetOffset` como sempre acumulável. O código real possui três ramos condicionais **obrigatórios** antes de qualquer acúmulo:

```csharp
// FreeAimController.cs — código ATUAL (completo)
public static void ApplyInput(ref Vector2 deltaRotation)
{
    bool isAiming = PlayerMotionController.IsAiming;

    // RAMO 1: Mod desativado OU stance ativa (HighReady/LowReady/etc.) OU correndo OU left-shoulder
    // → retorna ao centro com Lerp e NÃO acumula nada
    if (!PrimeMover.EnableMod.Value 
        || !isAiming && StanceController.CurrentStance != EStance.None 
        || PlayerMotionController.IsSprinting 
        || Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay)
    {
        Offset = Vector2.Lerp(Offset, Vector2.zero, Time.deltaTime * PrimeMover.FreeAimReturnSpeed.Value);
        return; // ← sem acúmulo
    }

    // RAMO 2: Feature desativada nas configs (F12) para o estado atual
    // → retorna ao centro com velocidade configurável e NÃO acumula
    if (!isAiming && !PrimeMover.EnableFreeAim.Value 
        || isAiming && !PrimeMover.EnableFreeAimADS.Value)
    {
        float returnSpeed = isAiming ? PrimeMover.FreeAimReturnSpeedADS.Value : PrimeMover.FreeAimReturnSpeed.Value;
        Offset = Vector2.Lerp(Offset, Vector2.zero, Time.deltaTime * returnSpeed);
        return; // ← sem acúmulo
    }

    // RAMO 3: Modo ativo → aqui entra a nova lógica do Gemini (SmoothDamp + translação)
    // ... (proposta do Gemini vai aqui, dentro deste else)
}
```

**O Ramo 1 é especialmente crítico:** `Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay` é uma propriedade calculada que retorna `true` durante a transição de ombro (left ↔ right shoulder). Sem esse guard, a arma acumula deadzone durante a animação de ombro e causa um salto visual ao terminar.

Os bounds também **precisam do Lerp gradual** por stance/ADS — sem isso a arma teletransporta quando o jogador mira:
```csharp
// Bounds fazem Lerp para transicionar suavemente entre hipfire e ADS
float lerpSpeed = isAiming ? 6f : 15f;
_currentBoundsX = Mathf.Lerp(_currentBoundsX, isAiming ? PrimeMover.FreeAimBoundsXADS.Value : PrimeMover.FreeAimBoundsX.Value, Time.deltaTime * lerpSpeed);
_currentBoundsY = Mathf.Lerp(_currentBoundsY, isAiming ? PrimeMover.FreeAimBoundsYADS.Value : PrimeMover.FreeAimBoundsY.Value, Time.deltaTime * lerpSpeed);
```

---

## Problema 2 — Cache de Player não é populado automaticamente

O `ConditionalWeakTable<PWA, Player>` **não se popula sozinho**. O Harmony não injeta o `Player` — o cache precisa de lógica explícita de busca via reflection. O código atual do `Patch_SetHeadRotation` (completo para referência):

```csharp
// Patch_SetHeadRotation.cs — código ATUAL (completo)
public class Patch_SetHeadRotation : ModulePatch
{
    private static FieldInfo _playerField;      // FirearmController._player
    private static FieldInfo _fcField;          // PWA._firearmController
    private static FieldInfo _headRotVecField;  // PWA._headRotationVec
    private static ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache 
        = new ConditionalWeakTable<ProceduralWeaponAnimation, Player>();

    protected override MethodBase GetTargetMethod()
    {
        // Os FieldInfos são resolvidos UMA vez aqui, via AccessTools (HarmonyLib)
        _playerField    = AccessTools.Field(typeof(Player.FirearmController), "_player");
        _fcField        = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
        _headRotVecField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec");
        return typeof(ProceduralWeaponAnimation).GetMethod("SetHeadRotation", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
    {
        if (__instance == null) return true;

        // Tenta recuperar do cache. Se não está, busca via reflection e insere.
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

        if (player == null) return true; // sem player → deixa engine rodar
        if (!player.IsYourPlayer 
            || player.MovementContext.CurrentState.Name == EPlayerState.Stationary 
            || !PrimeMover.EnableMod.Value 
            || Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay)
            return true; // sem patch para NPCs ou estados especiais

        // ... aqui entra o roll e a lógica de head rotation do Gemini
        
        // OBRIGATÓRIO: escrever no field privado do PWA além de player.HeadRotation
        // O Tarkov lê _headRotationVec internamente para calcular animações de arma.
        // Sem isso, a arma não acompanha a cabeça corretamente.
        player.HeadRotation = finalRotation;
        _headRotVecField.SetValue(__instance, finalRotation);
        return false;
    }
}
```

**Ponto crítico:** o `_headRotVecField.SetValue(__instance, finalRotation)` não é opcional. O Tarkov usa o field `_headRotationVec` dentro do `ProceduralWeaponAnimation` para posicionar a arma em relação à cabeça. Se só escrevermos em `player.HeadRotation` e não nesse field, a câmera dos olhos se move mas a arma não acompanha — produz o efeito de "cabeça desconectada da arma".

---

## Problema 3 — `HorizontalRotationDelta` não pode ser dividido por `deltaTime`

O pipeline do `PlayerMotionController` para o eixo horizontal é:

```
num2 = DeltaAngle(lastRot.x, newRot.x)          → ângulo bruto em graus deste frame
num4 = num2 * dt                                  → já ponderado por deltaTime
_horizontalRotationHistory += num4               → acumulador suavizado
_horizontalRotationValue   = _horizontalRotationHistory * dt * RotationAverageDTMulti  → dt aplicado SEGUNDA vez
HorizontalRotationDelta    ← _horizontalRotationValue   → já ponderado por dt²
```

`HorizontalRotationDelta` carrega `dt²` embutido. Dividir por `Time.deltaTime` uma vez não cancela os dois — resulta em um valor que ainda tem `dt` na escala, oscila com framerate e não tem unidade física consistente.

**Para o Roll, usar diretamente:**
```csharp
// RawHorizontalSpeed = Mathf.Abs(num2 / dt) → velocidade real em graus/segundo, SEM dt embutido
float rawSpeed = PlayerMotionController.RawHorizontalSpeed; // Sempre positivo

// Para o sinal (direita vs. esquerda), derivar do frame:
// Opção A: manter um campo privado que compara Rotation.x frame a frame dentro do próprio Patch_SetHeadRotation
// Opção B: expor um HorizontalRotationSign no PlayerMotionController (já existe _verticalAvg como referência)
float signedSpeed = rawSpeed * (/* sinal do delta horizontal */);
float targetRoll  = Mathf.Clamp(-signedSpeed * rollIntensity, -maxRollAngle, maxRollAngle);
currentHeadRoll   = Mathf.SmoothDamp(currentHeadRoll, targetRoll, ref rollVelocity, rollRecoveryTime);
```

**Recomendação:** Adicionar `public static float HorizontalRotationSign` no `PlayerMotionController` (equivalente ao `VerticalTrend` já existente), calculado como `num2 >= 0 ? 1f : -1f` dentro de `UpdateRotationEngine`. Isso evita duplicar lógica de delta no Patch.

---

## Problema 4 — Onde e como chamar `FreeAimController.Reset()` no ADS

O `NewSwayController.UpdateLerp()` **já detecta** a transição de ADS via `_wasAimingLastFrame`:

```csharp
// NewSwayController.cs — bloco de detecção de transição (ATUAL)
if (isAiming != _wasAimingLastFrame)   // ← QUALQUER mudança de estado (entra E sai do ADS)
{
    _lerpPosHorizontal = 0f;
    _lerpPosVertical   = 0f;
    _lerpRot           = 0f;
    _posSmoothed       = Vector3.zero;
    _rotSmoothed       = Vector3.zero;
    _lagginPosSmoothed = Vector3.zero;
    _lagginRotSmoothed = Vector3.zero;
    // arrays zerados...
}
_wasAimingLastFrame = isAiming;
```

**A solução mais limpa:** adicionar `FreeAimController.Reset()` dentro desse mesmo bloco:

```csharp
if (isAiming != _wasAimingLastFrame)
{
    // ... resets existentes ...
    FreeAimController.Reset(); // ← adicionar aqui, zera TargetOffset + CurrentOffset + currentVelocity
}
```

Isso garante que o reset do free aim acontece **na mesma frame** que o sway é zerado, sem criar dependências entre classes adicionais.

---

## Contexto de EFT/Assembly que o Gemini precisa saber

### Como `ProceduralWeaponAnimation.SetHeadRotation` funciona nativamente

O método nativo recebe `Vector3 headRot` e:
1. Escreve em `_headRotationVec`
2. Usa esse vetor para calcular o offset de posição da arma em relação à câmera
3. O `CalculateCameraPosition` depois usa `_headRotationVec` para posicionar o `HandsContainer`

Por isso o `return false` sem escrever em `_headRotationVec` quebra o posicionamento da arma — o Tarkov continua lendo o valor antigo do field.

### Como `player.HeadRotation` se relaciona com a câmera

`HeadRotation` em `EFT.Player` é um `Vector3` (pitch, yaw, roll em graus) que controla a câmera dos olhos. O EFT lê isso no seu ciclo de câmera e aplica via `Quaternion.Euler`. Escrever nele diretamente é seguro — é exatamente o que o `Player.Look()` nativo faz.

### `EPlayerState.Stationary`

É o estado do player quando está no inventário aberto ou em algum contexto fixo (ex: bancada de reparo). Nesse estado, o `ProceduralWeaponAnimation` ainda recebe callbacks mas o player não deve ter efeitos cinéticos — por isso o guard `CurrentState.Name == EPlayerState.Stationary` precisa permanecer.

### `Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay`

É uma propriedade computada que retorna `true` enquanto `_shoulderFadeMultiplier < 1f`. O `_shoulderFadeMultiplier` fade para 0 ao entrar em left-shoulder e volta para 1 (com delay de 0.5s) ao retornar para right-shoulder. Durante esse fade, todos os offsets do mod são suprimidos via `Slerp(identity, totalRot, multiplier)`. O free aim e o roll da cabeça devem respeitar essa supressão.

---

## Resumo do que o Gemini deve produzir na próxima iteração

Com base nas correções acima, a reescrita final deve:

1. **`FreeAimController.cs`** — manter os 3 ramos condicionais originais; substituir apenas o Ramo 3 com `TargetOffset`/`CurrentOffset`/`SmoothDamp`/translação; adicionar `currentVelocity` ao `Reset()`; manter `_currentBoundsX/Y` com Lerp gradual.

2. **`Patch_SetHeadRotation.cs`** — manter a lógica de cache completa (GetTargetMethod + reflection); adicionar roll via `RawHorizontalSpeed` com sinal separado; manter escrita obrigatória em `_headRotVecField`.

3. **`PlayerMotionController.cs`** — adicionar `public static float HorizontalRotationSign` calculado em `UpdateRotationEngine` (uma linha, equivalente ao `VerticalTrend` existente).

4. **`NewSwayController.cs`** — adicionar `FreeAimController.Reset()` dentro do bloco `if (isAiming != _wasAimingLastFrame)`.

5. **`Patch_CalculateCameraPosition_HandLayers.cs`** — adicionar `FreeAimController.UpdateWeaponInertia()` no início do Postfix (antes dos outros controladores); atualizar a chamada `FreeAimController.GetOffsets()` para receber também `posOffset` e somar ao `totalPos`.
