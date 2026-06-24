// Decompiled with JetBrains decompiler
// Type: Patch_LateUpdate_UpdateWpnStats
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using TarkovIRL;
using UnityEngine;

#nullable disable
public class Patch_LateUpdate_UpdateWpnStats : ModulePatch
{
  private static int _weaponHashLastFrame;

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player __instance)
  {
    Player.FirearmController handsController = __instance.HandsController as Player.FirearmController;
    if (Object.op_Equality((Object) handsController, (Object) null) || !__instance.IsYourPlayer)
      return;
    PlayerMotionController.UpdateMovementInformation(__instance);
    AnimStateController.SetCurrentWeaponAnimState(__instance.HandsAnimator.Animator.GetCurrentAnimatorStateInfo(1).nameHash);
    int hashCode = ((Item) handsController.Weapon).Name.GetHashCode();
    if (hashCode != Patch_LateUpdate_UpdateWpnStats._weaponHashLastFrame)
    {
      WeaponController.UpdateWpnStats(handsController);
      WeaponController.SetCurrentWeaponHash(hashCode);
    }
    Patch_LateUpdate_UpdateWpnStats._weaponHashLastFrame = hashCode;
  }
}
