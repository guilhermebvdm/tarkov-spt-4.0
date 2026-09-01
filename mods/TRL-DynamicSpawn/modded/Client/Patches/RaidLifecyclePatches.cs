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
                RaidLifecycle.OnWorldDestroyed();   // stops the poller (if still running) + invalidates the cache; logs source (CR-01-03)
            }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] GameWorldOnDestroyPatch: {ex}"); }
        }
    }

    /// <summary>
    /// Early raid-end hook for SPT without Fika (Left/Killed/MIA fire Stop before the scene is torn down).
    /// The 009 patch on the closed generic BaseLocalGame&lt;EftGamePlayerOwner&gt;.Stop was inert in V1: with Fika the
    /// game class is CoopGame, which never calls base.Stop. Patch the concrete overrides instead (PA-01-03 / 010).
    /// Only stops the poller/spawn loops; cache invalidation is left to OnDestroy (PA-01-01).
    /// ref: Assembly-CSharp/EFT/LocalGame.cs:357 (public override void Stop — concrete class :24, calls base :362)
    /// ref: AUD-01-02, AUD-01-06
    /// </summary>
    public class LocalGameStopPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(LocalGame), nameof(LocalGame.Stop));   // single Stop overload on LocalGame

        [PatchPrefix]
        private static void Prefix()
        {
            try { RaidLifecycle.OnRaidEnd("LocalGame.Stop"); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] LocalGameStopPatch: {ex}"); }
        }
    }

    /// <summary>
    /// Same hook for Fika host/headless: CoopGame : BaseLocalGame&lt;EftGamePlayerOwner&gt; (fika-plugin CoopGame.cs:42,
    /// sealed; override Stop :718 does NOT call base.Stop — it ends the raid through ExitManager :811-818).
    /// With Fika installed LocalGame is never instantiated (TarkovApplication_LocalGameCreator_Patch.cs:192 → CoopGame.Create),
    /// so in that setup this is the only early hook that fires; expected V2 log source: "CoopGame.Stop".
    /// Soft dependency: the type is resolved by name; when Fika is absent TargetType is null and Plugin does NOT call
    /// Enable() (ModulePatch throws PatchException on a null target — SPT AbstractPatch.cs:110-113). ref: PA-02-03
    /// </summary>
    public class CoopGameStopPatch : ModulePatch
    {
        public static readonly Type TargetType = AccessTools.TypeByName("Fika.Core.Main.GameMode.CoopGame");

        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(TargetType, "Stop", new[] { typeof(string), typeof(ExitStatus), typeof(string), typeof(float) });

        [PatchPrefix]
        private static void Prefix()
        {
            try { RaidLifecycle.OnRaidEnd("CoopGame.Stop"); }
            catch (Exception ex) { Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] CoopGameStopPatch: {ex}"); }
        }
    }
}
