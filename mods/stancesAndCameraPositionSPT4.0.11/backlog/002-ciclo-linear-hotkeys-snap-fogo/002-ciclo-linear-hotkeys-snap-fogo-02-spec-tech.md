# 002 — Ciclo linear, hotkeys e snap fogo · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [002-ciclo-linear-hotkeys-snap-fogo-01-spec.md](002-ciclo-linear-hotkeys-snap-fogo-01-spec.md)
**Criado:** 2026-05-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

A maior parte das 5 features se resolve dentro do próprio `StanceManager` e `Plugin` — só **Feature 4 (snap fogo)** demanda Harmony patch real, porque depende de interceptar o pipeline de gatilho do EFT. As outras features ficam assim:

| Feature | Estratégia | Tipo |
|---|---|---|
| F1 — Substituir `Use Only Stances` por `Include Stance 0 in Cycle` | Renomear/inverter ConfigEntry; ajustar `IsStanceEnabled` em [StanceManager.cs:213](../../modded/StanceManager.cs#L213) | Pure config + lógica do mod |
| F2 — Modo `Linear` no scroll | Novo enum `ScrollMode`; refatorar branch do scroll em [StanceManager.cs:113-137](../../modded/StanceManager.cs#L113); usar `Browsable` mutável + `SettingChanged` para reflexo em tempo real no F12 | Pure mod, sem patch |
| F3 — Hotkeys dedicadas | 4 novos `ConfigEntry<KeyCode>` + bloco em `StanceManager.Update()` ao lado do `_stanceToggleKeyConfig` | Pure mod |
| F4 — Snap to Stance 0 on Fire | **Prefix em `<OperationBase>.SetTriggerPressed(bool pressed)`** da nested operation-base de `Player.FirearmController` (resolvido por reflection). Bloqueia o trigger no button-down quando snap é elegível e ressuscita via delegate sintético no button-up se elapsed ≥ threshold. | Harmony Prefix |
| F5 — Iniciar em Stance 3 ao começar raid | Estender `GameWorldOnGameStartedPatch` (já existente em [RaidLifecyclePatches.cs:13](../../modded/Patches/RaidLifecyclePatches.cs#L13)) para chamar novo `StanceManager.ApplyInitialStanceImmediate(Stance.Stance3)`, com retry em `Update()` enquanto `pwa.HandsContainer` for null | Reuso do patch existente |

**Justificativa do F4 — patch único de intercept.** O fire pipeline do EFT dispara o tiro a partir de `IsTriggerPressed = true`, propagado para `FirearmsAnimator`. Para semi-auto, o tiro acontece quase imediatamente após button-down — não há janela natural para esperar 200ms antes de decidir. A solução é **bloquear o trigger no button-down** (Prefix retorna `false` ⇒ operation nunca recebe `pressed=true` ⇒ tiro não sai), e **ressuscitar o trigger no button-up** se elapsed ≥ threshold (Invoke do `MethodInfo` original). Esta é a única estratégia que honra a regra "clique único < threshold = nenhum tiro" da spec funcional. Alternativas descartadas: (a) Postfix duplo em SetTriggerPressed + Prefix em fire-event — quebra para semi-auto (tiro sai antes do button-up); (b) transpiler em `FirearmsAnimator.SetFire` — frágil contra updates do EFT.

**Nota — som de stance change no snap.** Quando o snap muda `CurrentStance` para `Default`, o `OnStanceChanged` dispara, o `SpringGetPatch` detecta a mudança e toca `PlayAimingSound` via [`PlayStanceChangeSound`](../../modded/Patches/SpringGetPatch.cs#L466). O som **toca uma vez por snap** — comportamento intencional. AC explícita em §8.8.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/GameWorld.cs:2584` — `GameWorld.OnGameStarted()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584) | Postfix (já existente) | Hook de início de raid; F5 estende para aplicar Stance 3 imediato. |
| [`EFT/GameWorld.cs:2111` — `GameWorld.OnDestroy()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111) | Postfix (já existente) | Limpa estado de snap, timer e stance inicial pendente. |
| **Nested operation-base de `Player.FirearmController`** — método `SetTriggerPressed(bool)` declarado | **Prefix** | F4: bloqueia o trigger no button-down quando snap é elegível (return false), e no button-up dispara o trigger sintético se elapsed ≥ threshold. Esta é a única patch de F4 — não há mais um Prefix separado para fire-event. |

### 2.1. Resolução por reflection (F4)

Verificação no Assembly em [`Player.cs:2441`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2441) (`public class FirearmController : ItemHandsController, …`) e arredores mostra que:

- A `Player.FirearmController` em si **não declara** `SetTriggerPressed(bool)` no escopo direto da classe — herda da base ou delega para `CurrentOperation`.
- O método `public virtual void SetTriggerPressed(bool pressed) { method_0(); }` em [`Player.cs:3810`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3810) está numa **nested operation-base class** dentro de FirearmController (corpo é stub `method_0()`; bloco circundante 3700–3819 contém outros stubs como `OnFireEvent`, `OnMagAppeared`, e `BlindFire_Internal` que usa `FirearmController_0` como back-reference em [`Player.cs:3719`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3719)).
- O caller-tree em [`Player.cs:4558`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L4558) (`FirearmController_0.CurrentOperation.SetTriggerPressed(pressed: true);`) confirma que o trigger é roteado: `FirearmController.SetTriggerPressed → CurrentOperation.SetTriggerPressed`. Patchando a operation-base, captamos *toda* operação ativa via dispatch virtual.

**Estratégia de resolução** (executada em `Plugin.Awake`, antes de `PatchAll`):

```csharp
// modded/Plugin.cs — método auxiliar chamado no Awake

private static MethodInfo _operationSetTriggerPressed;
private static MethodBase OperationSetTriggerPressedTarget => _operationSetTriggerPressed;
private static Type _operationBaseType;
public static Type OperationBaseType => _operationBaseType;
public static MethodBase OperationOriginalSetTrigger => _operationSetTriggerPressed;

private static void ResolveFirearmOperationBase()
{
    var fc = typeof(Player.FirearmController);
    var nested = fc.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
    const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    // PA-02-02: critério primário — classe ABSTRATA com SetTriggerPressed(bool) virtual declarado
    // e backing field FirearmController_0. Existem 12+ overrides concretos no Assembly; só a
    // base abstrata satisfaz IsAbstract == true.
    Type best = null;
    foreach (var t in nested)
    {
        if (!t.IsAbstract) continue;
        if (t.GetField("FirearmController_0", FieldFlags) == null) continue;
        var m = AccessTools.DeclaredMethod(t, "SetTriggerPressed", new[] { typeof(bool) });
        if (m == null || !m.IsVirtual) continue;
        if (best != null)
        {
            Logger.LogError($"[F4] Multiple abstract operation-bases found: " +
                            $"{best.FullName} and {t.FullName}. Aborting F4 (ambiguous).");
            return;
        }
        best = t;
    }

    // Fallback (PA-02-02): se a base não foi marcada IsAbstract pelo decompilador,
    // subir via GetBaseDefinition() até a topmost classe nested que declara o método.
    if (best == null)
    {
        foreach (var t in nested)
        {
            var m = AccessTools.DeclaredMethod(t, "SetTriggerPressed", new[] { typeof(bool) });
            if (m == null || !m.IsVirtual || m.IsFinal) continue;
            var declType = m.GetBaseDefinition().DeclaringType;
            if (declType == null || !declType.IsNested || declType.DeclaringType != fc) continue;
            if (declType.GetField("FirearmController_0", FieldFlags) == null) continue;
            best = declType;
            Logger.LogInfo("[F4] Operation-base resolvida via GetBaseDefinition fallback.");
            break;
        }
    }

    if (best == null)
    {
        Logger.LogWarning("[F4] Failed to resolve FirearmController operation-base — " +
                          "snap-on-fire (F4) desabilitado neste boot.");
        return;
    }

    _operationBaseType = best;
    _operationSetTriggerPressed = AccessTools.DeclaredMethod(best, "SetTriggerPressed",
                                                              new[] { typeof(bool) });
    Logger.LogInfo($"[F4] Operation-base = {best.FullName}, " +
                   $"SetTriggerPressed = {_operationSetTriggerPressed.DeclaringType?.FullName}." +
                   $"{_operationSetTriggerPressed.Name}");
}
```

Com isso, `SnapFireTriggerPatch.GetTargetMethod()` retorna `Plugin.OperationOriginalSetTrigger`. Se for `null`, o patch não registra (degradação graciosa) e o restante das features continuam funcionando.

> ⚠️ **Nunca hardcodar nome `GClassNNNN` ou linha**. A resolução por reflection é a única estratégia que sobrevive a updates do EFT.

### 2.2. Refs de leitura (não-patch) usadas

| Símbolo | Local | Uso |
|---|---|---|
| `Player.HandsController` | [`Player.cs` — propriedade pública virtual](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) (já em uso) | Verificar `is Player.FirearmController` para guard de F4. |
| `Player.IsSprintEnabled` | [`Player.cs` — arrow property](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) (já em uso) | Bloqueia hotkeys F3 durante sprint (alinhado com `V`). |
| `ProceduralWeaponAnimation.IsAiming` | (já em uso em [SpringGetPatch.cs:167](../../modded/Patches/SpringGetPatch.cs#L167)) | Detecta ADS para F3 (ignora hotkey) e F4 (bloqueia snap). |
| `ProceduralWeaponAnimation.HandsContainer.HandsRotation` / `.HandsPosition` | (já em uso em [SpringGetPatch.cs:156](../../modded/Patches/SpringGetPatch.cs#L156)) | F5: setar `Current` diretamente para "set imediato" sem animação. |
| `HideoutPlayer` | [`EFT/HideoutPlayer.cs:15`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/HideoutPlayer.cs#L15) | Guard `MainPlayer is HideoutPlayer` (mesmo padrão de [StanceManager.cs:718](../../modded/StanceManager.cs#L718)). |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Settings` | `Include Stance 0 - Vanilla in Cycle` | bool | `false` | — | — | Quando habilitado, inclui a Postura 0 - Vanilla no ciclo. Substitui a antiga propriedade `Use Only Stances`. Afeta sempre a tecla V; afeta o scroll só em modo Cycle. |
| `Settings` | `Mouse Wheel Scroll Mode` | enum (`Cycle`/`Linear`) | `Linear` | — | — | Define o comportamento da roda do mouse. Cycle = circular, respeita os toggles de stance. Linear = eixo fixo: Stance 1 (topo) ↔ Stance 0 (centro) ↔ Stance 2 (fundo); Stance 3 fica fora do eixo. Visível apenas com `Enable Mouse Wheel Stance Cycle`. |
| `Settings` | `Stance 0 - Vanilla Hotkey` | KeyCode | `None` | — | — | Tecla dedicada para retornar à Postura 0 - Vanilla. Bloqueada durante sprint e ignorada em ADS. |
| `Settings` | `Stance 1 - High Ready Hotkey` | KeyCode | `None` | — | — | Tecla dedicada para ativar Stance 1 - High Ready. Toggle: pressionar quando já ativa retorna à Stance 0. Bloqueada durante sprint e ignorada em ADS. |
| `Settings` | `Stance 2 - Custom Hotkey` | KeyCode | `None` | — | — | Tecla dedicada para ativar Stance 2 - Custom. Toggle: pressionar quando já ativa retorna à Stance 0. Bloqueada durante sprint e ignorada em ADS. |
| `Settings` | `Stance 3 - Low Ready Hotkey` | KeyCode | `O` | — | — | Tecla dedicada para ativar Stance 3 - Low Ready. Toggle: pressionar quando já ativa retorna à Stance 0. Bloqueada durante sprint e ignorada em ADS. |
| `Settings` | `Snap Fire Threshold` | int (ms) | `200` | 50 – 500 | ✓ | Tempo máximo (ms) entre apertar e soltar o gatilho para classificar como clique único. Clique único = snap para Stance 0 sem disparo. Pressão maior = snap + disparo natural. |
| `Settings` | `Start In Low Ready On Raid Begin` | bool | `true` | — | — | Quando habilitado, o jogador inicia toda raid já em Stance 3 - Low Ready, sem animação de transição. Aplica mesmo se Stance 3 estiver fora do ciclo. |
| `Stance 1 - High Ready` | `Stance 1 Snap to Stance 0 on Fire` | bool | `true` | — | — | Quando habilitado, atirar enquanto em Stance 1 faz snap automático para Stance 0. Não atua em ADS nem com arma branca/granada. |
| `Stance 2 - Custom` | `Stance 2 Snap to Stance 0 on Fire` | bool | `true` | — | — | Quando habilitado, atirar enquanto em Stance 2 faz snap automático para Stance 0. Não atua em ADS nem com arma branca/granada. |
| `Stance 3 - Low Ready` | `Stance 3 Snap to Stance 0 on Fire` | bool | `false` | — | — | Quando habilitado, atirar enquanto em Stance 3 faz snap automático para Stance 0. Não atua em ADS nem com arma branca/granada. |

### Removida

| Seção | Nome | Motivo |
|---|---|---|
| `Settings` | `Use Only Stances` | Substituída por `Include Stance 0 - Vanilla in Cycle` (lógica invertida e nome mais claro). |

> ⚠️ Migração de seções: as seções de stance no F12 mudam de `Stance 1 - Ready Up` / `Stance 2 - Ready Down` / `Stance 3 - Custom` para `Stance 1 - High Ready` / `Stance 2 - Custom` / `Stance 3 - Low Ready`. BepInEx casa entradas por `(section, key)` — todas as ConfigEntries com seção antiga serão recriadas com defaults. Documentar no changelog do mod e fornecer instrução manual de migração do `.cfg` antigo.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| [`modded/Plugin.cs`](../../modded/Plugin.cs) | MODIFICAR | Renomear `Stance1Section`/`Stance2Section`/`Stance3Section`; remover `_UseOnlyStances`; bind das 11 novas ConfigEntries (Settings + 3 nas Stance sections); adicionar `enum ScrollMode { Cycle, Linear }`; `SettingChanged` em `_MouseWheelScrollMode` para mutar `Browsable` das deps; `Snap to Stance 0 on Fire` por stance no helper `BuildStanceConfig`. |
| [`modded/StanceConfig.cs`](../../modded/StanceConfig.cs) | MODIFICAR | Adicionar `public ConfigEntry<bool> SnapToStance0OnFire;`. |
| [`modded/StanceManager.cs`](../../modded/StanceManager.cs) | MODIFICAR | Refatorar `IsStanceEnabled` para usar `_IncludeStance0InCycle`; novo bloco `Update()` para hotkeys e modo Linear; novo `ApplyInitialStanceImmediate(Stance)` com retry; tracking do snap (button-down timestamp, snap-pending flag). |
| [`modded/Patches/RaidLifecyclePatches.cs`](../../modded/Patches/RaidLifecyclePatches.cs) | MODIFICAR | Postfix de `OnGameStarted` chama `StanceManager.QueueInitialStance(Stance.Stance3)` quando `_StartInLowReadyOnRaidBegin = true`. |
| `modded/Patches/SnapFireTriggerPatch.cs` | CRIAR | **Prefix** em `<OperationBase>.SetTriggerPressed(bool)` (resolvido por reflection) — intercept-and-resurrect. Único patch de F4. |

## 5. Stubs de código

> Blocos compiláveis. Cada referência ao EFT comentada com `// ref: Assembly-CSharp/<arquivo>:<linha>`.

### 5.1. `StanceConfig` — adicionar campo

```csharp
// modded/StanceConfig.cs
using BepInEx.Configuration;

namespace CameraRotationMod
{
    public sealed class StanceConfig
    {
        public ConfigEntry<float> StaminaMultiplier;
        public ConfigEntry<bool> ModifiesMovementSpeed;
        public ConfigEntry<int> MovementSpeedMultiplier;
        public ConfigEntry<bool> ApplyWhenProne;
        // F4: snap automático para Stance 0 ao atirar.
        public ConfigEntry<bool> SnapToStance0OnFire;
    }
}
```

### 5.2. `Plugin.cs` — enum + novos binds (extrato)

```csharp
// modded/Plugin.cs (trechos relevantes)

public enum ScrollMode
{
    Cycle,
    Linear,
}

public static ConfigEntry<bool> _IncludeStance0InCycle;          // F1
public static ConfigEntry<ScrollMode> _MouseWheelScrollMode;     // F2
public static ConfigEntry<KeyCode> _Stance0Hotkey;               // F3
public static ConfigEntry<KeyCode> _Stance1Hotkey;
public static ConfigEntry<KeyCode> _Stance2Hotkey;
public static ConfigEntry<KeyCode> _Stance3Hotkey;
public static ConfigEntry<int>  _SnapFireThreshold;              // F4
public static ConfigEntry<bool> _StartInLowReadyOnRaidBegin;     // F5

// Atributos mantidos por referência para mutar Browsable em tempo real (F2)
private static ConfigurationManagerAttributes _attrIncludeStance0;
private static ConfigurationManagerAttributes _attrEnableStance1Cycle;
private static ConfigurationManagerAttributes _attrEnableStance2Cycle;
private static ConfigurationManagerAttributes _attrEnableStance3Cycle;

// Em Awake() — após binds existentes da seção Settings, antes de Stance sections:

_IncludeStance0InCycle = Config.Bind(
    Settings,
    "Include Stance 0 - Vanilla in Cycle",
    false,
    new ConfigDescription(
        "When enabled, Stance 0 (Vanilla) is included in the cycle. " +
        "Replaces the old `Use Only Stances` toggle. Always affects V key; " +
        "affects scroll only in Cycle mode.",
        null,
        _attrIncludeStance0 = new ConfigurationManagerAttributes { Order = 65 }));

// PA-04-01: Order 58 (slot livre deixado por `_UseOnlyStances` removido).
// Não usar Order 59 — colide com `_MouseWheelModifierKey` existente (Plugin.cs:244).
_MouseWheelScrollMode = Config.Bind(
    Settings,
    "Mouse Wheel Scroll Mode",
    ScrollMode.Linear,
    new ConfigDescription(
        "Cycle = circular, respects per-stance cycle toggles. " +
        "Linear = fixed axis: Stance 1 (top) ↔ Stance 0 (center) ↔ Stance 2 (bottom); " +
        "Stance 3 is off-axis.",
        null,
        new ConfigurationManagerAttributes { Order = 58 }));

// PA-03-04: método nomeado (não lambda) para permitir unsubscribe no OnDestroy.
_MouseWheelScrollMode.SettingChanged   += OnScrollModeSettingChanged;
_EnableMouseWheelCycle.SettingChanged  += OnScrollModeSettingChanged;

// Hotkeys (Settings, Order 53..50)
_Stance0Hotkey = Config.Bind(Settings, "Stance 0 - Vanilla Hotkey",   KeyCode.None,
    new ConfigDescription("Toggle dedicated key for Stance 0 - Vanilla.", null,
        new ConfigurationManagerAttributes { Order = 53 }));
_Stance1Hotkey = Config.Bind(Settings, "Stance 1 - High Ready Hotkey", KeyCode.None,
    new ConfigDescription("Toggle dedicated key for Stance 1 - High Ready.", null,
        new ConfigurationManagerAttributes { Order = 52 }));
_Stance2Hotkey = Config.Bind(Settings, "Stance 2 - Custom Hotkey",     KeyCode.None,
    new ConfigDescription("Toggle dedicated key for Stance 2 - Custom.", null,
        new ConfigurationManagerAttributes { Order = 51 }));
_Stance3Hotkey = Config.Bind(Settings, "Stance 3 - Low Ready Hotkey",  KeyCode.O,
    new ConfigDescription("Toggle dedicated key for Stance 3 - Low Ready.", null,
        new ConfigurationManagerAttributes { Order = 50 }));

_SnapFireThreshold = Config.Bind(Settings, "Snap Fire Threshold (ms)", 200,
    new ConfigDescription(
        "Max press-to-release time (ms) classified as a single click. " +
        "Single click = snap to Stance 0 without firing. " +
        "Held longer = snap + natural fire.",
        new AcceptableValueRange<int>(50, 500),
        new ConfigurationManagerAttributes { IsAdvanced = true, Order = 49 }));

_StartInLowReadyOnRaidBegin = Config.Bind(Settings, "Start In Low Ready On Raid Begin", true,
    new ConfigDescription(
        "When enabled, every raid starts already in Stance 3 - Low Ready " +
        "without transition animation. Applies even if Stance 3 is excluded from the cycle.",
        null,
        new ConfigurationManagerAttributes { Order = 48 }));

// Inicializar visibilidade na primeira chamada
RefreshScrollModeVisibility();

// PA-01-03: cachear MethodInfo + instância no Awake — evita FindObjectOfType por SettingChanged.
private static MethodInfo _cmBuildSettingListMethod;
private static UnityEngine.Object _cmInstance;
private static bool _cmRefreshAvailable;

private static void TryResolveConfigurationManager()
{
    var tCM = AccessTools.TypeByName("ConfigurationManager.ConfigurationManager");
    if (tCM == null)
    {
        Logger.LogInfo("[F2] ConfigurationManager não detectado — visibilidade dinâmica desabilitada.");
        return;
    }
    _cmBuildSettingListMethod = AccessTools.Method(tCM, "BuildSettingList");
    _cmInstance = UnityEngine.Object.FindObjectOfType(tCM);
    if (_cmBuildSettingListMethod == null || _cmInstance == null)
    {
        Logger.LogWarning(
            "[F2] ConfigurationManager presente mas API BuildSettingList não resolvida — " +
            "visibilidade só atualiza ao reabrir F12.");
        return;
    }
    _cmRefreshAvailable = true;
}

// PA-03-04: handler nomeado (estático) para subscribe/unsubscribe.
private static void OnScrollModeSettingChanged(object sender, EventArgs args)
    => RefreshScrollModeVisibility();

// PA-03-04: chamado no OnDestroy do Plugin para evitar leak em hot-reload.
internal static void UnsubscribeScrollModeHandlers()
{
    if (_MouseWheelScrollMode != null)
        _MouseWheelScrollMode.SettingChanged   -= OnScrollModeSettingChanged;
    if (_EnableMouseWheelCycle != null)
        _EnableMouseWheelCycle.SettingChanged  -= OnScrollModeSettingChanged;
}

private void OnDestroy()
{
    UnsubscribeScrollModeHandlers();
}

private static void RefreshScrollModeVisibility()
{
    bool wheelEnabled = _EnableMouseWheelCycle?.Value ?? false;
    bool isCycle = wheelEnabled && _MouseWheelScrollMode?.Value == ScrollMode.Cycle;

    // Em modo Linear (ou wheel desabilitado), os toggles de ciclo ficam ocultos.
    _attrIncludeStance0.Browsable      = isCycle;
    _attrEnableStance1Cycle.Browsable  = isCycle;
    _attrEnableStance2Cycle.Browsable  = isCycle;
    _attrEnableStance3Cycle.Browsable  = isCycle;

    // Forçar redesenho do CM em tempo real — só se cache disponível (degradação graciosa).
    if (!_cmRefreshAvailable) return;
    try { _cmBuildSettingListMethod.Invoke(_cmInstance, null); }
    catch (Exception ex) { Logger.LogError($"[F2] BuildSettingList falhou: {ex}"); }
}
```

### 5.3. `StanceManager.cs` — extensões (extrato)

```csharp
// modded/StanceManager.cs (trechos novos)

// === F4: snap state — modelo simplificado (PA-01-06) ===
// _triggerDownTimeUnscaled = -1f → idle (nenhum intercept ativo).
// >= 0 → button-down interceptado em t=value, aguardando button-up.
private const float SnapIdleSentinel = -1f;
private const float SnapStaleTimeoutSec = 2f;          // PA-01-05: stale guard contra weapon swap
private static float _triggerDownTimeUnscaled = SnapIdleSentinel;
private static bool  _snapInterceptActive;             // único bool de estado (substitui _snapPendingFromTrigger)

// === F4: deferred resurrection (PA-02-03) + 2-frame pulse (PA-03-01) ===
// Em vez de ressuscitar o trigger inline no Prefix de button-up (mesma stack frame, animator
// pode pular o tiro), agendamos para o próximo frame via Update. Mais: encadeamos um synthetic
// false no frame seguinte ao true para parar fullauto após ~1 tiro (sem isso, runaway até mag empty).
private static object     _pendingResurrectInstance;   // operation instance (frame N+1: synthetic true)
private static MethodBase _pendingResurrectMethod;
private static object     _pendingResetInstance;       // operation instance (frame N+2: synthetic false)
private static MethodBase _pendingResetMethod;

// PA-03-02: getter cacheado de Player.FirearmController.CurrentOperation para validar
// staleness do operation entre frames. Resolvido no Awake.
public static System.Reflection.MethodInfo CurrentOperationGetter;

// === F5: pending initial stance ===
private static Stance? _pendingInitialStance;

public static void QueueInitialStance(Stance s) => _pendingInitialStance = s;

private static void TryApplyPendingInitialStance()
{
    if (_pendingInitialStance == null) return;
    var gw = GetCachedGameWorld();
    if (gw?.MainPlayer?.ProceduralWeaponAnimation?.HandsContainer == null) return;
    // PA-04-04: F5 só em raid. Hideout não deve receber stance inicial (mesmo padrão de
    // IsActiveContext em StanceManager.cs:718). Defesa em profundidade — OnGameStarted
    // normalmente não dispara em hideout, mas guard barato.
    if (gw.MainPlayer is HideoutPlayer) return;

    // ref: Assembly-CSharp/EFT/Player.cs — ProceduralWeaponAnimation.HandsContainer
    // Definir CurrentStance via setter dispara OnStanceChanged → ApplyStaminaStance.
    var target = _pendingInitialStance.Value;
    _pendingInitialStance = null;

    // Set imediato — bypass spring: definir CurrentStance e mover SpringGetPatch
    // a inicializar com _isInitialized=false → próxima Postfix de Spring.Get assume target sem lerp.
    // PA-04-04: ResetState pode interromper transição em vôo, mas só relevante em hot-reload de dev
    // (raid start normal não tem transição em vôo). Aceitável.
    SpringGetPatch.ResetState();
    CurrentStance = target;
}

// === F1+F2: ciclo refatorado ===
// IsStanceEnabled passa a usar _IncludeStance0InCycle (lógica invertida)
private static bool IsStanceEnabled(Stance stance)
{
    return stance switch
    {
        Stance.Default => Plugin._IncludeStance0InCycle?.Value ?? false,
        Stance.Stance1 => Plugin._EnableStance1?.Value ?? true,
        Stance.Stance2 => Plugin._EnableStance2?.Value ?? true,
        Stance.Stance3 => Plugin._EnableStance3?.Value ?? true,
        _ => false,
    };
}

// === F2: scroll Linear ===
private static void HandleLinearScroll(float scrollDelta)
{
    // Eixo fixo: Stance 1 (topo) → Stance 0 (centro) → Stance 2 (fundo).
    // Stance 3 fora do eixo: scroll-up vai para Stance 1, scroll-down para Stance 2.
    Stance next = CurrentStance;
    if (scrollDelta > 0)
    {
        next = CurrentStance switch
        {
            Stance.Stance2 => Stance.Default,
            Stance.Default => Stance.Stance1,
            Stance.Stance1 => Stance.Stance1,        // já no topo, no-op
            Stance.Stance3 => Stance.Stance1,        // off-axis → topo
            _ => CurrentStance,
        };
    }
    else if (scrollDelta < 0)
    {
        next = CurrentStance switch
        {
            Stance.Stance1 => Stance.Default,
            Stance.Default => Stance.Stance2,
            Stance.Stance2 => Stance.Stance2,        // já no fundo, no-op
            Stance.Stance3 => Stance.Stance2,        // off-axis → fundo
            _ => CurrentStance,
        };
    }
    if (next != CurrentStance) CurrentStance = next;
}

// === F3: hotkeys (PA-01-04: prioridade do menor índice via early-return) ===
// PA-04-02: retorna bool — caller usa para early-return antes de processar tecla V (priority hotkey > V).
private static bool HandleStanceHotkeys()
{
    var gw = GetCachedGameWorld();
    if (gw?.MainPlayer == null) return false;
    if (gw.MainPlayer.IsSprintEnabled) return false;                                // bloqueio sprint
    if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return false;    // ignora em ADS

    // Ordem crescente de stance — primeira que matcha vence (Stance0 > Stance1 > Stance2 > Stance3
    // em prioridade quando duas teclas coincidirem).
    if (TryHotkey(Plugin._Stance0Hotkey, Stance.Default)) return true;
    if (TryHotkey(Plugin._Stance1Hotkey, Stance.Stance1)) return true;
    if (TryHotkey(Plugin._Stance2Hotkey, Stance.Stance2)) return true;
    if (TryHotkey(Plugin._Stance3Hotkey, Stance.Stance3)) return true;
    return false;
}

private static bool TryHotkey(ConfigEntry<KeyCode> entry, Stance target)
{
    var key = entry?.Value ?? KeyCode.None;
    if (key == KeyCode.None) return false;
    if (!UnityEngine.Input.GetKeyDown(key)) return false;

    // Toggle: pressionar a tecla da stance ativa retorna a Default — exceto a própria Default.
    if (CurrentStance == target)
    {
        if (target != Stance.Default) CurrentStance = Stance.Default;
        return true;
    }
    CurrentStance = target;
    return true;
}

// === F4: snap helpers — estratégia intercept-and-resurrect (PA-01-02) ===

/// <summary>
/// Chamado pelo Prefix de SetTriggerPressed quando pressed==true.
/// Retorna true se deve BLOQUEAR o original (skip) — quando snap é elegível.
/// Retorna false se deve deixar o trigger seguir normalmente.
/// </summary>
public static bool TryInterceptTriggerDown(object firearmControllerInstance)
{
    var gw = GetCachedGameWorld();
    if (gw?.MainPlayer == null) return false;
    if (gw.MainPlayer.ProceduralWeaponAnimation?.IsAiming == true) return false; // sem snap em ADS
    if (CurrentStance == Stance.Default) return false;                            // sem snap em Stance 0
    if (!Plugin._stanceConfigs.TryGetValue(CurrentStance, out var cfg)) return false;
    if (!cfg.SnapToStance0OnFire.Value) return false;
    if (!IsHoldingFirearm()) return false;                                        // só FirearmController

    // Snap imediato — comportamento desejado em todos os caminhos.
    CurrentStance = Stance.Default;

    // Marcar intercept ativo: timer começa, próximo button-up decide se ressuscita.
    _triggerDownTimeUnscaled = Time.unscaledTime;
    _snapInterceptActive = true;
    return true;   // Prefix retorna false → operation NÃO recebe trigger=true → tiro NÃO sai.
}

/// <summary>
/// Chamado pelo Prefix de SetTriggerPressed quando pressed==false (button-up).
/// Se houve intercept ativo e elapsed >= threshold, agenda ressurreição para o próximo frame
/// (PA-02-03: defer 1 frame para dar tempo do animator processar o button-up natural).
/// </summary>
public static void OnTriggerUpAfterIntercept(object operationInstance, MethodBase originalMethod)
{
    if (!_snapInterceptActive) return;
    _snapInterceptActive = false;

    float elapsedMs = (Time.unscaledTime - _triggerDownTimeUnscaled) * 1000f;
    _triggerDownTimeUnscaled = SnapIdleSentinel;

    int threshold = Plugin._SnapFireThreshold?.Value ?? 200;
    if (elapsedMs < threshold) return; // clique único — nada a fazer (snap já aconteceu no down)

    // Hold ≥ threshold → registrar intenção de ressurreição. O Update do próximo frame
    // chamará TryDispatchPendingResurrect → _operationSetTrigger.Invoke(operation, true).
    // Gap de ~16ms a 60fps dá ao animator tempo de processar o button-up=false natural antes
    // do trigger sintético=true.
    _pendingResurrectInstance = operationInstance;
    _pendingResurrectMethod   = originalMethod;
}

/// <summary>
/// Chamado no início de StanceManager.Update() — despacha a ressurreição agendada.
/// PA-03-01 (2-frame pulse): primeiro despacha o reset do frame anterior (synthetic false),
/// depois despacha a ressurreição (synthetic true) e agenda o reset do próximo frame.
/// Sem isso, fullauto fica em runaway até esvaziar mag.
///
/// PA-03-02 (operation staleness): valida fc.CurrentOperation == _pendingResurrectInstance
/// antes de cada Invoke. Cobre weapon swap / reload / hotkey de stance no gap de 1 frame.
///
/// O Prefix do SnapFireTriggerPatch usa _inSyntheticCall (ThreadStatic) como reentry guard
/// — sem isso, Invoke do método patcheado dispararia o próprio Prefix (recursão infinita).
/// </summary>
private static void TryDispatchPendingResurrect()
{
    // 1. Frame N+2: despachar synthetic false (reset agendado no frame anterior).
    if (_pendingResetMethod != null)
    {
        var resetInst = _pendingResetInstance;
        var resetMethod = _pendingResetMethod;
        _pendingResetInstance = null;
        _pendingResetMethod = null;
        if (!IsOperationStillCurrent(resetInst))
        {
            // Weapon swap / operation change — drop silencioso (PA-03-02).
        }
        else if (IsTriggerPressedNaturally())
        {
            // PA-04-03: jogador re-apertou o gatilho dentro de 1 frame (double-tap rápido).
            // Synthetic false agora interromperia o fire continuous em fullauto. Skip.
            Plugin.Logger.LogDebug("[F4] reset skipped: trigger pressed by natural input");
        }
        else
        {
            SnapFireTriggerPatch.RaiseSyntheticTrigger(resetInst, resetMethod, pressed: false);
        }
    }

    // 2. Frame N+1: despachar synthetic true (resurrect) e agendar reset do frame seguinte.
    if (_pendingResurrectMethod != null)
    {
        var inst = _pendingResurrectInstance;
        var method = _pendingResurrectMethod;
        _pendingResurrectInstance = null;
        _pendingResurrectMethod = null;

        if (!IsOperationStillCurrent(inst))
        {
            // PA-03-02: operation mudou entre frames — drop sem ressuscitar nem agendar reset.
            Plugin.Logger.LogDebug("[F4] resurrect skipped: CurrentOperation mudou entre frames");
            return;
        }
        SnapFireTriggerPatch.RaiseSyntheticTrigger(inst, method, pressed: true);
        _pendingResetInstance = inst;
        _pendingResetMethod = method;
    }
}

/// <summary>
/// PA-04-03: detecta se o gatilho está pressionado por input natural (double-tap rápido).
/// Usado para skipar o synthetic false do reset — sem isso, fullauto stutter 1 frame.
/// ref: Player.cs:2714 — FirearmController.IsTriggerPressed.
/// </summary>
private static bool IsTriggerPressedNaturally()
{
    var gw = GetCachedGameWorld();
    var fc = gw?.MainPlayer?.HandsController as Player.FirearmController;
    return fc?.IsTriggerPressed == true;
}

/// <summary>
/// PA-03-02: confirma que `operationInstance` ainda é o `CurrentOperation` do FirearmController
/// ativo no MainPlayer. Cobre weapon swap, reload start e hotkey de stance no gap de frame.
/// </summary>
private static bool IsOperationStillCurrent(object operationInstance)
{
    if (operationInstance == null) return false;
    var gw = GetCachedGameWorld();
    var fc = gw?.MainPlayer?.HandsController as Player.FirearmController;
    if (fc == null) return false;
    if (CurrentOperationGetter == null) return false;
    object current;
    try { current = CurrentOperationGetter.Invoke(fc, null); }
    catch { return false; }
    return current == operationInstance;
}

/// <summary>
/// PA-01-05: stale guard. Chamado todo frame em Update() — limpa flags se intercept ficou
/// pendurado por > SnapStaleTimeoutSec (provável weapon swap durante hold).
/// </summary>
private static void EvaluateSnapStaleTimeout()
{
    if (!_snapInterceptActive) return;
    if (Time.unscaledTime - _triggerDownTimeUnscaled <= SnapStaleTimeoutSec) return;
    _snapInterceptActive = false;
    _triggerDownTimeUnscaled = SnapIdleSentinel;
    Plugin.Logger.LogDebug("[F4] snap intercept timed out (likely weapon swap during hold).");
}

// === Update() — entrada única chamada por Plugin.Update ===
public static void Update()
{
    // F5: aplica stance inicial pendente — primeiro frame em que HandsContainer existe
    TryApplyPendingInitialStance();

    // F4 PA-02-03: dispatch de ressurreição agendada no frame anterior (1-frame defer)
    TryDispatchPendingResurrect();

    // F4 PA-01-05: stale guard contra weapon swap durante hold
    EvaluateSnapStaleTimeout();

    var gameWorld = GetCachedGameWorld();
    if (gameWorld?.MainPlayer?.IsSprintEnabled == true)
        return;

    // PA-04-02: hotkeys F3 PRIMEIRO — se uma matcha, return early (evita double-fire de
    // OnStanceChanged quando hotkey == tecla V; spec funcional linha 185 pede prioridade da hotkey).
    if (HandleStanceHotkeys()) return;

    // V key (existente) — só roda se nenhuma hotkey matchou.
    if (UnityEngine.Input.GetKeyDown(_stanceToggleKeyConfig.Value))
        CurrentStance = GetNextStance(CurrentStance);

    // Scroll
    if (_enableMouseWheelCycleConfig?.Value == true &&
        UnityEngine.Input.GetKey(_mouseWheelModifierKeyConfig.Value))
    {
        float scrollDelta = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
        if (scrollDelta != 0 && Time.time - _lastScrollTime > ScrollCooldown)
        {
            switch (Plugin._MouseWheelScrollMode?.Value ?? ScrollMode.Linear)
            {
                case ScrollMode.Cycle:
                    CurrentStance = scrollDelta > 0 ? GetNextStance(CurrentStance)
                                                    : GetPreviousStance(CurrentStance);
                    break;
                case ScrollMode.Linear:
                    HandleLinearScroll(scrollDelta);
                    break;
            }
            _lastScrollTime = Time.time;
        }
    }
}

// ResetState — adicionar limpeza dos novos campos (F4 + F5)
public static void ResetState()
{
    // ... limpeza existente ...
    _triggerDownTimeUnscaled = SnapIdleSentinel;
    _snapInterceptActive = false;
    _pendingResurrectInstance = null;          // PA-02-03
    _pendingResurrectMethod = null;
    _pendingResetInstance = null;              // PA-03-01
    _pendingResetMethod = null;
    _pendingInitialStance = null;
}
```

### 5.4. Patch — `SnapFireTriggerPatch` (intercept-and-resurrect)

```csharp
// modded/Patches/SnapFireTriggerPatch.cs
using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Prefix em &lt;OperationBase&gt;.SetTriggerPressed(bool) da nested operation-base dentro de
    /// Player.FirearmController. Resolução por reflection — ver Plugin.ResolveFirearmOperationBase.
    ///
    /// Estratégia:
    /// - pressed==true em stance snap-elegível → bloqueia o trigger (return false), snap imediato.
    /// - pressed==false: se intercept estava ativo e elapsed ≥ threshold, registra ressurreição
    ///   pendente; o despacho síncrono acontece no Update do PRÓXIMO frame (PA-02-03), via
    ///   StanceManager.TryDispatchPendingResurrect → SnapFireTriggerPatch.RaiseSyntheticTriggerDown.
    /// - O bypass de reentry usa _inSyntheticCall [ThreadStatic] (PA-02-01) — sem isso, o Invoke
    ///   do método patcheado dispararia o próprio Prefix (recursão infinita).
    /// </summary>
    public class SnapFireTriggerPatch : ModulePatch
    {
        // PA-02-01: reentry guard. Harmony patcheia o método; chamar MethodBase.Invoke nele
        // passa pelo wrapper → Prefix dispara de novo. [ThreadStatic] cobre callers em qualquer
        // thread (input pode chegar em thread diferente).
        [ThreadStatic] private static bool _inSyntheticCall;

        // PA-02-04 + PA-03-01: cachear args evita allocation; _falseArgs usado no reset (frame N+2).
        private static readonly object[] _trueArgs  = new object[] { true };
        private static readonly object[] _falseArgs = new object[] { false };

        private static MethodBase _originalSetTrigger;

        protected override MethodBase GetTargetMethod()
        {
            // PA-02-05: GetTargetMethod NÃO deve ser chamado se OperationOriginalSetTrigger
            // for null — Plugin.Awake garante isso checando antes de Enable(). Defesa em
            // profundidade: throw em vez de retornar null silencioso (Harmony.Patch lançaria
            // ArgumentNullException de qualquer forma — esta mensagem é mais clara).
            _originalSetTrigger = Plugin.OperationOriginalSetTrigger
                ?? throw new InvalidOperationException(
                    "SnapFireTriggerPatch não deveria ser registrado quando " +
                    "Plugin.OperationOriginalSetTrigger é null. Awake deve checar antes de Enable().");
            return _originalSetTrigger;
        }

        /// <summary>
        /// Despacha trigger sintético (true ou false) fora da pilha do Prefix.
        /// PA-03-01: pulse de 2 frames — true em N+1, false em N+2 — para parar fullauto após ~1 tiro.
        /// Chamado por StanceManager.TryDispatchPendingResurrect via Update.
        /// </summary>
        public static void RaiseSyntheticTrigger(object operationInstance, MethodBase original, bool pressed)
        {
            if (operationInstance == null || original == null) return;
            _inSyntheticCall = true;
            try { original.Invoke(operationInstance, pressed ? _trueArgs : _falseArgs); }
            catch (Exception ex) { Plugin.Logger.LogError($"[F4] synthetic trigger {pressed} failed: {ex}"); }
            finally { _inSyntheticCall = false; }
        }

        [PatchPrefix]
        private static bool Prefix(object __instance, bool pressed)
        {
            // PA-02-01: bypass durante a ressurreição — deixa o trigger sintético passar.
            if (_inSyntheticCall) return true;

            try
            {
                // Filtro: só firearm. O __instance é a operation; checar FirearmController_0.
                var fcRef = __instance.GetType().GetField("FirearmController_0",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var fc = fcRef?.GetValue(__instance) as Player.FirearmController;
                if (fc == null) return true; // não é firearm — deixa passar

                if (pressed)
                {
                    if (StanceManager.TryInterceptTriggerDown(fc))
                        return false;     // skip original: tiro NÃO sai
                    return true;           // caminho normal — sem snap
                }
                else
                {
                    // PA-02-03: agenda ressurreição para o próximo frame (sem closure / inline).
                    StanceManager.OnTriggerUpAfterIntercept(__instance, _originalSetTrigger);
                    return true;           // button-up natural sempre propaga (operation precisa resetar)
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[SnapFireTriggerPatch] {ex}");
                return true; // defensivo: em erro, vanilla segue
            }
        }
    }
}
```

> **Nota — recursão e dispatch virtual.** A ressurreição via `Invoke` no `MethodBase` original chama
> o método **declarado** (operation-base), mas como ele é virtual, o dispatch desce para a override
> da operação concreta ativa — equivalente a chamar `operation.SetTriggerPressed(true)` direto.
> O `[ThreadStatic] _inSyntheticCall` garante que o próprio Prefix não re-intercepte essa chamada.

### 5.5. `RaidLifecyclePatches.cs` — extensão

```csharp
// modded/Patches/RaidLifecyclePatches.cs (modificar)

[PatchPostfix]
private static void Postfix()
{
    try
    {
        StanceManager.OnRaidStart();

        // F5 — Iniciar em Stance 3 quando habilitado
        if (Plugin._StartInLowReadyOnRaidBegin?.Value == true)
            StanceManager.QueueInitialStance(Stance.Stance3);
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[GameWorldOnGameStartedPatch] {ex}"); }
}
```

### 5.7. `Plugin.cs` — extensão do helper `BuildStanceConfig` (PA-03-05)

Estende o tuple existente em [Plugin.cs:28](../../modded/Plugin.cs#L28) com o novo campo `SnapOnFire` (defaults divergem por stance):

```csharp
// modded/Plugin.cs (modificar tuple existente)
private static readonly (Stance Stance, string Section, float StaminaMultiplier,
                         bool ModSpeed, int Multiplier, bool ApplyProne, bool SnapOnFire)[]
    _stanceDefaults =
{
    (Stance.Default, Stance0Section, 0.5f,  true,  90,  false, false),  // Stance 0: irrelevante (no-op)
    (Stance.Stance1, Stance1Section, 1.5f,  true,  95,  false, true),
    (Stance.Stance2, Stance2Section, 2.0f,  true,  100, false, true),
    (Stance.Stance3, Stance3Section, 1.0f,  true,  90,  false, false),  // Low Ready: default off
};

// modded/Plugin.cs:861 — adicionar bind dentro do `return new StanceConfig {...}` do helper.
// Stance 0 não recebe ConfigEntry — sentinel null. O guard
// `if (CurrentStance == Stance.Default) return false;` em TryInterceptTriggerDown (§5.3)
// já cobre o caso e a F12 não exibe a entry para a Stance 0 - Vanilla.
SnapToStance0OnFire = (d.Stance == Stance.Default)
    ? null
    : Config.Bind(d.Section, $"Stance {n} Snap to Stance 0 on Fire", d.SnapOnFire,
        new ConfigDescription(
            "When enabled, firing while in this stance snaps to Stance 0 - Vanilla. " +
            "Single click (< Snap Fire Threshold) = no shot. Hold (≥ threshold) = snap + 1 shot. " +
            "Does not trigger in ADS or with non-firearm items.",
            null,
            new ConfigurationManagerAttributes { Order = 0 })),
```

E em `StanceManager.TryInterceptTriggerDown` (§5.3), a guard de Stance 0 + null-check do ConfigEntry:

```csharp
if (CurrentStance == Stance.Default) return false;  // sem snap em Stance 0
if (!Plugin._stanceConfigs.TryGetValue(CurrentStance, out var cfg)) return false;
if (cfg.SnapToStance0OnFire == null) return false;   // sentinela: Stance 0 (defesa em profundidade)
if (!cfg.SnapToStance0OnFire.Value) return false;
```

## 6. Fluxo de dados

### F4 — snap fire (caminho crítico) — intercept-and-resurrect

```
[1] Jogador pressiona botão de fogo
        ↓
[2] EFT chama FirearmController.SetTriggerPressed(true)
        → roteia para CurrentOperation.SetTriggerPressed(true)   // ref: Player.cs:4558
        ↓ Prefix de SnapFireTriggerPatch (patcheia operation-base, dispatch virtual)
[3] StanceManager.TryInterceptTriggerDown(fc)
        ├─ guard: ADS? Stance 0? não-firearm? snap-toggle off? → retorna false (não intercepta)
        │   → Prefix retorna true → operation segue normal → tiro vanilla
        └─ guard ok: snap imediato (CurrentStance = Stance.Default), set _snapInterceptActive=true,
                     _triggerDownTimeUnscaled = Time.unscaledTime
                     → retorna true → Prefix retorna FALSE → operation NÃO recebe trigger=true
                     → tiro NÃO sai (operation nem sabe que houve fogo)

[4] Jogador solta o botão (em qualquer momento)
        ↓
[5] EFT chama FirearmController.SetTriggerPressed(false) → CurrentOperation.SetTriggerPressed(false)
        ↓ Prefix de SnapFireTriggerPatch
[6] StanceManager.OnTriggerUpAfterIntercept(__instance, _originalSetTrigger)
        ├─ se !_snapInterceptActive: no-op (não houve intercept — clique normal)
        └─ se _snapInterceptActive:
              elapsedMs = (unscaledTime − _triggerDownTimeUnscaled) * 1000
              ├─ elapsedMs < threshold (clique único) → no-op (já bloqueamos no down)
              └─ elapsedMs ≥ threshold (hold) → REGISTRA ressurreição pendente:
                    _pendingResurrectInstance = __instance
                    _pendingResurrectMethod   = _operationSetTriggerPressed
        Prefix retorna true → button-up natural propaga → operation.SetTriggerPressed(false)
        executado normal → operation reseta trigger no animator.

[7] FRAME N+1 — Plugin.Update → StanceManager.Update → TryDispatchPendingResurrect
        ├─ Validar IsOperationStillCurrent(_pendingResurrectInstance)  // PA-03-02
        │   └─ Se operation mudou (weapon swap/reload/hotkey): drop, sem ressuscitar nem agendar reset.
        ├─ SnapFireTriggerPatch.RaiseSyntheticTrigger(operation, method, pressed: true)
        │   ├─ _inSyntheticCall = true   // bypass do Prefix (reentry guard, PA-02-01)
        │   ├─ method.Invoke(operation, _trueArgs)   // dispatch virtual desce à operação concreta
        │   │       ↓
        │   │   operation.SetTriggerPressed(true) executa → animator entra em fire state
        │   └─ _inSyntheticCall = false (em finally)
        └─ Agenda reset do frame seguinte: _pendingResetInstance/Method = inst/method.

[8] FRAME N+2 — TryDispatchPendingResurrect (mesmo Update da próxima iteração)
        ├─ Validar IsOperationStillCurrent(_pendingResetInstance)  // PA-03-02
        │   └─ Se mudou: drop silencioso. Sem o reset, o problema do auto-runaway só ocorreria
        │       se o jogador voltar pra mesma arma e operation — improvável e auto-stale-timeout cobre.
        └─ RaiseSyntheticTrigger(operation, method, pressed: false)
            └─ operation.SetTriggerPressed(false) → animator para fire cycle. ✓ Para fullauto, ~1 tiro.
              Para semi/burst, o fire-end-event já tinha resetado — este false é redundante mas inofensivo.
```

> **Trade-off documentado:** durante snap, o tiro acontece no **release + 1 frame** (≈16ms a 60fps).
> Defer explícito (PA-02-03) garante que o animator processou o button-up natural ([6]) antes do
> trigger sintético ([7]) — sem essa janela, semi-auto pode pular o tiro porque press+release
> aconteceriam na mesma frame sem oportunidade do `OnFireEvent` rodar. Para auto/burst, o trigger
> sintético dispara o ciclo normal da arma.
>
> **2-frame pulse (PA-03-01):** o synthetic false em [8] é essencial para fullauto. Sem ele,
> `IsTriggerPressed = true` permaneceria setado (o button-up natural já passou em [5], antes da
> nossa Invoke), e o animator continuaria ciclando até a magazine esvaziar. O false em N+2 simula
> o release que o usuário já fez fisicamente. Para semi/burst, o fire-end-event interno já reseta
> o trigger — o false em N+2 é redundante mas inofensivo (idempotente).
>
> **Operation staleness (PA-03-02):** entre os frames N+1 e N+2, o jogador pode trocar de arma,
> iniciar reload, ou pressionar hotkey de stance. Cada despacho valida `fc.CurrentOperation ==
> _pending*Instance`; se não bate, drop silencioso e o estado da nova arma fica intacto.
>
> **Reentry guard (PA-02-01):** o `[ThreadStatic] _inSyntheticCall` no Prefix é essencial — sem
> ele, `method.Invoke` no [7] re-entraria pelo wrapper Harmony do mesmo Prefix → recursão infinita.
>
> **Stale guard (PA-01-05):** se o jogador trocar de arma enquanto segura (button-up nunca chega),
> `EvaluateSnapStaleTimeout` no Update limpa o estado após 2s. Sem leak entre armas.
>
> **Som de stance change (PA-01-07):** `CurrentStance = Stance.Default` no [3] dispara
> `OnStanceChanged` → `SpringGetPatch` detecta a mudança e toca `PlayAimingSound` via
> `PlayStanceChangeSound` ([SpringGetPatch.cs:466](../../modded/Patches/SpringGetPatch.cs#L466)).
> O som **toca** uma vez por snap — comportamento intencional.

### F5 — start in Low Ready

```
[1] GameWorld.OnGameStarted()                                    // ref: GameWorld.cs:2584
        ↓ Postfix existente
[2] StanceManager.OnRaidStart() (já existente)
[3] Se Plugin._StartInLowReadyOnRaidBegin == true:
        StanceManager.QueueInitialStance(Stance.Stance3)
        ↓
[4] Plugin.Update() chama StanceManager.Update() — todo frame
        ↓
[5] StanceManager.TryApplyPendingInitialStance()
        ├─ Se ProceduralWeaponAnimation.HandsContainer == null → retornar (retry no próximo frame)
        └─ Quando disponível: SpringGetPatch.ResetState() + CurrentStance = Stance3
              → SpringGetPatch.PatchPostfix vai inicializar com _isInitialized=false
                e o primeiro frame já assume target sem lerp.
```

## 7. Riscos e dependências

### 7.1. Dependência soft do ConfigurationManager (F2)

A reflexão de `Browsable` em tempo real (PA-01-03) requer o mod **ConfigurationManager** instalado e expondo o método `BuildSettingList`. Cenários:

- **CM presente + API resolvida** → visibilidade atualiza imediatamente ao trocar `Mouse Wheel Scroll Mode` no F12 (atende AC da spec funcional).
- **CM presente + API renomeada/ausente** → log warning no Awake; `Browsable` ainda muda no atributo, mas o painel só repinta ao fechar/reabrir F12. Não bloqueia features.
- **CM ausente** → log info no Awake; mod opera normalmente, F12 do CM nem existe (o usuário não tem como ver as entries de qualquer forma).

A spec funcional (AC F2 linha 147) menciona "sem necessidade de fechar e reabrir o painel" — esse comportamento é **best-effort**, condicionado à presença do CM. Documentar no README do mod.

### 7.2. Patches existentes em `modded/Patches/` que podem conflitar

- **[SpringGetPatch.cs](../../modded/Patches/SpringGetPatch.cs)** — Postfix em `Spring.Get()`. F5 depende dele. Após `CurrentStance = Stance3` num set imediato, o fluxo natural do SpringGetPatch atualiza `_targetRotation`/`_targetPosition`. Para garantir "sem lerp", `ResetState()` é chamado antes; isso força o branch `!_isInitialized` que assume `_currentRotation = desiredRotation` no primeiro frame.
- **[StanceStaminaRecoveryPatch.cs](../../modded/Patches/StanceStaminaRecoveryPatch.cs)** — sem conflito direto (independente de stance changes).
- **[PlayerSpringPatch.cs](../../modded/Patches/PlayerSpringPatch.cs)** — câmera; F5 toca apenas mãos.

### 7.3. Compatibilidade com outros mods

- **`hazelify.StanceSync.dll`** (já instalado pelo usuário) — sincroniza shoulder swap com lean. Não interfere; opera sobre `LeftStanceEnabled`, não sobre o sistema de stances do mod.
- **SPT-Realism-Mod-Client** — tem seu próprio sistema de stances (High Ready, Low Ready, Active Aim) operando sobre o `ProceduralWeaponAnimation` por outro caminho. Risco: se ambos estiverem ativos, F5 pode disputar o estado inicial. **Mitigação:** documentar no README que os dois sistemas são mutuamente exclusivos; usuário escolhe um.
- **Mods que patcheiam `SetTriggerPressed`** — possível conflito de ordem. Mitigação: Postfix puro (não-mutativo); qualquer ordem produz o mesmo resultado.

### 7.4. Ordem de inicialização

Sequência exata (PA-02-05 — não confiar em "Harmony pula silenciosamente"):

1. `TryResolveConfigurationManager()` (PA-01-03) — cachear `MethodInfo BuildSettingList` + instância.
2. Bind de todas as `ConfigEntry` da seção Settings + Stance sections, capturando os `ConfigurationManagerAttributes` em `_attrIncludeStance0`, `_attrEnableStance1Cycle/2/3`.
3. Subscribe `_MouseWheelScrollMode.SettingChanged` e `_EnableMouseWheelCycle.SettingChanged` → `RefreshScrollModeVisibility`.
4. `ResolveFirearmOperationBase()` (§2.1) — popula `_operationSetTriggerPressed` (pode ser null se a base não foi achada).
5. **Registro condicional dos patches Harmony:**
   - Sempre: `new GameWorldOnGameStartedPatch().Enable()`, `new GameWorldOnDestroyPatch().Enable()`, e os patches existentes do mod (Spring, FOV, etc.).
   - **Só se `Plugin.OperationOriginalSetTrigger != null`:** `new SnapFireTriggerPatch().Enable()`. Se for null, log warning e segue. **Não confiar** em `ModulePatch.Enable()` pular silenciosamente — `Harmony.Patch` lança `ArgumentNullException` se o `MethodBase` for null.
6. `RefreshScrollModeVisibility()` — visibilidade inicial.

Detecção falha de qualquer dependency (CM ausente, operation-base não resolvida) é loggada como warning/info no Awake, sem crash. Mod degrada graciosamente: F2 perde o redraw em tempo real (mas `Browsable` ainda muda), F4 fica off, F1/F3/F5 continuam funcionando.

## 8. Checklist de implementação

### 8.1. Reorganização de seções/configs (preparação)

- [ ] Em `Plugin.cs`, renomear constantes `Stance1Section`/`Stance2Section`/`Stance3Section` para os novos nomes (`Stance 1 - High Ready` / `Stance 2 - Custom` / `Stance 3 - Low Ready`).
- [ ] Adicionar `enum ScrollMode { Cycle, Linear }` em `Plugin.cs` (top-level no namespace).
- [ ] Adicionar `public ConfigEntry<bool> SnapToStance0OnFire;` em `StanceConfig.cs`.

### 8.2. F1 — Include Stance 0 in Cycle

- [ ] Bind de `_IncludeStance0InCycle` (default `false`, Order 65) na seção Settings.
- [ ] Remover bind de `_UseOnlyStances` e o campo correspondente.
- [ ] Em `StanceManager.IsStanceEnabled`, trocar `useOnlyStances` por leitura direta de `Plugin._IncludeStance0InCycle.Value` (lógica invertida; ver stub §5.3).
- [ ] Atualizar [PROPRIEDADES.md](../../PROPRIEDADES.md) — remover linha de `Use Only Stances`, adicionar `Include Stance 0 - Vanilla in Cycle`.

### 8.3. F2 — Mouse Wheel Scroll Mode (PA-01-03 + PA-01-08)

- [ ] **Modificar binds existentes** de `_EnableStance1/2/3` em [Plugin.cs:198-220](../../modded/Plugin.cs#L198) — extrair o `ConfigurationManagerAttributes { Order = NN }` para variável local **antes** de passar ao `ConfigDescription`, e armazenar nos campos privados `_attrEnableStance1Cycle/2/3` de `Plugin`.
- [ ] Bind do **novo** `_IncludeStance0InCycle` capturando o atributo em `_attrIncludeStance0`.
- [ ] Bind de `_MouseWheelScrollMode` (enum, default `Linear`, Order 59).
- [ ] Implementar `TryResolveConfigurationManager()` (cachear `MethodInfo BuildSettingList` e instância) e chamá-lo no início do `Awake()`.
- [ ] Implementar `RefreshScrollModeVisibility()` mutando os 4 atributos `Browsable` + invocando `BuildSettingList` se cache disponível (degradação graciosa se CM ausente).
- [ ] Subscribe `_MouseWheelScrollMode.SettingChanged` e `_EnableMouseWheelCycle.SettingChanged` via **método nomeado estático** `OnScrollModeSettingChanged` (PA-03-04: não usar lambda; precisa permitir unsubscribe).
- [ ] Implementar `Plugin.OnDestroy()` chamando `UnsubscribeScrollModeHandlers()` (PA-03-04: evita leak em hot-reload).
- [ ] Chamar `RefreshScrollModeVisibility()` uma vez ao final do `Awake()` para inicializar visibilidade.
- [ ] Em `StanceManager.Update()`, branch `Linear` chama `HandleLinearScroll(scrollDelta)`; branch `Cycle` mantém `GetNextStance`/`GetPreviousStance`.

### 8.4. F3 — Hotkeys dedicadas (PA-01-04)

- [ ] Bind dos 4 `_StanceNHotkey` (default `None`/`None`/`None`/`O`, Order 53/52/51/50).
- [ ] Implementar `HandleStanceHotkeys()` em `StanceManager` com guards de sprint/ADS (ver stub §5.3).
- [ ] **`TryHotkey` e `HandleStanceHotkeys` retornam `bool`** (PA-04-02); `HandleStanceHotkeys` faz `if (TryHotkey(...)) return true;` em ordem Stance0→1→2→3 — primeira hotkey que matcha vence (prioridade do menor índice).
- [ ] Em `StanceManager.Update()`, chamar `if (HandleStanceHotkeys()) return;` **antes** do bloco da tecla `V` — garante prioridade de hotkey sobre tecla de ciclo quando coincidem (PA-04-02).

### 8.5. F4 — Snap to Stance 0 on Fire (PA-01-01 + PA-01-02 + PA-01-05 + PA-01-06)

- [ ] Bind de `_SnapFireThreshold` (int ms, default 200, faixa 50–500, Advanced, Order 49).
- [ ] **Estender o tuple `_stanceDefaults`** (Plugin.cs:28) com o campo `SnapOnFire` (`false/true/true/false` para Default/1/2/3) — ver stub §5.7.
- [ ] No helper `BuildStanceConfig` (Plugin.cs:854), adicionar bind de `Stance N Snap to Stance 0 on Fire` com **null sentinela para Stance 0** (sem ConfigEntry); defaults `true/true/false` para 1/2/3, Order 0 (final da seção). PA-03-05.
- [ ] Em `TryInterceptTriggerDown`, adicionar guard `cfg.SnapToStance0OnFire == null` (defesa em profundidade contra Stance 0). PA-03-05.
- [ ] Adicionar campos de estado simplificados em `StanceManager`: `_triggerDownTimeUnscaled` (sentinel `-1f`) e `_snapInterceptActive` (bool). Sem `_snapPendingFromTrigger`/`_abortNextFireEvent` redundantes (PA-01-06).
- [ ] Implementar `TryInterceptTriggerDown(object)` (retorna bool) e `OnTriggerUpAfterIntercept(object, MethodBase)` em `StanceManager` (ver stub §5.3) — apenas registra ressurreição pendente, sem closure (PA-02-04).
- [ ] Implementar `TryDispatchPendingResurrect()` e chamá-lo no início de `StanceManager.Update()` — despacha **2-frame pulse** (PA-03-01): synthetic true em N+1 (resurrect), synthetic false em N+2 (reset) para parar fullauto após ~1 tiro.
- [ ] Adicionar campos `_pendingResetInstance/Method` em `StanceManager` (PA-03-01: agendamento do synthetic false).
- [ ] Implementar `IsOperationStillCurrent(object)` em `StanceManager` (PA-03-02) usando `Plugin.CurrentOperationGetter` (cache resolvido no Awake via `AccessTools.PropertyGetter(typeof(Player.FirearmController), "CurrentOperation")`). `TryDispatchPendingResurrect` invoca o guard antes de cada Invoke (true e false) — drop silencioso se operation mudou.
- [ ] Implementar `IsTriggerPressedNaturally()` em `StanceManager` (PA-04-03) — lê `(MainPlayer.HandsController as FirearmController)?.IsTriggerPressed`. Usado no bloco de reset (frame N+2) para skipar synthetic false quando double-tap está em curso, evitando 1 frame de fire stutter em fullauto.
- [ ] Generalizar `SnapFireTriggerPatch.RaiseSyntheticTrigger(object, MethodBase, bool)` com `_trueArgs`/`_falseArgs` cacheados (PA-03-01 + PA-02-04).
- [ ] Implementar `EvaluateSnapStaleTimeout()` e chamá-lo no início de `StanceManager.Update()` — limpa flags se intercept ficou pendurado > 2s (PA-01-05: weapon swap durante hold).
- [ ] Implementar `Plugin.ResolveFirearmOperationBase()` no `Awake()` (PA-02-02) — filtrar `IsAbstract` + back-ref `FirearmController_0` + `SetTriggerPressed(bool)` virtual; fallback via `GetBaseDefinition()`. Detectar ambiguidade (>1 abstract) e abortar F4. Cachear em `Plugin.OperationOriginalSetTrigger`.
- [ ] Resolver `Plugin.CurrentOperationGetter` no Awake via `AccessTools.PropertyGetter(typeof(Player.FirearmController), "CurrentOperation")` — usado por `IsOperationStillCurrent` em frame N+1/N+2 (PA-03-02). Se for null, F4 ainda funciona mas sem o guard de stale (degradação aceitável).
- [ ] Criar `Patches/SnapFireTriggerPatch.cs` (ver stub §5.4) — Prefix único intercept-and-resurrect; `[ThreadStatic] _inSyntheticCall` como reentry guard (PA-02-01); `RaiseSyntheticTriggerDown(object, MethodBase)` público para chamada do Update; `_trueArgs` cacheado (PA-02-04).
- [ ] **Plugin.Awake — Enable condicional (PA-02-05):** registrar `new SnapFireTriggerPatch().Enable()` **apenas se `Plugin.OperationOriginalSetTrigger != null`**. Caso contrário, log warning e seguir. **Não** confiar em `Enable()` pular silenciosamente — `Harmony.Patch` crasharia com NRE.
- [ ] Limpar campos de F4 em `StanceManager.ResetState()`: sentinel + bool + `_pendingResurrectInstance/Method`.

### 8.6. F5 — Start In Low Ready On Raid Begin

- [ ] Bind de `_StartInLowReadyOnRaidBegin` (default `true`, Order 48).
- [ ] Adicionar `QueueInitialStance(Stance)` e `TryApplyPendingInitialStance()` em `StanceManager` (ver stub §5.3).
- [ ] Modificar `GameWorldOnGameStartedPatch.Postfix` em [RaidLifecyclePatches.cs:13](../../modded/Patches/RaidLifecyclePatches.cs#L13) para chamar `QueueInitialStance(Stance3)` quando o toggle for `true` (ver stub §5.6).
- [ ] Chamar `TryApplyPendingInitialStance()` no início de `StanceManager.Update()`.
- [ ] Limpar `_pendingInitialStance` em `ResetState()`.

### 8.7. Documentação

- [ ] Atualizar [PROPRIEDADES.md](../../PROPRIEDADES.md): remover `Use Only Stances`; adicionar 11 novas entries; renomear seções de Stance 1/2/3.
- [ ] Atualizar [CHANGELOG_SIMPLIFIED.md](../../modded/CHANGELOG_SIMPLIFIED.md) com nota de breaking change (renomeação de seções) e instrução de migração manual do `.cfg`.
- [ ] Atualizar status do item 002 em [mod-backlog.md](../mod-backlog.md) para 🟢 quando concluído.

### 8.8. Verificação manual (após `/compile-mod`)

- [ ] F1: `Use Only Stances` ausente; `Include Stance 0 - Vanilla in Cycle` presente; V cicla com/sem Stance 0 conforme toggle.
- [ ] F2: alternar `Mouse Wheel Scroll Mode` no F12 oculta/mostra os 4 toggles em tempo real (sem reabrir painel); Linear mode respeita topo/fundo; Stance 3 é off-axis.
- [ ] F3: cada hotkey ativa a stance correta; toggle (mesma tecla 2x) volta a Stance 0 (exceto Stance 0 Hotkey); sprint bloqueia; ADS ignora.
- [ ] F4: clique único em Stance 1/2 → snap sem tiro; segurar → snap + tiro **no release + 1 frame** (latência ≈ tempo de hold + ~16ms a 60fps); em ADS sem snap; arma branca/granada não dispara snap.
- [ ] F4 — som: cada snap toca o som de stance change exatamente uma vez (não duplicado nem mudo). PA-01-07.
- [ ] F4 — weapon swap durante hold: pressionar fogo em Stance 1, trocar de arma sem soltar, então atirar com a nova arma → primeiro tiro da nova arma sai normal (sem residual abort). PA-01-05.
- [ ] F4 — duplicate hotkey: configurar Stance1Hotkey == Stance3Hotkey == `O`; pressionar `O` ativa Stance 1 (menor índice prioriza). PA-01-04.
- [ ] F4 — recursão: hold ≥ threshold em Stance 1/2 não causa stack overflow (reentry guard `[ThreadStatic]`). PA-02-01.
- [ ] F4 — operation-base ambígua: log de erro se mais de uma classe abstrata aninhada satisfaz o filtro; F4 aborta sem crashar o mod. PA-02-02.
- [ ] F4 — Awake condicional: `Plugin.OperationOriginalSetTrigger == null` simulado → mod carrega normalmente, F4 reportado como "off" no log, F1/F2/F3/F5 funcionam. PA-02-05.
- [ ] F5+F4 simultâneos no spawn: aceitável que dois sons de stance-change toquem em sequência rápida (não é bug; comportamento documentado). PA-02-06.
- [ ] F4 — fullauto sem runaway: testar com AKM/M4 em modo auto, hold ≥ threshold em Stance 1 → snap + ~1 tiro (não esvazia mag). PA-03-01.
- [ ] F4 — operation stale: pressionar fogo em Stance 1 com pistola, soltar, em <1 frame trocar para rifle (impossível para humanos mas garantido pelo guard) → primeiro tiro do rifle sai normal (sem trigger sintético residual). PA-03-02.
- [ ] F4 + ChangeFireMode mid-hold: trocar fire mode (`B`) durante hold ≥ threshold não cancela o snap pendente. Resurrect dispara segundo o modo ATIVO no frame de release+1, não o modo no button-down. Comportamento intencional. PA-03-03.
- [ ] F3 + V key mesma tecla: configurar `Stance3Hotkey == V` → pressionar `V` ativa Stance 3 (hotkey prioridade); som de stance change toca **uma vez** (não duplicado). PA-04-02.
- [ ] F4 double-tap em fullauto: pressionar fogo, soltar (≥ threshold), pressionar de novo dentro de 1 frame → fire continuous sem stutter de 1 frame. Reset skipa o synthetic false quando trigger está pressionado naturalmente. PA-04-03.
- [ ] F2 — ConfigurationManager ausente: mod inicia sem crash; F12 do CM nem aparece, mas log confirma "ConfigurationManager não detectado" (PA-01-03).
- [ ] F4 — operation-base não resolvida: simular renomeação no Assembly (ou conferir log no boot real); F4 desabilita silenciosamente, F1/F2/F3/F5 funcionam normal (PA-01-01).
- [ ] F5: ao entrar em raid com toggle `true`, jogador inicia já em Stance 3 sem animação; com toggle `false`, inicia em Stance 0.

## Histórico

| Data | Evento |
|---|---|
| 2026-05-10 | Spec técnica criada via `/create-technical-spec` — 5 features mapeadas; 2 patches Harmony novos (F4); 3 patches existentes reusados; resolução por assinatura para overrides nested em `Player.FirearmController`. |
| 2026-05-10 | Revisão `/review-technical-spec` 01 — 2 bloqueadores + 3 importantes + 3 menores levantados. |
| 2026-05-10 | 8 sugestões aceitas. F4 reescrita como **patch único Prefix intercept-and-resurrect** (em vez de 2 patches Postfix+Prefix). Resolução por reflection da nested operation-base de `Player.FirearmController`. Modelo de estado simplificado (`_snapInterceptActive` + sentinel). Stale-timeout 2s. CM cacheado com degradação graciosa. Hotkey priority por menor índice via early-return. AC do som de stance change explicitada. Checklist refinado (8.3 detalhado, 8.5 reescrita). |
| 2026-05-10 | Revisão `/review-technical-spec` 02 — 1 bloqueador (recursão por Invoke), 2 importantes (ambiguidade da operation-base, timing do trigger sintético), 3 menores. |
| 2026-05-10 | 6 sugestões da review-02 aceitas. **PA-02-01:** `[ThreadStatic] _inSyntheticCall` no Prefix como reentry guard contra recursão infinita do `MethodBase.Invoke`. **PA-02-02:** `ResolveFirearmOperationBase` reescrita com filtro `IsAbstract` + fallback `GetBaseDefinition`; detecta ambiguidade. **PA-02-03:** ressurreição **adiada para o frame seguinte** via `_pendingResurrect*` + `TryDispatchPendingResurrect()` no Update — gap de ~16ms dá ao animator tempo de processar o button-up natural. **PA-02-04:** sem closure no Prefix; `_trueArgs` cacheado. **PA-02-05:** `Plugin.Awake` registra `SnapFireTriggerPatch` **condicionalmente** (`OperationOriginalSetTrigger != null`); `GetTargetMethod` lança `InvalidOperationException` defensivamente; spec corrigida (Harmony.Patch NÃO pula null silenciosamente). **PA-02-06:** AC adicionada para som duplicado em F5+F4 simultâneos. PA-02-04 fechado por consequência de PA-02-03. |
| 2026-05-10 | Revisão `/review-technical-spec` 03 — 1 bloqueador (auto-fire runaway por falta de synthetic false), 2 importantes (operation stale entre frames, ChangeFireMode mid-hold sem AC), 2 menores. |
| 2026-05-10 | 5 sugestões da review-03 aceitas. **PA-03-01:** F4 evolui para **2-frame pulse** — synthetic true em N+1, synthetic false em N+2 — para parar fullauto após ~1 tiro; sem isso, holding em fullauto esvaziava o carregador. `_pendingResetInstance/Method` adicionados; `RaiseSyntheticTrigger(bool)` generalizado; `_falseArgs` cacheado. **PA-03-02:** validação `IsOperationStillCurrent` em ambos os despachos; `Plugin.CurrentOperationGetter` cacheado no Awake. **PA-03-03:** AC documentando que ChangeFireMode mid-hold é tratado segundo o modo ativo no release+1. **PA-03-04:** `OnScrollModeSettingChanged` como método nomeado + `Plugin.OnDestroy` com unsubscribe explícito. **PA-03-05:** §5.7 com stub concreto da extensão do tuple `_stanceDefaults` + helper `BuildStanceConfig`; Stance 0 recebe `null` sentinela em `SnapToStance0OnFire` (sem ConfigEntry). |
| 2026-05-10 | Revisão `/review-technical-spec` 04 — 0 bloqueadores, 2 importantes (collision Order 59, ordem hotkey vs V), 3 menores. Após 3 reviews anteriores, ritmo de descobertas em queda — pontos de polimento. |
| 2026-05-10 | 5 sugestões da review-04 aceitas. **PA-04-01:** `_MouseWheelScrollMode` movido de Order 59 → 58 (slot livre por `_UseOnlyStances` removido); resolve colisão com `_MouseWheelModifierKey` (que permanece em 59 verdadeiramente "sem alteração"). **PA-04-02:** `HandleStanceHotkeys` retorna `bool`; `Update()` chama hotkeys **antes** da tecla V com early-return — prioridade de hotkey sobre V quando coincidem. **PA-04-03:** `IsTriggerPressedNaturally()` como guard no bloco de reset (frame N+2) — skip synthetic false se input natural já mantém o trigger, evita stutter de 1 frame em fullauto durante double-tap. **PA-04-04:** `TryApplyPendingInitialStance` ganha guard `is HideoutPlayer` + comentário sobre limitação do `ResetState` em hot-reload de dev. **PA-04-05:** fechado por consequência de PA-04-01. F12 layout em spec funcional atualizado. |
