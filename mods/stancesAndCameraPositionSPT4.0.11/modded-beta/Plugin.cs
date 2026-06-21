using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CameraRotationMod.Patches;
using EFT;
using HarmonyLib;
using UnityEngine;
using BepInEx.Bootstrap;
using System.Runtime.CompilerServices;

namespace CameraRotationMod;

// F2 (backlog 002) — modo de scroll do mouse: ciclo circular OU eixo linear fixo.
public enum ScrollMode
{
    Cycle,
    Linear,
}

[BepInPlugin("com.shwng.fpscamerastances", "shwngFpsCameraStances4", "1.3.1")]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; }

    // ⚠️ MANTER o `public static new` — shadow estático do BaseUnityPlugin.Logger.
    // Sem o `new`, vira shadowing implícito (CS0108) e os helpers static deste mod quebram.
    public static new ManualLogSource Logger;

    // Cause reservada pelo mod para AddStateSpeedLimit — int fora dos valores oficiais
    // de Player.ESpeedLimit (0..8). Documentado no README.
    public const int StanceSpeedLimitCauseId = 9001;
    public static Player.ESpeedLimit StanceSpeedLimitCause => (Player.ESpeedLimit)StanceSpeedLimitCauseId;

    // Indexado pelo enum Stance (Default/Stance1/Stance2/Stance3) — não usar int paralelo.
    public static readonly Dictionary<Stance, StanceConfig> _stanceConfigs = new(4);

    public static readonly Dictionary<string, Sprite> LoadedSprites = new();

    // StaminaMultiplier: <1.0 = drain, 1.0 = vanilla, >1.0 = recovery. ref: fix-01.
    // SnapOnFire (backlog 002 F4): defaults divergem por stance. Stance 0 = false (sentinel).
    private static readonly (Stance Stance, string Section, float StaminaMultiplier, bool ModSpeed, int Multiplier, bool ApplyProne, bool SnapOnFire)[]
        _stanceDefaults =
    {
        (Stance.Default, Stance0Section, 0.5f,  true,  90,  false, false),  // Stance 0: irrelevante
        (Stance.Stance1, Stance1Section, 1.5f,  true,  95,  false, true),
        // 06-fix-01: Stance 2 e Stance 3 trocaram. Stance 2 (Low Ready) é relaxada — stamina 1.0,
        // speed 90, snap off (pré-mira). Stance 3 (Custom) mantém defaults antigos da Stance 2.
        (Stance.Stance2, Stance2Section, 1.0f,  true,  90,  false, false),  // Low Ready (cano desce)
        (Stance.Stance3, Stance3Section, 2.0f,  true,  100, false, true),   // Custom (lateral)
    };

    // Section constants (in display order - top to bottom)
    private const string Positions = "Positions";
    private const string Settings = "Settings";
    private const string AdvancedADSSettings = "Advanced ADS Transitions";
    private const string ADSDefaults = "ADS Default Values (Advanced)";
    private const string DefaultHandsPositions = "Default Hands/Arms Positions (Advanced)";
    private const string Stance0Section = "Stance 0 - Vanilla";
    // backlog 002 — section renames: nomes refletem os eixos reais de Pitch/Yaw configurados.
    // ⚠️ BREAKING: usuários com .cfg antigo (Ready Up/Ready Down/Custom) terão entries recriadas
    // com defaults — migração manual documentada no changelog do mod.
    private const string Stance1Section = "Stance 1 - High Ready";
    // 06-fix-01: Stance 2 e Stance 3 trocaram de papel. Stance 2 agora é Low Ready (cano desce,
    // alvo do F5 e do fim do eixo Linear); Stance 3 é Custom (lateral, off-axis).
    private const string Stance2Section = "Stance 2 - Low Ready";
    private const string Stance3Section = "Stance 3 - Custom";
    private const string TacSprintSettings = "Tac Sprint Settings (Advanced)";
    private const string FOVSettings = "Field of View";
    private const string DebugSettings = "Debug (Advanced)";
    private const string PassiveMountSettings = "Weapon Mount (Passive)";
    private const string AnimationSettings = "Animations & Transitions (Item 005)";

    // Positions
    public static ConfigEntry<bool> _PositionEnabled;
    public static ConfigEntry<float> _ForwardBackwardOffset;
    public static ConfigEntry<float> _UpDownOffset;
    public static ConfigEntry<float> _SidewaysOffset;

    // Settings
    public static ConfigEntry<bool> _EnableStance1;
    public static ConfigEntry<bool> _EnableStance2;
    public static ConfigEntry<bool> _EnableStance3;
    public static ConfigEntry<KeyCode> _StanceToggleKey;
    public static ConfigEntry<bool> _EnableMouseWheelCycle;
    public static ConfigEntry<KeyCode> _MouseWheelModifierKey;
    public static ConfigEntry<float> _StanceTransitionSpeed;
    public static ConfigEntry<float> _StanceKickIntensity;
    public static ConfigEntry<float> _ADSKickDelay;
    public static ConfigEntry<float> _StanceOvershootDamping;
    public static ConfigEntry<float> _ADSTransitionSpeed;

    // Hold Breath
    public static ConfigEntry<float> _HoldBreathArmStaminaDrain;
    public static ConfigEntry<float> _HoldBreathOxygenDrain;
    public static ConfigEntry<bool> _EnableCustomBreathAudio;
    public static ConfigEntry<float> _BreathInVolume;
    public static ConfigEntry<float> _BreathOutVolume;
    public static ConfigEntry<float> _HeartbeatVolume;

    // Oxygen UI
    public static ConfigEntry<bool> _EnableOxygenUI;
    public static ConfigEntry<float> _OxygenUIPosX;
    public static ConfigEntry<float> _OxygenUIPosY;
    public static ConfigEntry<float> _OxygenUIWidth;
    public static ConfigEntry<float> _OxygenUIHeight;

    // backlog 002 F1 — substitui `Use Only Stances` (lógica invertida). True = Stance 0 entra no ciclo.
    public static ConfigEntry<bool> _IncludeStance0InCycle;
    // backlog 002 F2 — modo de scroll do mouse (Cycle circular OU Linear eixo fixo).
    public static ConfigEntry<ScrollMode> _MouseWheelScrollMode;
    // backlog 002 F3 — teclas dedicadas por stance (KeyCode.None = sem hotkey).
    public static ConfigEntry<KeyCode> _Stance0Hotkey;
    public static ConfigEntry<KeyCode> _Stance1Hotkey;
    public static ConfigEntry<KeyCode> _Stance2Hotkey;
    public static ConfigEntry<KeyCode> _Stance3Hotkey;
    // backlog 002 F4 — limiar (ms) entre press e release para classificar clique único.
    public static ConfigEntry<int> _SnapFireThreshold;
    // ref: CR-01-06 — timeout (s) do stale guard do snap intercept (cobre weapon swap durante hold).
    public static ConfigEntry<float> _SnapStaleTimeoutSec;
    // backlog 002 F5 — iniciar raid em Stance 3 - Low Ready (set imediato sem animação).
    public static ConfigEntry<bool> _StartInLowReadyOnRaidBegin;

    // backlog 002 F2 — refs aos `ConfigurationManagerAttributes` para mutar `Browsable` em runtime
    // (visibilidade dependente do Mouse Wheel Scroll Mode). Capturadas no Awake durante Config.Bind.
    private static ConfigurationManagerAttributes _attrIncludeStance0;
    private static ConfigurationManagerAttributes _attrEnableStance1Cycle;
    private static ConfigurationManagerAttributes _attrEnableStance2Cycle;
    private static ConfigurationManagerAttributes _attrEnableStance3Cycle;

    // backlog 002 F2 — cache da reflexão do ConfigurationManager (PA-01-03) para forçar
    // refresh do F12 em runtime. Resolvidos em TryResolveConfigurationManager (Awake).
    private static MethodInfo _cmBuildSettingListMethod;
    private static UnityEngine.Object _cmInstance;
    private static bool _cmRefreshAvailable;

    // backlog 002 F4 (06-fix-01) — patch target: Player.FirearmController.SetTriggerPressed
    // (NÃO nested operation). Resolvido por reflection no Awake.
    // ref: Player.cs:13668 — `public virtual void SetTriggerPressed(bool pressed)` na própria
    // FirearmController; roteia para CurrentOperation. Patchar aqui captura todos os fire inputs
    // ANTES da virtual dispatch para overrides (que tipicamente não chamam base).
    // null = F4 desabilitado.
    public static MethodBase FirearmControllerSetTrigger { get; private set; }

    // Advanced ADS Transitions
    public static ConfigEntry<bool> _EnableAdvancedADSTransitions;
    public static ConfigEntry<bool> _AffectStanceTransitionToo;
    public static ConfigEntry<float> _StanceChangeSoundVolume;

    // ADS Default Values
    public static ConfigEntry<bool> _ResetOnADS;
    public static ConfigEntry<float> _ADSHandsPitchRotation;
    public static ConfigEntry<float> _ADSHandsYawRotation;
    public static ConfigEntry<float> _ADSHandsRollRotation;
    public static ConfigEntry<float> _ADSHandsForwardBackwardOffset;
    public static ConfigEntry<float> _ADSHandsUpDownOffset;
    public static ConfigEntry<float> _ADSHandsSidewaysOffset;

    // Default Hands/Arms Positions
    public static ConfigEntry<bool> _DefaultHandsPositionEnabled;
    public static ConfigEntry<float> _DefaultHandsForwardBackwardOffset;
    public static ConfigEntry<float> _DefaultHandsUpDownOffset;
    public static ConfigEntry<float> _DefaultHandsSidewaysOffset;

    // Stance 1
    public static ConfigEntry<bool> _Stance1SprintAnimationEnabled;
    public static ConfigEntry<float> _Stance1HandsPitchRotation;
    public static ConfigEntry<float> _Stance1HandsYawRotation;
    public static ConfigEntry<float> _Stance1HandsRollRotation;
    public static ConfigEntry<float> _Stance1HandsForwardBackwardOffset;
    public static ConfigEntry<float> _Stance1HandsUpDownOffset;
    public static ConfigEntry<float> _Stance1HandsSidewaysOffset;

    // Stance 2
    public static ConfigEntry<bool> _Stance2SprintAnimationEnabled;
    public static ConfigEntry<float> _Stance2HandsPitchRotation;
    public static ConfigEntry<float> _Stance2HandsYawRotation;
    public static ConfigEntry<float> _Stance2HandsRollRotation;
    public static ConfigEntry<float> _Stance2HandsForwardBackwardOffset;
    public static ConfigEntry<float> _Stance2HandsUpDownOffset;
    public static ConfigEntry<float> _Stance2HandsSidewaysOffset;

    // Stance 3
    public static ConfigEntry<bool> _Stance3SprintAnimationEnabled;
    public static ConfigEntry<float> _Stance3HandsPitchRotation;
    public static ConfigEntry<float> _Stance3HandsYawRotation;
    public static ConfigEntry<float> _Stance3HandsRollRotation;
    public static ConfigEntry<float> _Stance3HandsForwardBackwardOffset;
    public static ConfigEntry<float> _Stance3HandsUpDownOffset;
    public static ConfigEntry<float> _Stance3HandsSidewaysOffset;

    // Tac Sprint Settings
    public static ConfigEntry<float> _TacSprintWeightLimit;
    public static ConfigEntry<float> _TacSprintWeightLimitBullpup;
    public static ConfigEntry<int> _TacSprintLengthLimit;
    public static ConfigEntry<float> _TacSprintErgoLimit;
    public static ConfigEntry<float> _TacSprintResetDelay;

    // Debug
    public static ConfigEntry<bool> _DebugApplyInHideout;

    // FOV Settings
    public static ConfigEntry<bool> _FOVExpandEnabled;
    public static ConfigEntry<int> _FOVMinRange;
    public static ConfigEntry<int> _FOVMaxRange;

    // Weapon Mount (Passive) — Item 011
    public static ConfigEntry<bool> _EnablePassiveMount;
    public static ConfigEntry<float> _PassiveRecoilMultiplier;
    public static ConfigEntry<float> _PassiveSwayMultiplier;
    public static ConfigEntry<bool> _PassiveStaminaSave;
    public static ConfigEntry<float> _ActiveMountStaminaRegen;
    public static ConfigEntry<float> _PassiveMountStaminaRegen;
    public static ConfigEntry<bool> _ShowMountIcon;

    // Animation Settings (Item 005)
    public static ConfigEntry<float> _CrouchSpeedMultiplier;
    public static ConfigEntry<float> _LeanSpeedMultiplier;

    // Movement & Inertia Settings (Item 007)
    public static ConfigEntry<float> _InertiaMultiplier;
    public static ConfigEntry<float> _WalkSpeedMultiplier;
    public static ConfigEntry<float> _SprintSpeedMultiplier;

    // Action Stance Settings (Item 008)
    public static ConfigEntry<bool> _EnableActionStanceSwap;

    // Stance Cinematic Curves Settings (Item 009)
    public static ConfigEntry<float> _CurveDuration;
    public static ConfigEntry<float> _StanceCurvePitchMultiplier;
    public static ConfigEntry<float> _StanceCurveYawMultiplier;
    public static ConfigEntry<float> _StanceCurveRollMultiplier;
    public static ConfigEntry<float> _StanceCurvePositionMultiplier;
    // Stance 0 (Vanilla) ADS Multipliers
    public static ConfigEntry<float> _Stance0ADSPitchMultiplier;
    public static ConfigEntry<float> _Stance0ADSYawMultiplier;
    public static ConfigEntry<float> _Stance0ADSRollMultiplier;
    public static ConfigEntry<float> _Stance0ADSPosYMultiplier;
    public static ConfigEntry<float> _Stance0ADSPosZMultiplier;
    public static ConfigEntry<float> _Stance0ADSOvershootDamping;

    // Stance 1 ADS Multipliers
    public static ConfigEntry<float> _Stance1ADSPitchMultiplier;
    public static ConfigEntry<float> _Stance1ADSYawMultiplier;
    public static ConfigEntry<float> _Stance1ADSRollMultiplier;
    public static ConfigEntry<float> _Stance1ADSPosYMultiplier;
    public static ConfigEntry<float> _Stance1ADSPosZMultiplier;
    public static ConfigEntry<float> _Stance1ADSOvershootDamping;

    // Stance 2 ADS Multipliers
    public static ConfigEntry<float> _Stance2ADSPitchMultiplier;
    public static ConfigEntry<float> _Stance2ADSYawMultiplier;
    public static ConfigEntry<float> _Stance2ADSRollMultiplier;
    public static ConfigEntry<float> _Stance2ADSPosYMultiplier;
    public static ConfigEntry<float> _Stance2ADSPosZMultiplier;
    public static ConfigEntry<float> _Stance2ADSOvershootDamping;

    public static ConfigEntry<float> _OvershootAmplitude;
    public static ConfigEntry<float> _OvershootFrequency;
    public static ConfigEntry<float> _CameraBobbingMultiplier;
    public static ConfigEntry<float> _StanceTransitionDamping;

    public static ConfigEntry<float> _MaxLeanLimit;

    // Manual Chambering (Item 010)
    public static ConfigEntry<bool> _EnableManualChambering;
    public static ConfigEntry<bool> _ManualChamberingOnRaidStart;
    public static ConfigEntry<bool> _ManualChamberingOnReload;

    public void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        Logger.LogInfo("Plugin shwngFpsCameraStances4 is loaded!");
        Logger.LogInfo($"Camera Rotation Mod has loaded!");

        // backlog 002 F2 — cachear API do ConfigurationManager para refresh de visibilidade em runtime.
        TryResolveConfigurationManager();

        // backlog 002 F4 (06-fix-01) — resolver Player.FirearmController.SetTriggerPressed por reflection.
        // Patch target trocado de operation-base para o método de roteamento da FC (virtual dispatch
        // bypassa Harmony quando override não chama base — só 1 de 14 overrides chama).
        ResolveFirearmControllerSetTrigger();

        // Enable patches — CADA UM isolado em try/catch (SafeEnable). Se um target Harmony não resolver
        // em algum build 0.16 (GClass volátil, method_NN renomeado), ele falha SOZINHO com log
        // `[enable] FAIL <nome>` em vez de derrubar todos os patches seguintes do Awake.
        SafeEnable("PlayerSpringPatch", () => new PlayerSpringPatch());        // camera position
        SafeEnable("ApplySimpleRotationPatch", () => new ApplySimpleRotationPatch());  // stance interpolation
        new CameraRotationMod.Patches.ApplyComplexRotationPatch().Enable();
        new CameraRotationMod.Patches.HoldBreathPatch().Enable();
        // Áudio NÃO é carregado aqui (cena de menu): a transição p/ o jogo descarrega os clips
        // (length vira 0). Carregamento é disparado em GameWorld.OnGameStarted via HoldBreathPatch.OnRaidStart().
        SafeEnable("FOVSliderPatch", () => new FOVSliderPatch());

        // Fika Multiplayer Sync Integration (Soft Dependency)
        if (Chainloader.PluginInfos.ContainsKey("com.fika.core"))
        {
            try
            {
                InitFikaSync();
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"[CameraRotationMod] Erro ao tentar inicializar integração com Fika: {ex.Message}");
            }
        }
        else
        {
            Logger.LogInfo("[CameraRotationMod] Fika multiplayer not found. Running in offline/standard mode.");
        }

        // UI
        gameObject.AddComponent<UI.OxygenUI>();

        // Stamina/velocidade por stance (backlog 001)
        SafeEnable("StanceStaminaRecoveryPatch", () => new StanceStaminaRecoveryPatch());
        SafeEnable("HandsStaminaConsumePatch", () => new Patches.HandsStaminaConsumePatch());
        SafeEnable("HandsStaminaProcessPatch", () => new Patches.HandsStaminaProcessPatch());
        SafeEnable("GameWorldOnGameStartedPatch", () => new GameWorldOnGameStartedPatch());
        SafeEnable("GameWorldOnDestroyPatch", () => new GameWorldOnDestroyPatch());

        // backlog 002 F4 — registro condicional do snap fire patch.
        if (FirearmControllerSetTrigger != null)
            SafeEnable("SnapFireTriggerPatch", () => new SnapFireTriggerPatch());
        else
            Logger.LogWarning("[F4] SnapFireTriggerPatch NOT enabled — Player.FirearmController.SetTriggerPressed " +
                              "não foi resolvida. F1/F2/F3/F5 funcionam normalmente; snap-on-fire desabilitado este boot.");

        // Item 011: Mount passivo sobre o vanilla (detecção + buffs + UI). Ativo = 100% vanilla.
        SafeEnable("PassiveMountDetectPatch", () => new Patches.PassiveMountDetectPatch());
        SafeEnable("PassiveRecoilPatch", () => new Patches.PassiveRecoilPatch());
        SafeEnable("PassiveSwayPatch", () => new Patches.PassiveSwayPatch());
        SafeEnable("BattleUIScreenPatch", () => new BattleUIScreenPatch());

        // Item 007: Movement & Inertia
        SafeEnable("MovementContextSpeedPatch", () => new Patches.MovementContextSpeedPatch());
        SafeEnable("MovementContextSprintSpeedPatch", () => new Patches.MovementContextSprintSpeedPatch());
        SafeEnable("PlayerChangeSpeedPatch", () => new Patches.PlayerChangeSpeedPatch());
        SafeEnable("PhysicalInertiaPatch", () => new Patches.PhysicalInertiaPatch());

        // Item 008: Action Stance Swap (Reload, Check Ammo, etc)
        SafeEnable("LocaleClassReloadPatch", () => new Patches.LocaleClassReloadPatch());
        SafeEnable("ActionStancePatch", () => new Patches.ActionStancePatch());
        SafeEnable("ActionStanceCheckChamberPatch", () => new Patches.ActionStanceCheckChamberPatch());
        SafeEnable("ActionStanceExamineWeaponPatch", () => new Patches.ActionStanceExamineWeaponPatch());
        SafeEnable("ActionStanceReloadPatch", () => new Patches.ActionStanceReloadPatch());
        SafeEnable("ActionStanceUnloadMagPatch", () => new Patches.ActionStanceUnloadMagPatch());
        SafeEnable("ActionStanceUnloadChamberPatch", () => new Patches.ActionStanceUnloadChamberPatch()); // GClass2046 (esvaziar câmara)
        SafeEnable("ActionStanceOnIdlePatch", () => new Patches.ActionStanceOnIdlePatch());
        SafeEnable("ActionStanceCheckFireModePatch", () => new Patches.ActionStanceCheckFireModePatch());

        // Item 010: Manual Chambering
        SafeEnable("StartEquipWeapPatch", () => new Patches.StartEquipWeapPatch());
        SafeEnable("StartReloadResetPatch", () => new Patches.StartReloadResetPatch());
        SafeEnable("SetAmmoCompatiblePatch", () => new Patches.SetAmmoCompatiblePatch());
        SafeEnable("SetAmmoOnMagPatch", () => new Patches.SetAmmoOnMagPatch());
        SafeEnable("PreChamberLoadPatch", () => new Patches.PreChamberLoadPatch());
        SafeEnable("ManualChamberingInputPatch", () => new Patches.ManualChamberingInputPatch());

        // Carrega sprites do ícone de mount (reusados pelo novo item de mount; carregamento mantido).
        LoadedSprites["mounting.png"] = LoadEmbeddedSprite("mounting.png");
        LoadedSprites["mountingleft.png"] = LoadEmbeddedSprite("mountingleft.png");
        LoadedSprites["mountingright.png"] = LoadEmbeddedSprite("mountingright.png");

        // Item 011: UI do mount passivo no GameObject PERSISTENTE do plugin (mesmo padrão do OxygenUI).
        gameObject.AddComponent<PassiveMountUI>();

        // ========================================
        // MANUAL CHAMBERING
        // ========================================
        _EnableManualChambering = Config.Bind(
            "Manual Chambering Settings (Item 010)",
            "Enable Manual Chambering",
            true,
            new ConfigDescription("Master toggle do Manual Chambering. Desligado = comportamento vanilla em TODOS os cenários (kill-switch seguro). Puxe o ferrolho com a tecla nativa 'Chamber/Unload' (ECommand.ChamberUnload) quando a câmara estiver vazia e houver munição no carregador.",
            null,
            new ConfigurationManagerAttributes { Order = 70 }));

        _ManualChamberingOnRaidStart = Config.Bind(
            "Manual Chambering Settings (Item 010)",
            "Manual Chambering On Raid Start",
            true,
            new ConfigDescription(
                "Quando ativado, a arma que inicia a raid com a câmara vazia NÃO carrega a primeira bala automaticamente no spawn — puxe o ferrolho manualmente. Desligado = vanilla no início da raid. Efetivo na PRÓXIMA RAID.",
                null,
                new ConfigurationManagerAttributes { Order = 69 }));

        _ManualChamberingOnReload = Config.Bind(
            "Manual Chambering Settings (Item 010)",
            "Manual Chambering On Reload",
            true,
            new ConfigDescription(
                "Quando ativado, recarregar com a câmara vazia NÃO carrega automaticamente a primeira bala após inserir o carregador — puxe o ferrolho manualmente. Desligado = vanilla no reload. Tempo real.",
                null,
                new ConfigurationManagerAttributes { Order = 68 }));


        // ========================================
        // POSITIONS (Order 68-65)
        // ========================================
        _PositionEnabled = Config.Bind(
            Positions,
            "Enable Camera Position",
            true,
            new ConfigDescription("Enable or disable camera position offsets",
            null,
            new ConfigurationManagerAttributes { Order = 68 }));

        _ForwardBackwardOffset = Config.Bind(
            Positions,
            "Forward/Backward Offset",
            0f,
            new ConfigDescription("Camera position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 67 }));

        _UpDownOffset = Config.Bind(
            Positions,
            "Up/Down Offset",
            0.02f,
            new ConfigDescription("Camera position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 66 }));

        _SidewaysOffset = Config.Bind(
            Positions,
            "Sideways Offset",
            0f,
            new ConfigDescription("Camera position left/right (positive = right)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 65 }));

        // ========================================
        // SETTINGS (Order 65 → 48)
        // ========================================
        const string GeneralSection = "General";

        // backlog 002 F1 — substitui `Use Only Stances` (lógica invertida).
        _attrIncludeStance0 = new ConfigurationManagerAttributes { Order = 65 };
        _IncludeStance0InCycle = Config.Bind(
            Settings,
            "Include Stance 0 - Vanilla in Cycle",
            false,
            new ConfigDescription(
                "When enabled, Stance 0 (Vanilla) is included in the stance cycle. " +
                "Replaces the old `Use Only Stances` toggle (inverted logic, clearer naming). " +
                "Always affects the V key cycle; affects mouse scroll only when Mouse Wheel Scroll Mode = Cycle.",
                null,
                _attrIncludeStance0));

        // backlog 002 F2 — capturar atributos para mutar Browsable em runtime
        _attrEnableStance1Cycle = new ConfigurationManagerAttributes { Order = 64 };
        _EnableStance1 = Config.Bind(
            Settings,
            "Enable Stance 1 - High Ready in Cycle",
            true,
            new ConfigDescription("When enabled, Stance 1 is included in the stance cycle. When disabled, Stance 1 is skipped.",
            null,
            _attrEnableStance1Cycle));

        _attrEnableStance2Cycle = new ConfigurationManagerAttributes { Order = 63 };
        _EnableStance2 = Config.Bind(
            Settings,
            "Enable Stance 2 - Custom in Cycle",
            true,
            new ConfigDescription("When enabled, Stance 2 is included in the stance cycle. When disabled, Stance 2 is skipped.",
            null,
            _attrEnableStance2Cycle));

        _attrEnableStance3Cycle = new ConfigurationManagerAttributes { Order = 62 };
        _EnableStance3 = Config.Bind(
            Settings,
            "Enable Stance 3 - Low Ready in Cycle",
            true,
            new ConfigDescription("When enabled, Stance 3 is included in the stance cycle. When disabled, Stance 3 is skipped.",
            null,
            _attrEnableStance3Cycle));

        _StanceToggleKey = Config.Bind(
            Settings,
            "Stance Toggle Hotkey",
            KeyCode.V,
            new ConfigDescription("Press this key to cycle through enabled stances: Default → Stance 1 → Stance 2 → Stance 3 → Default",
            null,
            new ConfigurationManagerAttributes { Order = 61 }));

        _EnableMouseWheelCycle = Config.Bind(
            Settings,
            "Enable Mouse Wheel Stance Cycle",
            false,
            new ConfigDescription("When enabled, hold the modifier key and scroll mouse wheel to cycle stances",
            null,
            new ConfigurationManagerAttributes { Order = 60 }));

        _MouseWheelModifierKey = Config.Bind(
            Settings,
            "Mouse Wheel Modifier Key",
            KeyCode.LeftAlt,
            new ConfigDescription("Hold this key while scrolling mouse wheel to cycle stances (when mouse wheel cycling is enabled)",
            null,
            new ConfigurationManagerAttributes { Order = 59 }));

        // backlog 002 F2 — modo de scroll: Cycle (circular) ou Linear (eixo fixo).
        // Order 58 (slot livre por `Use Only Stances` removido) — evita colisão com ModifierKey (PA-04-01).
        _MouseWheelScrollMode = Config.Bind(
            Settings,
            "Mouse Wheel Scroll Mode",
            ScrollMode.Linear,
            new ConfigDescription(
                "Cycle = circular, respects per-stance cycle toggles. " +
                "Linear = fixed axis: Stance 1 (top) ↔ Stance 0 (center) ↔ Stance 2 (bottom); Stance 3 is off-axis (only via dedicated hotkey).",
                null,
                new ConfigurationManagerAttributes { Order = 58 }));

        // PA-03-04: SettingChanged via método nomeado (não lambda) — permite unsubscribe no OnDestroy.
        _MouseWheelScrollMode.SettingChanged  += OnScrollModeSettingChanged;
        _EnableMouseWheelCycle.SettingChanged += OnScrollModeSettingChanged;

        _StanceTransitionSpeed = Config.Bind(
            GeneralSection,
            "Stance Transition Speed",
            1.0f,
            new ConfigDescription("Speed multiplier for transitioning between stances and default view",
            new AcceptableValueRange<float>(0.1f, 5.0f),
            new ConfigurationManagerAttributes { Order = 98 }));

        _StanceKickIntensity = Config.Bind(
            GeneralSection,
            "Stance Kick Intensity (Contra o Peito)",
            -0.05f,
            new ConfigDescription("How much the weapon kicks towards your chest when changing stances or ADS. Negative values pull it towards you.",
            new AcceptableValueRange<float>(-0.3f, 0.3f),
            new ConfigurationManagerAttributes { Order = 97 }));

        _ADSKickDelay = Config.Bind(
            GeneralSection,
            "ADS Kick Delay (In)",
            0.15f,
            new ConfigDescription("Delay in seconds before the kick is applied when entering ADS. Use this to sync the kick with the end of the ADS animation.",
            new AcceptableValueRange<float>(0f, 1f),
            new ConfigurationManagerAttributes { Order = 96 }));

        _StanceOvershootDamping = Config.Bind(
            GeneralSection,
            "Stance Overshoot Damping (Menos gera Mais Quicada)",
            12.0f,
            new ConfigDescription("Damping for the spring physics. Lower values mean more overshoot/bounce. Default is 12.",
            new AcceptableValueRange<float>(1f, 30.0f),
            new ConfigurationManagerAttributes { Order = 95 }));

        _ADSTransitionSpeed = Config.Bind(
            Settings,
            "ADS Transition Speed",
            1f,
            new ConfigDescription("How quickly hands transition between stance and ADS positions. 1 = slow, 2 = normal, 3+ = fast/snappy.",
            new AcceptableValueRange<float>(0.5f, 5f),
            new ConfigurationManagerAttributes { Order = 56 }));

        _StanceChangeSoundVolume = Config.Bind(
            Settings,
            "Stance Change Sound Volume",
            1f,
            new ConfigDescription("Volume multiplier for the aim rattle sound when switching stances. 0 = muted, 1 = normal, 2 = louder.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { Order = 55 }));

        // ========================================
        // backlog 002 F3 — STANCE HOTKEYS DEDICADAS (Order 53 → 50)
        // ========================================
        _Stance0Hotkey = Config.Bind(
            Settings,
            "Stance 0 - Vanilla Hotkey",
            KeyCode.None,
            new ConfigDescription(
                "Dedicated key to return to Stance 0 - Vanilla. " +
                "Pressing again while already in Stance 0 has no effect (no toggle target). " +
                "Blocked during sprint and silently ignored while ADS.",
                null,
                new ConfigurationManagerAttributes { Order = 53 }));

        _Stance1Hotkey = Config.Bind(
            Settings,
            "Stance 1 - High Ready Hotkey",
            KeyCode.None,
            new ConfigDescription(
                "Dedicated key to activate Stance 1 - High Ready. " +
                "Toggle: pressing while already in Stance 1 returns to Stance 0. " +
                "Blocked during sprint and silently ignored while ADS.",
                null,
                new ConfigurationManagerAttributes { Order = 52 }));

        // 06-fix-02: label e tooltip atualizados após swap Stance 2 ↔ 3 (06-fix-01).
        // Stance 2 agora é Low Ready, então a hotkey label reflete isso.
        _Stance2Hotkey = Config.Bind(
            Settings,
            "Stance 2 - Low Ready Hotkey",
            KeyCode.None,
            new ConfigDescription(
                "Dedicated key to activate Stance 2 - Low Ready. " +
                "Toggle: pressing while already in Stance 2 returns to Stance 0. " +
                "Blocked during sprint and silently ignored while ADS.",
                null,
                new ConfigurationManagerAttributes { Order = 51 }));

        // 06-fix-02: label e tooltip atualizados após swap. Stance 3 agora é Custom.
        _Stance3Hotkey = Config.Bind(
            Settings,
            "Stance 3 - Custom Hotkey",
            KeyCode.O,
            new ConfigDescription(
                "Dedicated key to activate Stance 3 - Custom. " +
                "Toggle: pressing while already in Stance 3 returns to Stance 0. " +
                "Blocked during sprint and silently ignored while ADS.",
                null,
                new ConfigurationManagerAttributes { Order = 50 }));

        // backlog 002 F4 — limiar (ms) para classificar single-click vs hold.
        _SnapFireThreshold = Config.Bind(
            Settings,
            "Snap Fire Threshold (ms)",
            600,
            new ConfigDescription(
                "Maximum press-to-release time (in milliseconds) classified as a single click. " +
                "Single click = snap to Stance 0 without firing. " +
                "Held longer = snap + 1 natural shot (semi-auto/burst) or short fullauto burst (~1 shot at 600 RPM).",
                new AcceptableValueRange<int>(50, 1000),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 49 }));

        // ref: CR-01-06 — timeout do stale guard do snap (s). Advanced — só para troubleshooting.
        _SnapStaleTimeoutSec = Config.Bind(
            Settings,
            "Snap Stale Timeout (s)",
            2f,
            new ConfigDescription(
                "Maximum time (seconds) the snap intercept can stay active without a button-up before being auto-cleared. " +
                "Lower values reduce risk of stale state in rare weapon-swap-during-hold edge cases. Default 2s is safe.",
                new AcceptableValueRange<float>(0.5f, 10f),
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = 47 }));

        // backlog 002 F5 — iniciar raid em Stance 3 - Low Ready.
        _StartInLowReadyOnRaidBegin = Config.Bind(
            Settings,
            "Start In Low Ready On Raid Begin",
            true,
            new ConfigDescription(
                "When enabled, the player starts every raid already in Stance 3 - Low Ready, " +
                "with no transition animation (immediate set). Applies even if Stance 3 is excluded from the cycle.",
                null,
                new ConfigurationManagerAttributes { Order = 48 }));

        // ========================================
        // ADVANCED ADS TRANSITIONS (Order 55-40)
        // ========================================
        _EnableAdvancedADSTransitions = Config.Bind(
            AdvancedADSSettings,
            "Advanced ADS Transitions",
            false,
            new ConfigDescription("When enabled, weapon is thrown forward then pushed back when aiming to simulate shouldering",
            null,
            new ConfigurationManagerAttributes { Order = 55 }));



        // ========================================
        // ADS DEFAULT VALUES (Order 39-33)
        // ========================================
        _ResetOnADS = Config.Bind(
            ADSDefaults,
            "Reset Positions When Aiming",
            true,
            new ConfigDescription("When enabled, smoothly transitions all positions to defaults when ADS",
            null,
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 39 }));

        _ADSHandsPitchRotation = Config.Bind(
            ADSDefaults,
            "ADS Pitch (Cano Sobe/Desce)",
            0f,
            new ConfigDescription("Hands pitch rotation (X-axis) when ADS with 'Reset On ADS' enabled. 0 = default game position",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 38 }));

        _ADSHandsYawRotation = Config.Bind(
            ADSDefaults,
            "ADS Roll (Tombar Arma)",
            0f,
            new ConfigDescription("Hands yaw rotation (Y-axis) when ADS with 'Reset On ADS' enabled. 0 = default game position",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 37 }));

        _ADSHandsRollRotation = Config.Bind(
            ADSDefaults,
            "ADS Yaw (Apontar Esq/Dir)",
            0f,
            new ConfigDescription("Hands roll rotation (Z-axis) when ADS with 'Reset On ADS' enabled. 0 = default game position",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 36 }));

        _ADSHandsForwardBackwardOffset = Config.Bind(
            ADSDefaults,
            "ADS Forward/Backward (Frente/Trás)",
            0f,
            new ConfigDescription("Hands position forward/backward (Z-axis) when ADS. Default is 0.04",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 35 }));

        _ADSHandsUpDownOffset = Config.Bind(
            ADSDefaults,
            "ADS Up/Down (Coronha Sobe/Desce)",
            0f,
            new ConfigDescription("Hands position up/down (Y-axis) when ADS. Default is 0.04",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 34 }));

        _ADSHandsSidewaysOffset = Config.Bind(
            ADSDefaults,
            "ADS Sideways (Coronha Esq/Dir)",
            0f,
            new ConfigDescription("Hands position left/right (X-axis) when ADS. Default is 0.04",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 33 }));

        // ========================================
        // DEFAULT HANDS/ARMS POSITIONS (Order 32-29)
        // ========================================
        _DefaultHandsPositionEnabled = Config.Bind(
            DefaultHandsPositions,
            "Enable Default Hands/Arms Position",
            false,
            new ConfigDescription("Enable or disable default hands/arms position offsets when NOT in stance",
            null,
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 32 }));

        _DefaultHandsForwardBackwardOffset = Config.Bind(
            DefaultHandsPositions,
            "Default Forward/Backward (Frente/Trás)",
            0f,
            new ConfigDescription("Default hands/weapon position forward/backward (positive = forward). This is your normal hip-fire position.",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 31 }));

        _DefaultHandsUpDownOffset = Config.Bind(
            DefaultHandsPositions,
            "Default Up/Down (Coronha Sobe/Desce)",
            0f,
            new ConfigDescription("Default hands/weapon position up/down (positive = up). This is your normal hip-fire position.",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 30 }));

        _DefaultHandsSidewaysOffset = Config.Bind(
            DefaultHandsPositions,
            "Default Sideways (Coronha Esq/Dir)",
            0f,
            new ConfigDescription("Default hands/weapon position left/right (positive = right). This is your normal hip-fire position.",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 29 }));

        // ========================================
        // STANCE 0 - VANILLA (06-fix-03: descoberta cedo)
        // ========================================
        // ConfigurationManager ordena seções por ordem de descoberta (originalCategoryOrder.IndexOf,
        // ver ConfigurationManager.Shared/ConfigurationManager.cs:229 do BepInEx). Por isso a seção
        // Stance 0 - Vanilla precisa ser criada ANTES das hand rotations da Stance 1 — caso contrário
        // o foreach BindStance no fim do Awake faz Stance 0 ser a última seção descoberta, e ela
        // cai no fim do F12. Stance.Default não tem hand rotations (vanilla), então criamos só as
        // entries do BindStance aqui. As outras stances continuam no foreach abaixo.
        // O tentativa do 06-fix-02 (Order = 35 alto) não funciona porque Order só ordena entries
        // DENTRO de uma seção, não as seções entre si.
        _stanceConfigs[Stance.Default] = BindStance(_stanceDefaults[0]);

        // ========================================
        // STANCE 1 (Order 28-22)
        // ========================================
        _Stance1SprintAnimationEnabled = Config.Bind(
            Stance1Section,
            "Enable Stance 1 Sprint Animation",
            true,
            new ConfigDescription("When enabled, uses a compact sprint animation when sprinting in Stance 1 (tac sprint style)",
            null,
            new ConfigurationManagerAttributes { Order = 28 }));

        _Stance1HandsPitchRotation = Config.Bind(
            Stance1Section,
            "Stance 1 Pitch (Cano Sobe/Desce)",
            -34.0f,
            new ConfigDescription("Stance 1 hands/arms pitch rotation in degrees (up/down tilt)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 27 }));

        _Stance1HandsYawRotation = Config.Bind(
            Stance1Section,
            "Stance 1 Roll (Tombar Arma)",
            0.0f,
            new ConfigDescription("Stance 1 hands/arms yaw rotation in degrees (left/right turn)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 26 }));

        _Stance1HandsRollRotation = Config.Bind(
            Stance1Section,
            "Stance 1 Yaw (Apontar Esq/Dir)",
            0.0f,
            new ConfigDescription("Stance 1 hands/arms roll rotation in degrees (weapon cant)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 25 }));

        _Stance1HandsForwardBackwardOffset = Config.Bind(
            Stance1Section,
            "Stance 1 Forward/Backward (Frente/Trás)",
            0.02f,
            new ConfigDescription("Stance 1 hands/weapon position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 24 }));

        _Stance1HandsUpDownOffset = Config.Bind(
            Stance1Section,
            "Stance 1 Up/Down (Coronha Sobe/Desce)",
            -0.01f,
            new ConfigDescription("Stance 1 hands/weapon position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 23 }));

        _Stance1HandsSidewaysOffset = Config.Bind(
            Stance1Section,
            "Stance 1 Sideways (Coronha Esq/Dir)",
            0.02f,
            new ConfigDescription("Stance 1 hands/weapon position left/right (positive = right)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 22 }));

        // ========================================
        // STANCE 2 (Order 21-15)
        // ========================================
        _Stance2SprintAnimationEnabled = Config.Bind(
            Stance2Section,
            "Enable Stance 2 Sprint Animation",
            false,
            new ConfigDescription("When enabled, uses a compact sprint animation when sprinting in Stance 2 (tac sprint style)",
            null,
            new ConfigurationManagerAttributes { Order = 21 }));

        // 06-fix-01: Stance 2 é agora Low Ready — Pitch +30° (cano desce), Yaw 0.
        _Stance2HandsPitchRotation = Config.Bind(
            Stance2Section,
            "Stance 2 Pitch (Cano Sobe/Desce)",
            25.0f,
            new ConfigDescription("Stance 2 hands/arms pitch rotation in degrees (up/down tilt)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 20 }));

        _Stance2HandsYawRotation = Config.Bind(
            Stance2Section,
            "Stance 2 Roll (Tombar Arma)",
            0.0f,
            new ConfigDescription("Stance 2 hands/arms yaw rotation in degrees (left/right turn)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 19 }));

        _Stance2HandsRollRotation = Config.Bind(
            Stance2Section,
            "Stance 2 Yaw (Apontar Esq/Dir)",
            0.0f,
            new ConfigDescription("Stance 2 hands/arms roll rotation in degrees (weapon cant)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 18 }));

        _Stance2HandsForwardBackwardOffset = Config.Bind(
            Stance2Section,
            "Stance 2 Forward/Backward (Frente/Trás)",
            0.015f,
            new ConfigDescription("Stance 2 hands/weapon position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 17 }));

        _Stance2HandsUpDownOffset = Config.Bind(
            Stance2Section,
            "Stance 2 Up/Down (Coronha Sobe/Desce)",
            -0.02f,
            new ConfigDescription("Stance 2 hands/weapon position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 16 }));

        // ========================================
        // WEAPON MOUNT (PASSIVE) — Item 011
        // ========================================
        _EnablePassiveMount = Config.Bind(
            PassiveMountSettings,
            "Enable Passive Mount",
            true,
            new ConfigDescription("Liga o apoio passivo: ao encostar a arma numa superfície (sem a tecla de mount) você ganha um benefício leve de estabilidade. Desligado = só o mount nativo do jogo.",
            null,
            new ConfigurationManagerAttributes { Order = 10 }));

        _PassiveRecoilMultiplier = Config.Bind(
            PassiveMountSettings,
            "Passive Recoil Multiplier",
            0.7f,
            new ConfigDescription("Multiplicador de recuo enquanto apoiado (passivo). 0.7 = 30% menos recuo. Deve ser MAIOR que o do mount ativo (vanilla) — o passivo é mais fraco.",
            new AcceptableValueRange<float>(0.1f, 1f),
            new ConfigurationManagerAttributes { Order = 9 }));

        _PassiveSwayMultiplier = Config.Bind(
            PassiveMountSettings,
            "Passive Sway Multiplier",
            0.65f,
            new ConfigDescription("Multiplicador de sway (respiração) enquanto apoiado. 0.65 = 35% menos sway.",
            new AcceptableValueRange<float>(0f, 1f),
            new ConfigurationManagerAttributes { Order = 8 }));

        _PassiveStaminaSave = Config.Bind(
            PassiveMountSettings,
            "Passive Stamina Save",
            true,
            new ConfigDescription("Enquanto apoiado, pausa/reduz o drain de stamina de braço (mais fraco que o mount nativo).",
            null,
            new ConfigurationManagerAttributes { Order = 7 }));

        _ActiveMountStaminaRegen = Config.Bind(
            PassiveMountSettings,
            "Active Mount Stamina Regen",
            5f,
            new ConfigDescription("Taxa de recuperação de stamina de braço no mount ATIVO (vanilla), em hipfire. Em ADS usa a taxa do passivo (recupera leve). 0 = não recupera.",
            new AcceptableValueRange<float>(0f, 20f),
            new ConfigurationManagerAttributes { Order = 6 }));

        _PassiveMountStaminaRegen = Config.Bind(
            PassiveMountSettings,
            "Passive Mount Stamina Regen",
            2.5f,
            new ConfigDescription("Taxa de recuperação no mount PASSIVO em hipfire (e no ativo em ADS). No passivo + ADS a stamina fica parada (segura, sem recuperar). Deve ser MENOR que a do ativo.",
            new AcceptableValueRange<float>(0f, 20f),
            new ConfigurationManagerAttributes { Order = 5 }));

        _ShowMountIcon = Config.Bind(
            PassiveMountSettings,
            "Show Mount Icon",
            true,
            new ConfigDescription("Mostra o ícone direcional (esquerda/direita/baixo) no canto inferior direito quando o apoio passivo está ativo.",
            null,
            new ConfigurationManagerAttributes { Order = 6 }));

        const string HoldBreathSection = "9. Respiração (Hold Breath)";

        // ========================================
        // HOLD BREATH SETTINGS (Order -10)
        // ========================================
        _HoldBreathArmStaminaDrain = Config.Bind(
            HoldBreathSection,
            "Arm Stamina Drain / sec",
            2.0f,
            new ConfigDescription("How much extra Arm Stamina (HandsStamina) is drained per second while holding breath.",
            new AcceptableValueRange<float>(0f, 20f),
            new ConfigurationManagerAttributes { Order = -10 }));

        _HoldBreathOxygenDrain = Config.Bind(
            HoldBreathSection,
            "Oxygen Drain / sec",
            5.0f,
            new ConfigDescription("How much extra Oxygen is drained per second while holding breath.",
            new AcceptableValueRange<float>(0f, 50f),
            new ConfigurationManagerAttributes { Order = -11 }));

        _EnableCustomBreathAudio = Config.Bind(
            HoldBreathSection,
            "Enable Custom Breath Audio",
            true,
            new ConfigDescription("Plays custom breath_in.wav and breath_out.wav from the mod folder when holding breath.",
            null,
            new ConfigurationManagerAttributes { Order = -11 }));

        _BreathInVolume = Config.Bind(
            HoldBreathSection,
            "Breath In Volume",
            1.0f,
            new ConfigDescription("Volume of the breath_in audio.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { Order = -12 }));

        _BreathOutVolume = Config.Bind(
            HoldBreathSection,
            "Breath Out Volume",
            1.0f,
            new ConfigDescription("Volume of the breath_out audio.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { Order = -13 }));

        _HeartbeatVolume = Config.Bind(
            HoldBreathSection,
            "Heartbeat Volume",
            1.0f,
            new ConfigDescription("Volume of the heartbeat loop audio.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { Order = -14 }));

        // ========================================
        // OXYGEN UI SETTINGS (Order -13)
        // ========================================
        const string OxygenUISection = "10. Barra de Oxigênio (UI)";
        
        _EnableOxygenUI = Config.Bind(
            OxygenUISection,
            "Enable Oxygen UI Bar",
            true,
            new ConfigDescription("Displays a white bar above the hands stamina that drains while holding breath.",
            null,
            new ConfigurationManagerAttributes { Order = -1 }));

        _OxygenUIPosX = Config.Bind(
            OxygenUISection,
            "UI X Position",
            20f,
            new ConfigDescription("Horizontal position of the oxygen bar (pixels from left).",
            new AcceptableValueRange<float>(0f, 3000f),
            new ConfigurationManagerAttributes { Order = -2 }));

        _OxygenUIPosY = Config.Bind(
            OxygenUISection,
            "UI Y Position",
            120f,
            new ConfigDescription("Vertical position of the oxygen bar (pixels from BOTTOM).",
            new AcceptableValueRange<float>(0f, 2000f),
            new ConfigurationManagerAttributes { Order = -3 }));

        _OxygenUIWidth = Config.Bind(
            OxygenUISection,
            "UI Width",
            260f,
            new ConfigDescription("Width of the oxygen bar.",
            new AcceptableValueRange<float>(10f, 1000f),
            new ConfigurationManagerAttributes { Order = -4 }));

        _OxygenUIHeight = Config.Bind(
            OxygenUISection,
            "UI Height",
            4f,
            new ConfigDescription("Height (thickness) of the oxygen bar.",
            new AcceptableValueRange<float>(1f, 20f),
            new ConfigurationManagerAttributes { Order = -5 }));

        // ========================================
        // ANIMATION SETTINGS (Item 005)
        // ========================================
        _CrouchSpeedMultiplier = Config.Bind(
            AnimationSettings,
            "Crouch Speed Multiplier",
            1.5f,
            new ConfigDescription("Multiplier for crouch and prone animation speeds.",
            new AcceptableValueRange<float>(1f, 5f),
            new ConfigurationManagerAttributes { Order = 2 }));

        const string MovementSection = "Movement & Inertia";
        _InertiaMultiplier = Config.Bind(
            MovementSection,
            "Inertia Multiplier",
            1.2f,
            new ConfigDescription("Global multiplier for character inertia (weight feeling). 1.0 is default.",
            new AcceptableValueRange<float>(0.1f, 3.0f),
            new ConfigurationManagerAttributes { Order = 3 }));

        _WalkSpeedMultiplier = Config.Bind(
            MovementSection,
            "Walk Speed Multiplier",
            0.85f,
            new ConfigDescription("Multiplier for maximum walking speed. 1.0 is default.",
            new AcceptableValueRange<float>(0.1f, 2.0f),
            new ConfigurationManagerAttributes { Order = 2 }));

        _SprintSpeedMultiplier = Config.Bind(
            MovementSection,
            "Sprint Speed Multiplier",
            0.9f,
            new ConfigDescription("Multiplier for maximum sprinting speed. 1.0 is default.",
            new AcceptableValueRange<float>(0.1f, 2.0f),
            new ConfigurationManagerAttributes { Order = 1 }));

        const string ActionStanceSection = "Action Stances";
        _EnableActionStanceSwap = Config.Bind(
            ActionStanceSection,
            "Enable Action Stance Swap",
            true,
            new ConfigDescription("Levanta a arma para a Stance 0 (Pronto) automaticamente ao recarregar, checar munição/câmara, examinar a arma, checar modo de fogo e ESVAZIAR A CÂMARA — e retorna à postura anterior ao fim. Tempo real.",
            null,
            new ConfigurationManagerAttributes { Order = 1 }));

        const string StanceWiggleSection = "8. Wiggle (Q/E) Dynamics (Stance Based)";
        _CurveDuration = Config.Bind(
            StanceWiggleSection,
            "Animation Curve Duration",
            0.35f,
            new ConfigDescription("How long the cinematic stance transition takes to complete (seconds).",
            new AcceptableValueRange<float>(0.1f, 10.0f),
            new ConfigurationManagerAttributes { Order = 4 }));
            
        _StanceCurvePitchMultiplier = Config.Bind(
            StanceWiggleSection,
            "Stance Pitch Multiplier (Cano sobe/desce)",
            1.0f,
            new ConfigDescription("Multiplier for the X-axis (Pitch) sway curve during STANCE transitions.",
            new AcceptableValueRange<float>(0.0f, 5.0f),
            new ConfigurationManagerAttributes { Order = 3 }));

        _StanceCurveYawMultiplier = Config.Bind(
            StanceWiggleSection,
            "Stance Yaw Multiplier (Apontar Esq/Dir)",
            1.0f,
            new ConfigDescription("Multiplier for the Yaw sway curve during STANCE transitions.",
            new AcceptableValueRange<float>(0.0f, 5.0f),
            new ConfigurationManagerAttributes { Order = 2 }));

        _StanceCurveRollMultiplier = Config.Bind(
            StanceWiggleSection,
            "Stance Roll Multiplier (Tombar Arma)",
            1.0f,
            new ConfigDescription("Multiplier for the Roll sway curve during STANCE transitions.",
            new AcceptableValueRange<float>(0.0f, 5.0f),
            new ConfigurationManagerAttributes { Order = 1 }));

        _StanceCurvePositionMultiplier = Config.Bind(
            StanceWiggleSection,
            "Stance Position Multiplier (Coronha no peito)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway curves during STANCE transitions.",
            new AcceptableValueRange<float>(0.0f, 5.0f),
            new ConfigurationManagerAttributes { Order = 0 }));

        // --- Stance 0 (Vanilla) ADS Multipliers ---
        _Stance0ADSPitchMultiplier = Config.Bind(
            "Stance 0 - Vanilla",
            "ADS Pitch Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Pitch sway curve during ADS from Stance 0.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance0ADSYawMultiplier = Config.Bind(
            "Stance 0 - Vanilla",
            "ADS Yaw Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Yaw sway curve during ADS from Stance 0.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance0ADSRollMultiplier = Config.Bind(
            "Stance 0 - Vanilla",
            "ADS Roll Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Roll sway curve during ADS from Stance 0.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance0ADSPosYMultiplier = Config.Bind(
            "Stance 0 - Vanilla",
            "ADS Pos Y Multiplier (Forward/Back)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway (Y axis) during ADS from Stance 0.",
            new AcceptableValueRange<float>(-5.0f, 5.0f)));

        _Stance0ADSPosZMultiplier = Config.Bind(
            "Stance 0 - Vanilla",
            "ADS Pos Z Multiplier (Up/Down)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway (Z axis) during ADS from Stance 0.",
            new AcceptableValueRange<float>(-5.0f, 5.0f)));

        // --- Stance 1 ADS Multipliers ---
        _Stance1ADSPitchMultiplier = Config.Bind(
            Stance1Section,
            "ADS Pitch Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Pitch sway curve during ADS from Stance 1.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance1ADSYawMultiplier = Config.Bind(
            Stance1Section,
            "ADS Yaw Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Yaw sway curve during ADS from Stance 1.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance1ADSRollMultiplier = Config.Bind(
            Stance1Section,
            "ADS Roll Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Roll sway curve during ADS from Stance 1.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance1ADSPosYMultiplier = Config.Bind(
            Stance1Section,
            "ADS Pos Y Multiplier (Forward/Back)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway (Y axis) during ADS from Stance 1.",
            new AcceptableValueRange<float>(-5.0f, 5.0f)));

        _Stance1ADSPosZMultiplier = Config.Bind(
            Stance1Section,
            "ADS Pos Z Multiplier (Up/Down)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway (Z axis) during ADS from Stance 1.",
            new AcceptableValueRange<float>(-5.0f, 5.0f)));

        // --- Stance 2 ADS Multipliers ---
        _Stance2ADSPitchMultiplier = Config.Bind(
            Stance2Section,
            "ADS Pitch Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Pitch sway curve during ADS from Stance 2.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance2ADSYawMultiplier = Config.Bind(
            Stance2Section,
            "ADS Yaw Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Yaw sway curve during ADS from Stance 2.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance2ADSRollMultiplier = Config.Bind(
            Stance2Section,
            "ADS Roll Multiplier",
            1.0f,
            new ConfigDescription("Multiplier for the Roll sway curve during ADS from Stance 2.",
            new AcceptableValueRange<float>(0.0f, 5.0f)));

        _Stance2ADSPosYMultiplier = Config.Bind(
            Stance2Section,
            "ADS Pos Y Multiplier (Forward/Back)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway (Y axis) during ADS from Stance 2.",
            new AcceptableValueRange<float>(-5.0f, 5.0f)));

        _Stance2ADSPosZMultiplier = Config.Bind(
            Stance2Section,
            "ADS Pos Z Multiplier (Up/Down)",
            1.0f,
            new ConfigDescription("Multiplier for the positional sway (Z axis) during ADS from Stance 2.",
            new AcceptableValueRange<float>(-5.0f, 5.0f)));

        _LeanSpeedMultiplier = Config.Bind(
            AnimationSettings,
            "Lean Speed Multiplier",
            1.5f,
            new ConfigDescription("Multiplier for leaning (Q/E) speeds.",
            new AcceptableValueRange<float>(1f, 5f),
            new ConfigurationManagerAttributes { Order = 1 }));

        _Stance2HandsSidewaysOffset = Config.Bind(
            Stance2Section,
            "Stance 2 Sideways (Coronha Esq/Dir)",
            0.05f,
            new ConfigDescription("Stance 2 hands/weapon position left/right (positive = right)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 15 }));

        // ========================================
        // STANCE 3 (Order 14-8)
        // ========================================
        _Stance3SprintAnimationEnabled = Config.Bind(
            Stance3Section,
            "Enable Stance 3 Sprint Animation",
            false,
            new ConfigDescription("When enabled, uses a compact sprint animation when sprinting in Stance 3 (tac sprint style)",
            null,
            new ConfigurationManagerAttributes { Order = 14 }));

        // 06-fix-01: Stance 3 é agora Custom — Pitch 0, Yaw -30° (lateral).
        _Stance3HandsPitchRotation = Config.Bind(
            Stance3Section,
            "Stance 3 Pitch (Cano Sobe/Desce)",
            0f,
            new ConfigDescription("Stance 3 hands/arms pitch rotation in degrees (up/down tilt)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 13 }));

        _Stance3HandsYawRotation = Config.Bind(
            Stance3Section,
            "Stance 3 Roll (Tombar Arma)",
            -30f,
            new ConfigDescription("Stance 3 hands/arms yaw rotation in degrees (left/right turn)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 12 }));

        _Stance3HandsRollRotation = Config.Bind(
            Stance3Section,
            "Stance 3 Yaw (Apontar Esq/Dir)",
            0f,
            new ConfigDescription("Stance 3 hands/arms roll rotation in degrees (weapon cant)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 11 }));

        // 06-fix-01: Custom não tem push forward (lateral pura).
        _Stance3HandsForwardBackwardOffset = Config.Bind(
            Stance3Section,
            "Stance 3 Forward/Backward (Frente/Trás)",
            0f,
            new ConfigDescription("Stance 3 hands/weapon position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 10 }));

        _Stance3HandsUpDownOffset = Config.Bind(
            Stance3Section,
            "Stance 3 Up/Down (Coronha Sobe/Desce)",
            0f,
            new ConfigDescription("Stance 3 hands/weapon position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 9 }));

        _Stance3HandsSidewaysOffset = Config.Bind(
            Stance3Section,
            "Stance 3 Sideways (Coronha Esq/Dir)",
            0f,
            new ConfigDescription("Stance 3 hands/weapon position left/right (positive = right)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 8 }));

        // ========================================
        // TAC SPRINT SETTINGS (Order 7-4)
        // ========================================
        _TacSprintWeightLimit = Config.Bind(
            TacSprintSettings,
            "Tac Sprint Weight Limit",
            5.1f,
            new ConfigDescription("Maximum weapon weight (kg) to allow tac sprint animation. Default: 5.1kg",
            new AcceptableValueRange<float>(1f, 15f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 7 }));

        _TacSprintWeightLimitBullpup = Config.Bind(
            TacSprintSettings,
            "Tac Sprint Weight Limit (Bullpup)",
            5.75f,
            new ConfigDescription("Maximum weapon weight (kg) for bullpup weapons to allow tac sprint. Bullpups get a higher limit. Default: 5.75kg",
            new AcceptableValueRange<float>(1f, 15f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 6 }));

        _TacSprintLengthLimit = Config.Bind(
            TacSprintSettings,
            "Tac Sprint Length Limit",
            6,
            new ConfigDescription("Maximum weapon length (inventory cells) to allow tac sprint animation. Default: 6 cells",
            new AcceptableValueRange<int>(1, 10),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 5 }));

        _TacSprintErgoLimit = Config.Bind(
            TacSprintSettings,
            "Tac Sprint Ergo Limit",
            35f,
            new ConfigDescription("Minimum weapon ergonomics to allow tac sprint animation. Default: 35",
            new AcceptableValueRange<float>(0f, 100f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

        _TacSprintResetDelay = Config.Bind(
            TacSprintSettings,
            "Tac Sprint Reset Delay",
            0.35f,
            new ConfigDescription("Delay (seconds) after sprint ends before weapon returns to normal size. 0 = instant. Prevents jarring snap-back.",
            new AcceptableValueRange<float>(0f, 1f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

        // ========================================
        // FIELD OF VIEW (Order 3-1)
        // ========================================
        _FOVExpandEnabled = Config.Bind(
            FOVSettings,
            "Enable Expanded FOV Range",
            false,
            new ConfigDescription("Allows extending the FOV slider beyond the default 50-75 range",
            null,
            new ConfigurationManagerAttributes { Order = 3 }));

        _FOVMinRange = Config.Bind(
            FOVSettings,
            "Minimum FOV",
            20,
            new ConfigDescription("Minimum FOV value. Default game minimum is 50",
            new AcceptableValueRange<int>(1, 50),
            new ConfigurationManagerAttributes { Order = 2 }));

        _FOVMaxRange = Config.Bind(
            FOVSettings,
            "Maximum FOV",
            150,
            new ConfigDescription("Maximum FOV value. Default game maximum is 75",
            new AcceptableValueRange<int>(75, 170),
            new ConfigurationManagerAttributes { Order = 1 }));

        // ========================================
        // DEBUG (Order -1)
        // ========================================
        _DebugApplyInHideout = Config.Bind(
            DebugSettings,
            "Debug Apply In Hideout",
            false,
            new ConfigDescription(
                "Allows stamina/speed effects to run in the hideout (firing range). " +
                "Useful for offline testing. DISABLE for normal gameplay — this feature is designed for raids only.",
                null,
                new ConfigurationManagerAttributes { IsAdvanced = true, Order = -1 }));

        // ========================================
        // STANCE STAMINA + VELOCIDADE (backlog 001) — 5 props × 4 stances
        // ========================================
        // 06-fix-03: Stance.Default já foi criada cedo (antes do bloco STANCE 1) para garantir
        // ordem correta no F12. Aqui processamos apenas Stance 1/2/3.
        foreach (var d in _stanceDefaults)
        {
            if (d.Stance == Stance.Default) continue;
            _stanceConfigs[d.Stance] = BindStance(d);
        }

        foreach (var cfg in _stanceConfigs.Values)
        {
            cfg.StaminaMultiplier.SettingChanged       += OnStanceConfigChanged;
            cfg.ModifiesMovementSpeed.SettingChanged   += OnStanceConfigChanged;
            cfg.MovementSpeedMultiplier.SettingChanged += OnStanceConfigChanged;
            cfg.ApplyWhenProne.SettingChanged          += OnStanceConfigChanged;
        }

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

        // Initialize StanceManager
        StanceManager.Initialize(_StanceToggleKey);

        // Subscribe to config changes for camera offset settings
        _PositionEnabled.SettingChanged += (_, __) => MarkCameraOffsetDirty();
        _SidewaysOffset.SettingChanged += (_, __) => MarkCameraOffsetDirty();
        _UpDownOffset.SettingChanged += (_, __) => MarkCameraOffsetDirty();
        _ForwardBackwardOffset.SettingChanged += (_, __) => MarkCameraOffsetDirty();
        
        // Subscribe to config changes for stance cached values
        _ResetOnADS.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _DefaultHandsPositionEnabled.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _ADSHandsPitchRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _ADSHandsYawRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _ADSHandsRollRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _ADSHandsForwardBackwardOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _ADSHandsUpDownOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _ADSHandsSidewaysOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _DefaultHandsForwardBackwardOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _DefaultHandsUpDownOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _DefaultHandsSidewaysOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance1HandsPitchRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance1HandsYawRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance1HandsRollRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance1HandsForwardBackwardOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance1HandsUpDownOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance1HandsSidewaysOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance2HandsPitchRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance2HandsYawRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance2HandsRollRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance2HandsForwardBackwardOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance2HandsUpDownOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance2HandsSidewaysOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance3HandsPitchRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance3HandsYawRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance3HandsRollRotation.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance3HandsForwardBackwardOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance3HandsUpDownOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        _Stance3HandsSidewaysOffset.SettingChanged += (_, __) => StanceManager.MarkStanceValuesDirty();
        
        // Subscribe to config changes for sprint animation cached values
        _Stance1SprintAnimationEnabled.SettingChanged += (_, __) => StanceManager.MarkSprintEnabledDirty();
        _Stance2SprintAnimationEnabled.SettingChanged += (_, __) => StanceManager.MarkSprintEnabledDirty();
        _Stance3SprintAnimationEnabled.SettingChanged += (_, __) => StanceManager.MarkSprintEnabledDirty();

        _CrouchSpeedMultiplier.SettingChanged += (_, __) => ApplyMovementSpeeds();
        _LeanSpeedMultiplier.SettingChanged += (_, __) => ApplyMovementSpeeds();

        // Initialize FOV bounds


        // Note: RotationEvents and SetItemInHandsPatch are deprecated now
        // All logic is handled by StanceManager + SpringGetPatch

        // backlog 002 F2 — visibilidade inicial dos toggles de ciclo conforme ScrollMode atual.
        RefreshScrollModeVisibility();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InitFikaSync()
    {
        CameraRotationMod.Networking.FikaSyncManager.Initialize(Logger);
    }

    // ==========================================================================
    // backlog 002 F2/F4 — helpers de Plugin
    // ==========================================================================

    /// <summary>
    /// PA-03-04: handler nomeado (não lambda) para subscribe/unsubscribe explícito.
    /// </summary>
    private static void OnScrollModeSettingChanged(object sender, EventArgs args)
        => RefreshScrollModeVisibility();

    /// <summary>
    /// F2 — atualiza Browsable das ConfigEntries dependentes do modo de scroll
    /// e força repaint do ConfigurationManager (se disponível). PA-01-03 + PA-03-04.
    /// </summary>
    private static void RefreshScrollModeVisibility()
    {
        bool wheelEnabled = _EnableMouseWheelCycle?.Value ?? false;
        bool isCycle = wheelEnabled && _MouseWheelScrollMode?.Value == ScrollMode.Cycle;

        // Em modo Linear (ou wheel desabilitado), os toggles de ciclo ficam ocultos.
        if (_attrIncludeStance0 != null)     _attrIncludeStance0.Browsable = isCycle;
        if (_attrEnableStance1Cycle != null) _attrEnableStance1Cycle.Browsable = isCycle;
        if (_attrEnableStance2Cycle != null) _attrEnableStance2Cycle.Browsable = isCycle;
        if (_attrEnableStance3Cycle != null) _attrEnableStance3Cycle.Browsable = isCycle;

        // Forçar redesenho do CM em tempo real — só se cache disponível (degradação graciosa).
        if (!_cmRefreshAvailable) return;
        try { _cmBuildSettingListMethod.Invoke(_cmInstance, null); }
        catch (Exception ex) { Logger.LogError($"[F2] BuildSettingList falhou: {ex}"); }
    }

    /// <summary>
    /// PA-01-03: cachear MethodInfo + instância do ConfigurationManager para forçar repaint
    /// do F12 quando Browsable muda. Soft dependency — se ausente, log info e segue.
    /// </summary>
    private static void TryResolveConfigurationManager()
    {
        var tCM = AccessTools.TypeByName("ConfigurationManager.ConfigurationManager");
        if (tCM == null)
        {
            Logger.LogInfo("[F2] ConfigurationManager não detectado — visibilidade dinâmica desabilitada (atualiza ao reabrir F12).");
            return;
        }
        _cmBuildSettingListMethod = AccessTools.Method(tCM, "BuildSettingList");
        _cmInstance = UnityEngine.Object.FindObjectOfType(tCM);
        if (_cmBuildSettingListMethod == null || _cmInstance == null)
        {
            Logger.LogWarning("[F2] ConfigurationManager presente mas BuildSettingList não resolvido — visibilidade só atualiza ao reabrir F12.");
            return;
        }
        _cmRefreshAvailable = true;
    }

    /// <summary>
    /// 06-fix-01: resolver Player.FirearmController.SetTriggerPressed (NÃO operation-base).
    /// ref: Player.cs:13668 — virtual público em Player.FirearmController que roteia para
    /// CurrentOperation.SetTriggerPressed.
    ///
    /// Por que NÃO a operation-base (estratégia anterior PA-01-01): C# virtual dispatch executa
    /// o IL do override diretamente; o Prefix patcheado na base só dispara se o override chamar
    /// `base.SetTriggerPressed()`. No Assembly, apenas 1 de 14 overrides chama base
    /// (Player.cs:3184). Os outros 13 (incluindo o default weapon op em Player.cs:2712 — caminho
    /// mais comum) NÃO chamam base, então o Prefix nunca disparava. Patchar a FC routing method
    /// (linha 13668) captura todos os caminhos ANTES da virtual dispatch.
    ///
    /// DeclaredMethod garante que pegamos o método declarado na FC (não inherited).
    /// Fallback Method (busca inherited) para o caso raro do decompilador estar errado.
    /// </summary>
    private static void ResolveFirearmControllerSetTrigger()
    {
        FirearmControllerSetTrigger = AccessTools.DeclaredMethod(
            typeof(Player.FirearmController),
            "SetTriggerPressed",
            new[] { typeof(bool) });

        if (FirearmControllerSetTrigger == null)
        {
            Logger.LogWarning("[F4] DeclaredMethod falhou — tentando Method (busca inherited).");
            FirearmControllerSetTrigger = AccessTools.Method(
                typeof(Player.FirearmController),
                "SetTriggerPressed",
                new[] { typeof(bool) });
        }

        if (FirearmControllerSetTrigger == null)
            Logger.LogWarning("[F4] Player.FirearmController.SetTriggerPressed não resolvida — F4 desabilitado.");
        else
            Logger.LogInfo($"[F4] Resolved {FirearmControllerSetTrigger.DeclaringType?.FullName}.SetTriggerPressed");
    }

    /// <summary>
    /// Habilita um ModulePatch de forma isolada: se o target Harmony não resolver (GClass volátil,
    /// method_NN renomeado em 0.16), loga `[enable] FAIL <label>` e segue — em vez de lançar e abortar
    /// o Awake, o que derrubaria todos os patches registrados depois.
    /// </summary>
    private static void SafeEnable(string label, Func<SPT.Reflection.Patching.ModulePatch> factory)
    {
        try { factory().Enable(); Logger.LogInfo($"[enable] OK   {label}"); }
        catch (Exception ex) { Logger.LogWarning($"[enable] FAIL {label} -> {ex.GetType().Name}: {ex.Message}"); }
    }

    /// <summary>
    /// PA-03-04: unsubscribe explícito dos handlers de SettingChanged em hot-reload.
    /// </summary>
    public static void ApplyMovementSpeeds()
    {
        if (EFTHardSettings.Instance != null)
        {
            // O padrão do Tarkov é 3f para POSE e 10f para TILT
            EFTHardSettings.Instance.POSE_CHANGING_SPEED = 3f * _CrouchSpeedMultiplier.Value;
            EFTHardSettings.Instance.TILT_CHANGING_SPEED = 10f * _LeanSpeedMultiplier.Value;
        }
    }

    private void OnDestroy()
    {
        if (_MouseWheelScrollMode != null)  _MouseWheelScrollMode.SettingChanged  -= OnScrollModeSettingChanged;
        if (_EnableMouseWheelCycle != null) _EnableMouseWheelCycle.SettingChanged -= OnScrollModeSettingChanged;
    }

    private void FixedUpdate()
    {

    }

    // Cached camera offset state to avoid setting every frame
    private static Vector3 _lastCameraOffset = new Vector3(0.04f, 0.04f, 0.04f);
    private static bool _cameraOffsetDirty = true;
    private static readonly Vector3 DefaultCameraOffset = new Vector3(0.04f, 0.04f, 0.04f);
    
    /// <summary>
    /// Mark camera offset as needing update (called when config changes)
    /// </summary>
    public static void MarkCameraOffsetDirty() => _cameraOffsetDirty = true;

    // Update is called every frame by Unity
    public void Update()
    {
        // Validação antiga de cache removida
        
        StanceManager.Update();
        StanceManager.UpdateTacSprint();
        StanceManager.TickStanceStamina();              // backlog 001: delta stamina (drain/recovery) em hipfire
        StanceManager.EvaluateProneSuspensionTick();    // backlog 001: prone toggle + refresh defensivo de speed limit
        UpdateCameraOffset();
    }
    
    private StanceConfig BindStance(
        (Stance Stance, string Section, float StaminaMultiplier, bool ModSpeed, int Multiplier, bool ApplyProne, bool SnapOnFire) d)
    {
        // Map Stance → label numérico para os nomes das ConfigEntry (Default=0, Stance1=1, ...)
        int n = (int)d.Stance;

        // Order só ordena entries DENTRO de uma seção (ConfigurationManager.Shared/
        // ConfigurationManager.cs:233 do BepInEx). Para ordenação de SEÇÕES, ver 06-fix-03 —
        // a seção Stance 0 - Vanilla é criada cedo (antes do bloco STANCE 1) porque
        // ConfigurationManager usa ordem de descoberta (originalCategoryOrder.IndexOf).
        // 06-fix-02 tinha bumpado para 35 numa tentativa errada — Order não muda ordem de seções.
        int orderBase = 5;

        return new StanceConfig
        {
            StaminaMultiplier = Config.Bind(d.Section, $"Stance {n} Stamina Multiplier", d.StaminaMultiplier,
                new ConfigDescription(
                    "Controls stamina behavior for this stance. " +
                    "< 1.0 = drain (e.g. 0.5 drains at half ADS rate). " +
                    "1.0 = vanilla, no effect. " +
                    "> 1.0 = accelerated recovery (e.g. 2.0 recovers at full ADS drain rate).",
                    new AcceptableValueRange<float>(0f, 10f),
                    new ConfigurationManagerAttributes { Order = orderBase })),
            ModifiesMovementSpeed = Config.Bind(d.Section, $"Stance {n} Modifies Movement Speed", d.ModSpeed,
                new ConfigDescription(
                    "When enabled, this stance applies a movement speed cap.",
                    null,
                    new ConfigurationManagerAttributes { Order = orderBase - 2 })),
            MovementSpeedMultiplier = Config.Bind(d.Section, $"Stance {n} Movement Speed Multiplier", d.Multiplier,
                new ConfigDescription(
                    "Speed cap in %. 50 = half speed, 75 = slightly slower, 100 = no reduction. Only reduction is supported (EFT speed limit system limitation).",
                    new AcceptableValueRange<int>(50, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = orderBase - 3 })),
            ApplyWhenProne = Config.Bind(d.Section, $"Stance {n} Apply When Prone", d.ApplyProne,
                new ConfigDescription(
                    "Apply this stance effects (drain/recovery and speed cap) while prone. Disabled by default as it may conflict with EFT prone animations.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = orderBase - 4 })),
            // backlog 002 F4 — Stance 0 NÃO recebe ConfigEntry (sentinel null). O guard
            // `if (CurrentStance == Stance.Default) return false;` em StanceManager.TryInterceptTriggerDown
            // já cobre. Isto evita exibir uma entry inútil na seção `Stance 0 - Vanilla` do F12.
            SnapToStance0OnFire = (d.Stance == Stance.Default)
                ? null
                : Config.Bind(d.Section, $"Stance {n} Snap to Stance 0 on Fire", d.SnapOnFire,
                    new ConfigDescription(
                        "When enabled, firing while in this stance snaps to Stance 0 - Vanilla. " +
                        "Single click (< Snap Fire Threshold) = no shot. Hold (>= threshold) = snap + 1 natural shot. " +
                        "Does not trigger in ADS or with non-firearm items.",
                        null,
                        new ConfigurationManagerAttributes { Order = 0 })),
        };
    }

    private void Start()
    {
        // Áudio do hold-breath é carregado em GameWorld.OnGameStarted (HoldBreathPatch.OnRaidStart),
        // não aqui — carregar na cena de menu faz os clips serem descarregados na transição p/ o jogo.
    }

    /// <summary>SettingChanged handler — marca config suja; tick re-aplica numa janela ≤ 1 frame.</summary>
    private static void OnStanceConfigChanged(object sender, EventArgs e)
    {
        StanceManager.MarkStaminaConfigDirty();
    }

    private void UpdateCameraOffset()
    {
        // Early exit if nothing changed
        if (!_cameraOffsetDirty)
            return;
            
        var gameWorld = StanceManager.GetCachedGameWorld();
        if (gameWorld?.MainPlayer?.ProceduralWeaponAnimation?.HandsContainer == null)
            return;
            
        var handsContainer = gameWorld.MainPlayer.ProceduralWeaponAnimation.HandsContainer;
        
        bool isEnabled = _PositionEnabled?.Value ?? false;
        
        Vector3 targetOffset = isEnabled ? new Vector3(
            _SidewaysOffset.Value,
            _UpDownOffset.Value,
            _ForwardBackwardOffset.Value
        ) : DefaultCameraOffset;
        
        // Only update if actually changed
        if (targetOffset != _lastCameraOffset)
        {
            handsContainer.CameraOffset = targetOffset;
            _lastCameraOffset = targetOffset;
        }
        
        _cameraOffsetDirty = false;
    }

    public static Sprite LoadEmbeddedSprite(string resourceName)
    {
        try
        {
            var assemblyDir = System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
            string fullPath = System.IO.Path.Combine(assemblyDir, resourceName);
            if (!System.IO.File.Exists(fullPath))
            {
                Logger.LogError($"[ResourceLoader] File '{fullPath}' not found!");
                return null;
            }
            byte[] data = System.IO.File.ReadAllBytes(fullPath);
            
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(data))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[ResourceLoader] Error loading file '{resourceName}': {ex}");
        }
        return null;
    }
}
