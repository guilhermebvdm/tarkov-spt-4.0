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
    if ((fc != null))
    {
      float num1 = PrimeMover.SwayCustomWeight.Value;
      float num2 = PrimeMover.SwayCustomErgo.Value;
      float num3 = (double) num1 > 0.0 ? num1 : ((Item) fc.Weapon).TotalWeight;
      float num4 = (double) num1 > 0.0 ? num2 : fc.TotalErgonomics / 100f;
      WeaponController.CurrentWeaponWeight = PrimeMover.Instance.WeightAttenuationCurve.Evaluate(num3);
      WeaponController.CurrentWeaponErgoNorm = PrimeMover.Instance.ErgoAttenuationCurve.Evaluate(num4);
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
    if ((double) num < (double) PrimeMover.MinimumWeaponSway.Value)
      num = PrimeMover.MinimumWeaponSway.Value;
    return getInverse ? 1f / num : num;
  }

  public static void SetCurrentWeaponHash(int weaponHash)
  {
    WeaponController._currentWeaponHash = weaponHash;
  }

  public static int WeaponHash => WeaponController._currentWeaponHash;

  public static bool HasCheekWeld() => RealismWrapper.IsShoulderContact();
}

