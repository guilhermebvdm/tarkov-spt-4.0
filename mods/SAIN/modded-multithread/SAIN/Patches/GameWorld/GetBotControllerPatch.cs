using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;

namespace SAIN.Patches.GameWorld;

/// <summary>
/// This patch makes sure that when the <see cref="BotsController">BotsController</see> is activated, it also activates in SAIN's <see cref="GameWorldComponent">GameWorldComponent</see>
/// </summary>
public class GetBotControllerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsController), nameof(BotsController.Init));
    }

    [PatchPostfix]
    public static void Patch(BotsController __instance)
    {
        GameWorldComponent gameWorld = GameWorldComponent.Instance;
        if (gameWorld == null)
        {
            Logger.LogError("gameWorld Null");
            return;
        }
        gameWorld.Activate(__instance);
    }
}
