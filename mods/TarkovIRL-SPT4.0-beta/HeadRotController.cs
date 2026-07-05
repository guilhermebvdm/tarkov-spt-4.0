// Decompiled with JetBrains decompiler
// Type: TarkovIRL.HeadRotController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class HeadRotController
{
  private static Vector3 _headRotLerp = Vector3.zero;
  private static Vector3 _headRotLerpTarget = Vector3.zero;

  public static void UpdateLerp(float dt)
  {
    HeadRotController._headRotLerp = Vector3.Lerp(HeadRotController._headRotLerp, HeadRotController._headRotLerpTarget, dt * PrimeMover.HeadRotationLerpSpeed.Value);
  }

  public static Vector3 GetHeadRotThisFrame(Vector3 headRotThisFrame)
  {
    Vector3 vector3 = headRotThisFrame;
    HeadRotController._headRotLerpTarget = !ThrowController.IsThrowing ? new Vector3(0.0f, 0.0f, PlayerMotionController.LeanNormal * PrimeMover.LeanCounterRotateMod.Value) : ThrowController.GetThrowOffset;
    Vector3 reloadHeadOffset = AugmentedReloadController.GetAugmentedReloadHeadOffset();
    HeadRotController._headRotLerpTarget = (HeadRotController._headRotLerpTarget + reloadHeadOffset);
    if (PrimeMover.IsHeadTiltADS.Value)
    {
      float Y;
      float Z;
      ParallaxAdsController.GetParallaxADSHeadTilt(out Y, out Z);
      HeadRotController._headRotLerpTarget.y += Y;
      HeadRotController._headRotLerpTarget.z += Z;
    }
    return (vector3 + HeadRotController._headRotLerp);
  }
}
