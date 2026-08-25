using System;
using System.Collections.Generic;
using EFT;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Helpers
{
    /// <summary>
    /// Cache centralizado de instâncias de BotZone para o raid ativo.
    /// Evita varreduras repetitivas e custosas de cena (LocationScene.GetAllObjects) durante o gameplay.
    /// ref: AUD-02-03
    /// </summary>
    public static class ZoneCache
    {
        private static readonly List<BotZone> _allZones = new List<BotZone>();
        private static readonly List<BotZone> _regularZones = new List<BotZone>();
        private static readonly List<BotZone> _sniperZones = new List<BotZone>();
        private static bool _isInitialized = false;

        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Popula o cache de BotZones uma única vez no início da raid (Warm-up).
        /// </summary>
        public static void Initialize()
        {
            _allZones.Clear();
            _regularZones.Clear();
            _sniperZones.Clear();

            try
            {
                var zones = LocationScene.GetAllObjects<BotZone>();
                if (zones != null)
                {
                    foreach (var z in zones)
                    {
                        if (z == null) continue;
                        _allZones.Add(z);

                        if (SpawnPointHelper.IsSniperZone(z))
                        {
                            _sniperZones.Add(z);
                        }
                        else
                        {
                            _regularZones.Add(z);
                        }
                    }
                }
                _isInitialized = true;
                Plugin.LogSource?.LogInfo($"[TRL-DynamicSpawn] ZoneCache initialized: Total={_allZones.Count}, Regular={_regularZones.Count}, Sniper={_sniperZones.Count}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] Error initializing ZoneCache: {ex.Message}");
            }
        }

        public static List<BotZone> GetAllZones()
        {
            if (!_isInitialized || _allZones.Count == 0) Initialize();
            return _allZones;
        }

        public static List<BotZone> GetRegularZones()
        {
            if (!_isInitialized || _allZones.Count == 0) Initialize();
            return _regularZones;
        }

        public static List<BotZone> GetSniperZones()
        {
            if (!_isInitialized || _allZones.Count == 0) Initialize();
            return _sniperZones;
        }

        /// <summary>
        /// Limpa todas as referências no término da raid para evitar retenção de memória.
        /// </summary>
        public static void Clear()
        {
            _allZones.Clear();
            _regularZones.Clear();
            _sniperZones.Clear();
            _isInitialized = false;
        }
    }
}
