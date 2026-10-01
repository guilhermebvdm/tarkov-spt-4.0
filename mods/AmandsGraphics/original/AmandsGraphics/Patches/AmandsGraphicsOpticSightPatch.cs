using System.Reflection;
using EFT.CameraControl;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsOpticSightPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(OpticSight).GetMethod(nameof(OpticSight.OnEnable));
    }
    
    [PatchPostfix]
    public static void PatchPostFix(ref OpticSight __instance)
    {
        //AmandsGraphicsClass.opticSight = __instance;
        foreach (Transform transform in __instance.gameObject.transform.GetChildren())
        {
            if (transform.name.Contains("backLens"))
            {
                AmandsGraphicsClass.backLens = transform;
            }
        }

        SightModVisualControllers sightModVisualControllers = __instance.gameObject.GetComponentInParent<SightModVisualControllers>();

        if (sightModVisualControllers != null)
        {
            AmandsGraphicsClass.sightComponent = sightModVisualControllers.SightMod;
        }
    }
}
