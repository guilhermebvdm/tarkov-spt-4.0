using System.Reflection;
using SPT.Reflection.Patching;
using BSG.CameraEffects;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsApplyNVGPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(NightVision).GetMethod(nameof(NightVision.StartSwitch));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref NightVision __instance)
    {
        if (AmandsGraphicsPlugin.AmandsGraphicsClass.GraphicsMode && AmandsGraphicsClass.Player != null)
        {
            AmandsGraphicsClass.defaultNightVisionNoiseIntensity = __instance.NoiseIntensity;
            switch (AmandsGraphicsClass.scene)
            {
                case "Shopping_Mall_Terrain":
                    __instance.NoiseIntensity = AmandsGraphicsClass.defaultNightVisionNoiseIntensity * AmandsGraphicsPlugin.InterchangeNVGNoiseIntensity.Value;
                    break;
                default:
                    __instance.NoiseIntensity = AmandsGraphicsClass.defaultNightVisionNoiseIntensity * AmandsGraphicsPlugin.NVGNoiseIntensity.Value;
                    break;
            }
            __instance.ApplySettings();
        }
    }
}
