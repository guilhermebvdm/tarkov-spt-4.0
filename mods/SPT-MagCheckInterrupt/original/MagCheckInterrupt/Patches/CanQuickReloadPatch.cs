using System.Reflection;
using HarmonyLib;
using MagCheckInterrupt.Components;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt.Patches;

/// <summary>
/// This patch allows the player to quick reload during a MagCheckReloadOperation
/// </summary>
public class CanQuickReloadPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Class1730), nameof(Class1730.Boolean_1));
    }

    [PatchPostfix]
    protected static void Postfix(Class1730 __instance, ref bool __result)
    {
        if (__result) return;

        if (__instance.IfirearmHandsController_0 is FirearmController { CurrentOperation: MagCheckReloadOperation })
        {
            __result = true;
        }
    }
}
