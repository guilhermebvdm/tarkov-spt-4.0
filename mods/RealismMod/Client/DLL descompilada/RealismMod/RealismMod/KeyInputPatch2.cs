// Decompiled with JetBrains decompiler
// Type: RealismMod.KeyInputPatch2
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using EFT;
using EFT.Animations;
using EFT.InputSystem;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class KeyInputPatch2 : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Class1602).GetMethod("TranslateCommand", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void ChangeScopeModeOnMount(
    ProceduralWeaponAnimation pwa,
    Player.FirearmController fc)
  {
    int aimIndex = pwa.AimIndex;
    if ((double) Mathf.Abs(pwa.ScopeAimTransforms[aimIndex].Rotation) < (double) EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD)
      return;
    for (int index = 0; index < pwa.ScopeAimTransforms.Count; ++index)
    {
      if ((double) Mathf.Abs(pwa.ScopeAimTransforms[index].Rotation) < (double) EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD)
      {
        fc.ChangeAimingMode(index);
        break;
      }
    }
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(Class1602 __instance, ECommand command)
  {
    if (command == 27 || command == 22)
    {
      StanceController.IsMounting = false;
      return true;
    }
    int num;
    if (command == 78 || command == 79)
    {
      KeyboardShortcut keyboardShortcut = PluginConfig.StanceWheelComboKeyBind.Value;
      num = !Input.GetKey(((KeyboardShortcut) ref keyboardShortcut).MainKey) ? 0 : (PluginConfig.UseMouseWheelPlusKey.Value ? 1 : 0);
    }
    else
      num = 0;
    if (num != 0)
      return false;
    if (command != 140 || !PluginConfig.OverrideMounting.Value || !Plugin.ServerConfig.enable_stances)
      return true;
    Player yourPlayer = Utils.GetYourPlayer();
    ProceduralWeaponAnimation proceduralWeaponAnimation = yourPlayer.ProceduralWeaponAnimation;
    Player.FirearmController handsController = yourPlayer.HandsController as Player.FirearmController;
    if (StanceController.IsBracing && !StanceController.IsColliding)
    {
      if (WeaponStats.BipodIsDeployed && StanceController.BracingDirection != 0)
        return false;
      StanceController.IsMounting = !StanceController.IsMounting;
      if (StanceController.IsMounting)
        StanceController.CancelAllStances();
      StanceController.DoWiggleEffects(yourPlayer, proceduralWeaponAnimation, handsController.Weapon, StanceController.IsMounting ? StanceController.CoverWiggleDirection : Vector3.op_Multiply(StanceController.CoverWiggleDirection, -1f), true, 1f, useGearSound: true);
    }
    if (!StanceController.IsBracing && StanceController.IsMounting)
      StanceController.IsMounting = false;
    if (StanceController.IsMounting && WeaponStats.BipodIsDeployed)
      KeyInputPatch2.ChangeScopeModeOnMount(proceduralWeaponAnimation, handsController);
    return false;
  }
}
