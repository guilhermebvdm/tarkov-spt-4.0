using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace MOAR.Patches;

internal class BotSpawnLoggerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.Create));
    }

    [PatchPostfix]
    private static void PatchPostfix(Player player)
    {
        if (player == null) return;
        
        // Se for um player humano, ignoramos
        if (!player.IsAI) return;
        
        string role = player.Profile?.Info?.Settings?.Role.ToString() ?? "UnknownRole";
        string nickname = player.Profile?.Nickname ?? "UnknownName";
        
        Plugin.LogSource.LogWarning($"[MOAR Logger] SPAWN -> Role: {role} | Name: {nickname}");
    }
}
