// Decompiled with JetBrains decompiler
// Type: TarkovIRL.ThrowController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal static class ThrowController
{
  private static readonly float _OverhandYExtraMulti = 4f;
  private static float _throwLerp = 1f;
  private static bool _isThrowing = false;
  private static bool _isUnderhand = false;

  public static void UpdateLerp(float dt)
  {
    if (!ThrowController._isThrowing)
      return;
    ThrowController._throwLerp = Mathf.Lerp(ThrowController._throwLerp, 1f, dt * PrimeMover.ThrowSpeedMulti.Value);
    if ((double) ThrowController._throwLerp < 0.98000001907348633)
      return;
    ThrowController._isThrowing = false;
    ThrowController._throwLerp = 1f;
  }

  public static void NewThrow(bool isUnderhand)
  {
    ThrowController._isThrowing = true;
    ThrowController._throwLerp = 0.0f;
    ThrowController._isUnderhand = isUnderhand;
  }

  public static Vector3 GetThrowOffset
  {
    get
    {
      AnimationCurve animationCurve1 = ThrowController._isUnderhand ? PrimeMover.Instance.ThrowVisualUnderhandCurveX : PrimeMover.Instance.ThrowVisualCurveX;
      AnimationCurve animationCurve2 = ThrowController._isUnderhand ? PrimeMover.Instance.ThrowVisualUnderhandCurveY : PrimeMover.Instance.ThrowVisualCurveY;
      float num1 = animationCurve1.Evaluate(ThrowController._throwLerp) * PrimeMover.ThrowStrengthMulti.Value;
      float num2 = ThrowController._isUnderhand ? -1f : 1f;
      float num3 = animationCurve2.Evaluate(ThrowController._throwLerp) * ThrowController._OverhandYExtraMulti * PrimeMover.ThrowStrengthMulti.Value * num2;
      return new Vector3(-num1, -num3, 0.0f);
    }
  }

  public static bool IsThrowing => ThrowController._isThrowing;

  public static float ThrowProgress => ThrowController._throwLerp;
}
