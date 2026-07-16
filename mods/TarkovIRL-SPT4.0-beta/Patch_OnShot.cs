using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_OnShot : ModulePatch
{
  protected override MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("OnMakingShot", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player __instance)
  {
    if ((__instance == null) || !__instance.IsYourPlayer)
      return;
    Player.FirearmController handsController = __instance.HandsController as Player.FirearmController;
    WeaponController.UpdateWpnStats(handsController);
    if (!WeaponController.HasCheekWeld() || !((Player.AbstractHandsController) handsController).IsAiming || !PrimeMover.EnableShotParallax.Value || !PrimeMover.EnableMod.Value)
      return;
    ParallaxAdsController.StartNewShot(handsController.Weapon);
  }
}

