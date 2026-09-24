using System;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;

namespace CWX_MegaMod.PainkillerDesat
{
    public class PainkillerDesatScript3 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EffectsController.Class645), nameof(EffectsController.Class645.Toggle));
        }

        [PatchPrefix] // removes the wiggle effect from some painkillers
        public static bool PatchPrefix(ref CC_Wiggle ___Cc_Wiggle_0)
        {
            if (!MegaMod.PainkillerDesat.Value)
            {
                return true;
            }

            if (___Cc_Wiggle_0 != null)
            {
                ___Cc_Wiggle_0.enabled = false;
            }

            return false; // dont do method
        }
    }
}