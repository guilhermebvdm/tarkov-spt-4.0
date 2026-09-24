using System.Reflection;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsPrismEffectsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(PrismEffects).GetMethod(nameof(PrismEffects.OnEnable));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref PrismEffects __instance)
    {
        if (__instance.gameObject.name == "FPS Camera")
        {
            AmandsGraphicsPlugin.AmandsGraphicsClass.GraphicsMode = false;
            OnEnableAsync(__instance);
        }
    }

    private static async void OnEnableAsync(PrismEffects instance)
    {
        await Task.Delay(100);
        AmandsGraphicsPlugin.AmandsGraphicsClass.ActivateAmandsGraphics(instance.gameObject, instance);
    }
}
