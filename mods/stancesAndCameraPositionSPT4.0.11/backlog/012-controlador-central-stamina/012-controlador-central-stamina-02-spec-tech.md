# 012 — Controlador central de stamina de braço · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [012-controlador-central-stamina-01-spec.md](012-controlador-central-stamina-01-spec.md)
**Criado:** 2026-06-21

> Fonte de verdade: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Refs verificadas via sub-agents de pesquisa (Sessão 6).

## 1. Estratégia

**`StaminaController` (autoridade única, escrita direta) + neutralização do `Process`/`Consume` vanilla.** Um `Tick()` estático roda 1×/frame no `Plugin.Update` (substitui `StanceManager.TickStanceStamina`): amostra o estado **num único ponto**, resolve **um** `StaminaScenario` (estado principal × modificador), lê o multiplicador daquele cenário num `ConfigEntry` e escreve `HandsStamina.Current += BaseRate*(mult-1)*dt`. Dois **Prefix** em `GClass774.Process`/`Consume` retornam `false` (pulam o vanilla) **somente** para a instância de braço do MainPlayer enquanto o controller comanda — a `Stamina` de perna (mesma classe `GClass774`, instância diferente) fica intacta. Os eventos nativos (`OnValueChanged`/`OnThresholdPass`/`OnExpired`) são re-disparados via reflection para preservar tremor de exaustão e barra. Substitui os 3 patches de stamina do `06-fix-01` e absorve o `ArmStaminaCoordinator.cs`.

