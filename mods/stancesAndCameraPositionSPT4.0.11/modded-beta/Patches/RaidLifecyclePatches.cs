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
    /// backlog 002 F5: também enfileira a stance inicial (Stance 3 - Low Ready) quando o toggle estiver on.
    /// </summary>
    public class GameWorldOnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix()
        {
            try
            {
                Plugin.ApplyMovementSpeeds();
                StanceManager.OnRaidStart();
                HoldBreathPatch.OnRaidStart();   // carrega o áudio no contexto da cena + reseta estado do hold

                // Item 010: estado limpo a cada raid + marcar que o próximo equip é o spawn (cenário 1).
                ManualChamberingState.Reset();
                ManualChamberingState.JustSpawned = true;

                // backlog 002 F5 + 06-fix-01 — iniciar em Stance 2 - Low Ready quando habilitado.
                // Após swap (06-fix-01), Stance 2 passou a ser Low Ready (era Stance 3).
                // A aplicação efetiva acontece no primeiro Update em que PWA.HandsContainer estiver pronto.
                if (Plugin._StartInLowReadyOnRaidBegin?.Value == true)
                    StanceManager.QueueInitialStance(Stance.Stance2);
            }
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
            try { StanceManager.OnRaidEnd(); HoldBreathPatch.OnRaidEnd(); ManualChamberingState.Reset(); }
            catch (Exception ex) { Plugin.Logger.LogError($"[GameWorldOnDestroyPatch] {ex}"); }
        }
    }

    // Nota: BaseLocalGame<TPlayerOwner> é genérico open generic — Harmony não patcheia diretamente.
    // GameWorld.OnDestroy já cobre todos os caminhos de saída (Left/Killed/MIA) na prática,
    // então removemos o patch redundante. _raidEnded é idempotente — segurança preservada.
}
