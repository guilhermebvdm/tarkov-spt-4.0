// Decompiled with JetBrains decompiler
// Type: TarkovIRL.NewSwayController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using RealismMod;
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

  public static void UpdateLerp(float deltaTime)
  {
    bool flag = WeaponController.HasCheekWeld();
    float num1 = PlayerMotionController.HorizontalRotationDelta * PrimeMover.WeaponSwayMulti.Value;
    float num2 = PrimeMover.NewSwayRotDeltaClamp.Value;
    float num3 = Mathf.Clamp(num1, 0.0f - num2, num2);
    float num4 = flag ? -1f : 0.5f;
    float num5 = !flag || !PlayerMotionController.IsAiming ? 1f : 0.0f;
    float num6 = flag || WeaponController.IsPistol || !PlayerMotionController.IsAiming ? 1f : 0.7f;
    float num7 = !flag || PlayerMotionController.IsAiming ? 1f : 0.7f;
    float num8 = flag || !PlayerMotionController.IsAiming ? 1f : 1.25f;
    float num9 = !flag || !PlayerMotionController.IsAiming ? 1f : -0.25f;
    float num10 = !flag || !PlayerMotionController.IsAiming ? 1f : 2f;
    float num11 = !flag ? 0.5f : 1f;
    float num12 = WeaponController.GetWeaponMulti(true) * EfficiencyController.EfficiencyModifierInverse;
    float num13 = !flag ? -1f : 0.0f;
    float num14 = PlayerMotionController.IsAiming ? 0.5f : 1f;
    float num15 = WeaponController.IsPistol ? 2f : 1f;
    float num16 = num3 * num4 * num5 * num6 * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier;
    float num17 = Mathf.Abs(num3) * num13 * num14 * num15;
    float num18 = PrimeMover.NewSwayPositionDTMulti.Value;
    float num19 = PrimeMover.NewSwayRotationDTMulti.Value;
    NewSwayController._lerpPosHorizontal = Mathf.Lerp(NewSwayController._lerpPosHorizontal, num16, deltaTime * num12 * num7 * num8 * num18);
    NewSwayController._lerpPosVertical = Mathf.Lerp(NewSwayController._lerpPosVertical, num17, deltaTime * num12 * PrimeMover.NewSwayWpnUnstockedDropSpeed.Value);
    float num20 = num3 * num9 * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier;
    float num21 = PlayerMotionController.IsAiming ? PrimeMover.NewSwayADSRotClamp.Value * num2 : 1f;
    float num22 = Mathf.Clamp(num20, 0.0f - num21, num21);
    NewSwayController._lerpRot = Mathf.Lerp(NewSwayController._lerpRot, num22, deltaTime * num12 * num10 * num11 * num19);
    float num23 = PrimeMover.NewSwayRotFinalClamp.Value;
    NewSwayController._lerpRot = Mathf.Clamp(NewSwayController._lerpRot, 0.0f - num23, num23);
    float num24 = PlayerMotionController.IsAiming ? 0.0f : PrimeMover.WeaponCantValue.Value * 0.1f;
    NewSwayController._weaponTiltLerp = Mathf.Lerp(NewSwayController._weaponTiltLerp, num24, deltaTime * 20f);
    float num25 = (PlayerMotionController.IsAiming ? 0.0f : PlayerMotionController.LeanNormal * PrimeMover.LeanExtraVerticalMulti.Value * WeaponController.GetWeaponMulti(false)) * -1f;
    if (AnimStateController.IsLeftShoulder)
      num25 *= -1f;
    NewSwayController._leanVerticalLerp = Mathf.Lerp(NewSwayController._leanVerticalLerp, num25, deltaTime * 10f * EfficiencyController.EfficiencyModifierInverse);
    float num26 = !WeaponController.IsStocked || !PlayerMotionController.IsAiming ? 1f : 0.2f;
    float num27 = PlayerMotionController.RotationDelta * PrimeMover.NewSwayWpnDropFromRotMulti.Value * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * num26;
    NewSwayController._vertDropFromRotLerp = Mathf.Lerp(NewSwayController._vertDropFromRotLerp, num27, deltaTime * PrimeMover.NewSwayWpnUnstockedDropSpeed.Value);
    float verticalRotationDelta = PlayerMotionController.VerticalRotationDelta;
    float num28 = (double) verticalRotationDelta < 0.0 ? -1f : 1f;
    float num29 = PlayerMotionController.IsAiming ? 0.0f : verticalRotationDelta * num28 * PrimeMover.HyperVerticalMulti.Value * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier;
    float num30 = PrimeMover.HyperVerticalClamp.Value;
    float num31 = Mathf.Clamp(num29, 0.0f - num30, num30);
    float num32 = PrimeMover.HyperVerticalDT.Value * RealismWrapper.WeaponBalanceMulti;
    NewSwayController._hyperVerticalLerp = Mathf.Lerp(NewSwayController._hyperVerticalLerp, num31, deltaTime * num32);
    NewSwayController.ProcessLagginSway();
  }

  private static void ProcessLagginSway()
  {
    NewSwayController._laggingSwayPoses[NewSwayController._laggingSwayIterator] = NewSwayController._posSmoothed;
    NewSwayController._laggingSwayRots[NewSwayController._laggingSwayIterator] = NewSwayController._rotSmoothed;
    ++NewSwayController._laggingSwayIterator;
    if (NewSwayController._laggingSwayIterator > 29)
      NewSwayController._laggingSwayIterator = 0;
    float num1 = Mathf.Clamp(EfficiencyController.EfficiencyModifier, 1f, 10f);
    float num2 = Mathf.Clamp(WeaponController.GetWeaponMulti(false) * RealismWrapper.WeaponBalanceMulti * num1 * PrimeMover.LaggingSwayMulti.Value * (StanceController.CurrentStance == EStance.HighReady ? 0.5f : 1f), 1f, PrimeMover.LaggingSwayClamp.Value);
    int index = NewSwayController._laggingSwayIterator - Mathf.RoundToInt(num2);
    if (index < 0)
      index = NewSwayController._lagginSwaySetSize + index;
    NewSwayController._lagginPos = NewSwayController._laggingSwayPoses[index];
    NewSwayController._lagginRot = NewSwayController._laggingSwayRots[index];
  }

  public static Vector3 GetNewSwayPosition()
  {
    float num = 18f;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(NewSwayController._lerpPosHorizontal, NewSwayController._lerpPosVertical, 0.0f);
    NewSwayController._posSmoothed = Vector3.Lerp(NewSwayController._posSmoothed, vector3, PrimeMover.Instance.DeltaTime * num);
    NewSwayController._lagginPosSmoothed = Vector3.Lerp(NewSwayController._lagginPosSmoothed, NewSwayController._lagginPos, PrimeMover.Instance.DeltaTime * num);
    NewSwayController._finalPos = Vector3.Lerp(NewSwayController._posSmoothed, NewSwayController._lagginPosSmoothed, PrimeMover.LaggingSwayNorm.Value);
    NewSwayController._finalPosSmoothed = Vector3.Lerp(NewSwayController._finalPosSmoothed, NewSwayController._finalPos, PrimeMover.Instance.DeltaTime * PrimeMover.NewSwayFinalLerpSpeed.Value);
    if (!PrimeMover.IsWeaponSway.Value || AnimStateController.IsBlindfire)
      return Vector3.zero;
    Vector3 zero = Vector3.zero;
    zero.x = NewSwayController._finalPosSmoothed.x * PrimeMover.NewSwayPositionMulti.Value * WeaponController.GetWeaponMulti(false);
    zero.y = NewSwayController._finalPosSmoothed.y * PrimeMover.NewSwayWpnUnstockedDropValue.Value * WeaponController.GetWeaponMulti(false);
    return zero;
  }

  public static Quaternion GetNewSwayRotation()
  {
    float num = 18f;
    Vector3 vector3;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3).\u002Ector(NewSwayController._lerpPosHorizontal, NewSwayController._lerpPosVertical, 0.0f);
    vector3.x = NewSwayController._leanVerticalLerp + NewSwayController._vertDropFromRotLerp + NewSwayController._hyperVerticalLerp;
    vector3.y = NewSwayController._weaponTiltLerp;
    vector3.z = NewSwayController._lerpRot;
    NewSwayController._rotSmoothed = Vector3.Lerp(NewSwayController._rotSmoothed, vector3, PrimeMover.Instance.DeltaTime * num);
    NewSwayController._lagginRotSmoothed = Vector3.Lerp(NewSwayController._lagginRotSmoothed, NewSwayController._lagginRot, PrimeMover.Instance.DeltaTime * num);
    NewSwayController._finalRot = Vector3.Lerp(NewSwayController._rotSmoothed, NewSwayController._lagginRotSmoothed, PrimeMover.LaggingSwayNorm.Value);
    NewSwayController._finalRotSmoothed = Vector3.Lerp(NewSwayController._finalRotSmoothed, NewSwayController._finalRot, PrimeMover.Instance.DeltaTime * PrimeMover.NewSwayFinalLerpSpeed.Value);
    if (!PrimeMover.IsWeaponSway.Value || AnimStateController.IsBlindfire)
      return Quaternion.identity;
    Quaternion identity = Quaternion.identity;
    identity.x = NewSwayController._finalRotSmoothed.x;
    identity.y = NewSwayController._finalRotSmoothed.y;
    identity.z = NewSwayController._finalRotSmoothed.z * PrimeMover.NewSwayRotationMulti.Value * WeaponController.GetWeaponMulti(false);
    return identity;
  }
}
