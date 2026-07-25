using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal static class FootstepController
{
  private static readonly float _UpdateMulti = 12f;
  private static readonly float _StepIntensityMulti = 1f / 400f;
  private static float _stepLerp = 0.0f;
  private static float _stepLerpSmoothed = 0.0f;
  private static float _currentSpeed = 0.0f;
  private static float _sideToSideRotationLerp = 0.0f;
  private static float _sideToSidePositionLerp = 0.0f;
  private static float _sideToSideRotationSmoothingLerp = 0.0f;
  private static float _sideToSidePositionSmoothingLerp = 0.0f;
  private static bool _currentStepLeft = false;
  private static bool _movementLastFrame = false;

  public static void UpdateStep(float dt)
  {
    PlayerMotionController.EPlayerDir direction = PlayerMotionController.Direction;
    float num1;
    int num2;
    switch (direction)
    {
      case PlayerMotionController.EPlayerDir.FWD:
        num1 = 1f;
        goto label_5;
      case PlayerMotionController.EPlayerDir.FWDLEFT:
        num2 = 1;
        break;
      default:
        num2 = direction == PlayerMotionController.EPlayerDir.FWDRIGHT ? 1 : 0;
        break;
    }
    num1 = num2 == 0 ? 0.0f : 0.35f;
label_5:
    float num3 = 1f + FootstepController._currentSpeed;
    FootstepController._stepLerp = Mathf.Lerp(FootstepController._stepLerp, 1f, dt * FootstepController._UpdateMulti * num3 * PrimeMover.FootstepLerpMulti.Value);
    FootstepController._sideToSideRotationLerp += dt * PrimeMover.SideToSideRotationDTMulti.Value * num3;
    FootstepController._sideToSidePositionLerp += dt * PrimeMover.SideToSidePositionDTMulti.Value * num3;
    float num4 = (float) ((double) PrimeMover.Instance.SideToSideCurve.Evaluate(FootstepController._sideToSideRotationLerp) * (double) PrimeMover.SideToSideSwayMulti.Value * (double) num1 * (FootstepController._currentStepLeft ? 1.0 : -1.0));
    float num5 = (float) ((double) PrimeMover.Instance.FootStepCurve.Evaluate(FootstepController._sideToSidePositionLerp) * (double) PrimeMover.SideToSideSwayMulti.Value * (double) num1 * (FootstepController._currentStepLeft ? -1.0 : 1.0));
    FootstepController._stepLerpSmoothed = Mathf.Lerp(FootstepController._stepLerpSmoothed, FootstepController._stepLerp, dt * 13f);
    FootstepController._sideToSideRotationSmoothingLerp = Mathf.Lerp(FootstepController._sideToSideRotationSmoothingLerp, num4, dt * 7f);
    FootstepController._sideToSidePositionSmoothingLerp = Mathf.Lerp(FootstepController._sideToSidePositionSmoothingLerp, num5, dt * 7f);
  }

  public static void NewStep(Player player)
  {
    FootstepController._stepLerp = 0.0f;
    FootstepController._currentSpeed = player.Speed;
    FootstepController._sideToSideRotationLerp = 0.0f;
    FootstepController._sideToSidePositionLerp = 0.0f;
    FootstepController._currentStepLeft = !PlayerMotionController.IsPlayerMovement && !FootstepController._movementLastFrame || !FootstepController._currentStepLeft;
    FootstepController._movementLastFrame = PlayerMotionController.IsPlayerMovement;
  }

  public static Vector3 GetModifiedHandPosFootstep
  {
    get
    {
      float num = 0.2f + FootstepController._currentSpeed;
      return new Vector3(0.0f, (float) ((double) PrimeMover.Instance.FootStepCurve.Evaluate(FootstepController._stepLerpSmoothed) * (double) FootstepController._StepIntensityMulti * (double) PrimeMover.FootstepIntesnityMulti.Value * ((double) EfficiencyController.EfficiencyModifier * 0.5)) * num, 0.0f);
    }
  }

  public static Quaternion GetSideToSideRotation()
  {
    return Quaternion.Euler(0.0f, 0.0f, (float) ((double) FootstepController._sideToSideRotationSmoothingLerp * (double) PlayerMotionController.GetNormalSpeed() * (PlayerMotionController.IsAiming ? 0.40000000596046448 : 1.0)));
  }

  public static Vector3 GetSideToSidePosition()
  {
    Vector3 zero = Vector3.zero;
    zero.x = FootstepController._sideToSidePositionSmoothingLerp * PlayerMotionController.GetNormalSpeed();
    zero.x *= PlayerMotionController.IsAiming ? 0.4f : 1f;
    return zero;
  }
}

