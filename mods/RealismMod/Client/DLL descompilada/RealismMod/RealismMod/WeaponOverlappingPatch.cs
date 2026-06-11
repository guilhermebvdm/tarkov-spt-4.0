// Decompiled with JetBrains decompiler
// Type: RealismMod.WeaponOverlappingPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class WeaponOverlappingPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo weaponLnField;

  protected virtual MethodBase GetTargetMethod()
  {
    WeaponOverlappingPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    WeaponOverlappingPatch.weaponLnField = AccessTools.Field(typeof (Player.FirearmController), "WeaponLn");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("WeaponOverlapping", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void Prefix(Player.FirearmController __instance)
  {
    if (!((Player) WeaponOverlappingPatch.playerField.GetValue((object) __instance)).IsYourPlayer)
      return;
    if (StanceController.CurrentStance == EStance.PatrolStance)
    {
      WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) (float) ((double) WeaponStats.NewWeaponLength * 0.75));
    }
    else
    {
      if (WeaponStats.IsPistol)
      {
        WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) (float) ((double) WeaponStats.NewWeaponLength * 0.85000002384185791));
      }
      else
      {
        if (Plugin.FikaPresent)
        {
          WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) (float) ((double) WeaponStats.NewWeaponLength * 0.800000011920929));
          return;
        }
        switch (StanceController.CurrentStance)
        {
          case EStance.LowReady:
            WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) (float) ((double) WeaponStats.NewWeaponLength * 0.98000001907348633));
            return;
          case EStance.HighReady:
            WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) (float) ((double) WeaponStats.NewWeaponLength * 0.949999988079071));
            return;
          case EStance.ShortStock:
            WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) (float) ((double) WeaponStats.NewWeaponLength * 0.89999997615814209));
            return;
        }
      }
      WeaponOverlappingPatch.weaponLnField.SetValue((object) __instance, (object) WeaponStats.NewWeaponLength);
    }
  }
}
