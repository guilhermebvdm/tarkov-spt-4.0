using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Patches
{
    /// <summary>
    /// Raid start hook. Target already patched by DynamicSpawnManagerPatch (independent postfix —
    /// that one returns early for Fika guests and must not be coupled to the poller lifecycle).
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (public virtual void OnGameStarted)
    /// ref: AUD-01-02
    /// </summary>
    public class RaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            try { RaidLifecycle.OnRaidStart(__instance); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] RaidStartPatch: {ex}"); }
        }
    }

    /// <summary>
    /// Primary raid-end hook. ClientGameWorld.OnDestroy (ClientGameWorld.cs:219) calls base.OnDestroy()
    /// (:222), so patching the base fires for every raid world, Fika included.
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (public virtual void OnDestroy)
    /// ref: AUD-01-01, AUD-01-02
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
                Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Raid end hook fired (GameWorld.OnDestroy)."); // 1x/raid — PA-01-05
                RaidLifecycle.OnWorldDestroyed();   // stops the poller (if still running) + invalidates the cache
            }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] GameWorldOnDestroyPatch: {ex}"); }
        }
    }

    /// <summary>
    /// Secondary raid-end hook (Left/Killed/MIA fire Stop before the scene is torn down).
    /// BaseLocalGame is an open generic — resolved through the closed type used by LocalGame
    /// (LocalGame.cs:24, override :357 calls base :362) and by Fika's CoopGame (CoopGame.cs:42).
    /// Only stops the poller; cache invalidation is left to OnDestroy (PA-01-01).
    /// ref: Assembly-CSharp/EFT/BaseLocalGame-1.cs:1018 (public virtual void Stop(string, ExitStatus, string, float))
    /// ref: AUD-01-02
    /// </summary>
    public class BaseLocalGameStopPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BaseLocalGame<EftGamePlayerOwner>), nameof(BaseLocalGame<EftGamePlayerOwner>.Stop));

        [PatchPrefix]
        private static void Prefix()
        {
            try
            {
                Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Raid stop hook fired (BaseLocalGame.Stop)."); // 1x/raid — PA-01-05
                RaidLifecycle.OnRaidEnd();
            }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] BaseLocalGameStopPatch: {ex}"); }
        }
    }
}
