using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Postfix em GameWorld.OnGameStarted — dispara StanceManager.OnRaidStart().
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (public virtual)
    /// </summary>
    public class GameWorldOnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix()
        {
            try { StanceManager.OnRaidStart(); }
            catch (Exception ex) { Plugin.Logger.LogError($"[GameWorldOnGameStartedPatch] {ex}"); }
        }
    }

    /// <summary>
    /// Postfix em GameWorld.OnDestroy — dispara StanceManager.OnRaidEnd() (idempotente).
    /// ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (public virtual)
    /// </summary>
    public class GameWorldOnDestroyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));

        [PatchPostfix]
        private static void Postfix()
        {
            try { StanceManager.OnRaidEnd(); }
            catch (Exception ex) { Plugin.Logger.LogError($"[GameWorldOnDestroyPatch] {ex}"); }
        }
    }

    // Nota: BaseLocalGame<TPlayerOwner> é genérico open generic — Harmony não patcheia diretamente.
    // GameWorld.OnDestroy já cobre todos os caminhos de saída (Left/Killed/MIA) na prática,
    // então removemos o patch redundante. _raidEnded é idempotente — segurança preservada.
}
