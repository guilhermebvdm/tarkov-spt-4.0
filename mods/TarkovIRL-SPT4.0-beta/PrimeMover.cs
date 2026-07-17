using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

[BepInPlugin("TarkovIRL", "TarkovIRL - WHM", "1.0.0")]
public class PrimeMover : BaseUnityPlugin
{
  private const string modGUID = "TarkovIRL";
  private const string modName = "TarkovIRL - WHM";
  private const string modVersion = "1.0.0";
  public static PrimeMover Instance;
  public readonly float FTDThresh = 0.0167f;
  private GUIStyle efficiencyIndicatorStyle = new GUIStyle();
  public AnimationCurve BreathCurve;
  public AnimationCurve PoseChangeCurve;
  public AnimationCurve PoseChangeDownRadialCurve;
  public AnimationCurve PoseChangeCurveUp;
  public AnimationCurve HandsShakeCurve;
  public AnimationCurve SmoothEdgesCurve;
  public AnimationCurve FootStepCurve;
  public AnimationCurve ErgoAttenuationCurve;
  public AnimationCurve ThrowVisualCurveX;
  public AnimationCurve ThrowVisualCurveY;
  public AnimationCurve ThrowVisualUnderhandCurveX;
  public AnimationCurve ThrowVisualUnderhandCurveY;
  public AnimationCurve WeightAttenuationCurve;
  public AnimationCurve ThrowAlphaCurve;
  public AnimationCurve NewSwayCurve;
  public AnimationCurve SideToSideCurve;
  public AnimationCurve SlowReloadCurve;
  public AnimationCurve PresentShoulderCurve;
  public AnimationCurve OrderShoulderCurve;
  public float DeltaTime = 0.0f;
  public float Time = 0.0f;
  public float FixedDeltaTime = 0.0f;
  private const string BASE_FEATURES_SECTION = "a - Toggle base features";
  private const string GENERAL_SLIDERS = "b - Adjust main feature values";
  private const string NEW_SWAY_SLIDERS = "c - Sway multipliers";
  private const string PARALLAX_SLIDERS = "d - Parallax multipliers";
  private const string EFFICIENCY_SLIDERS = "e - Efficiency multipliers";
  private const string ROTATION_ENGINE_SLIDERS = "f - Rotation engine multipliers";
  private const string MISC_SLIDERS = "g - Misc multipliers";
  private const string DIRECTIONAL_SWAY = "h - Directional Sway Values";
  private const string DEADZONE = "j - Deadzone Values";
  private const string DEBUG_OPTIONS = "k - Debug logs";
  private const string DEV_SECTION = "0 - Only for dev/testing";
  public static ConfigEntry<bool> EnableMod;
  public static ConfigEntry<KeyboardShortcut> EnableModKey;
  public static ConfigEntry<float> MasterSensitivityMultiplier;
  public static ConfigEntry<bool> IsWeaponDeadzone;
  public static ConfigEntry<bool> IsWeaponSway;
  public static ConfigEntry<bool> IsBreathingEffect;
  public static ConfigEntry<bool> IsPoseEffect;
  public static ConfigEntry<bool> IsPoseChangeEffect;
  public static ConfigEntry<bool> IsArmShakeEffect;
  public static ConfigEntry<bool> IsSmallMovementsEffect;
  public static ConfigEntry<bool> IsFootstepEffect;
  public static ConfigEntry<bool> IsParallaxEffect;
  public static ConfigEntry<bool> IsDirectionalSway;
  public static ConfigEntry<bool> IsHeadTiltADS;
  public static ConfigEntry<bool> IsWeaponTrans;
  public static ConfigEntry<bool> IsEfficiencyIndicator;
  public static ConfigEntry<bool> IsLogging;
  public static ConfigEntry<bool> DebugSpam;
  public static ConfigEntry<float> NewSwayFinalLerpSpeed;
  public static ConfigEntry<float> SwayCustomWeight;
  public static ConfigEntry<float> PistolSwayMulti;
  public static ConfigEntry<float> SwayCustomErgo;
  public static ConfigEntry<float> WeaponDeadzoneMulti;
  public static ConfigEntry<float> WeaponSwayMulti;
  public static ConfigEntry<float> MinimumWeaponSway;
  public static ConfigEntry<bool> InvertSwayVanilla;
  public static ConfigEntry<float> BreathingEffectMulti;
  public static ConfigEntry<float> ArmShakeMulti;
  public static ConfigEntry<float> ArmShakeRateMulti;
  public static ConfigEntry<float> ParallaxMulti;
  public static ConfigEntry<float> AdsParallaxTimeMulti;
  public static ConfigEntry<float> ShotParallaxResetTimeMulti;
  public static ConfigEntry<float> EfficiencyLerpMulti;
  public static ConfigEntry<float> ShotParallaxWeaponWeightMulti;
  public static ConfigEntry<bool> EnableShotParallax;
  public static ConfigEntry<float> ParallaxSetSizeMulti;
  public static ConfigEntry<float> CameraUpdateMulti;
  public static ConfigEntry<float> FootstepLerpMulti;
  public static ConfigEntry<float> FootstepIntesnityMulti;
  public static ConfigEntry<float> ParallaxInAds;
  public static ConfigEntry<float> ParallaxPosXMulti;
  public static ConfigEntry<float> PistolSpecificParallax;
  public static ConfigEntry<float> ThrowStrengthMulti;
  public static ConfigEntry<float> ThrowSpeedMulti;
  public static ConfigEntry<float> EfficiencyInjuryImpact;
  public static ConfigEntry<float> ParallaxRecenterFactor;
  public static ConfigEntry<float> NewSwayPositionMulti;
  public static ConfigEntry<float> FastTurnThreshold;
  public static ConfigEntry<float> FastTurnAttenuation;
  public static ConfigEntry<float> NewSwayPositionHardClampPos;
  public static ConfigEntry<float> NewSwayPositionHardClampNeg;
  public static ConfigEntry<float> NewSwayRotationMulti;
  public static ConfigEntry<float> NewSwaySlideMulti;
  public static ConfigEntry<float> NewSwayPositionDTMulti;
  public static ConfigEntry<float> NewSwayRotationDTMulti;
  public static ConfigEntry<bool> EnableFreeAimADS;
  public static ConfigEntry<float> FreeAimBoundsXADS;
  public static ConfigEntry<float> FreeAimBoundsYADS;
  public static ConfigEntry<float> FreeAimReturnSpeedADS;
  public static ConfigEntry<float> FreeAimSensitivityADS;
  public static ConfigEntry<bool> EnableFreeAim;
  public static ConfigEntry<float> FreeAimBoundsX;
  public static ConfigEntry<float> FreeAimBoundsY;
  public static ConfigEntry<float> FreeAimReturnSpeed;
  public static ConfigEntry<float> FreeAimSensitivity;
  public static ConfigEntry<bool> EnableCameraAutoCenter;
  public static ConfigEntry<float> CameraAutoCenterSpeed;
  public static ConfigEntry<bool> EnableCameraAutoCenterADS;
  public static ConfigEntry<float> CameraAutoCenterSpeedADS;
  public static ConfigEntry<float> FreeAimAutoCenterADSComp;
  public static ConfigEntry<float> DebugPosX;
  public static ConfigEntry<float> DebugPosY;
  public static ConfigEntry<float> DebugPosZ;
  public static ConfigEntry<float> DebugRotX;
  public static ConfigEntry<float> DebugRotY;
  public static ConfigEntry<float> DebugRotZ;
  public static ConfigEntry<float> NewSwayWpnUnstockedDropValue;
  public static ConfigEntry<float> NewSwayWpnUnstockedDropSpeed;
  public static ConfigEntry<float> WeaponCantValue;
  public static ConfigEntry<float> SideToSideSwayMulti;
  public static ConfigEntry<float> SideToSideRotationDTMulti;
  public static ConfigEntry<float> SideToSidePositionDTMulti;
  public static ConfigEntry<float> FootstepCutoffRatio;
  public static ConfigEntry<float> MotionTrackingThreshold;
  public static ConfigEntry<float> LeanExtraVerticalMulti;
  public static ConfigEntry<float> NewSwayWpnDropFromRotMulti;
  public static ConfigEntry<float> RotationAverageDTMulti;
  public static ConfigEntry<float> ParallaxDTMulti;
  public static ConfigEntry<float> ParallaxRotationSmoothingMulti;
  public static ConfigEntry<float> DevTestFloat7;
  public static ConfigEntry<float> RotationHistoryClamp;
  public static ConfigEntry<float> HyperVerticalClamp;
  public static ConfigEntry<float> HyperVerticalMulti;
  public static ConfigEntry<float> HyperVerticalDT;
  public static ConfigEntry<float> NewSwayADSRotClamp;
  public static ConfigEntry<float> NewSwayRotDeltaClamp;
  public static ConfigEntry<float> NewSwayRotFinalClampPos;
  public static ConfigEntry<float> NewSwayRotFinalClampNeg;
  public static ConfigEntry<float> LeanCounterRotateMod;
  public static ConfigEntry<float> ParallaxHardClamp;
  public static ConfigEntry<float> ParallaxHardClampPistols;
  public static ConfigEntry<float> DeadzoneInShortStock;
  public static ConfigEntry<float> DeadzoneInVanilla;
  public static ConfigEntry<float> DeadzoneInActiveAim;
  public static ConfigEntry<float> DeadzoneInADS;
  public static ConfigEntry<float> DeadzoneInHighReady;
  public static ConfigEntry<float> DeadzoneInLowReady;
  public static ConfigEntry<float> DeadzoneHeadFollowSpeedMulti;
  public static ConfigEntry<float> TransitionSpeedMulti;
  public static ConfigEntry<float> RunFadeDTMulti;
  public static ConfigEntry<float> LaggingSwayNorm;
  public static ConfigEntry<float> LaggingSwayMulti;
  public static ConfigEntry<float> LaggingSwayClamp;
  public static ConfigEntry<float> HandFinalSmooth;
  public static ConfigEntry<bool> DeadzoneWeightForEfficiency;
  public static ConfigEntry<float> DeadzoneCustomWeight;
  public static ConfigEntry<float> DeadzoneCustomErgo;
  public static ConfigEntry<bool> DebugEfficiency;
  public static ConfigEntry<float> AnimatorLayerInt;
  public static ConfigEntry<float> DirectionalSwayLateralPosValue;
  public static ConfigEntry<float> DirectionalSwayProjectedPosValue;
  public static ConfigEntry<float> DirectionalSwayLateralRotValue;
  public static ConfigEntry<float> DirectionalSwayVerticalRotValue;
  public static ConfigEntry<float> DirectionalSwayMulti;
  public static ConfigEntry<float> DirectionalSwayLerpSpeed;
  public static ConfigEntry<float> DirectionalSwayLerpOnAds;
  public static ConfigEntry<float> EfficiencyOverweightIpmact;
  public static ConfigEntry<float> DebugFloat1;
  public static ConfigEntry<float> LoggingUpdateFrequency;
  private readonly float ArmShakeRateMultiDefault = 1f;
  private readonly float BreathingEffectMultiDefault = 1f;
  private readonly float FootstepLerpMultiDefault = 0.1f;
  private readonly float ParallaxInAdsDefault = 0.3f;
  private readonly float ShotParallaxResetTimeMultiDefault = 10f;
  private readonly float ThrowStrengthMultiDefault = 14f;
  private readonly float ThrowSpeedMultiDefault = 2.25f;
  public static ConfigEntry<float> HeadRotationSmoothing;
  public static ConfigEntry<float> HeadRotationSmoothing2;
  public static ConfigEntry<float> HeadRotationSmoothing3;
  public static ConfigEntry<float> HeadRotationLerpSpeed;

