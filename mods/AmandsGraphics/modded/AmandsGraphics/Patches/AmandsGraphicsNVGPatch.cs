using System.Reflection;
using SPT.Reflection.Patching;
using BSG.CameraEffects;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsNVGPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(NightVision).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(x => x.GetParameters().Count() == 1 && x.GetParameters()[0].Name == "on" && x.Name != "StartSwitch");
    }

    [PatchPostfix]
    public static void PatchPostFix(ref NightVision __instance, bool on)
    {
        if (AmandsGraphicsPlugin.AmandsGraphicsClass.GraphicsMode && AmandsGraphicsClass.Player != null && AmandsGraphicsClass.NVG != on && AmandsGraphicsClass.FPSCameraNightVision != null)
        {
            AmandsGraphicsClass.NVG = on;
            AmandsGraphicsPlugin.AmandsGraphicsClass.UpdateAmandsGraphics();
        }
        AmandsGraphicsClass.NVG = on;
    }
}
