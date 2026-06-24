// Decompiled with JetBrains decompiler
// Type: TarkovIRL.HandMovWithRotController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class HandMovWithRotController
{
  private static readonly float _RotPullInValue = 0.07f;
  private static readonly float _LerpRate = 8f;
  private static readonly float _StockMovementAddedPosValue = 0.005f;
  private static float _rotPullInTarget = 0.0f;
  private static float _rotPullInLerp = 0.0f;
  private static float _stockedMovementAddedPosTarget = 0.0f;
  private static float _stockedMovementAddedPosLerp = 0.0f;
  private static float _stockedMovementAddedPosSmoothed = 0.0f;

  public static Vector3 GetModifiedHandPosZMovement(Player player)
  {
    float deltaTime = player.DeltaTime;
    if (player.ProceduralWeaponAnimation.IsAiming)
      HandMovWithRotController._rotPullInTarget = 0.0f;
    else if (AnimStateController.IsBlindfire)
    {
      HandMovWithRotController._rotPullInTarget = 0.0f;
    }
    else
    {
      float num = Mathf.Clamp01(Mathf.Abs(PlayerMotionController.HorizontalRotationDelta * 100f));
      HandMovWithRotController._rotPullInTarget = (WeaponController.HasCheekWeld() ? HandMovWithRotController._RotPullInValue * 0.75f : HandMovWithRotController._RotPullInValue) * num;
    }
    HandMovWithRotController._rotPullInLerp = Mathf.Lerp(HandMovWithRotController._rotPullInLerp, HandMovWithRotController._rotPullInTarget, deltaTime * 0.5f);
    return new Vector3(0.0f, 0.0f, -(PrimeMover.Instance.SmoothEdgesCurve.Evaluate(HandMovWithRotController._rotPullInLerp / HandMovWithRotController._RotPullInValue) * HandMovWithRotController._RotPullInValue));
  }

  public static Vector3 GetModifiedHandPosForLoweredWeapon(Player player)
  {
    float deltaTime = player.DeltaTime;
    HandMovWithRotController._stockedMovementAddedPosTarget = !AnimStateController.IsSideStep ? (WeaponController.HasCheekWeld() || WeaponController.IsPistol || !PlayerMotionController.IsPlayerMovement ? 0.0f : HandMovWithRotController._StockMovementAddedPosValue * WeaponController.GetWeaponMulti(false)) : HandMovWithRotController._StockMovementAddedPosValue * 0.2f * WeaponController.GetWeaponMulti(false);
    HandMovWithRotController._stockedMovementAddedPosLerp = Mathf.Lerp(HandMovWithRotController._stockedMovementAddedPosLerp, HandMovWithRotController._stockedMovementAddedPosTarget, deltaTime * HandMovWithRotController._LerpRate);
    HandMovWithRotController._stockedMovementAddedPosSmoothed = Mathf.Lerp(HandMovWithRotController._stockedMovementAddedPosSmoothed, HandMovWithRotController._stockedMovementAddedPosLerp, deltaTime);
    return new Vector3(0.0f, -HandMovWithRotController._stockedMovementAddedPosSmoothed, 0.0f);
  }
}