  private void Awake()
  {
    if ((PrimeMover.Instance == null))
      PrimeMover.Instance = this;
    this.Initialize();
    this.LoadConfigValues();
  }

  private void Initialize()
  {
    TIRLUtils.Logger = this.Logger;
    this.BreathCurve = TarkovIRLCurves.GetBreathCurve();
    this.PoseChangeDownRadialCurve = TarkovIRLCurves.GetPoseChangeDownRadialCurve();
    this.PoseChangeCurve = TarkovIRLCurves.GetPoseChangeCurve();
    this.PoseChangeCurveUp = TarkovIRLCurves.GetPoseChangeCurveUp();
    this.HandsShakeCurve = TarkovIRLCurves.GetHandsShakeCurve();
    this.SmoothEdgesCurve = TarkovIRLCurves.GetSmoothEdgesCurve();
    this.FootStepCurve = TarkovIRLCurves.GetFootStepCurve();
    this.ErgoAttenuationCurve = TarkovIRLCurves.GetErgoAttenuationCurve();
    this.ThrowVisualCurveX = TarkovIRLCurves.GetThrowVisualCurveX();
    this.ThrowVisualCurveY = TarkovIRLCurves.GetThrowVisualCurveY();
    this.ThrowVisualUnderhandCurveX = TarkovIRLCurves.GetThrowVisualUnderhandCurveX();
    this.ThrowVisualUnderhandCurveY = TarkovIRLCurves.GetThrowVisualUnderhandCurveY();
    this.WeightAttenuationCurve = TarkovIRLCurves.GetWeightAttenuationCurve();
    this.ThrowAlphaCurve = TarkovIRLCurves.GetThrowAlphaCurve();
    this.NewSwayCurve = TarkovIRLCurves.GetNewSwayCurve();
    this.SideToSideCurve = TarkovIRLCurves.GetSideToSideCurve();
    this.SlowReloadCurve = TarkovIRLCurves.GetSlowReloadCurve();
    this.PresentShoulderCurve = TarkovIRLCurves.GetPresentShoulderCurve();
    this.OrderShoulderCurve = TarkovIRLCurves.GetOrderShoulderCurve();
    this.TryLoadPatch((ModulePatch) new Patch_LerpCamera_ForceUpdateSway());
    this.TryLoadPatch((ModulePatch) new Patch_UpdateSwayFactors());
    this.TryLoadPatch((ModulePatch) new Patch_LateUpdate_UpdateWpnStats());
    this.TryLoadPatch((ModulePatch) new Patch_OnShot());
    this.TryLoadPatch((ModulePatch) new Patch_CalculateCameraPosition_HandLayers());
    this.TryLoadPatch((ModulePatch) new Patch_PlayerRotate());
    this.TryLoadPatch((ModulePatch) new Patch_PlayStepSound());
    this.TryLoadPatch((ModulePatch) new Patch_TranslateCommand());
    this.TryLoadPatch((ModulePatch) new StaminaRegenRatePatch());
    this.TryLoadPatch((ModulePatch) new Patch_SetHeadRotation());
    this.TryLoadPatch((ModulePatch) new Patch_Look());
    this.TryLoadPatch((ModulePatch) new Patch_GameWorldOnDestroy());
  }

