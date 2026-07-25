using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class HandPoseController
{
  private static readonly float _LerpRate = 4f;
  private static readonly float _OffsetTargetMultiY = 0.03f;
  private static readonly float _OffsetTargetMultiZ = -0.05f;
  private static readonly float _ChangePoseModifier = 0.01f;
  private static float _currentLerpRate = 0.0f;
  private static float _offsetTargetY = 0.0f;
  private static float _offsetTargetZ = 0.0f;
  private static float _offsetTargetYLerp = 0.0f;
  private static float _offsetTargetZLerp = 0.0f;
  private static Vector3 _poseShiftVectorVertical = Vector3.zero;
  private static Vector3 _poseShiftVectorVerticalLerped = Vector3.zero;
  private static Quaternion _poseShiftVectorRotation = Quaternion.identity;
  private static Quaternion _poseShiftVectorRotationLerped = Quaternion.identity;
  private static float _poseLevelLastFrame = 0.0f;
  private static float _poseDifference = 0.0f;
  private static float _changingPoseCycle = 0.0f;
  private static float _changingPoseCycle2 = 0.0f;
  private static bool _isChangingPose = false;

  private static void VerticalPoseUpdate(float dt)
  {
    HandPoseController._offsetTargetYLerp = Mathf.Lerp(HandPoseController._offsetTargetYLerp, HandPoseController._offsetTargetY, dt * HandPoseController._currentLerpRate);
    HandPoseController._offsetTargetZLerp = Mathf.Lerp(HandPoseController._offsetTargetZLerp, HandPoseController._offsetTargetZ, dt * HandPoseController._currentLerpRate);
  }

  private static void ChangePoseUpdate(Player player)
  {
    if (HandPoseController._isChangingPose)
    {
      float deltaTime = PrimeMover.Instance.DeltaTime;
      float num1 = Mathf.Abs(HandPoseController._poseDifference);
      bool flag1 = player.HealthController.IsBodyPartBroken((EBodyPart) 3);
      bool flag2 = player.HealthController.IsBodyPartBroken((EBodyPart) 4);
      float num2 = (float) (1.0 * (flag1 ? 0.5 : 1.0) * (flag2 ? 0.5 : 1.0));
      float num3 = (float) (1.0 + (1.0 - (double) num1) * (double) num2);
      bool flag3 = (double) HandPoseController._poseDifference < 0.0;
      float num4 = flag3 ? 1.5f : 2f;
      float weaponMulti = WeaponController.GetWeaponMulti(false);
      float num5 = WeaponController.GetWeaponMulti(true) * 2.2f;
      float num6 = flag3 ? 2f : 1.1f;
      float efficiencyModifier = EfficiencyController.EfficiencyModifier;
      HandPoseController._changingPoseCycle += deltaTime * num3 * num6 * num5;
      HandPoseController._changingPoseCycle2 += (float) ((double) deltaTime * (double) num3 * (double) num6 * (double) num5 * 0.699999988079071);
      if ((double) HandPoseController._changingPoseCycle >= 1.0 && (double) HandPoseController._changingPoseCycle2 >= 1.0)
      {
        HandPoseController._isChangingPose = false;
        HandPoseController._changingPoseCycle = 0.0f;
        HandPoseController._changingPoseCycle2 = 0.0f;
        return;
      }
      float num7 = (double) HandPoseController._poseDifference < 0.0 ? 1f : 0.5f;
      float num8 = flag3 ? PrimeMover.Instance.PoseChangeCurve.Evaluate(HandPoseController._changingPoseCycle) : PrimeMover.Instance.PoseChangeCurveUp.Evaluate(HandPoseController._changingPoseCycle);
      float num9 = flag3 ? PrimeMover.Instance.PoseChangeDownRadialCurve.Evaluate(HandPoseController._changingPoseCycle) * 5f : 0.0f;
      float num10 = flag3 ? 0.0f : PrimeMover.Instance.PoseChangeDownRadialCurve.Evaluate(HandPoseController._changingPoseCycle2);
      float num11 = num1 * HandPoseController._ChangePoseModifier * num7 * num4 * weaponMulti * efficiencyModifier;
      float num12 = num8 * num11;
      float num13 = num9 * num11;
      float num14 = 1f - PlayerMotionController.ArmStamNorm;
      float num15 = (float) ((double) num10 * (double) num11 * 7.0) * num14;
      HandPoseController._poseShiftVectorVertical = new Vector3(0.0f, num12, num13);
      HandPoseController._poseShiftVectorRotation = TIRLUtils.GetQuatFromV3(new Vector3(num15, 0.0f, 0.0f));
    }
    else
    {
      HandPoseController._poseShiftVectorVertical = Vector3.zero;
      HandPoseController._poseShiftVectorRotation = Quaternion.identity;
    }
    HandPoseController._poseShiftVectorVerticalLerped = Vector3.Lerp(HandPoseController._poseShiftVectorVerticalLerped, HandPoseController._poseShiftVectorVertical, PrimeMover.Instance.DeltaTime * 8f);
    HandPoseController._poseShiftVectorRotationLerped = Quaternion.Lerp(HandPoseController._poseShiftVectorRotationLerped, HandPoseController._poseShiftVectorRotation, PrimeMover.Instance.DeltaTime * 8f);
  }

  public static Vector3 GetModifiedHandPosWithPose(Player player)
  {
    HandPoseController.VerticalPoseUpdate(player.DeltaTime);
    HandPoseController._currentLerpRate = HandPoseController._LerpRate;
    float poseLevel = player.PoseLevel;
    HandPoseController._offsetTargetY = (1f - poseLevel) * HandPoseController._OffsetTargetMultiY;
    HandPoseController._offsetTargetZ = (1f - poseLevel) * HandPoseController._OffsetTargetMultiZ;
    if (player.ProceduralWeaponAnimation.IsAiming)
    {
      HandPoseController._offsetTargetY = 0.0f;
      HandPoseController._offsetTargetZ = 0.0f;
      HandPoseController._currentLerpRate = HandPoseController._LerpRate * 2f;
    }
    return new Vector3(0.0f, HandPoseController._offsetTargetYLerp, HandPoseController._offsetTargetZLerp);
  }

  public static Vector3 GetModifiedHandPosWithPoseChange(Player player)
  {
    HandPoseController.ChangePoseUpdate(player);
    Vector3 vectorVerticalLerped = HandPoseController._poseShiftVectorVerticalLerped;
    float poseLevel = player.PoseLevel;
    if ((double) poseLevel != (double) HandPoseController._poseLevelLastFrame)
    {
      HandPoseController._poseDifference = poseLevel - HandPoseController._poseLevelLastFrame;
      if (!HandPoseController._isChangingPose)
        HandPoseController._isChangingPose = true;
    }
    if (!player.ProceduralWeaponAnimation.IsAiming)
      vectorVerticalLerped.y *= 3f;
    HandPoseController._poseLevelLastFrame = poseLevel;
    return vectorVerticalLerped;
  }

  public static Quaternion GetModifiedHandRotWithPoseChange()
  {
    return HandPoseController._poseShiftVectorRotationLerped;
  }
}

