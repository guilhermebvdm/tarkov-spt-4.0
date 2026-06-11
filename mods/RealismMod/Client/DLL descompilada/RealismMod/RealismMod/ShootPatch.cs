// Decompiled with JetBrains decompiler
// Type: RealismMod.ShootPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ShootPatch : ModulePatch
{
  private static FieldInfo fcField;
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ShootPatch.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    ShootPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("Shoot");
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(ProceduralWeaponAnimation __instance, float str)
  {
    Player.FirearmController firearmController = (Player.FirearmController) ShootPatch.fcField.GetValue((object) __instance);
    Player player = (Player) ShootPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return;
    ShootController.ResetFiringState(__instance, firearmController.Weapon, player);
  }
}