Alternativas descartadas: subordinar via `GetHandsRestorationFunc` (não funciona em ADS — o aim-drain seta `DisableRestoration`, PlayerPhysicalClass.cs:711, bloqueando a restauração); coordenador consultado por cada patch (06-fix-01 — divergência intra-frame, causa do bug).

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`GClass774.cs:303`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L303) `Process(float dt)` → void | Prefix `return false` | Neutraliza o loop de stamina vanilla do braço do MainPlayer |
| [`GClass774.cs:241`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L241) `Consume(GClass773, bool)` → float | Prefix `return false` (`__result=Current`) | Neutraliza consumos pontuais do braço do MainPlayer |
| [`GClass774.cs:23`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L23) `Current` (campo público) | escrita direta | O controller aplica o delta por cenário |
| [`GClass774.cs:298`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L298) `HandleExpiration()` público | chamada | Dispara `OnExpired` ao cruzar 0 |
| [`GClass774.cs:47`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L47) `action_1` / [`:53`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L53) `action_3` | reflection (`AccessTools.Field`) | Re-disparar `OnThresholdPass`/`OnValueChanged` (tremor/barra) |
| [`BasePhysicalClass.cs:353-355`](../../../../references/eft-decompiled/Assembly-CSharp/BasePhysicalClass.cs#L353) `Stamina`/`HandsStamina` (ambos `GClass774`) | leitura (gate) | Distinguir braço de perna no Prefix |

Estado amostrado (leitura): `ProceduralWeaponAnimation.IsMountedState`/`IsAiming`, `Player.IsInPronePose`, `PlayerPhysicalClass.HoldingBreath`, `PassiveMountState.IsBracing` (mod), `StanceManager.CurrentStance` (mod). `BaseRate` = `StanceManager._cachedAimDrainRate` ([StanceManager.cs:1151](../../modded-beta/StanceManager.cs), populado de `backend.Stamina.AimDrainRate` em :1234).

## 3. Novas propriedades F12 (BepInEx)

Grupo único **`Stamina Management`** (15 multiplicadores) + 1 toggle de debug. Semântica de todos os multiplicadores: `< 1.0` drena · `1.0` mantém · `> 1.0` recupera.

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `Stamina Management` | `Stance 0 Stamina Multiplier` | float | 0.5 | 0 a 10 | Stand up sem mount, Stance 0 (hipfire). <1 drena, 1 mantém, >1 recupera. |
| `Stamina Management` | `Stance 1 Stamina Multiplier` | float | 1.5 | 0 a 10 | Stand up sem mount, Stance 1. |
| `Stamina Management` | `Stance 2 Stamina Multiplier` | float | 1.0 | 0 a 10 | Stand up sem mount, Stance 2. |
| `Stamina Management` | `Stance 3 Stamina Multiplier` | float | 2.0 | 0 a 10 | Stand up sem mount, Stance 3. |
| `Stamina Management` | `ADS - Stand up Multiplier` | float | 0.7 | 0 a 10 | Stand up sem mount, mirando (ADS). |
| `Stamina Management` | `Hold Breath - Stand up Multiplier` | float | 0.5 | 0 a 10 | Stand up sem mount, segurando a respiração. |
| `Stamina Management` | `Prone Stamina Multiplier` | float | 1.5 | 0 a 10 | Deitado (prone) sem mount, hipfire. |
| `Stamina Management` | `ADS - Prone Multiplier` | float | 0.9 | 0 a 10 | Deitado, mirando. |
| `Stamina Management` | `Hold Breath - Prone Multiplier` | float | 0.7 | 0 a 10 | Deitado, segurando a respiração. |
| `Stamina Management` | `Passive Mount Multiplier` | float | 1.5 | 0 a 10 | Apoio passivo (encostado), Stance 0. |
| `Stamina Management` | `ADS - Passive Mount Multiplier` | float | 1.0 | 0 a 10 | Apoio passivo, mirando (segura, não recupera). |
| `Stamina Management` | `Hold Breath - Passive Mount Multiplier` | float | 0.9 | 0 a 10 | Apoio passivo, segurando a respiração. |
| `Stamina Management` | `Active Mount Multiplier` | float | 3.0 | 0 a 10 | Mount nativo (montado), Stance 0. |
| `Stamina Management` | `ADS - Active Mount Multiplier` | float | 1.5 | 0 a 10 | Mount nativo, mirando. |
| `Stamina Management` | `Hold Breath - Active Mount Multiplier` | float | 1.0 | 0 a 10 | Mount nativo, segurando a respiração. |
| `Stamina Management` | `Debug Stamina State` | bool | false | — | Mostra na tela + loga o cenário de stamina ativo. |

**Removidas:** `Active/Passive Mount Stamina Regen` (06-fix-01) e `_HoldBreathArmStaminaDrain` (drain agora é o multiplicador Hold Breath). Os `Stance X Stamina Multiplier` **migram** da seção `Stance X` para `Stamina Management` (breaking — §7).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-beta/StaminaController.cs` | CRIAR | Enum `StaminaScenario` (15+Inactive), `Resolve`, `Tick` (escrita+eventos), `CurrentLabel`, `ControllingHands`, `Reset`. Absorve `ArmStaminaCoordinator.cs`. |
| `modded-beta/StaminaDebugUI.cs` | CRIAR | `MonoBehaviour` com `OnGUI` (texto `STAMINA STATE` + `Current`), gated por toggle. |
| `modded-beta/ArmStaminaCoordinator.cs` | DELETAR | Absorvido pelo `StaminaController`. |
| `modded-beta/Patches/StanceStaminaRecoveryPatch.cs` | MODIFICAR | Trocar os 3 patches por `HandsStaminaNeutralizePatch` (Process) + `HandsConsumeNeutralizePatch` (Consume), com gate de braço/MainPlayer. |
| `modded-beta/Plugin.cs` | MODIFICAR | `Update` chama `StaminaController.Tick`; `BindStaminaManagement()` (15 entries + toggle) antes de `HoldBreathSection`; remove `*_Regen`/`_HoldBreathArmStaminaDrain`; `AddComponent<StaminaDebugUI>`; reordena descoberta Stance 2/3; enables dos 2 patches novos. |
| `modded-beta/StanceManager.cs` | MODIFICAR | Esvaziar `TickStanceStamina`; expor `CachedAimDrainRate`; `ApplyStaminaStance` mantém só speed-limit; `BindStance` deixa de criar `StaminaMultiplier`. |
| `modded-beta/StanceConfig.cs` | MODIFICAR | `StaminaMultiplier` passa a ser atribuído por `BindStaminaManagement`. |
| `modded-beta/Patches/PassiveMountDetectPatch.cs` | MODIFICAR | Corner case: não `SetBracing` se `stance∈{1,2,3} && !ads`. |
| `modded-beta/Patches/ApplyComplexRotationPatch.cs` | MODIFICAR | Remover o arm-stamina drain do hold-breath (vira multiplicador); manter o oxygen drain. |
| `modded-beta/Patches/RaidLifecyclePatches.cs` | MODIFICAR | `StaminaController.Reset()` em OnRaidStart/OnRaidEnd. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar o grupo `Stamina Management`; remover as linhas órfãs. |

## 5. Stubs de código

```csharp
// modded-beta/StaminaController.cs
using System;
using System.Reflection;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace CameraRotationMod
{
    public enum StaminaScenario
    {
        Inactive,
        StandStance0, StandStance1, StandStance2, StandStance3, StandAds, StandHoldBreath,
        ProneHip, ProneAds, ProneHoldBreath,
        PassiveStance0, PassiveAds, PassiveHoldBreath,
        ActiveStance0, ActiveAds, ActiveHoldBreath
    }

    public static class StaminaController
    {
        public static StaminaScenario Current { get; private set; } = StaminaScenario.Inactive;
        public static string CurrentLabel { get; private set; } = "Inactive";
        public static bool ControllingHands { get; private set; }   // lido pelos Prefixes de neutralização
        private static StaminaScenario _prev = StaminaScenario.Inactive;

        // Índice = (int)StaminaScenario; preenchido por Plugin.BindStaminaManagement().
        public static ConfigEntry<float>[] Multipliers = new ConfigEntry<float>[16];

        // ref: Assembly-CSharp/GClass774.cs:47 (action_1=OnThresholdPass), :53 (action_3=OnValueChanged)
        private static readonly FieldInfo _onThreshold = AccessTools.Field(typeof(GClass774), "action_1");
        private static readonly FieldInfo _onValueChanged = AccessTools.Field(typeof(GClass774), "action_3");

        public static void Tick()
        {
            try
            {
                if (!StanceManager.IsActiveContext()) { SetScenario(StaminaScenario.Inactive); ControllingHands = false; return; }
                Player p = Singleton<GameWorld>.Instance.MainPlayer;
                GClass774 hands = p?.Physical?.HandsStamina;            // ref: BasePhysicalClass.cs:355
                if (hands == null || !(p.HandsController is Player.FirearmController))
                { SetScenario(StaminaScenario.Inactive); ControllingHands = false; return; }   // mãos vazias → cede ao vanilla

                StaminaScenario s = Resolve(p);
                SetScenario(s);
                ControllingHands = true;

                ConfigEntry<float> cfg = Multipliers[(int)s];
                float mult = cfg != null ? cfg.Value : 1f;
                float delta = StanceManager.CachedAimDrainRate * (mult - 1f) * Time.deltaTime;

                float prev = hands.Current;                            // ref: GClass774.cs:23
                float target = Mathf.Clamp(prev + delta, 0f, (float)hands.TotalCapacity);
                if (Mathf.Abs(target - prev) < 0.0001f) return;
                hands.Current = target;

                if (_onValueChanged != null && (int)prev != (int)target) (_onValueChanged.GetValue(hands) as Action)?.Invoke();   // barra (PA-01-02 null-guard)
                if (_onThreshold != null && (prev >= 15f) != (target >= 15f)) (_onThreshold.GetValue(hands) as Action)?.Invoke(); // tremor (Exhausted<15, GClass774.cs:106)
                if (delta < 0f && target <= 0f && prev > 0f) hands.HandleExpiration();                  // ref: GClass774.cs:298
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[StaminaController] {ex}"); }
        }

        private static StaminaScenario Resolve(Player p)
        {
            EFT.Animations.ProceduralWeaponAnimation pwa = p.ProceduralWeaponAnimation;
            bool ads = pwa != null && pwa.IsAiming;
            bool hb = p.Physical != null && p.Physical.HoldingBreath;  // ref: PlayerPhysicalClass.HoldingBreath
            if (pwa != null && pwa.IsMountedState)                      // Active
                return hb ? StaminaScenario.ActiveHoldBreath : ads ? StaminaScenario.ActiveAds : StaminaScenario.ActiveStance0;
            if (PassiveMountState.IsBracing && Plugin._EnablePassiveMount.Value)   // Passive
                return hb ? StaminaScenario.PassiveHoldBreath : ads ? StaminaScenario.PassiveAds : StaminaScenario.PassiveStance0;
            if (p.IsInPronePose)                                        // Prone (ignora stance)
                return hb ? StaminaScenario.ProneHoldBreath : ads ? StaminaScenario.ProneAds : StaminaScenario.ProneHip;
            if (hb) return StaminaScenario.StandHoldBreath;             // Stand
            if (ads) return StaminaScenario.StandAds;
            switch (StanceManager.CurrentStance)
            {
                case Stance.Stance1: return StaminaScenario.StandStance1;
                case Stance.Stance2: return StaminaScenario.StandStance2;
                case Stance.Stance3: return StaminaScenario.StandStance3;
                default: return StaminaScenario.StandStance0;
            }
        }

        private static void SetScenario(StaminaScenario s)
        {
            Current = s;
            if (s == _prev) return;
            _prev = s; CurrentLabel = Label(s);
            if (Plugin._DebugStaminaState != null && Plugin._DebugStaminaState.Value)
                Plugin.Logger.LogInfo($"STAMINA STATE: {CurrentLabel}");
        }

        public static string Label(StaminaScenario s)
        {
            switch (s)
            {
                case StaminaScenario.StandStance0: return "Stand up sem mount - Stance 0";
                case StaminaScenario.StandStance1: return "Stand up sem mount - Stance 1";
                case StaminaScenario.StandStance2: return "Stand up sem mount - Stance 2";
                case StaminaScenario.StandStance3: return "Stand up sem mount - Stance 3";
                case StaminaScenario.StandAds: return "Stand up sem mount - ADS";
                case StaminaScenario.StandHoldBreath: return "Stand up sem mount - Hold Breath";
                case StaminaScenario.ProneHip: return "Prone sem mount - Hipfire";
                case StaminaScenario.ProneAds: return "Prone sem mount - ADS";
                case StaminaScenario.ProneHoldBreath: return "Prone sem mount - Hold Breath";
                case StaminaScenario.PassiveStance0: return "Passive Mount - Stance 0";
                case StaminaScenario.PassiveAds: return "Passive Mount - ADS";
                case StaminaScenario.PassiveHoldBreath: return "Passive Mount - Hold Breath";
                case StaminaScenario.ActiveStance0: return "Active Mount - Stance 0";
                case StaminaScenario.ActiveAds: return "Active Mount - ADS";
                case StaminaScenario.ActiveHoldBreath: return "Active Mount - Hold Breath";
                default: return "Inactive";
            }
        }

        public static void Reset() { Current = _prev = StaminaScenario.Inactive; CurrentLabel = "Inactive"; ControllingHands = false; }
    }
}
```

```csharp
// modded-beta/Patches/StanceStaminaRecoveryPatch.cs  (substitui os 3 patches antigos)
using System;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace CameraRotationMod.Patches
{
    // Neutraliza o loop de stamina vanilla SOMENTE para o braço do MainPlayer enquanto o controller comanda.
    // Stamina (perna) e HandsStamina (braço) são ambos GClass774 — o gate por instância protege a perna.
    public class HandsStaminaNeutralizePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GClass774), nameof(GClass774.Process));   // ref: GClass774.cs:303

        [PatchPrefix]
        private static bool Prefix(GClass774 __instance)
        {
            try
            {
                var h = Singleton<GameWorld>.Instance?.MainPlayer?.Physical?.HandsStamina;
                if (StaminaController.ControllingHands && __instance == h) return false;  // controller escreve
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[HandsNeutralize.Process] {ex}"); }
            return true;
        }
    }

    public class HandsConsumeNeutralizePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GClass774), nameof(GClass774.Consume));   // ref: GClass774.cs:241

        [PatchPrefix]
        private static bool Prefix(GClass774 __instance, ref float __result)
        {
            try
            {
                var h = Singleton<GameWorld>.Instance?.MainPlayer?.Physical?.HandsStamina;
                if (StaminaController.ControllingHands && __instance == h)
                { __result = __instance.Current; return false; }   // ref: GClass774.cs:23
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[HandsNeutralize.Consume] {ex}"); }
            return true;
        }
    }
}
```

```csharp
// modded-beta/StaminaDebugUI.cs
using Comfort.Common;
using EFT;
using UnityEngine;

