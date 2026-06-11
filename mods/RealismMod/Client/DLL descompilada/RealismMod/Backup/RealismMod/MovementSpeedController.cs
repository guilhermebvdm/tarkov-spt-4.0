// Decompiled with JetBrains decompiler
// Type: RealismMod.MovementSpeedController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class MovementSpeedController
{
  private static Dictionary<BaseBallistic.ESurfaceSound, float> SurfaceSpeedModifiers = new Dictionary<BaseBallistic.ESurfaceSound, float>()
  {
    {
      (BaseBallistic.ESurfaceSound) 1,
      0.95f
    },
    {
      (BaseBallistic.ESurfaceSound) 8,
      0.95f
    },
    {
      (BaseBallistic.ESurfaceSound) 15,
      0.82f
    },
    {
      (BaseBallistic.ESurfaceSound) 14,
      0.8f
    },
    {
      (BaseBallistic.ESurfaceSound) 0,
      1f
    },
    {
      (BaseBallistic.ESurfaceSound) 5,
      1f
    },
    {
      (BaseBallistic.ESurfaceSound) 7,
      0.84f
    },
    {
      (BaseBallistic.ESurfaceSound) 10,
      0.85f
    },
    {
      (BaseBallistic.ESurfaceSound) 11,
      0.81f
    },
    {
      (BaseBallistic.ESurfaceSound) 16 /*0x10*/,
      0.95f
    },
    {
      (BaseBallistic.ESurfaceSound) 6,
      0.86f
    },
    {
      (BaseBallistic.ESurfaceSound) 17,
      0.91f
    },
    {
      (BaseBallistic.ESurfaceSound) 2,
      0.96f
    },
    {
      (BaseBallistic.ESurfaceSound) 12,
      0.95f
    },
    {
      (BaseBallistic.ESurfaceSound) 18,
      0.94f
    },
    {
      (BaseBallistic.ESurfaceSound) 3,
      0.95f
    },
    {
      (BaseBallistic.ESurfaceSound) 4,
      0.94f
    },
    {
      (BaseBallistic.ESurfaceSound) 13,
      1f
    },
    {
      (BaseBallistic.ESurfaceSound) 9,
      0.8f
    },
    {
      (BaseBallistic.ESurfaceSound) 19,
      0.88f
    }
  };
  public static BaseBallistic.ESurfaceSound CurrentSurface;
  private static float currentModifier = 1f;
  private static float targetModifier = 1f;
  private static float smoothness = 0.5f;
  private static float maxSlopeAngle = 1f;
  private static float maxSlowdownFactor = 0.1f;

  public static float GetSurfaceSpeed()
  {
    float num;
    MovementSpeedController.targetModifier = MovementSpeedController.SurfaceSpeedModifiers.TryGetValue(MovementSpeedController.CurrentSurface, out num) ? num : 1f;
    MovementSpeedController.currentModifier = Mathf.Lerp(MovementSpeedController.currentModifier, MovementSpeedController.targetModifier, MovementSpeedController.smoothness);
    return (float) Math.Round((double) MovementSpeedController.currentModifier, 3);
  }

  public static float GetSlope(Player player)
  {
    Vector2 movementDirection = player.MovementContext.MovementDirection;
    Vector2.op_Implicit(((Vector2) ref movementDirection).normalized);
    Vector3 position = player.Transform.position;
    float slope = 1f;
    RaycastHit raycastHit;
    if (Physics.Raycast(position, Vector3.op_UnaryNegation(Vector3.up), ref raycastHit))
    {
      float num = Vector3.Angle(((RaycastHit) ref raycastHit).normal, Vector3.up);
      if ((double) num > (double) MovementSpeedController.maxSlopeAngle)
        slope = Mathf.Lerp(1f, MovementSpeedController.maxSlowdownFactor, (float) (((double) num - (double) MovementSpeedController.maxSlopeAngle) / (90.0 - (double) MovementSpeedController.maxSlopeAngle)));
    }
    return slope;
  }

  public static float GetFiringMovementSpeedFactor(Player player)
  {
    if (!ShootController.IsFiringMovement || Object.op_Equality((Object) (player.HandsController as Player.FirearmController), (Object) null))
      return 1f;
    float num1 = (float) (1.0 - (double) ShootController.BaseTotalConvergence / 100.0);
    float num2 = (float) (1.0 + (double) ShootController.BaseTotalDispersion / 100.0);
    float num3 = ShootController.FactoredTotalVRecoil + ShootController.FactoredTotalHRecoil;
    float num4 = Mathf.Clamp((float) (1.0 - (80.0 - (double) WeaponStats.ErgoFactor) / 100.0), 0.1f, 1f);
    float num5 = num3 * num2 * num1 * num4;
    float num6 = WeaponStats.IsPistol ? num5 * 0.1f : num5;
    float num7 = (float) (1.0 - (double) num6 / 100.0);
    return Mathf.Clamp((float) (1.0 - (double) num6 / 400.0 * (double) ShootController.ShotCount), 0.6f * num7, 1f);
  }
}
