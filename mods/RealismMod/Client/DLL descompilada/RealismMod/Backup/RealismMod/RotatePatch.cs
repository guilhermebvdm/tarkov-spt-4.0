// Decompiled with JetBrains decompiler
// Type: RealismMod.RotatePatch
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

public class RotatePatch : ModulePatch
{
  private static FieldInfo _movementContextField;
  private static FieldInfo _playerField;
  private const float FpsFactor = 144f;
  private const float FpsSmoothingFactor = 0.42f;
  private const float ShotCountThreshold = 3f;
  private static Vector2 _initialRotation = Vector2.zero;

  protected virtual MethodBase GetTargetMethod()
  {
    RotatePatch._movementContextField = AccessTools.Field(typeof (MovementState), "MovementContext");
    RotatePatch._playerField = AccessTools.Field(typeof (MovementContext), "_player");
    return (MethodBase) typeof (MovementState).GetMethod("Rotate", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void DoMounting(
    MovementContext movementContext,
    Player.FirearmController fc,
    ref Vector2 deltaRotation)
  {
    Vector2 rotation = movementContext.Rotation;
    deltaRotation = Vector2.op_Multiply(deltaRotation, ((Player.AbstractHandsController) fc).AimingSensitivity * 0.9f);
    float num1 = WeaponStats.BipodIsDeployed ? 14f : 19f;
    double num2;
    switch (StanceController.BracingDirection)
    {
      case EBracingDirection.Top:
        num2 = -(double) num1;
        break;
      case EBracingDirection.Right:
        num2 = -4.0;
        break;
      default:
        num2 = -15.0;
        break;
    }
    float num3 = (float) num2;
    double num4;
    switch (StanceController.BracingDirection)
    {
      case EBracingDirection.Top:
        num4 = (double) num1;
        break;
      case EBracingDirection.Right:
        num4 = 15.0;
        break;
      default:
        num4 = 1.0;
        break;
    }
    float num5 = (float) num4;
    float num6 = WeaponStats.BipodIsDeployed ? 12f : 10f;
    float num7 = StanceController.BracingDirection == EBracingDirection.Top ? -num6 : -8f;
    float num8 = StanceController.BracingDirection == EBracingDirection.Top ? num6 : 15f;
    float num9 = RotatePatch._initialRotation.x + num3;
    float num10 = RotatePatch._initialRotation.x + num5;
    float num11 = RotatePatch._initialRotation.y + num7;
    float num12 = RotatePatch._initialRotation.y + num8;
    float num13 = Mathf.Clamp(rotation.x + deltaRotation.x, num9, num10);
    float num14 = Mathf.Clamp(rotation.y + deltaRotation.y, num11, num12);
    deltaRotation.x = num13 - rotation.x;
    deltaRotation.y = num14 - rotation.y;
    deltaRotation = movementContext.ClampRotation(deltaRotation);
    MovementContext movementContext1 = movementContext;
    movementContext1.Rotation = Vector2.op_Addition(movementContext1.Rotation, deltaRotation);
  }

  [PatchPrefix]
  private static bool Prefix(MovementState __instance, Vector2 deltaRotation, bool ignoreClamp)
  {
    MovementContext movementContext = (MovementContext) RotatePatch._movementContextField.GetValue((object) __instance);
    Player player = (Player) RotatePatch._playerField.GetValue((object) movementContext);
    if (player.IsYourPlayer && player.MovementContext.CurrentState.Name != 21 && !ignoreClamp)
    {
      deltaRotation = movementContext.ClampRotation(deltaRotation);
      if (!StanceController.IsMounting)
        RotatePatch._initialRotation = movementContext.Rotation;
      if (StanceController.IsMounting)
      {
        Player.FirearmController handsController = player.HandsController as Player.FirearmController;
        if (Object.op_Equality((Object) handsController, (Object) null))
          return true;
        RotatePatch.DoMounting(movementContext, handsController, ref deltaRotation);
        return false;
      }
    }
    return true;
  }
}
