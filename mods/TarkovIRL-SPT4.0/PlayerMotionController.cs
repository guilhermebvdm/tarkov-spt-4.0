// Decompiled with JetBrains decompiler
// Type: TarkovIRL.PlayerMotionController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class PlayerMotionController
{
  private static PlayerMotionController.EPlayerDir _dir;
  private static Vector2 _playerRotLastFrame = Vector2.zero;
  private static Vector3 _playerPosLastFrame = Vector3.zero;
  private static float _playerRotationHistory = 0.0f;
  private static float _playerRotationAvg = 0.0f;
  private static float _horizontalRotationHistory = 0.0f;
  private static float _horizontalRotationValue = 0.0f;
  private static float _verticalRotationHistory = 0.0f;
  private static float _verticalRotationValue = 0.0f;
  private static bool _playerMoving = false;
  private static float _verticalAvg = 0.0f;
  private static float _playerSpeed = 0.0f;
  private static float _leanNormalized = 0.0f;
  private static float _armStam = 0.0f;
  public static bool IsAiming = false;
  public static bool IsSprinting = false;
  public static bool IsProne = false;
  public static bool IsHoldingBreath = false;
  public static bool IsAugmentedBreath = false;
  private static Vector3 _lastPositionRecorded = Vector3.zero;
  private static Vector2 _lastRotationRecorded = Vector2.zero;

  public static void UpdateMovementMeasurementsInFDT(float fdt)
  {
    PlayerMotionController.UpdateIsMovingBool(PlayerMotionController._lastPositionRecorded);
    PlayerMotionController.UpdateRotationEngine(PlayerMotionController._lastRotationRecorded, fdt);
  }

  public static void UpdateMovementInformation(Player player)
  {
    PlayerMotionController._lastPositionRecorded = player.Position;
    PlayerMotionController._lastRotationRecorded = player.Rotation;
    PlayerMotionController.IsAiming = player.ProceduralWeaponAnimation.IsAiming;
    PlayerMotionController.IsProne = player.IsInPronePose;
    PlayerMotionController.IsSprinting = player.IsSprintEnabled;
    PlayerMotionController._playerSpeed = player.Speed;
    PlayerMotionController._leanNormalized = player.MovementContext.Tilt / 5f;
    PlayerMotionController._armStam = player.Physical.HandsStamina.NormalValue;
    PlayerMotionController.UpdateMovementDirection((Vector2)player.InputDirection);
  }

  private static void UpdateIsMovingBool(Vector3 position)
  {
    PlayerMotionController._playerMoving = (double) Vector3.Distance(position, PlayerMotionController._playerPosLastFrame) > (double) PrimeMover.MotionTrackingThreshold.Value;
    PlayerMotionController._playerPosLastFrame = position;
  }

  private static void UpdateRotationEngine(Vector2 newRot, float dt)
  {
    PlayerMotionController._verticalAvg = (double) newRot.y > (double) PlayerMotionController._playerRotLastFrame.y ? 1f : -1f;
    float num1 = Vector2.Distance(newRot, PlayerMotionController._playerRotLastFrame) * dt;
    float num2 = newRot.x - PlayerMotionController._playerRotLastFrame.x;
    float num3 = newRot.y - PlayerMotionController._playerRotLastFrame.y;
    float num4 = num2 * dt;
    float num5 = num3 * dt;
    if ((double) num4 < -1.0)
      num4 = 0.0f;
    if ((double) num4 > 1.0)
      num4 = 0.0f;
    PlayerMotionController._playerRotationHistory += num1;
    PlayerMotionController._playerRotationHistory -= PlayerMotionController._playerRotationAvg;
    PlayerMotionController._playerRotationHistory = Mathf.Clamp(PlayerMotionController._playerRotationHistory, 0.0f, PrimeMover.RotationHistoryClamp.Value);
    PlayerMotionController._playerRotationAvg = PlayerMotionController._playerRotationHistory * dt * PrimeMover.RotationAverageDTMulti.Value;
    PlayerMotionController._horizontalRotationHistory += num4;
    PlayerMotionController._horizontalRotationHistory -= PlayerMotionController._horizontalRotationValue;
    PlayerMotionController._horizontalRotationValue = PlayerMotionController._horizontalRotationHistory * dt * PrimeMover.RotationAverageDTMulti.Value;
    PlayerMotionController._verticalRotationHistory += num5;
    PlayerMotionController._verticalRotationHistory -= PlayerMotionController._verticalRotationValue;
    PlayerMotionController._verticalRotationValue = PlayerMotionController._verticalRotationHistory * dt * PrimeMover.RotationAverageDTMulti.Value;
    PlayerMotionController._playerRotLastFrame = newRot;
  }

  public static float GetNormalSpeed() => PlayerMotionController._playerSpeed / 0.6f;

  public static float VerticalTrend => PlayerMotionController._verticalAvg;

  public static bool IsPlayerMovement => PlayerMotionController._playerMoving;

  public static float RotationDelta => PlayerMotionController._playerRotationAvg;

  public static float HorizontalRotationDelta => PlayerMotionController._horizontalRotationValue;

  public static float VerticalRotationDelta => PlayerMotionController._verticalRotationValue;

  public static float LeanNormal => PlayerMotionController._leanNormalized;

  private static void UpdateMovementDirection(Vector3 playerInput)
  {
    bool flag1 = (double) playerInput.y > 0.0;
    bool flag2 = (double) playerInput.y < 0.0;
    bool flag3 = (double) playerInput.x < 0.0;
    bool flag4 = (double) playerInput.x > 0.0;
    if (!PlayerMotionController._playerMoving)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.NONE;
    else if (flag1 && !flag3 && !flag4)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.FWD;
    else if (flag1 & flag3)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.FWDLEFT;
    else if (flag1 & flag4)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.FWDRIGHT;
    else if (flag2 && !flag3 && !flag4)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.BWD;
    else if (flag2 & flag3)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.BWDLEFT;
    else if (flag2 & flag4)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.BWDRIGHT;
    else if (flag3)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.LEFT;
    else if (flag4)
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.RIGHT;
    else
      PlayerMotionController._dir = PlayerMotionController.EPlayerDir.NONE;
  }

  public static PlayerMotionController.EPlayerDir Direction => PlayerMotionController._dir;

  public static float ArmStamNorm => PlayerMotionController._armStam;

  public enum EPlayerDir
  {
    FWD,
    BWD,
    LEFT,
    RIGHT,
    FWDLEFT,
    FWDRIGHT,
    BWDLEFT,
    BWDRIGHT,
    NONE,
  }
}
