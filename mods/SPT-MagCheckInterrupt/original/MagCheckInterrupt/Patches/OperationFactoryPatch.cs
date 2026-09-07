using System;
using System.Collections.Generic;
using System.Reflection;
using MagCheckInterrupt.Components;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt.Patches;

public class OperationFactoryPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(FirearmController).GetMethod(nameof(FirearmController.GetOperationFactoryDelegates));
    }

    [PatchPostfix]
    public static void Postfix(FirearmController __instance, Dictionary<Type, ItemHandsController.OperationFactoryDelegate> __result)
    {
        __result.TryAdd(typeof(MagCheckReloadOperation), () => new MagCheckReloadOperation(__instance));
        __result.TryAdd(typeof(SwapReloadOperation), () => new SwapReloadOperation(__instance));
    }
}
