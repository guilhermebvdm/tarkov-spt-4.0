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
        
        private bool _isSpawningWave = false;
        private int _delayBeforeFirstWave = 60;
        private int _secondsBetweenWaves = 360; 
        private float _nextWaveTime = 0f;
        
        private string _activePreset = "Balanced";
        private TRLConfig _serverConfig;
        
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

            while (true)
            {
                if (!_isSpawningWave)
                {
                    yield return StartCoroutine(ProcessWave());
                }
                
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Wave completed. Next wave in {_secondsBetweenWaves}s...");
                _nextWaveTime = Time.time + _secondsBetweenWaves;
                yield return new WaitForSeconds(_secondsBetweenWaves);
            }
        }

        private IEnumerator ProcessWave()
        {
            _isSpawningWave = true;

            int maxCap = Settings.GetMapCap(Singleton<GameWorld>.Instance.MainPlayer.Location);
            int aliveBots = _gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive);
            int availableSlots = maxCap - aliveBots;

            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Calculating Wave: MaxCap={maxCap}, Alive={aliveBots}, Available={availableSlots}");

            if (availableSlots <= 0)
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] No slots available for this wave. Skipping.");
                _isSpawningWave = false;
                yield break;
            }

            Queue<Tuple<BotCreationDataClass, BotZone>> spawnQueue = new Queue<Tuple<BotCreationDataClass, BotZone>>();
            
            // Helper Task to generate bots
            async Task<BotCreationDataClass> GenerateBotsAsync(EPlayerSide side, WildSpawnType spawnType, BotDifficulty difficulty, int count)
            {
                if (count <= 0) return null;
                
                BotSpawnParams spawnParams = new BotSpawnParams();
                BotProfileDataClass profileData = new BotProfileDataClass(side, spawnType, difficulty, 0f, spawnParams);
                
                return await BotCreationDataClass.Create(profileData, _botCreator, count, _botsController.BotSpawner);
            }
            
            // A more robust helper that returns the data directly
            async Task<BotCreationDataClass> GenerateBossAsync(WildSpawnType spawnType, BotDifficulty difficulty, int count)
            {
                if (count <= 0) return null;
                BotSpawnParams spawnParams = new BotSpawnParams();
                BotProfileDataClass profileData = new BotProfileDataClass(EPlayerSide.Savage, spawnType, difficulty, 0f, spawnParams);
                return await BotCreationDataClass.Create(profileData, _botCreator, count, _botsController.BotSpawner);
            }

            // ======================================
            // PROCESS ELITES / BOSSES
            // ======================================
            var mapName = _gameWorld.MainPlayer.Location.ToLower();
            var eliteConfig = _serverConfig?.EliteConfig;
            
            List<Tuple<BotCreationDataClass, BotZone>> bossQueue = new List<Tuple<BotCreationDataClass, BotZone>>();

            if (eliteConfig != null)
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
                            
                            int count = info.DisableFollowers ? 1 : 1; 
                            
                            var tBoss = GenerateBossAsync(bossType, BotDifficulty.normal, count);
                            while (!tBoss.IsCompleted) yield return null;
                            
                            var bossData = tBoss.Result;
                            
                            if (bossData != null)
                            {
                                bossQueue.Add(new Tuple<BotCreationDataClass, BotZone>(bossData, bz));
                                availableSlots -= count;
                            }
                        }
                    }
                }
            }

            // Group Splitting Local Function
            IEnumerator GenerateAndEnqueueGroups(EPlayerSide side, WildSpawnType role, BotDifficulty diff, int totalSlots, EliteLocationInfo info)
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

                    var t = GenerateBotsAsync(side, role, diff, groupSize);
                    while (!t.IsCompleted) yield return null;
                    if (t.Result != null)
                    {
                        spawnQueue.Enqueue(new Tuple<BotCreationDataClass, BotZone>(t.Result, GetHotzone(info, mapName)));
                    }
                    slotsRemaining -= groupSize;
                }
            }            // ======================================
            // PROCESS REGULAR HORDE (PMCs/Scavs)
            // ======================================
            if (availableSlots > 0)
            {
                if (eliteConfig != null && eliteConfig.RandomRaiderGroup && UnityEngine.Random.Range(1, 101) <= eliteConfig.RandomRaiderGroupChance)
                {
                    int raiderSlots = Mathf.Min(availableSlots, UnityEngine.Random.Range(2, 5));
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] RANDOM RAIDER GROUP INVASION! Spawning {raiderSlots} Raiders.");
                    yield return StartCoroutine(GenerateAndEnqueueGroups(EPlayerSide.Savage, WildSpawnType.pmcBot, BotDifficulty.normal, raiderSlots, eliteConfig.Raiders));
                    availableSlots -= raiderSlots;
                }

                if (eliteConfig != null && eliteConfig.RandomRogueGroup && availableSlots > 0 && UnityEngine.Random.Range(1, 101) <= eliteConfig.RandomRogueGroupChance)
                {
                    int rogueSlots = Mathf.Min(availableSlots, UnityEngine.Random.Range(2, 5));
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] RANDOM ROGUE GROUP INVASION! Spawning {rogueSlots} Rogues.");
                    yield return StartCoroutine(GenerateAndEnqueueGroups(EPlayerSide.Savage, WildSpawnType.exUsec, BotDifficulty.normal, rogueSlots, eliteConfig.Rogues));
                    availableSlots -= rogueSlots;
                }

                float pmcRatio = 0.5f;
                if (_activePreset == "PMC War") pmcRatio = 0.7f;
                else if (_activePreset == "Scav Infestation") pmcRatio = 0.3f;
                else if (_activePreset == "Warzone") pmcRatio = UnityEngine.Random.Range(0.2f, 0.8f);

                int pmcSlots = Mathf.RoundToInt(availableSlots * pmcRatio);
                int scavSlots = availableSlots - pmcSlots;

                int bearSlots = Mathf.RoundToInt(pmcSlots * 0.5f);
                int usecSlots = pmcSlots - bearSlots;

                int pScavSlots = Mathf.RoundToInt(scavSlots * 0.5f);
                int normalScavSlots = scavSlots - pScavSlots;

                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Horde Breakdown (Preset: {_activePreset}):");
                Plugin.LogSource.LogInfo($"  PMCs: {pmcSlots} ({bearSlots} Bear, {usecSlots} Usec)");
                Plugin.LogSource.LogInfo($"  Scavs: {scavSlots} ({normalScavSlots} Normal, {pScavSlots} pScav)");

                yield return StartCoroutine(GenerateAndEnqueueGroups(EPlayerSide.Usec, WildSpawnType.pmcUSEC, BotDifficulty.normal, usecSlots, eliteConfig?.Usec));
                yield return StartCoroutine(GenerateAndEnqueueGroups(EPlayerSide.Bear, WildSpawnType.pmcBEAR, BotDifficulty.normal, bearSlots, eliteConfig?.Bear));
                
                int sniperCount = 0;
                int mapSniperChance = _serverConfig?.MapConfigs?.ContainsKey(mapName) == true ? _serverConfig.MapConfigs[mapName].SniperChance : 30;
                if (UnityEngine.Random.Range(1, 101) <= mapSniperChance && normalScavSlots > 0)
                {
                    sniperCount = 1;
                    normalScavSlots -= 1;
                    var tSniper = GenerateBotsAsync(EPlayerSide.Savage, WildSpawnType.marksman, BotDifficulty.normal, sniperCount);
                    while (!tSniper.IsCompleted) yield return null;
                    if (tSniper.Result != null) spawnQueue.Enqueue(new Tuple<BotCreationDataClass, BotZone>(tSniper.Result, null));
                }

                yield return StartCoroutine(GenerateAndEnqueueGroups(EPlayerSide.Savage, WildSpawnType.assault, BotDifficulty.normal, normalScavSlots, eliteConfig?.Scav));
                yield return StartCoroutine(GenerateAndEnqueueGroups(EPlayerSide.Savage, WildSpawnType.assault, BotDifficulty.normal, pScavSlots, eliteConfig?.Scav));
            }

            Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Profiles generated. Starting smooth injection...");

            // First, inject Bosses in their designated zones
            foreach (var bq in bossQueue)
            {
                if (_botsController.Bots.Count >= maxCap) break;
                Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Spawning BOSS gracefully in {bq.Item2.NameZone}...");
                _botsController.BotSpawner.TryToSpawnInZoneAndDelay(bq.Item2, bq.Item1, true, true, null, true);
                
                if (TRLDynamicSpawn.Helpers.Settings.enableSmoothSpawning.Value)
                {
                    yield return new WaitForSeconds(TRLDynamicSpawn.Helpers.Settings.smoothSpawningDelay.Value * 2f); // Wait a bit longer for bosses
                }
            }

            // Then, inject regular horde
            while (spawnQueue.Count > 0)
            {
                if (_gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive) >= maxCap)
                {
                    Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Max cap reached during smooth spawn. Stopping wave.");
                    break;
                }

                var tuple = spawnQueue.Dequeue();
                BotCreationDataClass data = tuple.Item1;
                BotZone preferredZone = tuple.Item2;
                
                BotZone selectedZone = preferredZone;
                int retries = 5;
                bool zoneValid = false;

                while (retries > 0)
                {
                    if (selectedZone == null)
                    {
                        selectedZone = TRLDynamicSpawn.Helpers.Methods.GetRandomZone(_botsController.BotSpawner);
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
                    Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] SUCCESS: Spawning bot group (Size: {data.Count}) gracefully in {selectedZone.NameZone}...");
                    _botsController.BotSpawner.TryToSpawnInZoneAndDelay(selectedZone, data, true, true, null, true);
                }
                else
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] FAILED: Could not find a safe/LoS-free zone for bot group after 5 tries. Dropping group to prevent infinite loop.");
                    // DO NOT push back to queue, let it drop. The next wave will recreate them if slots are available.
                }

                if (TRLDynamicSpawn.Helpers.Settings.enableSmoothSpawning.Value)
                {
                    yield return new WaitForSeconds(TRLDynamicSpawn.Helpers.Settings.smoothSpawningDelay.Value);
                }
                else
                {
                    yield return null; // Just wait 1 frame to not lock main thread
                }
            } // End while queue

            _isSpawningWave = false;
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
            BotProfileDataClass profileData = new BotProfileDataClass(side, role, difficulty, 0f, spawnParams);
            
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

        private void OnGUI()
        {
            if (!TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value || _gameWorld == null) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.green;
            style.alignment = TextAnchor.UpperRight;

            float remaining = Mathf.Max(0, _nextWaveTime - Time.time);
            string text = $"Next Wave: {Mathf.CeilToInt(remaining)}s\nLive Bots: {_gameWorld.RegisteredPlayers.Count(p => p.IsAI && p.HealthController != null && p.HealthController.IsAlive)}";
            
            GUI.Label(new Rect(Screen.width - 250, 10, 240, 100), text, style);
        }
    }
}
