using SPT.Reflection.Patching;
using System.Reflection;
using EFT.Animations;
using UnityEngine;
using HarmonyLib;
using CameraRotationMod.Patches;

namespace CameraRotationMod
{
    public class LerpCameraPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.LerpCamera));
        }

        [PatchPostfix]
        public static void PatchPostfix(ProceduralWeaponAnimation __instance)
        {
            if (Camera.main == null) return;

            float bobbingMult = Plugin._CameraBobbingMultiplier?.Value ?? 1f;
            if (bobbingMult <= 0.01f) return;

            Vector3 cameraWiggle = SpringGetPatch.CurrentCurveRotation * bobbingMult;
            if (cameraWiggle == Vector3.zero) return;

            // Aplica a rotação por cima do que o LerpCamera do EFT acabou de calcular!
            Camera.main.transform.localRotation *= Quaternion.Euler(cameraWiggle);
        }
    }
}
