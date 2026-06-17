using SPT.Reflection.Patching;
using EFT.Animations;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Comfort.Common;
using EFT;

namespace CameraRotationMod.Patches
{
    public class PlayerSpringPatch : ModulePatch
    {
        private static FieldInfo _cameraOffsetField;
        
        protected override MethodBase GetTargetMethod()
        {
            // Get the field reference for CameraOffset
            _cameraOffsetField = AccessTools.Field(typeof(PlayerSpring), nameof(PlayerSpring.CameraOffset));
            
            // Patch PlayerSpring.Start - simple and works
            return AccessTools.Method(typeof(PlayerSpring), nameof(PlayerSpring.Start));
        }

        [PatchPostfix]
        private static void PatchPostfix(PlayerSpring __instance)
        {
            if (__instance == null)
                return;
                
            bool isEnabled = Plugin._PositionEnabled?.Value ?? false;
            
            // Calculate target offset
            Vector3 targetOffset = isEnabled ? new Vector3(
                Plugin._SidewaysOffset.Value,
                Plugin._UpDownOffset.Value,
                Plugin._ForwardBackwardOffset.Value
            ) : new Vector3(0.04f, 0.04f, 0.04f); // Default game value
            
            // Directly set the CameraOffset field
            __instance.CameraOffset = targetOffset;
        }
    }
}
