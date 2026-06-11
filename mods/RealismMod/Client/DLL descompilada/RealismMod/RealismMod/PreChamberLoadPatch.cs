// Decompiled with JetBrains decompiler
// Type: RealismMod.PreChamberLoadPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class PreChamberLoadPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_18", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void Prefix(Player.FirearmController __instance)
  {
    if (!__instance.Weapon.HasChambers || __instance.Weapon.Chambers.Length != 1 || __instance.Weapon.ChamberAmmoCount != 0 || __instance.IsStationaryWeapon)
      return;
    Plugin.BlockChambering = true;
  }
}
