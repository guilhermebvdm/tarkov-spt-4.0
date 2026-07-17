using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TarkovIRL;

public class Patch_GameWorldOnDestroy : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GameWorld).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix()
    {
        if (TIRLUtils.Logger != null)
        {
            TIRLUtils.Logger.LogInfo("GameWorld Destroyed. Cleaning up static states.");
        }
        FreeAimController.Reset();
        EfficiencyController.Reset();
        NewSwayController.Reset();
        ThrowController.Reset();
        WeaponSelectionController.Reset();
    }
}
