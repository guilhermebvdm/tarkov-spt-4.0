// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_OnShot
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_OnShot : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("OnMakingShot", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player __instance)
  {
    if (!Object.op_Inequality((Object) __instance, (Object) null) || !__instance.IsYourPlayer)
      return;
    Player.FirearmController handsController = __instance.HandsController as Player.FirearmController;
    WeaponController.UpdateWpnStats(handsController);
    if (!WeaponController.HasCheekWeld() || !((Player.AbstractHandsController) handsController).IsAiming)
      return;
    ParallaxAdsController.StartNewShot(handsController.Weapon);
  }
}
