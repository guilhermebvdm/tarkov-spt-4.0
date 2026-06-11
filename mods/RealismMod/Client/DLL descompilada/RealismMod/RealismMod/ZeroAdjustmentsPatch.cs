// Decompiled with JetBrains decompiler
// Type: RealismMod.ZeroAdjustmentsPatch
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

public class ZeroAdjustmentsPatch : ModulePatch
{
  private static FieldInfo blindfireStrengthField;
  private static FieldInfo blindfireRotationField;
  private static PropertyInfo overlappingBlindfireField;
  private static FieldInfo blindfirePositionField;
  private static FieldInfo firearmControllerField;
  private static FieldInfo playerField;
  private static Vector3 targetPosition = Vector3.zero;

  protected virtual MethodBase GetTargetMethod()
  {
    ZeroAdjustmentsPatch.blindfireStrengthField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_blindfireStrength");
    ZeroAdjustmentsPatch.blindfireRotationField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_blindFireRotation");
    ZeroAdjustmentsPatch.overlappingBlindfireField = AccessTools.Property(typeof (ProceduralWeaponAnimation), "Single_3");
    ZeroAdjustmentsPatch.blindfirePositionField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_blindFirePosition");
    ZeroAdjustmentsPatch.firearmControllerField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    ZeroAdjustmentsPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("ZeroAdjustments", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(ProceduralWeaponAnimation __instance)
  {
    Player.FirearmController firearmController = (Player.FirearmController) ZeroAdjustmentsPatch.firearmControllerField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return true;
    Player player = (Player) ZeroAdjustmentsPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return true;
    float num1 = (float) ZeroAdjustmentsPatch.overlappingBlindfireField.GetValue((object) __instance);
    Vector3 vector3 = (Vector3) ZeroAdjustmentsPatch.blindfirePositionField.GetValue((object) __instance);
    __instance.PositionZeroSum.y = __instance._shouldMoveWeaponCloser ? 0.05f : 0.0f;
    __instance.RotationZeroSum.y = __instance.SmoothedTilt * __instance.PossibleTilt;
    float num2 = ((Player.ValueBlender) StanceController.StanceBlender).Value;
    float num3 = ((Player.ValueBlender) __instance.BlindfireBlender).Value;
    if ((double) Mathf.Abs(num3) > 0.0)
    {
      StanceController.IsBlindFiring = true;
      float num4 = (double) Mathf.Abs(__instance.Pitch) < 45.0 ? 1f : (float) ((90.0 - (double) Mathf.Abs(__instance.Pitch)) / 45.0);
      ZeroAdjustmentsPatch.blindfireStrengthField.SetValue((object) __instance, (object) num4);
      __instance.BlindFireEndPosition = (double) num3 > 0.0 ? __instance.BlindFireOffset : __instance.SideFireOffset;
      ProceduralWeaponAnimation proceduralWeaponAnimation = __instance;
      proceduralWeaponAnimation.BlindFireEndPosition = Vector3.op_Multiply(proceduralWeaponAnimation.BlindFireEndPosition, num4);
    }
    else
    {
      StanceController.IsBlindFiring = false;
      ZeroAdjustmentsPatch.blindfirePositionField.SetValue((object) __instance, (object) Vector3.zero);
      ZeroAdjustmentsPatch.blindfireRotationField.SetValue((object) __instance, (object) Vector3.zero);
    }
    if ((double) Mathf.Abs(num2) > 0.0)
    {
      float num5 = (double) Mathf.Abs(__instance.Pitch) < 45.0 ? 1f : (float) ((90.0 - (double) Mathf.Abs(__instance.Pitch)) / 45.0);
      ZeroAdjustmentsPatch.blindfireStrengthField.SetValue((object) __instance, (object) num5);
      ZeroAdjustmentsPatch.targetPosition = Vector3.op_Multiply(StanceController.StanceTargetPosition, num2);
      __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(__instance.PositionZeroSum, Vector3.op_Multiply((float) ZeroAdjustmentsPatch.blindfireStrengthField.GetValue((object) __instance), ZeroAdjustmentsPatch.targetPosition));
      __instance.HandsContainer.HandsRotation.Zero = __instance.RotationZeroSum;
      return false;
    }
    ZeroAdjustmentsPatch.targetPosition = Vector3.zero;
    __instance.HandsContainer.HandsPosition.Zero = Vector3.op_Addition(Vector3.op_Addition(__instance.PositionZeroSum, ZeroAdjustmentsPatch.targetPosition), Vector3.op_Multiply(Vector3.op_Multiply((Vector3) ZeroAdjustmentsPatch.blindfirePositionField.GetValue((object) __instance), (float) ZeroAdjustmentsPatch.blindfireStrengthField.GetValue((object) __instance)), num1));
    __instance.HandsContainer.HandsRotation.Zero = __instance.RotationZeroSum;
    return false;
  }
}
