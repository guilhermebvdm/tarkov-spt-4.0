using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;
using TRLDynamicSpawn.Helpers;
using EFT.Communications;
using Newtonsoft.Json;
using SPT.Common.Http;
using TRLDynamicSpawn.Models;

namespace TRLDynamicSpawn.Components
{
    public class DynamicSpawnManager : MonoBehaviour
    {
        public static DynamicSpawnManager Instance { get; private set; }
        public static bool IsGeneratingDynamicWave = false;
        public static bool IsWarmupActive = true;
        public static bool RaidInitialElitesSpawned = false;
        
        private int _delayBeforeFirstWave = 60;
        private int _secondsBetweenWaves = 360; 
        private float _nextWaveTime = 0f;
        
        public static float NextWaveTime => Instance != null ? Instance._nextWaveTime : 0f;
        
        private string _activePreset = "Balanced";
        private TRLConfig _serverConfig;
        public TRLConfig ServerConfig => ServerConfigProvider.Config ?? _serverConfig;
        
        // Caching settings
        private GameWorld _gameWorld;
        private IBotCreator _botCreator;
        private BotsController _botsController;
        private Coroutine _activeWaveCoroutine;
        private bool _sptQueueClearedThisRaid;   // ref: AUD-01-05 — instance field: the component is re-created per raid (DynamicSpawnManagerPatch.cs:60)
        private string _cachedFikaStatus;
        private BotZone _lastSelectedZone;

        public void Init(GameWorld gameWorld, IBotCreator botCreator, BotsController botsController)
        {
            Instance = this;
            _gameWorld = gameWorld;
            _botCreator = botCreator;
            _botsController = botsController;

            ZoneCache.Initialize();
            StartCoroutine(FetchServerConfigAndStart());
        }

        public static Dictionary<string, List<Vector3Model>> PmcSpawns = new();
        public static Dictionary<string, List<Vector3Model>> ScavSpawns = new();

        public static void ClearStaticState()
        {
            PmcSpawns.Clear();
            ScavSpawns.Clear();
            IsGeneratingDynamicWave = false;
            IsWarmupActive = true;
            RaidInitialElitesSpawned = false;
            ZoneCache.Clear();
        }
        
        private IEnumerator FetchServerConfigAndStart()
        {
            if (FikaHelper.IsClient())
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Client peer detected. Spawning is managed by the host. Disabling manager loop.");
                yield break;
            }


