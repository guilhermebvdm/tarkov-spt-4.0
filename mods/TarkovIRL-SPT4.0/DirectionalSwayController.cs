// Decompiled with JetBrains decompiler
// Type: TarkovIRL.DirectionalSwayController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class DirectionalSwayController
{
  private static float _lateralPosLerp;
  private static float _projectedPosLerp;
  private static float _lateralRotLerp;
  private static float _verticalRotLerp;

  public static void UpdateDirectionalSwayLerp(float dt)
  {
    float num1 = PrimeMover.DirectionalSwayLateralPosValue.Value;
    float num2 = PrimeMover.DirectionalSwayProjectedPosValue.Value;
    float num3 = PrimeMover.DirectionalSwayLateralRotValue.Value;
    float num4 = PrimeMover.DirectionalSwayVerticalRotValue.Value;
    PlayerMotionController.EPlayerDir direction = PlayerMotionController.Direction;
    float num5 = 0.0f;
    float num6 = 0.0f;
    if (direction == PlayerMotionController.EPlayerDir.FWD || direction == PlayerMotionController.EPlayerDir.BWD)
      num5 = 0.0f;
    else if (direction == PlayerMotionController.EPlayerDir.LEFT)
      num5 = -1f;
    else if (direction == PlayerMotionController.EPlayerDir.FWDLEFT)
      num5 = -0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.BWDLEFT)
      num5 = 0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.RIGHT)
      num5 = 1f;
    else if (direction == PlayerMotionController.EPlayerDir.FWDRIGHT)
      num5 = 0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.BWDRIGHT)
      num5 = -0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.NONE)
      num5 = 0.0f;
    if (direction == PlayerMotionController.EPlayerDir.FWD)
      num6 = 1f;
    else if (direction == PlayerMotionController.EPlayerDir.FWDLEFT)
      num6 = 0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.FWDRIGHT)
      num6 = 0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.BWD)
      num6 = -1f;
    else if (direction == PlayerMotionController.EPlayerDir.BWDLEFT)
      num6 = -0.5f;
    else if (direction == PlayerMotionController.EPlayerDir.BWDRIGHT)
      num6 = -0.5f;
    float num7 = Mathf.Clamp(PlayerMotionController.GetNormalSpeed(), 0.1f, 1f);
    float num8 = PlayerMotionController.IsAiming ? PrimeMover.DirectionalSwayLerpOnAds.Value : 1f;
    float num9 = num5 * (num7 * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * num8);
    float num10 = num6 * (num7 * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * num8);
    float num11 = num1 * num9 * PrimeMover.DirectionalSwayMulti.Value;
    float num12 = num2 * num9 * PrimeMover.DirectionalSwayMulti.Value;
    float num13 = num3 * num9 * PrimeMover.DirectionalSwayMulti.Value;
    float num14 = num4 * num10 * PrimeMover.DirectionalSwayMulti.Value;
    if (!WeaponController.HasCheekWeld())
    {
      num11 = 0.0f;
      num13 = 0.0f;
      num12 = 0.0f;
      num14 = 0.0f;
    }
    float num15 = Mathf.Clamp(PlayerMotionController.GetNormalSpeed(), 0.5f, 1f);
    float num16 = dt * PrimeMover.DirectionalSwayLerpSpeed.Value * num15;
    DirectionalSwayController._lateralPosLerp = Mathf.Lerp(DirectionalSwayController._lateralPosLerp, num11, num16);
    DirectionalSwayController._projectedPosLerp = Mathf.Lerp(DirectionalSwayController._projectedPosLerp, num12, num16);
    DirectionalSwayController._lateralRotLerp = Mathf.Lerp(DirectionalSwayController._lateralRotLerp, num13, num16);
    DirectionalSwayController._verticalRotLerp = Mathf.Lerp(DirectionalSwayController._verticalRotLerp, num14, num16);
  }

  public static void GetDirectionalSway(out Vector3 position, out Quaternion rotation)
  {
    if (!PrimeMover.IsDirectionalSway.Value)
    {
      position = Vector3.zero;
      rotation = Quaternion.identity;
    }
    else
    {
      position = Vector3.zero;
      position.x = DirectionalSwayController._lateralPosLerp;
      position.z = DirectionalSwayController._projectedPosLerp;
      rotation = Quaternion.identity;
      rotation.z = DirectionalSwayController._lateralRotLerp;
      rotation.x = DirectionalSwayController._verticalRotLerp;
    }
  }
}
