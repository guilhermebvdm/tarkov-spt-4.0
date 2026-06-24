// Decompiled with JetBrains decompiler
// Type: TarkovIRL.ParallaxController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;

using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class ParallaxController
{
  private static readonly float _ParallaxSetSizeFixed = 0.2f;
  private static float _rotAvgXSet = 0.0f;
  private static float _rotAvgYSet = 0.0f;
  private static float _rotAvgX = 0.0f;
  private static float _rotAvgY = 0.0f;
  private static float _posLerpXTarget = 0.0f;
  private static float _posLerpYTarget = 0.0f;
  private static float _rotLerpXTarget = 0.0f;
  private static float _rotLerpYTarget = 0.0f;
  private static float _posLerpX = 0.0f;
  private static float _posLerpY = 0.0f;
  private static float _rotLerpX = 0.0f;
  private static float _rotLerpY = 0.0f;
  private static Vector2 _playerRotationLastFrame = Vector2.zero;
  private static float _parallaxWeightADS = 1f;
  private static bool _aimingLastFrame = false;
  private static Player _player = (Player) null;

  public static void Update(float dt)
  {
    if (ParallaxController._player == null)
      return;
    Vector2 vector2 = (ParallaxController._playerRotationLastFrame - ParallaxController._player.Rotation) * dt;
    ParallaxController._playerRotationLastFrame = ParallaxController._player.Rotation;
    float num1 = StanceController.CurrentStance == EStance.ShortStock ? 2f : 1f;
    float num2 = ParallaxController._ParallaxSetSizeFixed * PrimeMover.ParallaxSetSizeMulti.Value * RealismWrapper.WeaponBalanceMulti * num1;
    ParallaxController._rotAvgXSet += vector2.x;
    ParallaxController._rotAvgYSet += vector2.y;
    ParallaxController._rotAvgXSet -= ParallaxController._rotAvgX;
    ParallaxController._rotAvgYSet -= ParallaxController._rotAvgY;
    ParallaxController._rotAvgXSet = Mathf.Clamp(ParallaxController._rotAvgXSet, -num2, num2);
    ParallaxController._rotAvgYSet = Mathf.Clamp(ParallaxController._rotAvgYSet, -num2, num2);
    ParallaxController._rotAvgX = ParallaxController._rotAvgXSet * dt;
    ParallaxController._rotAvgY = ParallaxController._rotAvgYSet * dt;
    ParallaxController._parallaxWeightADS = ParallaxAdsController.ParallaxWeight;
    float num3 = WeaponController.IsPistol ? PrimeMover.PistolSpecificParallax.Value : 1f;
    float num4 = Mathf.Pow(1f - Mathf.Clamp01(PlayerMotionController.RotationDelta / 0.1f), 2f);
    float num5 = StanceController.CurrentStance == EStance.LowReady ? 0.0f : 1f;
    float num6 = StanceController.CurrentStance == EStance.HighReady ? 0.25f : 1f;
    float num7 = StanceController.CurrentStance == EStance.ActiveAiming ? 0.5f : 1f;
    float num8 = Mathf.Pow(PrimeMover.ParallaxMulti.Value * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * num3 * num4 * num5 * num6 * num7, 2f) / 100f;
    float num9 = dt * PrimeMover.ParallaxDTMulti.Value * EfficiencyController.EfficiencyModifierInverse;
    ParallaxController._posLerpXTarget = Mathf.Lerp(ParallaxController._posLerpXTarget, ParallaxController._rotAvgX * num8, num9);
    ParallaxController._posLerpYTarget = Mathf.Lerp(ParallaxController._posLerpYTarget, ParallaxController._rotAvgY * num8, num9);
    ParallaxController._rotLerpXTarget = Mathf.Lerp(ParallaxController._rotLerpXTarget, ParallaxController._rotAvgX * num8, num9);
    ParallaxController._rotLerpYTarget = Mathf.Lerp(ParallaxController._rotLerpYTarget, ParallaxController._rotAvgY * num8, num9);
    float num10 = WeaponController.IsPistol ? PrimeMover.ParallaxHardClampPistols.Value : PrimeMover.ParallaxHardClamp.Value;
    float num11 = WeaponController.IsPistol ? PrimeMover.ParallaxHardClampPistols.Value * 0.5f : PrimeMover.ParallaxHardClamp.Value * 0.5f;
    ParallaxController._rotLerpXTarget = Mathf.Clamp(ParallaxController._rotLerpXTarget, -num10, num10);
    ParallaxController._rotLerpYTarget = Mathf.Clamp(ParallaxController._rotLerpYTarget, -num10, num10);
    ParallaxController._posLerpXTarget = Mathf.Clamp(ParallaxController._posLerpXTarget, -num11, num11);
    ParallaxController._posLerpYTarget = Mathf.Clamp(ParallaxController._posLerpYTarget, -num11, num11);
    ParallaxController._rotLerpX = Mathf.Lerp(ParallaxController._rotLerpX, ParallaxController._rotLerpXTarget, dt * PrimeMover.ParallaxRotationSmoothingMulti.Value);
    ParallaxController._rotLerpY = Mathf.Lerp(ParallaxController._rotLerpY, ParallaxController._rotLerpYTarget, dt * PrimeMover.ParallaxRotationSmoothingMulti.Value);
    ParallaxController._posLerpX = Mathf.Lerp(ParallaxController._posLerpX, ParallaxController._posLerpXTarget, dt * PrimeMover.ParallaxRotationSmoothingMulti.Value);
    ParallaxController._posLerpY = Mathf.Lerp(ParallaxController._posLerpY, ParallaxController._posLerpYTarget, dt * PrimeMover.ParallaxRotationSmoothingMulti.Value);
  }

  public static void GetModifiedHandPosRotParallax(
    Player player,
    ref Vector3 position,
    ref Quaternion rotation)
  {
    if (ParallaxController._player == null)
      ParallaxController._player = player;
    if (ParallaxController._player == null)
      return;
    if (AnimStateController.IsBlindfire)
    {
      position = Vector3.zero;
      rotation = Quaternion.identity;
    }
    else if (WeaponController.IsUsingMounted)
    {
      position = Vector3.zero;
      rotation = Quaternion.identity;
    }
    else
    {
      float wpnMulti = WeaponController.GetWeaponMulti(true) * EfficiencyController.EfficiencyModifierInverse;
      bool isAiming = player.ProceduralWeaponAnimation.IsAiming;
      if (WeaponController.HasCheekWeld())
      {
        bool flag1 = !ParallaxController._aimingLastFrame & isAiming;
        bool flag2 = ParallaxController._aimingLastFrame && !isAiming;
        ParallaxController._aimingLastFrame = isAiming;
        if (flag1)
          ParallaxAdsController.StartNewAds(true, wpnMulti);
        else if (flag2)
          ParallaxAdsController.StartNewAds(false, wpnMulti);
      }
      rotation = Quaternion.Euler(0.0f, (float) (-(double) ParallaxController._rotLerpY * 100.0) * ParallaxController._parallaxWeightADS, (float) (-(double) ParallaxController._rotLerpX * 100.0) * ParallaxController._parallaxWeightADS);
      position = new Vector3(ParallaxController._posLerpX * ParallaxController._parallaxWeightADS, -ParallaxController._posLerpY * ParallaxController._parallaxWeightADS, 0.0f);
    }
  }
}
