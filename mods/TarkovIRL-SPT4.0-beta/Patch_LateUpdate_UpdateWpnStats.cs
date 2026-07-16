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

  protected override MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player __instance)
  {
    Player.FirearmController handsController = __instance.HandsController as Player.FirearmController;
    if ((handsController == null) || !__instance.IsYourPlayer)
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

