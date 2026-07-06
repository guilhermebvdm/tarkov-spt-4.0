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
        private static bool PatchPrefix(ref System.Threading.Tasks.Task __result)
        {
            if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
            {
                Plugin.LogSource.LogInfo("[TRLDynamicSpawn] Blocked Vanilla Normal Wave to give 100% control to DynamicSpawn.");
            }
            __result = System.Threading.Tasks.Task.CompletedTask;
            return false;
        }
    }

    public class DisableVanillaBossWavesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.ActivateBotsByWave), new[] { typeof(BossLocationSpawn) });
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            if (TRLDynamicSpawn.Helpers.Settings.enableDebugLogs.Value)
            {
                Plugin.LogSource.LogInfo("[TRLDynamicSpawn] Blocked Vanilla Boss Wave to give 100% control to DynamicSpawn.");
            }
            return false;
        }
    }

}


