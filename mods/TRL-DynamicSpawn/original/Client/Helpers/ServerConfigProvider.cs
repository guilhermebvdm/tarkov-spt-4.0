using System;
using Newtonsoft.Json;
using SPT.Common.Http;
using TRLDynamicSpawn.Models;
using UnityEngine;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>
    /// Server config with raid scope: one fetch per raid (fetch-on-miss, no TTL).
    /// Invalidation happens only through ForceRefresh() — called by the raid-end hook
    /// (RaidLifecycle.OnWorldDestroyed) and by the F12 "Reload Server Config" toggle.
    /// ref: AUD-01-01 (5 s TTL removed — was 111 blocking HTTP calls per raid)
    /// ref: AUD-01-03 (failed attempts now respect a minimum retry interval)
    /// </summary>
    public static class ServerConfigProvider
    {
        private const string ConfigRoute = "/trldynamicspawn/getConfig";
        private const float FailedFetchRetrySeconds = 30f;   // ref: AUD-01-03

        private static TRLConfig _cachedConfig;
        private static string _cachedJson;
        private static float _lastAttemptTime = -1000f;      // advances on success AND on failure

        /// <summary>
        /// Raw JSON of the last successful response, for consumers that need a private copy
        /// (DynamicSpawnManager mutates its copy with preset modifiers).
        /// bypassBackoff = true: one-shot consumer ignores the 30 s retry window — a cache hit
        /// still costs no HTTP. ref: PA-01-02
        /// </summary>
        public static string GetConfigJson(bool bypassBackoff)
        {
            EnsureFetched(bypassBackoff);
            return _cachedJson;
        }

        public static TRLConfig Config
        {
            get
            {
                EnsureFetched(bypassBackoff: false);
                return _cachedConfig;
            }
        }

        private static void EnsureFetched(bool bypassBackoff)
        {
            // Cache hit: the only cost is this branch. ref: AUD-01-01
            if (_cachedConfig != null) return;

            // Backoff: a failed attempt blocks new attempts for FailedFetchRetrySeconds. ref: AUD-01-03
            if (!bypassBackoff && Time.realtimeSinceStartup - _lastAttemptTime < FailedFetchRetrySeconds) return;

            // Register the attempt BEFORE the I/O so a failure also advances the clock.
            _lastAttemptTime = Time.realtimeSinceStartup;

            try
            {
                // Synchronous (SPT.Common) — acceptable because it now runs once per raid.
                string json = RequestHandler.GetJson(ConfigRoute);
                if (!string.IsNullOrEmpty(json))
                {
                    _cachedConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
                    _cachedJson = json;
                    Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Server config fetched (raid-scoped cache).");
                }
                else
                {
                    Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Empty ServerConfig response; retry in {FailedFetchRetrySeconds}s.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Failed to fetch ServerConfig: {ex.Message} — retry in {FailedFetchRetrySeconds}s.");
            }
        }

        /// <summary>Invalidates the cache. The next read of Config/GetConfigJson performs one fetch.</summary>
        public static void ForceRefresh()
        {
            _cachedConfig = null;
            _cachedJson = null;
            _lastAttemptTime = -1000f;
        }
    }
}
