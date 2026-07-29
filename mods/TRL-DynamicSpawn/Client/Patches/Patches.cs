using System;
using System.Collections.Generic;
using Comfort.Common;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Game.Spawning;
using EFT.UI.Matchmaker;
using HarmonyLib;
using TRLDynamicSpawn.Helpers;
using SPT.Custom.CustomAI;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TRLDynamicSpawn.Patches
{
    internal class NoTeleportPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(BotMover),
                nameof(BotMover.GoToPoint),
                [
                    typeof(Vector3),
                    typeof(bool),
                    typeof(float),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                ]
            );
        }

        [PatchPrefix]
        public static void PatchPrefix(ref bool mustHaveWay)
        {
            mustHaveWay = false;
        }
    }

    // MatchMakerAcceptScreen
    public class RefreshLocation : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(MatchMakerAcceptScreen),
                nameof(MatchMakerAcceptScreen.Add)
            );
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            Methods.RefreshLocationInfo();
            return true;
        }
    }

    public class MarkerDumper : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocationScene), nameof(LocationScene.Awake));
        }

        [PatchPostfix]
        public static void Postfix(LocationScene __instance)
        {
            foreach (var marker in __instance.SpawnPointMarkers)
            {
                if (marker.SpawnPoint.Categories.ContainPlayerCategory())
                    Logger.LogInfo(
                        $"marker name: {marker.SpawnPoint.Position} ID: {marker.SpawnPoint.Id}"
                    );
            }
        }
    }

    public class SniperPatch : ModulePatch
    {
        private static double Sq(double n)
        {
            return n * n;
        }

        private static double Pt(double a, double b)
        {
            return Math.Sqrt(Sq(a) + Sq(b));
        }

        public static double GetDistance(
            double x,
            double y,
            double z,
            double mX,
            double mY,
            double mZ
        )
        {
            x = Math.Abs(x - mX);
            y = Math.Abs(y - mY);
            z = Math.Abs(z - mZ);

            return Pt(Pt(x, z), y);
        }

        public static double GetVectorDistance(Vector3 v1, Vector3 v2)
        {
            return GetDistance(v1.x, v1.y, v1.z, v2.x, v2.y, v2.z);
        }

        public static BotZone FindFarthestZone(List<BotZone> botZones, Vector3 referencePoint)
        {
            if (botZones == null || botZones.Count == 0)
            {
                throw new ArgumentException("The botZones list cannot be null or empty.");
            }

            // Order the zones by distance in descending order
            var orderedZones = botZones
                .OrderBy(botZone => GetVectorDistance(botZone.CenterOfSpawnPoints, referencePoint))
                .ToList();

            // Get the last half of the list
            int halfCount = orderedZones.Count / 2;
            var lastHalfZones = orderedZones.Skip(halfCount).ToList();

            // Select a random zone from the last half
            System.Random random = new();
            int randomIndex = random.Next(lastHalfZones.Count);

            return lastHalfZones[randomIndex];
        }

        static BotZone GetNearestZone(List<BotZone> zones, string name)
        {
            foreach (BotZone zone in zones)
            {
                if (zone.NameZone == name)
                {
                    return zone;
                }
            }
            System.Random random = new();
            // Generate a random index between 0 and the count of the list (exclusive)
            int randomIndex = random.Next(zones.Count);

            // Return the element at the random index
            return zones[randomIndex];
        }

        static PatrolWay GetRandomPatrol(PatrolWay[] patrol)
        {
            // Create a random number generator
            System.Random random = new();

            // Generate a random index between 0 and the count of the list (exclusive)
            int randomIndex = random.Next(patrol.Length);

            // Return the element at the random index
            return patrol[randomIndex];
        }

        static bool IsNameInBotzones(List<BotZone> zones, string name)
        {
            // Create a random number generator
            System.Random random = new();

            foreach (BotZone zone in zones)
            {
                if (zone.NameZone == name)
                {
                    return true;
                }
            }
            // Generate a random index between 0 and the count of the list (exclusive)

            return false;
        }

        public static string GetBotZoneNameById(SpawnPointParams[] spawnPoints, string id)
        {
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint.Id != null && spawnPoint.BotZoneName != null && spawnPoint.Id == id)
                {
                    return spawnPoint.BotZoneName;
                }
            }
            return ""; // Return null if the ID is not found
        }

        public static void SetBotZoneName(SpawnPointParams[] spawnPoints, string id, string newName)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].Id == id)
                {
                    spawnPoints[i].BotZoneName = newName;
                }
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(SpawnPointManagerClass),
                nameof(SpawnPointManagerClass.smethod_1)
            );
        }

        [PatchPostfix]
        static void Postfix(ref SpawnPointMarker[] __result, SpawnPointParams[] parameters)
        {
            Plugin.LogSource.LogInfo("Attempting spawnzone updates");
            if (
                __result == null
                || parameters == null
                || __result.Length == 0
                || parameters.Length == 0
            )
            {
                Plugin.LogSource.LogInfo("TRLDynamicSpawn: We hit the error case, skipping implementation");
                return;
            }

            List<BotZone> snipeZones = new List<BotZone>();
            List<BotZone> botZones = new List<BotZone>();

            foreach (SpawnPointMarker zone in __result)
            {
                if (zone == null)
                    continue;
                var botzoneExists = !zone.BotZone.IsNullOrDestroyed();
                if (botzoneExists)
                {
                    if (zone.BotZone.SnipeZone)
                    {
                        snipeZones.Add(zone.BotZone);
                    }
                    else
                    {
                        botZones.Add(zone.BotZone);
                    }
                }
            }
            // Plugin.LogSource.LogInfo("1");
            if (botZones.Count == 0 || snipeZones.Count == 0)
                return;

            List<BotZone> nonSniperZones = botZones.ApplyFilter(zone => !zone.SnipeZone);

            for (int index = 0; index < __result.Length; index++)
            {
                SpawnPointMarker zone = __result[index];
                if (
                    zone == null
                    || zone.SpawnPoint.Categories == ESpawnCategoryMask.None
                    || zone.SpawnPoint.Categories.ContainPlayerCategory()
                )
                {
                    if (
                        zone.SpawnPoint.Categories.ContainPlayerCategory()
                        && !zone.BotZone.IsNullOrDestroyed()
                    )
                    {
                        zone.BotZone = null;
                    }
                    continue;
                }

                // Plugin.LogSource.LogInfo("3");
                var botzoneDoesNotExist = zone.BotZone.IsNullOrDestroyed();
                // Plugin.LogSource.LogInfo("4");
                string botZoneName = GetBotZoneNameById(parameters, zone.Id);
                // bool isPmc = botZoneName.Contains("pmc");

                if (botzoneDoesNotExist)
                {
                    if (
                        IsNameInBotzones(snipeZones, botZoneName)
                        || botZoneName.ToLower().Contains("custom_snipe")
                    )
                    {
                        if (botZoneName.ToLower().Contains("custom_snipe"))
                        {
                            SetBotZoneName(parameters, zone.Id, "");
                            botZoneName = "";
                        }

                        BotZone RandomBotZone = GetNearestZone(snipeZones, botZoneName);

                        int newVal =
                            RandomBotZone.MaxPersons > 0 ? RandomBotZone.MaxPersons + 1 : 5;

                        AccessTools
                            .Field(typeof(BotZone), "_maxPersons")
                            .SetValue(RandomBotZone, newVal);

                        zone.BotZone = RandomBotZone;
                    }
                    else
                    {
                        BotZone RandomBotZone = GetNearestZone(nonSniperZones, botZoneName);

                        // if (RandomBotZone.name != botZoneName)
                        //     SetBotZoneName(parameters, zone.Id, RandomBotZone.name);

                        // if (!RandomBotZone.SpawnPointMarkers.Contains(zone))
                        // {
                        //     RandomBotZone.SpawnPointMarkers =
                        //     [
                        //         .. RandomBotZone.SpawnPointMarkers,
                        //         zone,
                        //     ];
                        // }

                        if (RandomBotZone.MaxPersons != -1)
                        {
                            AccessTools
                                .Field(typeof(BotZone), "_maxPersons")
                                .SetValue(RandomBotZone, -1);
                        }

                        zone.BotZone = RandomBotZone;
                    }
                    // else
                    // {
                    //     if (zone.BotZone.name != botZoneName)
                    //         SetBotZoneName(parameters, zone.Id, zone.BotZone.name);
                }
            }
            Plugin.LogSource.LogInfo("Spawnszone updates complete");
        }
    }

    public class SetMaxBotCountPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.SetSettings));
        }

        [PatchPostfix]
        private static void PatchPostfix(BotsController __instance, int maxCount)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return;

            var location = gameWorld.LocationId;
            if (string.IsNullOrEmpty(location)) return;

            int localCap = Settings.GetMapCap(location);
            
            Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Local Host Performance: Setting max bots to {localCap} on {location}");
            __instance.MaxCount = localCap;

            if (__instance.BotSpawner == null)
            {
                return;
            }
                
            __instance.BotSpawner.SetMaxBots(__instance.MaxCount);
            __instance.ZonesLeaveController.SetMaxBots(__instance.MaxCount);
        }
    }

    public class MapCullingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(SpawnPointManagerClass),
                nameof(SpawnPointManagerClass.smethod_1)
            );
        }

        [PatchPostfix]
        static void Postfix(ref SpawnPointMarker[] __result)
        {
            if (__result == null || __result.Length == 0) return;

            try
            {
                string json = SPT.Common.Http.RequestHandler.GetJson("/trldynamicspawn/getConfig");
                if (string.IsNullOrEmpty(json)) return;

                var cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<TRLDynamicSpawn.Models.TRLConfig>(json);
                if (cfg == null || !cfg.EnableMapOverlapCulling) return;
                
                double cullingDist = cfg.GlobalAntiOverlapDistance;
                if (cullingDist <= 0) return;

                List<SpawnPointMarker> validMarkers = new List<SpawnPointMarker>();
                int removed = 0;

                foreach (var marker in __result)
                {
                    if (marker == null || marker.SpawnPoint == null) continue;

                    bool tooClose = false;
                    foreach (var valid in validMarkers)
                    {
                        if (Vector3.Distance(marker.SpawnPoint.Position, valid.SpawnPoint.Position) < cullingDist)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        validMarkers.Add(marker);
                    }
                    else
                    {
                        removed++;
                    }
                }

                if (removed > 0)
                {
                    __result = validMarkers.ToArray();
                    if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                    {
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] MapCulling removed {removed} overlapping spawn points (Min Dist: {cullingDist}m).");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error in MapCullingPatch: {ex.Message}");
            }
        }
    }

    public class DisableVanillaWavesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.ActivateBotsByWave), new[] { typeof(BotWaveDataClass) });
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotWaveDataClass wave, ref System.Threading.Tasks.Task __result)
        {
            if (TRLDynamicSpawn.Components.DynamicSpawnManager.IsGeneratingDynamicWave) return true; // Let our wave run!
            
            // Se for Scav comum ou PMC do vanilla, bloqueia 100% (inclusive no 1º minuto) para o DynamicSpawn controlar
            if (wave != null && (wave.WildSpawnType == WildSpawnType.assault || 
                                wave.WildSpawnType == WildSpawnType.cursedAssault || 
                                wave.WildSpawnType == WildSpawnType.pmcUSEC || 
                                wave.WildSpawnType == WildSpawnType.pmcBEAR))
            {
                if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                {
                    Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Blocked Vanilla Normal Wave ({wave.WildSpawnType}) to give 100% control to DynamicSpawn.");
                }
                __result = System.Threading.Tasks.Task.CompletedTask;
                return false;
            }

            // Permite Scav Snipers (marksman), Raiders, Rogues e outros tipos nativos passarem livremente
            return true;
        }
    }

    public class DisableVanillaBossWavesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.ActivateBotsByWave), new[] { typeof(BossLocationSpawn) });
        }

        [PatchPrefix]
        private static bool PatchPrefix(BossLocationSpawn wave)
        {
            if (wave == null || wave.BossName == null) return true;

            string name = wave.BossName.ToLower();

            // Bloqueia PMCs e Scavs normais (incluindo cursedassault, assaultgroup, etc. do Ground Zero)
            bool isPmcOrScav = (name.Contains("assault") || name.Contains("savage") || name.Contains("scav") || name.Contains("bear") || name.Contains("usec"))
                               && name != "pmcbot" && name != "exusec";

            if (isPmcOrScav)
            {
                if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                {
                    Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Blocked Vanilla Horde Wave ({wave.BossName}) to give 100% control to DynamicSpawn.");
                }
                return false;
            }

            // Permite todos os outros bosses reais, elites, snipers, raiders (pmcbot), rogues (exusec) e cultistas nativos
            return true;
        }
    }

    public class TryToSpawnInZoneAndDelayPatch : ModulePatch
    {
        private static Queue<Vector3> _lastSpawnPositions = new Queue<Vector3>();
        private static int _maxHistorySpawnPoint = 6;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(BotSpawner),
                nameof(BotSpawner.TryToSpawnInZoneAndDelay)
            );
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotSpawner __instance, BotZone botZone, BotCreationDataClass data, bool withCheckMinMax, bool newWave, ref List<ISpawnPoint> pointsToSpawn, bool forcedSpawn = false)
        {
            if (pointsToSpawn != null && pointsToSpawn.Count > 0) return true;

            try
            {
                var allPoints = botZone.SpawnPoints;

                if (allPoints == null || allPoints.Length == 0) return true;

                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null) return true;
                string mapName = gameWorld.MainPlayer?.Location?.ToLower() ?? "";

                var mapSettings = TRLDynamicSpawn.Components.DynamicSpawnManager.Instance != null ? MapNameHelper.GetMapSettings(TRLDynamicSpawn.Components.DynamicSpawnManager.Instance.ServerConfig, mapName) : null;
                double safeDist = mapSettings != null ? mapSettings.SafeZoneDistance : (mapName.Contains("factory") || mapName.Contains("sandbox") || mapName.Contains("laboratory") ? 15.0 : 30.0);

                bool enableLos = TRLDynamicSpawn.Helpers.Settings.enableLoSCulling.Value;
                float losDist = TRLDynamicSpawn.Helpers.Settings.losCullingDistance.Value;

                var playersList = gameWorld.AllAlivePlayersList;
                var players = new List<Player>();
                if (playersList != null)
                {
                    foreach (var p in playersList)
                    {
                        if (p == null || p.Profile == null) continue;
                        if (p.IsAI && !p.IsYourPlayer) continue;
                        if (UnityEngine.Application.isBatchMode && p.IsYourPlayer) continue;
                        players.Add(p);
                    }
                }

                var strictPoints = new List<ISpawnPoint>();
                var noLosPoints = new List<ISpawnPoint>();
                var noBubblePoints = new List<ISpawnPoint>();

                var fallbackStrictPoints = new List<ISpawnPoint>();
                var fallbackNoLosPoints = new List<ISpawnPoint>();
                var fallbackNoBubblePoints = new List<ISpawnPoint>();

                float maxDist = 300f;
                bool isBubbleEnabled = TRLDynamicSpawn.Helpers.Settings.enableSpawnBubble.Value;

                if (mapSettings != null)
                {
                    isBubbleEnabled = isBubbleEnabled && mapSettings.EnableSpawnBubble;
                    maxDist = mapSettings.SpawnBubbleDistance;
                }

                foreach (var checkPoint in allPoints)
                {
                    if (checkPoint == null) continue;

                    bool tooCloseToRecent = false;
                    foreach (var recentPos in _lastSpawnPositions)
                    {
                        if (Vector3.Distance(checkPoint.Position, recentPos) < 50f)
                        {
                            tooCloseToRecent = true;
                            break;
                        }
                    }

                    bool insideBubble = true;
                    bool outsideSafe = true;
                    bool hasLoS = false;

                    if (players.Count > 0)
                    {
                        if (isBubbleEnabled)
                        {
                            bool closeEnough = false;
                            foreach (var p in players)
                            {
                                if (Vector3.Distance(p.Position, checkPoint.Position) <= maxDist)
                                {
                                    closeEnough = true;
                                    break;
                                }
                            }
                            if (!closeEnough) insideBubble = false;
                        }

                        float heightLimit = (mapName == "factory4_day" || mapName == "factory4_night" || mapName == "sandbox" || mapName == "sandbox_high") ? 5.0f : 15.0f;
                        foreach (var p in players)
                        {
                            float dx = p.Position.x - checkPoint.Position.x;
                            float dz = p.Position.z - checkPoint.Position.z;
                            float dh = (float)System.Math.Sqrt(dx * dx + dz * dz);
                            float dv = System.Math.Abs(p.Position.y - checkPoint.Position.y);
                            float limitW = System.Math.Max((float)safeDist, 5f);
                            float limitH = System.Math.Max(heightLimit, 3f);
                            if ((dh / limitW) + (dv / limitH) <= 1.0f)
                            {
                                outsideSafe = false;
                                break;
                            }
                        }

                        if (outsideSafe && enableLos)
                        {
                            foreach (var p in players)
                            {
                                float d = Vector3.Distance(p.Position, checkPoint.Position);
                                if (d <= losDist)
                                {
                                    bool isVis = false;
                                    if (p.IsYourPlayer && Camera.main != null)
                                    {
                                        Vector3 screenPoint = Camera.main.WorldToViewportPoint(checkPoint.Position + Vector3.up * 1f);
                                        if (screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1) isVis = true;
                                    }
                                    else
                                    {
                                        Vector3 dir = (checkPoint.Position - p.Position).normalized;
                                        if (Vector3.Dot(p.LookDirection, dir) > 0.5f) isVis = true;
                                    }
                                    if (isVis)
                                    {
                                        Vector3 headPos = p.MainParts.ContainsKey(BodyPartType.head) ? p.MainParts[BodyPartType.head].Position : p.Position + Vector3.up * 1.5f;
                                        if (!Physics.Linecast(headPos, checkPoint.Position + Vector3.up * 1f, LayerMaskClass.HighPolyWithTerrainMask))
                                        {
                                            hasLoS = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (outsideSafe)
                    {
                        if (tooCloseToRecent)
                        {
                            if (insideBubble && !hasLoS) fallbackStrictPoints.Add(checkPoint);
                            if (insideBubble) fallbackNoLosPoints.Add(checkPoint);
                            if (!hasLoS) fallbackNoBubblePoints.Add(checkPoint);
                        }
                        else
                        {
                            if (insideBubble && !hasLoS) strictPoints.Add(checkPoint);
                            if (insideBubble) noLosPoints.Add(checkPoint);
                            if (!hasLoS) noBubblePoints.Add(checkPoint);
                        }
                    }
                }

                List<ISpawnPoint> chosenList = null;
                if (strictPoints.Count > 0) chosenList = strictPoints;
                else if (fallbackStrictPoints.Count > 0) chosenList = fallbackStrictPoints;
                else if (noLosPoints.Count > 0) chosenList = noLosPoints;
                else if (fallbackNoLosPoints.Count > 0) chosenList = fallbackNoLosPoints;
                // MASTER FALLBACK: Acionado APENAS em última instância se nenhum ponto na bolha funcionar!
                else if (noBubblePoints.Count > 0)
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] MASTER FALLBACK LEVEL 1: Spawning outside bubble in {botZone.NameZone} to prevent spawn drop.");
                    chosenList = noBubblePoints;
                }
                else if (fallbackNoBubblePoints.Count > 0)
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] MASTER FALLBACK LEVEL 2: Spawning outside bubble with history in {botZone.NameZone}.");
                    chosenList = fallbackNoBubblePoints;
                }

                if (chosenList != null && chosenList.Count > 0)
                {
                    var selectedPoint = chosenList[UnityEngine.Random.Range(0, chosenList.Count)];
                    pointsToSpawn = new List<ISpawnPoint> { selectedPoint };
                    _lastSpawnPositions.Enqueue(selectedPoint.Position);
                    if (_lastSpawnPositions.Count > _maxHistorySpawnPoint)
                    {
                        _lastSpawnPositions.Dequeue();
                    }
                }
                else
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] No safe spawn points found in {botZone.NameZone}. Falling back to default.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error in TryToSpawnInZoneAndDelayPatch: {ex.Message}");
            }
            return true;
        }
    }

    public class ChooseProfilePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotProfileDataClass), nameof(BotProfileDataClass.ChooseProfile));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Profile __result, BotProfileDataClass __instance, List<Profile> profiles2Select, bool withDelete)
        {
            // Se for PMC, ignoramos a checagem rigorosa de Dificuldade e Side, garantindo que o bot não retorne nulo.
            if (__instance.WildSpawnType_0 == WildSpawnType.pmcUSEC || __instance.WildSpawnType_0 == WildSpawnType.pmcBEAR)
            {
                var list = profiles2Select.Where(x => x.Info.Settings.Role == __instance.WildSpawnType_0).ToList();
                if (list.Count > 0)
                {
                    Profile profile = list[UnityEngine.Random.Range(0, list.Count)];
                    if (withDelete)
                    {
                        profiles2Select.Remove(profile);
                    }
                    __result = profile;
                    return false; // Skips original method
                }
            }
            return true;
        }
    }
}


