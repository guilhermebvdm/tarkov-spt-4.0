using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TRLDynamicSpawn.Patches;

internal class BotSpawnLoggerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.Create));
    }

    [PatchPostfix]
    private static void PatchPostfix(Player player)
    {
        // ref: AUD-01-07 — gate BEFORE any formatting; Info instead of Warning (diagnostic, not a problem)
        if (!TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value) return;
        if (player == null || !player.IsAI) return;

        string role = player.Profile?.Info?.Settings?.Role.ToString() ?? "UnknownRole";
        string nickname = player.Profile?.Nickname ?? "UnknownName";
        Plugin.LogSource.LogInfo($"[TRLDynamicSpawn Logger] SPAWN -> Role: {role} | Name: {nickname}");
    }
}
