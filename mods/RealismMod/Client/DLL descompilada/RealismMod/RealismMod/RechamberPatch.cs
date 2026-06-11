// Decompiled with JetBrains decompiler
// Type: RealismMod.RechamberPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RechamberPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("Rechamber");
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(FirearmsAnimator __instance)
  {
    Player player = Utils.GetYourPlayer();
    if (Object.op_Equality((Object) player, (Object) null))
      return;
    Player.FirearmController handsController = player.HandsController as Player.FirearmController;
    if (Object.op_Equality((Object) handsController, (Object) null) || ((Player.AbstractHandsController) handsController).FirearmsAnimator != __instance)
      return;
    if (Plugin.ServerConfig.reload_changes)
    {
      float totalFixSpeed = WeaponStats.TotalFixSpeed;
      float num = Mathf.Clamp((!(WeaponStats._WeapClass == "pistol") ? totalFixSpeed * PluginConfig.GlobalRechamberSpeedMulti.Value : totalFixSpeed * PluginConfig.RechamberPistolSpeedMulti.Value) * PlayerState.FixSkillMulti * PlayerState.ReloadInjuryMulti * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus, 0.7f, 1.75f);
      player.ExecuteSkill((Action) (() => player.Skills.WeaponFixAction.Complete(1f)));
      ((ObjectInHandsAnimator) ((Player.AbstractHandsController) handsController).FirearmsAnimator).SetAnimationSpeed(num);
      if (PluginConfig.EnableReloadLogging.Value)
      {
        ModulePatch.Logger.LogWarning((object) "===Rechamber===");
        ModulePatch.Logger.LogWarning((object) ("Rechamber = " + num.ToString()));
        ModulePatch.Logger.LogWarning((object) "=============");
      }
    }
    StanceController.CancelShortStock = true;
    StanceController.CancelLeftShoulder = true;
  }
}
