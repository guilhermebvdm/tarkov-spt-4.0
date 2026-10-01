using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine.Rendering.PostProcessing;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsFastBlurPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(FastBlur).GetMethod(nameof(FastBlur.Start));
    }

    [PatchPostfix]
    private static void PatchPostFix(ref FastBlur __instance)
    {
        if (__instance.gameObject.name == "FPS Camera")
        {
            AmandsGraphicsClass.fastBlur = __instance;
            PostProcessLayer FPSCameraPostProcessLayer = __instance.gameObject.GetComponent<PostProcessLayer>();
            if (FPSCameraPostProcessLayer != null)
            {
                AmandsGraphicsClass.FPSCameraChromaticAberration = Traverse.Create(FPSCameraPostProcessLayer).Field("m_Bundles").GetValue<Dictionary<Type, PostProcessBundle>>()[typeof(UnityEngine.Rendering.PostProcessing.ChromaticAberration)].settings as UnityEngine.Rendering.PostProcessing.ChromaticAberration;
                AmandsGraphicsClass.FPSCameraChromaticAberration.enabled.value = false;
            }
        }
    }
}
