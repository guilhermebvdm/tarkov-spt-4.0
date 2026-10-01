using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;

namespace SAIN.Patches.GameWorld;

/// <summary>
/// This patch makes sure that whenever a new bot is activated, that bot is also activated in SAIN
/// </summary>
public class ActivateBotComponentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.method_10));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref BotOwner __instance)
    {
        var component = __instance.GetComponent<BotComponent>();

        if (component != null)
        {
            component.Activate(__instance);
        }
    }
}
