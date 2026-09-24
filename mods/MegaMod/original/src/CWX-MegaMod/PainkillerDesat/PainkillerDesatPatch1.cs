using System;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using HarmonyLib;

namespace CWX_MegaMod.PainkillerDesat
{
    public class PainkillerDesatScript1 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EffectsController.Class640), nameof(EffectsController.Class640.method_2));
        }

        [PatchPrefix] // removes the sharpen/desat effect from some painkillers
        public static bool PatchPrefix(ref CC_Sharpen ___Cc_Sharpen_0)
        {
            if (!MegaMod.PainkillerDesat.Value)
            {
                return true;
            }

            if (___Cc_Sharpen_0 != null)
            {
                ___Cc_Sharpen_0.MaskDesaturate = 0f;
                ___Cc_Sharpen_0.Radius = 1f;
                ___Cc_Sharpen_0.RadiusFalloff = 0.425f;

                if (___Cc_Sharpen_0.DesaturateEffectSettingsProvider != null)
                {
                    ___Cc_Sharpen_0.DesaturateEffectSettingsProvider.MaskDesaturate = 0f;
                    ___Cc_Sharpen_0.DesaturateEffectSettingsProvider.Radius = 1f;
                    ___Cc_Sharpen_0.DesaturateEffectSettingsProvider.RadiusFalloff = 0.425f;
                }
            }

            return false; // dont do method
        }
    }
}