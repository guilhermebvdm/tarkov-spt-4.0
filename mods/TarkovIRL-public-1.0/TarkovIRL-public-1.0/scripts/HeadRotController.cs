using EFT;
using UnityEngine;

namespace TarkovIRL
{
    internal class HeadRotController
    {
        static Vector3 _headRotLerp = Vector3.zero;
        static Vector3 _headRotLerpTarget = Vector3.zero;
        static readonly float _HeadRotLerpSpeed = 7f;

        public static void UpdateLerp(float dt)
        {
            _headRotLerp = Vector3.Lerp(_headRotLerp, _headRotLerpTarget, dt * _HeadRotLerpSpeed);
        }

        public static Vector3 GetHeadRotThisFrame(Vector3 headRotThisFrame)
        {
            Vector3 newHeadRot = headRotThisFrame;
            if (ThrowController.IsThrowing)
            {
                _headRotLerpTarget = ThrowController.GetThrowOffset;
            }
            else
            {
                _headRotLerpTarget = new Vector3(0, 0, PlayerMotionController.LeanNormal * PrimeMover.LeanCounterRotateMod.Value);
            }

            Vector3 headRotForAugmentedReload = AugmentedReloadController.GetAugmentedReloadHeadOffset();
            // add other head movs
            _headRotLerpTarget += headRotForAugmentedReload;

            if (PrimeMover.IsHeadTiltADS.Value)
            {
                ParallaxAdsController.GetParallaxADSHeadTilt(out float addedY, out float addedZ);
                _headRotLerpTarget.y += addedY;
                _headRotLerpTarget.z += addedZ;
            }

            /*
            _headRotLerpTarget.x += PrimeMover.DebugHeadRotX.Value;
            _headRotLerpTarget.y += PrimeMover.DebugHeadRotY.Value;
            _headRotLerpTarget.z += PrimeMover.DebugHeadRotZ.Value;
            */

            newHeadRot += _headRotLerp;
            return newHeadRot;
        }
    }
}
