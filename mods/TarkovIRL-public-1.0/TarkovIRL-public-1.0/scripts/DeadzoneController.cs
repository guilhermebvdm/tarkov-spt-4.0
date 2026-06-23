using UnityEngine;
using EFT;
using EFT.Animations;
using EFT.NextObservedPlayer;

namespace TarkovIRL
{
    internal static class DeadzoneController
    {
        static readonly float _LerpRate = 10f;
        static readonly float _RotDeltaThresh = 0.0002f;

        static float _deadZoneLerp = 0;
        static bool _updateDZ = true;

        public static bool DeadzoneUpdatedThisFrame = false;

        static float ProcessHeadDelta(float rawHeadDelta)
        {
            float adjustedHeadDelta = rawHeadDelta / WeaponController.CurrentWeaponErgoNorm / 10f;
            return adjustedHeadDelta * WeaponController.CurrentWeaponWeight * 0.1f;
        }

        public static Vector3 GetHeadRotationWithDeadzone(Player player, float deadzoneSetting, Vector3 headRotInitial)
        {
            // depro
            return Vector3.zero;
        }
        
    }
}