namespace CameraRotationMod
{
    public class StaminaDebugUI : MonoBehaviour   // AddComponent no GameObject persistente do plugin
    {
        private GUIStyle _style;
        private void OnGUI()
        {
            if (Plugin._DebugStaminaState == null || !Plugin._DebugStaminaState.Value) return;
            var h = Singleton<GameWorld>.Instance?.MainPlayer?.Physical?.HandsStamina;
            if (h == null) return;
            if (_style == null) _style = new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = Color.yellow } };
            GUI.Label(new Rect(20f, 20f, 640f, 28f),
                $"STAMINA STATE: {StaminaController.CurrentLabel}  ({h.Current:F0}/{(float)h.TotalCapacity:F0})", _style);
        }
    }
}
```

```csharp
// modded-beta/Plugin.cs  (trecho — chamado ANTES de HoldBreathSection; ver §7 risco de ordenação)
private void BindStaminaManagement()
{
    const string SEC = "Stamina Management";
    // Stance 0-3 — também preenchem _stanceConfigs[stance].StaminaMultiplier (BindStance não cria mais)
    StaminaController.Multipliers[(int)StaminaScenario.StandStance0] =
        _stanceConfigs[Stance.Default].StaminaMultiplier =
            Config.Bind(SEC, "Stance 0 Stamina Multiplier", 0.5f,
                new ConfigDescription("Stand up sem mount, Stance 0 (hipfire). <1 drena, 1 mantém, >1 recupera.",
                    new AcceptableValueRange<float>(0f, 10f), new ConfigurationManagerAttributes { Order = 80 }));
    // ... Stance 1/2/3, ADS×4, Hold Breath×4, Prone, Passive, Active (mesmo padrão, Order decrescente) ...
    _DebugStaminaState = Config.Bind(SEC, "Debug Stamina State", false,
        new ConfigDescription("Mostra na tela + loga o cenário de stamina ativo.", null,
            new ConfigurationManagerAttributes { Order = 1 }));
}
```

## 6. Fluxo de dados

```
[Plugin.Update:1630] → StaminaController.Tick()
  └─ IsActiveContext? (StanceManager.cs:1213) ─não→ Inactive, ControllingHands=false (vanilla assume)
  └─ amostra: IsMountedState / IsBracing(mod) / IsInPronePose / IsAiming / HoldingBreath / CurrentStance(mod)
       └─ Resolve → StaminaScenario  → Multipliers[s].Value = mult
            └─ hands.Current += BaseRate*(mult-1)*dt   (GClass774.cs:23)
                 └─ dispara OnValueChanged/OnThresholdPass (reflection action_3/action_1) + HandleExpiration

