using System.Reflection;
using AmandsGraphics.Enums;
using EFT.CameraControl;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsCameraManagerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(CameraManager).GetMethod(nameof(CameraManager.Blur));
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref CameraManager __instance, bool isActive, float time)
    {
        AmandsGraphicsClass.CameraClassBlur = isActive;

        if (!isActive && __instance.IsActive)
        {
            return true;
        }

        return AmandsGraphicsPlugin.UIDepthOfField.Value == EUIDepthOfField.Off;
    }
}
