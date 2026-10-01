using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public sealed class AmandsPlayerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player).GetMethod(nameof(Player.Init));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref Player __instance)
    {
        if (__instance != null && __instance.IsYourPlayer)
        {
            AmandsGraphicsClass.Player = __instance;
        }
    }
}
