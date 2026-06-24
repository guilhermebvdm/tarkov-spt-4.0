using EFT;
using System;
using UnityEngine;
using static TarkovIRL.AnimStateController;

namespace TarkovIRL
{
    internal class AugmentedReloadController
    {
        static readonly float _IntoReloadX = 17f;
        static readonly float _IntoReloadY = 4f;
        static readonly float _IntoReloadZ = 5f;

        static readonly float _OutReloadX = 7f;
        static readonly float _OutReloadY = 2f;
        static readonly float _OutReloadZ = -3f;

        public static bool _AugmentedModeOn = false;
        static EWeaponState _state;
        static EWeaponState _stateLastFrame;
        static ObjectInHandsAnimator _animator = null;

        static readonly int _animatorLayer = 1;

        public static Vector3 GetAugmentedReloadHeadOffset()
        {
            if (!PrimeMover.IsAugmentedReload.Value)
            {
                return Vector3.zero;
            }

            _state = AnimStateController.WeaponState;

            if (!_AugmentedModeOn)
            {
                return Vector3.zero;
            }

            if (_state == EWeaponState.INTO_RELOAD)
            {
                return new Vector3(_OutReloadX, _OutReloadY, _OutReloadZ);
            }

            else if (_state == EWeaponState.MID_RELOAD)
            {
                return new Vector3(_IntoReloadX, _IntoReloadY, _IntoReloadZ);
            }

            else if (_state == EWeaponState.MID_RELOAD_2)
            {
                return new Vector3(_OutReloadX, _OutReloadY, _OutReloadZ);
            }

            else if (_state == EWeaponState.CHECK_MAG)
            {
                return new Vector3(_OutReloadX, _OutReloadY, _OutReloadZ);
            }

            return Vector3.zero;
        }

        public static void ToggleAugmentedMode()
        {
            if (AugmentedSwitchOpen())
            {
                _AugmentedModeOn = !_AugmentedModeOn;
            }
            else
            {
                _AugmentedModeOn = false;
            }
        }

        public static void RefreshAnimator(ObjectInHandsAnimator animator)
        {
            _animator = animator;
        }

        public static void Update()
        {
            if (!PrimeMover.IsAugmentedReload.Value)
            {
                return;
            }

            if (_animator == null)
            {
                return;
            }

            UpdateSpeed();

            if (_stateLastFrame != EWeaponState.INTO_RELOAD && _state == EWeaponState.INTO_RELOAD)
            {
                if (PrimeMover.IsAugmentedReloadDefault.Value)
                {
                    _AugmentedModeOn = true;
                }
            }
            _stateLastFrame = _state;
        }

        static void UpdateSpeed()
        {
            if (!AugmentedSwitchOpen())
            {
                _AugmentedModeOn = false;
                //SetAnimSpeed(1f);
            }
            else
            {
                SetAnimSpeed(GetReloadSpeedFromContext());
            }
        }

        static void SetAnimSpeed(float speed)
        {
            if (_animator == null)
            {
                return;
            }
            try
            {
                _animator.SetAnimationSpeed(speed);
            }
            catch (Exception e)
            {
                //TIRLUtils.LogError($"set anim speed failed, {e}");
                _animator = null;
            }
        }

        static float GetReloadSpeedFromContext()
        {
            float sprintingMulti = PlayerMotionController.IsSprinting ? PrimeMover.AugmentedReloadSprintingDebuff.Value : 1f;
            float augmentedMulti = _AugmentedModeOn ? PrimeMover.AugmentedReloadSpeed.Value : 1f;
            float slowerReloadMulti;
            if (_animator != null && _state == EWeaponState.MID_RELOAD)
            {
                try
                {
                    float animTime = _animator.Animator.GetCurrentAnimatorStateInfo(_animatorLayer).normalizedTime;
                    slowerReloadMulti = animTime < 1f ? PrimeMover.Instance.SlowReloadCurve.Evaluate(animTime) : 1f;
                }
                catch (NullReferenceException e)
                {
                    TIRLUtils.LogError($"anim layer {_animatorLayer} returned null -- {e}");
                    slowerReloadMulti = 1f;
                }
            }
            else
            {
                slowerReloadMulti = 1f;
            }
            float realismSpeed = _state == EWeaponState.CHECK_MAG ? RealismWrapper.GetRealismCheckMagSpeed() : RealismWrapper.GetRealismReloadSpeed();
            return sprintingMulti * augmentedMulti * realismSpeed * slowerReloadMulti;
        }

        static bool AugmentedSwitchOpen()
        {
            return _state == EWeaponState.INTO_RELOAD || _state == EWeaponState.MID_RELOAD || _state == EWeaponState.MID_RELOAD_2 || _state == EWeaponState.OUT_OF_RELOAD || _state == EWeaponState.RELOAD_FAST || _state == EWeaponState.CHECK_MAG;
        }
    }
}