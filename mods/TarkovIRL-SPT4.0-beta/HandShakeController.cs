using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class HandShakeController
{
  private static readonly float _HandShakeCurveSpeedMulti = 0.25f;
  private static readonly float _HandShakeMultiGeneral = 0.005f;
  private static float _handShakeLoopTimeX = 0.0f;
  private static float _handShakeLoopTimeY = 0.0f;
  private static float _handShakeStrengthLerp = 0.0f;

  public static Vector3 GetHandsShakePosition(Player player)
  {
    if (!player.ProceduralWeaponAnimation.IsAiming)
      return Vector3.zero;
    float num1 = HandShakeController._HandShakeCurveSpeedMulti * PrimeMover.ArmShakeRateMulti.Value;
    HandShakeController._handShakeLoopTimeX += (float) ((double) player.DeltaTime * (double) num1 * 0.37000000476837158);
    if ((double) HandShakeController._handShakeLoopTimeX >= 1.0)
      --HandShakeController._handShakeLoopTimeX;
    HandShakeController._handShakeLoopTimeY -= player.DeltaTime * num1;
    if ((double) HandShakeController._handShakeLoopTimeY <= 0.0)
      ++HandShakeController._handShakeLoopTimeY;
    float num2 = 1f;
    float num3 = WeaponController.HasCheekWeld() ? 1f : 1.8f;
    float num4 = PlayerMotionController.IsAugmentedBreath ? 0.5f : 1f;
    bool flag1 = player.HealthController.IsBodyPartBroken((EBodyPart) 3);
    bool flag2 = player.HealthController.IsBodyPartBroken((EBodyPart) 4);
    float num5 = (float) (1.0 * (flag1 ? 2.0 : 1.0) * (flag2 ? 2.0 : 1.0));
    float num6 = EfficiencyController.EfficiencyModifier * PrimeMover.ArmShakeMulti.Value * HandShakeController._HandShakeMultiGeneral * num2 * num3 * WeaponController.CurrentWeaponWeight * num4 * num5;
    HandShakeController._handShakeStrengthLerp = Mathf.Lerp(HandShakeController._handShakeStrengthLerp, num6, player.DeltaTime * 7f);
    AnimationCurve handsShakeCurve = PrimeMover.Instance.HandsShakeCurve;
    return new Vector3(handsShakeCurve.Evaluate(HandShakeController._handShakeLoopTimeX) * HandShakeController._handShakeStrengthLerp, handsShakeCurve.Evaluate(HandShakeController._handShakeLoopTimeY) * HandShakeController._handShakeStrengthLerp, 0.0f);
  }
}

