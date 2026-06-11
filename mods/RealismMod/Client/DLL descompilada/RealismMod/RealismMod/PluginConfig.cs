// Decompiled with JetBrains decompiler
// Type: RealismMod.PluginConfig
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using System;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class PluginConfig
{
  public static ConfigEntry<bool> EnableMaterialSpeed { get; set; }

  public static ConfigEntry<bool> EnableSlopeSpeed { get; set; }

  public static ConfigEntry<bool> EnableZeroShift { get; set; }

  public static ConfigEntry<bool> IncreaseCOI { get; set; }

  public static ConfigEntry<float> DuraMalfReductionThreshold { get; set; }

  public static ConfigEntry<float> DuraMalfThreshold { get; set; }

  public static ConfigEntry<float> MalfMulti { get; set; }

  public static ConfigEntry<float> AimPunchMulti { get; set; }

  public static ConfigEntry<float> ShotResetDelay { get; set; }

  public static ConfigEntry<float> SwayIntensity { get; set; }

  public static ConfigEntry<float> ProceduralIntensity { get; set; }

  public static ConfigEntry<float> RecoilIntensity { get; set; }

  public static ConfigEntry<float> VertMulti { get; set; }

  public static ConfigEntry<float> PistolVertMulti { get; set; }

  public static ConfigEntry<float> HorzMulti { get; set; }

  public static ConfigEntry<float> RifleDispMulti { get; set; }

  public static ConfigEntry<float> PistolDispMulti { get; set; }

  public static ConfigEntry<float> RifleCamMulti { get; set; }

  public static ConfigEntry<float> PistolCamMulti { get; set; }

  public static ConfigEntry<float> CamWiggle { get; set; }

  public static ConfigEntry<float> CamReturn { get; set; }

  public static ConfigEntry<bool> EnableAngle { get; set; }

  public static ConfigEntry<float> RecoilAngleMulti { get; set; }

  public static ConfigEntry<float> ConvergenceMulti { get; set; }

  public static ConfigEntry<float> PistolConvergenceMulti { get; set; }

  public static ConfigEntry<float> RiflRecoilDampingMulti { get; set; }

  public static ConfigEntry<float> PistolRecoilDampingMulti { get; set; }

  public static ConfigEntry<float> HandsDampingMulti { get; set; }

  public static ConfigEntry<bool> EnableCrank { get; set; }

  public static ConfigEntry<bool> EnableBSGVisRecoil { get; set; }

  public static ConfigEntry<float> VisRecoilMulti { get; set; }

  public static ConfigEntry<float> BSGVisRecoilMulti { get; set; }

  public static ConfigEntry<float> RecoilClimbFactor { get; set; }

  public static ConfigEntry<float> RecoilClimbSpeed { get; set; }

  public static ConfigEntry<float> PistolRecClimbFactor { get; set; }

  public static ConfigEntry<float> RecoilDispersionFactor { get; set; }

  public static ConfigEntry<float> RecoilDispersionSpeed { get; set; }

  public static ConfigEntry<float> AfterRecoilRandomness { get; set; }

  public static ConfigEntry<bool> UseFpsRecoilFactor { get; set; }

  public static ConfigEntry<float> RecoilRandomness { get; set; }

  public static ConfigEntry<bool> EnableMuzzleEffects { get; set; }

  public static ConfigEntry<bool> ShowBalance { get; set; }

  public static ConfigEntry<bool> ShowCamRecoil { get; set; }

  public static ConfigEntry<bool> ShowDispersion { get; set; }

  public static ConfigEntry<bool> ShowRecoilAngle { get; set; }

  public static ConfigEntry<bool> ShowSemiROF { get; set; }

  public static ConfigEntry<float> PistolGlobalAimSpeedModifier { get; set; }

  public static ConfigEntry<float> GlobalAimSpeedModifier { get; set; }

  public static ConfigEntry<float> GlobalReloadSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalFixSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalUBGLReloadMulti { get; set; }

  public static ConfigEntry<float> GlobalRechamberSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalShotgunRackSpeedFactor { get; set; }

  public static ConfigEntry<float> GlobalCheckChamberSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalCheckChamberShotgunSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalCheckChamberPistolSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalCheckAmmoPistolSpeedMulti { get; set; }

  public static ConfigEntry<float> GlobalCheckAmmoMulti { get; set; }

  public static ConfigEntry<float> QuickReloadSpeedMulti { get; set; }

  public static ConfigEntry<float> InternalMagReloadMulti { get; set; }

  public static ConfigEntry<float> GlobalBoltSpeedMulti { get; set; }

  public static ConfigEntry<float> RechamberPistolSpeedMulti { get; set; }

  public static ConfigEntry<float> DeafenResetDelay { get; set; }

  public static ConfigEntry<int> HeadsetGain { get; set; }

  public static ConfigEntry<bool> EnableAmbientChanges { get; set; }

  public static ConfigEntry<float> OutdoorAmbientMulti { get; set; }

  public static ConfigEntry<float> IndoorAmbientMulti { get; set; }

  public static ConfigEntry<int> HeadsetNoiseReduction { get; set; }

  public static ConfigEntry<float> GunshotVolume { get; set; }

  public static ConfigEntry<float> PlayerMovementVolume { get; set; }

  public static ConfigEntry<float> NPCMovementVolume { get; set; }

  public static ConfigEntry<float> SharedMovementVolume { get; set; }

  public static ConfigEntry<float> ADSVolume { get; set; }

  public static ConfigEntry<KeyboardShortcut> IncGain { get; set; }

  public static ConfigEntry<KeyboardShortcut> DecGain { get; set; }

  public static ConfigEntry<float> ArmorDurabilityModifier { get; set; }

  public static ConfigEntry<float> DragModifier { get; set; }

  public static ConfigEntry<bool> EnablePlateChanges { get; set; }

  public static ConfigEntry<float> GlobalDamageModifier { get; set; }

  public static ConfigEntry<bool> EnableBodyHitZones { get; set; }

  public static ConfigEntry<bool> EnableHitSounds { get; set; }

  public static ConfigEntry<float> FleshHitSoundMulti { get; set; }

  public static ConfigEntry<float> ArmorCloseHitSoundMulti { get; set; }

  public static ConfigEntry<float> ArmorFarHitSoundMulti { get; set; }

  public static ConfigEntry<bool> CanDisarmPlayer { get; set; }

  public static ConfigEntry<bool> CanDisarmBot { get; set; }

  public static ConfigEntry<float> DisarmBaseChance { get; set; }

  public static ConfigEntry<bool> CanFellPlayer { get; set; }

  public static ConfigEntry<bool> CanFellBot { get; set; }

  public static ConfigEntry<float> FallBaseChance { get; set; }

  public static ConfigEntry<bool> EnableAmmoStats { get; set; }

  public static ConfigEntry<float> RagdollForceModifier { get; set; }

  public static ConfigEntry<bool> EnableRagdollFix { get; set; }

  public static ConfigEntry<bool> ShowRadEffects { get; set; }

  public static ConfigEntry<bool> ShowGasEffects { get; set; }

  public static ConfigEntry<float> DeviceVolume { get; set; }

  public static ConfigEntry<float> GasMaskBreathVolume { get; set; }

  public static ConfigEntry<bool> EnableTrueHazardRates { get; set; }

  public static ConfigEntry<KeyboardShortcut> MuteGeigerKey { get; set; }

  public static ConfigEntry<KeyboardShortcut> MuteGasAnalyserKey { get; set; }

  public static ConfigEntry<KeyboardShortcut> ToggleGasMaskKey { get; set; }

  public static ConfigEntry<float> HeartBeatVolume { get; set; }

  public static ConfigEntry<bool> EnableMedNotes { get; set; }

  public static ConfigEntry<bool> ResourceRateChanges { get; set; }

  public static ConfigEntry<bool> PassiveRegen { get; set; }

  public static ConfigEntry<float> EnergyRateMulti { get; set; }

  public static ConfigEntry<float> HydrationRateMulti { get; set; }

  public static ConfigEntry<bool> GearBlocksHeal { get; set; }

  public static ConfigEntry<bool> GearBlocksEat { get; set; }

  public static ConfigEntry<bool> EnableAdrenaline { get; set; }

  public static ConfigEntry<bool> EnableTrnqtEffect { get; set; }

  public static ConfigEntry<bool> EnableHealthEffects { get; set; }

  public static ConfigEntry<KeyboardShortcut> DropGearKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> ActiveAimKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> LowReadyKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> HighReadyKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> ShortStockKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> CycleStancesKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> PatrolKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> MeleeKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> StanceWheelComboKeyBind { get; set; }

  public static ConfigEntry<float> StanceSfxModifier { get; set; }

  public static ConfigEntry<bool> EnableAnimationFixes { get; set; }

  public static ConfigEntry<bool> ModifyBSGCollision { get; set; }

  public static ConfigEntry<bool> OverrideCollision { get; set; }

  public static ConfigEntry<bool> OverrideMounting { get; set; }

  public static ConfigEntry<bool> UseMouseWheelStance { get; set; }

  public static ConfigEntry<bool> UseMouseWheelPlusKey { get; set; }

  public static ConfigEntry<bool> EnableFSPatch { get; set; }

  public static ConfigEntry<bool> EnableNVGPatch { get; set; }

  public static ConfigEntry<bool> EnableMountUI { get; set; }

  public static ConfigEntry<bool> ToggleActiveAim { get; set; }

  public static ConfigEntry<bool> ActiveAimReload { get; set; }

  public static ConfigEntry<bool> EnableAltPistol { get; set; }

  public static ConfigEntry<bool> EnableAltRifle { get; set; }

  public static ConfigEntry<bool> EnableAltRifleRecoil { get; set; }

  public static ConfigEntry<bool> EnableIdleStamDrain { get; set; }

  public static ConfigEntry<float> IdleStamDrainModi { get; set; }

  public static ConfigEntry<bool> EnableStanceStamChanges { get; set; }

  public static ConfigEntry<bool> EnableTacSprint { get; set; }

  public static ConfigEntry<bool> BlockFiring { get; set; }

  public static ConfigEntry<bool> RememberStanceFiring { get; set; }

  public static ConfigEntry<bool> RememberStanceItem { get; set; }

  public static ConfigEntry<bool> EnableExtraProcEffects { get; set; }

  public static ConfigEntry<bool> EnableSprintPenalty { get; set; }

  public static ConfigEntry<bool> EnableMouseSensPenalty { get; set; }

  public static ConfigEntry<float> LeftShoulderOffset { get; set; }

  public static ConfigEntry<float> StanceRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> StanceTransitionSpeedMulti { get; set; }

  public static ConfigEntry<float> ThirdPersonPositionSpeed { get; set; }

  public static ConfigEntry<float> ThirdPersonRotationSpeed { get; set; }

  public static ConfigEntry<float> ActiveAimPosSpeedMulti { get; set; }

  public static ConfigEntry<float> ActiveAimResetSpeedMulti { get; set; }

  public static ConfigEntry<float> ActiveAimRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> PistolRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> HighReadyRotationMulti { get; set; }

  public static ConfigEntry<float> LowReadyRotationMulti { get; set; }

  public static ConfigEntry<float> ShortStockRotationMulti { get; set; }

  public static ConfigEntry<float> ShortStockAdditionalRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> ActiveAimAdditionalRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> HighReadyAdditionalRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> LowReadyAdditionalRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> PistolAdditionalRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> ActiveAimResetRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> PistolResetRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> HighReadyResetRotationMulti { get; set; }

  public static ConfigEntry<float> LowReadyResetRotationMulti { get; set; }

  public static ConfigEntry<float> ShortStockResetRotationSpeedMulti { get; set; }

  public static ConfigEntry<float> HighReadySpeedMulti { get; set; }

  public static ConfigEntry<float> HighReadyResetSpeedMulti { get; set; }

  public static ConfigEntry<float> LowReadySpeedMulti { get; set; }

  public static ConfigEntry<float> LowReadyResetSpeedMulti { get; set; }

  public static ConfigEntry<float> PistolPosSpeedMulti { get; set; }

  public static ConfigEntry<float> PistolPosResetSpeedMulti { get; set; }

  public static ConfigEntry<float> ShortStockSpeedMulti { get; set; }

  public static ConfigEntry<float> ShortStockResetSpeedMulti { get; set; }

  public static ConfigEntry<Vector3> WeapOffset { get; set; }

  public static ConfigEntry<Vector3> ActiveAimRotation { get; set; }

  public static ConfigEntry<Vector3> PistolRotation { get; set; }

  public static ConfigEntry<Vector3> ActiveThirdPersonPosition { get; set; }

  public static ConfigEntry<Vector3> ActiveThirdPersonRotation { get; set; }

  public static ConfigEntry<Vector3> ActiveAimAdditionalRotation { get; set; }

  public static ConfigEntry<Vector3> ActiveAimResetRotation { get; set; }

  public static ConfigEntry<Vector3> HighReadyThirdPersonPosition { get; set; }

  public static ConfigEntry<Vector3> HighReadyThirdPersonRotation { get; set; }

  public static ConfigEntry<Vector3> HighReadyAdditionalRotation { get; set; }

  public static ConfigEntry<Vector3> HighReadyResetRotation { get; set; }

  public static ConfigEntry<Vector3> LowReadyThirdPersonPosition { get; set; }

  public static ConfigEntry<Vector3> LowReadyThirdPersonRotation { get; set; }

  public static ConfigEntry<Vector3> LowReadyAdditionalRotation { get; set; }

  public static ConfigEntry<Vector3> LowReadyResetRotation { get; set; }

  public static ConfigEntry<Vector3> PistolAdditionalRotation { get; set; }

  public static ConfigEntry<Vector3> PistolResetRotation { get; set; }

  public static ConfigEntry<Vector3> ShortStockAdditionalRotation { get; set; }

  public static ConfigEntry<Vector3> ShortStockResetRotation { get; set; }

  public static ConfigEntry<Vector3> PistolThirdPersonPosition { get; set; }

  public static ConfigEntry<Vector3> PistolThirdPersonRotation { get; set; }

  public static ConfigEntry<Vector3> PistolOffset { get; set; }

  public static ConfigEntry<Vector3> ActiveAimOffset { get; set; }

  public static ConfigEntry<Vector3> LowReadyOffset { get; set; }

  public static ConfigEntry<Vector3> LowReadyRotation { get; set; }

  public static ConfigEntry<Vector3> HighReadyOffset { get; set; }

  public static ConfigEntry<Vector3> HighReadyRotation { get; set; }

  public static ConfigEntry<Vector3> ShortStockThirdPersonPosition { get; set; }

  public static ConfigEntry<Vector3> ShortStockThirdPersonRotation { get; set; }

  public static ConfigEntry<Vector3> ShortStockOffset { get; set; }

  public static ConfigEntry<Vector3> ShortStockRotation { get; set; }

  public static ConfigEntry<Vector3> ShortStockReadyOffset { get; set; }

  public static ConfigEntry<Vector3> ShortStockReadyRotation { get; set; }

  public static ConfigEntry<bool> DevMode { get; set; }

  public static ConfigEntry<bool> ZoneDebug { get; set; }

  public static ConfigEntry<string> TargetZone { get; set; }

  public static ConfigEntry<bool> EnableGeneralLogging { get; set; }

  public static ConfigEntry<bool> EnableMedicalLogging { get; set; }

  public static ConfigEntry<bool> EnableReloadLogging { get; set; }

  public static ConfigEntry<bool> EnablePWALogging { get; set; }

  public static ConfigEntry<bool> EnableRecoilLogging { get; set; }

  public static ConfigEntry<bool> EnableBallisticsLogging { get; set; }

  public static ConfigEntry<float> test1 { get; set; }

  public static ConfigEntry<float> test2 { get; set; }

  public static ConfigEntry<float> test3 { get; set; }

  public static ConfigEntry<float> test4 { get; set; }

  public static ConfigEntry<float> test5 { get; set; }

  public static ConfigEntry<float> test6 { get; set; }

  public static ConfigEntry<float> test7 { get; set; }

  public static ConfigEntry<float> test8 { get; set; }

  public static ConfigEntry<float> test9 { get; set; }

  public static ConfigEntry<float> test10 { get; set; }

  public static ConfigEntry<KeyboardShortcut> AddEffectKeybind { get; set; }

  public static ConfigEntry<KeyboardShortcut> AddZone { get; set; }

  public static ConfigEntry<int> AddEffectBodyPart { get; set; }

  public static ConfigEntry<string> AddEffectType { get; set; }

  public static void InitConfigBindings(ConfigFile config)
  {
    string str1 = ".0. Testing";
    string str2 = ".1. Misc. Settings.";
    string str3 = ".2. Ballistics Settings.";
    string str4 = ".3. Recoil Settings.";
    string str5 = ".4. Advanced Recoil Settings.";
    string str6 = ".5. Stat Display Settings.";
    string str7 = ".6. Weapon Settings.";
    string str8 = ".7. Health and Meds Settings.";
    string str9 = ".8. Hazard Zone Settings.";
    string str10 = ".9. Movement Settings.";
    string str11 = ".10. Deafening and Audio.";
    string str12 = "11. Weapon Speed Modifiers.";
    string str13 = "12. Weapon Stances And Position.";
    string str14 = "13. Weapon Stances Keybinds.";
    string str15 = "14. Active Aim.";
    string str16 = "15. High Ready.";
    string str17 = "16. Low Ready.";
    string str18 = "17. Pistol Position And Stance.";
    string str19 = "18. Short-Stocking.";
    string str20 = "19. Third Person Animations.";
    PluginConfig.test1 = config.Bind<float>(str1, "test 1", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(170),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test2 = config.Bind<float>(str1, "test 2", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(160 /*0xA0*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test3 = config.Bind<float>(str1, "test 3", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(150),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test4 = config.Bind<float>(str1, "test 4", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(140),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test5 = config.Bind<float>(str1, "test 5", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(130),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test6 = config.Bind<float>(str1, "test 6", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(120),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test7 = config.Bind<float>(str1, "test 7", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test8 = config.Bind<float>(str1, "test 8", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(100),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test9 = config.Bind<float>(str1, "test 9", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(90),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.test10 = config.Bind<float>(str1, "test 10", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-5000f, 5000f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(80 /*0x50*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.AddZone = config.Bind<KeyboardShortcut>(str1, "Create Debug Zone", new KeyboardShortcut((KeyCode) 0, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(70),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.TargetZone = config.Bind<string>(str1, "TargetZone", "", new ConfigDescription("DebugZone", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(65),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.AddEffectType = config.Bind<string>(str1, "Effect Type", "", new ConfigDescription("HeavyBleeding, LightBleeding, Fracture, removeHP, addHP.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(60),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.AddEffectBodyPart = config.Bind<int>(str1, "Body Part Index", 1, new ConfigDescription("Head = 0, Chest = 1, Stomach = 2, Letft Arm, Right Arm, Left Leg, Right Leg, Common (whole body)", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(50),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.AddEffectKeybind = config.Bind<KeyboardShortcut>(str1, "Add Effect Keybind", new KeyboardShortcut((KeyCode) 0, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(40),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.ZoneDebug = config.Bind<bool>(str1, "Enable Zone Debug", false, new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.DevMode = config.Bind<bool>(str1, "Enable Dev Mode", false, new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(29),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.EnableBallisticsLogging = config.Bind<bool>(str1, "Enable Ballistics Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.EnableGeneralLogging = config.Bind<bool>(str1, "Enable General Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.EnableMedicalLogging = config.Bind<bool>(str1, "Enable Medical Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.EnableReloadLogging = config.Bind<bool>(str1, "Enable Reload Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.EnablePWALogging = config.Bind<bool>(str1, "Enable PWA Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.EnableRecoilLogging = config.Bind<bool>(str1, "Enable Recoil Logging", false, new ConfigDescription("Enables Logging For Debug And Dev", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(true)
      }
    }));
    PluginConfig.RecoilIntensity = config.Bind<float>(str4, "Recoil Intensity", 1.75f, new ConfigDescription("Changes The Overall Intenisty Of Recoil. This Will Increase/Decrease Horizontal Recoil, Dispersion, Vertical Recoil. Does Not Affect Recoil Climb Much, Mostly Spread And Visual.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(50),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.VertMulti = config.Bind<float>(str4, "Rifle Vertical Recoil Multi.", 1.05f, new ConfigDescription("Up/Down. Will Also Increase Recoil Climb.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(40),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.PistolVertMulti = config.Bind<float>(str4, "Pistol Vertical Recoil Multi", 3f, new ConfigDescription("Up/Down. Will Also Increase Recoil Climb.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(40),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.HorzMulti = config.Bind<float>(str4, "Horizontal Recoil Multi", 1f, new ConfigDescription("Forward/Back. Will Also Increase Weapon Shake While Firing.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RifleDispMulti = config.Bind<float>(str4, "Rifle Dispersion Recoil Multi", 1f, new ConfigDescription("Spread. Will Also Increase S-Pattern Size.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.PistolDispMulti = config.Bind<float>(str4, "Pistol Dispersion Recoil Multi", 0.4f, new ConfigDescription("Spread. Will Also Increase S-Pattern Size.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RifleCamMulti = config.Bind<float>(str4, "Rifle Camera Recoil Multi.", 1f, new ConfigDescription("Visual Camera Recoil.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.PistolCamMulti = config.Bind<float>(str4, "Pistol Camera Recoil Multi", 0.4f, new ConfigDescription("Visual Camera Recoil.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableAngle = config.Bind<bool>(str4, "Enable Recoil Angle", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Weapons Will Recoil At Different Angles, And Weight Out Front Will Make The Angle More Steep. If Disabled All Recoil Will Be At 90 Degrees.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(7),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RecoilAngleMulti = config.Bind<float>(str4, "Recoil Angle Multi", 1f, new ConfigDescription("Multiplier For Recoil Angle, Lower = Steeper Angle.", (AcceptableValueBase) new AcceptableValueRange<float>(0.8f, 1.2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(5),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ConvergenceMulti = config.Bind<float>(str4, "Rifle Convergence Multi", 0.6f, new ConfigDescription("AKA Auto-Compensation. Higher = Snappier Recoil, Faster Reset And Tighter Recoil Pattern.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 40f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(2),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.PistolConvergenceMulti = config.Bind<float>(str4, "Pistol Convergence Multi", 1.3f, new ConfigDescription("AKA Auto-Compensation. Higher = Snappier Recoil, Faster Reset And Tighter Recoil Pattern.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 40f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.UseFpsRecoilFactor = config.Bind<bool>(str5, "Use FPS Recoil Factor", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Factors in current FPS to keep recoil climb consistent", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(142),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.AfterRecoilRandomness = config.Bind<float>(str5, "Reset Recoil Randomness Multi", 1.15f, new ConfigDescription("Higher = More Deviation From Point Of Aim After Firing", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(140),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RecoilRandomness = config.Bind<float>(str5, "Recoil Randomness", 2.8f, new ConfigDescription("Higher = Recoil Bounces Around More, More Erratic Recoil Pattern", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(135),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.CamReturn = config.Bind<float>(str5, "Camera Recoil Speed", 0.07f, new ConfigDescription("Higher = More Faster Camera Recoil", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 0.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(132),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.CamWiggle = config.Bind<float>(str5, "Camera Recoil Wiggle", 0.82f, new ConfigDescription("Higher = More Camera Wiggle", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 0.9f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(130),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableBSGVisRecoil = config.Bind<bool>(str5, "Enable Additional Visual Recoil", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Enables Additonal Visual Recoil Elements. Makes The Weapon Visually Move More In New Directions While Firing, Doesn't Have A Significant Effect On Spread.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(120),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.BSGVisRecoilMulti = config.Bind<float>(str5, "BSG Visual Recoil Multi", 1f, new ConfigDescription("Multi For All Of The BSG's Visual Recoil Elements, Makes The Weapon Vibrate More While Firing. BSG Visual Recoil Is Affected By Realism Weapon Stats.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.VisRecoilMulti = config.Bind<float>(str5, "Realism Visual Recoil Multi", 1f, new ConfigDescription("Multi For All Of The Mod's Visual Recoil Elements, Makes The Weapon Vibrate More While Firing. Visual Recoil Is Affected By Weapon Stats.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RecoilClimbSpeed = config.Bind<float>(str5, "Recoil Climb Speed Multi", 4f, new ConfigDescription("How Fast The Recoil CLimb Is, Can Make It Feel Smoother Or Snappier.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RecoilClimbFactor = config.Bind<float>(str5, "Recoil Climb Multi", 4f, new ConfigDescription("Multiplier For How Much Non-Pistols Climbs Vertically Per Shot. Weapon's Vertical Recoil Stat Increases This.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 50f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.PistolRecClimbFactor = config.Bind<float>(str5, "Pistol Recoil Climb Multi.", 0.4f, new ConfigDescription("Multiplier For How Much Pistols Vertically Per Shot. Weapon's Vertical Recoil Stat Increases This.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 50f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(29),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RecoilDispersionFactor = config.Bind<float>(str5, "S-Pattern Multi.", 1.2f, new ConfigDescription("Increases The Size The Classic S Pattern. Weapon's Dispersion Stat Increases This.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 50f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RecoilDispersionSpeed = config.Bind<float>(str5, "S-Pattern Speed Multi", 3f, new ConfigDescription("Increases The Speed At Which Recoil Makes The Classic S Pattern.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ShotResetDelay = config.Bind<float>(str5, "Reset Delay", 0.14f, new ConfigDescription("The Time In Seconds That Has To Be Elapsed Before Firing Is Considered Over, Recoil Will Not Reset Until It Is Over.", (AcceptableValueBase) new AcceptableValueRange<float>(-0.1f, 0.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(4),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableCrank = config.Bind<bool>(str5, "Rearward Recoil", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Makes Recoil Go Towards Player's Shoulder Instead Of Forward.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(3),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.HandsDampingMulti = config.Bind<float>(str5, "Rearward Recoil Wiggle Multi", 1f, new ConfigDescription("The Amount Of Rearward Wiggle After Firing.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 1.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(2),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RiflRecoilDampingMulti = config.Bind<float>(str5, "Rifle Vertical Recoil Wiggle Multi", 1f, new ConfigDescription("The Amount Of Vertical Wiggle After Firing.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 1.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.PistolRecoilDampingMulti = config.Bind<float>(str5, "Pistol Vertical Recoil Wiggle Multi", 0.7f, new ConfigDescription("The Amount Of Vertical Wiggle After Firing.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 1.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableMaterialSpeed = config.Bind<bool>(str10, "Enable Ground Material Speed Modifier", (Plugin.ServerConfig.movement_changes ? 1 : 0) != 0, new ConfigDescription("Enables Movement Speed Being Affected By Ground Material (Concrete, Grass, Metal, Glass Etc.)", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.movement_changes)
      }
    }));
    PluginConfig.EnableSlopeSpeed = config.Bind<bool>(str10, "Enable Ground Slope Speed Modifier", false, new ConfigDescription("Enables Slopes Slowing Down Movement. Can Cause Random Speed Slowdowns In Some Small Spots Due To BSG's Bad Map Geometry.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.movement_changes)
      }
    }));
    PluginConfig.EnableSprintPenalty = config.Bind<bool>(str10, "Enable Sprint Aim Penalties", (Plugin.ServerConfig.movement_changes ? 1 : 0) != 0, new ConfigDescription("ADS Out Of Sprint Has A Short Delay, Reduced Aim Speed And Increased Sway. The Longer You Sprint The Bigger The Penalty.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(5),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.DeviceVolume = config.Bind<float>(str9, "Device Volume", 0.45f, new ConfigDescription("Volume Modifier For Geiger And Gas Analyser.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.GasMaskBreathVolume = config.Bind<float>(str9, "Gas Mask Breath Volume", 0.4f, new ConfigDescription("Volume Modifier For Gas Mask SFX.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.EnableTrueHazardRates = config.Bind<bool>(str9, "Display True Hazard Rates", false, new ConfigDescription("Enable To Show The 'True' Hazard Rate, I.E Not Factoring Meds Or Gas Mask.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.ShowRadEffects = config.Bind<bool>(str9, "Visualize Radiation", (Plugin.ServerConfig.enable_hazard_zones ? 1 : 0) != 0, new ConfigDescription("Radiation Causes Visual Noise, The Strength Of Which Depends On Current Radiation Rate And Total Radiation Poisoning.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(40),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.ShowGasEffects = config.Bind<bool>(str9, "Visualize Gas", (Plugin.ServerConfig.enable_hazard_zones ? 1 : 0) != 0, new ConfigDescription("Gas Will Become Visible", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(40),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.MuteGasAnalyserKey = config.Bind<KeyboardShortcut>(str9, "Mute Gas Analysed Key", new KeyboardShortcut((KeyCode) 109, new KeyCode[1]
    {
      (KeyCode) 306
    }), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(50),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.MuteGeigerKey = config.Bind<KeyboardShortcut>(str9, "Mute Geiger Key", new KeyboardShortcut((KeyCode) 109, new KeyCode[1]
    {
      (KeyCode) 308
    }), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(60),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.MuteGasAnalyserKey = config.Bind<KeyboardShortcut>(str9, "Mute Gas Analysed Key", new KeyboardShortcut((KeyCode) 109, new KeyCode[1]
    {
      (KeyCode) 306
    }), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(70),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.ToggleGasMaskKey = config.Bind<KeyboardShortcut>(str9, "Toggle Equip Gas Mask", new KeyboardShortcut((KeyCode) 122, new KeyCode[1]
    {
      (KeyCode) 306
    }), new ConfigDescription("Will Equip Gas Mask From Tac Rig, Armband (Used By Belt Mods), Or Pockets/Special Slots. When Un-Equiping It Will Try To Move Back To Original Slot.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(80 /*0x50*/),
        Browsable = new bool?(Plugin.ServerConfig.enable_hazard_zones)
      }
    }));
    PluginConfig.EnableMedNotes = config.Bind<bool>(str8, "Medical Notifications", (Plugin.ServerConfig.med_changes ? 1 : 0) != 0, new ConfigDescription("Enables Notifications For Medical Status Effects, Healing Etc..", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(130),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.ResourceRateChanges = config.Bind<bool>(str8, "Enable Hydration/Energy Loss Rate Changes", (Plugin.ServerConfig.med_changes ? 1 : 0) != 0, new ConfigDescription("Enables Changes To How Hydration And Energy Loss Rates Are Calculated. They Are Increased By Injuries, Drug Use, Sprinting And Weight.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(120),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.PassiveRegen = config.Bind<bool>(str8, "Enable Passive Regen", (Plugin.ServerConfig.med_changes ? 1 : 0) != 0, new ConfigDescription("Enables Regen Under Certain Conditions, And If THe Player Has Not Taken Damage For Some Time", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(115),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.HydrationRateMulti = config.Bind<float>(str8, "Hydration Drain Rate Multi.", 0.5f, new ConfigDescription("Lower = Less Drain", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 1.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.EnergyRateMulti = config.Bind<float>(str8, "Energy Drain Rate Multi.", 0.3f, new ConfigDescription("Lower = Less Drain", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 1.5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(100),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.EnableTrnqtEffect = config.Bind<bool>(str8, "Enable Tourniquet Effect", (Plugin.ServerConfig.med_changes ? 1 : 0) != 0, new ConfigDescription("Tourniquet Will Drain HP Of The Limb They Are Applied To.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(90),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.GearBlocksEat = config.Bind<bool>(str8, "Gear Blocks Consumption", (Plugin.ServerConfig.med_changes ? 1 : 0) != 0, new ConfigDescription("Gear Blocks Eating & Drinking. This Includes Some Masks & NVGs & Faceshields That Are Toggled On.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(80 /*0x50*/),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.GearBlocksHeal = config.Bind<bool>(str8, "Gear Blocks Healing", false, new ConfigDescription("Gear Blocks Use Of Meds If The Wound Is Covered By It.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(70),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.EnableAdrenaline = config.Bind<bool>(str8, "Adrenaline", (Plugin.ServerConfig.med_changes ? 1 : 0) != 0, new ConfigDescription("If The Player Is Shot or Shot At They Will Get A Painkiller Effect, As Well As Tunnel Vision and Tremors. The Duration And Strength Of These Effects Are Determined By The Stress Resistence Skill.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(55),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.DropGearKeybind = config.Bind<KeyboardShortcut>(str8, "Remove Gear Keybind (Double Press)", new KeyboardShortcut((KeyCode) 112 /*0x70*/, Array.Empty<KeyCode>()), new ConfigDescription("Removes Any Gear That Is Blocking The Healing Of A Wound, It's A Double Press Like Bag Keybind Is.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(50),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.HeartBeatVolume = config.Bind<float>(str8, "Heartbeat SFX Volume", 0.4f, new ConfigDescription("Volume modifier for heartbeat sfx (used for Adrenaline)", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.med_changes)
      }
    }));
    PluginConfig.EnableFSPatch = config.Bind<bool>(str2, "Enable Faceshield Patch", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Faceshields Block ADS Unless The Specfic Stock/Weapon/Faceshield Allows It.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(4),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableNVGPatch = config.Bind<bool>(str2, "Enable NVG/Thermal ADS Patch", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Magnified Optics Block ADS When Using NVGs. Can't Aim With Sights When Using Thermal Goggles.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(5),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableMouseSensPenalty = config.Bind<bool>(str2, "Enable Weight Mouse Sensitivity Penalty", (Plugin.ServerConfig.gear_weight ? 1 : 0) != 0, new ConfigDescription("Instead Of Using Gear Mouse Sens Penalty Stats, It Is Calculated Based On The Gear + Content's Weight As Modified By The Comfort Stat.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.gear_weight)
      }
    }));
    PluginConfig.EnableZeroShift = config.Bind<bool>(str2, "Enable Zero Shift", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Sights Simulate Losing Zero While Firing. The Reticle Has A Chance To Move Off Target. The Chance Is Determined By The Scope And Its Mount's Accuracy Stat, And The Weapon's Recoil. High Quality Scopes And Mounts Won't Lose Zero. SCAR-H Has Worse Zero-Shift.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableAmmoStats = config.Bind<bool>(str3, "Display Ammo Stats", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("Requiures Restart.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(160 /*0xA0*/),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ArmorDurabilityModifier = config.Bind<float>(str3, "Armor Durability Loss Modifier", 1.25f, new ConfigDescription("Modified Armor Durabiltiy Loss Per Shot", (AcceptableValueBase) new AcceptableValueRange<float>(0.25f, 2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(151),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.DragModifier = config.Bind<float>(str3, "Ballistic Coefficient Modifier", 1.25f, new ConfigDescription("Determines The Amount Of Drag On Projectiles. Higher Value = Slower Flight Time And More Drop.", (AcceptableValueBase) new AcceptableValueRange<float>(0.5f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(150),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.GlobalDamageModifier = config.Bind<float>(str3, "Global Damage Modifier", 1f, new ConfigDescription("Lower = Less Damage Received (Except Head) For Bots And Player.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(140),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.EnablePlateChanges = config.Bind<bool>(str3, "Enable Armor Plate Hitbox Changes", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("Reduces The Size Of Armor Plate Hitboxes To Be Closer To Real Life, And Closer To How They Were When First Implemented.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(130),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.EnableBodyHitZones = config.Bind<bool>(str3, "Enable Body Hit Zones", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("Divides Body Into A, C and D Hit Zones Like On IPSC Targets. In Addtion, There Are Upper Arm, Forearm, Thigh, Calf, Neck, Spine And Heart Hit Zones. Each Zone Modifies Damage And Bleed Chance. ", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(120),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.EnableHitSounds = config.Bind<bool>(str3, "Enable Hit Sounds", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("Enables Additional Sounds To Be Played When Hitting The New Body Zones And Armor Hit Sounds By Material.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.FleshHitSoundMulti = config.Bind<float>(str3, "Flesh Hit Sound Multi", 1f, new ConfigDescription("Raises/Lowers New Hit Sounds Volume.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(100),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.ArmorCloseHitSoundMulti = config.Bind<float>(str3, "Close Armor Hit Sound Multi", 1f, new ConfigDescription("Raises/Lowers New Hit Sounds Volume.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(90),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.ArmorFarHitSoundMulti = config.Bind<float>(str3, "Distant Armor Hit Sound Mutli", 1f, new ConfigDescription("Raises/Lowers New Hit Sounds Volume.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(80 /*0x50*/),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.EnableRagdollFix = config.Bind<bool>(str3, "Enable Ragdoll Fix (Experimental)", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("Requiures Restart. Enables Fix For Ragdolls Flying Into The Stratosphere.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(70),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.RagdollForceModifier = config.Bind<float>(str3, "Ragdoll Force Modifier", 0.01f, new ConfigDescription("Requires Ragdoll Fix To Be Enabled.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        IsAdvanced = new bool?(true),
        Order = new int?(60),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.DisarmBaseChance = config.Bind<float>(str3, "Disarm Base Chance.", 1f, new ConfigDescription("The Base Chance To Be Disarmed. 1 = 1% Chance. This Value Is Increased By The Bullet's Kinetic Energy, Reduced By Armor Armor If Hit, And Doubled If Forearm Is Hit.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        IsAdvanced = new bool?(true),
        Order = new int?(50),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.FallBaseChance = config.Bind<float>(str3, "Fall Base Chance", 20f, new ConfigDescription("The Base Chance To Toggle Prone If Shot In Leg. 1 = 1% Chance. This Value Is Increased By The Bullet's Kinetic Energy And Doubled If Calf Is Hit.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        IsAdvanced = new bool?(true),
        Order = new int?(40),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.CanFellBot = config.Bind<bool>(str3, "Enable Bot Knockdown", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("If Hit In The Leg And The Leg Has/Will Have 0 HP, There Is A Chance That Prone Will Be Toggled. Chance Is Modified By Bullet Kinetic EnergyAnd Doubled If Calf Is Hit.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.CanFellPlayer = config.Bind<bool>(str3, "Enable Player Knockdown", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("If Hit In The Leg And The Leg Has/Will Have 0 HP, There Is A Chance That Prone Will Be Toggled. Chance Is Modified By Bullet Kinetic Energy And Doubled If Calf Is Hit.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.CanDisarmBot = config.Bind<bool>(str3, "Can Disarm Bot.", false, new ConfigDescription("If Hit In The Arms, There Is A Chance That The Currently Equipped Weapon Will Be Dropped. Chance Is Modified By Bullet Kinetic Energy And Reduced If Hit Arm Armor, And Doubled If Forearm Is Hit. WARNING: Disarmed Bots Will Become Passive And Not Attack Player, So This Is Disabled By Default.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.CanDisarmPlayer = config.Bind<bool>(str3, "Can Disarm Player", (Plugin.ServerConfig.realistic_ballistics ? 1 : 0) != 0, new ConfigDescription("If Hit In The Arms, There Is A Chance That The Currently Equipped Weapon Will Be Dropped. Chance Is Modified By Bullet Kinetic Energy And Reduced If Hit Arm Armor, And Doubled If Forearm Is Hit.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.realistic_ballistics)
      }
    }));
    PluginConfig.ShowBalance = config.Bind<bool>(str6, "Show Balance Stat", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Requiures Restart. Warning: Showing Too Many Stats On Weapons With Lots Of Slots Makes The Inspect Menu UI Difficult To Use.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(5),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ShowCamRecoil = config.Bind<bool>(str6, "Show Camera Recoil Stat", false, new ConfigDescription("Requiures Restart. Warning: Showing Too Many Stats On Weapons With Lots Of Slots Makes The Inspect Menu UI Difficult To Use.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(4),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ShowDispersion = config.Bind<bool>(str6, "Show Dispersion Stat", false, new ConfigDescription("Requiures Restart. Warning: Showing Too Many Stats On Weapons With Lots Of Slots Makes The Inspect Menu UI Difficult To Use.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(3),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ShowRecoilAngle = config.Bind<bool>(str6, "Show Recoil Angle Stat", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Requiures Restart. Warning: Showing Too Many Stats On Weapons With Lots Of Slots Makes The Inspect Menu UI Difficult To Use..", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(2),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ShowSemiROF = config.Bind<bool>(str6, "Show Semi Auto ROF Stat", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Requiures Restart. Warning: Showing Too Many Stats On Weapons With Lots Of Slots Makes The Inspect Menu UI Difficult To Use.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableMuzzleEffects = config.Bind<bool>(str7, "Enable Muzzle Effects.", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Enanbes Changes To Muzzle Flash, Smoke, Etc. And Makes Their Intensity Dependent On Caliber, Weapon Condition, Attachments Etc.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(40),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.AimPunchMulti = config.Bind<float>(str7, "Aim Punch Intensity.", 0.8f, new ConfigDescription("Amount Of Aim Punch.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(35),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.SwayIntensity = config.Bind<float>(str7, "Sway Intensity.", 1f, new ConfigDescription("Changes The Intensity Of Aim Sway.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(30),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.ProceduralIntensity = config.Bind<float>(str7, "Procedural Intensity.", 1f, new ConfigDescription("Changes The Intensity Of Procedural Animations, Including Sway, Weapon Movement, And Weapon Inertia.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 3f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(20),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.DuraMalfReductionThreshold = config.Bind<float>(str7, "Malfunction Reduction Durability Threshold.", 90f, new ConfigDescription("Durabiltiy-Based Malfunction Chance Is Reduced Until This Durability Threshold Is Exceeded.", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.malf_changes)
      }
    }));
    PluginConfig.DuraMalfThreshold = config.Bind<float>(str7, "Malfunction Durability Threshold", 98f, new ConfigDescription("Malfunction Chance Is Almost 0 Till This Durabiltiy Threshold Is Met, Unless One Of A Number Of Critera Or Thresholds Is Met (Heat, Burst Round Count, Mag Malf Chance, Ammo Malf Chance, Weapon Mod Malf Chance, Subsonic Ammo + Gun That Can't Cycle It, Etc.)", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(8),
        Browsable = new bool?(Plugin.ServerConfig.malf_changes)
      }
    }));
    PluginConfig.MalfMulti = config.Bind<float>(str7, "Malfunction Multi", 0.75f, new ConfigDescription("Malfunction Chance Multiplier.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.malf_changes)
      }
    }));
    PluginConfig.IncreaseCOI = config.Bind<bool>(str7, "Enable Increased Inaccuracy", (Plugin.ServerConfig.recoil_attachment_overhaul ? 1 : 0) != 0, new ConfigDescription("Requires Restart. Increases The Innacuracy Of All Weapons So That MOA/Accuracy Is A More Important Stat.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.DecGain = config.Bind<KeyboardShortcut>(str11, "Reduce Gain Keybind", new KeyboardShortcut((KeyCode) 269, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(120),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.IncGain = config.Bind<KeyboardShortcut>(str11, "Increase Gain Keybind", new KeyboardShortcut((KeyCode) 270, Array.Empty<KeyCode>()), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.HeadsetNoiseReduction = config.Bind<int>(str11, "Headset Impulse Noise Reduction", 0, new ConfigDescription("To What Level Of Amplification The Headset Reduces To When There's Gunfire Or Explosions. It Is Hard-Coded To Not Exceed Current Headset Gain Value", (AcceptableValueBase) new AcceptableValueRange<int>(-10, 15), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(100),
        IsAdvanced = new bool?(false),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.HeadsetGain = config.Bind<int>(str11, "Headset Gain", 2, new ConfigDescription("WARNING: BE CAREFUL INCREASING THIS TOO HIGH! IT MAY DAMAGE YOUR HEARING! Adjusts The Gain Of Equipped Headsets In Real Time, Acts Just Like The Volume Control On IRL Ear Defenders.", (AcceptableValueBase) new AcceptableValueRange<int>(-5, 15), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(90),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.GunshotVolume = config.Bind<float>(str11, "Gunshot Volume", 0.7f, new ConfigDescription("Multiplier For Gunshot Volume, Player and NPC. Higher = Louder.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 2f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(80 /*0x50*/),
        IsAdvanced = new bool?(false),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.EnableAmbientChanges = config.Bind<bool>(str11, "Enable Ambient Audio Changes", false, new ConfigDescription("Enable The Use Of The Ambient Audio Multis. Can Cause Audio Glitches When Transitioning From Indoors To Outdoors ", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(73),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.OutdoorAmbientMulti = config.Bind<float>(str11, "Outdoor Ambient Audio Offset", 0.0f, new ConfigDescription("Adjusts The Ambient Volume With And Without Headsets. Higher = Louder.", (AcceptableValueBase) new AcceptableValueRange<float>(-60f, 50f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(72),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.IndoorAmbientMulti = config.Bind<float>(str11, "Indoor Ambient Audio Offset", -20f, new ConfigDescription("Adjusts The Ambient Volume With And Without Headsets. Higher = Louder.", (AcceptableValueBase) new AcceptableValueRange<float>(-60f, 50f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(70),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.SharedMovementVolume = config.Bind<float>(str11, "Shared Movement Volume Multi", 1f, new ConfigDescription("Multiplier For Player + NPC Sprint Volume. Has To Be Shared Due To BSG Jank.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(60),
        IsAdvanced = new bool?(false),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.NPCMovementVolume = config.Bind<float>(str11, "NPC Movement Volume Multi", 1f, new ConfigDescription("Multiplier For NPC Movement Volume. Includes Walking And Equipment Rattle.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(50),
        IsAdvanced = new bool?(false),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.PlayerMovementVolume = config.Bind<float>(str11, "Player Movement Volume Multi", 1f, new ConfigDescription("Multiplier For Player Movment Volume.  Includes Walking And Equipment Rattle.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(40),
        IsAdvanced = new bool?(false),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.ADSVolume = config.Bind<float>(str11, "ADS Volume Multi", 2.5f, new ConfigDescription("ADS Volume. Higher = Louder.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(20),
        IsAdvanced = new bool?(false),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.DeafenResetDelay = config.Bind<float>(str11, "Deafen Reset Delay", 1f, new ConfigDescription("Dekay Before Deafening And Tunnel Vision To Start Resetting.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.headset_changes)
      }
    }));
    PluginConfig.PistolGlobalAimSpeedModifier = config.Bind<float>(str12, "Pistol Aim Speed Multi.", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(17),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalAimSpeedModifier = config.Bind<float>(str12, "Aim Speed Multi.", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(16 /*0x10*/),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalReloadSpeedMulti = config.Bind<float>(str12, "Magazine Reload Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(15),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalFixSpeedMulti = config.Bind<float>(str12, "Malfunction Fix Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(14),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalUBGLReloadMulti = config.Bind<float>(str12, "UBGL Reload Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(13),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.RechamberPistolSpeedMulti = config.Bind<float>(str12, "Pistol Rechamber Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(12),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalRechamberSpeedMulti = config.Bind<float>(str12, "Rechamber Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(11),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalBoltSpeedMulti = config.Bind<float>(str12, "Bolt Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(10),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalShotgunRackSpeedFactor = config.Bind<float>(str12, "Shotgun Rack Speed Multi", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(9),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalCheckChamberSpeedMulti = config.Bind<float>(str12, "Chamber Check Speed Multi", 1.25f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(8),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalCheckChamberShotgunSpeedMulti = config.Bind<float>(str12, "Shotgun Chamber Check Speed Multi", 1.25f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(7),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalCheckChamberPistolSpeedMulti = config.Bind<float>(str12, "Pistol Chamber Check Speed Multi", 1.25f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(6),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalCheckAmmoPistolSpeedMulti = config.Bind<float>(str12, "Pistol Check Ammo Multi", 1.25f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(5),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.GlobalCheckAmmoMulti = config.Bind<float>(str12, "Check Ammo Multi.", 1.3f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(4),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.QuickReloadSpeedMulti = config.Bind<float>(str12, "Quick Reload Multi", 1.45f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(2),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.InternalMagReloadMulti = config.Bind<float>(str12, "Internal Magazine Reload", 1.15f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(1),
        Browsable = new bool?(Plugin.ServerConfig.recoil_attachment_overhaul)
      }
    }));
    PluginConfig.EnableAnimationFixes = config.Bind<bool>(str13, "De-Jank EFT Animations", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Attempts To Make EFT Certain Animations Less Janky, Like Inventory And Door Animations.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(430),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ModifyBSGCollision = config.Bind<bool>(str13, "Modify BSG Collision", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("If 'Override Collision' Is Enabled, This Will Be Ignored. Tweaks BSG's Handling Of Weapon Collisions. Slows It Down, Makes It Less Janky.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(420),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.OverrideCollision = config.Bind<bool>(str13, "Override Collision", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("If This Is Enabled, 'Modify BSG Collision' Will Be Ignored. Overrides BSG Collision Handling Complately With Custom Solution. Requires FOV Fix Mod To Function Correctly.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(410),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.OverrideMounting = config.Bind<bool>(str13, "Use Realism Mounting System", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Overrides BSG's Mounting System WIth Realism's (That Was Implemented First). Recoil, Stance and Sway Mechanics Are All Built Around Realism's Mounting And Won't Function Correctly With BSG's.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(300),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableExtraProcEffects = config.Bind<bool>(str13, "Enable Extra Weapon Position/Rotation Effects", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Weapon Has A Slight Cant To It based On Ergo. ADS With Gasmask/Faceshield Is Canted. Weapon Cant Increases When Crouching, And Moves Closer To You. Other Sublte Effects.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(280),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.RememberStanceItem = config.Bind<bool>(str13, "Remember Stance After Using Item", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Remember Stance After Actions (Using Items)", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(260),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.RememberStanceFiring = config.Bind<bool>(str13, "Remember Stance After Firing", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Remember Stance After Firing If The Player Was Aiming.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(260),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.BlockFiring = config.Bind<bool>(str13, "Block Shooting While In Stance", false, new ConfigDescription("Blocks Firing While In A Stance, Will Cancel Stance If Attempting To Fire.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(250),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableTacSprint = config.Bind<bool>(str13, "Enable High Ready Sprint Animation", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Enables Usage Of High Ready Sprint Animation When Sprinting From High Ready Position.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(230),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableAltPistol = config.Bind<bool>(str13, "Enable Alternative Pistol Position And ADS", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Pistol Will Be Held Centered And In A Compressed Stance. ADS is animated. If FOV Fix is used, the gun will move to the camera for smoother ADS.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(229),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableAltRifle = config.Bind<bool>(str13, "Enable Alternative Rifle Position And ADS", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Rifle position will be more centered. If FOV Fix is used, the gun will move to the camera for smoother ADS.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(229),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableAltRifleRecoil = config.Bind<bool>(str13, "Enable Alternative Rifle Recoil Override", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("When using alt rifle, let it override recoil. This results in different recoil feel but smoother transition from firing to non-firing ADS state.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(229),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableIdleStamDrain = config.Bind<bool>(str13, "Enable Idle Arm Stamina Drain", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Arm Stamina Will Drain When Not In A Stance (High And Low Ready, Short-Stocking).", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(210),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.IdleStamDrainModi = config.Bind<float>(str13, "Idle Stam Drain Modifer", 1f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(200),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableStanceStamChanges = config.Bind<bool>(str13, "Enable Stance Stamina And Movement Effects", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Enabled Stances And Mounting To Affect Stamina And Movement Speed. Stamina Drain May Not Work Correctly If Disabled. High + Low Ready, Short-Stocking And Pistol Idle Will Regenerate Stamina Faster And Optionally Idle With Rifles Drains Stamina. High Ready Has Faster Sprint Speed And Sprint Accel, Low Ready Has Faster Sprint Accel. Arm Stamina Won't Drain Regular Stamina If It Reaches 0.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(183),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimReload = config.Bind<bool>(str13, "Allow Reload From Active Aim", false, new ConfigDescription("Allows Reload From Magazine While In Active Aim With Speed Bonus.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(190),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.EnableMountUI = config.Bind<bool>(str13, "Enable Mounting UI", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("If Enabled, An Icon On Screen Will Indicate If Player Is Bracing, Mounting And What Side Of Cover They Are On.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(179),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LeftShoulderOffset = config.Bind<float>(str13, "Left Shoulder Offset", -0.13f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(-0.2f, 0.1f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(153),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.WeapOffset = config.Bind<Vector3>(str13, "Rifle Position Offset", new Vector3(-0.04f, -0.015f, 0.0f), new ConfigDescription("Config option 'alt rife' is required. Adjusts The Starting Position Of Rifle On Screen.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(152),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.StanceRotationSpeedMulti = config.Bind<float>(str13, "Stance Rotation Speed Multi", 1f, new ConfigDescription("Adjusts The Speed Of Stance Rotation Changes.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(146),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.StanceTransitionSpeedMulti = config.Bind<float>(str13, "Stance Transition Speed.", 15f, new ConfigDescription("Adjusts The Position Change Speed Between Stances", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 35f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(145),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.StanceSfxModifier = config.Bind<float>(str13, "Stance Sfx Volume Modifier", 2f, new ConfigDescription("Gear rattle volume modifer when doing stance related things", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(153),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.CycleStancesKeybind = config.Bind<KeyboardShortcut>(str14, "Cycle Stances Keybind", new KeyboardShortcut((KeyCode) 0, Array.Empty<KeyCode>()), new ConfigDescription("Cycles Between High, Low Ready and Short-Stocking. Double Click Returns To Idle.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(80 /*0x50*/),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimKeybind = config.Bind<KeyboardShortcut>(str14, "Active Aim Keybind", new KeyboardShortcut((KeyCode) 327, Array.Empty<KeyCode>()), new ConfigDescription("Cants The Weapon Sideways, Improving Hipfire Accuracy.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(90),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ToggleActiveAim = config.Bind<bool>(str14, "Use Toggle For Active Aim", false, new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(100),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyKeybind = config.Bind<KeyboardShortcut>(str14, "High Ready Keybind", new KeyboardShortcut((KeyCode) 326, new KeyCode[1]
    {
      (KeyCode) 308
    }), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(110),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyKeybind = config.Bind<KeyboardShortcut>(str14, "Low Ready Keybind", new KeyboardShortcut((KeyCode) 326, new KeyCode[1]
    {
      (KeyCode) 306
    }), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(120),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockKeybind = config.Bind<KeyboardShortcut>(str14, "Short-Stock Keybind", new KeyboardShortcut((KeyCode) 106, Array.Empty<KeyCode>()), new ConfigDescription("Tucks The Weapon's Stock Under Player's Arm, Shortening The Overall Length Of The Wweapon To Prevent Muzzle Being Pushed Away From Target.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(130),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PatrolKeybind = config.Bind<KeyboardShortcut>(str14, "Patrol/Neutral Stance Keybind", new KeyboardShortcut((KeyCode) 107, Array.Empty<KeyCode>()), new ConfigDescription("Puts The Weapon In A Neutral Position, Improving Arm Stam Regen And Walk Speed. For Maximum Larping.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(155),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.MeleeKeybind = config.Bind<KeyboardShortcut>(str14, "Melee Keybind", new KeyboardShortcut((KeyCode) 0, Array.Empty<KeyCode>()), new ConfigDescription("Strike With Muzzle Or Bayonet Of Equipped Weapon.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(150),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.UseMouseWheelStance = config.Bind<bool>(str14, "Enable Mouse Wheel Stance Switching", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Switches Between High And Low Ready Via Mouse Wheel.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(160 /*0xA0*/),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.UseMouseWheelPlusKey = config.Bind<bool>(str14, "Require Key + Mouse Wheel", (Plugin.ServerConfig.enable_stances ? 1 : 0) != 0, new ConfigDescription("Require Keybind + Mouse Wheel To Change Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(170),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.StanceWheelComboKeyBind = config.Bind<KeyboardShortcut>(str14, "Keybind To Use With Mouse Wheel", new KeyboardShortcut((KeyCode) 306, Array.Empty<KeyCode>()), new ConfigDescription("Key Used In Combination With Mouse Wheel If Enabled ", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        Order = new int?(180),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ThirdPersonRotationSpeed = config.Bind<float>(str20, "Third Person Rotation Speed Multi", 1.5f, new ConfigDescription("Speed Of Stance Rotation Change In Third Person.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(1000),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ThirdPersonPositionSpeed = config.Bind<float>(str20, "Third Person Position Speed Multi", 1f, new ConfigDescription("Speed Of Stance Position Change In Third Person.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(1100),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolThirdPersonPosition = config.Bind<Vector3>(str20, "Pistol Third Person Position", new Vector3(-0.03f, 0.04f, -0.05f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(260),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolThirdPersonRotation = config.Bind<Vector3>(str20, "Pistol Third Person Rotation", new Vector3(0.0f, 15f, 0.0f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(230),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockThirdPersonPosition = config.Bind<Vector3>(str20, "Short-Stock Third Person Position", new Vector3(0.03f, 0.065f, -0.075f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(200),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockThirdPersonRotation = config.Bind<Vector3>(str20, "Short-Stock Third Person Rotation", new Vector3(0.0f, -15f, 0.0f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(170),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveThirdPersonPosition = config.Bind<Vector3>(str20, "Active Aim Third Person Position", new Vector3(-0.02f, -0.02f, 0.02f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(140),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveThirdPersonRotation = config.Bind<Vector3>(str20, "Active Aim Third Person Rotation", new Vector3(0.0f, -35f, 0.0f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(110),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyThirdPersonPosition = config.Bind<Vector3>(str20, "High Ready Third Person Position", new Vector3(0.02f, 0.05f, -0.045f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(80 /*0x50*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyThirdPersonRotation = config.Bind<Vector3>(str20, "High Ready Third Person Rotation", new Vector3(-8f, -25f, 0.0f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(50),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyThirdPersonPosition = config.Bind<Vector3>(str20, "Low Ready Third Person Position", new Vector3(0.01f, -0.025f, 0.0f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(20),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyThirdPersonRotation = config.Bind<Vector3>(str20, "Low Ready Third Person Rotation", new Vector3(24f, 10f, -1f), new ConfigDescription("", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(8),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimAdditionalRotationSpeedMulti = config.Bind<float>(str15, "Active Aim Additonal Rotation Speed Multi.", 2f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(145),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimResetRotationSpeedMulti = config.Bind<float>(str15, "Active Aim Reset Rotation Speed Multi.", 3.5f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(145),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimRotationSpeedMulti = config.Bind<float>(str15, "Active Aim Rotation Speed Multi.", 2f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 10f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(144 /*0x90*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimPosSpeedMulti = config.Bind<float>(str15, "Active Aim Speed Multi", 15f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(143),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimResetSpeedMulti = config.Bind<float>(str15, "Active Aim Reset Speed Multi", 6f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(142),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimOffset = config.Bind<Vector3>(str15, "Active Aim Position", new Vector3(-0.02f, 0.008f, 0.0f), new ConfigDescription("Weapon Position When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(135),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimRotation = config.Bind<Vector3>(str15, "Active Aim Rotation", new Vector3(0.0f, -35f, 0.0f), new ConfigDescription("Weapon Rotation When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(122),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimAdditionalRotation = config.Bind<Vector3>(str15, "Active Aiming Additional Rotation", new Vector3(0.0f, -35f, 0.0f), new ConfigDescription("Additional Seperate Weapon Rotation When Going Into Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(111),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ActiveAimResetRotation = config.Bind<Vector3>(str15, "Active Aiming Reset Rotation", new Vector3(-0.5f, 20.5f, -2f), new ConfigDescription("Weapon Rotation When Going Out Of Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(102),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyAdditionalRotationSpeedMulti = config.Bind<float>(str16, "High Ready Additonal Rotation Speed Multi.", 0.1f, new ConfigDescription("How Fast The Weapon Rotates Going Out Of Stance.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(94),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyResetRotationMulti = config.Bind<float>(str16, "High Ready Reset Rotation Speed Multi.", 1.5f, new ConfigDescription("How Fast The Weapon Rotates Going Out Of Stance.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(93),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyRotationMulti = config.Bind<float>(str16, "High Ready Rotation Speed Multi.", 2f, new ConfigDescription("How Fast The Weapon Rotates Going Into Stance.", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(92),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyResetSpeedMulti = config.Bind<float>(str16, "High Ready Reset Speed Multi", 6.5f, new ConfigDescription("How Fast The Weapon Moves Going Out Of Stance", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(91),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadySpeedMulti = config.Bind<float>(str16, "High Ready Speed Multi", 6f, new ConfigDescription("How Fast The Weapon Moves Going Into Stance", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(90),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyOffset = config.Bind<Vector3>(str16, "High Ready Position", new Vector3(0.005f, 0.035f, -0.04f), new ConfigDescription("Weapon Position When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(85),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyRotation = config.Bind<Vector3>(str16, "High Ready Rotation", new Vector3(-8f, -20f, 0.0f), new ConfigDescription("Weapon Rotation When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(72),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyAdditionalRotation = config.Bind<Vector3>(str16, "High Ready Additional Rotation", new Vector3(-50f, -25f, -5f), new ConfigDescription("Additional Seperate Weapon Rotation When Going Into Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(69),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.HighReadyResetRotation = config.Bind<Vector3>(str16, "High Ready Reset Rotation", new Vector3(0.0f, 2f, 0.0f), new ConfigDescription("Weapon Rotation When Going Out Of Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(66),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyAdditionalRotationSpeedMulti = config.Bind<float>(str17, "Low Ready Additonal Rotation Speed Multi", 0.75f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(64 /*0x40*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyResetRotationMulti = config.Bind<float>(str17, "Low Ready Reset Rotation Speed Multi", 2.25f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(63 /*0x3F*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyRotationMulti = config.Bind<float>(str17, "Low Ready Rotation Speed Multi", 1.5f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(62),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadySpeedMulti = config.Bind<float>(str17, "Low Ready Speed Multi.", 14f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(61),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyResetSpeedMulti = config.Bind<float>(str17, "Low Ready Reset Speed Multi", 8.7f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(60),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyOffset = config.Bind<Vector3>(str17, "Low Ready Position", new Vector3(0.0f, -0.01f, 0.0f), new ConfigDescription("Weapon Position When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(55),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyRotation = config.Bind<Vector3>(str17, "Low Ready Rotation", new Vector3(8f, -5f, -1f), new ConfigDescription("Weapon Rotation When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(42),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyAdditionalRotation = config.Bind<Vector3>(str17, "Low Ready Additional Rotation", new Vector3(12f, -1f, 0.0f), new ConfigDescription("Additional Seperate Weapon Rotation When Going Into Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(39),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.LowReadyResetRotation = config.Bind<Vector3>(str17, "Low Ready Reset Rotation", new Vector3(-1f, 0.0f, 0.0f), new ConfigDescription("Weapon Rotation When Going Out Of Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(36),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolAdditionalRotationSpeedMulti = config.Bind<float>(str18, "Pistol Additional Rotation Speed Multi", 0.1f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(35),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolResetRotationSpeedMulti = config.Bind<float>(str18, "Pistol Reset Rotation Speed Multi", 0.5f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(34),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolRotationSpeedMulti = config.Bind<float>(str18, "Pistol Rotation Speed Multi", 1f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 20f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(33),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolPosSpeedMulti = config.Bind<float>(str18, "Pistol Position Speed Multi", 6f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(32 /*0x20*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolPosResetSpeedMulti = config.Bind<float>(str18, "Pistol Position Reset Speed Multi", 8f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(30),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolOffset = config.Bind<Vector3>(str18, "Pistol Position", new Vector3(0.0f, 0.04f, -0.015f), new ConfigDescription("Weapon Position When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(25),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolRotation = config.Bind<Vector3>(str18, "Pistol Rotation", new Vector3(0.0f, -5f, 0.0f), new ConfigDescription("Weapon Rotation When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(12),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolAdditionalRotation = config.Bind<Vector3>(str18, "Pistol Ready Additional Rotation", new Vector3(0.0f, 0.0f, 0.0f), new ConfigDescription("Additional Seperate Weapon Rotation When Going Into Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(6),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.PistolResetRotation = config.Bind<Vector3>(str18, "Pistol Ready Reset Rotation", new Vector3(-5f, 0.0f, 0.0f), new ConfigDescription("Weapon Rotation When Going Out Of Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(3),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockAdditionalRotationSpeedMulti = config.Bind<float>(str19, "Short-Stock Additional Rotation Speed Multi", 1.5f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(35),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockResetRotationSpeedMulti = config.Bind<float>(str19, "Short-Stock Reset Rotation Speed Multi", 1f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(34),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockRotationMulti = config.Bind<float>(str19, "Short-Stock Rotation Speed Multi", 2f, new ConfigDescription("How Fast The Weapon Rotates.", (AcceptableValueBase) new AcceptableValueRange<float>(0.1f, 5f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(33),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockSpeedMulti = config.Bind<float>(str19, "Short-Stock Position Speed Multi.", 4f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(32 /*0x20*/),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockResetSpeedMulti = config.Bind<float>(str19, "Short-Stock Position Reset Speed Mult", 3.8f, new ConfigDescription("", (AcceptableValueBase) new AcceptableValueRange<float>(1f, 100f), new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(30),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockOffset = config.Bind<Vector3>(str19, "Short-Stock Position", new Vector3(0.02f, 0.1f, -0.025f), new ConfigDescription("Weapon Position When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(25),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockRotation = config.Bind<Vector3>(str19, "Short-Stock Rotation", new Vector3(0.0f, -15f, 0.0f), new ConfigDescription("Weapon Rotation When In Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(12),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockAdditionalRotation = config.Bind<Vector3>(str19, "Short-Stock Ready Additional Rotation", new Vector3(-3f, -15f, 1f), new ConfigDescription("Additional Seperate Weapon Rotation When Going Into Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(6),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
    PluginConfig.ShortStockResetRotation = config.Bind<Vector3>(str19, "Short-Stock Ready Reset Rotation", new Vector3(-1.5f, 2f, 0.0f), new ConfigDescription("Weapon Rotation When Going Out Of Stance.", (AcceptableValueBase) null, new object[1]
    {
      (object) new ConfigurationManagerAttributes()
      {
        ShowRangeAsPercent = new bool?(false),
        Order = new int?(3),
        IsAdvanced = new bool?(true),
        Browsable = new bool?(Plugin.ServerConfig.enable_stances)
      }
    }));
  }

  private static void ResetToDefault(ConfigFile config, ConfigDefinition definition)
  {
    Utils.Logger.LogWarning((object) ("definition key: " + definition.Key));
    Utils.Logger.LogWarning((object) ("definition section: " + definition.Section));
    object defaultValue = config[definition].DefaultValue;
    config[definition].BoxedValue = defaultValue;
  }
}
