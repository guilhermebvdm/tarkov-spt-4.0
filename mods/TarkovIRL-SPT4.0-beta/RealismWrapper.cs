// Decompiled with JetBrains decompiler
// Type: TarkovIRL.RealismWrapper
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using UnityEngine;
using EFT;
using Comfort.Common;
using System.Runtime.CompilerServices;

#nullable disable
namespace TarkovIRL;

internal class RealismWrapper
{
  private static bool? _isUnderFireLoaded;

  public static bool IsUnderFireLoaded
  {
    get
    {
      if (_isUnderFireLoaded == null)
      {
        _isUnderFireLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rpmwpm.UnderFire");
      }
      return _isUnderFireLoaded.Value;
    }
  }

  public static float GetRealismReloadSpeed()
  {
    // Use native EFT Player ReloadSpeed skill multiplier
    float reloadMulti = 1f;
    if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
    {
        reloadMulti = 1f; // TODO: Fetch from EFT skills
    }
    return Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse * reloadMulti, 0.65f, 1.35f);
  }

  public static float GetRealismCheckMagSpeed()
  {
    return Mathf.Clamp(EfficiencyController.EfficiencyModifierInverse, 0.7f, 1.35f);
  }

  public static bool IsShoulderContact() => true;

  public static bool IsAdrenaline
  {
    get
    {
      if (IsUnderFireLoaded)
      {
        return UnderFireSoftWrapper.GetAdrenaline();
      }
      return false;
    }
  }

  public static float WeaponBalanceMulti
  {
    get => 1.0f; // Simplified native balance
  }

  public static bool IsOverdose => false;
}

internal static class UnderFireSoftWrapper
{
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool GetAdrenaline()
  {
    return UnderFire.Plugin.isAdrenalineActive;
  }
}
