using EFT;
using TRLDynamicSpawn.Components;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>
    /// Idempotent raid start/stop orchestration (AP-01). Owns the raid-scoped lifecycle of the
    /// despawn poller and the server-config cache.
    /// ref: AUD-01-01 (cache invalidation per raid) · ref: AUD-01-02 (poller only alive in raid)
    /// </summary>
    public static class RaidLifecycle
    {
        private static bool _raidActive;

        // ref: Assembly-CSharp/EFT/GameWorld.cs:2584 (OnGameStarted postfix)
        public static void OnRaidStart(GameWorld gameWorld)
        {
            if (gameWorld == null || gameWorld.MainPlayer == null) return;
            if (gameWorld.MainPlayer is HideoutPlayer) return;   // hideout is not a raid
            if (FikaHelper.IsClient()) return;                   // guest: no spawn/despawn work at all (PA-01-03)
            if (_raidActive) return;                             // Fika may re-enter OnGameStarted
            _raidActive = true;
            BotDespawnManager.StartLoop();
        }

        // ref: Assembly-CSharp/EFT/BaseLocalGame-1.cs:1018 (Stop) and EFT/GameWorld.cs:2111 (OnDestroy)
        // Only stops the poller. Does NOT invalidate the cache: after Stop the world is still alive
        // (delay / extraction screen) and a reader would re-fetch during teardown. ref: PA-01-01
        public static void OnRaidEnd()
        {
            if (!_raidActive) return;                            // second hook = no-op
            _raidActive = false;
            BotDespawnManager.StopLoop();
        }

        // ref: Assembly-CSharp/EFT/GameWorld.cs:2111 (OnDestroy) — last raid event; after it there is
        // no Config reader until the next raid. Single point of automatic cache invalidation.
        public static void OnWorldDestroyed()
        {
            OnRaidEnd();                                         // covers the case where Stop did not fire (PA-01-05)
            ServerConfigProvider.ForceRefresh();                 // next raid fetches a fresh config (idempotent)
        }
    }
}
