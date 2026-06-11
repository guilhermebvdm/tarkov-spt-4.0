// Decompiled with JetBrains decompiler
// Type: RealismMod.SetMagInWeaponPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using RealismMod.Weapons;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetMagInWeaponPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("SetMagInWeapon", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(FirearmsAnimator __instance)
  {
    if (!PlayerState.IsMagReloading)
      return;
    float num1 = Mathf.Clamp(WeaponStats.NewMagReloadSpeed * PlayerState.ReloadSkillMulti * PlayerState.ReloadInjuryMulti * GearController.GearReloadMulti * StanceController.HighReadyManipBuff * StanceController.ActiveAimManipBuff * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus, ReloadController.MinimumReloadSpeed, ReloadController.MaxReloadSpeed);
    ((ObjectInHandsAnimator) __instance).SetAnimationSpeed(num1);
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===SetMagInWeapon===");
      ModulePatch.Logger.LogWarning((object) ("Total Reload Speed = " + num1.ToString()));
      ModulePatch.Logger.LogWarning((object) ("ReloadSkillMulti = " + PlayerState.ReloadSkillMulti.ToString()));
      ModulePatch.Logger.LogWarning((object) ("ReloadInjuryMulti = " + PlayerState.ReloadInjuryMulti.ToString()));
      ModulePatch.Logger.LogWarning((object) ("GearReloadMulti = " + GearController.GearReloadMulti.ToString()));
      ManualLogSource logger1 = ModulePatch.Logger;
      float num2 = StanceController.HighReadyManipBuff;
      string str1 = "HighReadyManipBuff = " + num2.ToString();
      logger1.LogWarning((object) str1);
      ManualLogSource logger2 = ModulePatch.Logger;
      num2 = Mathf.Max(PlayerState.RemainingArmStamReloadFactor, 0.75f);
      string str2 = "RemainingArmStamPercReload = " + num2.ToString();
      logger2.LogWarning((object) str2);
      ModulePatch.Logger.LogWarning((object) ("NewMagReloadSpeed = " + WeaponStats.NewMagReloadSpeed.ToString()));
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
