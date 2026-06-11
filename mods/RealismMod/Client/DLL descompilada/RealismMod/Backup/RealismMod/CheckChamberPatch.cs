// Decompiled with JetBrains decompiler
// Type: RealismMod.CheckChamberPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class CheckChamberPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    CheckChamberPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("CheckChamber", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    Player player = (Player) CheckChamberPatch._playerField.GetValue((object) __instance);
    if (Object.op_Equality((Object) player, (Object) null) || !player.IsYourPlayer)
      return;
    if (Plugin.ServerConfig.reload_changes)
    {
      float chamberCheckSpeed = WeaponStats.TotalChamberCheckSpeed;
      float num1;
      switch (WeaponStats._WeapClass)
      {
        case "pistol":
          num1 = chamberCheckSpeed * PluginConfig.GlobalCheckChamberPistolSpeedMulti.Value;
          break;
        case "shotgun":
          num1 = chamberCheckSpeed * PluginConfig.GlobalCheckChamberShotgunSpeedMulti.Value;
          break;
        default:
          num1 = chamberCheckSpeed * PluginConfig.GlobalCheckChamberSpeedMulti.Value;
          break;
      }
      float num2 = Mathf.Clamp(num1 * PlayerState.FixSkillMulti * PlayerState.ReloadInjuryMulti * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus, 0.55f, 1.8f);
      if (((Player.AbstractHandsController) __instance)?.FirearmsAnimator != null)
        ((ObjectInHandsAnimator) ((Player.AbstractHandsController) __instance).FirearmsAnimator).SetAnimationSpeed(num2);
      if (player?.Skills?.WeaponFixAction != null)
        player.ExecuteSkill((Action) (() => player.Skills.WeaponFixAction.Complete(1f)));
      if (PluginConfig.EnableReloadLogging.Value)
      {
        ModulePatch.Logger.LogWarning((object) "===CheckChamber===");
        ModulePatch.Logger.LogWarning((object) ("Check Chamber = " + num2.ToString()));
        ModulePatch.Logger.LogWarning((object) "=============");
      }
    }
    StanceController.CancelLowReady = true;
    StanceController.CancelHighReady = true;
    StanceController.CancelShortStock = true;
    StanceController.CancelLeftShoulder = true;
  }
}
