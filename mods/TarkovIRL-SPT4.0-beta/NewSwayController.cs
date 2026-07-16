using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class NewSwayController
{
  private static float _lerpPosHorizontal = 0.0f;
  private static float _lerpPosVertical = 0.0f;
  private static float _lerpRot = 0.0f;
  private static float _weaponTiltLerp = 0.0f;
  private static float _leanVerticalLerp = 0.0f;
  private static float _vertDropFromRotLerp = 0.0f;
  private static float _hyperVerticalLerp = 0.0f;
  private static Vector3 _posSmoothed = Vector3.zero;
  private static Vector3 _rotSmoothed = Vector3.zero;
  private static int _lagginSwaySetSize = 30;
  private static Vector3[] _laggingSwayPoses = new Vector3[30];
  private static Vector3[] _laggingSwayRots = new Vector3[30];
  private static int _laggingSwayIterator = 0;
  private static Vector3 _lagginPos = Vector3.zero;
  private static Vector3 _lagginPosSmoothed = Vector3.zero;
  private static Vector3 _lagginRot = Vector3.zero;
  private static Vector3 _lagginRotSmoothed = Vector3.zero;
  private static Vector3 _finalPos = Vector3.zero;
  private static Vector3 _finalPosSmoothed = Vector3.zero;
  private static Vector3 _finalRot = Vector3.zero;
  private static Vector3 _finalRotSmoothed = Vector3.zero;
  private static bool _wasAimingLastFrame = false;
  private static float _attenFactorLerp = 1f;

  public static void UpdateLerp(float deltaTime)
  {
    bool isAiming = PlayerMotionController.IsAiming;
    bool isSprinting = PlayerMotionController.IsSprinting;
    if (isAiming != NewSwayController._wasAimingLastFrame)
    {
      NewSwayController._lerpPosHorizontal = 0.0f;
      NewSwayController._lerpPosVertical = 0.0f;
      NewSwayController._lerpRot = 0.0f;
      NewSwayController._posSmoothed = Vector3.zero;
      NewSwayController._rotSmoothed = Vector3.zero;
      NewSwayController._lagginPosSmoothed = Vector3.zero;
      NewSwayController._lagginRotSmoothed = Vector3.zero;
      for (int index = 0; index < NewSwayController._lagginSwaySetSize; ++index)
      {
        NewSwayController._laggingSwayPoses[index] = Vector3.zero;
        NewSwayController._laggingSwayRots[index] = Vector3.zero;
      }
    }
    NewSwayController._wasAimingLastFrame = isAiming;
    if (isSprinting | isAiming)
    {
      NewSwayController._lerpPosHorizontal = 0.0f;
      NewSwayController._lerpPosVertical = 0.0f;
      NewSwayController._lerpRot = 0.0f;
      NewSwayController._weaponTiltLerp = 0.0f;
      NewSwayController._leanVerticalLerp = 0.0f;
      NewSwayController._vertDropFromRotLerp = 0.0f;
      NewSwayController._hyperVerticalLerp = 0.0f;
      NewSwayController.ProcessLagginSway();
    }
    else
    {
      float num1 = 1f;
      bool flag = WeaponController.HasCheekWeld();
      float num2 = PlayerMotionController.HorizontalRotationDelta * PrimeMover.WeaponSwayMulti.Value;
      float rawHorizontalSpeed = PlayerMotionController.RawHorizontalSpeed;
      float num3 = 1f;
      if ((double) rawHorizontalSpeed > (double) PrimeMover.FastTurnThreshold.Value && (double) PrimeMover.FastTurnAttenuation.Value > 0.0)
        num3 = Mathf.Clamp01((float) (1.0 - (double) (rawHorizontalSpeed - PrimeMover.FastTurnThreshold.Value) * (double) PrimeMover.FastTurnAttenuation.Value * 0.004999999888241291));
      NewSwayController._attenFactorLerp = (double) num3 >= (double) NewSwayController._attenFactorLerp ? Mathf.Lerp(NewSwayController._attenFactorLerp, num3, deltaTime * 10f) : num3;
      if (PrimeMover.InvertSwayVanilla.Value && StanceController.CurrentStance == EStance.None && !PlayerMotionController.IsAiming)
        num2 *= -1f;
      float num4 = PrimeMover.NewSwayRotDeltaClamp.Value;
      float num5 = Mathf.Clamp(num2, 0.0f - num4, num4) * NewSwayController._attenFactorLerp;
      float num6 = flag ? -1f : 0.5f;
      float num7 = !flag || !PlayerMotionController.IsAiming ? 1f : 0.0f;
      float num8 = flag || WeaponController.IsPistol || !PlayerMotionController.IsAiming ? 1f : 0.7f;
      float num9 = !flag || PlayerMotionController.IsAiming ? 1f : 0.7f;
      float num10 = flag || !PlayerMotionController.IsAiming ? 1f : 1.25f;
      float num11 = !flag || !PlayerMotionController.IsAiming ? 1f : -0.25f;
      float num12 = !flag || !PlayerMotionController.IsAiming ? 1f : 2f;
      float num13 = !flag ? 0.5f : 1f;
      float num14 = WeaponController.GetWeaponMulti(true) * EfficiencyController.EfficiencyModifierInverse;
      float num15 = !flag ? -1f : 0.0f;
      float num16 = PlayerMotionController.IsAiming ? 0.5f : 1f;
      float num17 = WeaponController.IsPistol ? 2f : 1f;
      float num18 = (float) ((double) num5 * (double) num6 * (double) num7 * (double) num8 * (double) WeaponController.GetWeaponMulti(false) * (double) EfficiencyController.EfficiencyModifier * (WeaponController.IsPistol ? (double) PrimeMover.PistolSwayMulti.Value : 1.0)) * PrimeMover.NewSwaySlideMulti.Value;
      float num19 = Mathf.Abs(num5) * num15 * num16 * num17;
      float num20 = PrimeMover.NewSwayPositionDTMulti.Value;
      float num21 = PrimeMover.NewSwayRotationDTMulti.Value;
      NewSwayController._lerpPosHorizontal = Mathf.Lerp(NewSwayController._lerpPosHorizontal, num18, deltaTime * num14 * num9 * num10 * num20 * num1);
      NewSwayController._lerpPosVertical = Mathf.Lerp(NewSwayController._lerpPosVertical, num19, deltaTime * num14 * PrimeMover.NewSwayWpnUnstockedDropSpeed.Value);
      float num22 = WeaponController.IsPistol ? PrimeMover.PistolSwayMulti.Value : 1f;
      float num23 = num5 * num11 * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * num22;
      float num24 = PlayerMotionController.IsAiming ? PrimeMover.NewSwayADSRotClamp.Value * num4 : 1f;
      float num25 = Mathf.Clamp(num23, 0.0f - num24, num24);
      NewSwayController._lerpRot = Mathf.Lerp(NewSwayController._lerpRot, num25, deltaTime * num14 * num12 * num13 * num21 * num1);
      float num26 = PrimeMover.NewSwayRotFinalClampPos.Value;
      float num27 = PrimeMover.NewSwayRotFinalClampNeg.Value;
      NewSwayController._lerpRot = Mathf.Clamp(NewSwayController._lerpRot, -num27, num26);
      float num28 = PlayerMotionController.IsAiming ? 0.0f : PrimeMover.WeaponCantValue.Value * 0.1f;
      NewSwayController._weaponTiltLerp = Mathf.Lerp(NewSwayController._weaponTiltLerp, num28, deltaTime * 20f);
      float num29 = (float) ((PlayerMotionController.IsAiming ? 0.0 : (double) PlayerMotionController.LeanNormal * (double) PrimeMover.LeanExtraVerticalMulti.Value * (double) WeaponController.GetWeaponMulti(false)) * -1.0);
      if (AnimStateController.IsLeftShoulder)
        num29 *= -1f;
      NewSwayController._leanVerticalLerp = Mathf.Lerp(NewSwayController._leanVerticalLerp, num29, deltaTime * 10f * EfficiencyController.EfficiencyModifierInverse);
      float num30 = !WeaponController.IsStocked || !PlayerMotionController.IsAiming ? 1f : 0.2f;
      float num31 = PlayerMotionController.RotationDelta * PrimeMover.NewSwayWpnDropFromRotMulti.Value * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * num30;
      NewSwayController._vertDropFromRotLerp = Mathf.Lerp(NewSwayController._vertDropFromRotLerp, num31, deltaTime * PrimeMover.NewSwayWpnUnstockedDropSpeed.Value);
      float verticalRotationDelta = PlayerMotionController.VerticalRotationDelta;
      float num32 = (double) verticalRotationDelta < 0.0 ? -1f : 1f;
      float num33 = PlayerMotionController.IsAiming ? 0.0f : verticalRotationDelta * num32 * PrimeMover.HyperVerticalMulti.Value * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier;
      float num34 = PrimeMover.HyperVerticalClamp.Value;
      float num35 = Mathf.Clamp(num33, 0.0f - num34, num34);
      float num36 = PrimeMover.HyperVerticalDT.Value * RealismWrapper.WeaponBalanceMulti;
      NewSwayController._hyperVerticalLerp = Mathf.Lerp(NewSwayController._hyperVerticalLerp, num35, deltaTime * num36);
      NewSwayController.ProcessLagginSway();
    }
  }

  private static void ProcessLagginSway()
  {
    NewSwayController._laggingSwayPoses[NewSwayController._laggingSwayIterator] = NewSwayController._posSmoothed;
    NewSwayController._laggingSwayRots[NewSwayController._laggingSwayIterator] = NewSwayController._rotSmoothed;
    ++NewSwayController._laggingSwayIterator;
    if (NewSwayController._laggingSwayIterator > 29)
      NewSwayController._laggingSwayIterator = 0;
    float num1 = Mathf.Clamp(EfficiencyController.EfficiencyModifier, 1f, 10f);
    float num2 = Mathf.Clamp((float) ((double) WeaponController.GetWeaponMulti(false) * (double) RealismWrapper.WeaponBalanceMulti * (double) num1 * (double) PrimeMover.LaggingSwayMulti.Value * (StanceController.CurrentStance == EStance.HighReady ? 0.5 : 1.0)), 1f, PrimeMover.LaggingSwayClamp.Value);
    int index = NewSwayController._laggingSwayIterator - Mathf.RoundToInt(num2);
    if (index < 0)
      index = NewSwayController._lagginSwaySetSize + index;
    NewSwayController._lagginPos = NewSwayController._laggingSwayPoses[index];
    NewSwayController._lagginRot = NewSwayController._laggingSwayRots[index];
  }

  public static Vector3 GetNewSwayPosition()
  {
    float num1 = PlayerMotionController.IsAiming ? 8f : 1f;
    float num2 = 18f;
    Vector3 vector3;
    vector3 = new Vector3(NewSwayController._lerpPosHorizontal, NewSwayController._lerpPosVertical, 0.0f);
    NewSwayController._posSmoothed = Vector3.Lerp(NewSwayController._posSmoothed, vector3, PrimeMover.Instance.DeltaTime * num2 * num1);
    NewSwayController._lagginPosSmoothed = Vector3.Lerp(NewSwayController._lagginPosSmoothed, NewSwayController._lagginPos, PrimeMover.Instance.DeltaTime * num2 * num1);
    NewSwayController._finalPos = Vector3.Lerp(NewSwayController._posSmoothed, NewSwayController._lagginPosSmoothed, PrimeMover.LaggingSwayNorm.Value);
    NewSwayController._finalPosSmoothed = Vector3.Lerp(NewSwayController._finalPosSmoothed, NewSwayController._finalPos, PrimeMover.Instance.DeltaTime * PrimeMover.NewSwayFinalLerpSpeed.Value * num1);
    if (!PrimeMover.IsWeaponSway.Value || AnimStateController.IsBlindfire)
      return Vector3.zero;
    Vector3 zero = Vector3.zero;
    zero.x = Mathf.Clamp(NewSwayController._finalPosSmoothed.x * PrimeMover.NewSwayPositionMulti.Value * WeaponController.GetWeaponMulti(false), -PrimeMover.NewSwayPositionHardClampNeg.Value, PrimeMover.NewSwayPositionHardClampPos.Value);
    zero.y = NewSwayController._finalPosSmoothed.y * PrimeMover.NewSwayWpnUnstockedDropValue.Value * WeaponController.GetWeaponMulti(false);
    return zero;
  }

  public static Quaternion GetNewSwayRotation()
  {
    float num1 = PlayerMotionController.IsAiming ? 8f : 1f;
    float num2 = 18f;
    Vector3 vector3;
    vector3 = new Vector3(NewSwayController._lerpPosHorizontal, NewSwayController._lerpPosVertical, 0.0f);
    vector3.x = NewSwayController._leanVerticalLerp + NewSwayController._vertDropFromRotLerp + NewSwayController._hyperVerticalLerp;
    vector3.y = NewSwayController._weaponTiltLerp;
    vector3.z = NewSwayController._lerpRot;
    NewSwayController._rotSmoothed = Vector3.Lerp(NewSwayController._rotSmoothed, vector3, PrimeMover.Instance.DeltaTime * num2 * num1);
    NewSwayController._lagginRotSmoothed = Vector3.Lerp(NewSwayController._lagginRotSmoothed, NewSwayController._lagginRot, PrimeMover.Instance.DeltaTime * num2 * num1);
    NewSwayController._finalRot = Vector3.Lerp(NewSwayController._rotSmoothed, NewSwayController._lagginRotSmoothed, PrimeMover.LaggingSwayNorm.Value);
    NewSwayController._finalRotSmoothed = Vector3.Lerp(NewSwayController._finalRotSmoothed, NewSwayController._finalRot, PrimeMover.Instance.DeltaTime * PrimeMover.NewSwayFinalLerpSpeed.Value * num1);
    return !PrimeMover.IsWeaponSway.Value || AnimStateController.IsBlindfire ? Quaternion.identity : Quaternion.Euler(NewSwayController._finalRotSmoothed.x, NewSwayController._finalRotSmoothed.y, NewSwayController._finalRotSmoothed.z * PrimeMover.NewSwayRotationMulti.Value * WeaponController.GetWeaponMulti(false));
  }
}

