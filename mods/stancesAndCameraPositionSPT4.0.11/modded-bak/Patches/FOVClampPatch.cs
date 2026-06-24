using SPT.Reflection.Patching;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Patch to override the FOV value clamping in GClass1085.Class1841.method_0.
    /// This allows FOV values outside the default 50-75 range to be applied.
    /// </summary>
    public class FOVClampPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Target the nested class method that clamps FOV values
            return AccessTools.Method(typeof(GClass1085.Class1841), nameof(GClass1085.Class1841.method_0));
        }

        [PatchPostfix]
        private static void PatchPostfix(int x, ref int __result)
        {
            if (!Plugin._FOVExpandEnabled.Value)
                return;

            // Override the clamping with extended range
            __result = Mathf.Clamp(x, Plugin._FOVMinRange.Value, Plugin._FOVMaxRange.Value);
        }
    }
}
