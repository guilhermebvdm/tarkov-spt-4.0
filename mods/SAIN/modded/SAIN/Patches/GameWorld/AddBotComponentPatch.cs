using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components.BotController;
using SPT.Reflection.Patching;

namespace SAIN.Patches.GameWorld;

/// <summary>
/// This patch makes sure that whenever a new bot is spawned, that bot is also added in SAIN
/// </summary>
public class AddBotComponentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.PreActivate));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref BotOwner __instance)
    {
        try
        {
            BotSpawnController.Instance.AddBot(__instance);
        }
        catch (Exception ex)
        {
            Logger.LogError($" SAIN Add Bot Error: {ex}");
        }
    }
}
