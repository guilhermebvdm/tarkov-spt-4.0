// Decompiled with JetBrains decompiler
// Type: RealismMod.SetMalfRepairSpeedPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetMalfRepairSpeedPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("SetMalfRepairSpeed", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void Prefix(FirearmsAnimator __instance, float fix)
  {
    Player yourPlayer = Utils.GetYourPlayer();
    if (Object.op_Equality((Object) yourPlayer, (Object) null))
      return;
    Player.FirearmController handsController = yourPlayer.HandsController as Player.FirearmController;
    if (Object.op_Equality((Object) handsController, (Object) null) || ((Player.AbstractHandsController) handsController).FirearmsAnimator != __instance)
      return;
    float num = Mathf.Clamp(fix * WeaponStats.TotalFixSpeed * PlayerState.ReloadInjuryMulti * PluginConfig.GlobalFixSpeedMulti.Value * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus, 0.7f, 1.15f);
    WeaponAnimationSpeedControllerClass.SetSpeedFix(((ObjectInHandsAnimator) __instance).Animator, num);
    ((ObjectInHandsAnimator) __instance).SetAnimationSpeed(num);
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===SetMalfRepairSpeed===");
      ModulePatch.Logger.LogWarning((object) ("SetMalfRepairSpeed = " + num.ToString()));
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
