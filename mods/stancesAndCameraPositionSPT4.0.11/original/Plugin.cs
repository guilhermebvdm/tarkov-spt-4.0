using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CameraRotationMod.Patches;
using UnityEngine;

namespace CameraRotationMod;

[BepInPlugin("shwng.camerarotation", "shwng.FpsCameraStances", "1.1.0")]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;

    // Section constants (in display order - top to bottom)
    private const string Positions = "Positions";
    private const string Settings = "Settings";
    private const string AdvancedADSSettings = "Advanced ADS Transitions";
    private const string ADSDefaults = "ADS Default Values (Advanced)";
    private const string DefaultHandsPositions = "Default Hands/Arms Positions (Advanced)";
    private const string Stance1Section = "Stance 1";
    private const string Stance2Section = "Stance 2";
    private const string Stance3Section = "Stance 3";
    private const string TacSprintSettings = "Tac Sprint Settings (Advanced)";
    private const string FOVSettings = "Field of View";

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
    public static ConfigEntry<bool> _UseOnlyStances;
    public static ConfigEntry<float> _StanceTransitionSpeed;
    public static ConfigEntry<float> _ADSTransitionSpeed;

    // Advanced ADS Transitions
    public static ConfigEntry<bool> _EnableAdvancedADSTransitions;
    public static ConfigEntry<bool> _AffectStanceTransitionToo;
    public static ConfigEntry<float> _StanceChangeSoundVolume;
    public static ConfigEntry<float> _ADSShoulderThrowIntensity;
    public static ConfigEntry<float> _StanceShoulderThrowIntensity;
    public static ConfigEntry<bool> _ScaleByWeaponStats;
    public static ConfigEntry<float> _WeaponStatsScaleIntensity;
    public static ConfigEntry<float> _AdvancedStanceTransitionIntensity;
    public static ConfigEntry<float> _ADSShoulderThrowForward;
    public static ConfigEntry<float> _ADSShoulderThrowUp;
    public static ConfigEntry<float> _ADSShoulderThrowYaw;
    public static ConfigEntry<float> _ADSShoulderThrowPitch;
    public static ConfigEntry<float> _ADSShoulderThrowRoll;
    public static ConfigEntry<float> _ADSShoulderThrowSpeed;
    public static ConfigEntry<float> _ADSShoulderSettleSpeed;
    public static ConfigEntry<float> _ADSShoulderThrowDuration;

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

    // FOV Settings
    public static ConfigEntry<bool> _FOVExpandEnabled;
    public static ConfigEntry<int> _FOVMinRange;
    public static ConfigEntry<int> _FOVMaxRange;

    public void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Camera Rotation Mod has loaded!");

        // Enable patches
        new PlayerSpringPatch().Enable(); // Handles camera position
        new SpringGetPatch().Enable(); // Handles stance rotation/position transitions
        new FOVSliderPatch().Enable(); // Extends FOV slider range in settings
        new FOVClampPatch().Enable(); // Allows FOV values outside default 50-75 range

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
        // SETTINGS (Order 64-56)
        // ========================================
        _EnableStance1 = Config.Bind(
            Settings,
            "Enable Stance 1 in Cycle",
            true,
            new ConfigDescription("When enabled, Stance 1 is included in the stance cycle. When disabled, Stance 1 is skipped.",
            null,
            new ConfigurationManagerAttributes { Order = 64 }));

        _EnableStance2 = Config.Bind(
            Settings,
            "Enable Stance 2 in Cycle",
            true,
            new ConfigDescription("When enabled, Stance 2 is included in the stance cycle. When disabled, Stance 2 is skipped.",
            null,
            new ConfigurationManagerAttributes { Order = 63 }));

        _EnableStance3 = Config.Bind(
            Settings,
            "Enable Stance 3 in Cycle",
            true,
            new ConfigDescription("When enabled, Stance 3 is included in the stance cycle. When disabled, Stance 3 is skipped.",
            null,
            new ConfigurationManagerAttributes { Order = 62 }));

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

        _UseOnlyStances = Config.Bind(
            Settings,
            "Use Only Stances",
            true,
            new ConfigDescription("When enabled, cycle skips Default (non-stance) position and only cycles through enabled stances",
            null,
            new ConfigurationManagerAttributes { Order = 58 }));

        _StanceTransitionSpeed = Config.Bind(
            Settings,
            "Stance Transition Speed",
            1f,
            new ConfigDescription("How quickly hands transition between Default and Stance. Higher = faster/snappier, Lower = slower/smoother. Recommended: 3-10",
            new AcceptableValueRange<float>(0.5f, 20f),
            new ConfigurationManagerAttributes { Order = 57 }));

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
        // ADVANCED ADS TRANSITIONS (Order 55-40)
        // ========================================
        _EnableAdvancedADSTransitions = Config.Bind(
            AdvancedADSSettings,
            "Advanced ADS Transitions",
            false,
            new ConfigDescription("When enabled, weapon is thrown forward then pushed back when aiming to simulate shouldering",
            null,
            new ConfigurationManagerAttributes { Order = 55 }));

        _AffectStanceTransitionToo = Config.Bind(
            AdvancedADSSettings,
            "Affect Stance Transition Too",
            true,
            new ConfigDescription("When enabled (requires 'Advanced ADS Transitions'), applies the same shouldering effect when switching between stances",
            null,
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 54 }));

        _ADSShoulderThrowIntensity = Config.Bind(
            AdvancedADSSettings,
            "ADS Shoulder Throw Intensity",
            1f,
            new ConfigDescription("Overall intensity of ADS throw effect. Multiplies Forward, Up, Yaw, Pitch, Roll amounts. 0 = no throw, 1 = use config values, 2 = double effect.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 52 }));

        _StanceShoulderThrowIntensity = Config.Bind(
            AdvancedADSSettings,
            "Stance Shoulder Throw Intensity",
            0.75f,
            new ConfigDescription("Overall intensity of stance switch throw effect. Multiplies Forward, Up, Yaw, Pitch, Roll amounts. 0 = no throw, 1 = use config values, 2 = double effect.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 51 }));

        _ScaleByWeaponStats = Config.Bind(
            AdvancedADSSettings,
            "Scale by Weapon Stats",
            true,
            new ConfigDescription("When enabled, shouldering speed/duration/amount scales with weapon weight and ergonomics (uses EFT's AimingSpeed calculation). Heavy/low-ergo = slower, dramatic. Light/high-ergo = fast, subtle.",
            null,
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 50 }));

        _WeaponStatsScaleIntensity = Config.Bind(
            AdvancedADSSettings,
            "Advanced ADS Transition Stat Intensity",
            1f,
            new ConfigDescription("How strongly weapon stats affect ADS shouldering. 0 = no scaling (all weapons same), 1 = normal, 2 = exaggerated difference between light/heavy weapons.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 49 }));

        _AdvancedStanceTransitionIntensity = Config.Bind(
            AdvancedADSSettings,
            "Advanced Stance Transition Stat Intensity",
            1f,
            new ConfigDescription("How strongly weapon weight/ergonomics affects stance transition speed and shouldering. 0.01 = minimal effect, 1 = normal, 2 = exaggerated. Works when 'Affect Stance Transition Too' is enabled.",
            new AcceptableValueRange<float>(0.01f, 2f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 48 }));

        _ADSShoulderThrowForward = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Forward Amount",
            0.02f,
            new ConfigDescription("Base forward throw distance. With 'Scale by Weapon Stats' enabled, this is multiplied by inverse AimingSpeed (heavy weapons throw more).",
            new AcceptableValueRange<float>(0f, 0.3f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 47 }));

        _ADSShoulderThrowUp = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Up Amount",
            -0.015f,
            new ConfigDescription("Base vertical offset during throw. Negative = down. With 'Scale by Weapon Stats', scales with inverse AimingSpeed.",
            new AcceptableValueRange<float>(-0.15f, 0.15f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 46 }));

        _ADSShoulderThrowYaw = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Yaw",
            6f,
            new ConfigDescription("Yaw rotation during throw phase (degrees). Positive = rotate right. Applied to both ADS and stance transitions.",
            new AcceptableValueRange<float>(-15f, 15f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 45 }));

        _ADSShoulderThrowPitch = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Pitch",
            -3f,
            new ConfigDescription("Pitch rotation during throw phase (degrees). Positive = rotate up. Applied to both ADS and stance transitions.",
            new AcceptableValueRange<float>(-15f, 15f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 44 }));

        _ADSShoulderThrowRoll = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Roll",
            -1.5f,
            new ConfigDescription("Roll rotation during throw phase (degrees). Positive = tilt right. Applied to both ADS and stance transitions.",
            new AcceptableValueRange<float>(-15f, 15f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 43 }));

        _ADSShoulderThrowSpeed = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Speed",
            2f,
            new ConfigDescription("Base speed of throw motion. With 'Scale by Weapon Stats', multiplied by AimingSpeed (light weapons = faster).",
            new AcceptableValueRange<float>(0.5f, 5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 42 }));

        _ADSShoulderSettleSpeed = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Settle Speed",
            1.5f,
            new ConfigDescription("Base speed of settling to ADS. With 'Scale by Weapon Stats', multiplied by AimingSpeed.",
            new AcceptableValueRange<float>(0.5f, 5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 41 }));

        _ADSShoulderThrowDuration = Config.Bind(
            AdvancedADSSettings,
            "Shoulder Throw Duration",
            0.15f,
            new ConfigDescription("Base throw phase duration (seconds). With 'Scale by Weapon Stats', heavy weapons have longer duration.",
            new AcceptableValueRange<float>(0.01f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 40 }));

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
            "ADS Hands Pitch Rotation",
            0f,
            new ConfigDescription("Hands pitch rotation (X-axis) when ADS with 'Reset On ADS' enabled. 0 = default game position",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 38 }));

        _ADSHandsYawRotation = Config.Bind(
            ADSDefaults,
            "ADS Hands Yaw Rotation",
            0f,
            new ConfigDescription("Hands yaw rotation (Y-axis) when ADS with 'Reset On ADS' enabled. 0 = default game position",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 37 }));

        _ADSHandsRollRotation = Config.Bind(
            ADSDefaults,
            "ADS Hands Roll Rotation",
            0f,
            new ConfigDescription("Hands roll rotation (Z-axis) when ADS with 'Reset On ADS' enabled. 0 = default game position",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 36 }));

        _ADSHandsForwardBackwardOffset = Config.Bind(
            ADSDefaults,
            "ADS Hands Forward/Backward Offset",
            0f,
            new ConfigDescription("Hands position forward/backward (Z-axis) when ADS. Default is 0.04",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 35 }));

        _ADSHandsUpDownOffset = Config.Bind(
            ADSDefaults,
            "ADS Hands Up/Down Offset",
            0f,
            new ConfigDescription("Hands position up/down (Y-axis) when ADS. Default is 0.04",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 34 }));

        _ADSHandsSidewaysOffset = Config.Bind(
            ADSDefaults,
            "ADS Hands Sideways Offset",
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
            "Default Hands Forward/Backward Offset",
            0f,
            new ConfigDescription("Default hands/weapon position forward/backward (positive = forward). This is your normal hip-fire position.",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 31 }));

        _DefaultHandsUpDownOffset = Config.Bind(
            DefaultHandsPositions,
            "Default Hands Up/Down Offset",
            0f,
            new ConfigDescription("Default hands/weapon position up/down (positive = up). This is your normal hip-fire position.",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 30 }));

        _DefaultHandsSidewaysOffset = Config.Bind(
            DefaultHandsPositions,
            "Default Hands Sideways Offset",
            0f,
            new ConfigDescription("Default hands/weapon position left/right (positive = right). This is your normal hip-fire position.",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { IsAdvanced = true, Order = 29 }));

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
            "Stance 1 Hands Pitch (X-Axis)",
            -15f,
            new ConfigDescription("Stance 1 hands/arms pitch rotation in degrees (up/down tilt)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 27 }));

        _Stance1HandsYawRotation = Config.Bind(
            Stance1Section,
            "Stance 1 Hands Yaw (Y-Axis)",
            -15f,
            new ConfigDescription("Stance 1 hands/arms yaw rotation in degrees (left/right turn)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 26 }));

        _Stance1HandsRollRotation = Config.Bind(
            Stance1Section,
            "Stance 1 Hands Roll (Z-Axis)",
            0f,
            new ConfigDescription("Stance 1 hands/arms roll rotation in degrees (weapon cant)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 25 }));

        _Stance1HandsForwardBackwardOffset = Config.Bind(
            Stance1Section,
            "Stance 1 Hands Forward/Backward Offset",
            -0.15f,
            new ConfigDescription("Stance 1 hands/weapon position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 24 }));

        _Stance1HandsUpDownOffset = Config.Bind(
            Stance1Section,
            "Stance 1 Hands Up/Down Offset",
            0f,
            new ConfigDescription("Stance 1 hands/weapon position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 23 }));

        _Stance1HandsSidewaysOffset = Config.Bind(
            Stance1Section,
            "Stance 1 Hands Sideways Offset",
            0f,
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

        _Stance2HandsPitchRotation = Config.Bind(
            Stance2Section,
            "Stance 2 Hands Pitch (X-Axis)",
            0f,
            new ConfigDescription("Stance 2 hands/arms pitch rotation in degrees (up/down tilt)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 20 }));

        _Stance2HandsYawRotation = Config.Bind(
            Stance2Section,
            "Stance 2 Hands Yaw (Y-Axis)",
            -30f,
            new ConfigDescription("Stance 2 hands/arms yaw rotation in degrees (left/right turn)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 19 }));

        _Stance2HandsRollRotation = Config.Bind(
            Stance2Section,
            "Stance 2 Hands Roll (Z-Axis)",
            0f,
            new ConfigDescription("Stance 2 hands/arms roll rotation in degrees (weapon cant)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 18 }));

        _Stance2HandsForwardBackwardOffset = Config.Bind(
            Stance2Section,
            "Stance 2 Hands Forward/Backward Offset",
            0f,
            new ConfigDescription("Stance 2 hands/weapon position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 17 }));

        _Stance2HandsUpDownOffset = Config.Bind(
            Stance2Section,
            "Stance 2 Hands Up/Down Offset",
            0f,
            new ConfigDescription("Stance 2 hands/weapon position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 16 }));

        _Stance2HandsSidewaysOffset = Config.Bind(
            Stance2Section,
            "Stance 2 Hands Sideways Offset",
            0f,
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

        _Stance3HandsPitchRotation = Config.Bind(
            Stance3Section,
            "Stance 3 Hands Pitch (X-Axis)",
            30f,
            new ConfigDescription("Stance 3 hands/arms pitch rotation in degrees (up/down tilt)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 13 }));

        _Stance3HandsYawRotation = Config.Bind(
            Stance3Section,
            "Stance 3 Hands Yaw (Y-Axis)",
            0f,
            new ConfigDescription("Stance 3 hands/arms yaw rotation in degrees (left/right turn)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 12 }));

        _Stance3HandsRollRotation = Config.Bind(
            Stance3Section,
            "Stance 3 Hands Roll (Z-Axis)",
            0f,
            new ConfigDescription("Stance 3 hands/arms roll rotation in degrees (weapon cant)",
            new AcceptableValueRange<float>(-45f, 45f),
            new ConfigurationManagerAttributes { Order = 11 }));

        _Stance3HandsForwardBackwardOffset = Config.Bind(
            Stance3Section,
            "Stance 3 Hands Forward/Backward Offset",
            0.03f,
            new ConfigDescription("Stance 3 hands/weapon position forward/backward (positive = forward)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 10 }));

        _Stance3HandsUpDownOffset = Config.Bind(
            Stance3Section,
            "Stance 3 Hands Up/Down Offset",
            0f,
            new ConfigDescription("Stance 3 hands/weapon position up/down (positive = up)",
            new AcceptableValueRange<float>(-0.5f, 0.5f),
            new ConfigurationManagerAttributes { Order = 9 }));

        _Stance3HandsSidewaysOffset = Config.Bind(
            Stance3Section,
            "Stance 3 Hands Sideways Offset",
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

        // Note: RotationEvents and SetItemInHandsPatch are deprecated now
        // All logic is handled by StanceManager + SpringGetPatch
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
        // Validate spring cache FIRST — detects stale references after transit/weapon swap
        // so the fast-exit in SpringGetPatch.PatchPostfix doesn't reject new Spring instances
        SpringGetPatch.ValidateSpringCache();
        
        StanceManager.Update();
        StanceManager.UpdateTacSprint();
        UpdateCameraOffset();
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
}