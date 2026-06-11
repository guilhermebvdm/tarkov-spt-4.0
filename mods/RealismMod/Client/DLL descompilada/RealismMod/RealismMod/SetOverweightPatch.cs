// Decompiled with JetBrains decompiler
// Type: RealismMod.SetOverweightPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SetOverweightPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    SetOverweightPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    SetOverweightPatch.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("set_Overweight", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(ProceduralWeaponAnimation __instance, float value)
  {
    Player.FirearmController firearmController = (Player.FirearmController) SetOverweightPatch.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return false;
    Player player = (Player) SetOverweightPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return true;
    Weapon weapon = firearmController.Weapon;
    __instance.Breath.Overweight = value;
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_overweight").SetValue((object) __instance, (object) 0);
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_overweightAimingMultiplier").SetValue((object) __instance, (object) Mathf.Lerp(1f, Singleton<BackendConfigSettingsClass>.Instance.Stamina.AimingSpeedMultiplier, 0.0f));
    __instance.Walk.Overweight = Mathf.Lerp(0.0f, Singleton<BackendConfigSettingsClass>.Instance.Stamina.WalkVisualEffectMultiplier, value);
    return false;
  }
}