[Player.Update → PlayerPhysicalClass.Update:1111] → HandsStamina.Process(dt)   (GClass774.cs:303)
  └─ Prefix: ControllingHands && __instance==MainPlayer.HandsStamina ? return false (skip) : roda vanilla
       (Stamina de perna = outra instância GClass774 → nunca neutralizada)
```

## 7. Riscos e dependências

- **🔴 Gate de perna (invariante):** `Process`/`Consume` Prefix DEVEM checar `__instance == MainPlayer.Physical.HandsStamina`. Sem isso, a `Stamina` de perna (mesma classe `GClass774`, BasePhysicalClass.cs:353) é neutralizada → sprint/movimento quebram. Teste de regressão obrigatório.
- **🟡 Ordenação do grupo no F12 (assunção a validar in-game):** assumido que a ordem de seções é por **descoberta** (comentário Plugin.cs:764) → `BindStaminaManagement` é chamado antes de `HoldBreathSection`. Sub-agent indicou possível ordenação **por nome**; se in-game o grupo não aparecer acima de `9. Respiração`, prefixar o nome (ex.: `8. Stamina Management`) — 1 linha. **Escalar ao usuário na validação.**
- **🟡 Ordem Tick × Process no frame:** `ControllingHands` é setado no `Plugin.Update`; o `Process` roda no `Player.Update`. Na 1ª transição há ≤1 frame de defasagem (vanilla roda 1 frame). Irrelevante na prática.
- **Breaking change F12:** `Stance X Stamina Multiplier` migram de seção (BepInEx casa por `(section,key)` → valores resetam); `*_Regen` e `_HoldBreathArmStaminaDrain` removidas. Documentar em PROPRIEDADES.
- **Patches existentes:** remove `StanceStaminaRecoveryPatch`/`HandsStaminaConsumePatch`/`HandsStaminaProcessPatch` (06-fix-01). O `PassiveMountDetectPatch`/buffs do 011 permanecem.
- **Reflection de eventos:** `action_1`/`action_3` são `[CompilerGenerated]` privados — acessíveis, mas se subscriber for null o `?.Invoke()` é no-op seguro. **PA-01-02:** os `FieldInfo` têm null-guard (degradam sem o evento + log único se faltarem).
- **🟡 Buffs ignorados (PA-01-03):** ao neutralizar o `Process`, `BuffRestoration`/skills (stims, Endurance) deixam de afetar o braço — **consequência aceita** do controle 100%; documentar em PROPRIEDADES.
- **Limpeza (PA-01-04):** `StanceStaminaState.Multiplier`/`ShouldApplyStamina` ficam órfãos — remover no `/code-mod` (manter só `IsSuspendedByProne` para o speed-limit).

## 8. Checklist de implementação

- [ ] **Passo 0 — ordenação:** confirmar (dummy/observação) se a seção nova fica acima de `9. Respiração`; decidir nome `Stamina Management` vs prefixado.
- [ ] Criar `StaminaController.cs` (enum, Resolve, Tick, eventos, Reset); deletar `ArmStaminaCoordinator.cs`.
- [ ] `StanceManager`: expor `CachedAimDrainRate`; esvaziar `TickStanceStamina`; `BindStance` sem `StaminaMultiplier`; reordenar descoberta Stance 2/3.
- [ ] `Plugin`: `BindStaminaManagement()` (15 entries + toggle) antes de `HoldBreathSection`; `Update`→`StaminaController.Tick`; remover `*_Regen`/`_HoldBreathArmStaminaDrain`; `AddComponent<StaminaDebugUI>`; enables.
- [ ] Reescrever `StanceStaminaRecoveryPatch.cs` → 2 Prefixes de neutralização com gate de braço/MainPlayer.
- [ ] `PassiveMountDetectPatch`: corner case Stance0/ADS.
- [ ] `ApplyComplexRotationPatch`: remover arm-drain do hold-breath.
- [ ] `RaidLifecyclePatches`: `StaminaController.Reset()`.
- [ ] Criar `StaminaDebugUI.cs`.
- [ ] `PROPRIEDADES.md`: grupo + remoções.
- [ ] `/compile-mod` 0 erros.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop idempotentes — AP-01 | ✅ | `StaminaController.Reset()` em OnRaidStart/OnRaidEnd (RaidLifecyclePatches); `ControllingHands=false` quando `!IsActiveContext` |
| 2 | Filtro MainPlayer/Fika — AP-02 | ✅ | `Tick` só `MainPlayer`; Prefixes gate `__instance==MainPlayer.HandsStamina`; debug local; peers usam `Process` vanilla |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ N/A virtual | `GClass774.Process/Consume` não são virtuais (classe concreta, 2 instâncias); resolvidos em compile-time via `typeof` (padrão já usado pelo mod). Ofuscação: recompila por versão |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | escrevo `Current` (campo público) e **re-disparo** `OnValueChanged`/`OnThresholdPass`/`OnExpired` (GClass774.cs:261/268/300) para preservar barra/tremor/expiração |
| 5 | Estado entre raids cobertos | ✅ | `Reset()` (OnRaidStart/OnRaidEnd); sem estado estático preso (`Current`/`ControllingHands` resetados) |
| 6 | Semântica/defaults/faixas de cada ConfigEntry — AP-05 | ✅ | §3: 15 multiplicadores `0..10`, semântica `<1/1/>1` no tooltip; toggle debug default false |
| 7 | Reentry-guard / sem recursão — AP-07 | ✅ N/A | Prefixes só `return false` (não re-invocam); `Tick` não chama `Process`/`Consume` |
| 8 | Flags/caches validados contra contexto após troca — AP-08 | ✅ | `Tick` re-amostra todo o estado a cada frame (sem cache stale); `ControllingHands` re-avaliado; sem arma → cede |

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Spec técnica criada via `/create-technical-spec` (refs via 2 sub-agents de pesquisa) |
