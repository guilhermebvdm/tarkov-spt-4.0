using System.Reflection;
using EFT;
using HarmonyLib;
using MOAR.Components;
using SPT.Reflection.Patching;

namespace MOAR.Patches;

public class OnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPrefix]
    private static void PatchPrefix(GameWorld __instance)
    {
        __instance.GetOrAddComponent<BotZoneRenderer>();
    }
}

public class OnGameStartedPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotZone), nameof(BotZone.Awake));
    }

    [PatchPrefix]
    private static void PatchPrefix(GameWorld __instance)
    {
        __instance.GetOrAddComponent<BotZoneRenderer>();
    }
}
