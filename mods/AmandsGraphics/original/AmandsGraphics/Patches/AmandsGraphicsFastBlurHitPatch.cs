using System.Reflection;
using AmandsGraphics.Enums;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsFastBlurHitPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(FastBlur).GetMethod(nameof(FastBlur.Hit));
    }

    [PatchPostfix]
    private static void PatchPostFix(ref FastBlur __instance, float power)
    {
        if (AmandsGraphicsPlugin.HealthEffectHit.Value == EEnabledFeature.On)
        {
            AmandsGraphicsPlugin.AmandsGraphicsClass.AmandsGraphicsHitEffect(power);
        }
    }
}