  private void LoadConfigValues()
  {
    PrimeMover.EnableMod = this.ConstructBoolConfig(true, "a - Mod Status", "Enable Mod", "Master toggle for all features.");
    PrimeMover.EnableModKey = this.Config.Bind<KeyboardShortcut>("a - Mod Status", "Toggle Mod Key", new KeyboardShortcut((KeyCode) 285, Array.Empty<KeyCode>()), new ConfigDescription("Press to toggle the mod on/off", (AcceptableValueBase) null, Array.Empty<object>()));
    PrimeMover.MasterSensitivityMultiplier = this.ConstructFloatConfig(1f, "a - Mod Status", "Master Sensitivity Multiplier", "Scale sensitivity of all mouse-driven effects (Free Aim, Sway) at once. Useful for adjusting to different mouse DPIs.", 0.1f, 5f);
    PrimeMover.IsWeaponDeadzone = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable weapon deadzone", "Aiming deadzone, like in other games but better.");
    PrimeMover.IsEfficiencyIndicator = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable efficiency indicator", "When this toggle is enabled, you see two dots at the bottom of your screen whose distance apart indicates how efficient your character currently is (or isn't). Closer is better.");
    PrimeMover.IsWeaponSway = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable custom weapon sway", "Completely scratch-built weapon sway. Generally leads aimpoint instead of lagging behind it.");
    PrimeMover.IsBreathingEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable breathing effect", "Adds a visual oscillation to your character's weapon, the intensity of which depends on your current stamina.");
    PrimeMover.IsPoseEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable stance-dependent weapon position", "When you crouch, your weapon position is pulled in closer to your character.");
    PrimeMover.IsPoseChangeEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable stance transition effect", "When you change your crouch position, you see a dip in your sight picture, the speed and intensity of which is driven by how much you change your stance (e.g. incrimental change versus full change.");
    PrimeMover.IsArmShakeEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable extra arm stam shake", "Adds additional arm shake as arm stam decreases.");
    PrimeMover.IsSmallMovementsEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable small visual effects", "Toggles small details: pulling the weapon in on rotation, lowering with unstocked weapon, new grenade throwing effects, alternative peek head position.");
    PrimeMover.IsFootstepEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable footstep effect", "Player's weapon will bounce a bit more when taking steps, effect intesnity depends on several factors.");
    PrimeMover.IsParallaxEffect = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable aiming misalignment feature", "Extensive feature when causes the weapon to rotate in the player's hand and thus un-align the sights, when the player is rotating. The intensity of the effect depends on your efficiency and the weight and ergo of the weapon.");
    PrimeMover.IsDirectionalSway = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable directional sway feature", "Additional layer of weapon sway that is caused by forward-back-left-right movements.");
    PrimeMover.IsHeadTiltADS = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable ADS head tilt", "Additional slight head tilt on ADS when using a stocked weapon stock.");
    PrimeMover.IsWeaponTrans = this.ConstructBoolConfig(true, "a - Toggle base features", "Enable Enhanced Weapon Transitions", "Custom implementation of weapon transitions (sling-shoulder-holster)");
    PrimeMover.IsLogging = this.ConstructBoolConfig(false, "k - Debug logs", "Enable debug logging", "");
    PrimeMover.DebugSpam = this.ConstructBoolConfig(false, "k - Debug logs", "Enable debug spam", "");
    PrimeMover.DebugEfficiency = this.ConstructBoolConfig(false, "k - Debug logs", "Show efficiency value (inverse version)", "");
    PrimeMover.WeaponDeadzoneMulti = this.ConstructFloatConfig(0.3f, "b - Adjust main feature values", "Deadzone multiplier", "Change overall deadzone effect strength.", 0.0f, 5f);
    PrimeMover.WeaponSwayMulti = this.ConstructFloatConfig(0.5f, "b - Adjust main feature values", "Sway multiplier", "Change overall sway effect strength.", 0.0f, 2f);
    PrimeMover.MinimumWeaponSway = this.ConstructFloatConfig(0.3f, "c - Sway Values", "Minimum Weapon Sway", "Guarantees a minimum sway effect even for very light weapons.", 0.0f, 2f);
    PrimeMover.InvertSwayVanilla = this.ConstructBoolConfig(false, "c - Sway Values", "Invert Sway Direction in Vanilla", "Makes the weapon lead the camera in Vanilla Stance (Bodycam true free aim style).");
    PrimeMover.ParallaxMulti = this.ConstructFloatConfig(16f, "b - Adjust main feature values", "Aiming misalignment multiplier", "Change overall parallax effect strength.", 1f, 100f);
    PrimeMover.DirectionalSwayMulti = this.ConstructFloatConfig(0.12f, "b - Adjust main feature values", "Directional Sway Final Modifier", "", 0.0f, 5f);
    PrimeMover.TransitionSpeedMulti = this.ConstructFloatConfig(1.3f, "b - Adjust main feature values", "Weapon transition speed multiplier", "Global multiplier for how fast all weapon transitions occur", 0.1f, 5f);
    PrimeMover.HandFinalSmooth = this.ConstructFloatConfig(1f, "b - Adjust main feature values", "Main hand smoothing layer", "Value to control how much all of the hand movements are smoothed before being pushed to visual layer", 1f, 20f);
    PrimeMover.PistolSwayMulti = this.ConstructFloatConfig(2f, "c - Sway multipliers", "Sway Pistol Multiplier", "Exaggerates the visual sway for pistols to compensate for their short length and grip pivot point.", 1f, 10f);
    PrimeMover.SwayCustomWeight = this.ConstructFloatConfig(4f, "c - Sway multipliers", "Sway Fixed Weight", "Fixed weight value used strictly for calculating Sway. (Overrides dynamic weapon weight)", 0.0f, 20f);
    PrimeMover.SwayCustomErgo = this.ConstructFloatConfig(0.5f, "c - Sway multipliers", "Sway Fixed Ergo Norm", "Fixed ergo value (0.0 to 1.0) used strictly for calculating Sway. (Overrides dynamic weapon ergo)", 0.0f, 1f);
    PrimeMover.NewSwayFinalLerpSpeed = this.ConstructFloatConfig(15f, "c - Sway multipliers", "Sway return to centre speed", "The speed at which the sway functions corrects back to zero", -5f, 50f);
    PrimeMover.NewSwayPositionMulti = this.ConstructFloatConfig(0.75f, "c - Sway multipliers", "Sway position multiplier", "", -10f, 10f);
    PrimeMover.FastTurnThreshold = this.ConstructFloatConfig(150f, "b - Adjust main feature values", "Fast Turn Threshold", "Rotation speed (deg/sec) at which Sway and Free Aim start to attenuate.", 0.1f, 500f);
    PrimeMover.FastTurnAttenuation = this.ConstructFloatConfig(0.8f, "b - Adjust main feature values", "Fast Turn Attenuation", "How much Sway and Free Aim are reduced during fast turns (1 = max reduction).", 0.0f, 1f);
    PrimeMover.NewSwayPositionHardClampPos = this.ConstructFloatConfig(0.05f, "c - Sway multipliers", "Sway position hard clamp (Right)", "Limits how far the weapon can slide right on screen.", 0.0f, 0.5f);
    PrimeMover.NewSwayPositionHardClampNeg = this.ConstructFloatConfig(0.05f, "c - Sway multipliers", "Sway position hard clamp (Left)", "Limits how far the weapon can slide left on screen.", 0.0f, 0.5f);
    PrimeMover.NewSwayRotationMulti = this.ConstructFloatConfig(2f, "c - Sway multipliers", "Sway rotation multiplier", "", 0.0f, 10f);
    PrimeMover.NewSwaySlideMulti = this.ConstructFloatConfig(1f, "e - Sway Values", "Sway Slide (Pivot) Multiplier", "Multiplier for horizontal position sway. Set to 0 to keep stock anchored when Invert Sway is true.", 0.0f, 5f);
    PrimeMover.NewSwayPositionDTMulti = this.ConstructFloatConfig(55f, "c - Sway multipliers", "Sway position change speed", "", 0.0f, 150f);
    PrimeMover.NewSwayRotationDTMulti = this.ConstructFloatConfig(13f, "c - Sway multipliers", "Sway rotation change speed", "", 0.0f, 15f);
    PrimeMover.EnableFreeAimADS = this.ConstructBoolConfig(false, "z - Free Aim ADS", "Enable Free Aim (ADS)", "Deadzone style free aim during ADS.");
    PrimeMover.FreeAimBoundsXADS = this.ConstructFloatConfig(5f, "z - Free Aim ADS", "Bounds Horizontal", "Max horizontal degrees.", 0.0f, 25f);
    PrimeMover.FreeAimBoundsYADS = this.ConstructFloatConfig(3f, "z - Free Aim ADS", "Bounds Vertical", "Max vertical degrees.", 0.0f, 25f);
    PrimeMover.FreeAimReturnSpeedADS = this.ConstructFloatConfig(10f, "z - Free Aim ADS", "Return Speed", "Speed to center.", 0.1f, 20f);
    PrimeMover.FreeAimSensitivityADS = this.ConstructFloatConfig(0.1f, "z - Free Aim ADS", "Sensitivity", "Multiplier for mouse input.", 0.01f, 5f);
    PrimeMover.EnableCameraAutoCenterADS = this.ConstructBoolConfig(false, "z - Free Aim ADS", "Enable Camera Auto-Center (ADS)", "The camera will automatically rotate to catch up with the weapon's free aim offset during ADS.");
    PrimeMover.CameraAutoCenterSpeedADS = this.ConstructFloatConfig(1f, "z - Free Aim ADS", "Camera Auto-Center Speed", "Speed at which the camera catches up to the weapon during ADS.", 0.1f, 10f);
    PrimeMover.FreeAimAutoCenterADSComp = this.ConstructFloatConfig(0.35f, "z - Free Aim ADS", "Camera Auto-Center Sensitivity Compensation", "Adjust this value so that when you stop moving the mouse in ADS, the crosshair stays perfectly static in the world. Increase it if the crosshair drifts forward, decrease if it drifts backward.", 0.01f, 2f);
    PrimeMover.EnableFreeAim = this.ConstructBoolConfig(true, "z - Free Aim", "Enable True Free Aim", "Deadzone style free aim.");
    PrimeMover.FreeAimBoundsX = this.ConstructFloatConfig(15f, "z - Free Aim", "Bounds Horizontal", "Max horizontal degrees.", 0.0f, 25f);
    PrimeMover.FreeAimBoundsY = this.ConstructFloatConfig(10f, "z - Free Aim", "Bounds Vertical", "Max vertical degrees.", 0.0f, 25f);
    PrimeMover.FreeAimReturnSpeed = this.ConstructFloatConfig(5f, "z - Free Aim", "Return Speed", "Speed to center.", 0.1f, 20f);
    PrimeMover.FreeAimSensitivity = this.ConstructFloatConfig(1f, "z - Free Aim", "Sensitivity", "Multiplier for mouse input.", 0.01f, 5f);
    PrimeMover.EnableCameraAutoCenter = this.ConstructBoolConfig(false, "z - Free Aim", "Enable Camera Auto-Center", "The camera will automatically rotate to catch up with the weapon's free aim offset.");
    PrimeMover.CameraAutoCenterSpeed = this.ConstructFloatConfig(3f, "z - Free Aim", "Camera Auto-Center Speed", "Speed at which the camera catches up to the weapon.", 0.1f, 10f);
    PrimeMover.DebugPosX = this.ConstructFloatConfig(0.0f, "z - Debug Axes", "Position X", "Move to test axis.", -0.5f, 0.5f);
    PrimeMover.DebugPosY = this.ConstructFloatConfig(0.0f, "z - Debug Axes", "Position Y", "Move to test axis.", -0.5f, 0.5f);
    PrimeMover.DebugPosZ = this.ConstructFloatConfig(0.0f, "z - Debug Axes", "Position Z", "Move to test axis.", -0.5f, 0.5f);
    PrimeMover.DebugRotX = this.ConstructFloatConfig(0.0f, "z - Debug Axes", "Rotation X (Pitch)", "Rotate to test axis.", -45f, 45f);
    PrimeMover.DebugRotY = this.ConstructFloatConfig(0.0f, "z - Debug Axes", "Rotation Y (Yaw)", "Rotate to test axis.", -45f, 45f);
    PrimeMover.DebugRotZ = this.ConstructFloatConfig(0.0f, "z - Debug Axes", "Rotation Z (Roll)", "Rotate to test axis.", -45f, 45f);
    PrimeMover.NewSwayWpnUnstockedDropValue = this.ConstructFloatConfig(0.3f, "c - Sway multipliers", "Weapon drop for unstocked value", "Unstocked weapons will drop a little when rotating.", 0.0f, 10f);
    PrimeMover.NewSwayWpnUnstockedDropSpeed = this.ConstructFloatConfig(3f, "c - Sway multipliers", "Weapon drop for unstocked speed", "", 0.0f, 10f);
    PrimeMover.WeaponCantValue = this.ConstructFloatConfig(0.0f, "c - Sway multipliers", "Weapon cant value", "", -1f, 1f);
    PrimeMover.LeanExtraVerticalMulti = this.ConstructFloatConfig(0.01f, "c - Sway multipliers", "Lean vertical change value", "When you peek, your weapon points up/down a little to complicate aiming.", 0.0f, 0.05f);
    PrimeMover.NewSwayWpnDropFromRotMulti = this.ConstructFloatConfig(0.3f, "c - Sway multipliers", "Weapon aimpoint drop on rotation", "When rotating, your weapon will point down a little consonent with the rotation speed. A slight emulation of manipulating a physical object, stacks with weapon weight etc.", -10f, 10f);
    PrimeMover.HyperVerticalClamp = this.ConstructFloatConfig(0.1f, "c - Sway multipliers", "Hyper-vertical effect input clamp", "The 'hyper-vertical' effect emulates gravity, so to speak. Figure it out.", -1f, 1f);
    PrimeMover.HyperVerticalMulti = this.ConstructFloatConfig(2f, "c - Sway multipliers", "Hyper-vertical effect value", "The 'hyper-vertical' effect emulates gravity, so to speak. Figure it out.", 0.0f, 30f);
    PrimeMover.HyperVerticalDT = this.ConstructFloatConfig(2.5f, "c - Sway multipliers", "Hyper-vertical change speed", "The 'hyper-vertical' effect emulates gravity, so to speak. Figure it out.", -10f, 10f);
    PrimeMover.NewSwayADSRotClamp = this.ConstructFloatConfig(0.9f, "c - Sway multipliers", "Clamp sway effect specifically in ADS", "", 0.0f, 1f);
    PrimeMover.NewSwayRotDeltaClamp = this.ConstructFloatConfig(0.01f, "c - Sway multipliers", "Sway rotation input clamp", "Touch at your own risk kek", 0.0f, 0.1f);
    PrimeMover.NewSwayRotFinalClampPos = this.ConstructFloatConfig(0.06f, "c - Sway multipliers", "Sway input smoothing clamp (Positive)", "Maximum rotation in positive axis.", 0.0f, 0.1f);
    PrimeMover.NewSwayRotFinalClampNeg = this.ConstructFloatConfig(0.06f, "c - Sway multipliers", "Sway input smoothing clamp (Negative)", "Maximum rotation in negative axis.", 0.0f, 0.1f);
    PrimeMover.LaggingSwayNorm = this.ConstructFloatConfig(0.5f, "c - Sway multipliers", "Lagging sway norm", "", 0.0f, 5f);
    PrimeMover.LaggingSwayMulti = this.ConstructFloatConfig(2f, "c - Sway multipliers", "Lagging sway multiplier", "Defines strength of laggin sway effect, a component of the overall sway effect package.", 0.0f, 5f);
    PrimeMover.LaggingSwayClamp = this.ConstructFloatConfig(12f, "c - Sway multipliers", "Lagging sway clamp", "Defines max strength of laggin sway effect, a component of the overall sway effect package.", 1f, 30f);
    PrimeMover.ParallaxSetSizeMulti = this.ConstructFloatConfig(0.5f, "d - Parallax multipliers", "Parallax set size", "Amount of parallax that is possible per player rotation.", 0.0f, 20f);
    PrimeMover.ParallaxPosXMulti = this.ConstructFloatConfig(1f, "d - Parallax multipliers", "Parallax Position X Multiplier", "Multiplier for the horizontal (X-axis) sliding effect of Parallax. Set to 0 to disable lateral dancing.", 0.0f, 2f);
    PrimeMover.ParallaxInAds = this.ConstructFloatConfig(this.ParallaxInAdsDefault, "d - Parallax multipliers", "Parallax effect in ADS", "The % of parallax effect that you see in ADS (with a stocked weapon)", 0.0f, 1f);
    PrimeMover.PistolSpecificParallax = this.ConstructFloatConfig(4f, "d - Parallax multipliers", "Parallax effect value specifically for pistols", "", 0.0f, 10f);
    PrimeMover.ShotParallaxResetTimeMulti = this.ConstructFloatConfig(this.ShotParallaxResetTimeMultiDefault, "d - Parallax multipliers", "Shot-parallax cooldown multiplier", "Rate of return to reduced parallax in the ADS, after a shot.", 0.0f, 20f);
    PrimeMover.AdsParallaxTimeMulti = this.ConstructFloatConfig(2f, "d - Parallax multipliers", "Ads-parallax cooldown multiplier", "", 0.0f, 160f);
    PrimeMover.EnableShotParallax = this.ConstructBoolConfig(false, "a - Toggle base features", "Enable Shot Parallax", "Adds a twist/shake effect to the weapon when shooting based on bullet mass and weapon weight.");
    PrimeMover.ShotParallaxWeaponWeightMulti = this.ConstructFloatConfig(0.9f, "d - Parallax multipliers", "Shot parallax weapon weight factor multiplier", "After a shot in ADS, the parallax effect is momentarilly returned to full strength.", 0.0f, 10f);
    PrimeMover.ParallaxDTMulti = this.ConstructFloatConfig(10f, "d - Parallax multipliers", "Parallax main lerp multiplier", "How fast the main parallax routine processes (so, responsiveness we can say).", 0.05f, 1000f);
    PrimeMover.ParallaxRotationSmoothingMulti = this.ConstructFloatConfig(10f, "d - Parallax multipliers", "ParallaxRotationSmoothingMulti", "The strength with which the parallax effect is bled-off when not rotating.", 0.05f, 1000f);
    PrimeMover.ParallaxHardClamp = this.ConstructFloatConfig(0.08f, "d - Parallax multipliers", "Parallax Hard Stop", "Absolute maximum value of the parallax effect -- if you want to prevent noodle-arms, this is a good place to start.", 0.0f, 1f);
    PrimeMover.ParallaxHardClampPistols = this.ConstructFloatConfig(0.03f, "d - Parallax multipliers", "Parallax Hard Stop Pistols", "Absolute maximum value of the parallax effect -- if you want to prevent noodle-arms, this is a good place to start.", 0.0f, 1f);
    PrimeMover.EfficiencyLerpMulti = this.ConstructFloatConfig(8f, "e - Efficiency multipliers", "Efficiency change rate multiplier", "", 0.0f, 10f);
    PrimeMover.EfficiencyOverweightIpmact = this.ConstructFloatConfig(250f, "e - Efficiency multipliers", "Efficiency overweight coef", "Controls how much being overweight affects your Efficiency.", 0.0f, 25f);
    PrimeMover.EfficiencyInjuryImpact = this.ConstructFloatConfig(0.5f, "e - Efficiency multipliers", "Efficiency injury effect coef", "Controls how much injuries affect your Efficiency stat (before you heal them).", 0.0f, 2f);
    PrimeMover.LeanCounterRotateMod = this.ConstructFloatConfig(-8f, "g - Misc multipliers", "Peek counter-rotate value", "Just a small visual effect so that your head does not rotate perfectly with the weapon when peeking; I find this more immersive.", -10f, 10f);
    PrimeMover.BreathingEffectMulti = this.ConstructFloatConfig(this.BreathingEffectMultiDefault, "g - Misc multipliers", "Breathing effect multiplier", "Adjust the strength of the breathing effect which comes with non-full stamina.", 0.0f, 5f);
    PrimeMover.ArmShakeMulti = this.ConstructFloatConfig(2f, "g - Misc multipliers", "Arm shake multiplier", "", 0.0f, 5f);
    PrimeMover.ArmShakeRateMulti = this.ConstructFloatConfig(this.ArmShakeRateMultiDefault, "g - Misc multipliers", "Arm shake effect speed multiplier", "The arm shake effect is an animation, this changes the speed of the animation playback.", 0.0f, 5f);
    PrimeMover.FootstepLerpMulti = this.ConstructFloatConfig(this.FootstepLerpMultiDefault, "g - Misc multipliers", "Footstep lerp speed multiplier", "The footstep effect is an animation, this changes the speed of the animation playback", 0.0f, 1f);
    PrimeMover.FootstepIntesnityMulti = this.ConstructFloatConfig(6f, "g - Misc multipliers", "Footstep intensity multiplier", "", 0.0f, 10f);
    PrimeMover.ThrowStrengthMulti = this.ConstructFloatConfig(this.ThrowStrengthMultiDefault, "g - Misc multipliers", "Throw visual effect multi", "Change the strength of the throw animation", 0.0f, 100f);
    PrimeMover.ThrowSpeedMulti = this.ConstructFloatConfig(this.ThrowSpeedMultiDefault, "g - Misc multipliers", "Throw effect speed", "The throw effect is an animation, this changes the speed of the animation playback.", 0.0f, 10f);
    PrimeMover.SideToSideSwayMulti = this.ConstructFloatConfig(0.01f, "g - Misc multipliers", "Footstep side-to-side value", "", 0.0f, 0.03f);
    PrimeMover.SideToSideRotationDTMulti = this.ConstructFloatConfig(0.9f, "g - Misc multipliers", "Footstep side-to-side rotation speed", "", 0.0f, 10f);
    PrimeMover.SideToSidePositionDTMulti = this.ConstructFloatConfig(0.35f, "g - Misc multipliers", "Footstep side-to-side position speed", "", 0.0f, 10f);
    PrimeMover.FootstepCutoffRatio = this.ConstructFloatConfig(0.65f, "g - Misc multipliers", "Footstep animation cutoff", "Just don't touch this.", 0.0f, 1f);
    PrimeMover.MotionTrackingThreshold = this.ConstructFloatConfig(0.005f, "g - Misc multipliers", "Motion tracking threshold", "Just don't touch this.", 0.0f, 0.01f);
    PrimeMover.RunFadeDTMulti = this.ConstructFloatConfig(10f, "g - Misc multipliers", "Run Transition Animation Fade Speed Multi", "", 1f, 50f);
    PrimeMover.HeadRotationLerpSpeed = this.ConstructFloatConfig(7f, "g - Misc multipliers", "Bodycam Delay Speed", "Controls how fast the camera follows the weapon. Lower = more bodycam effect.", 1f, 20f);
    PrimeMover.RotationAverageDTMulti = this.ConstructFloatConfig(80f, "f - Rotation engine multipliers", "RotationAverageDTMulti", "Just don't touch this.", 0.1f, 100f);
    PrimeMover.RotationHistoryClamp = this.ConstructFloatConfig(0.1f, "f - Rotation engine multipliers", "RotationHistoryClamp", "Just don't touch this.", 0.0f, 1f);
    PrimeMover.DirectionalSwayLateralPosValue = this.ConstructFloatConfig(-0.01f, "h - Directional Sway Values", "Directional Sway Lateral Pos Value", "", -0.1f, 0.1f);
    PrimeMover.DirectionalSwayProjectedPosValue = this.ConstructFloatConfig(-0.04f, "h - Directional Sway Values", "Directional Sway Projected Pos Value", "", -0.1f, 0.1f);
    PrimeMover.DirectionalSwayLateralRotValue = this.ConstructFloatConfig(0.2f, "h - Directional Sway Values", "Directional Sway Lateral Rot Value", "", -0.2f, 0.2f);
    PrimeMover.DirectionalSwayVerticalRotValue = this.ConstructFloatConfig(0.04f, "h - Directional Sway Values", "Directional Sway Vertical Rot Value", "", -0.1f, 0.1f);
    PrimeMover.DirectionalSwayLerpSpeed = this.ConstructFloatConfig(3f, "h - Directional Sway Values", "Directional Sway Lerp Speed", "", 1f, 20f);
    PrimeMover.DirectionalSwayLerpOnAds = this.ConstructFloatConfig(0.25f, "h - Directional Sway Values", "Directional Sway % During ADS", "", 0.0f, 1f);
    PrimeMover.DeadzoneInShortStock = this.ConstructFloatConfig(0.35f, "j - Deadzone Values", "Deadzone % in Short Stock Pose", "", -5f, 5f);
    PrimeMover.DeadzoneInActiveAim = this.ConstructFloatConfig(0.5f, "j - Deadzone Values", "Deadzone % in Active Aim Pose", "", -5f, 5f);
    PrimeMover.DeadzoneInVanilla = this.ConstructFloatConfig(0.65f, "j - Deadzone Values", "Deadzone % in BSG Pose", "", -5f, 5f);
    PrimeMover.DeadzoneInADS = this.ConstructFloatConfig(0.2f, "j - Deadzone Values", "Deadzone % in ADS", "", -5f, 5f);
    PrimeMover.DeadzoneInHighReady = this.ConstructFloatConfig(0.35f, "j - Deadzone Values", "Deadzone % in High Ready", "", -5f, 5f);
    PrimeMover.DeadzoneInLowReady = this.ConstructFloatConfig(0.75f, "j - Deadzone Values", "Deadzone % in Low Ready", "", -5f, 5f);
    PrimeMover.DeadzoneHeadFollowSpeedMulti = this.ConstructFloatConfig(10f, "j - Deadzone Values", "Headfollow Speed Multi", "", 0.0f, 20f);
    PrimeMover.DeadzoneWeightForEfficiency = this.ConstructBoolConfig(true, "j - Deadzone Values", "Deadzone weighted for Efficiency", "Ties Efficiency stat (see documentation) to your camera deadzone; less efficiency will cause more lag in the camera to recenter");
    PrimeMover.DeadzoneCustomWeight = this.ConstructFloatConfig(4f, "j - Deadzone Values", "Deadzone Fixed Weight", "Fixed weight value used strictly for calculating Deadzone. (Overrides dynamic weapon weight)", 0.0f, 20f);
    PrimeMover.DeadzoneCustomErgo = this.ConstructFloatConfig(0.5f, "j - Deadzone Values", "Deadzone Fixed Ergo Norm", "Fixed ergo value (0.0 to 1.0) used strictly for calculating Deadzone. (Overrides dynamic weapon ergo)", 0.0f, 1f);
    PrimeMover.DebugFloat1 = this.ConstructFloatConfig(550f, "b - Adjust main feature values", "Efficiency Indicator Position", "Adjust vertical position of the Efficiency Indicator", 400f, 600f);
    PrimeMover.LoggingUpdateFrequency = this.ConstructFloatConfig(0.5f, "z - Debug", "Logging update frequency", "", 0.1f, 5f);
  }

  private void Update()
  {
    KeyboardShortcut keyboardShortcut = PrimeMover.EnableModKey.Value;
    if (keyboardShortcut.IsDown())
      PrimeMover.EnableMod.Value = !PrimeMover.EnableMod.Value;
    this.DeltaTime += UnityEngine.Time.unscaledDeltaTime - this.DeltaTime;
    this.Time += UnityEngine.Time.unscaledDeltaTime;
    this.DriveLerps();
  }

  private void DriveLerps()
  {
    if ((Singleton<GameWorld>.Instance == null))
      return;
    TIRLUtils.Update(this.DeltaTime);
    EfficiencyController.UpdateEfficiencyLerp(this.DeltaTime);
  }

  private void FixedUpdate()
  {
    if ((Singleton<GameWorld>.Instance == null))
      return;
    this.FixedDeltaTime += UnityEngine.Time.unscaledDeltaTime - this.FixedDeltaTime;
    if ((double) this.FixedDeltaTime <= (double) this.FTDThresh || !PrimeMover.IsLogging.Value)
      return;
    TIRLUtils.LogError($"fixed deltaTime exceeds limits : {this.FixedDeltaTime} / 0.01666 -- movement updates skipped this frame");
  }

  private void LateUpdate()
  {
    SwayController.IsSwayUpdatedThisFrame = false;
    if ((Singleton<GameWorld>.Instance == null))
      return;
    float num = Mathf.Min(UnityEngine.Time.unscaledDeltaTime, 0.02f);
    PlayerMotionController.UpdateMovementMeasurementsInFDT(num);
    WeaponSelectionController.UpdateAnimationPump(num);
    NewDeadzoneController.Update(num);
    NewSwayController.UpdateLerp(num);
    HeadRotController.UpdateLerp(num);
    DirectionalSwayController.UpdateDirectionalSwayLerp(num);
    FootstepController.UpdateStep(num);
    SwayController.UpdateLerp(num);
    ParallaxAdsController.UpdateLerps(num);
    ParallaxController.Update(num);
  }

  private void TryLoadPatch(ModulePatch patch)
  {
    try
    {
      patch.Enable();
    }
    catch (Exception ex)
    {
      patch.ToString();
      throw;
    }
  }

  private ConfigEntry<float> ConstructFloatConfig(
    float defaultValue,
    string category,
    string descriptionShort,
    string descriptionFull,
    float min,
    float max)
  {
    return this.Config.Bind<float>(category, descriptionShort, defaultValue, new ConfigDescription(descriptionFull, (AcceptableValueBase) new AcceptableValueRange<float>(min, max), Array.Empty<object>()));
  }

  private ConfigEntry<bool> ConstructBoolConfig(
    bool defaultValue,
    string category,
    string descriptionShort,
    string descriptionFull)
  {
    return this.Config.Bind<bool>(category, descriptionShort, defaultValue, new ConfigDescription(descriptionFull, (AcceptableValueBase) null, Array.Empty<object>()));
  }

  private void OnGUI()
  {
    if ((Singleton<GameWorld>.Instance == null) || !PrimeMover.IsEfficiencyIndicator.Value)
      return;
    float num1 = 40f;
    this.efficiencyIndicatorStyle.normal.textColor = Color.grey;
    this.efficiencyIndicatorStyle.fontSize = 30;
    float num2 = (float) ((double) Screen.width / 2.0 + (double) num1 * (double) EfficiencyController.EfficiencyModifier);
    float num3 = (float) ((double) Screen.width / 2.0 + (double) num1 * (double) EfficiencyController.EfficiencyModifier * -1.0);
    float num4 = (float) Screen.height / 2f + PrimeMover.DebugFloat1.Value;
    GUI.Label(new Rect(num2, num4, 40f, 40f), ".", this.efficiencyIndicatorStyle);
    GUI.Label(new Rect(num3, num4, 40f, 40f), ".", this.efficiencyIndicatorStyle);
  }
}

