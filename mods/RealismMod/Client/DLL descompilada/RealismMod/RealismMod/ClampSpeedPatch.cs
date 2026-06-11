// Decompiled with JetBrains decompiler
// Type: RealismMod.ClampSpeedPatch
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

public class ClampSpeedPatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ClampSpeedPatch.playerField = AccessTools.Field(typeof (MovementContext), "_player");
    return (MethodBase) typeof (MovementContext).GetMethod("ClampSpeed", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(MovementContext __instance, float speed, ref float __result)
  {
    Player player = (Player) ClampSpeedPatch.playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return true;
    float num1 = 1f;
    if (Utils.PlayerIsReady && PluginConfig.EnableSlopeSpeed.Value)
      num1 = MovementSpeedController.GetSlope(player);
    float num2 = WeaponStats._WeapClass == "pistol" ? 1f : Mathf.Pow((float) (1.0 - (double) WeaponStats.ErgoFactor / 100.0 * (1.0 - (double) PlayerState.StrengthWeightBuff)), 0.15f);
    float num3 = Mathf.Pow((float) (1.0 - (double) PlayerState.TotalModifiedWeightMinusWeapon / 100.0 * (1.0 - (double) PlayerState.StrengthWeightBuff)), 0.3f);
    float num4 = PluginConfig.EnableMaterialSpeed.Value ? MovementSpeedController.GetSurfaceSpeed() : 1f;
    float movementSpeedFactor = MovementSpeedController.GetFiringMovementSpeedFactor(player);
    double num5;
    switch (StanceController.CurrentStance)
    {
      case EStance.LowReady:
        num5 = 1.1499999761581421;
        break;
      case EStance.HighReady:
        num5 = 1.0499999523162842;
        break;
      case EStance.ShortStock:
        num5 = 0.949999988079071;
        break;
      case EStance.PatrolStance:
        num5 = 1.3300000429153442;
        break;
      default:
        num5 = 1.0;
        break;
    }
    float num6 = (float) num5;
    float num7 = PlayerState.HealthWalkSpeedFactor * num4 * num1 * movementSpeedFactor * num6 * num2 * num3 * Plugin.RealHealthController.AdrenalineMovementBonus;
    __result = Mathf.Clamp(speed, 0.0f, __instance.StateSpeedLimit * num7);
    return false;
  }
}
