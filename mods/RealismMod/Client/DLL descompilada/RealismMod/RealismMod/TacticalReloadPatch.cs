// Decompiled with JetBrains decompiler
// Type: RealismMod.TacticalReloadPatch
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

public class TacticalReloadPatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    TacticalReloadPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    TacticalReloadPatch.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("get_TacticalReload", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Patch(ProceduralWeaponAnimation __instance, ref bool __result)
  {
    Player.FirearmController firearmController = (Player.FirearmController) TacticalReloadPatch.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return false;
    Player player = (Player) TacticalReloadPatch._playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || !StanceController.IsMounting || !WeaponStats.BipodIsDeployed || StanceController.BracingDirection != EBracingDirection.Top)
      return true;
    __result = true;
    return false;
  }
}
