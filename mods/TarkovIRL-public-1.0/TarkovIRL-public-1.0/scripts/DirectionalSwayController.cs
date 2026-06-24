using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TarkovIRL
{
    internal class DirectionalSwayController
    {
        static float _lateralPosLerp = 0;
        static float _projectedPosLerp = 0;

        static float _lateralRotLerp = 0;
        static float _verticalRotLerp = 0;

        public static void UpdateDirectionalSwayLerp(float dt)
        {
            float lateralPosOffset = PrimeMover.DirectionalSwayLateralPosValue.Value;
            float projectedPosOffset = PrimeMover.DirectionalSwayProjectedPosValue.Value;
            float lateralRotOffset = PrimeMover.DirectionalSwayLateralRotValue.Value;
            float verticalRotOffset = PrimeMover.DirectionalSwayVerticalRotValue.Value;

            PlayerMotionController.EPlayerDir dir = PlayerMotionController.Direction;
            float dirSwayLateralValue = 0;
            float dirSwayVerticalValue = 0;

            if (dir == PlayerMotionController.EPlayerDir.FWD || dir == PlayerMotionController.EPlayerDir.BWD) dirSwayLateralValue = 0;
            else if (dir == PlayerMotionController.EPlayerDir.LEFT) dirSwayLateralValue = -1f;
            else if (dir == PlayerMotionController.EPlayerDir.FWDLEFT) dirSwayLateralValue = -0.5f;
            else if (dir == PlayerMotionController.EPlayerDir.BWDLEFT) dirSwayLateralValue = 0.5f;
            else if (dir == PlayerMotionController.EPlayerDir.RIGHT) dirSwayLateralValue = 1f;
            else if (dir == PlayerMotionController.EPlayerDir.FWDRIGHT) dirSwayLateralValue = 0.5f;
            else if (dir == PlayerMotionController.EPlayerDir.BWDRIGHT) dirSwayLateralValue = -0.5f;
            else if (dir == PlayerMotionController.EPlayerDir.NONE) dirSwayLateralValue = 0f;

            if (dir == PlayerMotionController.EPlayerDir.FWD) dirSwayVerticalValue = 1f;
            else if (dir == PlayerMotionController.EPlayerDir.FWDLEFT) dirSwayVerticalValue = 0.5f;
            else if (dir == PlayerMotionController.EPlayerDir.FWDRIGHT) dirSwayVerticalValue = 0.5f;

            else if (dir == PlayerMotionController.EPlayerDir.BWD) dirSwayVerticalValue = -1f;
            else if (dir == PlayerMotionController.EPlayerDir.BWDLEFT) dirSwayVerticalValue = -0.5f;
            else if (dir == PlayerMotionController.EPlayerDir.BWDRIGHT) dirSwayVerticalValue = -0.5f;

            float speedAlpha = Mathf.Clamp(PlayerMotionController.GetNormalSpeed(), 0.1f, 1f);
            float adsAlpha = PlayerMotionController.IsAiming ? PrimeMover.DirectionalSwayLerpOnAds.Value : 1f;
            dirSwayLateralValue *= speedAlpha * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * adsAlpha;
            dirSwayVerticalValue *= speedAlpha * WeaponController.GetWeaponMulti(false) * EfficiencyController.EfficiencyModifier * adsAlpha;

            float lateralPosTarget = lateralPosOffset * dirSwayLateralValue * PrimeMover.DirectionalSwayMulti.Value;
            float projectedPosTarget = projectedPosOffset * dirSwayLateralValue * PrimeMover.DirectionalSwayMulti.Value;

            float lateralRotTarget = lateralRotOffset * dirSwayLateralValue * PrimeMover.DirectionalSwayMulti.Value;
            float verticalRotTarget = verticalRotOffset * dirSwayVerticalValue * PrimeMover.DirectionalSwayMulti.Value;

            if (!WeaponController.HasCheekWeld())
            {
                lateralPosTarget = 0;
                lateralRotTarget = 0;
                projectedPosTarget = 0;
                verticalRotTarget = 0;
            }

            float speedDTmulti = Mathf.Clamp(PlayerMotionController.GetNormalSpeed(), 0.5f, 1f);
            float finalDT = dt * PrimeMover.DirectionalSwayLerpSpeed.Value * speedDTmulti;

            _lateralPosLerp = Mathf.Lerp(_lateralPosLerp, lateralPosTarget, finalDT);
            _projectedPosLerp = Mathf.Lerp(_projectedPosLerp, projectedPosTarget, finalDT);

            _lateralRotLerp = Mathf.Lerp(_lateralRotLerp, lateralRotTarget, finalDT);
            _verticalRotLerp = Mathf.Lerp(_verticalRotLerp, verticalRotTarget, finalDT);
        }

        public static void GetDirectionalSway(out Vector3 position, out Quaternion rotation)
        {
            if (!PrimeMover.IsDirectionalSway.Value)
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return;
            }

            position = Vector3.zero;
            position.x = _lateralPosLerp;
            position.z = _projectedPosLerp;

            rotation = Quaternion.identity;
            rotation.z = _lateralRotLerp;
            rotation.x = _verticalRotLerp;
        }
    }
}
