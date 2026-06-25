// Decompiled with JetBrains decompiler
// Type: TarkovIRL.RealismWrapper
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using RealismMod;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class RealismWrapper
{
  private static RealismHealthController _realHealth;
  private static float _blurEffectStrength;
  private static float _chromaEffectStrength;

  public static float GetRealismReloadSpeed()
  {
    return Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse * WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadSkillMulti * PlayerState.GearErgoPenalty * StanceController.ActiveAimManipBuff, 0.65f, 1.35f);
  }

  public static float GetRealismCheckMagSpeed()
  {
    float num = PluginConfig.GlobalCheckAmmoMulti.Value;
    if (WeaponStats._WeapClass == "pistol")
      num = PluginConfig.GlobalCheckAmmoPistolSpeedMulti.Value;
    return Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse * WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadSkillMulti * num, 0.7f, 1.35f);
  }

  public static bool IsShoulderContact() => WeaponStats.HasShoulderContact;

  public static bool IsAdrenaline
  {
    get
    {
      if (RealismWrapper._realHealth == null)
        RealismWrapper._realHealth = Plugin.RealHealthController;
      return RealismWrapper._realHealth != null && (RealismWrapper._realHealth.HasNegativeAdrenalineEffect || RealismWrapper._realHealth.HasPositiveAdrenalineEffect);
    }
  }

  public static float WeaponBalanceMulti
  {
    get => (float) (1.0 + (double) Mathf.Pow(WeaponStats.Balance, 2f) * (1.0 / 1000.0));
  }

  public static bool IsTacSprint => StanceController.IsDoingTacSprint;

  public static bool IsOverdose => false;
}
