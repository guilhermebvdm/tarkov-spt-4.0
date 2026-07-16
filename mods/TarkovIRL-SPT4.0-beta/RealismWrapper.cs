using BepInEx.Bootstrap;
using Comfort.Common;
using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class RealismWrapper
{
  private static bool? _isUnderFireLoaded;

  public static bool IsUnderFireLoaded
  {
    get
    {
      if (!RealismWrapper._isUnderFireLoaded.HasValue)
        RealismWrapper._isUnderFireLoaded = new bool?(Chainloader.PluginInfos.ContainsKey("com.rpmwpm.UnderFire"));
      return RealismWrapper._isUnderFireLoaded.Value;
    }
  }

  public static float GetRealismReloadSpeed()
  {
    float num = 1f;
    if (Singleton<GameWorld>.Instantiated && (Singleton<GameWorld>.Instance.MainPlayer != null))
      num = 1f;
    return Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse * num, 0.65f, 1.35f);
  }

  public static float GetRealismCheckMagSpeed()
  {
    return Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse, 0.7f, 1.35f);
  }

  public static bool IsShoulderContact() => true;

  public static bool IsAdrenaline
  {
    get => RealismWrapper.IsUnderFireLoaded && UnderFireSoftWrapper.GetAdrenaline();
  }

  public static float WeaponBalanceMulti => 1f;

  public static bool IsOverdose => false;
}

