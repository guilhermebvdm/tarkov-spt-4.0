// Decompiled with JetBrains decompiler
// Type: TarkovIRL.AugmentedReloadController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using System;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class AugmentedReloadController
{
  private static readonly float _IntoReloadX = 17f;
  private static readonly float _IntoReloadY = 4f;
  private static readonly float _IntoReloadZ = 5f;
  private static readonly float _OutReloadX = 7f;
  private static readonly float _OutReloadY = 2f;
  private static readonly float _OutReloadZ = -3f;
  public static bool _AugmentedModeOn = false;
  private static AnimStateController.EWeaponState _state;
  private static AnimStateController.EWeaponState _stateLastFrame;
  private static ObjectInHandsAnimator _animator = (ObjectInHandsAnimator) null;
  private static readonly int _animatorLayer = 1;

  public static Vector3 GetAugmentedReloadHeadOffset()
  {
    if (!PrimeMover.IsAugmentedReload.Value)
      return Vector3.zero;
    AugmentedReloadController._state = AnimStateController.WeaponState;
    if (!AugmentedReloadController._AugmentedModeOn)
      return Vector3.zero;
    switch (AugmentedReloadController._state)
    {
      case AnimStateController.EWeaponState.INTO_RELOAD:
        return new Vector3(AugmentedReloadController._OutReloadX, AugmentedReloadController._OutReloadY, AugmentedReloadController._OutReloadZ);
      case AnimStateController.EWeaponState.MID_RELOAD:
        return new Vector3(AugmentedReloadController._IntoReloadX, AugmentedReloadController._IntoReloadY, AugmentedReloadController._IntoReloadZ);
      case AnimStateController.EWeaponState.MID_RELOAD_2:
        return new Vector3(AugmentedReloadController._OutReloadX, AugmentedReloadController._OutReloadY, AugmentedReloadController._OutReloadZ);
      case AnimStateController.EWeaponState.CHECK_MAG:
        return new Vector3(AugmentedReloadController._OutReloadX, AugmentedReloadController._OutReloadY, AugmentedReloadController._OutReloadZ);
      default:
        return Vector3.zero;
    }
  }

  public static void ToggleAugmentedMode()
  {
    if (AugmentedReloadController.AugmentedSwitchOpen())
      AugmentedReloadController._AugmentedModeOn = !AugmentedReloadController._AugmentedModeOn;
    else
      AugmentedReloadController._AugmentedModeOn = false;
  }

  public static void RefreshAnimator(ObjectInHandsAnimator animator)
  {
    AugmentedReloadController._animator = animator;
  }

  public static void Update()
  {
    if (!PrimeMover.IsAugmentedReload.Value || AugmentedReloadController._animator == null)
      return;
    AugmentedReloadController.UpdateSpeed();
    if (AugmentedReloadController._stateLastFrame != AnimStateController.EWeaponState.INTO_RELOAD && AugmentedReloadController._state == AnimStateController.EWeaponState.INTO_RELOAD && PrimeMover.IsAugmentedReloadDefault.Value)
      AugmentedReloadController._AugmentedModeOn = true;
    AugmentedReloadController._stateLastFrame = AugmentedReloadController._state;
  }

  private static void UpdateSpeed()
  {
    if (!AugmentedReloadController.AugmentedSwitchOpen())
      AugmentedReloadController._AugmentedModeOn = false;
    else
      AugmentedReloadController.SetAnimSpeed(AugmentedReloadController.GetReloadSpeedFromContext());
  }

  private static void SetAnimSpeed(float speed)
  {
    if (AugmentedReloadController._animator == null)
      return;
    try
    {
      AugmentedReloadController._animator.SetAnimationSpeed(speed);
    }
    catch (Exception ex)
    {
      AugmentedReloadController._animator = (ObjectInHandsAnimator) null;
    }
  }

  private static float GetReloadSpeedFromContext()
  {
    float num1 = PlayerMotionController.IsSprinting ? PrimeMover.AugmentedReloadSprintingDebuff.Value : 1f;
    float num2 = AugmentedReloadController._AugmentedModeOn ? PrimeMover.AugmentedReloadSpeed.Value : 1f;
    float num3;
    if (AugmentedReloadController._animator != null && AugmentedReloadController._state == AnimStateController.EWeaponState.MID_RELOAD)
    {
      try
      {
        float normalizedTime = AugmentedReloadController._animator.Animator.GetCurrentAnimatorStateInfo(AugmentedReloadController._animatorLayer).normalizedTime;
        num3 = (double) normalizedTime < 1.0 ? PrimeMover.Instance.SlowReloadCurve.Evaluate(normalizedTime) : 1f;
      }
      catch (NullReferenceException ex)
      {
        TIRLUtils.LogError($"anim layer {AugmentedReloadController._animatorLayer} returned null -- {ex}");
        num3 = 1f;
      }
    }
    else
      num3 = 1f;
    float num4 = AugmentedReloadController._state == AnimStateController.EWeaponState.CHECK_MAG ? RealismWrapper.GetRealismCheckMagSpeed() : RealismWrapper.GetRealismReloadSpeed();
    return num1 * num2 * num4 * num3;
  }

  private static bool AugmentedSwitchOpen()
  {
    return AugmentedReloadController._state == AnimStateController.EWeaponState.INTO_RELOAD || AugmentedReloadController._state == AnimStateController.EWeaponState.MID_RELOAD || AugmentedReloadController._state == AnimStateController.EWeaponState.MID_RELOAD_2 || AugmentedReloadController._state == AnimStateController.EWeaponState.OUT_OF_RELOAD || AugmentedReloadController._state == AnimStateController.EWeaponState.RELOAD_FAST || AugmentedReloadController._state == AnimStateController.EWeaponState.CHECK_MAG;
  }
}
