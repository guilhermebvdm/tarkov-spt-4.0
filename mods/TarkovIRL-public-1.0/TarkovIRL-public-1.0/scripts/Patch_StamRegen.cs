using EFT;
using HarmonyLib;
using RealismMod;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TarkovIRL
{
    public class StaminaRegenRatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(PlayerPhysicalClass).GetMethod("method_21", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(PlayerPhysicalClass __instance, ref float __result)
        {
            float extraDrainFromBreath = PlayerMotionController.IsAugmentedBreath ? -4f : 0;
            __result += extraDrainFromBreath;
            return;
        }
    }
}
