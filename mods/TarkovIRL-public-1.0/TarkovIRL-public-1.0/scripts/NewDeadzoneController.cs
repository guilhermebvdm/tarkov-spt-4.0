using RealismMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TarkovIRL
{
    internal class NewDeadzoneController
    {
        static float _rotDeltaHistory = 0;
        static float _rotDeltaSmoothed = 0;
        static float _rotDeltaSmoothedInDeltaTime = 0;

        public static void Update(float fdt)
        {
            float rotDelta = PlayerMotionController.HorizontalRotationDelta * 100f;
            //UtilsTIRL.Log($"rot delta {rotDelta}");

            _rotDeltaHistory += rotDelta;
            _rotDeltaHistory -= _rotDeltaSmoothed;
            _rotDeltaSmoothed = _rotDeltaHistory * fdt * 9f;

            float deadzoneMulti = PrimeMover.WeaponDeadzoneMulti.Value * WeaponController.GetWeaponMulti(false);
            if (WeaponController.HasCheekWeld() && PlayerMotionController.IsAiming)
            {
                deadzoneMulti *= PrimeMover.DeadzoneInADS.Value;
            }
            else if (StanceController.CurrentStance == EStance.ShortStock)
            {
                deadzoneMulti *= PrimeMover.DeadzoneInShortStock.Value;
            }
            else if (StanceController.CurrentStance == EStance.ActiveAiming)
            {
                deadzoneMulti *= PrimeMover.DeadzoneInActiveAim.Value;
            }
            else if (StanceController.CurrentStance == EStance.None)
            {
                deadzoneMulti *= PrimeMover.DeadzoneInVanilla.Value;
            }
            else if (StanceController.CurrentStance == EStance.HighReady)
            {
                deadzoneMulti *= PrimeMover.DeadzoneInHighReady.Value;
            }
            else if (StanceController.CurrentStance == EStance.LowReady)
            {
                deadzoneMulti *= PrimeMover.DeadzoneInLowReady.Value;
            }

            float efficiencyWeight = PrimeMover.DeadzoneWeightForEfficiency.Value ? EfficiencyController.EfficiencyModifierInverse : 1f;
            _rotDeltaSmoothedInDeltaTime = Mathf.Lerp(_rotDeltaSmoothedInDeltaTime, _rotDeltaSmoothed * deadzoneMulti, fdt * PrimeMover.DeadzoneHeadFollowSpeedMulti.Value * efficiencyWeight);
        }

        public static Vector3 GetHeadRotWithDeadzone(Vector3 headRotInitial) 
        {

            Vector3 headRotFinal = headRotInitial;
            headRotFinal.y += _rotDeltaSmoothedInDeltaTime;
            return headRotFinal;

        }
    }
}
