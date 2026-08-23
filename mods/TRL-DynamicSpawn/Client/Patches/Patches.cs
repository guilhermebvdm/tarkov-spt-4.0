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
            if (FikaHelper.IsClient()) return;

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

    public class DisableVanillaWavesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.ActivateBotsByWave), new[] { typeof(BotWaveDataClass) });
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotWaveDataClass wave, ref System.Threading.Tasks.Task __result)
        {
            if (FikaHelper.IsClient()) return true;
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
            if (FikaHelper.IsClient()) return true;
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

            var elite = ServerConfigProvider.Config?.EliteConfig;
            if (elite != null)

            {
                if (elite.DisableVanillaRogues && name == "exusec")
                {
                    if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                    {
                        Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Blocked Native Vanilla Rogue Wave ({wave.BossName}) per user config.");
                    }
                    return false;
                }

                // Lighthouse Rogue Zone Filter + Count Limiter
                if (name == "exusec")
                {
                    var gameWorld = Comfort.Common.Singleton<GameWorld>.Instance;
                    if (gameWorld != null && string.Equals(gameWorld.MainPlayer?.Location, "lighthouse", StringComparison.OrdinalIgnoreCase))
                    {
                        // Zone filter: discard rogues not in the Treatment area
                        if (elite.LighthouseRogueZoneFilter)
                        {
                            string bossZone = wave.BossZone ?? "";

                            // Força 100% de chance para Zone_Blockpost garantindo a dupla de Rogues nas armas montadas da guarita frontal
                            if (bossZone.Contains("Zone_Blockpost"))
                            {
                                wave.BossChance = 100;
                            }

                            bool isForbiddenZone = bossZone.Contains("Island")
                                                || bossZone.Contains("Chalet")
                                                || bossZone.Contains("Village")
                                                || bossZone.Contains("Bridge")
                                                || bossZone.Contains("OldHouse")
                                                || bossZone.Contains("LongRoad")
                                                || bossZone.Contains("DestroyedHouse")
                                                || bossZone.Contains("SniperPeak");

                            if (isForbiddenZone)
                            {
                                Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Rogue wave DISCARDED — zone '{bossZone}' is outside Treatment area. Bot slot freed.");
                                return false;
                            }
                        }



                        // Count limiter: discard if alive rogues already hit the cap
                        int maxRogues = elite.LighthouseRogueMaxCount;
                        if (maxRogues > 0)
                        {
                            int aliveRogues = 0;
                            var allPlayers = gameWorld.AllAlivePlayersList;
                            if (allPlayers != null)
                            {
                                foreach (var p in allPlayers)
                                {
                                    if (p != null && p.IsAI && p.Profile?.Info?.Settings?.Role == WildSpawnType.exUsec)
                                        aliveRogues++;
                                }
                            }

                            if (aliveRogues >= maxRogues)
                            {
                                Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Rogue wave DISCARDED — cap reached ({aliveRogues}/{maxRogues}). Bot slot freed.");
                                return false;
                            }
                        }
                    }
                }


                if (elite.DisableVanillaRaiders && name == "pmcbot")
                {
                    if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                    {
                        Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Blocked Native Vanilla Raider Wave ({wave.BossName}) per user config.");
                    }
                    return false;
                }
            }

            // Permite todos os outros bosses reais, elites, snipers, raiders (pmcbot), rogues (exusec) e cultistas nativos
            return true;
        }
    }

    public class TryToSpawnInZoneAndDelayPatch : ModulePatch
    {
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
            if (FikaHelper.IsClient()) return true;

            WildSpawnType botRole = WildSpawnType.assault;
            if (data != null && data.Profiles != null && data.Profiles.Count > 0 && data.Profiles[0] != null && data.Profiles[0].Info != null && data.Profiles[0].Info.Settings != null)
            {
                botRole = data.Profiles[0].Info.Settings.Role;
            }

            // Bloqueio cirúrgico de Scavs comuns (assault / cursedAssault) vindos do motor nativo do jogo (vanilla/SPT)
            if (!TRLDynamicSpawn.Components.DynamicSpawnManager.IsGeneratingDynamicWave)
            {
                if (botRole == WildSpawnType.assault || botRole == WildSpawnType.cursedAssault)
                {
                    if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                    {
                        Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Blocked Vanilla Assault Scav Spawn ({botRole}) from native game engine.");
                    }
                    return false;
                }
            }

            // Filtro de Zona & Limitador Estrito por Bot para Rogues no Lighthouse
            var gameWorld = Singleton<GameWorld>.Instance;
            string mapName = (gameWorld?.LocationId ?? gameWorld?.MainPlayer?.Location ?? "").ToLower();

            if (botRole == WildSpawnType.exUsec && mapName == "lighthouse")
            {
                var elite = ServerConfigProvider.Config?.EliteConfig;
                if (elite != null)

                {
                    // 1. Filtro de Zona: Bloqueia QUALQUER Rogue fora da área da Unidade de Tratamento
                    if (elite.LighthouseRogueZoneFilter)
                    {
                        string zoneName = botZone?.NameZone ?? "";
                        bool isForbiddenZone = zoneName.Contains("Island")
                                            || zoneName.Contains("Chalet")
                                            || zoneName.Contains("Village")
                                            || zoneName.Contains("Bridge")
                                            || zoneName.Contains("OldHouse")
                                            || zoneName.Contains("LongRoad")
                                            || zoneName.Contains("DestroyedHouse")
                                            || zoneName.Contains("SniperPeak");

                        if (isForbiddenZone)
                        {
                            if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                            {
                                Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Rogue spawn attempt in '{zoneName}' DISCARDED (outside Treatment area). Bot slot freed.");
                            }
                            return false;
                        }
                    }


                    // 2. Limitador Estrito por Bot: Impede que qualquer bot passe se a contagem viva já atingiu o limite
                    int maxRogues = elite.LighthouseRogueMaxCount;
                    if (maxRogues > 0 && gameWorld != null)
                    {
                        int aliveRogues = 0;
                        var allPlayers = gameWorld.AllAlivePlayersList;
                        if (allPlayers != null)
                        {
                            for (int i = 0; i < allPlayers.Count; i++)
                            {
                                var p = allPlayers[i];
                                if (p != null && p.IsAI && p.HealthController != null && p.HealthController.IsAlive && p.Profile?.Info?.Settings?.Role == WildSpawnType.exUsec)
                                {
                                    aliveRogues++;
                                }
                            }
                        }

                        if (aliveRogues >= maxRogues)
                        {
                            if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
                            {
                                Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Rogue spawn attempt DISCARDED — cap reached ({aliveRogues}/{maxRogues}). Bot slot freed.");
                            }
                            return false;
                        }
                    }
                }
            }

            if (pointsToSpawn != null && pointsToSpawn.Count > 0) return true;

            // Isentar Bosses e Seguidores nativos do jogo das restrições de SafeZone e LoS do mod
            string roleStr = botRole.ToString();
            bool isNativeBossOrFollower = roleStr.StartsWith("boss", StringComparison.OrdinalIgnoreCase) || roleStr.StartsWith("follower", StringComparison.OrdinalIgnoreCase);
            if (isNativeBossOrFollower)
            {
                return true;
            }

            try
            {
                var allPoints = botZone.SpawnPoints;

                if (allPoints == null || allPoints.Length == 0) return true;

                if (gameWorld == null) return true;

                var mapSettings = MapNameHelper.GetMapSettings(ServerConfigProvider.Config, mapName);


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

                float maxDist = 300f;
                bool isBubbleEnabled = TRLDynamicSpawn.Helpers.Settings.enableSpawnBubble.Value;

                if (mapSettings != null)
                {
                    isBubbleEnabled = isBubbleEnabled && mapSettings.EnableSpawnBubble;
                    maxDist = mapSettings.SpawnBubbleDistance;
                }

                bool isSniperBot = SpawnPointHelper.IsSniperRole(botRole);

                foreach (var checkPoint in allPoints)
                {
                    if (checkPoint == null) continue;

                    bool isSniperPoint = SpawnPointHelper.IsSniperSpawnPoint(checkPoint, botZone);

                    // Regra Estrita Bilateral de Sniper:
                    // 1. Bots não-sniper NUNCA usam pontos de sniper.
                    // 2. Bots sniper NUNCA usam pontos normais.
                    if (!isSniperBot && isSniperPoint) continue;
                    if (isSniperBot && !isSniperPoint) continue;

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

                        float heightLimit = (mapName.Contains("factory") || mapName.Contains("sandbox") || mapName.Contains("laboratory") || mapName.Contains("interchange") || mapName.Contains("tarkovstreets") || mapName.Contains("shoreline") || mapName.Contains("rezervbase")) ? 4.0f : 15.0f;
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
                                        if (!Physics.Linecast(headPos, checkPoint.Position + Vector3.up * 1f, LayerMaskClass.HighPolyWithTerrainMask | LayerMaskClass.PlayerStaticCollisionsMask))
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
                        if (insideBubble && !hasLoS) strictPoints.Add(checkPoint);
                        if (insideBubble) noLosPoints.Add(checkPoint);
                        if (!hasLoS) noBubblePoints.Add(checkPoint);
                    }
                }

                List<ISpawnPoint> chosenList = null;
                if (strictPoints.Count > 0) chosenList = strictPoints;
                else if (noLosPoints.Count > 0) chosenList = noLosPoints;
                else if (noBubblePoints.Count > 0)
                {
                    Plugin.LogSource.LogWarning($"[TRL-DynamicSpawn] MASTER FALLBACK: Spawning outside bubble in {botZone.NameZone} to prevent spawn drop.");
                    chosenList = noBubblePoints;
                }

                if (chosenList != null && chosenList.Count > 0)
                {
                    var selectedPoint = chosenList[UnityEngine.Random.Range(0, chosenList.Count)];
                    if (pointsToSpawn == null) pointsToSpawn = new List<ISpawnPoint>();
                    pointsToSpawn.Add(selectedPoint);
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

        /// <summary>
        /// Tolerant profile choice for EVERY role (ref: AUD-01-04). Vanilla (BotProfileDataClass.cs:85-96) requires an exact
        /// Side + Role + Difficulty match and, on a miss, BotsPresets.CreateProfile (BotsPresets.cs:170-189) generates 3 new
        /// profiles of that exact combination — orphans that nobody consumes when the mod samples a per-wave difficulty.
        /// Order: exact (Side+Role+Difficulty) → relaxed (Side+Role, any difficulty — AC-X1) → vanilla (nothing of this
        /// Side+Role in the pool: generating is the right thing). PMC keeps its faction tolerance (Side OR Role), but the old
        /// "any profile, even a Scav" fallback is dropped on purpose (AC-X5). Single pass, no LINQ. Logs gated (AUD-01-07).
        /// </summary>
        [PatchPrefix]
        private static bool PatchPrefix(ref Profile __result, BotProfileDataClass __instance, List<Profile> profiles2Select, bool withDelete)
        {
            if (__instance == null || profiles2Select == null || profiles2Select.Count == 0) return true;

            var role = __instance.WildSpawnType_0;   // ref: BotProfileDataClass.cs:16/:19/:43 — public fields; Side is EPlayerSide? (lifted ==, no .Value)
            var side = __instance.Side;
            var diff = __instance.BotDifficulty_0;
            bool debug = Settings.enableDebugLogs.Value;   // gate BEFORE formatting — ref: AUD-01-07

            if (debug)
            {
                Plugin.LogSource.LogInfo($"[TRLDynamicSpawn Logger] ChooseProfile CALLED for Role: {role} ({diff}) (profilesInList: {profiles2Select.Count})");
                int sample = System.Math.Min(5, profiles2Select.Count);
                for (int i = 0; i < sample; i++)
                {
                    var p = profiles2Select[i];
                    Plugin.LogSource.LogInfo($"   -> Available profile [{i}]: Name='{p?.Nickname}', Side={p?.Info?.Side}, Role={p?.Info?.Settings?.Role}, Diff={p?.Info?.Settings?.BotDifficulty}");
                }
            }

            bool isPmc = role == WildSpawnType.pmcUSEC || role == WildSpawnType.pmcBEAR;
            Profile exact = null, relaxed = null;
            int exactCount = 0, relaxedCount = 0;

            // Reservoir-style random pick: uniform among "exact" and among "relaxed" without building lists.
            for (int i = 0; i < profiles2Select.Count; i++)
            {
                var p = profiles2Select[i];
                var info = p?.Info;
                var st = info?.Settings;
                if (info == null || st == null) continue;

                bool roleMatch = isPmc ? PmcMatches(info, st, role) : (info.Side == side && st.Role == role);
                if (!roleMatch) continue;

                relaxedCount++;
                if (UnityEngine.Random.Range(0, relaxedCount) == 0) relaxed = p;
                if (st.BotDifficulty == diff)
                {
                    exactCount++;
                    if (UnityEngine.Random.Range(0, exactCount) == 0) exact = p;
                }
            }

            Profile chosen = exact ?? relaxed;   // exact first (NR-4); relaxed only on a difficulty miss (AC-X1)
            if (chosen == null) return true;     // nothing of this Side+Role → vanilla → null → LoadBots(3)

            if (withDelete) profiles2Select.Remove(chosen);   // same semantics as vanilla :91-94
            __result = chosen;
            if (debug)
                Plugin.LogSource.LogInfo($"[TRLDynamicSpawn Logger] CHOSEN PROFILE: '{chosen.Nickname}' (Side={chosen.Info.Side}, Role={chosen.Info.Settings?.Role}, Diff={chosen.Info.Settings?.BotDifficulty}) for {role} ({diff}){(exact == null ? " [difficulty relaxed]" : "")}");
            return false; // skips original method
        }

        // ref: EFT/Profile.cs:632 (InfoClass Info) · InfoClass.cs:123 (ProfileInfoSettingsClass Settings) · ProfileInfoSettingsClass.cs:7/:9 (Role, BotDifficulty)
        // Side covers any USEC/BEAR profile regardless of Role; Role covers pmcUSEC/pmcBEAR (EFT/WildSpawnType.cs).
        // No spt* roles exist in 0.16.9 — the old ToString().Contains() heuristic is dropped on purpose (PA-02-01).
        private static bool PmcMatches(InfoClass info, ProfileInfoSettingsClass st, WildSpawnType requested)
        {
            EPlayerSide wantedSide = requested == WildSpawnType.pmcUSEC ? EPlayerSide.Usec : EPlayerSide.Bear;
            return info.Side == wantedSide || st.Role == requested;
        }
    }

}


