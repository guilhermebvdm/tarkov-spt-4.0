# 001 — Stamina e Velocidade por Postura · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [001-stamina-e-velocidade-01-spec.md](001-stamina-e-velocidade-01-spec.md)
**Criado:** 2026-05-07

> Fonte primária: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) (Assembly-CSharp.dll, SHA256 `faef6f0b…7982`). Toda referência ao código do EFT cita `arquivo.cs:linha`.

## 1. Estratégia

Quatro pontos de hook, todos respeitando o ciclo de vida da raid e isolados ao `MainPlayer`:

1. **Drain de stamina (modo `Drain`):** **tick manual** dentro do `StanceManager.Update()`. Quando a stance ativa tem `Mode = Drain`, o jogador está em **hipfire** (não-ADS) e não está em prone-suspenso, acumula drain frame-a-frame em buffer e **flusha via `HandsStamina.UpdateStamina(novo)`** quando o acumulado ≥ 1f (limiar interno de `GClass774.UpdateStamina` — [GClass774.cs:392](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L392)). Usar `UpdateStamina` (não mutação direta de `Current`) garante que `action_3`, `InvokeChangedAction`, threshold de 15f e detecção de exhausted disparem normalmente. Em ADS, drain vanilla do EFT (`method_10`) toma conta; nosso tick faz no-op.
2. **Recovery de stamina (modo `Recovery`):** **Harmony postfix** em `PlayerPhysicalClass.GetHandsRestorationFunc` ([PlayerPhysicalClass.cs:1022](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L1022)). Filtra por `__instance.Player_0 == MainPlayer`. Multiplica `__result` por `Intensity` quando stance ativa = Recovery, fora de ADS, fora de prone-suspenso. `[HarmonyPriority(Priority.Low)]` para rodar depois de outros mods de stamina.
3. **Redutor de velocidade:** **chamada direta** a `MovementContext.AddStateSpeedLimit(value, cause)` ao trocar de stance e **re-aplicação defensiva no tick** com cache de último valor aplicado (resolve staleness de `MaxSpeed` dinâmico sem disparar evento `OnCharacterControllerSpeedLimitChanged` à toa). Cause: `(Player.ESpeedLimit)9001` — int reservado pelo mod, fora dos valores oficiais ([Player.cs:1584-1595](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L1584-L1595)).
4. **Ciclo de vida da raid:** Harmony postfix em `GameWorld.OnGameStarted` (start), `GameWorld.OnDestroy` e `BaseLocalGame.Stop` (end, idempotente via `_raidEnded` flag). `OnRaidEnd` chama `ResetState()` existente ([StanceManager.cs:365](../../modded/StanceManager.cs#L365)) para consolidar todo o cleanup do mod.

### Modelo de stance — usar o `enum Stance` já existente

O `StanceManager.cs` existente já define ([linha 9](../../modded/StanceManager.cs#L9)):

```csharp
public enum Stance { Default, Stance1, Stance2, Stance3 }
public static Stance CurrentStance { get; private set; } = Stance.Default;
```

Toda esta feature usa esse enum como chave — **não** introduzimos `int 0..3` paralelo. "Stance 0" no F12 e nas tabelas refere-se a `Stance.Default`. As 4 stances do enum mapeiam diretamente nas 4 seções do F12 (`Stance 0/1/2/3`).

**Cycle e Stance 0:** o cycle nativo do mod (`_EnableStance1/2/3 in Cycle`) transiciona `Default → Stance1 → Stance2 → Stance3 → Default`. Quando o cycle volta a `Stance.Default`, `OnStanceChanged` dispara e aplica a config da Stance 0 (drain leve, velocidade 90% por default) — não é "desligar a feature", é simplesmente reentrar na configuração base. Para desativar drain quando voltar ao Default, o jogador deve setar `Stance 0 Stamina Mode = None` no F12.

### Wiring da troca de stance — modificar a property existente

O `StanceManager.CurrentStance` ([linha 22](../../modded/StanceManager.cs#L22)) hoje é `public static Stance CurrentStance { get; private set; }`. Mutação acontece em 3 sítios internos: [linhas 94, 111, 116](../../modded/StanceManager.cs#L94) (tecla, scroll up, scroll down).

Para acionar `OnStanceChanged`, **modificamos a property existente** (não criamos field paralelo). O `private set` continua — call-sites não mudam:

```csharp
// ANTES (linha 22):
// public static Stance CurrentStance { get; private set; } = Stance.Default;

// DEPOIS:
private static Stance _currentStanceField = Stance.Default;
public static Stance CurrentStance
{
    get => _currentStanceField;
    private set
    {
        if (value == _currentStanceField) return;
        var prev = _currentStanceField;
        _currentStanceField = value;
        OnStanceChanged(prev, value);
    }
}
```

### Reuso do `ResetState()` existente

`StanceManager.ResetState()` ([linha 365](../../modded/StanceManager.cs#L365)) já reseta `CurrentStance = Stance.Default`, flags de tac sprint, caches de aim, dirty-flags de offsets, etc. Nosso `OnRaidEnd` chama `ResetState()` para consolidar o cleanup — sem duplicar lógica, sem deixar caches existentes vazando entre raids.

### Reuso do padrão dirty-flag

O `StanceManager` existente já usa flags de invalidação por `SettingChanged`: `_stanceValuesDirty`, `_sprintEnabledDirty`, e helpers `MarkStanceValuesDirty()` / `MarkSprintEnabledDirty()` ([linhas 70, 75](../../modded/StanceManager.cs#L70)). Para coerência arquitetural, esta feature segue o mesmo padrão: introduz `_staminaConfigDirty` + `MarkStaminaConfigDirty()`, e `OnStanceConfigChanged` (handler do `SettingChanged`) apenas seta a flag — `TickStanceStamina` percebe a flag suja e chama `ApplyStaminaStance(_activeStaminaStance)` uma única vez por tick.

### Granularidade do drain — atualização suave da HUD

`UpdateStamina` ignora deltas < 1f, então usá-lo deixaria a HUD atualizando em degraus de 1 unidade (visível mas chunky). Como **a visualização contínua na HUD é requisito**, o tick faz **mutação direta de `hands.Current` por frame** e dispara manualmente os eventos que a HUD escuta. Os eventos relevantes em `GClass774` são **eventos C# públicos** ([GClass774.cs:138-226](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L138-L226)):

| Evento público | Backing field (decompiler) | Disparado por (vanilla) |
|---|---|---|
| `OnValueChanged` | `action_3` | `Consume`/`UpdateStamina` quando stamina muda |
| `OnChanged` | `action_2` | `InvokeChangedAction()` (método público) |
| `OnThresholdPass` | `action_1` | quando cruza 15f |
| `OnExpired` | `action_0` | `HandleExpiration()` (método público) quando hits 0 |

Resolvemos os backing fields tentando uma **lista de candidatos** (nome público do evento → nomes do decompilador). Isso sobrevive a renomeação ou re-ordenação dos campos privados:

```csharp
private static FieldInfo ResolveBackingFieldByCandidates(Type t, params string[] candidates)
{
    foreach (var name in candidates)
    {
        var f = AccessTools.Field(t, name);
        if (f != null && f.FieldType == typeof(Action)) return f;
    }
    return null;
}
```

`InvokeChangedAction()` e `HandleExpiration()` são **públicos** — sem reflection, chamada direta.

`AimDrainRate` (constante de [BackendConfigSettingsClass.cs:904](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L904), imutável em runtime) é **cacheado em `OnRaidStart`** para evitar `Singleton<>.Instance` lookup todo frame.

Falha de resolução é detectada no `Awake` (helper `HasMissingReflection`) e logada como warning explícito — drain continua funcional, só os eventos para HUD param de disparar (degradação graciosa).

### Singleton — atenção ao namespace

Existem dois `Singleton<T>` no Assembly: o correto é **`Comfort.Common.Singleton<T>`**. `RootMotion.Singleton` seria importado por engano por autocomplete. **Todos os stubs declaram `using Comfort.Common;` explicitamente.**

Justificativas das escolhas:
- **Buffer + UpdateStamina (vs `Current -= drain`):** preserva todos os side-effects do `GClass774` (HUD, sons, exhausted, threshold de 15f, `Multiplier` guard). Mutação direta pularia eventos.
- **Tick manual para Drain:** spec funcional decidiu Opção B (drain em hipfire). `method_10` só é chamado em ADS via `EConsumptionTarget.Hands`.
- **Postfix multiplicativo para Recovery:** preserva fatores nativos.
- **`AddStateSpeedLimit`:** mesmo mecanismo que o EFT usa para Weight, Swamp, Fall, Aiming.
- **Modificar a property `CurrentStance` existente (vs criar field paralelo):** mantém um único ponto de verdade; `private set` preserva encapsulamento.
- **`enum Stance` (vs `int`):** evita dois mapeamentos paralelos; bate com a base do mod existente.
- **Cache de `_lastAppliedSpeedLimit`:** evita disparar `OnCharacterControllerSpeedLimitChanged` à toa (60 vezes/s).

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`GameWorld.OnGameStarted`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584) (GameWorld.cs:2584) | Harmony postfix | Disparar `StanceManager.OnRaidStart()` |
| [`GameWorld.OnDestroy`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111) (GameWorld.cs:2111) | Harmony postfix | Disparar `StanceManager.OnRaidEnd()` (idempotente) |
| [`BaseLocalGame.Stop(string, ExitStatus, string, float)`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame.cs#L1018) | Harmony postfix | Idem `OnRaidEnd` — cobre `Left`/`Killed`/`MIA`. **Resolver com tipos explícitos** (4 params) para evitar overload ambíguo. |
| [`PlayerPhysicalClass.GetHandsRestorationFunc`](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L1022) (PlayerPhysicalClass.cs:1022) | Harmony postfix `[HarmonyPriority(Priority.Low)]` | Multiplicar regen final por `Intensity` quando stance ativa = Recovery, filtrado por `MainPlayer` |
| [`MovementContext.AddStateSpeedLimit`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1672) e [`RemoveStateSpeedLimit`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1790) | Chamada direta (sem patch) | Aplicar/retirar redutor 50–100%. Cause `(ESpeedLimit)9001`. |

> **Nota:** o postfix em `PlayerPhysicalClass.method_10` foi removido. Drain agora é tick manual em `StanceManager.Update()`.

### Constantes de referência (vanilla)

[BackendConfigSettingsClass.cs:903-992](../../../../references/eft-decompiled/Assembly-CSharp/BackendConfigSettingsClass.cs#L903-L992):

```csharp
public float AimDrainRate = 3f;          // base drain ao mirar — referência para o tick manual de Drain
public float HandsCapacity = 150f;
public float HandsRestoration = 5f;       // base regen — multiplicada pelo postfix de Recovery
```

### API de drain (`GClass774`)

[GClass774.cs:23, 75, 241, 389](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs):

```csharp
public float Current;
public float Multiplier { get; }                                          // ≤ 0 desativa drain
public float Consume(PlayerPhysicalClass.GClass773 consumption, ...)
public void  UpdateStamina(float stamina)                                 // aplica se |delta| ≥ 1f e dispara action_3 + InvokeChangedAction
```

### Fórmula de regeneração das mãos (vanilla)

[PlayerPhysicalClass.cs:1022-1029](../../../../references/eft-decompiled/Assembly-CSharp/PlayerPhysicalClass.cs#L1022-L1029):

```csharp
public override float GetHandsRestorationFunc()
{
    return method_21(HandsRestoreRate);   // = HandsRestoration (5f)
}

public float method_21(float baseValue)
{
    return baseValue
         * Float_7[(int)Epose_0]
         * StaminaRestoration.GetAt(Player_0.HealthController.Energy.Normalized)
         * (Skills.EnduranceBuffRestoration + 1f)
         / Single_0;
}
```

Postfix multiplica `__result` por `Intensity`, preservando todos os fatores nativos.

### Detecção de Prone

API pública confirmada em [Player.cs:24609](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L24609):

```csharp
public bool IsInPronePose => MovementContext.IsInPronePose;
```

### API de speed limit

[MovementContext.cs:1672-1812](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1672-L1812):

```csharp
public void AddStateSpeedLimit(float speedLimit, Player.ESpeedLimit cause);  // ignora se cause já registrada
public void RemoveStateSpeedLimit(Player.ESpeedLimit cause);
```

- `StateSpeedLimit` final = **mínimo** entre todos os limits ativos.
- Valores são **absolutos em m/s**: padrão do EFT é `walkSpeedLimit * MaxSpeed`.
- `MaxSpeed` é dinâmico — depende da skill Strength.
- Nossa cause: `(Player.ESpeedLimit)9001` — int fora da enum oficial.

## 3. Novas propriedades F12 (BepInEx)

5 propriedades por stance × 4 stances = **20 entradas novas**. Adicionar após os offsets em `Stance 1/2/3`. Para `Stance 0` (= `Stance.Default`), criar **seção nova** contendo apenas estas 5.

### Schema

| Nome (EN) | Tipo | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|
| `Stance N Stamina Mode` | enum (`None`/`Drain`/`Recovery`) | — | — | Como esta stance afeta a stamina das mãos. None = sem efeito. Drain = consome stamina enquanto ativa em hipfire. Recovery = acelera a regeneração base em hipfire. |
| `Stance N Stamina Intensity` | float | 0.0 a 2.0 | **(Avançado)** | Multiplicador de intensidade do efeito (drain ou recovery). 0.25=muito baixo · 0.50=baixo · 1.00=normal · 1.50=alto · 2.00=muito alto. Sem efeito se Mode = None. |
| `Stance N Modifies Movement Speed` | bool | — | — | Quando habilitado, esta stance aplica um redutor à velocidade de movimentação. |
| `Stance N Movement Speed Multiplier` | int (%) | 50 a 100 | **(Avançado)** | Redutor de velocidade em %. 50 = metade · 75 = um pouco mais lento · 100 = sem redução. |
| `Stance N Apply When Prone` | bool | — | **(Avançado)** | Aplicar esta stance também quando o personagem está deitado. Desligado por padrão. |

### Defaults (instalação limpa)

| Stance | Posição | Stamina Mode | Stamina Intensity | Modifies Speed | Speed Multiplier | Apply When Prone |
|---|---|---|---|---|---|---|
| `Stance.Default` (Stance 0) | Pronto de tiro (vanilla) | `Drain` | `0.50` | `true` | `90` | `false` |
| `Stance.Stance1` (Stance 1) | Pronto baixo | `Recovery` | `2.00` | `true` | `100` | `false` |
| `Stance.Stance2` (Stance 2) | Coringa | `None` | `1.00` | `false` | `100` | `false` |
| `Stance.Stance3` (Stance 3) | Pronto alto | `Recovery` | `1.50` | `true` | `95` | `false` |

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| [`modded/Plugin.cs`](../../modded/Plugin.cs) | MODIFICAR | Bind das 20 `ConfigEntry` (defaults da tabela §3); enum `EStanceStaminaMode`; `Dictionary<Stance, StanceConfig> _stanceConfigs`; constante `StanceSpeedLimitCauseId = 9001`; subscribe global em `SettingChanged` chamando `MarkStaminaConfigDirty()`; registrar 4 patches no `Awake`. **Manter `public static new ManualLogSource Logger;`**. |
| [`modded/StanceManager.cs`](../../modded/StanceManager.cs) | MODIFICAR | (1) Modificar a property `CurrentStance` (linha 22) para detectar mudança e disparar `OnStanceChanged`. (2) Adicionar `_activeStaminaStance` (`Stance`, default `Stance.Default`). (3) Adicionar membros: `OnRaidStart`/`OnRaidEnd` (idempotente, chama `ResetState()`), `ApplyStaminaStance`, `OnStanceChanged`, `TickStanceStamina`, `EvaluateProneSuspensionTick`, `IsActiveContext`, `GetActiveStaminaStance`, `MarkStaminaConfigDirty`. **Não usar `partial class`**. |
| `modded/StanceStaminaState.cs` | CRIAR | Classe estática com `Mode`, `Intensity`, `IsSuspendedByProne`, `AccumulatedDrain` (buffer), `Reset()` |
| `modded/StanceConfig.cs` | CRIAR | Classe agrupando 5 `ConfigEntry` |
| `modded/Patches/StanceStaminaRecoveryPatch.cs` | CRIAR | Postfix em `GetHandsRestorationFunc`, filtrado por MainPlayer + hideout, `[HarmonyPriority(Priority.Low)]`, try/catch |
| `modded/Patches/RaidLifecyclePatches.cs` | CRIAR | 3 patches com **resolução por tipos explícitos** para `BaseLocalGame.Stop` |
| [`modded/CameraRotationMod.csproj`](../../modded/CameraRotationMod.csproj) | VERIFICAR | `<LangVersion>` ≥ 9 (recomendado). Stubs usam apenas `!(... is X)` — compatível com qualquer versão. |
| [`PROPRIEDADES.md`](../../PROPRIEDADES.md) | MODIFICAR | Nova seção `Stance 0` + 5 entradas em cada `Stance N` |
| [`README.md`](../../README.md) | MODIFICAR | Documentar feature + reserva de `(Player.ESpeedLimit)9001` |

## 5. Stubs de código

> **Padrão de imports:** `using Comfort.Common;` (não `RootMotion.Singleton`), `using EFT;`, `using HarmonyLib;`, `using SPT.Reflection.Patching;`, `using UnityEngine;`, `using BepInEx.Configuration;` conforme aplicável.

### `modded/StanceStaminaState.cs` (CRIAR)

```csharp
namespace CameraRotationMod;

public enum EStanceStaminaMode { None, Drain, Recovery }

public static class StanceStaminaState
{
    public static EStanceStaminaMode Mode = EStanceStaminaMode.None;
    public static float Intensity = 1f;
    public static bool IsSuspendedByProne = false;

    public static bool ShouldApplyStamina => Mode != EStanceStaminaMode.None && !IsSuspendedByProne;

    public static void Reset()
    {
        Mode = EStanceStaminaMode.None;
        Intensity = 1f;
        IsSuspendedByProne = false;
    }
}
```

### `modded/StanceConfig.cs` (CRIAR)

```csharp
using BepInEx.Configuration;

namespace CameraRotationMod;

public sealed class StanceConfig
{
    public ConfigEntry<EStanceStaminaMode> StaminaMode;
    public ConfigEntry<float>              StaminaIntensity;
    public ConfigEntry<bool>               ModifiesMovementSpeed;
    public ConfigEntry<int>                MovementSpeedMultiplier;
    public ConfigEntry<bool>               ApplyWhenProne;
}
```

### `modded/Plugin.cs` (trecho — adicionar ao existente)

```csharp
using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CameraRotationMod.Patches;
using EFT;

namespace CameraRotationMod;

public class Plugin : BaseUnityPlugin
{
    // ⚠️ MANTER o `public static new` — shadow estático do BaseUnityPlugin.Logger.
    // Sem o `new`, vira shadowing implícito (CS0108) e os stubs static deste backlog quebram.
    public static new ManualLogSource Logger;

    public const int StanceSpeedLimitCauseId = 9001;
    public static Player.ESpeedLimit StanceSpeedLimitCause => (Player.ESpeedLimit)StanceSpeedLimitCauseId;

    /// <summary>Indexado pelo enum Stance existente — não introduzir int paralelo.</summary>
    public static readonly Dictionary<Stance, StanceConfig> _stanceConfigs = new(4);

    private static readonly (Stance Stance, string Section, EStanceStaminaMode Mode, float Intensity, bool ModSpeed, int Multiplier, bool ApplyProne)[]
        _stanceDefaults =
    {
        (Stance.Default, "Stance 0", EStanceStaminaMode.Drain,    0.50f, true,  90,  false),
        (Stance.Stance1, "Stance 1", EStanceStaminaMode.Recovery, 2.00f, true,  100, false),
        (Stance.Stance2, "Stance 2", EStanceStaminaMode.None,     1.00f, false, 100, false),
        (Stance.Stance3, "Stance 3", EStanceStaminaMode.Recovery, 1.50f, true,  95,  false),
    };

    public void Awake()
    {
        Logger = base.Logger;
        // ... (binds existentes)

        foreach (var d in _stanceDefaults)
            _stanceConfigs[d.Stance] = BindStance(d);

        // SettingChanged → marca dirty (padrão coerente com MarkStanceValuesDirty existente)
        foreach (var cfg in _stanceConfigs.Values)
        {
            cfg.StaminaMode.SettingChanged              += OnStanceConfigChanged;
            cfg.StaminaIntensity.SettingChanged         += OnStanceConfigChanged;
            cfg.ModifiesMovementSpeed.SettingChanged    += OnStanceConfigChanged;
            cfg.MovementSpeedMultiplier.SettingChanged  += OnStanceConfigChanged;
            cfg.ApplyWhenProne.SettingChanged           += OnStanceConfigChanged;
        }

        new StanceStaminaRecoveryPatch().Enable();
        new GameWorldOnGameStartedPatch().Enable();
        new GameWorldOnDestroyPatch().Enable();
        new BaseLocalGameStopPatch().Enable();

        // Validar reflection — se BSG renomear backing fields de events em update do EFT,
        // a HUD para de receber sinal de drain. Drain continua funcional, só visualmente silencioso.
        if (StanceManager.HasMissingReflection(out var missing))
        {
            Logger.LogWarning(
                "[StanceStamina] Reflection incompleta — HUD pode não atualizar durante drain. " +
                $"Campos não resolvidos: {string.Join(", ", missing)}. " +
                "Provavelmente uma nova versão do EFT renomeou backing fields de eventos de GClass774. " +
                "Drain continua funcional, mas eventos para a HUD podem não disparar.");
        }
    }

    private StanceConfig BindStance(
        (Stance Stance, string Section, EStanceStaminaMode Mode, float Intensity, bool ModSpeed, int Multiplier, bool ApplyProne) d)
    {
        // Map Stance → label numérico para os nomes das ConfigEntry
        int n = (int)d.Stance;   // Default=0, Stance1=1, ...

        return new StanceConfig
        {
            StaminaMode = Config.Bind(d.Section, $"Stance {n} Stamina Mode", d.Mode,
                new ConfigDescription(
                    "Como esta stance afeta a stamina das mãos. None = sem efeito. Drain = consome stamina enquanto ativa em hipfire. Recovery = acelera a regeneração base em hipfire.",
                    null,
                    new ConfigurationManagerAttributes { Order = 5 })),
            StaminaIntensity = Config.Bind(d.Section, $"Stance {n} Stamina Intensity", d.Intensity,
                new ConfigDescription(
                    "Multiplicador de intensidade do efeito. 0.25=muito baixo · 0.50=baixo · 1.00=normal · 1.50=alto · 2.00=muito alto.",
                    new AcceptableValueRange<float>(0f, 2f),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 })),
            ModifiesMovementSpeed = Config.Bind(d.Section, $"Stance {n} Modifies Movement Speed", d.ModSpeed,
                new ConfigDescription(
                    "Quando habilitado, esta stance aplica um redutor à velocidade de movimentação.",
                    null,
                    new ConfigurationManagerAttributes { Order = 3 })),
            MovementSpeedMultiplier = Config.Bind(d.Section, $"Stance {n} Movement Speed Multiplier", d.Multiplier,
                new ConfigDescription(
                    "Redutor de velocidade em %. 50 = metade · 75 = um pouco mais lento · 100 = sem redução.",
                    new AcceptableValueRange<int>(50, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 })),
            ApplyWhenProne = Config.Bind(d.Section, $"Stance {n} Apply When Prone", d.ApplyProne,
                new ConfigDescription(
                    "Aplicar esta stance também quando o personagem está deitado. Desligado por padrão porque pode conflitar com as animações nativas de prone.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 })),
        };
    }

    private static void OnStanceConfigChanged(object sender, EventArgs e)
    {
        // Coerente com o padrão MarkStanceValuesDirty/MarkSprintEnabledDirty existente
        StanceManager.MarkStaminaConfigDirty();
    }
}
```

### `modded/StanceManager.cs` (adições à classe estática existente — **não criar partial**)

> Inserir os membros abaixo dentro da declaração `public static class StanceManager { ... }` existente.

**Modificação 1 — property `CurrentStance` (substituir a declaração da linha 22):**

```csharp
private static Stance _currentStanceField = Stance.Default;
public static Stance CurrentStance
{
    get => _currentStanceField;
    private set
    {
        if (value == _currentStanceField) return;
        var prev = _currentStanceField;
        _currentStanceField = value;
        OnStanceChanged(prev, value);
    }
}
```

Os 3 sítios internos que mutam ([linhas 94, 111, 116](../../modded/StanceManager.cs#L94)) continuam idênticos — `private set` ainda funciona dentro da classe.

**Modificação 2 — adicionar membros à classe:**

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using UnityEngine;

// === ESTADO ===
// Default `true` = "nenhuma raid começou ainda" — defende contra OnRaidEnd
// disparando antes de qualquer OnRaidStart (ex.: BepInEx reload no hideout).
private static bool _raidEnded = true;
private static Stance _activeStaminaStance = Stance.Default;
private static bool _staminaConfigDirty = true;
private static float _lastAppliedSpeedLimit = -1f;   // -1 = nada aplicado; força re-apply na 1ª iteração
private static float _cachedAimDrainRate = 3f;       // cacheado em OnRaidStart — fallback ao default vanilla

public static Stance GetActiveStaminaStance() => _activeStaminaStance;

/// <summary>
/// Marca config de stamina/velocidade como suja. Coerente com MarkStanceValuesDirty/MarkSprintEnabledDirty.
/// </summary>
public static void MarkStaminaConfigDirty() => _staminaConfigDirty = true;

// === GUARDS ===
public static bool IsActiveContext()
{
    var gw = Singleton<GameWorld>.Instance;
    if (gw == null || gw.MainPlayer == null) return false;
    return !(gw.MainPlayer is HideoutPlayer);
}

// === LIFECYCLE ===
public static void OnRaidStart()
{
    try
    {
        _raidEnded = false;
        _activeStaminaStance = Stance.Default;
        _staminaConfigDirty = true;          // força re-apply
        _lastAppliedSpeedLimit = -1f;

        // Cachear AimDrainRate (constante imutável em runtime) para evitar Singleton lookup todo frame
        var backend = Singleton<BackendConfigSettingsClass>.Instance;
        if (backend?.Stamina != null)
            _cachedAimDrainRate = backend.Stamina.AimDrainRate;

        StanceStaminaState.Reset();
        ApplyStaminaStance(_activeStaminaStance);
        Plugin.Logger.LogInfo("[StanceManager] Raid start — state initialized");
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnRaidStart] {ex}"); }
}

public static void OnRaidEnd()
{
    if (_raidEnded) return;                  // idempotente
    _raidEnded = true;
    try
    {
        var mc = Singleton<GameWorld>.Instance?.MainPlayer?.MovementContext;
        mc?.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);

        StanceStaminaState.Reset();
        _activeStaminaStance = Stance.Default;
        _lastAppliedSpeedLimit = -1f;

        // Reuso: ResetState() existente em StanceManager.cs:365 já limpa
        // CurrentStance, tac sprint, caches de aim, dirty-flags de offsets.
        // Consolida o cleanup num único método em vez de duplicar.
        ResetState();

        Plugin.Logger.LogInfo("[StanceManager] Raid end — state cleaned");
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnRaidEnd] {ex}"); }
}

// === APLICAÇÃO DE CONFIG ===
public static void ApplyStaminaStance(Stance stance)
{
    if (!IsActiveContext()) return;
    if (!Plugin._stanceConfigs.TryGetValue(stance, out var cfg)) return;

    var mc = Singleton<GameWorld>.Instance.MainPlayer.MovementContext;

    mc.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
        _lastAppliedSpeedLimit = -1f;            // força próximo tick a re-aplicar

    StanceStaminaState.Mode      = cfg.StaminaMode.Value;
    StanceStaminaState.Intensity = cfg.StaminaIntensity.Value;

    bool inProne = Singleton<GameWorld>.Instance.MainPlayer.IsInPronePose;
    StanceStaminaState.IsSuspendedByProne = inProne && !cfg.ApplyWhenProne.Value;

    if (cfg.ModifiesMovementSpeed.Value && !StanceStaminaState.IsSuspendedByProne)
    {
        float fraction = cfg.MovementSpeedMultiplier.Value / 100f;
        float target = fraction * mc.MaxSpeed;
        mc.AddStateSpeedLimit(target, Plugin.StanceSpeedLimitCause);
        _lastAppliedSpeedLimit = target;
    }

    _staminaConfigDirty = false;             // config recém-aplicada
}

public static void OnStanceChanged(Stance previousStance, Stance newStance)
{
    try
    {
        _activeStaminaStance = newStance;
        ApplyStaminaStance(newStance);
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.OnStanceChanged] {ex}"); }
}

// === REFLECTION CACHEADA PARA BACKING FIELDS DE EVENTOS DE GClass774 ===
//
// Eventos públicos: OnValueChanged (action_3), OnChanged (action_2), OnThresholdPass (action_1).
// Os backing fields são private e foram renomeados pelo decompilador.
// Resolvemos por lista de candidatos: nome público primeiro, nomes do decompilador como fallback.
private static readonly FieldInfo _onValueChangedBacking =
    ResolveBackingFieldByCandidates(typeof(GClass774), nameof(GClass774.OnValueChanged), "action_3");
private static readonly FieldInfo _onThresholdPassBacking =
    ResolveBackingFieldByCandidates(typeof(GClass774), nameof(GClass774.OnThresholdPass), "action_1");

/// <summary>True se algum field-info essencial para HUD updates não foi resolvido.</summary>
public static bool HasMissingReflection(out List<string> missing)
{
    missing = new List<string>();
    if (_onValueChangedBacking == null) missing.Add("GClass774.OnValueChanged backing field");
    if (_onThresholdPassBacking == null) missing.Add("GClass774.OnThresholdPass backing field");
    return missing.Count > 0;
}

/// <summary>
/// Resolve um backing field privado de event Action tentando uma lista ordenada de candidatos.
/// Estratégia: passar primeiro o nome do <b>evento público</b> (estável se BSG mantiver a API
/// pública de GClass774), depois nomes do <b>decompilador ILSpy</b> (action_N, podem mudar
/// entre patches do EFT porque os backing fields são privados/renomeados).
/// Retorna o primeiro Action field encontrado, ou null se nenhum candidato bater.
/// Falhas são detectadas no Awake via HasMissingReflection e logadas como warning.
/// </summary>
private static FieldInfo ResolveBackingFieldByCandidates(Type t, params string[] candidates)
{
    foreach (var name in candidates)
    {
        var f = AccessTools.Field(t, name);
        if (f != null && f.FieldType == typeof(Action)) return f;
    }
    return null;
}

private static void NotifyHandsStaminaChanged(GClass774 hands, float prevValue)
{
    try
    {
        // OnValueChanged — sinal "stamina mudou" que a HUD escuta
        ((Action)_onValueChangedBacking?.GetValue(hands))?.Invoke();
        // OnChanged — propaga para listeners gerais (método público, sem reflection)
        hands.InvokeChangedAction();
        // OnThresholdPass — só dispara quando cruza o threshold de 15f (som "tired")
        if ((hands.Current < 15f) ^ (prevValue < 15f))
            ((Action)_onThresholdPassBacking?.GetValue(hands))?.Invoke();
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[NotifyHandsStaminaChanged] {ex}"); }
}

// === DRAIN MANUAL EM HIPFIRE (modo Drain) — atualiza HUD por frame ===
public static void TickStanceStamina()
{
    try
    {
        // Re-aplicar config se foi marcada suja por SettingChanged
        if (_staminaConfigDirty) ApplyStaminaStance(_activeStaminaStance);

        if (!IsActiveContext()) return;
        if (!StanceStaminaState.ShouldApplyStamina) return;
        if (StanceStaminaState.Mode != EStanceStaminaMode.Drain) return;

        var player = Singleton<GameWorld>.Instance.MainPlayer;

        // Em ADS o drain vanilla do EFT toma conta — nosso tick faz no-op.
        if (player.ProceduralWeaponAnimation?.IsAiming == true) return;

        var hands = player.Physical?.HandsStamina;
        if (hands == null) return;

        // Honra o guard interno do GClass774 — Multiplier ≤ 0 desativa drenagem
        if (hands.Multiplier <= 0f) return;
        // Honra ForceMode do GClass774 — Consume() vanilla pula redução de Current quando ForceMode = true
        if (hands.ForceMode) return;

        // _cachedAimDrainRate populado em OnRaidStart (constante imutável)
        float drain = _cachedAimDrainRate * StanceStaminaState.Intensity * hands.Multiplier * Time.deltaTime;
        if (!float.IsFinite(drain) || drain < 0.0001f) return;     // tolerância prática (não float.Epsilon)

        // Mutação direta de Current — drain suave por frame, HUD atualiza fluido.
        float prev = hands.Current;
        float target = Mathf.Max(0f, prev - drain);
        hands.Current = target;
        NotifyHandsStaminaChanged(hands, prev);   // dispara OnValueChanged + OnChanged (+ OnThresholdPass se cruzou 15f)

        // Replica HandleExpiration vanilla para disparar OnExpired event quando hits 0
        if (target <= 0f && prev > 0f)
            hands.HandleExpiration();
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.TickStanceStamina] {ex}"); }
}

// === SUSPENSÃO POR PRONE + REFRESH DEFENSIVO DE SPEED LIMIT (com cache) ===
public static void EvaluateProneSuspensionTick()
{
    try
    {
        if (!IsActiveContext()) return;
        if (!Plugin._stanceConfigs.TryGetValue(_activeStaminaStance, out var cfg)) return;

        var player = Singleton<GameWorld>.Instance.MainPlayer;
        bool wasSuspended = StanceStaminaState.IsSuspendedByProne;
        bool isSuspended  = player.IsInPronePose && !cfg.ApplyWhenProne.Value;

        if (wasSuspended != isSuspended)
        {
            StanceStaminaState.IsSuspendedByProne = isSuspended;
                        if (isSuspended)
            {
                player.MovementContext.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
                _lastAppliedSpeedLimit = -1f;
                return;
            }
        }

        // Re-aplicação defensiva — só executa se o valor calculado mudou
        // (cobre staleness de MaxSpeed sem disparar OnCharacterControllerSpeedLimitChanged à toa)
        if (cfg.ModifiesMovementSpeed.Value && !StanceStaminaState.IsSuspendedByProne)
        {
            var mc = player.MovementContext;
            float target = (cfg.MovementSpeedMultiplier.Value / 100f) * mc.MaxSpeed;

            // Tolerância evita re-apply por flutuação numérica de MaxSpeed
            if (Mathf.Abs(target - _lastAppliedSpeedLimit) > 0.001f)
            {
                mc.RemoveStateSpeedLimit(Plugin.StanceSpeedLimitCause);
                mc.AddStateSpeedLimit(target, Plugin.StanceSpeedLimitCause);
                _lastAppliedSpeedLimit = target;
            }
        }
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceManager.EvaluateProneSuspensionTick] {ex}"); }
}
```

> **Hook em `Plugin.Update()`** (já existente) — adicionar as 2 chamadas novas:
>
> ```csharp
> public void Update()
> {
>     SpringGetPatch.ValidateSpringCache();
>     StanceManager.Update();
>     StanceManager.UpdateTacSprint();
>     StanceManager.TickStanceStamina();           // NOVO
>     StanceManager.EvaluateProneSuspensionTick(); // NOVO
>     UpdateCameraOffset();
> }
> ```

### `modded/Patches/StanceStaminaRecoveryPatch.cs` (CRIAR)

```csharp
using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches;

public class StanceStaminaRecoveryPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/PlayerPhysicalClass.cs:1022
        return AccessTools.Method(
            typeof(PlayerPhysicalClass),
            nameof(PlayerPhysicalClass.GetHandsRestorationFunc));
    }

    [PatchPostfix]
    [HarmonyPriority(Priority.Low)]      // rodar depois de outros mods de stamina
    private static void Postfix(PlayerPhysicalClass __instance, ref float __result)
    {
        try
        {
            var gw = Singleton<GameWorld>.Instance;
            if (gw?.MainPlayer == null) return;
            if (gw.MainPlayer is HideoutPlayer) return;             // hideout — feature inerte
            if (__instance.Player_0 != gw.MainPlayer) return;       // só MainPlayer

            if (StanceStaminaState.Mode != EStanceStaminaMode.Recovery) return;
            if (StanceStaminaState.IsSuspendedByProne) return;

            // Recovery não aplica em ADS — EFT já zera regen ali
            if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return;

            float intensity = StanceStaminaState.Intensity;
            if (!float.IsFinite(intensity)) return;
            __result *= intensity;
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[StanceStaminaRecoveryPatch] {ex}");
        }
    }
}
```

### `modded/Patches/RaidLifecyclePatches.cs` (CRIAR)

```csharp
using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches;

public class GameWorldOnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        // ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (public virtual)
        => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

    [PatchPostfix]
    private static void Postfix()
    {
        try { StanceManager.OnRaidStart(); }
        catch (Exception ex) { Plugin.Logger.LogError($"[GameWorldOnGameStartedPatch] {ex}"); }
    }
}

public class GameWorldOnDestroyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        // ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (public virtual)
        => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));

    [PatchPostfix]
    private static void Postfix()
    {
        try { StanceManager.OnRaidEnd(); }
        catch (Exception ex) { Plugin.Logger.LogError($"[GameWorldOnDestroyPatch] {ex}"); }
    }
}

public class BaseLocalGameStopPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/BaseLocalGame.cs:1018 — cobre Left/Killed/MIA
        // Resolução por tipos explícitos para evitar overload ambíguo.
        return AccessTools.Method(
            typeof(BaseLocalGame),
            nameof(BaseLocalGame.Stop),
            new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) });
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try { StanceManager.OnRaidEnd(); }   // idempotente via _raidEnded
        catch (Exception ex) { Plugin.Logger.LogError($"[BaseLocalGameStopPatch] {ex}"); }
    }
}
```

## 6. Fluxo de dados

### Drain por stance (modo Drain) — mutação direta com eventos manuais

```
Frame em raid (não-hideout)
   ↓
StanceManager.Update() → TickStanceStamina()
   ↓ (re-apply se _staminaConfigDirty)
   ↓ (guards: IsActiveContext + ShouldApplyStamina + Mode == Drain)
Player não está em ADS && hands.Multiplier > 0?
   ↓ sim
drain = AimDrainRate × Intensity × Multiplier × deltaTime
hands.Current = max(0, Current − drain)         ← mutação direta por frame
NotifyHandsStaminaChanged(hands, prev):
   action_3.Invoke()          ← HUD atualiza no frame
   InvokeChangedAction()      ← listeners gerais
   action_1.Invoke() se cruzou 15f  ← som "tired"
   ↓
HUD vê valor novo todo frame; barra anima fluida
```

### Recovery por stance (modo Recovery)

```
EFT chama PlayerPhysicalClass.GetHandsRestorationFunc para cada Player
   ↓
Postfix StanceStaminaRecoveryPatch:
  - filtro: gw.MainPlayer != null && !(MainPlayer is HideoutPlayer)
  - filtro: __instance.Player_0 == MainPlayer
  - guard: Mode == Recovery && !IsSuspendedByProne && !IsAiming
   ↓
__result *= Intensity
```

### Velocidade — registro inicial + refresh com cache

```
Jogador troca de stance
   ↓
property setter de CurrentStance dispara OnStanceChanged(prev, new)
   ↓
StanceManager.ApplyStaminaStance(new)
   ↓ (reset _lastAppliedSpeedLimit = -1)
mc.RemoveStateSpeedLimit + AddStateSpeedLimit(target)
   ↓ _lastAppliedSpeedLimit = target

[ ... cada frame seguinte ... ]

EvaluateProneSuspensionTick:
   ↓ se ModifiesMovementSpeed && !IsSuspendedByProne
target' = (Multiplier/100) × MaxSpeed atual
   ↓ |target' − _lastAppliedSpeedLimit| > 0.001?
   ↓ sim → Remove + Add(target') ; _lastAppliedSpeedLimit = target'
   ↓ não → no-op (cache hit)
```

### Suspensão por prone

```
Jogador entra/sai de prone
   ↓
EvaluateProneSuspensionTick detecta mudança
StanceStaminaState.IsSuspendedByProne = newValue
AccumulatedDrain = 0
   ↓ se suspenso
mc.RemoveStateSpeedLimit; _lastAppliedSpeedLimit = -1
   ↓ se sair de suspensão
re-aplica speed limit no mesmo tick (passa pelo cache check)
```

### Ciclo de vida da raid

```
Jogador entra em raid
   ↓
GameWorld.OnGameStarted (postfix) → StanceManager.OnRaidStart()
   ↓
_raidEnded = false; _activeStaminaStance = Default; StanceStaminaState.Reset();
_lastAppliedSpeedLimit = -1; _staminaConfigDirty = true; ApplyStaminaStance(Default)

  [ ... raid ... ]

Jogador extrai/morre/MIA/menu
   ↓
GameWorld.OnDestroy (postfix) E/OU BaseLocalGame.Stop (postfix) — qualquer um primeiro
   ↓
StanceManager.OnRaidEnd() (idempotente — _raidEnded)
   ↓
mc.RemoveStateSpeedLimit; StanceStaminaState.Reset(); ResetState() (existente);
_activeStaminaStance = Default; _lastAppliedSpeedLimit = -1
```

## 7. Riscos e dependências

- **Patches existentes em `modded/Patches/`:** `PlayerSpringPatch`, `SpringGetPatch`, `FOVSliderPatch`, `FOVClampPatch`. Nenhum toca em `PlayerPhysicalClass`, `MovementContext`, `GameWorld` ou `BaseLocalGame` — sem conflito direto.
- **Compatibilidade com mods de stamina** (ex.: `SPT-BetterArmStamina`): postfixes empilham. `[HarmonyPriority(Priority.Low)]` faz nosso rodar por último.
- **Compatibilidade com mods de velocidade:** `(ESpeedLimit)9001` reservado pelo mod — anunciar no README.
- **`Singleton<>` namespace:** importar **`Comfort.Common.Singleton`** — não `RootMotion.Singleton`.
- **`__instance.Player_0`:** se o nome ofuscado mudar, atualizar.
- **`AddStateSpeedLimit` ignora cause já registrada:** sempre Remove antes de re-Add.
- **`hands.Multiplier`:** se outro mod ou debug seta `Multiplier ≤ 0`, nosso tick respeita.
- **Ordem de inicialização dos patches:** registrados em `Plugin.Awake` antes da raid começar — patches Harmony são globais.
- **Reflection sobre `GClass774.action_3`/`action_1`:** se BSG renomear esses campos privados em uma atualização do EFT, `AccessTools.Field` retorna null e a HUD para de receber sinal de drain (mas drain continua funcional — só silencioso visualmente). Logar warning no Awake se algum field-info for null.
- **`OnGameStarted` resolution:** confirmar via `Plugin.Logger.LogInfo` no Awake se o `MethodInfo` retornado é não-null para os 4 patches. Falha silenciosa de `AccessTools.Method` é difícil de debugar.

## 8. Checklist de implementação

- [ ] **Verificar `<LangVersion>` em [`modded/CameraRotationMod.csproj`](../../modded/CameraRotationMod.csproj)** — recomendado ≥ 9. Stubs usam apenas `!(... is X)`, então funcionam em qualquer versão; bumpar é melhoria de qualidade.
- [ ] **Confirmar `Plugin.Logger` permaneceu `public static new ManualLogSource Logger;`** — sem o `new`, shadow estático quebra.
- [x] Criar `modded/StanceStaminaState.cs` com enum `EStanceStaminaMode`, classe estática + `Reset()`.
- [x] Criar `modded/StanceConfig.cs`.
- [x] Em `modded/Plugin.cs`: declarar `StanceSpeedLimitCauseId = 9001`, `_stanceConfigs` (`Dictionary<Stance, StanceConfig>`), array de defaults usando enum `Stance`; bindar 20 `ConfigEntry`; registrar `OnStanceConfigChanged` chamando `MarkStaminaConfigDirty`; registrar 4 patches.
- [x] Em `modded/StanceManager.cs`: **modificar property `CurrentStance` (linha 22)** para detectar mudança e chamar `OnStanceChanged`; adicionar `_activeStaminaStance` (`Stance`), `_staminaConfigDirty`, `_lastAppliedSpeedLimit`, `_cachedAimDrainRate`; cachear backing fields de eventos via `ResolveBackingFieldByCandidates` (`_onValueChangedBacking`, `_onThresholdPassBacking`); adicionar `HasMissingReflection(out missing)`; adicionar helpers `NotifyHandsStaminaChanged`, `OnRaidStart` (caching de `AimDrainRate`)/`OnRaidEnd` (chama `ResetState()`)/`ApplyStaminaStance(Stance)`/`OnStanceChanged(Stance, Stance)`/`TickStanceStamina`/`EvaluateProneSuspensionTick`/`IsActiveContext`/`MarkStaminaConfigDirty`. **Não alterar** os 3 sítios internos onde `CurrentStance = X` (linhas 94/111/116) — o `private set` continua funcionando.
- [x] Hook em `Plugin.Update()`: adicionar `StanceManager.TickStanceStamina()` e `StanceManager.EvaluateProneSuspensionTick()`.
- [x] Criar `modded/Patches/StanceStaminaRecoveryPatch.cs`.
- [x] Criar `modded/Patches/RaidLifecyclePatches.cs` — **`BaseLocalGameStopPatch` resolve com tipos explícitos** `new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) }`.
- [x] Logar no `Awake` se algum `MethodInfo` resolver para null (defesa contra regressão silenciosa) — **incluindo** o check `StanceManager.HasMissingReflection` que valida os backing fields de eventos do `GClass774`.
- [x] Atualizar [PROPRIEDADES.md](../../PROPRIEDADES.md): nova seção `Stance 0` + 5 entradas em cada `Stance N`.
- [x] Atualizar [README.md](../../README.md): seção da feature; nota de incompatibilidade com mods de stamina/velocidade; reserva de `(Player.ESpeedLimit)9001` documentada.
- [ ] Testar in-game os 18 ACs e 19 corner cases da spec funcional. **(Pendente — fase de QA pós-build)**

## Histórico

| Data | Evento |
|---|---|
| 2026-05-07 | Spec técnica criada com base no Assembly descompilado |
| 2026-05-07 | Adicionada Stance 0; modos Drain/Recovery via enum; postfix em `GetHandsRestorationFunc`; toggle Apply When Prone |
| 2026-05-07 | Sincronizada para o template canônico |
| 2026-05-07 | Tabela de defaults por stance adicionada |
| 2026-05-07 | Revisão `/review-technical-spec` 01 — 5 🔴 / 7 🟡 / 4 🟢 |
| 2026-05-07 | Aplicadas as 16 correções da review-01 |
| 2026-05-07 | Revisão `/review-technical-spec` 02 — 3 🔴 / 3 🟡 / 2 🟢 |
| 2026-05-07 | Aplicadas as 8 correções da review-02 |
| 2026-05-08 | Revisão `/review-technical-spec` 03 — 3 🔴 / 3 🟡 / 2 🟢 (alinhamento com estrutura real do `StanceManager.cs`) |
| 2026-05-08 | **Aplicadas as 8 correções da review-03:** modelo passa a usar `enum Stance` em vez de `int 0..3` — PA-03-01; property `CurrentStance` existente é modificada (não criada) — PA-03-02; `BaseLocalGame.Stop` resolvido com tipos explícitos — PA-03-03; `OnRaidEnd` chama `ResetState()` existente — PA-03-04; **PA-03-05 revertido para Opção B** após feedback do usuário (HUD precisa atualizar suavemente): drain agora muta `Current` por frame e dispara `action_3`/`InvokeChangedAction`/`action_1` via reflection cacheada; padrão dirty-flag (`MarkStaminaConfigDirty`) — PA-03-06; double-negação removida — PA-03-07; cache `_lastAppliedSpeedLimit` — PA-03-08. |
| 2026-05-08 | Revisão `/review-technical-spec` 04 — 0 🔴 / 3 🟡 / 3 🟢. Spec liberada para `/build-item`. |
| 2026-05-08 | **Aplicadas as 6 correções da review-04:** reflection nos backing fields de events agora resolve via `ResolveBackingFieldByCandidates` com nome público primeiro (`OnValueChanged`/`OnThresholdPass`) — PA-04-01; `_cachedAimDrainRate` populado em `OnRaidStart` evita `Singleton` lookup todo frame — PA-04-02; `HasMissingReflection(out missing)` chamado no `Plugin.Awake` com warning explícito — PA-04-03; `HandleExpiration()` (público) é chamado quando drain hits 0 para disparar `OnExpired` — PA-04-04; tolerância `< 0.0001f` em vez de `< float.Epsilon` — PA-04-05; `using System.Reflection;` declarado nos imports do `StanceManager` — PA-04-06. |
| 2026-05-08 | Revisão `/review-technical-spec` 05 — 0 🔴 / 1 🟡 / 3 🟢. |
| 2026-05-08 | **Aplicadas as 4 correções da review-05 (última rodada antes do build):** `_raidEnded = true` por default — defende contra OnRaidEnd antes de qualquer OnRaidStart (PA-05-01); guard `if (hands.ForceMode) return;` no `TickStanceStamina` honra contrato vanilla de `Consume()` (PA-05-02); parágrafo explicativo "Cycle e Stance 0" em §1 esclarecendo que voltar ao Default re-arma drain por design (PA-05-03); XMLDoc adicionada ao helper `ResolveBackingFieldByCandidates` documentando estratégia de candidatos (PA-05-04). **Spec liberada para `/build-item`.** |
| 2026-05-08 | **`/build-item` executado.** Implementação aplicada em `mods/stancesAndCameraPositionSPT4.0.11/modded/`: 2 arquivos novos (`StanceStaminaState.cs`, `StanceConfig.cs`); 2 patches novos (`Patches/StanceStaminaRecoveryPatch.cs`, `Patches/RaidLifecyclePatches.cs` com 3 patches); `Plugin.cs` modificado (constantes, dicionário, defaults, helper `BindStance`, handler `OnStanceConfigChanged`, registro de 4 patches, validação de reflection, hooks no `Update()`); `StanceManager.cs` modificado (property setter de `CurrentStance` dispara `OnStanceChanged`; adicionados `OnRaidStart`/`OnRaidEnd`/`ApplyStaminaStance`/`OnStanceChanged`/`TickStanceStamina`/`EvaluateProneSuspensionTick` + helpers de reflection); `PROPRIEDADES.md` atualizado (83 props, +20 novas); `README.md` atualizado com seção "Mudanças nossas". Status do `mod-backlog.md` → 🟢 Entregue. **Pendente:** testes in-game (18 ACs + 19 corner cases) + compilação real do .dll. |
