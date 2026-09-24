using System.Reflection;
using EFT.CameraControl;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsOpticPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(OpticComponentUpdater).GetMethod(nameof(OpticComponentUpdater.Awake));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref OpticComponentUpdater __instance)
    {
        if (__instance.gameObject.name == "BaseOpticCamera(Clone)")
        {
            AmandsGraphicsPlugin.AmandsGraphicsClass.ActivateAmandsOpticDepthOfField(__instance.gameObject);
            AmandsGraphicsClass.OpticCameraCamera = __instance.GetComponent<Camera>();
            AmandsGraphicsClass.OpticCameraThermalVision = __instance.GetComponent<ThermalVision>();
        }
    }
}
