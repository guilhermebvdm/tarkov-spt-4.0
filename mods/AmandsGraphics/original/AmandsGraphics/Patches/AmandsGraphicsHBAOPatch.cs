using System.Reflection;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsHBAOPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(HBAO_Core).GetMethod(nameof(HBAO_Core.ApplyPreset));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref HBAO_Core __instance, HBAO_Core.Preset preset)
    {
        AmandsGraphicsClass.defaultFPSCameraHBAOAOSettings = __instance.aoSettings;
        AmandsGraphicsClass.defaultFPSCameraHBAOColorBleedingSettings = __instance.colorBleedingSettings;
        AmandsGraphicsClass.FPSCameraHBAOAOSettings = __instance.aoSettings;
        AmandsGraphicsClass.FPSCameraHBAOColorBleedingSettings = __instance.colorBleedingSettings;
    }
}
