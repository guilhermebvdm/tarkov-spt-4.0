using RealismMod.Health;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RealismMod.VisualEffects
{
    //grab reference for EFT's player camera visual effects class for things like tunnel vision
    public class PrismEffectsEnablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(PrismEffects).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostFix(PrismEffects __instance)
        {
            if (__instance.gameObject.name == "FPS Camera")
            {
                if (ScreenEffectsController.PrismEffects == __instance)
                {
                    return;
                }
                ScreenEffectsController.PrismEffects = __instance;
            }
        }
    }

    //properly depose of PrismEffects reference
    public class PrismEffectsDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(PrismEffects).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostFix(PrismEffects __instance)
        {
            if (__instance.gameObject.name == "FPS Camera" && ScreenEffectsController.PrismEffects == __instance)
            {
                ScreenEffectsController.PrismEffects = null;
            }
        }
    }
}
