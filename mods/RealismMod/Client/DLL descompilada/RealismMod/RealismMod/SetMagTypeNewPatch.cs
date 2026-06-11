// Decompiled with JetBrains decompiler
// Type: RealismMod.SetMagTypeNewPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using HarmonyLib;
using RealismMod.Weapons;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetMagTypeNewPatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo faField;

  protected virtual MethodBase GetTargetMethod()
  {
    SetMagTypeNewPatch._playerField = AccessTools.Field(typeof (Player.FirearmController.GClass1819), "player_0");
    SetMagTypeNewPatch.faField = AccessTools.Field(typeof (Player.FirearmController.GClass1819), "firearmsAnimator_0");
    return (MethodBase) typeof (Player.FirearmController.GClass1808).GetMethod("Start", new Type[2]
    {
      typeof (Player.FirearmController.GClass1774),
      typeof (Callback)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController.GClass1808 __instance)
  {
    if (!((Player) SetMagTypeNewPatch._playerField.GetValue((object) __instance)).IsYourPlayer)
      return;
    FirearmsAnimator firearmsAnimator = (FirearmsAnimator) SetMagTypeNewPatch.faField.GetValue((object) __instance);
    float num = Mathf.Clamp(WeaponStats.CurrentMagReloadSpeed * PlayerState.ReloadSkillMulti * PlayerState.ReloadInjuryMulti * StanceController.HighReadyManipBuff * StanceController.ActiveAimManipBuff * PlayerState.RemainingArmStamReloadFactor * Plugin.RealHealthController.AdrenalineReloadBonus, ReloadController.MinimumReloadSpeed, ReloadController.MaxReloadSpeed);
    ((ObjectInHandsAnimator) firearmsAnimator).SetAnimationSpeed(num);
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===SetMagTypeNew===");
      ModulePatch.Logger.LogWarning((object) ("SetMagTypeNew = " + num.ToString()));
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
