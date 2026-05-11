# Fix 01 — Redesign do sistema de stamina (StaminaMultiplier)

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-05-09
**Relacionado a:** [001-stamina-e-velocidade-01-spec.md](001-stamina-e-velocidade-01-spec.md)

---

## Problema identificado

O modo `Recovery` das stances 1 e 2 não funcionava: a stamina das mãos permanecia zerada mesmo com a stance ativa e intensity configurada.

### Causa raiz

O sistema de stamina foi implementado com dois mecanismos assimétricos:

| Mode | Mecanismo | Controle da stamina |
| --- | --- | --- |
| `Drain` | `TickStanceStamina` manipula `hands.Current` diretamente | Independente do vanilla |
| `Recovery` | `StanceStaminaRecoveryPatch` faz `__result *= intensity` | **Depende do vanilla** |
| `None` | Nenhum | Vanilla puro |

O Recovery dependia de `PlayerPhysicalClass.GetHandsRestorationFunc` retornar um valor positivo. Essa função calcula:

```csharp
// PlayerPhysicalClass.cs:1027
return baseValue * Float_7[(int)Epose_0] * StaminaRestoration.GetAt(energy) * (endurance+1f) / Single_0;
```

O fator `Float_7[(int)Epose_0]` (multiplicador por pose) retorna 0 em hipfire weapon-in-hand. Portanto `0 * 2.0 = 0` — a multiplicação não tinha efeito e a recovery nunca funcionou nesse contexto.

---

## Decisão de design — StaminaMultiplier unificado

Em vez de remendar o Recovery, o sistema foi redesenhado para usar um único `float StaminaMultiplier` por stance que unifica drain e recovery numa única fórmula simétrica.

### Escala de valores

| Valor | Comportamento |
| --- | --- |
| `0.0` | drain máximo (taxa = `_cachedAimDrainRate × 1.0`) |
| `0.5` | drain suave (taxa = `_cachedAimDrainRate × 0.5`) |
| `1.0` | **vanilla** — mod não toca stamina |
| `1.5` | recovery suave (taxa = `_cachedAimDrainRate × 0.5`) |
| `2.0` | recovery pleno (taxa = `_cachedAimDrainRate × 1.0`) |

### Fórmula unificada

```
delta/s = _cachedAimDrainRate × (multiplier - 1.0) × hands.Multiplier
```

- `multiplier < 1.0` → delta negativo → drain
- `multiplier = 1.0` → delta zero → vanilla livre (mod não toca)
- `multiplier > 1.0` → delta positivo → recovery

Ambos os casos usam `_cachedAimDrainRate` como base, tornando drain e recovery simétricos e proporcionais à mesma taxa de referência (ADS drain rate do EFT).

**Em ADS:** o mod não interfere em nenhum caso — o patch não zera `__result` durante ADS, preservando o comportamento vanilla do EFT.

---

## Arquivos modificados

| Arquivo | Mudança |
| --- | --- |
| `modded/StanceStaminaState.cs` | Removido `EStanceStaminaMode` enum; `Mode` + `Intensity` → `Multiplier`; `Reset()` e `ShouldApplyStamina` atualizados |
| `modded/StanceConfig.cs` | `StaminaMode` + `StaminaIntensity` → `StaminaMultiplier` |
| `modded/StanceManager.cs` | `TickStanceStamina`: fórmula unificada substituindo o Drain-only; `ApplyStaminaStance`: seta `Multiplier` |
| `modded/Patches/StanceStaminaRecoveryPatch.cs` | Lógica simplificada: zera `__result` sempre que `Multiplier ≠ 1.0` e não está em ADS |
| `modded/Plugin.cs` | `_stanceDefaults` (nova assinatura de tupla), `BindStance` (um bind em vez de dois), `SettingChanged` (uma subscription em vez de duas) |
| `PROPRIEDADES.md` | Remoção de `Stamina Mode` e `Stamina Intensity`; adição de `Stamina Multiplier` |

---

