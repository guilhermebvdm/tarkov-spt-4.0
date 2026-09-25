using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Core;
using UnityEngine;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Hook de início de raid. Anexa o PerformanceManager ao GameWorld.
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (OnGameStarted)
    /// </summary>
    public class RaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            try
            {
                if (__instance == null || __instance.MainPlayer == null)
                {
                    return;
                }

                var manager = __instance.gameObject.GetComponent<PerformanceManager>();
                if (manager == null)
                {
                    manager = __instance.gameObject.AddComponent<PerformanceManager>();
                    manager.Initialize(__instance.MainPlayer);
                    Plugin.LogSource?.LogInfo("[TRL-CoreSight] PerformanceManager inicializado com sucesso para a raid.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro no RaidStartPatch: {ex}");
            }
        }
    }

    /// <summary>
    /// Hook de término de raid. Garante cleanup idempotente de memória e restauração de settings.
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (OnDestroy)
    /// </summary>
    public class GameWorldOnDestroyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));

        [PatchPrefix]
        private static void Prefix()
        {
            try
            {
                PerformanceManager.Instance?.Cleanup();
                Plugin.LogSource?.LogInfo("[TRL-CoreSight] Cleanup de raid concluído.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-CoreSight] Erro no GameWorldOnDestroyPatch: {ex}");
            }
        }
    }
}
