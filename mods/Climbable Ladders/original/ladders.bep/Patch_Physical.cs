using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

using Physical = PlayerPhysicalClass;

namespace tarkin.ladders.bep
{
    internal class Patch_Physical_CanClimb : ModulePatch
    {
        public static bool OverrideCanClimb;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Physical), nameof(Physical.CanClimb));
        }

        [PatchPostfix]
        private static void PatchPostfix(Physical __instance, ref bool __result)
        {
            if (!__result)
                __result = OverrideCanClimb;
        }
    }

    internal class Patch_Physical_CanVault : ModulePatch
    {
        public static bool OverrideCanVault;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Physical), nameof(Physical.CanVault));
        }

        [PatchPostfix]
        private static void PatchPostfix(Physical __instance, ref bool __result)
        {
            if (!__result)
                __result = OverrideCanVault;
        }
    }
}
