using System.Reflection;
using AmandsGraphics.Enums;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public class AmandsGraphicsOnPlayerDamagedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EffectsController).GetMethod(nameof(EffectsController.OnPlayerDamaged));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref EffectsController __instance)
    {
        if (AmandsGraphicsClass.fastBlur != null && AmandsGraphicsPlugin.HealthEffectHit.Value == EEnabledFeature.On)
        {
            AmandsGraphicsClass.fastBlur.enabled = false;
        }
    }
}
