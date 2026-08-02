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
        protected override MethodBase GetTargetMethod()
        {
            // Item 020 (F2): aqui se resolvia `_cameraOffsetField` por reflection e o campo NUNCA era lido —
            // o Postfix escreve `__instance.CameraOffset` direto, que é público. Removido.
            //
            // `PlayerSpring.Start` roda uma vez por instância do rig de mãos, ou seja, uma vez por raid — é o
            // que garante que o offset valha em TODA raid. (O cache `Plugin._cameraOffsetDirty` serve só para
            // refletir mudanças do F12 ao vivo; nada no EFT reescreve `CameraOffset`.)
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
