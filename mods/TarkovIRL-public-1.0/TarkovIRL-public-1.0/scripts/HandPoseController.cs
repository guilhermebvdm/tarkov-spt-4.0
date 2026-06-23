using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TarkovIRL
{
    internal class HandPoseController
    {
        // readonlys
        static readonly float _LerpRate = 4f;
        static readonly float _OffsetTargetMultiY = .03f;
        static readonly float _OffsetTargetMultiZ = -0.05f;
        static readonly float _ChangePoseModifier = 0.01f;

        // vars
        static float _currentLerpRate = 0;
        static float _offsetTargetY = 0;
        static float _offsetTargetZ = 0;
        static float _offsetTargetYLerp = 0;
        static float _offsetTargetZLerp = 0;

        static Vector3 _poseShiftVectorVertical = Vector3.zero;
        static Vector3 _poseShiftVectorVerticalLerped = Vector3.zero;
        static Quaternion _poseShiftVectorRotation = Quaternion.identity;
        static Quaternion _poseShiftVectorRotationLerped = Quaternion.identity;
        static float _poseLevelLastFrame = 0;
        static float _poseDifference = 0;
        static float _changingPoseCycle = 0;
        static float _changingPoseCycle2 = 0;
        static bool _isChangingPose = false;

        static void VerticalPoseUpdate(float dt)
        {
            _offsetTargetYLerp = Mathf.Lerp(_offsetTargetYLerp, _offsetTargetY, dt * _currentLerpRate);
            _offsetTargetZLerp = Mathf.Lerp(_offsetTargetZLerp, _offsetTargetZ, dt * _currentLerpRate);
        }
        static void ChangePoseUpdate(Player player)
        {
            if (_isChangingPose)
            {
                float dt = PrimeMover.Instance.DeltaTime;
                float vertDiffAbs = Mathf.Abs(_poseDifference);
                bool leftBreak = player.HealthController.IsBodyPartBroken(EBodyPart.LeftArm);
                bool rightBreak = player.HealthController.IsBodyPartBroken(EBodyPart.RightArm);
                float handBreakMulti = 1f;
                handBreakMulti *= leftBreak ? 0.5f : 1f;
                handBreakMulti *= rightBreak ? 0.5f : 1f;
                float speedMod = 1f + (1f - vertDiffAbs) * handBreakMulti;
                bool goingDown = _poseDifference < 0;
                float directionMulti = goingDown ? 1.5f : 2f;
                float weaponSizeMulti = WeaponController.GetWeaponMulti(false);
                float weaponSizeSpeedMulti = WeaponController.GetWeaponMulti(true) * 2.2f ;
                float directionSpeedMod = goingDown ? 2f : 1.1f;
                float efficiencyMulti = EfficiencyController.EfficiencyModifier;
                _changingPoseCycle += dt * speedMod * directionSpeedMod * weaponSizeSpeedMulti;
                _changingPoseCycle2 += dt * speedMod * directionSpeedMod * weaponSizeSpeedMulti * 0.7f;
                if (_changingPoseCycle >= 1f && _changingPoseCycle2 >= 1f)
                {
                    _isChangingPose = false;
                    _changingPoseCycle = 0;
                    _changingPoseCycle2 = 0;
                    return;
                }
                float directionAttenuationMod = _poseDifference < 0 ? 1f : 0.5f;
                float curveVert = goingDown ? PrimeMover.Instance.PoseChangeCurve.Evaluate(_changingPoseCycle) : PrimeMover.Instance.PoseChangeCurveUp.Evaluate(_changingPoseCycle);
                float curveRadial = goingDown ? PrimeMover.Instance.PoseChangeDownRadialCurve.Evaluate(_changingPoseCycle) * 5f: 0f;
                float curveRot = goingDown ? 0 : PrimeMover.Instance.PoseChangeDownRadialCurve.Evaluate(_changingPoseCycle2);
                float finalMulti = vertDiffAbs * _ChangePoseModifier * directionAttenuationMod * directionMulti * weaponSizeMulti * efficiencyMulti;
                float addedVert = curveVert * finalMulti;
                float addedRadial = curveRadial * finalMulti;
                float armStamMulti = 1f - PlayerMotionController.ArmStamNorm;
                float addedRot = curveRot * finalMulti * 7f * armStamMulti;
                _poseShiftVectorVertical = new Vector3(0, addedVert, addedRadial);
                _poseShiftVectorRotation = TIRLUtils.GetQuatFromV3(new Vector3(addedRot, 0, 0));

            }
            else
            {
                _poseShiftVectorVertical = Vector3.zero;
                _poseShiftVectorRotation = Quaternion.identity;
            }
            _poseShiftVectorVerticalLerped = Vector3.Lerp(_poseShiftVectorVerticalLerped, _poseShiftVectorVertical, PrimeMover.Instance.DeltaTime * 8f);
            _poseShiftVectorRotationLerped = Quaternion.Lerp(_poseShiftVectorRotationLerped, _poseShiftVectorRotation, PrimeMover.Instance.DeltaTime * 8f);
        }

        public static Vector3 GetModifiedHandPosWithPose(Player player)
        {
            VerticalPoseUpdate(player.DeltaTime);

            _currentLerpRate = _LerpRate;
            float poseLevel = player.PoseLevel;

            _offsetTargetY = (1f - poseLevel) * _OffsetTargetMultiY;
            _offsetTargetZ = (1f - poseLevel) * _OffsetTargetMultiZ;

            if (player.ProceduralWeaponAnimation.IsAiming)
            {
                _offsetTargetY = 0;
                _offsetTargetZ = 0;

                _currentLerpRate = _LerpRate * 2f;
            }

            return new Vector3(0, _offsetTargetYLerp, _offsetTargetZLerp);
        }

        public static Vector3 GetModifiedHandPosWithPoseChange(Player player)
        {
            ChangePoseUpdate(player);

            Vector3 addedVert = _poseShiftVectorVerticalLerped;
            float poseLevelThisFrame = player.PoseLevel;
            if (poseLevelThisFrame != _poseLevelLastFrame)
            {
                _poseDifference = poseLevelThisFrame - _poseLevelLastFrame;
                if (!_isChangingPose)
                {
                    _isChangingPose = true;
                }
            }

            if (!player.ProceduralWeaponAnimation.IsAiming)
            {
                addedVert.y *= 3f;
            }

            _poseLevelLastFrame = poseLevelThisFrame;
            return addedVert;
        }

        public static Quaternion GetModifiedHandRotWithPoseChange()
        {
            return _poseShiftVectorRotationLerped;
        }
    }
}
