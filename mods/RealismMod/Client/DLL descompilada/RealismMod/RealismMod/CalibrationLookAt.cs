// Decompiled with JetBrains decompiler
// Type: RealismMod.CalibrationLookAt
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class CalibrationLookAt : ModulePatch
{
  private static float recordedDistance = 0.0f;
  private static Vector3 recoilOffset = Vector3.zero;
  private static Vector3 target = Vector3.zero;
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    CalibrationLookAt.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    CalibrationLookAt.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("method_7", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(ProceduralWeaponAnimation __instance, ref Vector3 point)
  {
    Player.FirearmController firearmController = (Player.FirearmController) CalibrationLookAt.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return;
    Player player = (Player) CalibrationLookAt.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21 || __instance.CurrentAimingMod == null || __instance.CurrentScope.IsOptic || WeaponStats.ScopeID == null || !(WeaponStats.ScopeID != ""))
      return;
    float calibrationDistance = (float) __instance.CurrentAimingMod.GetCurrentOpticCalibrationDistance();
    if ((double) CalibrationLookAt.recordedDistance != (double) calibrationDistance)
    {
      WeaponStats.ZeroRecoilOffset = Vector2.zero;
      if (WeaponStats.ZeroOffsetDict.ContainsKey(WeaponStats.ScopeID))
        WeaponStats.ZeroOffsetDict[WeaponStats.ScopeID] = WeaponStats.ZeroRecoilOffset;
    }
    CalibrationLookAt.recordedDistance = calibrationDistance;
    float num = calibrationDistance / 25f;
    CalibrationLookAt.recoilOffset.x = WeaponStats.ZeroRecoilOffset.x * num;
    CalibrationLookAt.recoilOffset.y = WeaponStats.ZeroRecoilOffset.y * num;
    point = Vector3.op_Addition(point, CalibrationLookAt.recoilOffset);
  }
}
