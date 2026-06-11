// Decompiled with JetBrains decompiler
// Type: RealismMod.UpdateHipInaccuracyPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using EFT.Visual;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class UpdateHipInaccuracyPatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _tacticalModesField;

  protected virtual MethodBase GetTargetMethod()
  {
    UpdateHipInaccuracyPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    UpdateHipInaccuracyPatch._tacticalModesField = AccessTools.Field(typeof (TacticalComboVisualController), "list_0");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("UpdateHipInaccuracy", BindingFlags.Instance | BindingFlags.Public);
  }

  private static bool CheckVisibleLaser(List<Transform> tacticalModes)
  {
    foreach (Transform tacticalMode in tacticalModes)
    {
      if (((Component) tacticalMode).gameObject.activeInHierarchy)
      {
        foreach (Object child in GClass6.GetChildren(tacticalMode))
        {
          if (child.name.StartsWith("VIS_"))
            return true;
        }
      }
    }
    return false;
  }

  private static bool CheckIRLight(List<Transform> tacticalModes)
  {
    foreach (Transform tacticalMode in tacticalModes)
    {
      if (((Component) tacticalMode).gameObject.activeInHierarchy && Object.op_Inequality((Object) ((Component) tacticalMode).GetComponentInChildren<IkLight>(), (Object) null))
        return true;
    }
    return false;
  }

  private static bool CheckIRLaser(List<Transform> tacticalModes)
  {
    foreach (Transform tacticalMode in tacticalModes)
    {
      if (((Component) tacticalMode).gameObject.activeInHierarchy)
      {
        foreach (Object child in GClass6.GetChildren(tacticalMode))
        {
          if (child.name.StartsWith("IR_"))
            return true;
        }
      }
    }
    return false;
  }

  private static bool CheckWhiteLight(List<Transform> tacticalModes)
  {
    foreach (Transform tacticalMode in tacticalModes)
    {
      if (((Component) tacticalMode).gameObject.activeInHierarchy && Object.op_Inequality((Object) ((Component) tacticalMode).GetComponentInChildren<VolumetricLight>(), (Object) null))
        return true;
    }
    return false;
  }

  private static void CheckDevice(
    Player.FirearmController firearmController,
    FieldInfo tacticalModesField)
  {
    if (tacticalModesField == (FieldInfo) null)
    {
      ModulePatch.Logger.LogError((object) "Could find not find _tacticalModesField");
    }
    else
    {
      List<TacticalComboVisualController> ignoreFirstLevel = GClass6.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>(((Player.AbstractHandsController) firearmController).WeaponRoot);
      if (ignoreFirstLevel == null)
      {
        ModulePatch.Logger.LogError((object) "Could find not find tacticalComboVisualControllers");
      }
      else
      {
        PlayerState.WhiteLightActive = false;
        PlayerState.LaserActive = false;
        PlayerState.IRLightActive = false;
        PlayerState.IRLaserActive = false;
        foreach (TacticalComboVisualController visualController in ignoreFirstLevel)
        {
          List<Transform> tacticalModes = tacticalModesField.GetValue((object) visualController) as List<Transform>;
          if (UpdateHipInaccuracyPatch.CheckWhiteLight(tacticalModes))
            PlayerState.WhiteLightActive = true;
          if (UpdateHipInaccuracyPatch.CheckVisibleLaser(tacticalModes))
            PlayerState.LaserActive = true;
          if (UpdateHipInaccuracyPatch.CheckIRLight(tacticalModes))
            PlayerState.IRLightActive = true;
          if (UpdateHipInaccuracyPatch.CheckIRLaser(tacticalModes))
            PlayerState.IRLaserActive = true;
        }
      }
    }
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    Player player = (Player) UpdateHipInaccuracyPatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    if (__instance.AimingDevices.Length != 0 && ((IEnumerable<TacticalComboItemClass>) __instance.AimingDevices).Any<TacticalComboItemClass>((Func<TacticalComboItemClass, bool>) (x => x.Light.IsActive)))
    {
      UpdateHipInaccuracyPatch.CheckDevice(__instance, UpdateHipInaccuracyPatch._tacticalModesField);
      PlayerState.HasActiveDevice = true;
      NightVisionComponent component = player.NightVisionObserver.Component;
      if (component != null && (component.Togglable == null || component.Togglable.On))
      {
        float num = PlayerState.IRLaserActive || PlayerState.LaserActive ? 0.5f : (!PlayerState.IRLightActive || !PlayerState.IRLaserActive && !PlayerState.LaserActive ? (PlayerState.IRLightActive ? 0.6f : 1f) : 0.4f);
        PlayerState.DeviceBonus = PlayerState.WhiteLightActive ? 1f : num;
      }
      else
        PlayerState.DeviceBonus = PlayerState.LaserActive ? 0.5f : (!PlayerState.WhiteLightActive || !PlayerState.LaserActive ? (PlayerState.WhiteLightActive ? 0.6f : 1f) : 0.4f);
    }
    else
    {
      PlayerState.HasActiveDevice = false;
      PlayerState.DeviceBonus = 1f;
    }
  }
}
