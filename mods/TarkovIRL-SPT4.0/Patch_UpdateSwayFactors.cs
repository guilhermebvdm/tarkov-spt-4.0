// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_UpdateSwayFactors
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_UpdateSwayFactors : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    Patch_UpdateSwayFactors.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_UpdateSwayFactors.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("UpdateSwayFactors", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(ProceduralWeaponAnimation __instance)
  {
    if (Object.op_Equality((Object) __instance, (Object) null) || !PrimeMover.IsWeaponSway.Value || SwayController.IsSwayUpdatedThisFrame)
      return;
    Player.FirearmController firearmController = (Player.FirearmController) Patch_UpdateSwayFactors.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return;
    Player player = (Player) Patch_UpdateSwayFactors.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return;
    Vector3 newSway = SwayController.GetNewSway(__instance.MotionReact.SwayFactors, __instance.IsAiming);
    __instance.MotionReact.SwayFactors = newSway;
    SwayController.IsSwayUpdatedThisFrame = true;
  }
}
