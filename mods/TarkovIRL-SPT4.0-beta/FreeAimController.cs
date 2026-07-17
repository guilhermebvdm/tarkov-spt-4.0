using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class FreeAimController
{
  public static Vector2 Offset = Vector2.zero;
  private static float _attenFactorLerp = 1f;
  private static float _currentSensitivity = 1f;
  private static float _currentAutoCenterSpeed = 0.0f;
  private static float _currentBoundsX = 10f;
  private static float _currentBoundsY = 10f;

  public static void ApplyInput(ref Vector2 deltaRotation)
  {
    bool isAiming = PlayerMotionController.IsAiming;
    if (!PrimeMover.EnableMod.Value || !isAiming && StanceController.CurrentStance != EStance.None || PlayerMotionController.IsSprinting || Patch_CalculateCameraPosition_HandLayers.IsLeftShoulderOrDelay)
      FreeAimController.Offset = Vector2.Lerp(FreeAimController.Offset, Vector2.zero, Time.deltaTime * PrimeMover.FreeAimReturnSpeed.Value);
    else if (!isAiming && !PrimeMover.EnableFreeAim.Value || isAiming && !PrimeMover.EnableFreeAimADS.Value)
    {
      float num = isAiming ? PrimeMover.FreeAimReturnSpeedADS.Value : PrimeMover.FreeAimReturnSpeed.Value;
      FreeAimController.Offset = Vector2.Lerp(FreeAimController.Offset, Vector2.zero, Time.deltaTime * num);
    }
    else
    {
      float num1 = (isAiming ? PrimeMover.FreeAimSensitivityADS.Value : PrimeMover.FreeAimSensitivity.Value) * PrimeMover.MasterSensitivityMultiplier.Value;
      FreeAimController._currentSensitivity = Mathf.Lerp(FreeAimController._currentSensitivity, num1, Time.deltaTime * 8f);
      float currentSensitivity = FreeAimController._currentSensitivity;
      float num2 = isAiming ? PrimeMover.FreeAimBoundsXADS.Value : PrimeMover.FreeAimBoundsX.Value;
      float num3 = isAiming ? PrimeMover.FreeAimBoundsYADS.Value : PrimeMover.FreeAimBoundsY.Value;
      float num4 = isAiming ? 6f : 15f;
      FreeAimController._currentBoundsX = Mathf.Lerp(FreeAimController._currentBoundsX, num2, Time.deltaTime * num4);
      FreeAimController._currentBoundsY = Mathf.Lerp(FreeAimController._currentBoundsY, num3, Time.deltaTime * num4);
      float rawHorizontalSpeed = PlayerMotionController.RawHorizontalSpeed;
      float num5 = 1f;
      if ((double) rawHorizontalSpeed > (double) PrimeMover.FastTurnThreshold.Value)
        num5 = Mathf.Clamp01((float) (1.0 - (double) (rawHorizontalSpeed - PrimeMover.FastTurnThreshold.Value) * (double) PrimeMover.FastTurnAttenuation.Value * 0.004999999888241291));
      FreeAimController._attenFactorLerp = (double) num5 >= (double) FreeAimController._attenFactorLerp ? Mathf.Lerp(FreeAimController._attenFactorLerp, num5, Time.deltaTime * 10f) : num5;
      
      if (FreeAimController._attenFactorLerp < 1f)
      {
        float springForce = (1f - (float)FreeAimController._attenFactorLerp) * 15f;
        FreeAimController.Offset = Vector2.Lerp(FreeAimController.Offset, Vector2.zero, Time.deltaTime * springForce);
      }

      Vector2 vector2_1 = deltaRotation * currentSensitivity * (float)FreeAimController._attenFactorLerp;
      Vector2 vector2_2 = (FreeAimController.Offset + vector2_1);
      Vector2 vector2_3;
      vector2_3 = new Vector2(Mathf.Clamp(vector2_2.x, -FreeAimController._currentBoundsX, FreeAimController._currentBoundsX), Mathf.Clamp(vector2_2.y, -FreeAimController._currentBoundsY, FreeAimController._currentBoundsY));
      Vector2 vector2_4 = (vector2_3 - FreeAimController.Offset);
      FreeAimController.Offset = vector2_3;
      deltaRotation = (deltaRotation - (vector2_4 / currentSensitivity));
      if (!(isAiming ? PrimeMover.EnableCameraAutoCenterADS.Value : PrimeMover.EnableCameraAutoCenter.Value))
        return;
      float num6 = isAiming ? PrimeMover.CameraAutoCenterSpeedADS.Value : PrimeMover.CameraAutoCenterSpeed.Value;
      FreeAimController._currentAutoCenterSpeed = Mathf.Lerp(FreeAimController._currentAutoCenterSpeed, num6, Time.deltaTime * 6f);
      float currentAutoCenterSpeed = FreeAimController._currentAutoCenterSpeed;
      Vector2 vector2_5 = (FreeAimController.Offset * Time.deltaTime * currentAutoCenterSpeed);
      deltaRotation = (deltaRotation + (vector2_5 / currentSensitivity));
      FreeAimController.Offset = (FreeAimController.Offset - vector2_5);
    }
  }

  public static void GetOffsets(out Vector3 posOffset, out Quaternion rotOffset)
  {
    if (!PrimeMover.EnableFreeAim.Value || !PrimeMover.EnableMod.Value || !PlayerMotionController.IsAiming && StanceController.CurrentStance != 0)
    {
      posOffset = Vector3.zero;
      rotOffset = Quaternion.identity;
    }
    else
    {
      posOffset = Vector3.zero;
      rotOffset = Quaternion.Euler(FreeAimController.Offset.y, 0.0f, FreeAimController.Offset.x);
    }
  }
}


