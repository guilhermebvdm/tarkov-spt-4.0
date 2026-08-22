using System;
using Newtonsoft.Json;
using SPT.Common.Http;
using TRLDynamicSpawn.Models;
using UnityEngine;

namespace TRLDynamicSpawn.Helpers
{
    public static class ServerConfigProvider
    {
        private static TRLConfig _cachedConfig;
        private static float _lastFetchTime = -1000f;

        public static TRLConfig Config
        {
            get
            {
                if (_cachedConfig == null || (Time.realtimeSinceStartup - _lastFetchTime > 5f))
                {
                    try
                    {
                        string json = RequestHandler.GetJson("/trldynamicspawn/getConfig");
                        if (!string.IsNullOrEmpty(json))
                        {
                            _cachedConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
                            _lastFetchTime = Time.realtimeSinceStartup;
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.LogSource?.LogWarning($"[TRL-DynamicSpawn] Failed to fetch static ServerConfig: {ex.Message}");
                    }
                }
                return _cachedConfig;
            }
        }

        public static void ForceRefresh()
        {
            _cachedConfig = null;
            _lastFetchTime = -1000f;
        }
    }
}
