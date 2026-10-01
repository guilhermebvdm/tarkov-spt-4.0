using System.Reflection;
using HarmonyLib;
using Orbit.Systems;
using SPT.Reflection.Patching;

namespace Orbit.Patches;

public class NativeGhostDecisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(AICoreStrategyAbstractClass<BotLogicDecision>), "Update");

    [PatchPostfix]
    public static void Postfix(AICoreStrategyAbstractClass<BotLogicDecision> __instance,
        ref AICoreActionResultStruct<BotLogicDecision, CoreActionResultParams>? __result)
    {
        NativeGhostSystem.RefreshPeacefulDecision(__instance, ref __result);
        NativeGhostSystem.GuardDecision(__instance, ref __result);
    }
}
