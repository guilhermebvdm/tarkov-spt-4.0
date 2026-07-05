// Decompiled with JetBrains decompiler
// Type: TarkovIRL.RealismWrapper
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using UnityEngine;
using EFT;
using Comfort.Common;
using System.Reflection;

#nullable disable
namespace TarkovIRL;

internal class RealismWrapper
{
  private static FieldInfo _isTacSprintActiveField;

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
      return UnderFire.Plugin.isAdrenalineActive;
    }
  }

  public static float WeaponBalanceMulti
  {
    get => 1.0f; // Simplified native balance
  }

  public static bool IsTacSprint
  {
    get
    {
      if (_isTacSprintActiveField == null)
      {
          var type = System.Type.GetType("CameraRotationMod.StanceManager, shwngFpsCameraStances4");
          if (type != null)
          {
              _isTacSprintActiveField = type.GetField("_isTacSprintActive", BindingFlags.NonPublic | BindingFlags.Static);
          }
      }
      if (_isTacSprintActiveField != null)
      {
          return (bool)_isTacSprintActiveField.GetValue(null);
      }
      return false;
    }
  }

  public static bool IsOverdose => false;
}
