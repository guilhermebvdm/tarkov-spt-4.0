// Decompiled with JetBrains decompiler
// Type: RealismMod.SprintAccelerationPatch
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

public class SprintAccelerationPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo rotationFrameSpanField;

  protected virtual MethodBase GetTargetMethod()
  {
    SprintAccelerationPatch.playerField = AccessTools.Field(typeof (MovementContext), "_player");
    SprintAccelerationPatch.rotationFrameSpanField = AccessTools.Field(typeof (MovementContext), "_averageRotationX");
    return (MethodBase) typeof (MovementContext).GetMethod("SprintAcceleration", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MovementContext __instance, float deltaTime)
  {
    Player player = (Player) SprintAccelerationPatch.playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return true;
    GClass807 gclass807 = (GClass807) SprintAccelerationPatch.rotationFrameSpanField.GetValue((object) __instance);
    float num1 = 1f + player.Skills.EnduranceBuffRestoration.Value;
    float num2 = 1f + player.Skills.HeavyVestMoveSpeedPenaltyReduction.Value;
    float num3 = GearController.HasRespirator ? 0.25f * num1 : (GearController.HasGasMask ? 0.3f * num1 : (!GearController.FSIsActive || !GearController.GearBlocksMouth ? (GearController.NVGIsActive ? 0.6f : 1f) : 0.5f * num2));
    float num4 = WeaponStats._WeapClass == "pistol" ? 1f : Mathf.Pow((float) (1.0 - (double) WeaponStats.ErgoFactor / 100.0 * (1.0 - (double) PlayerState.StrengthWeightBuff)), 0.15f);
    float num5 = (double) PlayerState.TotalModifiedWeightMinusWeapon >= 50.0 ? (float) (1.0 - (double) PlayerState.TotalModifiedWeightMinusWeapon / 100.0 * (1.0 - (double) PlayerState.StrengthWeightBuff)) : 1f;
    float num6 = PluginConfig.EnableSlopeSpeed.Value ? MovementSpeedController.GetSlope(player) : 1f;
    float num7 = PluginConfig.EnableMaterialSpeed.Value ? MovementSpeedController.GetSurfaceSpeed() : 1f;
    float num8 = StanceController.IsDoingTacSprint ? (float) (1.1499999761581421 * (1.0 + (double) player.Skills.EnduranceBuffRestoration.Value)) : 1f;
    double num9;
    switch (StanceController.CurrentStance)
    {
      case EStance.LowReady:
        num9 = 1.25;
        break;
      case EStance.ShortStock:
        num9 = 0.89999997615814209;
        break;
      case EStance.PatrolStance:
        num9 = 1.4500000476837158;
        break;
      default:
        num9 = StanceController.IsDoingTacSprint ? 1.3700000047683716 : (StanceController.CurrentStance == EStance.HighReady ? 1.2000000476837158 : 1.0);
        break;
    }
    float num10 = (float) num9;
    if ((double) num7 < 1.0)
      num7 = Mathf.Max(num7 * 0.85f, 0.2f);
    float num11 = player.Physical.SprintAcceleration * num10 * PlayerState.HealthSprintAccelFactor * num7 * num6 * PlayerState.GearSpeedPenalty * num4 * Plugin.RealHealthController.AdrenalineMovementBonus * num3 * deltaTime * num5;
    float num12 = (float) ((double) player.Physical.SprintSpeed * (double) __instance.SprintingSpeed + 1.0) * __instance.StateSprintSpeedLimit * num8 * PlayerState.HealthSprintSpeedFactor * num7 * num6 * PlayerState.GearSpeedPenalty * num4 * num3 * Plugin.RealHealthController.AdrenalineMovementBonus * num5;
    float num13 = Mathf.Max(EFTHardSettings.Instance.sprintSpeedInertiaCurve.Evaluate(Mathf.Abs((float) gclass807.Average)), EFTHardSettings.Instance.sprintSpeedInertiaCurve.Evaluate((float) int.MaxValue) * (2f - player.Physical.Inertia));
    float num14 = Mathf.Clamp(num12 * num13, 0.1f, num12);
    __instance.SprintSpeed = Mathf.Clamp(__instance.SprintSpeed + num11 * Mathf.Sign(num14 - __instance.SprintSpeed), 0.01f, num14) * num8;
    return false;
  }
}
