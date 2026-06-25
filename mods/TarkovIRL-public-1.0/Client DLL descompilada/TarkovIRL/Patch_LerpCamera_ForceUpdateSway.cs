// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_LerpCamera_ForceUpdateSway
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

public class Patch_LerpCamera_ForceUpdateSway : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    Patch_LerpCamera_ForceUpdateSway.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_LerpCamera_ForceUpdateSway.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("LerpCamera", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance, float dt)
  {
    if (Object.op_Equality((Object) __instance, (Object) null))
      return;
    Player.FirearmController firearmController = (Player.FirearmController) Patch_LerpCamera_ForceUpdateSway.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return;
    Player player = (Player) Patch_LerpCamera_ForceUpdateSway.playerField.GetValue((object) firearmController);
    AnimStateController.SetCurrentBodyAnimState(player.MovementContext.CurrentState.AnimatorStateHash);
    PlayerMotionController.UpdateMovementInformation(player);
    if (player.IsInventoryOpened)
      return;
    __instance.UpdateSwayFactors();
  }
}