## Mapeamento de configuração (antiga → nova)

| Config antiga | Config nova | Notas |
| --- | --- | --- |
| `Drain` + `intensity 0.5` | `multiplier 0.5` | Taxa de drain idêntica |
| `Recovery` + `intensity 1.25` | `multiplier 1.25` | Recovery agora funciona (0.25× aimDrain) |
| `Recovery` + `intensity 1.50` | `multiplier 1.50` | Recovery agora funciona (0.50× aimDrain) |
| `None` + qualquer | `multiplier 1.0` | Vanilla |

Os nomes das `ConfigEntry` mudaram — BepInEx descarta o arquivo `.cfg` antigo e aplica os novos defaults automaticamente.

### Novos defaults

| Stance | StaminaMultiplier |
| --- | --- |
| Stance 0 - Vanilla | `0.5` |
| Stance 1 - Ready Up | `1.5` |
| Stance 2 - Ready Down | `2.0` |
| Stance 3 - Custom | `1.0` |

---

## Detalhes de implementação relevantes

### `StanceStaminaState.cs` — sem `using UnityEngine`

`ShouldApplyStamina` usa `System.Math.Abs` (não `Mathf.Approximately`) para não adicionar uma dependência de `UnityEngine` neste arquivo de estado puro.

### `(float)hands.TotalCapacity` — cast válido

`GClass774.TotalCapacity` é `GClass848<float>`. O cast explícito para `float` é válido — o próprio código-fonte do EFT usa o mesmo cast em `NormalValue = Current / (float)TotalCapacity`.

### Conflito de nomes na tupla `_stanceDefaults`

A tupla já tinha um campo `Multiplier` (int) para `MovementSpeedMultiplier`. O novo campo de stamina usa o nome `StaminaMultiplier` (float) para evitar ambiguidade. O campo int `Multiplier` permanece inalterado.

---

## Código de referência — `TickStanceStamina` após o fix

```csharp
public static void TickStanceStamina()
{
    try
    {
        if (_staminaConfigDirty) ApplyStaminaStance(_activeStaminaStance);
        if (!IsActiveContext()) return;
        if (!StanceStaminaState.ShouldApplyStamina) return;

        var player = Singleton<GameWorld>.Instance.MainPlayer;
        if (player.ProceduralWeaponAnimation?.IsAiming == true) return;

        var hands = player.Physical?.HandsStamina;
        if (hands == null) return;
        if (hands.Multiplier <= 0f) return;
        if (hands.ForceMode) return;

        float mult = StanceStaminaState.Multiplier;
        float delta = _cachedAimDrainRate * (mult - 1.0f) * hands.Multiplier * Time.deltaTime;
        if (float.IsNaN(delta) || float.IsInfinity(delta) || Mathf.Abs(delta) < 0.0001f) return;

        float prev = hands.Current;
        float target = Mathf.Clamp(prev + delta, 0f, (float)hands.TotalCapacity);
        if (Mathf.Abs(target - prev) < 0.0001f) return;
        hands.Current = target;
        NotifyHandsStaminaChanged(hands, prev);

        if (delta < 0 && target <= 0f && prev > 0f)
            hands.HandleExpiration();
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.TickStanceStamina] {ex}"); }
}
```

---

## Verificação

Testar no hideout com `Debug Apply In Hideout = true`:

1. **Stance 0 (mult=0.5):** stamina cai progressivamente sem ADS
2. **Stance 1 (mult=1.5):** stamina esgotada sobe visivelmente, mais devagar que Stance 2
3. **Stance 2 (mult=2.0):** stamina sobe mais rápido que Stance 1
4. **Stance 3 (mult=1.0):** stamina segue vanilla — comportamento idêntico a não ter o mod ativo
5. **Qualquer stance em ADS:** vanilla puro (patch não zera `__result`)
6. **Prone com `ApplyWhenProne = false`:** modo suspenso, vanilla
7. **Transição de stance:** ao trocar Stance 2 → Stance 0, drain inicia imediatamente
