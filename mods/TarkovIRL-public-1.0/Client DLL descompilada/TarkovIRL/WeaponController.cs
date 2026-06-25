// Decompiled with JetBrains decompiler
// Type: TarkovIRL.WeaponController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.InventoryLogic;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class WeaponController
{
  public static float CurrentWeaponErgoNorm = 0.0f;
  public static float CurrentWeaponWeight = 0.0f;
  public static bool IsStocked = false;
  public static bool IsStockFolded = false;
  public static bool IsPistol = false;
  public static bool SwayThisFrame = false;
  public static bool IsFoldable = false;
  public static bool IsUsingMounted = false;
  private static int _currentWeaponHash = 0;
  private static readonly int _MP5KHash = 25347301;

  public static void UpdateWpnStats(Player.FirearmController fc)
  {
    if (Object.op_Inequality((Object) fc, (Object) null))
    {
      WeaponController.CurrentWeaponWeight = PrimeMover.Instance.WeightAttenuationCurve.Evaluate(((Item) fc.Weapon).TotalWeight);
      WeaponController.CurrentWeaponErgoNorm = PrimeMover.Instance.ErgoAttenuationCurve.Evaluate(fc.TotalErgonomics / 100f);
      WeaponController.IsStocked = WeaponController.CheckForStock(fc.Weapon);
    }
    else
    {
      WeaponController.CurrentWeaponWeight = 0.0f;
      WeaponController.CurrentWeaponErgoNorm = 1f;
    }
  }

  public static void ToggleFolded()
  {
    if (!WeaponController.IsFoldable || AnimStateController.WeaponState != AnimStateController.EWeaponState.IDLE)
      return;
    WeaponController.IsStockFolded = !WeaponController.IsStockFolded;
  }

  private static bool CheckForStock(Weapon weapon)
  {
    bool flag = weapon.WeapClass == "pistol";
    WeaponController.IsPistol = flag;
    if (flag || WeaponController._currentWeaponHash == WeaponController._MP5KHash)
      return false;
    if (weapon.GetFoldable() != null)
    {
      WeaponController.IsFoldable = true;
      WeaponController.IsStockFolded = weapon.Folded;
    }
    return true;
  }

  public static float GetWeaponMulti(bool getInverse)
  {
    float num = WeaponController.CurrentWeaponWeight * (1f - WeaponController.CurrentWeaponErgoNorm);
    return getInverse ? 1f / num : num;
  }

  public static void SetCurrentWeaponHash(int weaponHash)
  {
    WeaponController._currentWeaponHash = weaponHash;
  }

  public static int WeaponHash => WeaponController._currentWeaponHash;

  public static bool HasCheekWeld() => RealismWrapper.IsShoulderContact();
}
