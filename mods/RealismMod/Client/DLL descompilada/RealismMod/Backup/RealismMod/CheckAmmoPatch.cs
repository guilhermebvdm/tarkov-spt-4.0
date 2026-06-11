// Decompiled with JetBrains decompiler
// Type: RealismMod.CheckAmmoPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class CheckAmmoPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    CheckAmmoPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("CheckAmmo", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    if (!((Player) CheckAmmoPatch._playerField.GetValue((object) __instance)).IsYourPlayer)
      return;
    if (Plugin.ServerConfig.reload_changes)
    {
      float num1 = PluginConfig.GlobalCheckAmmoMulti.Value;
      if (WeaponStats._WeapClass == "pistol")
        num1 = PluginConfig.GlobalCheckAmmoPistolSpeedMulti.Value;
      float num2 = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadSkillMulti * PlayerState.ReloadInjuryMulti * StanceController.HighReadyManipBuff * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus * num1, 0.7f, 1.35f);
      ((ObjectInHandsAnimator) ((Player.AbstractHandsController) __instance).FirearmsAnimator).SetAnimationSpeed(num2);
      if (PluginConfig.EnableReloadLogging.Value)
      {
        ModulePatch.Logger.LogWarning((object) "===CheckAmmo===");
        ModulePatch.Logger.LogWarning((object) ("Check Ammo =" + num2.ToString()));
        ModulePatch.Logger.LogWarning((object) "=============");
      }
    }
    StanceController.CancelLeftShoulder = true;
    StanceController.CancelLowReady = true;
    StanceController.CancelShortStock = true;
    StanceController.CancelActiveAim = true;
    StanceController.ModifyHighReady = true;
    StanceController.ManipTimer = 0.0f;
  }
}
