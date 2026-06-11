// Decompiled with JetBrains decompiler
// Type: RealismMod.Health.ScreenEffectsController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using UnityEngine;

#nullable disable
namespace RealismMod.Health;

public static class ScreenEffectsController
{
  private static float _radiationEffectStrength;
  private static float _blurEffectStrength;
  private static float _chromaEffectStrength;

  public static PrismEffects PrismEffects { get; set; }

  public static void EffectsUpdate()
  {
    if (!Object.op_Inequality((Object) ScreenEffectsController.PrismEffects, (Object) null))
      return;
    ScreenEffectsController.DoRadiationEffects();
    ScreenEffectsController.DoAdrenalineAndOverdose();
  }

  public static void DoAdrenalineAndOverdose()
  {
    ScreenEffectsController.PrismEffects.useChromaticAberration = true;
    ScreenEffectsController.PrismEffects.useChromaticBlur = true;
    ScreenEffectsController.PrismEffects.chromaticDistanceOne = -0.1f;
    ScreenEffectsController.PrismEffects.chromaticDistanceTwo = 0.7f;
    bool adrenalineEffect = Plugin.RealHealthController.HasNegativeAdrenalineEffect;
    bool hasOverdosedOnStim = Plugin.RealHealthController.HasOverdosedOnStim;
    float num1 = adrenalineEffect ? Mathf.Lerp(0.8f, 7f, Mathf.PingPong(Time.time * 0.5f, 1f)) : 0.0f;
    float num2 = hasOverdosedOnStim ? Mathf.Lerp(0.05f, 0.2f, Mathf.PingPong(Time.time * 0.2f, 1f)) : 0.0f;
    ScreenEffectsController._blurEffectStrength = Mathf.Lerp(ScreenEffectsController._blurEffectStrength, num1, 0.025f);
    ScreenEffectsController._chromaEffectStrength = Mathf.Lerp(ScreenEffectsController._chromaEffectStrength, num2, 0.01f);
    ScreenEffectsController.PrismEffects.chromaticBlurWidth = ScreenEffectsController._blurEffectStrength;
    ScreenEffectsController.PrismEffects.SetChromaticIntensity(ScreenEffectsController._chromaEffectStrength);
  }

  public static void DoRadiationEffects()
  {
    float num1 = (float) ((double) HazardTracker.TotalRadiationRate * 30.0 + (double) HazardTracker.TotalRadiation * 0.15000000596046448);
    float num2 = (double) num1 <= 2.75 ? 0.0f : num1;
    ScreenEffectsController._radiationEffectStrength = Mathf.Lerp(ScreenEffectsController._radiationEffectStrength, num2, 0.008f);
    if (PluginConfig.ShowRadEffects.Value)
    {
      ScreenEffectsController.PrismEffects.noiseIntensity = Mathf.Max(ScreenEffectsController._radiationEffectStrength, 0.0f);
      ScreenEffectsController.PrismEffects.useNoise = true;
    }
    else
    {
      ScreenEffectsController.PrismEffects.noiseIntensity = 0.0f;
      ScreenEffectsController.PrismEffects.useNoise = false;
    }
  }
}
