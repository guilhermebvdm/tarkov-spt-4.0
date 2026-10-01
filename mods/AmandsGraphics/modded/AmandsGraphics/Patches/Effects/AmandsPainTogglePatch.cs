using System.Reflection;
using AmandsGraphics.Enums;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches.Effects;

public sealed class AmandsPainTogglePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EffectsController.CC_RadialBlurAccumulator).GetMethod(nameof(EffectsController.CC_RadialBlurAccumulator.Toggle));
    }

    [PatchPrefix]
    public static bool PatchPreFix(ref object __instance, ref bool value)
    {
        if (AmandsGraphicsPlugin.HealthEffectPain.Value == EEnabledFeature.On)
        {
            value = false;
        }

        return true;
    }
}
