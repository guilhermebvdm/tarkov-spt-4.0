using System.Reflection;
using HarmonyLib;
using MagCheckInterrupt.Components;
using MagCheckInterrupt.Utils;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt.External;

public class UIFixes
{
    public static void Init()
    {
        LoggerUtil.Info("Initializing UI Fixes compatibility");

        new CanExecuteSwapPatch().Enable();
    }
}

#region PATCHES
/// <summary>
/// This allows execution of <see cref="SwapOperationClass"/> during a <see cref="MagCheckReloadOperation"/>.
/// </summary>
[IgnoreAutoPatch]
public class CanExecuteSwapPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(FirearmController), nameof(FirearmController.CanExecute));
    }

    [PatchPostfix]
    public static void Postfix(FirearmController __instance, IInventoryOperation operation, ref bool __result)
    {
        if (__result) return;
        if (__instance.CurrentOperation is not MagCheckReloadOperation) return;
        if (operation is not (SwapOperationClass or RemoveOperation or AttachOperation)) return;

        __result = true;
    }
}
#endregion