            try 
            {
                // ref: AUD-01-01 — same HTTP response as ServerConfigProvider (1 fetch per raid), private copy
                // because the preset modifiers below mutate _serverConfig (NR-6).
                // bypassBackoff: this is the raid's one-shot consumer — an earlier failure by another reader
                // must not deny its single attempt (PA-01-02). Cache hit → no HTTP.
                string json = ServerConfigProvider.GetConfigJson(bypassBackoff: true);
                if (string.IsNullOrEmpty(json))
                    throw new InvalidOperationException("ServerConfig unavailable (see provider warning).");
                _serverConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
                _activePreset = _serverConfig.ActivePreset;

                // Handle Random Preset Selection
                if (_activePreset == "Random")
                {
                    string[] availablePresets = { "Balanced", "PMC War", "Scav Infestation", "Quiet Raid", "Warzone" };
                    _activePreset = availablePresets[UnityEngine.Random.Range(0, availablePresets.Length)];
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Aleatorio rolled preset: {_activePreset}");
                }

                string currentMapName = GetCurrentMapName();
                _delayBeforeFirstWave = GetDelayBeforeFirstWaveForMap(currentMapName);
                _secondsBetweenWaves = GetSecondsBetweenWavesForMap(currentMapName);
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Connected to Server. Active Preset: {_activePreset}");

                // Apply Preset Modifiers
                if (_activePreset == "Quiet Raid")
                {
                    if (_serverConfig.EliteConfig != null)
                    {
                        _serverConfig.EliteConfig.Usec.MaxGroupSize = Mathf.Min(_serverConfig.EliteConfig.Usec.MaxGroupSize, 2);
                        _serverConfig.EliteConfig.Bear.MaxGroupSize = Mathf.Min(_serverConfig.EliteConfig.Bear.MaxGroupSize, 2);
                        _serverConfig.EliteConfig.Scav.MaxGroupSize = Mathf.Min(_serverConfig.EliteConfig.Scav.MaxGroupSize, 2);
                    }
                }
                else if (_activePreset == "Warzone")
                {
                    if (_serverConfig.EliteConfig != null)
                    {
                        _serverConfig.EliteConfig.RandomRaiderGroup = true;
                        _serverConfig.EliteConfig.RandomRaiderGroupChance = 30;
                        _serverConfig.EliteConfig.RandomRogueGroup = true;
                        _serverConfig.EliteConfig.RandomRogueGroupChance = 30;
                    }
                }
                else if (_activePreset == "PMC War")
                {
                    if (_serverConfig.EliteConfig != null)
                    {
                        _serverConfig.EliteConfig.Usec.GroupChance = 80;
                        _serverConfig.EliteConfig.Bear.GroupChance = 80;
                        _serverConfig.EliteConfig.Usec.MaxGroupSize += 1;
                        _serverConfig.EliteConfig.Bear.MaxGroupSize += 1;
                    }
                }
                else if (_activePreset == "Scav Infestation")
                {
                    if (_serverConfig.EliteConfig != null)
                    {
                        _serverConfig.EliteConfig.Scav.GroupChance = 90;
                        _serverConfig.EliteConfig.Scav.MaxGroupSize += 2;
                    }
                }

                if (_serverConfig.CustomSpawnsConfig != null)
                {
                    if (_serverConfig.CustomSpawnsConfig.EnableCustomPmcSpawns)
                    {
                        string pmcJson = RequestHandler.GetJson("/trldynamicspawn/getPmcSpawns");
                        PmcSpawns = JsonConvert.DeserializeObject<Dictionary<string, List<Vector3Model>>>(pmcJson);
                    }
                    if (_serverConfig.CustomSpawnsConfig.EnableCustomScavSpawns)
                    {
                        string scavJson = RequestHandler.GetJson("/trldynamicspawn/getScavSpawns");
                        ScavSpawns = JsonConvert.DeserializeObject<Dictionary<string, List<Vector3Model>>>(scavJson);
                    }
                }

                // Pre-populate SPT Bot Creator Backup target for PMCs to resolve profile generation empty queues
                if (_botCreator != null)
                {
                    // ref: AUD-01-04 / AC-X2 / CR-01-01 — AddToTargetBackup is NOT "request N profiles now": it registers a
                    // STANDING cache level per (role, difficulty) that SPT keeps replenished for the whole raid (GClass684.cs:258-263
                    // registers the key only if absent; a 5 s timer refills it, GClass684.cs:129-192). The level is configurable
                    // (was fixed 30/30). assault/marksman are NOT registered here: vanilla already holds them at 8 per difficulty
                    // (GClass684.cs:113-118) and a second call for the same key is a no-op.
                    int preload = Settings.initialProfilePreload.Value;
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Registering standing PMC profile cache levels (USEC/BEAR normal = {preload}; Scav levels are vanilla's 8/difficulty) + Rogues/Goons per config...");
                    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcUSEC, preload);
                    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcBEAR, preload);
                    if (_serverConfig?.EliteConfig?.Rogues != null && _serverConfig.EliteConfig.Rogues.Enable)
                    {
                        _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.exUsec, 10);
                    }
                    if (_serverConfig?.EliteConfig?.BossKnight != null && _serverConfig.EliteConfig.BossKnight.Enable)
                    {
                        _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.bossKnight, 5);
                        _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.followerBigPipe, 5);
                        _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.followerBirdEye, 5);
                    }
                }

                // Ajusta as waves de boss originais do vanilla baseado nas configurações recebidas do servidor
                AdjustVanillaBossWaves();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error fetching config from server: {ex.Message}");
                _serverConfig = new TRLConfig(); // Fallback
            }
            
            yield return StartCoroutine(SpawnHordeLoop());
        }

        private void ClearSptQueue()
        {
            if (Singleton<BotEventHandler>.Instantiated)
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Clearing pending/stuck bot profile creation queue in SPT...");
                Singleton<BotEventHandler>.Instance.StopBotSpawn();
            }
        }

        private int GetSecondsBetweenWavesForMap(string mapName)
        {
            int seconds = 360;
            var mapSettings = MapNameHelper.GetMapSettings(_serverConfig, mapName);
            if (mapSettings != null && mapSettings.SecondsBetweenWaves > 0)
            {
                seconds = mapSettings.SecondsBetweenWaves;
            }
            else if (_serverConfig?.MapTimers != null && _serverConfig.MapTimers.ContainsKey("global") && _serverConfig.MapTimers["global"].SecondsBetweenWaves > 0)
            {
                seconds = _serverConfig.MapTimers["global"].SecondsBetweenWaves;
            }

            if (_activePreset == "Quiet Raid")
            {
                seconds = (int)(seconds * 1.5f);
            }
            else if (_activePreset == "Warzone")
            {
                seconds = (int)(seconds * 0.5f);
            }

            return seconds;
        }

        private int GetDelayBeforeFirstWaveForMap(string mapName)
        {
            var timerConfig = MapNameHelper.GetWaveTimerConfig(_serverConfig, mapName);
            return timerConfig != null ? timerConfig.DelayBeforeFirstWave : 30;
        }

        public int GetRealAliveBotsCount()
        {
            if (_botsController == null || _botsController.Bots == null || _botsController.Bots.BotOwners == null)
                return 0;

            int count = 0;
            var botArray = _botsController.Bots.BotOwners.ToArray();
            for (int i = 0; i < botArray.Length; i++)
            {
                var b = botArray[i];
                if (b != null && b.GetPlayer != null && b.HealthController != null && b.HealthController.IsAlive)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Alive human players (host + Fika guests), ignoring bots and the headless "player".
        /// ref: AUD-01-06 — waves are only computed while someone is alive to spawn for.
        /// ref: Assembly-CSharp/EFT/GameWorld.cs:556 (AllAlivePlayersList : List&lt;Player&gt;)
        /// </summary>
        public int GetAliveHumanCount()
        {
            if (_gameWorld == null) return 0;
            var list = _gameWorld.AllAlivePlayersList;
            if (list == null) return 0;
            int n = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p == null || p.IsAI || IsHeadlessPlayer(p)) continue;
                if (p.HealthController != null && p.HealthController.IsAlive) n++;
            }
            return n;
        }

        /// <summary>
        /// Raid end (called by RaidLifecycle.OnRaidEnd from the first end hook that fires). Stops every spawn coroutine
        /// of this manager and resets the static flags. ref: AUD-01-06
        /// </summary>
        public static void StopSpawnLoops()
        {
            // Static flags FIRST: called from the GameWorld.OnDestroy prefix, the manager (same GameObject) may already
            // be destroyed (Unity fake-null) and the early return below would skip the reset. ref: PA-01-06
            IsGeneratingDynamicWave = false;
            IsWarmupActive = false;
            RaidInitialElitesSpawned = false;
            if (Instance == null) return;
            Instance.StopAllCoroutines();   // SpawnHordeLoop, FetchServerConfigAndStart, ProcessWave, SpawnGroupBotsCoroutine
            Instance._activeWaveCoroutine = null;
        }

        // Roles controlados pelo TRL-DynamicSpawn — qualquer outro é "especial" (boss, guard, rogue, raider, etc.)
        private static readonly HashSet<WildSpawnType> _normalBotRoles = new HashSet<WildSpawnType>
        {
            WildSpawnType.assault,
            WildSpawnType.cursedAssault,
            WildSpawnType.pmcUSEC,
            WildSpawnType.pmcBEAR
        };

        /// <summary>
        /// Conta bots vivos que NÃO são controlados pelo TRL-DynamicSpawn
        /// (bosses, guards/followers, rogues, raiders, cultistas, snipers, etc.).
        /// </summary>
        public int GetSpecialBotsCount()
        {
            if (_gameWorld?.RegisteredPlayers == null) return 0;
            return _gameWorld.RegisteredPlayers.Count(p =>
                p.IsAI &&
                p.HealthController != null &&
                p.HealthController.IsAlive &&
                p.Profile?.Info?.Settings != null &&
                !_normalBotRoles.Contains(p.Profile.Info.Settings.Role));
        }

        /// <summary>
        /// Detecta se o mod SAIN está presente na sessão.
        /// </summary>
        private static bool? _cachedIsSainInstalled = null;
        public static bool IsSainInstalled()
        {
            if (_cachedIsSainInstalled.HasValue) return _cachedIsSainInstalled.Value;
            try
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("me.sol.sain") ||
                    BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("SAIN"))
                {
                    _cachedIsSainInstalled = true;
                    return true;
                }

                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                bool found = assemblies.Any(a => a.GetName().Name.StartsWith("SAIN", StringComparison.OrdinalIgnoreCase));
                _cachedIsSainInstalled = found;
                return found;
            }
            catch
            {
                _cachedIsSainInstalled = false;
                return false;
            }
        }

        /// <summary>
        /// Amostragem ponderada de dificuldade com base nas porcentagens do painel Web.
        /// </summary>
        public BotDifficulty GetRandomDifficulty(Dictionary<string, double> diffWeights)
        {
            if (diffWeights == null || diffWeights.Count == 0) return BotDifficulty.normal;

            double easy = diffWeights.TryGetValue("easy", out double e) ? e : 0;
            double normal = diffWeights.TryGetValue("normal", out double n) ? n : 0;
            double hard = diffWeights.TryGetValue("hard", out double h) ? h : 0;
            double impossible = diffWeights.TryGetValue("impossible", out double imp) ? imp : 0;

            double totalWeight = easy + normal + hard + impossible;
            if (totalWeight <= 0) return BotDifficulty.normal;

            double roll = UnityEngine.Random.Range(0f, (float)totalWeight);
            if (roll < easy) return BotDifficulty.easy;
            roll -= easy;
            if (roll < normal) return BotDifficulty.normal;
            roll -= normal;
            if (roll < hard) return BotDifficulty.hard;
            return BotDifficulty.impossible;
        }

        /// <summary>
        /// Retorna o teto dinâmico de bots = playerCap + bots especiais vivos.
        /// Garante que PMCs e Scavs sempre tenham suas cotas completas.
        /// </summary>
        public int GetDynamicCap(string mapName)
        {
            int playerCap = Settings.GetMapCap(mapName);
            int specialBots = GetSpecialBotsCount();
            return playerCap + specialBots;
        }

        /// <summary>
        /// Aplica o teto dinâmico no motor do EFT (BotsController, BotSpawner, ZonesLeaveController).
        /// Chamado a cada wave e a cada ciclo do SpawnHordeLoop.
        /// </summary>
        private void ApplyDynamicMaxBot(int dynamicCap)
        {
            if (_botsController == null) return;
            _botsController.MaxCount = dynamicCap;
            _botsController.BotSpawner?.SetMaxBots(dynamicCap);
            _botsController.ZonesLeaveController?.SetMaxBots(dynamicCap);
        }

        private IEnumerator SpawnHordeLoop()
        {
            // 0. Checagem de desativação global de bots via RaidSettings (AI amount = "None" / EBotAmount.NoBots)
            try
            {
                var app = Comfort.Common.Singleton<ClientApplication<ISession>>.Instance as TarkovApplication;
                if (app != null)
                {
                    var field = typeof(TarkovApplication).GetField("_raidSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    var raidSettings = field?.GetValue(app) as RaidSettings;
                    if (raidSettings != null && (raidSettings.BotSettings.BotAmount == EFT.Bots.EBotAmount.NoBots || raidSettings.WavesSettings.BotAmount == EFT.Bots.EBotAmount.NoBots))
                    {
                        Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] AI Amount set to 'None' (NoBots) in Raid Settings. DynamicSpawn is DISABLED for this raid.");
                        IsWarmupActive = false;
                        yield break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error checking RaidSettings BotAmount: {ex.Message}");
            }

            // 1. Janela Vanilla Inicial (DelayBeforeFirstWave configurável)
            IsWarmupActive = true;

            string mapName = GetCurrentMapName();
            _delayBeforeFirstWave = GetDelayBeforeFirstWaveForMap(mapName);

            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Initial Vanilla Window active. Waiting {_delayBeforeFirstWave}s for native bosses/snipers before taking control...");
            _nextWaveTime = Time.time + _delayBeforeFirstWave;

            if (_delayBeforeFirstWave > 0)
            {
                yield return new WaitForSeconds(_delayBeforeFirstWave);
            }

            // 2. Trava do Spawn Vanilla após o atraso inicial
            IsWarmupActive = false;
            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Initial {_delayBeforeFirstWave}s elapsed. Vanilla normal spawns LOCKED. DynamicSpawn taking 100% control.");

            // 3. Loop Contínuo do Mod (Warmup lendo DelayBeforeFirstWave <-> Cooldown de Horda)
            while (true)
            {
                string currentMap = GetCurrentMapName();
                int playerCap = Settings.GetMapCap(currentMap);
                int dynamicCap = GetDynamicCap(currentMap);
                ApplyDynamicMaxBot(dynamicCap);

                int aliveRealBots = GetRealAliveBotsCount();
                _secondsBetweenWaves = GetSecondsBetweenWavesForMap(currentMap);
                int warmupInterval = GetDelayBeforeFirstWaveForMap(currentMap);
                if (warmupInterval <= 0) warmupInterval = 30;

                // ESTÁGIO A: Se os BOTS REALMENTE VIVOS forem menores que o dynamicCap, roda o Warmup com o intervalo configurado
                if (aliveRealBots < dynamicCap)
                {
                    int warmupAttempt = 1;
                    while (true)
                    {
                        yield return new WaitForSeconds(1f);

                        // ref: AUD-01-06 / AC-X3 — no human alive → nothing to spawn for. Stop the wave in flight
                        // (the child SpawnGroupBotsCoroutine finishes its current group — Unity limitation) and keep
                        // re-checking; the raid is ending anyway.
                        if (GetAliveHumanCount() == 0)
                        {
                            if (_activeWaveCoroutine != null) { StopCoroutine(_activeWaveCoroutine); _activeWaveCoroutine = null; }
                            IsGeneratingDynamicWave = false;
                            _nextWaveTime = 0f;
                            yield return new WaitForSeconds(5f);
                            continue;
                        }

                        // ref: AUD-01-05 — ONE clean-up of the SPT creation queue per raid (item 006 intent preserved).
                        // It used to run every 1 s here: BotEventHandler.StopBotSpawn() cancels EVERY BotCreationDataClass
                        // in flight (vanilla and the mod's own) → Create returns null → 44 NRE/raid in TrySpawnFreeInner.
                        if (!_sptQueueClearedThisRaid)
                        {
                            _sptQueueClearedThisRaid = true;
                            ClearSptQueue();
                        }

                        currentMap = GetCurrentMapName();
                        playerCap = Settings.GetMapCap(currentMap);
                        dynamicCap = GetDynamicCap(currentMap);
                        ApplyDynamicMaxBot(dynamicCap);
                        aliveRealBots = GetRealAliveBotsCount();
                        warmupInterval = GetDelayBeforeFirstWaveForMap(currentMap);
                        if (warmupInterval <= 0) warmupInterval = 30;

                        if (aliveRealBots >= dynamicCap)
                        {
                            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Map is 100% full of REAL alive bots ({aliveRealBots}/{dynamicCap} | playerCap={playerCap}). Stopping {warmupInterval}s Warmup loop.");
                            break;
                        }

                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Map needs bots (REAL Alive: {aliveRealBots}/{dynamicCap} | playerCap={playerCap}). Triggering {warmupInterval}s Warmup Wave (Attempt {warmupAttempt})...");

                        if (_activeWaveCoroutine != null) StopCoroutine(_activeWaveCoroutine);
                        bool isFirstWave = (warmupAttempt == 1);
                        _activeWaveCoroutine = StartCoroutine(ProcessWave(isFirstWave));
                        _nextWaveTime = Time.time + warmupInterval;

                        bool capReachedEarly = false;
                        for (int elapsed = 0; elapsed < warmupInterval; elapsed++)
                        {
                            yield return new WaitForSeconds(1f);

                            // ref: CR-01-02 / AC-X3 — reach the wave in flight within 1 s, not after warmupInterval:
                            // stop it here and fall through (capReachedEarly stays false → warmupAttempt++ → top of the
                            // inner while → the pause branch takes over with its 5 s re-check).
                            if (GetAliveHumanCount() == 0)
                            {
                                if (_activeWaveCoroutine != null) { StopCoroutine(_activeWaveCoroutine); _activeWaveCoroutine = null; }
                                IsGeneratingDynamicWave = false;
                                _nextWaveTime = 0f;
                                break;
                            }

                            currentMap = GetCurrentMapName();
                            playerCap = Settings.GetMapCap(currentMap);
                            dynamicCap = GetDynamicCap(currentMap);
                            ApplyDynamicMaxBot(dynamicCap);
                            aliveRealBots = GetRealAliveBotsCount();

                            if (aliveRealBots >= dynamicCap)
                            {
                                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Max cap reached early with REAL alive bots ({aliveRealBots}/{dynamicCap} | playerCap={playerCap}) at {elapsed + 1}s during Warmup. Interrupting {warmupInterval}s timer immediately.");
                                capReachedEarly = true;
                                break;
                            }
                        }

                        if (capReachedEarly)
                        {
                            break;
                        }

                        warmupAttempt++;
                    }
                }

                // ESTÁGIO B: Mapa 100% cheio de BOTS REAIS -> Entra no Cooldown Longo (SecondsBetweenWaves, ex: 600s)
                currentMap = GetCurrentMapName();
                playerCap = Settings.GetMapCap(currentMap);
                dynamicCap = GetDynamicCap(currentMap);
                ApplyDynamicMaxBot(dynamicCap);
                aliveRealBots = GetRealAliveBotsCount();
                _secondsBetweenWaves = GetSecondsBetweenWavesForMap(currentMap);

                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Map is 100% full with REAL alive bots ({aliveRealBots}/{dynamicCap} | playerCap={playerCap} | specialBots={GetSpecialBotsCount()}). Wave Cooldown active for '{currentMap}'. Next Warmup check in {_secondsBetweenWaves}s...");
                _nextWaveTime = Time.time + _secondsBetweenWaves;

                float waitNormal = Mathf.Max(1f, _secondsBetweenWaves);
                yield return new WaitForSeconds(waitNormal);
            }
        }

        private class DummyToken : GInterface22
        {
            private System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
            public System.Threading.CancellationToken GetCancelToken()
            {
                return _cts.Token;
            }
        }


        private string GetCurrentMapName()
        {
            if (_gameWorld.MainPlayer != null && !string.IsNullOrEmpty(_gameWorld.MainPlayer.Location))
                return _gameWorld.MainPlayer.Location;
            
            var anyPlayer = _gameWorld.RegisteredPlayers.OfType<Player>().FirstOrDefault(p => p != null && !string.IsNullOrEmpty(p.Location));
            return anyPlayer != null ? anyPlayer.Location : "factory4_day";
        }

        private IEnumerator ProcessWave(bool isFirstWave)
        {
            // ref: CR-01-03 — `_isSpawningWave` removed (dead field, never read); `_activeWaveCoroutine != null` is the real gate.
            string currentMap = GetCurrentMapName();
            int playerCap = Settings.GetMapCap(currentMap);
            int specialBots = GetSpecialBotsCount();
            int dynamicCap = playerCap + specialBots;
            ApplyDynamicMaxBot(dynamicCap);

            int aliveBots = GetRealAliveBotsCount();
            int availableSlots = Mathf.Max(0, dynamicCap - aliveBots);

            if (Settings.enableDebugLogs.Value)   // ref: AUD-01-07
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Calculating Wave: PlayerCap={playerCap}, SpecialBots={specialBots}, DynamicCap={dynamicCap}, Alive={aliveBots}, Available={availableSlots}");

            // ref: AUD-01-04 / CR-01-01 — the fixed 10/10/10 'normal' AddToTargetBackup per wave was removed: the key
            // (role, normal) is already registered at raid start, so those calls were no-ops (GClass684.cs:260).

            if (availableSlots <= 0)
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] No slots available for this wave. Skipping.");
                yield break;
            }

            List<Tuple<SpawnGroupData, BotZone>> spawnList = new List<Tuple<SpawnGroupData, BotZone>>();

            // ======================================
            // PROCESS ELITES / BOSSES (Handled by vanilla now, skipping manual queue)
            // ======================================
            var mapName = currentMap.ToLower();
            var eliteConfig = _serverConfig?.EliteConfig;

            // Group Splitting Local Function
            void GenerateAndEnqueueGroups(WildSpawnType role, BotDifficulty diff, int totalSlots, EliteLocationInfo info, BotZone overrideZone = null)
            {
                int slotsRemaining = totalSlots;
                while (slotsRemaining > 0)
                {
                    int groupSize = 1;
                    if (info != null && info.GroupChance > 0 && UnityEngine.Random.Range(1, 101) <= info.GroupChance)
                    {
                        int maxSize = Mathf.Max(2, info.MaxGroupSize);
                        groupSize = UnityEngine.Random.Range(2, maxSize + 1);
                    }
                    if (groupSize > slotsRemaining) groupSize = slotsRemaining;

                    var gData = new SpawnGroupData
                    {
                        Role = role,
                        Difficulty = diff,
                        GroupSize = groupSize,
                        Info = info
                    };

                    BotZone zoneToUse = overrideZone ?? GetHotzone(info, mapName);
                    spawnList.Add(new Tuple<SpawnGroupData, BotZone>(gData, zoneToUse));
                    slotsRemaining -= groupSize;
                }
            }

            // ======================================
            // PROCESS NON-NATIVE ELITES / ROGUES / BOSSES
            // ======================================
            if (isFirstWave && !RaidInitialElitesSpawned && eliteConfig != null && !eliteConfig.DisableBosses)
            {
                var eliteEntries = new (EliteLocationInfo info, WildSpawnType role, string bossName)[]
                {
                    (eliteConfig.Rogues, WildSpawnType.exUsec, "exusec"),
                    (eliteConfig.Raiders, WildSpawnType.pmcBot, "pmcbot"),
                    (eliteConfig.Bloodhounds, WildSpawnType.arenaFighterEvent, "arenafighterevent"),
                    (eliteConfig.Cultists, WildSpawnType.sectantPriest, "sectantpriest"),
                    (eliteConfig.BossKnight, WildSpawnType.bossKnight, "bossknight"),
                    (eliteConfig.BossTagilla, WildSpawnType.bossTagilla, "bosstagilla"),
                    (eliteConfig.BossKilla, WildSpawnType.bossKilla, "bosskilla"),
                    (eliteConfig.BossZryachiy, WildSpawnType.bossZryachiy, "bosszryachiy"),
                    (eliteConfig.BossGluhar, WildSpawnType.bossGluhar, "bossgluhar"),
                    (eliteConfig.BossSanitar, WildSpawnType.bossSanitar, "bosssanitar"),
                    (eliteConfig.BossKolontay, WildSpawnType.bossKolontay, "bosskolontay"),
                    (eliteConfig.BossReshala, WildSpawnType.bossBully, "bossreshala"),
                    (eliteConfig.BossKaban, WildSpawnType.bossBoar, "bosskaban"),
                    (eliteConfig.BossShturman, WildSpawnType.bossKojaniy, "bossshturman"),
                    (eliteConfig.BossPartisan, WildSpawnType.bossPartisan, "bosspartisan"),
                    (eliteConfig.BossGifter, WildSpawnType.gifter, "gifter")
                };

                foreach (var entry in eliteEntries)
                {
                    if (entry.info == null || !entry.info.Enable) continue;

                    // Regra Vanilla: Cultistas (sectantPriest) só nascem de noite (22:00 às 06:00)
                    if (entry.role == WildSpawnType.sectantPriest && !IsNightTimeForCultists())
                    {
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Skipping Cultists (sectantPriest) spawn on {mapName}: Raid hour is day time (Cultists only spawn between 22:00 and 06:00).");
                        continue;
                    }

                    int spawnChance = GetBossChanceForMap(entry.info.SpawnChance, mapName);
                    if (spawnChance <= 0) continue;

                    // Se o mapa JÁ possui uma wave vanilla nativa para este chefe/elite (ex: Rogues no Lighthouse ou Reshala no Customs),
                    // a wave vanilla é ajustada no AdjustVanillaBossWaves() e controlada nativamente.
                    if (HasNativeVanillaWave(entry.bossName)) continue;

                    // Se NÃO possui wave nativa no mapa (ex: Rogues no Customs ou Ground Zero), geramos o spawn dinamicamente!
                    if (UnityEngine.Random.Range(1, 101) <= spawnChance)
                    {
                        // Regra Especial: Trio Goons (Knight, BigPipe, BirdEye)
                        if (entry.role == WildSpawnType.bossKnight && !entry.info.DisableFollowers)
                        {
                            BotZone goonZone = GetZoneFromConfig(entry.info, mapName, entry.role);
                            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Non-Native Goon Trio Invasion: Queueing full Goon Squad (Knight, BigPipe, BirdEye) on {mapName} in zone '{goonZone?.NameZone ?? "Random"}' (Chance: {spawnChance}%)...");
                            
                            spawnList.Add(new Tuple<SpawnGroupData, BotZone>(new SpawnGroupData { Role = WildSpawnType.bossKnight, Difficulty = BotDifficulty.normal, GroupSize = 1, Info = entry.info }, goonZone));
                            spawnList.Add(new Tuple<SpawnGroupData, BotZone>(new SpawnGroupData { Role = WildSpawnType.followerBigPipe, Difficulty = BotDifficulty.normal, GroupSize = 1, Info = entry.info }, goonZone));
                            spawnList.Add(new Tuple<SpawnGroupData, BotZone>(new SpawnGroupData { Role = WildSpawnType.followerBirdEye, Difficulty = BotDifficulty.normal, GroupSize = 1, Info = entry.info }, goonZone));
                            continue;
                        }

                        int targetGroupSize = GetBossGroupSizeForMap(entry.info, mapName);
                        if (entry.role != WildSpawnType.exUsec && entry.role != WildSpawnType.pmcBot)
                        {
                            if (entry.info.GroupChance > 0 && entry.info.GroupChance < 100)
                            {
                                if (UnityEngine.Random.Range(1, 101) > entry.info.GroupChance)
                                {
                                    targetGroupSize = 1;
                                }
                                else
                                {
                                    targetGroupSize = UnityEngine.Random.Range(2, Mathf.Max(2, entry.info.MaxGroupSize) + 1);
                                }
                            }
                        }

                        BotZone selectedZone = GetZoneFromConfig(entry.info, mapName, entry.role);
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Non-Native Elite Invasion: Queueing squad of {targetGroupSize}x {entry.role} ({entry.bossName}) on {mapName} in zone '{selectedZone?.NameZone ?? "Random"}' (Chance: {spawnChance}%)...");
                        
                        var gData = new SpawnGroupData
                        {
                            Role = entry.role,
                            Difficulty = BotDifficulty.normal,
                            GroupSize = targetGroupSize,
                            Info = entry.info
                        };
                        spawnList.Add(new Tuple<SpawnGroupData, BotZone>(gData, selectedZone));
                    }
                }
            }

            // ======================================
            // PROCESS REGULAR HORDE (PMCs/Scavs)
            // ======================================
            if (availableSlots > 0)
            {
                if (isFirstWave && !RaidInitialElitesSpawned && eliteConfig != null && !eliteConfig.DisableBosses)
                {
                    int raiderMapChance = GetBossChanceForMap(eliteConfig.Raiders?.SpawnChance, mapName);
                    if (eliteConfig.RandomRaiderGroup && (eliteConfig.Raiders?.Enable ?? true) && raiderMapChance > 0 && UnityEngine.Random.Range(1, 101) <= eliteConfig.RandomRaiderGroupChance)
                    {
                        int configuredRaiderSquad = GetBossGroupSizeForMap(eliteConfig.Raiders, mapName);
                        int raiderSlots = Mathf.Min(availableSlots, configuredRaiderSquad > 0 ? configuredRaiderSquad : UnityEngine.Random.Range(2, 5));
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] RANDOM RAIDER GROUP INVASION! Spawning {raiderSlots} Raiders on {mapName}.");
                        GenerateAndEnqueueGroups(WildSpawnType.pmcBot, BotDifficulty.normal, raiderSlots, eliteConfig.Raiders);
                        availableSlots -= raiderSlots;
                    }

                    int rogueMapChance = GetBossChanceForMap(eliteConfig.Rogues?.SpawnChance, mapName);
                    if (eliteConfig.RandomRogueGroup && (eliteConfig.Rogues?.Enable ?? true) && rogueMapChance > 0 && availableSlots > 0 && UnityEngine.Random.Range(1, 101) <= eliteConfig.RandomRogueGroupChance)
                    {
                        int configuredRogueSquad = GetBossGroupSizeForMap(eliteConfig.Rogues, mapName);
                        int rogueSlots = Mathf.Min(availableSlots, configuredRogueSquad > 0 ? configuredRogueSquad : UnityEngine.Random.Range(2, 5));
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] RANDOM ROGUE GROUP INVASION! Spawning {rogueSlots} Rogues on {mapName}.");
                        GenerateAndEnqueueGroups(WildSpawnType.exUsec, BotDifficulty.normal, rogueSlots, eliteConfig.Rogues);
                        availableSlots -= rogueSlots;
                    }
                }

                if (isFirstWave)
                {
                    RaidInitialElitesSpawned = true;
                }

                float pmcRatio = 0.5f;
                if (_activePreset == "PMC War") pmcRatio = 0.7f;
                else if (_activePreset == "Scav Infestation") pmcRatio = 0.3f;
                else if (_activePreset == "Warzone") pmcRatio = UnityEngine.Random.Range(0.2f, 0.8f);

                int aliveBears = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive && p.Profile.Side == EPlayerSide.Bear);
                int aliveUsecs = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive && p.Profile.Side == EPlayerSide.Usec);
                int alivePMCs = aliveBears + aliveUsecs;
                
                int aliveBotsTotal = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive);
                
                // Scavs normais (Assault / CursedAssault) — Bots especiais (Rogues, Bosses, Raiders) NÃO contam como Scav!
                int aliveNormalScavs = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive && (p.Profile.Info.Settings.Role == WildSpawnType.assault || p.Profile.Info.Settings.Role == WildSpawnType.cursedAssault));

                // idealPMCs e idealScavs usam playerCap (cota que o player definiu),
                // NÃO o dynamicCap — bots especiais expandem o teto mas não roubam vagas normais.
                bool isLabs = mapName == "laboratory" || mapName == "lab";
                int idealPMCs = isLabs ? playerCap : Mathf.RoundToInt(playerCap * pmcRatio);
                int idealScavs = isLabs ? 0 : (playerCap - idealPMCs);

                int pmcSlots = Mathf.Max(0, idealPMCs - alivePMCs);
                int scavSlots = isLabs ? 0 : Mathf.Max(0, idealScavs - aliveNormalScavs);

                int totalTarget = pmcSlots + scavSlots;
                if (totalTarget > availableSlots && totalTarget > 0)
                {
                    float scale = (float)availableSlots / totalTarget;
                    pmcSlots = Mathf.RoundToInt(pmcSlots * scale);
                    scavSlots = isLabs ? 0 : Mathf.Max(0, availableSlots - pmcSlots);
                }


                // Balanceamento interno (BEAR vs USEC)
                int idealBears = Mathf.RoundToInt(idealPMCs * 0.5f);
                int idealUsecs = idealPMCs - idealBears;

                int bearDeficit = Mathf.Max(0, idealBears - aliveBears);
                int usecDeficit = Mathf.Max(0, idealUsecs - aliveUsecs);
                int totalDeficit = bearDeficit + usecDeficit;

                int bearSlots = 0;
                int usecSlots = 0;

                if (totalDeficit > 0 && pmcSlots > 0)
                {
                    if (totalDeficit <= pmcSlots)
                    {
                        bearSlots = bearDeficit;
                        usecSlots = usecDeficit;
                        int leftover = pmcSlots - totalDeficit;
                        bearSlots += Mathf.RoundToInt(leftover * 0.5f);
                        usecSlots += leftover - Mathf.RoundToInt(leftover * 0.5f);
                    }
                    else
                    {
                        // Se não tem vagas suficientes para sanar o déficit, distribui proporcionalmente
                        float bearScale = (float)bearDeficit / totalDeficit;
                        bearSlots = Mathf.RoundToInt(pmcSlots * bearScale);
                        usecSlots = pmcSlots - bearSlots;
                    }
                }
                else if (pmcSlots > 0)
                {
                    bearSlots = Mathf.RoundToInt(pmcSlots * 0.5f);
                    usecSlots = pmcSlots - bearSlots;
                }

                int pScavSlots = Mathf.RoundToInt(scavSlots * 0.5f);
                int normalScavSlots = scavSlots - pScavSlots;

                bool isSainActive = IsSainInstalled();
                bool debugLogs = Settings.enableDebugLogs.Value;   // ref: AUD-01-07 — gate before formatting; Info level
                if (isSainActive && debugLogs)
                {
                    Plugin.LogSource.LogInfo("[TRL-DynamicSpawn][SPY] SAIN Mod detected! DynamicSpawn difficulty override disabled. SAIN controls AI difficulty.");
                }

                BotDifficulty pmcDiff = isSainActive ? BotDifficulty.normal : GetRandomDifficulty(_serverConfig?.PmcDifficulty);
                BotDifficulty scavDiff = isSainActive ? BotDifficulty.normal : GetRandomDifficulty(_serverConfig?.ScavDifficulty);

                if (debugLogs)
                {
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn][SPY] Sampled Wave Difficulties (SAIN Active: {isSainActive}): PMC={pmcDiff}, Scav={scavDiff}");
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Horde Breakdown (Preset: {_activePreset}):");
                    Plugin.LogSource.LogInfo($"  PlayerCap={playerCap} | SpecialBots={specialBots} | DynamicCap={dynamicCap} | Available={availableSlots}");
                    Plugin.LogSource.LogInfo($"  Alive Bots: {aliveBotsTotal} | PMCs: {alivePMCs} ({aliveBears} BEAR, {aliveUsecs} USEC) | Normal Scavs: {aliveNormalScavs}");
                    Plugin.LogSource.LogInfo($"  Spawning PMCs ({pmcDiff}): {pmcSlots} ({bearSlots} BEAR, {usecSlots} USEC)");
                    Plugin.LogSource.LogInfo($"  Spawning Scavs ({scavDiff}): {scavSlots} ({normalScavSlots} Normal, {pScavSlots} pScav)");
                }

                // ref: CR-01-01 — these calls register a STANDING cache level for (role, sampled difficulty), kept replenished
                // by SPT for the rest of the raid; a second call for the same key is ignored (GClass684.cs:258-263). The number
                // of keys is bounded (2 PMC roles × 4 difficulties; assault easy/normal/hard already vanilla's), so the pool
                // stays bounded — this is what keeps the per-wave difficulty feature alive without SAIN. With SAIN the
                // difficulty is always 'normal' (key already registered at raid start → no-op).
                if (_botCreator != null && !isSainActive)
                {
                    if (usecSlots > 0) _botCreator.AddToTargetBackup(pmcDiff, WildSpawnType.pmcUSEC, usecSlots);
                    if (bearSlots > 0) _botCreator.AddToTargetBackup(pmcDiff, WildSpawnType.pmcBEAR, bearSlots);
                    int totalScavSlots = normalScavSlots + pScavSlots;
                    if (totalScavSlots > 0 && scavDiff == BotDifficulty.impossible)   // easy/normal/hard: vanilla key exists (no-op)
                        _botCreator.AddToTargetBackup(scavDiff, WildSpawnType.assault, totalScavSlots);
                    if (debugLogs)
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Standing backup targets ensured for this wave: USEC/BEAR ({pmcDiff}) = {usecSlots}/{bearSlots}, Scav ({scavDiff}) = {totalScavSlots} (no-op if key already registered).");
                }

                GenerateAndEnqueueGroups(WildSpawnType.pmcUSEC, pmcDiff, usecSlots, eliteConfig?.Usec);
                GenerateAndEnqueueGroups(WildSpawnType.pmcBEAR, pmcDiff, bearSlots, eliteConfig?.Bear);
                
                int sniperCount = 0;
                var currentMapSettings = MapNameHelper.GetMapSettings(_serverConfig, mapName);
                int mapSniperChance = currentMapSettings != null ? currentMapSettings.SniperChance : 30;
                if (isFirstWave && UnityEngine.Random.Range(1, 101) <= mapSniperChance && normalScavSlots > 0)
                {
                    sniperCount = 1;
                    normalScavSlots -= 1;
                    var sniperGroup = new SpawnGroupData
                    {
                        Role = WildSpawnType.marksman,
                        Difficulty = scavDiff,
                        GroupSize = sniperCount,
                        Info = null
                    };
                    spawnList.Add(new Tuple<SpawnGroupData, BotZone>(sniperGroup, null));
                }

                GenerateAndEnqueueGroups(WildSpawnType.assault, scavDiff, normalScavSlots, eliteConfig?.Scav);
                GenerateAndEnqueueGroups(WildSpawnType.assault, scavDiff, pScavSlots, eliteConfig?.Scav);
            }

            if (Settings.enableDebugLogs.Value)   // ref: AUD-01-07
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Profiles generated. Starting smooth injection...");



            // Embaralha as sublistas, mas intercala de forma balanceada PMCs e Scavs
            var rng = new System.Random();
            var pmcs = spawnList.Where(t => t.Item1.Role == WildSpawnType.pmcUSEC || t.Item1.Role == WildSpawnType.pmcBEAR).OrderBy(x => rng.Next()).ToList();
            var scavs = spawnList.Where(t => t.Item1.Role == WildSpawnType.assault || t.Item1.Role == WildSpawnType.marksman).OrderBy(x => rng.Next()).ToList();
            var elites = spawnList.Except(pmcs).Except(scavs).OrderBy(x => rng.Next()).ToList();

            var interleavedList = new List<Tuple<SpawnGroupData, BotZone>>();
            interleavedList.AddRange(elites);

            int pIdx = 0, sIdx = 0;
            int diff = System.Math.Abs(pmcs.Count - scavs.Count);

            // 1. Otimização do Equilíbrio: Spawna as diferenças primeiro para nivelar o mapa rapidamente
            if (pmcs.Count > scavs.Count)
            {
                for (int i = 0; i < diff; i++) interleavedList.Add(pmcs[pIdx++]);
            }
            else if (scavs.Count > pmcs.Count)
            {
                for (int i = 0; i < diff; i++) interleavedList.Add(scavs[sIdx++]);
            }

            // 2. Agora que o mapa está "nivelado", alterna 1:1 (começando aleatoriamente)
            bool pmcTurn = rng.Next(2) == 0;
            while (pIdx < pmcs.Count || sIdx < scavs.Count)
            {
                if (pmcTurn)
                {
                    if (pIdx < pmcs.Count) interleavedList.Add(pmcs[pIdx++]);
                    else if (sIdx < scavs.Count) interleavedList.Add(scavs[sIdx++]);
                }
                else
                {
                    if (sIdx < scavs.Count) interleavedList.Add(scavs[sIdx++]);
                    else if (pIdx < pmcs.Count) interleavedList.Add(pmcs[pIdx++]);
                }
                pmcTurn = !pmcTurn;
            }

            spawnList = interleavedList;

            // Then, inject regular horde
            foreach (var tuple in spawnList)
            {
                if (_botsController.AliveAndLoadingBotsCount >= dynamicCap)
                {
                    Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Max dynamic cap reached during smooth spawn. Stopping wave.");
                    break;
                }

                SpawnGroupData gData = tuple.Item1;
                BotZone preferredZone = tuple.Item2;
                
                BotZone selectedZone = preferredZone;

                if (selectedZone == null)
                {
                    if (SpawnPointHelper.IsSniperRole(gData.Role))
                    {
                        var snipeZones = ZoneCache.GetSniperZones();
                        var validSnipeZones = new List<BotZone>();
                        for (int zIdx = 0; zIdx < snipeZones.Count; zIdx++)
                        {
                            var sz = snipeZones[zIdx];
                            if (sz == null) continue;
                            var botsInZone = _botsController.Bots.GetListByZone(sz);
                            bool hasAliveSniper = false;
                            if (botsInZone != null)
                            {
                                for (int bIdx = 0; bIdx < botsInZone.Count; bIdx++)
                                {
                                    var b = botsInZone[bIdx];
                                    if (b != null && b.HealthController != null && b.HealthController.IsAlive &&
                                        b.Profile?.Info?.Settings != null && SpawnPointHelper.IsSniperRole(b.Profile.Info.Settings.Role))
                                    {
                                        hasAliveSniper = true;
                                        break;
                                    }
                                }
                            }
                            if (!hasAliveSniper)
                            {
                                validSnipeZones.Add(sz);
                            }
                        }

                        if (validSnipeZones.Count > 0)
                        {
                            selectedZone = validSnipeZones[UnityEngine.Random.Range(0, validSnipeZones.Count)];
                        }
                    }
                    else
                    {
                        var regularZones = ZoneCache.GetRegularZones();
                        float maxDistBuffer = 300f + 50f;
                        var mapSettings = MapNameHelper.GetMapSettings(_serverConfig, mapName);
                        if (mapSettings != null)
                        {
                            maxDistBuffer = mapSettings.SpawnBubbleDistance + 50f;
                        }
                        float maxDistBufferSq = maxDistBuffer * maxDistBuffer;

                        var playersList = _gameWorld.AllAlivePlayersList;
                        var aliveHumans = new List<Player>();
                        if (playersList != null)
                        {
                            for (int plIdx = 0; plIdx < playersList.Count; plIdx++)
                            {
                                var p = playersList[plIdx];
                                if (p == null || p.Profile == null) continue;
                                if (p.IsAI && !p.IsYourPlayer) continue;
                                if (IsHeadlessPlayer(p)) continue;
                                aliveHumans.Add(p);
                            }
                        }

                        var roughZones = new List<BotZone>();
                        for (int zIdx = 0; zIdx < regularZones.Count; zIdx++)
                        {
                            var z = regularZones[zIdx];
                            if (z == null) continue;

                            if (aliveHumans.Count == 0)
                            {
                                roughZones.Add(z);
                                continue;
                            }

                            Vector3 zoneCenter = z.transform.position;
                            bool isWithinBuffer = false;
                            for (int hIdx = 0; hIdx < aliveHumans.Count; hIdx++)
                            {
                                Vector3 dPos = aliveHumans[hIdx].Position - zoneCenter;
                                if (dPos.sqrMagnitude <= maxDistBufferSq)
                                {
                                    isWithinBuffer = true;
                                    break;
                                }
                            }
                            if (isWithinBuffer)
                            {
                                roughZones.Add(z);
                            }
                        }

                        if (roughZones.Count > 0)
                        {
                            Player mainPlayer = _gameWorld.MainPlayer;
                            List<BotZone> candidateZones = roughZones;

                            if (mainPlayer != null)
                            {
                                Vector3 moveDir = (mainPlayer.MovementContext != null && mainPlayer.MovementContext.Velocity.sqrMagnitude > 0.1f)
                                                  ? mainPlayer.MovementContext.Velocity.normalized
                                                  : mainPlayer.LookDirection;

                                var forwardZones = new List<BotZone>();
                                for (int rIdx = 0; rIdx < roughZones.Count; rIdx++)
                                {
                                    var rz = roughZones[rIdx];
                                    Vector3 dirToZone = (rz.transform.position - mainPlayer.Position).normalized;
                                    if (Vector3.Dot(moveDir, dirToZone) > 0f)
                                    {
                                        forwardZones.Add(rz);
                                    }
                                }

                                if (forwardZones.Count > 0)
                                {
                                    candidateZones = forwardZones;
                                }
                            }

                            var nonRecentZones = new List<BotZone>();
                            for (int cIdx = 0; cIdx < candidateZones.Count; cIdx++)
                            {
                                var cz = candidateZones[cIdx];
                                if (cz != _lastSelectedZone)
                                {
                                    nonRecentZones.Add(cz);
                                }
                            }

                            var poolToPick = nonRecentZones.Count > 0 ? nonRecentZones : candidateZones;
                            BotZone validatedZone = null;

                            // Sorteia e valida com IsValidSpawnZone
                            int startIndex = UnityEngine.Random.Range(0, poolToPick.Count);
                            for (int tryCount = 0; tryCount < poolToPick.Count; tryCount++)
                            {
                                int idx = (startIndex + tryCount) % poolToPick.Count;
                                var candidate = poolToPick[idx];
                                if (IsValidSpawnZone(candidate, mapName, gData.Role))
                                {
                                    validatedZone = candidate;
                                    break;
                                }
                            }

                            selectedZone = validatedZone ?? poolToPick[startIndex];
                            _lastSelectedZone = selectedZone;
                        }
                        else
                        {
                            selectedZone = TRLDynamicSpawn.Helpers.Methods.GetRandomZone(_botsController.BotSpawner, SpawnPointHelper.IsSniperRole(gData.Role));
                            _lastSelectedZone = selectedZone;
                        }
                    }
                }

                if (selectedZone != null)
                {
                    yield return StartCoroutine(SpawnGroupBotsCoroutine(gData.Role, gData.Difficulty, gData.GroupSize, selectedZone));
                }
                else
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] FAILED: Could not find any safe/LoS-free zone in the map for this bot group. Dropping group.");
                }

                if (TRLDynamicSpawn.Helpers.Settings.enableSmoothSpawning.Value)
                {
                    yield return new WaitForSeconds(TRLDynamicSpawn.Helpers.Settings.smoothSpawningDelay.Value);
                }
                else
                {
                    yield return null; // Just wait 1 frame to not lock main thread
                }
            } // End foreach
        }

        public class SpawnGroupData
        {
            public WildSpawnType Role;
            public BotDifficulty Difficulty;
            public int GroupSize;
            public EliteLocationInfo Info;
        }

        private IEnumerator SpawnGroupBotsCoroutine(WildSpawnType role, BotDifficulty diff, int groupSize, BotZone zone)
        {
            if (groupSize <= 0) yield break;
            if (zone == null)
            {
                zone = TRLDynamicSpawn.Helpers.Methods.GetRandomZone(_botsController?.BotSpawner);
                if (zone == null) yield break;
            }

            bool isSain = IsSainInstalled();
            if (Settings.enableDebugLogs.Value)   // ref: AUD-01-07
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn][SPY] SQUAD SPAWN INITIATED: Role={role}, Difficulty={diff}, GroupSize={groupSize}, Zone={zone.NameZone}, SAIN Active={isSain}");

            EPlayerSide side = EPlayerSide.Savage;
            if (role == WildSpawnType.pmcUSEC) side = EPlayerSide.Usec;
            else if (role == WildSpawnType.pmcBEAR) side = EPlayerSide.Bear;

            BotSpawnParams spawnParams = new BotSpawnParams();
            var botProfile = new BotProfileDataClass(side, role, diff, 0f, spawnParams);

            // ref: AUD-02-01 — Geração atômica de esquadrão em uma única Task assíncrona
            var task = BotCreationDataClass.Create(botProfile, _botCreator, groupSize, _botsController.BotSpawner);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            BotCreationDataClass botResult = task.Result;
            if (botResult == null)
            {
                yield return new WaitForSeconds(0.1f);
                var retryTask = BotCreationDataClass.Create(botProfile, _botCreator, groupSize, _botsController.BotSpawner);
                while (!retryTask.IsCompleted)
                {
                    yield return null;
                }
                botResult = retryTask.Result;
            }

            if (botResult == null && diff != BotDifficulty.normal)
            {
                Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn][SPY-FALLBACK] Profile creation returned NULL for {role} at difficulty {diff}! Triggering safety fallback to BotDifficulty.normal for group size {groupSize}...");
                var fallbackProfile = new BotProfileDataClass(side, role, BotDifficulty.normal, 0f, spawnParams);
                var fallbackTask = BotCreationDataClass.Create(fallbackProfile, _botCreator, groupSize, _botsController.BotSpawner);
                while (!fallbackTask.IsCompleted)
                {
                    yield return null;
                }
                botResult = fallbackTask.Result;
            }

            if (botResult != null && botResult.Profiles != null && botResult.Profiles.Count > 0)
            {
                if (Settings.enableDebugLogs.Value)   // ref: AUD-01-07
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn][SPY] SQUAD ATOMIC SPAWN: {role} ({diff}) Size={botResult.Profiles.Count} in {zone.NameZone}");
                
                IsGeneratingDynamicWave = true;
                try
                {
                    _botsController.BotSpawner.TryToSpawnInZoneAndDelay(zone, botResult, false, true, null, true);
                }
                finally
                {
                    IsGeneratingDynamicWave = false;
                }
            }
            else
            {
                Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] Bot profile creation skipped for {role} in zone '{zone?.NameZone ?? "Unknown"}'. Member safely skipped.");
            }
        }

        public bool IsValidSpawnZone(BotZone zone, string mapName, WildSpawnType? role = null, Vector3? overridePosition = null, double? overrideSafeDistance = null)
        {
            if (zone == null && overridePosition == null) return false;

            var mapSettings = MapNameHelper.GetMapSettings(_serverConfig, mapName);
            double safeDist = overrideSafeDistance ?? (mapSettings != null ? mapSettings.SafeZoneDistance : (mapName == "factory4_day" || mapName == "factory4_night" || mapName == "sandbox" || mapName == "sandbox_high" ? 15.0 : 30.0));
            bool enableLos = TRLDynamicSpawn.Helpers.Settings.enableLoSCulling.Value;
            float losDist = TRLDynamicSpawn.Helpers.Settings.losCullingDistance.Value;

            var playersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
            if (playersList == null || playersList.Count == 0) return true;

            var players = new List<Player>();
            foreach (var p in playersList)
            {
                if (p == null || p.Profile == null || p.Profile.Info == null) continue;
                if (p.IsAI && !p.IsYourPlayer) continue;
                if (IsHeadlessPlayer(p)) continue;

                players.Add(p);
            }

            if (players.Count == 0) return true;

            // Use the override custom position if provided, else the BotZone's center
            Vector3 zonePos = overridePosition ?? zone.transform.position;

            // Se for bot comum (PMC ou Scav), ele deve estar dentro da bolha de spawn
            if (role != null)
            {
                WildSpawnType rType = role.Value;
                bool isCommonBot = rType == WildSpawnType.pmcUSEC || rType == WildSpawnType.pmcBEAR || rType == WildSpawnType.assault;
                if (isCommonBot)
                {
                    float maxDist = 300f;
                    bool isBubbleEnabled = TRLDynamicSpawn.Helpers.Settings.enableSpawnBubble.Value;

                    if (mapSettings != null)
                    {
                        isBubbleEnabled = isBubbleEnabled && mapSettings.EnableSpawnBubble;
                        maxDist = mapSettings.SpawnBubbleDistance;
                    }

                    if (isBubbleEnabled)
                    {
                        float maxDistSq = maxDist * maxDist;
                        bool closeEnoughToAnyPlayer = false;
                        for (int pIdx = 0; pIdx < players.Count; pIdx++)
                        {
                            var player = players[pIdx];
                            if (player == null || player.Profile == null || player.Profile.Info == null) continue;
                            if (player.IsAI && !player.IsYourPlayer) continue;

                            Vector3 diff = player.Position - zonePos;
                            if (diff.sqrMagnitude <= maxDistSq)
                            {
                                closeEnoughToAnyPlayer = true;
                                break;
                            }
                        }
                        if (!closeEnoughToAnyPlayer)
                        {
                            return false; // Fora do raio da bolha, inválida
                        }
                    }
                }
            }

            float heightLimit = (mapName.Contains("factory") || mapName.Contains("sandbox") || mapName.Contains("laboratory") || mapName.Contains("interchange") || mapName.Contains("tarkovstreets") || mapName.Contains("shoreline") || mapName.Contains("rezervbase")) ? 4.0f : 15.0f;

            foreach (var player in players)
            {
                if (player == null || player.Profile == null || player.Profile.Info == null) continue;
                if (player.IsAI && !player.IsYourPlayer) continue;

                // Safe Zone Distance Check (Losango 3D / Bipirâmide Otimizado sem alocação de Heap)
                float dx = player.Position.x - zonePos.x;
                float dz = player.Position.z - zonePos.z;
                float dh = (float)System.Math.Sqrt(dx * dx + dz * dz);
                float dv = System.Math.Abs(player.Position.y - zonePos.y);

                float limitW = System.Math.Max((float)safeDist, 5f);
                float limitH = System.Math.Max(heightLimit, 3f);

                if ((dh / limitW) + (dv / limitH) <= 1.0f)
                {
                    return false;
                }
            }
            return true;
        }


        private BotZone GetHotzone(EliteLocationInfo info, string mapName)
        {
            if (info == null) return null;
            int chance = GetChanceForMap(info, mapName);
            if (chance > 0 && UnityEngine.Random.Range(1, 101) <= chance)
            {
                string zonesString = GetZonesForMap(info, mapName);
                if (!string.IsNullOrEmpty(zonesString))
                {
                    string[] possibleZones = zonesString.Split(',').Select(z => z.Trim()).Where(z => !string.IsNullOrEmpty(z)).ToArray();
                    if (possibleZones.Length > 0)
                    {
                        string selectedZoneName = possibleZones[UnityEngine.Random.Range(0, possibleZones.Length)];
                        var allZones = LocationScene.GetAllObjects<BotZone>();
                        return allZones != null ? allZones.FirstOrDefault(z => z.NameZone == selectedZoneName) : null;
                    }
                }
            }
            return null;
        }

        private int GetChanceForMap(EliteLocationInfo info, string mapName)
        {
            switch (mapName)
            {
                case "bigmap": return info.SpawnChance.Customs;
                case "factory4_day":
                case "factory4_night": return info.SpawnChance.Factory4Day;
                case "interchange": return info.SpawnChance.Interchange;
                case "laboratory": return info.SpawnChance.Laboratory;
                case "lighthouse": return info.SpawnChance.Lighthouse;
                case "rezervbase": return info.SpawnChance.Reserve;
                case "shoreline": return info.SpawnChance.Shoreline;
                case "tarkovstreets": return info.SpawnChance.TarkovStreets;
                case "woods": return info.SpawnChance.Woods;
                case "sandbox":
                case "sandbox_high": return info.SpawnChance.GroundZero;
                default: return 0;
            }
        }

        private string GetZonesForMap(EliteLocationInfo info, string mapName)
        {
            switch (mapName)
            {
                case "bigmap": return info.BossZone.Customs;
                case "factory4_day":
                case "factory4_night": return info.BossZone.Factory4Day;
                case "interchange": return info.BossZone.Interchange;
                case "laboratory": return info.BossZone.Laboratory;
                case "lighthouse": return info.BossZone.Lighthouse;
                case "rezervbase": return info.BossZone.Reserve;
                case "shoreline": return info.BossZone.Shoreline;
                case "tarkovstreets": return info.BossZone.TarkovStreets;
                case "woods": return info.BossZone.Woods;
                case "sandbox":
                case "sandbox_high": return info.BossZone.GroundZero;
                default: return "";
            }
        }

        private void AdjustVanillaBossWaves()
        {
            try
            {
                var game = Singleton<IBotGame>.Instance;
                if (game == null || game.BossSpawnScenario == null || game.BossSpawnScenario.BossSpawnWaves == null)
                {
                    Plugin.LogSource.LogWarning("[TRL-DynamicSpawn] BossSpawnScenario or BossSpawnWaves is null. Cannot adjust vanilla boss waves.");
                    return;
                }

                string mapName = GetCurrentMapName().ToLower();
                if (string.IsNullOrEmpty(mapName)) return;

                var waves = game.BossSpawnScenario.BossSpawnWaves;
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Adjusting {waves.Length} vanilla boss waves using server configurations...");

                foreach (var wave in waves)
                {
                    if (wave == null || wave.BossName == null) continue;

                    string bossNameLower = wave.BossName.ToLower();

                    EliteLocationInfo bossInfo = null;

                    if (bossNameLower == "bossknight") bossInfo = _serverConfig?.EliteConfig?.BossKnight;
                    else if (bossNameLower == "bosstagilla") bossInfo = _serverConfig?.EliteConfig?.BossTagilla;
                    else if (bossNameLower == "bosskilla") bossInfo = _serverConfig?.EliteConfig?.BossKilla;
                    else if (bossNameLower == "bosszryachiy") bossInfo = _serverConfig?.EliteConfig?.BossZryachiy;
                    else if (bossNameLower == "bossgluhar") bossInfo = _serverConfig?.EliteConfig?.BossGluhar;
                    else if (bossNameLower == "bosssanitar") bossInfo = _serverConfig?.EliteConfig?.BossSanitar;
                    else if (bossNameLower == "bosskolontay") bossInfo = _serverConfig?.EliteConfig?.BossKolontay;
                    else if (bossNameLower == "bossreshala") bossInfo = _serverConfig?.EliteConfig?.BossReshala;
                    else if (bossNameLower == "bosskaban") bossInfo = _serverConfig?.EliteConfig?.BossKaban;
                    else if (bossNameLower == "bossshturman") bossInfo = _serverConfig?.EliteConfig?.BossShturman;
                    else if (bossNameLower == "bosspartisan") bossInfo = _serverConfig?.EliteConfig?.BossPartisan;
                    else if (bossNameLower == "pmcbot") bossInfo = _serverConfig?.EliteConfig?.Raiders;
                    else if (bossNameLower == "arenafighterevent") bossInfo = _serverConfig?.EliteConfig?.Bloodhounds;
                    else if (bossNameLower == "sectantpriest") bossInfo = _serverConfig?.EliteConfig?.Cultists;
                    else if (bossNameLower == "gifter") bossInfo = _serverConfig?.EliteConfig?.BossGifter;
                    else if (bossNameLower == "exusec") bossInfo = _serverConfig?.EliteConfig?.Rogues;

                    if (bossInfo != null)
                    {
                        // Regra Vanilla: Se for wave de Cultistas nativos e a raid for diurna, desativa o spawn nativo
                        if (bossNameLower == "sectantpriest" && !IsNightTimeForCultists())
                        {
                            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Disabling native Cultists (sectantpriest) wave on {mapName} due to day time.");
                            wave.BossChance = 0;
                            continue;
                        }

                        int spawnChance = GetBossChanceForMap(bossInfo.SpawnChance, mapName);
                        string spawnZones = GetBossZoneForMap(bossInfo.BossZone, mapName);

                        wave.BossChance = (float)spawnChance;
                        if (!string.IsNullOrEmpty(spawnZones))
                        {
                            wave.BossZone = spawnZones;
                        }

                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Configured vanilla boss wave {wave.BossName}: Chance={wave.BossChance}%, Zones='{wave.BossZone}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error adjusting vanilla boss waves: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private bool HasNativeVanillaWave(string bossNameLower)
        {
            var game = Singleton<IBotGame>.Instance;
            if (game == null || game.BossSpawnScenario == null || game.BossSpawnScenario.BossSpawnWaves == null)
                return false;

            return game.BossSpawnScenario.BossSpawnWaves.Any(w => w != null && w.BossName != null && w.BossName.ToLower() == bossNameLower);
        }

        private bool IsNightTimeForCultists()
        {
            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null || gameWorld.GameDateTime == null) return true;

                DateTime currentInRaidTime = gameWorld.GameDateTime.Calculate();
                int hour = currentInRaidTime.Hour;

                // Regra Vanilla Tarkov: Cultistas só nascem de noite (22:00 às 06:00)
                return hour >= 22 || hour < 6;
            }
            catch
            {
                return true;
            }
        }

        private BotZone GetZoneFromConfig(EliteLocationInfo info, string mapName, WildSpawnType role = WildSpawnType.assault)
        {
            if (info != null)
            {
                string zonesString = GetBossZoneForMap(info.BossZone, mapName);
                if (!string.IsNullOrEmpty(zonesString))
                {
                    string[] possibleZones = zonesString.Split(',').Select(z => z.Trim()).Where(z => !string.IsNullOrEmpty(z)).ToArray();
                    if (possibleZones.Length > 0)
                    {
                        string selectedZoneName = possibleZones[UnityEngine.Random.Range(0, possibleZones.Length)];
                        var allZones = LocationScene.GetAllObjects<BotZone>();
                        var zone = allZones != null ? allZones.FirstOrDefault(z => z != null && string.Equals(z.NameZone, selectedZoneName, StringComparison.OrdinalIgnoreCase)) : null;
                        if (zone != null) return zone;
                    }
                }
            }

            // Regra Especial: Zryachiy fora de Lighthouse procura preferencialmente uma SnipeZone no mapa (ex: Pedra do Sniper em Woods)
            if (role == WildSpawnType.bossZryachiy && mapName.ToLower() != "lighthouse")
            {
                try
                {
                    var allZones = LocationScene.GetAllObjects<BotZone>();
                    var snipeZones = allZones != null ? allZones.Where(z => z != null && z.SnipeZone).ToList() : null;
                    if (snipeZones != null && snipeZones.Count > 0)
                    {
                        BotZone selectedSnipeZone = snipeZones[UnityEngine.Random.Range(0, snipeZones.Count)];
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Zryachiy Sniper Rule: Assigned SnipeZone '{selectedSnipeZone.NameZone}' on {mapName}.");
                        return selectedSnipeZone;
                    }
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error searching SnipeZone for Zryachiy: {ex.Message}");
                }
            }

            return TRLDynamicSpawn.Helpers.Methods.GetRandomZone(_botsController?.BotSpawner);
        }

        private int GetBossChanceForMap(ValidLocationInt chance, string mapName)
        {
            if (chance == null) return 0;
            string name = mapName.ToLower();
            if (name.Contains("factory4_day")) return chance.Factory4Day;
            if (name.Contains("factory4_night")) return chance.Factory4Night;
            if (name.Contains("bigmap") || name.Contains("customs")) return chance.Customs;
            if (name.Contains("woods")) return chance.Woods;
            if (name.Contains("shoreline")) return chance.Shoreline;
            if (name.Contains("interchange")) return chance.Interchange;
            if (name.Contains("rezerv")) return chance.Reserve;
            if (name.Contains("lighthouse")) return chance.Lighthouse;
            if (name.Contains("tarkovstreets")) return chance.TarkovStreets;
            if (name.Contains("sandbox_high")) return chance.GroundZeroHigh;
            if (name.Contains("sandbox")) return chance.GroundZero;
            if (name.Contains("laboratory")) return chance.Laboratory;
            return 0;
        }

        private int GetBossGroupSizeForMap(EliteLocationInfo info, string mapName)
        {
            if (info == null) return 3;
            if (info.MaxGroupSizeByMap != null)
            {
                int val = GetBossChanceForMap(info.MaxGroupSizeByMap, mapName);
                if (val > 0) return val;
            }
            return info.MaxGroupSize > 0 ? info.MaxGroupSize : 3;
        }

        private string GetBossZoneForMap(ValidLocationString zone, string mapName)
        {
            if (zone == null) return "";
            string name = mapName.ToLower();
            if (name.Contains("factory4_day")) return zone.Factory4Day;
            if (name.Contains("factory4_night")) return zone.Factory4Night;
            if (name.Contains("bigmap") || name.Contains("customs")) return zone.Customs;
            if (name.Contains("woods")) return zone.Woods;
            if (name.Contains("shoreline")) return zone.Shoreline;
            if (name.Contains("interchange")) return zone.Interchange;
            if (name.Contains("rezerv")) return zone.Reserve;
            if (name.Contains("lighthouse")) return zone.Lighthouse;
            if (name.Contains("tarkovstreets")) return zone.TarkovStreets;
            if (name.Contains("sandbox_high")) return zone.GroundZeroHigh;
            if (name.Contains("sandbox")) return zone.GroundZero;
            if (name.Contains("laboratory")) return zone.Laboratory;
            return "";
        }

        private bool _showDebugUI = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                _showDebugUI = !_showDebugUI;
            }
        }

        private void OnGUI()
        {
            if (!_showDebugUI || _gameWorld == null) return;
            
            // Info Setup
            Dictionary<string, int> factionCounts = new Dictionary<string, int>();
            var allAlive = _gameWorld.RegisteredPlayers.Where(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive).ToList();
            
            foreach (var bot in allAlive)
            {
                if (bot.Profile == null) continue;
                
                string roleName = "Outros";
                if (bot.Profile.Side == EPlayerSide.Bear)
                {
                    roleName = "BEAR";
                }
                else if (bot.Profile.Side == EPlayerSide.Usec)
                {
                    roleName = "USEC";
                }
                else if (bot.Profile.Info != null && bot.Profile.Info.Settings != null)
                {
                    var role = bot.Profile.Info.Settings.Role;
                    
                    switch (role)
                    {
                        case WildSpawnType.assault: roleName = "Scav"; break;
                        case WildSpawnType.marksman: roleName = "Sniper Scav"; break;
                        case WildSpawnType.exUsec: roleName = "Rogue"; break;
                        case WildSpawnType.pmcBot: roleName = "Raider"; break;
                        case WildSpawnType.arenaFighterEvent: 
                        case WildSpawnType.arenaFighter: roleName = "Bloodhound"; break;
                        case WildSpawnType.sectantPriest:
                        case WildSpawnType.sectantWarrior:
                        case WildSpawnType.sectantOni:
                        case WildSpawnType.sectantPredvestnik:
                        case WildSpawnType.sectantPrizrak: roleName = "Cultist"; break;
                        default:
                            string rawName = role.ToString();
                            if (rawName.StartsWith("boss", System.StringComparison.OrdinalIgnoreCase) || 
                                rawName.StartsWith("follower", System.StringComparison.OrdinalIgnoreCase))
                            {
                                roleName = "Bosses/Guards";
                            }
                            else if (int.TryParse(rawName, out int numericVal))
                            {
                                roleName = $"Outros (Mod {numericVal})";
                            }
                            else
                            {
                                roleName = rawName;
                            }
                            break;
                    }
                }

                if (!factionCounts.ContainsKey(roleName))
                {
                    factionCounts[roleName] = 0;
                }
                factionCounts[roleName]++;
            }

            // Layout dimensions
            float boxWidth = 360;
            float boxHeight = Mathf.Max(300, 200 + (factionCounts.Count * 22));
            float margin = 20;

            Rect rect = new Rect(margin, margin, boxWidth, boxHeight);
            GUI.Box(rect, "TRL-DynamicSpawn - Developer HUD");

            if (string.IsNullOrEmpty(_cachedFikaStatus))
            {
                _cachedFikaStatus = "Solo";
                try
                {
                    var type = System.Type.GetType("Fika.Core.Main.Utils.FikaBackendUtils, Fika.Core");
                    if (type != null)
                    {
                        var isServerProp = type.GetProperty("IsServer", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var isSingleProp = type.GetProperty("IsSinglePlayer", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        
                        if (isServerProp != null && isSingleProp != null)
                        {
                            bool isServer = (bool)isServerProp.GetValue(null);
                            bool isSingle = (bool)isSingleProp.GetValue(null);

                            if (!isSingle)
                                _cachedFikaStatus = isServer ? "Fika Host" : "Fika Client";
                        }
                    }
                }
                catch { }
            }

            float nextSpawnDelay = Mathf.Max(0, _nextWaveTime - Time.time);
            bool isClient = _cachedFikaStatus == "Fika Client";

            // Draw Texts
            GUILayout.BeginArea(new Rect(margin + 10, margin + 25, boxWidth - 20, boxHeight - 30));
            
            GUILayout.Label($"<b>Status de Sessão:</b> {_cachedFikaStatus}");
            GUILayout.Label($"<b>Config Preset:</b> {_activePreset}");
            
            string hudMapName = GetCurrentMapName();
            int hudPlayerCap = Settings.GetMapCap(hudMapName);
            int hudSpecialBots = GetSpecialBotsCount();
            int hudMaxBotCap = hudPlayerCap + hudSpecialBots;
            GUILayout.Label($"<b>Máximo de Bots:</b> {hudPlayerCap} (Config) + {hudSpecialBots} (Especiais) = <color=yellow>{hudMaxBotCap}</color>");
            GUILayout.Space(10);
            
            if (isClient)
            {
                GUI.color = Color.cyan;
                GUILayout.Label($"<b>Próxima Wave em:</b> Controlado pelo Host");
            }
            else
            {
                GUI.color = nextSpawnDelay < 10f ? Color.red : Color.white;
                GUILayout.Label($"<b>Próxima Wave em:</b> {Mathf.CeilToInt(nextSpawnDelay)}s");
            }
            GUI.color = Color.white;
            
            GUILayout.Space(10);
            GUILayout.Label($"<b>População Ativa (Total {allAlive.Count}):</b>");
            
            foreach (var kvp in factionCounts.OrderByDescending(k => k.Value))
            {
                GUILayout.Label($"- {kvp.Key}: <color=yellow>{kvp.Value}</color>");
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=grey>[F12] Ocultar Painel</color>");
            
            GUILayout.EndArea();
        }

        private bool IsFikaClient()
        {
            // Se estiver em batchmode (Headless Host dedicado), ele DEVE atuar como servidor/host e rodar o loop de spawn
            if (UnityEngine.Application.isBatchMode)
            {
                return false;
            }

            try
            {
                var assembly = System.AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Fika.Core");
                if (assembly != null)
                {
                    var type = assembly.GetType("Fika.Core.Main.Utils.FikaBackendUtils");
                    if (type != null)
                    {
                        var prop = type.GetProperty("IsClient", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (prop != null)
                        {
                            return (bool)prop.GetValue(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error checking Fika client status via reflection: {ex.Message}");
            }
            return false;
        }

        private static bool _fikaReflectionEvaluated = false;
        private static System.Reflection.PropertyInfo _fikaIsHeadlessProperty = null;

        public static string SanitizeMongoId(string input)
        {
            if (string.IsNullOrEmpty(input)) return MongoID.Generate();
            string trimmed = input.Trim().ToLowerInvariant();
            if (trimmed.Length == 24 && IsValidHex24(trimmed))
            {
                return trimmed;
            }
            if (trimmed.Length > 24) return trimmed.Substring(0, 24);
            return trimmed.PadRight(24, '0');
        }

        private static bool IsValidHex24(string s)
        {
            for (int i = 0; i < 24; i++)
            {
                char c = s[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if a player is the Headless Server host instance.
        /// References: fika-plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs (IsHeadless)
        /// and fika-headless/Fika.Headless/Patches/ConsoleScreen_OnProfileReceive_Patch.cs (headless_).
        /// </summary>
        public static bool IsHeadlessPlayer(Player p)
        {
            if (p == null) return false;

            if (p.IsYourPlayer)
            {
                if (UnityEngine.Application.isBatchMode) return true;

                if (!_fikaReflectionEvaluated)
                {
                    _fikaReflectionEvaluated = true;
                    try
                    {
                        var assembly = System.AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == "Fika.Core");
                        if (assembly != null)
                        {
                            var type = assembly.GetType("Fika.Core.Main.Utils.FikaBackendUtils");
                            if (type != null)
                            {
                                _fikaIsHeadlessProperty = type.GetProperty("IsHeadless", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                            }
                        }
                    }
                    catch { }
                }

                if (_fikaIsHeadlessProperty != null)
                {
                    try
                    {
                        if ((bool)_fikaIsHeadlessProperty.GetValue(null)) return true;
                    }
                    catch { }
                }
            }

            if (p.Profile != null)
            {
                // ref: CR-01-05 — ordinal, allocation-free (called once per human per second by GetAliveHumanCount)
                string nickname = p.Profile.Nickname;
                string accountId = p.Profile.AccountId;
                if ((nickname != null && nickname.IndexOf("headless", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (accountId != null && accountId.IndexOf("headless", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
