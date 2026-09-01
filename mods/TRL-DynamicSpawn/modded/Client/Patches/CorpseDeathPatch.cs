using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLDynamicSpawn.Components;

namespace TRLDynamicSpawn.Patches
{
    public class CorpseDeathPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnDead));
        }

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            try
            {
                if (__instance == null || __instance.IsYourPlayer || !__instance.IsAI) return;
                CorpseCleanupManager.RegisterDeadBot(__instance);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] CorpseDeathPatch Error: {ex.Message}");
            }
        }
    }
}
