// Decompiled with JetBrains decompiler
// Type: RealismMod.SetAimingSlowdownPatch
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

public class SetAimingSlowdownPatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    SetAimingSlowdownPatch.playerField = AccessTools.Field(typeof (MovementContext), "_player");
    return (MethodBase) typeof (MovementContext).GetMethod("SetAimingSlowdown", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MovementContext __instance, bool isAiming)
  {
    if (!((Player) SetAimingSlowdownPatch.playerField.GetValue((object) __instance)).IsYourPlayer)
      return true;
    if (isAiming)
    {
      float num1 = 0.5f * WeaponStats.AimMoveSpeedWeapModifier * PlayerState.AimMoveSpeedInjuryMulti;
      float num2 = StanceController.CurrentStance == EStance.ActiveAiming ? num1 * 1.45f : num1;
      float num3 = WeaponStats._WeapClass == "pistol" ? num2 + 0.15f : num2;
      __instance.AddStateSpeedLimit(Mathf.Clamp(num3, 0.3f, 0.9f), (Player.ESpeedLimit) 2);
      return false;
    }
    __instance.RemoveStateSpeedLimit((Player.ESpeedLimit) 2);
    return false;
  }
}
