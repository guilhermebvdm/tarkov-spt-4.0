using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
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
        
        private bool _isSpawningWave = false;
        private int _delayBeforeFirstWave = 60;
        private int _secondsBetweenWaves = 360; 
        private float _nextWaveTime = 0f;
        
        private string _activePreset = "Balanced";
        private TRLConfig _serverConfig;
        public TRLConfig ServerConfig => _serverConfig;
        
        // Caching settings
        private GameWorld _gameWorld;
        private IBotCreator _botCreator;
        private BotsController _botsController;

        public void Init(GameWorld gameWorld, IBotCreator botCreator, BotsController botsController)
        {
            Instance = this;
            _gameWorld = gameWorld;
            _botCreator = botCreator;
            _botsController = botsController;

            StartCoroutine(FetchServerConfigAndStart());
        }

        public static Dictionary<string, List<Vector3Model>> PmcSpawns = new();
        public static Dictionary<string, List<Vector3Model>> ScavSpawns = new();
        
        private IEnumerator FetchServerConfigAndStart()
        {
            if (IsFikaClient())
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Client peer detected. Spawning is managed by the host. Disabling manager loop.");
                yield break;
            }

            try 
            {
                string json = RequestHandler.GetJson("/trldynamicspawn/getConfig");
                _serverConfig = JsonConvert.DeserializeObject<TRLConfig>(json);
                _activePreset = _serverConfig.ActivePreset;

                // Handle Random Preset Selection
                if (_activePreset == "Random")
                {
                    string[] availablePresets = { "Balanced", "PMC War", "Scav Infestation", "Quiet Raid", "Warzone" };
                    _activePreset = availablePresets[UnityEngine.Random.Range(0, availablePresets.Length)];
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Aleatorio rolled preset: {_activePreset}");
                }

                _delayBeforeFirstWave = _serverConfig.MapTimers["global"].DelayBeforeFirstWave;
                _secondsBetweenWaves = _serverConfig.MapTimers["global"].SecondsBetweenWaves;
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Connected to Server. Active Preset: {_activePreset}");

                // Apply Preset Modifiers
                if (_activePreset == "Quiet Raid")
                {
                    _secondsBetweenWaves = (int)(_secondsBetweenWaves * 1.5f);
                    if (_serverConfig.EliteConfig != null)
                    {
                        _serverConfig.EliteConfig.Usec.MaxGroupSize = Mathf.Min(_serverConfig.EliteConfig.Usec.MaxGroupSize, 2);
                        _serverConfig.EliteConfig.Bear.MaxGroupSize = Mathf.Min(_serverConfig.EliteConfig.Bear.MaxGroupSize, 2);
                        _serverConfig.EliteConfig.Scav.MaxGroupSize = Mathf.Min(_serverConfig.EliteConfig.Scav.MaxGroupSize, 2);
                    }
                }
                else if (_activePreset == "Warzone")
                {
                    _secondsBetweenWaves = (int)(_secondsBetweenWaves * 0.5f);
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
                    Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Pre-populating SPT Bot Creator Backup target for PMCs...");
                    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcUSEC, 30);
                    _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcBEAR, 30);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error fetching config from server: {ex.Message}");
                _serverConfig = new TRLConfig(); // Fallback
            }
            
            yield return StartCoroutine(SpawnHordeLoop());
        }

        private IEnumerator SpawnHordeLoop()
        {
            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Waiting {_delayBeforeFirstWave}s for initial warmup...");
            _nextWaveTime = Time.time + _delayBeforeFirstWave;
            yield return new WaitForSeconds(_delayBeforeFirstWave);

            bool isFirstWave = true;
            while (true)
            {
                if (!_isSpawningWave)
                {
                    StartCoroutine(ProcessWave(isFirstWave));
                    isFirstWave = false;
                }
                
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Wave completed. Next wave in {_secondsBetweenWaves}s...");
                _nextWaveTime = Time.time + _secondsBetweenWaves;
                yield return new WaitForSeconds(_secondsBetweenWaves);
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


        private IEnumerator ProcessWave(bool isFirstWave)
        {
            _isSpawningWave = true;

            int maxCap = Settings.GetMapCap(Singleton<GameWorld>.Instance.MainPlayer.Location);
            int aliveBots = _botsController.AliveAndLoadingBotsCount;
            int availableSlots = maxCap - aliveBots;

            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Calculating Wave: MaxCap={maxCap}, Alive={aliveBots}, Available={availableSlots}");

            if (_botCreator != null)
            {
                _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcUSEC, 10);
                _botCreator.AddToTargetBackup(BotDifficulty.normal, WildSpawnType.pmcBEAR, 10);
            }

            if (availableSlots <= 0)
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] No slots available for this wave. Skipping.");
                _isSpawningWave = false;
                yield break;
            }

            List<Tuple<SpawnGroupData, BotZone>> spawnList = new List<Tuple<SpawnGroupData, BotZone>>();

            // ======================================
            // PROCESS ELITES / BOSSES
            // ======================================
            var mapName = _gameWorld.MainPlayer.Location.ToLower();
            var eliteConfig = _serverConfig?.EliteConfig;
            
            List<Tuple<BossLocationSpawn, BotZone>> bossQueue = new List<Tuple<BossLocationSpawn, BotZone>>();

            if (isFirstWave && eliteConfig != null)
            {
                var bossesToProcess = new Dictionary<WildSpawnType, EliteLocationInfo>
                {
                    { WildSpawnType.bossKnight, eliteConfig.BossKnight },
                    { WildSpawnType.bossTagilla, eliteConfig.BossTagilla },
                    { WildSpawnType.bossKilla, eliteConfig.BossKilla },
                    { WildSpawnType.bossZryachiy, eliteConfig.BossZryachiy },
                    { WildSpawnType.bossGluhar, eliteConfig.BossGluhar },
                    { WildSpawnType.bossSanitar, eliteConfig.BossSanitar },
                    { WildSpawnType.bossKolontay, eliteConfig.BossKolontay },
                    { WildSpawnType.bossBully, eliteConfig.BossReshala },
                    { WildSpawnType.bossBoar, eliteConfig.BossKaban },
                    { WildSpawnType.bossPartisan, eliteConfig.BossPartisan },
                    { WildSpawnType.bossKojaniy, eliteConfig.BossShturman },
                    { WildSpawnType.pmcBot, eliteConfig.Raiders },
                    { WildSpawnType.exUsec, eliteConfig.Rogues },
                    { WildSpawnType.arenaFighterEvent, eliteConfig.Bloodhounds },
                    { WildSpawnType.sectantPriest, eliteConfig.Cultists }
                };

                bool IsNonBoss(WildSpawnType type) => type == WildSpawnType.pmcBot || type == WildSpawnType.exUsec || type == WildSpawnType.arenaFighterEvent || type == WildSpawnType.sectantPriest;

                string invadedMap = "";
                List<WildSpawnType> invadedBosses = new List<WildSpawnType>();

                if (eliteConfig.BossInvasion != null && eliteConfig.BossInvasion.Enable)
                {
                    if (UnityEngine.Random.Range(1, 101) <= eliteConfig.BossInvasion.InvasionChance)
                    {
                        if (eliteConfig.BossInvasion.SelectedMaps.Contains("Random"))
                            invadedMap = mapName; // Simplified: Invasion hits your current map if Random rolled successfully
                        else if (eliteConfig.BossInvasion.SelectedMaps.Contains("All") || eliteConfig.BossInvasion.SelectedMaps.Contains(mapName))
                            invadedMap = mapName;

                        if (invadedMap == mapName)
                        {
                            if (eliteConfig.BossInvasion.SelectedBosses.Contains("Random"))
                            {
                                var bossKeys = bossesToProcess.Keys.Where(k => !IsNonBoss(k)).ToList();
                                invadedBosses.Add(bossKeys[UnityEngine.Random.Range(0, bossKeys.Count)]);
                            }
                            else if (eliteConfig.BossInvasion.SelectedBosses.Contains("All"))
                            {
                                invadedBosses.AddRange(bossesToProcess.Keys.Where(k => !IsNonBoss(k)));
                            }
                            else
                            {
                                foreach (var bStr in eliteConfig.BossInvasion.SelectedBosses)
                                {
                                    if (Enum.TryParse(bStr, out WildSpawnType bType)) invadedBosses.Add(bType);
                                }
                            }
                            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] BOSS INVASION TRIGGERED! Bosses: {string.Join(",", invadedBosses)} on {mapName}");
                        }
                    }
                }

                foreach (var kvp in bossesToProcess)
                {
                    var bossType = kvp.Key;
                    var info = kvp.Value;

                    if (info == null || !info.Enable) continue;
                    
                    bool isInvading = invadedBosses.Contains(bossType);
                    if (eliteConfig.DisableBosses && !IsNonBoss(bossType) && !isInvading) continue; // Skip if disabled unless invading
 
                    int spawnChance = isInvading ? 100 : GetChanceForMap(info, mapName);
                    if (spawnChance <= 0) continue;

                    int roll = UnityEngine.Random.Range(1, 101);
                    if (roll <= spawnChance)
                    {
                        BotZone bz = null;
                        var allZones = LocationScene.GetAllObjects<BotZone>();
                        if (allZones != null && allZones.Any())
                        {
                            if (eliteConfig.BossOpenZones || isInvading)
                            {
                                var allZonesArr = allZones.ToArray(); bz = allZonesArr[UnityEngine.Random.Range(0, allZonesArr.Length)];
                            }
                            else
                            {
                                string zonesString = GetZonesForMap(info, mapName);
                                if (!string.IsNullOrEmpty(zonesString))
                                {
                                    string[] possibleZones = zonesString.Split(',').Select(z => z.Trim()).Where(z => !string.IsNullOrEmpty(z)).ToArray();
                                    if (possibleZones.Length > 0)
                                    {
                                        string selectedZoneName = possibleZones[UnityEngine.Random.Range(0, possibleZones.Length)];
                                        bz = allZones.FirstOrDefault(z => z.NameZone == selectedZoneName);
                                    }
                                }
                            }
                        }

                        if (bz != null)
                        {
                            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Rolled SUCCESS ({roll} <= {spawnChance}) for {bossType} in {bz.NameZone} on {mapName}!");
                            
                            BossLocationSpawn newBossWave = null;

                            if (!info.DisableFollowers && Singleton<IBotGame>.Instantiated)
                            {
                                var game = Singleton<IBotGame>.Instance;
                                if (game.BossSpawnScenario != null && game.BossSpawnScenario.BossSpawnWaves != null)
                                {
                                    var origWave = game.BossSpawnScenario.BossSpawnWaves.FirstOrDefault(w => w.BossName.ToLower() == bossType.ToString().ToLower());
                                    if (origWave != null)
                                    {
                                        newBossWave = new BossLocationSpawn()
                                        {
                                            BossName = origWave.BossName,
                                            BossChance = 100f,
                                            BossZone = bz.NameZone,
                                            BossPlayer = false,
                                            BossDifficult = origWave.BossDifficult,
                                            BossEscortType = origWave.BossEscortType,
                                            BossEscortDifficult = origWave.BossEscortDifficult,
                                            BossEscortAmount = origWave.BossEscortAmount,
                                            Time = -1f,
                                            Supports = origWave.Supports,
                                            ForceSpawn = true,
                                            Delay = 0f
                                        };
                                    }
                                }
                            }

                            if (newBossWave == null)
                            {
                                // Fallback for simple boss spawn if no origWave found or followers disabled
                                newBossWave = new BossLocationSpawn()
                                {
                                    BossName = bossType.ToString(),
                                    BossChance = 100f,
                                    BossZone = bz.NameZone,
                                    BossPlayer = false,
                                    BossDifficult = BotDifficulty.normal.ToString(),
                                    BossEscortType = bossType.ToString(),
                                    BossEscortDifficult = BotDifficulty.normal.ToString(),
                                    BossEscortAmount = "0",
                                    Time = -1f,
                                    ForceSpawn = true,
                                    Delay = 0f
                                };
                            }

                            bossQueue.Add(new Tuple<BossLocationSpawn, BotZone>(newBossWave, bz));
                            
                            int count = 1;
                            if (int.TryParse(newBossWave.BossEscortAmount, out int escortCount) && escortCount > 0)
                            {
                                count += escortCount;
                            }
                            availableSlots -= count;
                        }
                    }
                }
            }

            // Group Splitting Local Function
            void GenerateAndEnqueueGroups(WildSpawnType role, BotDifficulty diff, int totalSlots, EliteLocationInfo info)
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

                    spawnList.Add(new Tuple<SpawnGroupData, BotZone>(gData, GetHotzone(info, mapName)));
                    slotsRemaining -= groupSize;
                }
            }

            // ======================================
            // PROCESS REGULAR HORDE (PMCs/Scavs)
            // ======================================
            if (availableSlots > 0)
            {
                if (isFirstWave && eliteConfig != null && eliteConfig.RandomRaiderGroup && UnityEngine.Random.Range(1, 101) <= eliteConfig.RandomRaiderGroupChance)
                {
                    int raiderSlots = Mathf.Min(availableSlots, UnityEngine.Random.Range(2, 5));
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] RANDOM RAIDER GROUP INVASION! Spawning {raiderSlots} Raiders.");
                    GenerateAndEnqueueGroups(WildSpawnType.pmcBot, BotDifficulty.normal, raiderSlots, eliteConfig.Raiders);
                    availableSlots -= raiderSlots;
                }

                if (isFirstWave && eliteConfig != null && eliteConfig.RandomRogueGroup && availableSlots > 0 && UnityEngine.Random.Range(1, 101) <= eliteConfig.RandomRogueGroupChance)
                {
                    int rogueSlots = Mathf.Min(availableSlots, UnityEngine.Random.Range(2, 5));
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] RANDOM ROGUE GROUP INVASION! Spawning {rogueSlots} Rogues.");
                    GenerateAndEnqueueGroups(WildSpawnType.exUsec, BotDifficulty.normal, rogueSlots, eliteConfig.Rogues);
                    availableSlots -= rogueSlots;
                }

                float pmcRatio = 0.5f;
                if (_activePreset == "PMC War") pmcRatio = 0.7f;
                else if (_activePreset == "Scav Infestation") pmcRatio = 0.3f;
                else if (_activePreset == "Warzone") pmcRatio = UnityEngine.Random.Range(0.2f, 0.8f);

                int alivePMCs = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive && (p.Profile.Side == EPlayerSide.Usec || p.Profile.Side == EPlayerSide.Bear));
                int aliveScavs = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive && p.Profile.Side == EPlayerSide.Savage);

                int idealPMCs = Mathf.RoundToInt(maxCap * pmcRatio);
                int idealScavs = maxCap - idealPMCs;

                int pmcSlots = Mathf.Max(0, idealPMCs - alivePMCs);
                int scavSlots = Mathf.Max(0, idealScavs - aliveScavs);

                int totalTarget = pmcSlots + scavSlots;
                if (totalTarget > availableSlots && totalTarget > 0)
                {
                    float scale = (float)availableSlots / totalTarget;
                    pmcSlots = Mathf.RoundToInt(pmcSlots * scale);
                    scavSlots = availableSlots - pmcSlots;
                }

                int bearSlots = Mathf.RoundToInt(pmcSlots * 0.5f);
                int usecSlots = pmcSlots - bearSlots;

                int pScavSlots = Mathf.RoundToInt(scavSlots * 0.5f);
                int normalScavSlots = scavSlots - pScavSlots;

                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Horde Breakdown (Preset: {_activePreset}):");
                Plugin.LogSource.LogInfo($"  PMCs: {pmcSlots} ({bearSlots} Bear, {usecSlots} Usec)");
                Plugin.LogSource.LogInfo($"  Scavs: {scavSlots} ({normalScavSlots} Normal, {pScavSlots} pScav)");

                GenerateAndEnqueueGroups(WildSpawnType.pmcUSEC, BotDifficulty.normal, usecSlots, eliteConfig?.Usec);
                GenerateAndEnqueueGroups(WildSpawnType.pmcBEAR, BotDifficulty.normal, bearSlots, eliteConfig?.Bear);
                
                int sniperCount = 0;
                int mapSniperChance = _serverConfig?.MapConfigs?.ContainsKey(mapName) == true ? _serverConfig.MapConfigs[mapName].SniperChance : 30;
                if (isFirstWave && UnityEngine.Random.Range(1, 101) <= mapSniperChance && normalScavSlots > 0)
                {
                    sniperCount = 1;
                    normalScavSlots -= 1;
                    var sniperGroup = new SpawnGroupData
                    {
                        Role = WildSpawnType.marksman,
                        Difficulty = BotDifficulty.normal,
                        GroupSize = sniperCount,
                        Info = null
                    };
                    spawnList.Add(new Tuple<SpawnGroupData, BotZone>(sniperGroup, null));
                }

                GenerateAndEnqueueGroups(WildSpawnType.assault, BotDifficulty.normal, normalScavSlots, eliteConfig?.Scav);
                GenerateAndEnqueueGroups(WildSpawnType.assault, BotDifficulty.normal, pScavSlots, eliteConfig?.Scav);
            }

            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Profiles generated. Starting smooth injection...");

            // First, inject Bosses in their designated zones
            foreach (var bq in bossQueue)
            {
                if (_botsController.AliveAndLoadingBotsCount >= maxCap) break;
                
                BossLocationSpawn wave = bq.Item1;
                BotZone zone = bq.Item2;

                if (wave == null || zone == null) continue;

                // Tenta resolver a role do Boss
                if (!Enum.TryParse(wave.BossName, out WildSpawnType bossRole))
                {
                    // Fallback se o nome estiver ligeiramente diferente
                    if (wave.BossName.ToLower() == "bossreshala" || wave.BossName.ToLower() == "bully")
                        bossRole = WildSpawnType.bossBully;
                    else if (wave.BossName.ToLower() == "shturman" || wave.BossName.ToLower() == "bosskojaniy")
                        bossRole = WildSpawnType.bossKojaniy;
                    else if (wave.BossName.ToLower() == "kaban")
                        bossRole = WildSpawnType.bossBoar;
                    else
                        continue; // Ignora se não conseguir resolver
                }

                BotDifficulty bossDiff = BotDifficulty.normal;
                Enum.TryParse(wave.BossDifficult, out bossDiff);

                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Spawning BOSS ({bossRole}) directly in {zone.NameZone}...");
                var bossSpawnTask = DirectSpawnBots(bossRole, bossDiff, 1, zone);
                yield return new WaitUntil(() => bossSpawnTask.IsCompleted);

                // Spawna os seguidores (escort) se houver
                if (int.TryParse(wave.BossEscortAmount, out int escortCount) && escortCount > 0)
                {
                    if (Enum.TryParse(wave.BossEscortType, out WildSpawnType escortRole))
                    {
                        BotDifficulty escortDiff = BotDifficulty.normal;
                        Enum.TryParse(wave.BossEscortDifficult, out escortDiff);

                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Spawning boss followers ({escortRole}, Count: {escortCount}) directly in {zone.NameZone}...");
                        var escortSpawnTask = DirectSpawnBots(escortRole, escortDiff, escortCount, zone);
                        yield return new WaitUntil(() => escortSpawnTask.IsCompleted);
                    }
                }

                if (TRLDynamicSpawn.Helpers.Settings.enableSmoothSpawning.Value)
                {
                    yield return new WaitForSeconds(TRLDynamicSpawn.Helpers.Settings.smoothSpawningDelay.Value * 2f); // Wait a bit longer for bosses
                }
            }

            // Embaralha a lista de spawn para misturar grupos de forma balanceada (alternando Scavs/PMCs/Snipers)
            var rng = new System.Random();
            spawnList = spawnList.OrderBy(x => rng.Next()).ToList();

            // Then, inject regular horde
            foreach (var tuple in spawnList)
            {
                if (_botsController.AliveAndLoadingBotsCount >= maxCap)
                {
                    Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Max cap reached during smooth spawn. Stopping wave.");
                    break;
                }

                SpawnGroupData gData = tuple.Item1;
                BotZone preferredZone = tuple.Item2;
                
                BotZone selectedZone = preferredZone;
                int retries = 5;
                bool zoneValid = false;

                while (retries > 0)
                {
                    if (selectedZone == null)
                    {
                        if (gData.Role == WildSpawnType.marksman)
                        {
                            var snipeZones = LocationScene.GetAllObjects<BotZone>()
                                .Where(z => z.SnipeZone)
                                .ToList();

                            var emptySnipeZones = snipeZones.Where(z => 
                            {
                                var botsInZone = _botsController.Bots.GetListByZone(z);
                                return botsInZone == null || !botsInZone.Any(b => b.Profile.Info.Settings.Role == WildSpawnType.marksman && b.HealthController.IsAlive);
                            }).ToList();

                            if (emptySnipeZones.Count > 0)
                            {
                                selectedZone = emptySnipeZones[UnityEngine.Random.Range(0, emptySnipeZones.Count)];
                            }
                            else
                            {
                                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] All sniper zones occupied. Skipping sniper.");
                                break;
                            }
                        }
                        else
                        {
                            var nonSnipeZones = LocationScene.GetAllObjects<BotZone>()
                                .Where(z => !z.SnipeZone)
                                .ToList();

                            if (nonSnipeZones.Count > 0)
                            {
                                selectedZone = nonSnipeZones[UnityEngine.Random.Range(0, nonSnipeZones.Count)];
                            }
                            else
                            {
                                selectedZone = TRLDynamicSpawn.Helpers.Methods.GetRandomZone(_botsController.BotSpawner);
                            }
                        }
                    }

                    if (selectedZone != null && IsValidSpawnZone(selectedZone, mapName))
                    {
                        zoneValid = true;
                        break;
                    }
                    
                    selectedZone = null;
                    retries--;
                }

                if (zoneValid && selectedZone != null)
                {
                    var spawnTask = DirectSpawnBots(gData.Role, gData.Difficulty, gData.GroupSize, selectedZone);
                    yield return new UnityEngine.WaitUntil(() => spawnTask.IsCompleted || spawnTask.IsFaulted || spawnTask.IsCanceled);
                }
                else
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] FAILED: Could not find a safe/LoS-free zone for bot group after 5 tries. Dropping group to prevent infinite loop.");
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

            _isSpawningWave = false;
        }

        public class SpawnGroupData
        {
            public WildSpawnType Role;
            public BotDifficulty Difficulty;
            public int GroupSize;
            public EliteLocationInfo Info;
        }

        private async Task<bool> DirectSpawnBots(WildSpawnType role, BotDifficulty diff, int groupSize, BotZone zone)
        {
            if (zone == null || groupSize <= 0) return false;
            try
            {
                var botProfile = new BotProfileDataClass(EPlayerSide.Savage, role, diff, 0f);
                var creationData = await BotCreationDataClass.Create(botProfile, _botCreator, groupSize, _botsController.BotSpawner);
                if (creationData != null)
                {
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] DIRECT SPAWN SUCCESS: Spawning group (Size: {groupSize}, Type: {role}) in zone {zone.NameZone}...");
                    _botsController.BotSpawner.TryToSpawnInZoneAndDelay(zone, creationData, false, true, null, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Direct spawn failed for {role}: {ex.Message}\n{ex.StackTrace}");
            }
            return false;
        }

        private bool IsValidSpawnZone(BotZone zone, string mapName, Vector3? overridePosition = null)
        {
            if (zone == null && overridePosition == null) return false;

            double safeDist = _serverConfig?.MapConfigs?.ContainsKey(mapName) == true ? _serverConfig.MapConfigs[mapName].SafeZoneDistance : (mapName == "factory4_day" || mapName == "factory4_night" || mapName == "sandbox" || mapName == "sandbox_high" ? 15.0 : 30.0);
            bool enableLos = TRLDynamicSpawn.Helpers.Settings.enableLoSCulling.Value;
            float losDist = TRLDynamicSpawn.Helpers.Settings.losCullingDistance.Value;

            var players = Singleton<GameWorld>.Instance.AllAlivePlayersList;
            if (players == null || players.Count == 0) return true;

            // Use the override custom position if provided, else the BotZone's center
            Vector3 zonePos = overridePosition ?? zone.transform.position;

            foreach (var player in players)
            {
                if (player == null || player.Profile == null || player.Profile.Info == null) continue;
                if (player.IsAI && !player.IsYourPlayer) continue;

                float dist = Vector3.Distance(player.Position, zonePos);
                
                // Safe Zone Distance Check
                if (dist < safeDist) 
                {
                    return false;
                }

                // Line of Sight Culling Check
                if (enableLos && dist <= losDist)
                {
                    Vector3 directionToZone = (zonePos - player.Position).normalized;
                    float dot = Vector3.Dot(player.LookDirection, directionToZone);
                    
                    // If in front of the player (FOV approx 90 degrees -> Dot > 0.5)
                    if (dot > 0.5f)
                    {
                        // Raycast to check if there is a wall. If it doesn't hit a wall, the spawn is visible!
                        Vector3 headPos = player.MainParts.ContainsKey(BodyPartType.head) ? player.MainParts[BodyPartType.head].Position : player.Position + Vector3.up * 1.5f;
                        if (!Physics.Linecast(headPos, zonePos + Vector3.up * 1f, LayerMaskClass.HighPolyWithTerrainMask))
                        {
                            return false; // Visible to player!
                        }
                    }
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

        public void RequestReplacementBot(EPlayerSide side, WildSpawnType role, BotDifficulty difficulty)
        {
            StartCoroutine(SpawnReplacementBotCoroutine(side, role, difficulty));
        }

        private IEnumerator SpawnReplacementBotCoroutine(EPlayerSide side, WildSpawnType role, BotDifficulty difficulty)
        {
            BotSpawnParams spawnParams = new BotSpawnParams();
            BotProfileDataClass profileData = new BotProfileDataClass(EPlayerSide.Savage, role, difficulty, 0f, spawnParams);
            
            var t = BotCreationDataClass.Create(profileData, _botCreator, 1, _botsController.BotSpawner);
            while (!t.IsCompleted) yield return null;
            
            if (t.Result != null)
            {
                string mapName = _gameWorld.MainPlayer.Location.ToLower();
                BotZone selectedZone = null;
                int retries = 5;
                bool zoneValid = false;

                while (retries > 0)
                {
                    selectedZone = TRLDynamicSpawn.Helpers.Methods.GetRandomZone(_botsController.BotSpawner);
                    if (selectedZone != null && IsValidSpawnZone(selectedZone, mapName))
                    {
                        zoneValid = true;
                        break;
                    }
                    retries--;
                }

                if (zoneValid && selectedZone != null)
                {
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] SUCCESS: Spawning replacement bot ({role}) in {selectedZone.NameZone}...");
                    _botsController.BotSpawner.TryToSpawnInZoneAndDelay(selectedZone, t.Result, true, true, null, true);
                }
                else
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] FAILED: Could not find a safe zone for replacement bot. Dropping spawn.");
                }
            }
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
            
            // Layout dimensions
            float boxWidth = 320;
            float boxHeight = 280;
            float margin = 20;

            Rect rect = new Rect(margin, margin, boxWidth, boxHeight);
            GUI.Box(rect, "TRL-DynamicSpawn - Developer HUD");

            // Info Setup
            int pmcCount = 0;
            int scavCount = 0;
            int sniperCount = 0;
            int bossCount = 0;
            int otherCount = 0;
            
            var allAlive = _gameWorld.RegisteredPlayers.Where(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive).ToList();
            
            foreach (var bot in allAlive)
            {
                if (bot.Profile == null || bot.Profile.Info == null || bot.Profile.Info.Settings == null) continue;
                
                var role = bot.Profile.Info.Settings.Role;
                if (role == WildSpawnType.pmcBot || role == WildSpawnType.exUsec || bot.Profile.Side == EPlayerSide.Bear || bot.Profile.Side == EPlayerSide.Usec)
                {
                    pmcCount++;
                }
                else if (role == WildSpawnType.assault)
                {
                    scavCount++;
                }
                else if (role == WildSpawnType.marksman)
                {
                    sniperCount++;
                }
                else if (role.ToString().Contains("boss") || role.ToString().Contains("follower") || role.ToString().Contains("Boss") || role.ToString().Contains("Follower"))
                {
                    bossCount++;
                }
                else
                {
                    otherCount++;
                }
            }

            string fikaStatus = "Solo";
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
                            fikaStatus = isServer ? "Fika Host" : "Fika Client";
                    }
                }
            }
            catch { }

            float nextSpawnDelay = Mathf.Max(0, _nextWaveTime - Time.time);

            // Draw Texts
            GUILayout.BeginArea(new Rect(margin + 10, margin + 25, boxWidth - 20, boxHeight - 30));
            
            GUILayout.Label($"<b>Status de Sessão:</b> {fikaStatus}");
            GUILayout.Label($"<b>Config Preset:</b> {_activePreset}");
            GUILayout.Space(10);
            
            GUI.color = nextSpawnDelay < 10f ? Color.red : Color.white;
            GUILayout.Label($"<b>Próxima Wave em:</b> {Mathf.CeilToInt(nextSpawnDelay)}s");
            GUI.color = Color.white;
            
            GUILayout.Space(10);
            GUILayout.Label($"<b>População Ativa (Total {allAlive.Count}):</b>");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"- PMCs: <color=yellow>{pmcCount}</color>");
            GUILayout.Label($"- Scavs: <color=yellow>{scavCount}</color>");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"- Bosses/Guards: <color=yellow>{bossCount}</color>");
            GUILayout.Label($"- Snipers: <color=yellow>{sniperCount}</color>");
            GUILayout.EndHorizontal();

            if (otherCount > 0)
                GUILayout.Label($"- Outros (Raiders, etc): <color=yellow>{otherCount}</color>");
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("<color=grey>[F12] Ocultar Painel</color>");
            
            GUILayout.EndArea();
        }

        private bool IsFikaClient()
        {
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
    }
}
