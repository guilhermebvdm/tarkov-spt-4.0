using AnimationEventSystem;
using EFT;
using EFT.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TarkovIRL
{
    public static class HandMovWithRotController
    {
        // readonlys
        static readonly float _RotPullInValue = 0.07f;
        static readonly float _LerpRate = 8f;
        static readonly float _StockMovementAddedPosValue = 0.005f;

        // vars
        static float _rotPullInTarget = 0;
        static float _rotPullInLerp = 0;

        static float _stockedMovementAddedPosTarget = 0;
        static float _stockedMovementAddedPosLerp = 0;
        static float _stockedMovementAddedPosSmoothed = 0;

        public static Vector3 GetModifiedHandPosZMovement(Player player)
        {

            float dt = player.DeltaTime;

            if (player.ProceduralWeaponAnimation.IsAiming)
            {
                _rotPullInTarget = 0;
            }
            else if (AnimStateController.IsBlindfire)
            {
                _rotPullInTarget = 0;
            }
            else
            {
                float rotDelta = Mathf.Abs(PlayerMotionController.HorizontalRotationDelta * 100f);
                rotDelta = Mathf.Clamp01( rotDelta );
                float pullInValue = WeaponController.HasCheekWeld() ? _RotPullInValue * 0.75f : _RotPullInValue;
                pullInValue *= rotDelta;
                _rotPullInTarget = pullInValue;
            }

            _rotPullInLerp = Mathf.Lerp(_rotPullInLerp, _rotPullInTarget, dt * 0.5f);
            float finalValue = PrimeMover.Instance.SmoothEdgesCurve.Evaluate(_rotPullInLerp / _RotPullInValue) * _RotPullInValue;
            return new Vector3(0, 0, -finalValue);
        }

        public static Vector3 GetModifiedHandPosForLoweredWeapon(Player player)
        {
            float dt = player.DeltaTime;
            if (AnimStateController.IsSideStep)
            {
                _stockedMovementAddedPosTarget = _StockMovementAddedPosValue * 0.2f * WeaponController.GetWeaponMulti(false);
            }
            else if (!WeaponController.HasCheekWeld() && !WeaponController.IsPistol && PlayerMotionController.IsPlayerMovement)  
            {
                _stockedMovementAddedPosTarget = _StockMovementAddedPosValue * WeaponController.GetWeaponMulti(false);
            }
            else
            {
                _stockedMovementAddedPosTarget = 0;
            }
            _stockedMovementAddedPosLerp = Mathf.Lerp(_stockedMovementAddedPosLerp, _stockedMovementAddedPosTarget, dt * _LerpRate);
            _stockedMovementAddedPosSmoothed = Mathf.Lerp(_stockedMovementAddedPosSmoothed, _stockedMovementAddedPosLerp, dt);
            return new Vector3(0, -_stockedMovementAddedPosSmoothed, 0);
        }
    }
}
