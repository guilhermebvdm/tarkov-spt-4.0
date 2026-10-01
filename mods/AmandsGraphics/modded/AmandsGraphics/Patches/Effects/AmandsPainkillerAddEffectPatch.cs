using System.Reflection;
using AmandsGraphics.Enums;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsGraphics.Patches.Effects;

public sealed class AmandsPainkillerAddEffectPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EffectsController.DesaturateMaskAccumulator).GetMethod(nameof(EffectsController.DesaturateMaskAccumulator.AddEffect));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref EffectsController.DesaturateMaskAccumulator __instance)
    {
        if (AmandsGraphicsPlugin.HealthEffectPainkiller.Value == EEnabledFeature.On)
        {
            if (__instance.ActiveEffects.Count == 0)
            {
                __instance.MaxEffectValue = 0f;
            }
            else
            {
                __instance.MaxEffectValue = Mathf.Min(1.0f * AmandsGraphicsPlugin.PainkillerSaturation.Value, 1f);
            }
        }
    }
}
