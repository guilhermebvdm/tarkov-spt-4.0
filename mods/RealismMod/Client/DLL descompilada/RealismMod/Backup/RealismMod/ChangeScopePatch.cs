// Decompiled with JetBrains decompiler
// Type: RealismMod.ChangeScopePatch
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

public class ChangeScopePatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _weaponManagerClassField;

  protected virtual MethodBase GetTargetMethod()
  {
    ChangeScopePatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    ChangeScopePatch._weaponManagerClassField = AccessTools.Field(typeof (Player.FirearmController), "weaponManagerClass");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("ChangeAimingMode", new Type[0]);
  }

  [PatchPrefix]
  private static bool PatchPreFix(Player.FirearmController __instance)
  {
    Player player = (Player) ChangeScopePatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer || !PluginConfig.OverrideMounting.Value || !Plugin.ServerConfig.enable_stances || !WeaponStats.BipodIsDeployed || !StanceController.IsMounting)
      return true;
    WeaponManagerClass weaponManagerClass = (WeaponManagerClass) ChangeScopePatch._weaponManagerClassField.GetValue((object) __instance);
    int index = ((GClass3882<int>) __instance.Item.AimIndex).Value + 1;
    if (index >= ((GClass1855) weaponManagerClass).ProceduralWeaponAnimation.ScopeAimTransforms.Count)
      index = 0;
    while (index != ((GClass3882<int>) __instance.Item.AimIndex).Value && (double) Mathf.Abs(((GClass1855) weaponManagerClass).ProceduralWeaponAnimation.ScopeAimTransforms[index].Rotation) >= (double) EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD)
    {
      ++index;
      if (index >= ((GClass1855) weaponManagerClass).ProceduralWeaponAnimation.ScopeAimTransforms.Count)
        index = 0;
    }
    if (index == ((GClass3882<int>) __instance.Item.AimIndex).Value || (double) Mathf.Abs(((GClass1855) weaponManagerClass).ProceduralWeaponAnimation.ScopeAimTransforms[index].Rotation) >= (double) EFTHardSettings.Instance.SCOPE_ROTATION_THRESHOLD)
      return false;
    ((GClass3882<int>) __instance.Item.AimIndex).Value = index;
    __instance.UpdateSensitivity();
    player.RaiseSightChangedEvent(player.ProceduralWeaponAnimation.CurrentAimingMod);
    return false;
  }
}
