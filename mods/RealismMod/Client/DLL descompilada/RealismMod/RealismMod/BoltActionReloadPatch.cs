// Decompiled with JetBrains decompiler
// Type: RealismMod.BoltActionReloadPatch
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

public class BoltActionReloadPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    BoltActionReloadPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("InitiateShot", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(Player.FirearmController __instance)
  {
    if (!((Player) BoltActionReloadPatch._playerField.GetValue((object) __instance)).IsYourPlayer || !WeaponStats._IsManuallyOperated && !__instance.Item.IsGrenadeLauncher && !__instance.Item.IsUnderBarrelDeviceActive)
      return;
    float firingChamberSpeed = WeaponStats.TotalFiringChamberSpeed;
    float ammoRec = (float) __instance.Item.CurrentAmmoTemplate.ammoRec;
    float num1 = 2f - ((double) ammoRec < 0.0 ? (float) (1.0 + (double) ammoRec / 100.0) : (float) (1.0 + (double) ammoRec / 150.0));
    float num2 = 1f;
    float num3 = 1.4f;
    float num4 = StanceController.IsLeftShoulder ? 0.75f : 1f;
    if (WeaponStats._WeapClass == "shotgun")
    {
      num3 = 1.5f;
      firingChamberSpeed *= PluginConfig.GlobalShotgunRackSpeedFactor.Value;
      num2 = StanceController.IsMounting ? 1.2f : (StanceController.IsBracing ? 1.1f : (StanceController.CurrentStance == EStance.ActiveAiming ? 1.32f : 1f));
    }
    if (__instance.Item.IsGrenadeLauncher || __instance.Item.IsUnderBarrelDeviceActive)
      firingChamberSpeed *= PluginConfig.GlobalUBGLReloadMulti.Value;
    if (WeaponStats._WeapClass == "sniperRifle")
    {
      firingChamberSpeed *= PluginConfig.GlobalBoltSpeedMulti.Value;
      num2 = !StanceController.IsMounting || !WeaponStats.BipodIsDeployed ? (StanceController.IsMounting ? 1.2f : (StanceController.IsBracing ? 1.14f : (StanceController.CurrentStance == EStance.ActiveAiming ? 1.1f : 1f))) : 1.3f;
    }
    float num5 = Mathf.Clamp(firingChamberSpeed * PlayerState.ReloadSkillMulti * PlayerState.ReloadInjuryMulti * num2 * num1 * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus * num4, 0.75f, num3);
    ((ObjectInHandsAnimator) ((Player.AbstractHandsController) __instance).FirearmsAnimator).SetAnimationSpeed(num5);
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===SetBoltActionReload===");
      ModulePatch.Logger.LogWarning((object) ("Set Bolt Action Reload = " + num5.ToString()));
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
