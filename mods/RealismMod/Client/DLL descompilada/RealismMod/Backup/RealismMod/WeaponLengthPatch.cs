// Decompiled with JetBrains decompiler
// Type: RealismMod.WeaponLengthPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class WeaponLengthPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo weapLn;

  protected virtual MethodBase GetTargetMethod()
  {
    WeaponLengthPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    WeaponLengthPatch.weapLn = AccessTools.Field(typeof (Player.FirearmController), "WeaponLn");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_10", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    Player player = (Player) WeaponLengthPatch.playerField.GetValue((object) __instance);
    float num = (float) WeaponLengthPatch.weapLn.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    WeaponStats.BaseWeaponLength = num;
    WeaponStats.NewWeaponLength = (double) num < 0.92000001668930054 ? num * 0.95f : num * 1.05f;
  }
}
