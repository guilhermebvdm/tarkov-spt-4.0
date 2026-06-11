// Decompiled with JetBrains decompiler
// Type: RealismMod.DeafenController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using RealismMod.Health;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class DeafenController
{
  public const float AmbientOutVolume = 0.0f;
  public const float AmbientInVolume = 0.0f;
  public const float WeaponMuffleBase = -70f;
  public const float BaseMainVolume = 0.0f;
  public const float LightHelmetDeafReduction = 0.9f;
  public const float HeavyHelmetDeafreduction = 0.8f;
  public const float MinHeadsetProtection = 0.8f;
  public const float MaxHeadsetProtection = 0.25f;
  public const float HeadsetUpperProtection = 26f;
  public const float HeadestLowerProtection = 16f;
  public const float VignetteFactor = 0.16f;
  public const int MaxGain = 15;
  public const int MinGain = -10;
  public const float MaxMuffle = 15f;
  public const float MaxDeafen = 30f;
  public const float MaxVignette = 2.4f;
  private static float _mainVolumeReduction = 0.0f;
  private static float _mainVolumeReductionTarget = 0.0f;
  private static float _maxShootVolumeReduction = 0.0f;
  private static float _maxBotVolumeReduction = 0.0f;
  private static float _maxShootMuffle = 0.0f;
  private static float _maxBotShootMuffle = 0.0f;
  private static float _muffleTarget = -70f;
  private static float _gunShotMuffle = -70f;
  private static float _vignetteAmount = 0.0f;
  private static float _vignetteTarget = 0.0f;
  private static float _maxShootVignette = 0.0f;
  private static float _maxBotShootVignette = 0.0f;
  private static float _maxExplosionVignette = 0.0f;

  public static float EarProtectionFactor { get; set; } = 1f;

  public static float AmmoDeafFactor { get; set; } = 1f;

  public static float GunDeafFactor { get; set; } = 1f;

  public static float BotFiringDeafFactor { get; set; } = 1f;

  public static float ExplosionDeafFactor { get; set; } = 1f;

  public static bool HasHeadSet { get; set; } = false;

  public static float HeadSetGain { get; set; } = (float) PluginConfig.HeadsetGain.Value;

  public static bool IsBotFiring { get; set; } = false;

  public static bool GrenadeExploded { get; set; } = false;

  public static float BotTimer { get; set; } = 0.0f;

  public static float GrenadeTimer { get; set; } = 0.0f;

  public static float EnvironmentFactor => PlayerState.EnviroType == 1 ? 1.4f : 1f;

  public static void DoLogging()
  {
  }

  public static void IncreaseDeafeningShooting()
  {
    float num1 = DeafenController.AmmoDeafFactor * DeafenController.GunDeafFactor * DeafenController.EarProtectionFactor * DeafenController.EnvironmentFactor;
    float num2 = num1 * 2f;
    float num3 = num1;
    float num4 = num1 * 0.28f;
    DeafenController._mainVolumeReductionTarget = (double) DeafenController._mainVolumeReductionTarget < (double) num2 ? DeafenController._mainVolumeReductionTarget + num1 : DeafenController._mainVolumeReductionTarget;
    DeafenController._maxShootVolumeReduction = Mathf.Max(num2, DeafenController._maxShootVolumeReduction);
    DeafenController._muffleTarget = (double) DeafenController._muffleTarget < (double) num3 ? DeafenController._muffleTarget + num1 * 2f : DeafenController._muffleTarget;
    DeafenController._maxShootMuffle = Mathf.Max(num3, DeafenController._maxShootMuffle);
    DeafenController._vignetteTarget = (double) DeafenController._vignetteTarget < (double) num4 ? DeafenController._vignetteTarget + num1 * 0.16f * DeafenController.EarProtectionFactor : DeafenController._vignetteTarget;
    DeafenController._maxShootVignette = Mathf.Max(num4, DeafenController._maxShootVignette);
  }

  public static void IncreaseDeafeningShotAt()
  {
    float num = DeafenController.BotFiringDeafFactor * DeafenController.EnvironmentFactor;
    DeafenController._mainVolumeReductionTarget += num * 0.5f;
    DeafenController._maxBotVolumeReduction = Mathf.Max(num, DeafenController._maxBotVolumeReduction);
    DeafenController._muffleTarget += num;
    DeafenController._maxBotShootMuffle = Mathf.Max(num, DeafenController._maxBotShootMuffle);
    DeafenController._vignetteTarget += num * 0.09f;
    DeafenController._maxBotShootVignette = Mathf.Max(num * 0.15f, DeafenController._maxBotShootVignette);
  }

  public static void IncreaseDeafeningExplosion()
  {
    float num = DeafenController.ExplosionDeafFactor * DeafenController.EnvironmentFactor;
    DeafenController._mainVolumeReductionTarget += num * 0.2f;
    DeafenController._muffleTarget += num * 0.9f;
    DeafenController._vignetteTarget += num * 0.025f;
    DeafenController._maxExplosionVignette = Mathf.Max(num * 0.035f, DeafenController._maxExplosionVignette);
  }

  public static void SetAudio(float mainVolume)
  {
    Singleton<BetterAudio>.Instance.Master.SetFloat("InGame", mainVolume + DeafenController._mainVolumeReduction);
    Singleton<BetterAudio>.Instance.Master.SetFloat("Tinnitus1", DeafenController._gunShotMuffle);
    if (!PluginConfig.EnableAmbientChanges.Value)
      return;
    float num = DeafenController.HasHeadSet ? 10f : 5f;
    Singleton<BetterAudio>.Instance.Master.SetFloat("AmbientOutVolume", (float) (0.0 + (double) num + (double) PluginConfig.OutdoorAmbientMulti.Value - 7.0));
    Singleton<BetterAudio>.Instance.Master.SetFloat("AmbientInVolume", (float) (0.0 + (double) num + (double) PluginConfig.IndoorAmbientMulti.Value - 15.0));
  }

  public static void DoVignette()
  {
    ScreenEffectsController.PrismEffects.SetVignetteStrength(DeafenController._vignetteAmount);
  }

  public static void DoTimers()
  {
    if (DeafenController.IsBotFiring)
    {
      DeafenController.BotTimer += Time.deltaTime;
      if ((double) DeafenController.BotTimer >= 0.5)
      {
        DeafenController.IsBotFiring = false;
        DeafenController.BotTimer = 0.0f;
      }
    }
    if (!DeafenController.GrenadeExploded)
      return;
    DeafenController.GrenadeTimer += Time.deltaTime;
    if ((double) DeafenController.GrenadeTimer >= 25.0)
    {
      DeafenController.GrenadeExploded = false;
      DeafenController.GrenadeTimer = 0.0f;
    }
  }

  public static void ResetMaxValues()
  {
    if (Utils.AreFloatsEqual(DeafenController._mainVolumeReduction, 0.0f, 2f))
    {
      DeafenController._maxShootVolumeReduction = 0.0f;
      DeafenController._maxBotVolumeReduction = 0.0f;
    }
    if (Utils.AreFloatsEqual(DeafenController._gunShotMuffle, -70f, 5f))
    {
      DeafenController._maxShootMuffle = 0.0f;
      DeafenController._maxBotShootMuffle = 0.0f;
    }
    if (!Utils.AreFloatsEqual(DeafenController._vignetteAmount, 0.0f, 0.1f))
      return;
    if (ScreenEffectsController.PrismEffects.useVignette)
      ScreenEffectsController.PrismEffects.useVignette = false;
    DeafenController._maxShootVignette = 0.0f;
    DeafenController._maxBotShootVignette = 0.0f;
    DeafenController._maxExplosionVignette = 0.0f;
  }

  public static void ResetValues()
  {
    DeafenController._mainVolumeReductionTarget = 0.0f;
    DeafenController._muffleTarget = -70f;
    DeafenController._vignetteTarget = 0.0f;
    DeafenController._mainVolumeReduction = Mathf.Lerp(DeafenController._mainVolumeReduction, 0.0f, 0.0009f);
    DeafenController._gunShotMuffle = Mathf.Lerp(DeafenController._gunShotMuffle, -70f, 0.0014f);
    DeafenController._vignetteAmount = Mathf.Lerp(DeafenController._vignetteAmount, 0.0f, 0.01f);
  }

  public static void SetValues()
  {
    float num1 = Mathf.Min(30f, DeafenController._maxShootVolumeReduction + DeafenController._maxBotVolumeReduction);
    DeafenController._mainVolumeReduction = Mathf.Lerp(DeafenController._mainVolumeReduction, Mathf.Max(-DeafenController._mainVolumeReductionTarget, -num1), 0.01f);
    float num2 = Mathf.Min(15f, DeafenController._maxShootMuffle + DeafenController._maxBotShootMuffle);
    DeafenController._gunShotMuffle = Mathf.Lerp(DeafenController._gunShotMuffle, Mathf.Min(DeafenController._muffleTarget, num2), 0.035f);
    float num3 = Mathf.Min(2.4f, DeafenController._maxShootVignette + DeafenController._maxBotShootVignette + DeafenController._maxExplosionVignette);
    DeafenController._vignetteAmount = Mathf.Lerp(DeafenController._vignetteAmount, Mathf.Min(DeafenController._vignetteTarget, num3), 0.1f);
    ScreenEffectsController.PrismEffects.useVignette = true;
    ScreenEffectsController.PrismEffects.vignetteStart = 1.5f;
    ScreenEffectsController.PrismEffects.vignetteEnd = 0.1f;
  }

  public static void DoDeafening()
  {
    float mainVolume = 0.0f;
    if (DeafenController.IsBotFiring || DeafenController.GrenadeExploded || ShootController.IsFiringDeafen)
    {
      if (DeafenController.HasHeadSet)
        mainVolume = Mathf.Min((float) PluginConfig.HeadsetNoiseReduction.Value, (float) PluginConfig.HeadsetGain.Value - 5f);
      DeafenController.SetValues();
    }
    else
    {
      if (DeafenController.HasHeadSet)
        mainVolume = (float) PluginConfig.HeadsetGain.Value;
      DeafenController.ResetValues();
      DeafenController.ResetMaxValues();
    }
    DeafenController.SetAudio(mainVolume);
    DeafenController.DoVignette();
    DeafenController.DoTimers();
    if (!PluginConfig.EnableGeneralLogging.Value)
      return;
    DeafenController.DoLogging();
  }
}
