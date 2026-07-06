using MOARServer.Globals;
using MOARServer.Helpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Collections.Generic;

namespace MOARServer.Controllers;

[Injectable]
public class PmcSpawns(
    ISptLogger<PmcSpawns> logger,
    RandomUtil randomUtil)
{
    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit, string configKey)
    {
        var result = new List<BossLocationSpawn>();
        
        if (!ModConfig.MapConfig.TryGetValue(configKey, out var mapConfig))
            return result;
            
        // Calculate escape time ratio
        // Default escape time fallback to 40 if not found, but we will simplify
        double defaultEscapeTime = 40.0;
        if (configKey == "factoryDay" || configKey == "factoryNight") defaultEscapeTime = 20.0;
        
        double escapeTimeRatio = Math.Round(escapeTimeLimit / defaultEscapeTime);
        if (escapeTimeRatio < 1) escapeTimeRatio = 1;
        
        int totalWaves = (int)Math.Round(mapConfig.PmcWaveCount * ModConfig.Config.PmcWaveQuantity * escapeTimeRatio);
        
        if (mapConfig.PmcHotZones.Count > 0 && totalWaves > 0)
        {
            totalWaves += mapConfig.PmcHotZones.Count;
        }
        
        if (ModConfig.Config.Debug)
        {
            logger.Info($"[MOAR] {configKey}: Generating {totalWaves} PMC waves (Ratio: {escapeTimeRatio})");
        }
        
        double botDistribution = ModConfig.Config.PmcWaveDistribution;
        double timeLimit = escapeTimeLimit * 60;
        double workingTimeLimit = timeLimit;
        
        if (ModConfig.Config.StartingPmcs)
        {
            workingTimeLimit = timeLimit * 0.2;
        }
        
        double spawnDelay = ModConfig.MapConfig[configKey].InitialSpawnDelay + randomUtil.GetInt(0, 10);
        
        bool pushToEnd = botDistribution > 1.0;
        bool pullFromEnd = botDistribution < 1.0;
        
        double startTime = pushToEnd
            ? Math.Round((botDistribution - 1.0) * workingTimeLimit)
            : spawnDelay;

        workingTimeLimit = pullFromEnd
            ? Math.Round(workingTimeLimit * botDistribution)
            : Math.Round(workingTimeLimit - startTime);
            
        double averageTime = workingTimeLimit / Math.Max(1, totalWaves);
        
        for (int i = 0; i < totalWaves; i++)
        {
            var isUsec = i % 2 == 0;
            var pmcType = isUsec ? "pmcUSEC" : "pmcBEAR";
            
            var difficulty = MOARServer.Helpers.WaveBuilder.GetDifficulty(ModConfig.Config.PmcDifficulty);
            var bossEscortAmount = (randomUtil.GetInt(1, 100) <= ModConfig.Config.PmcGroupChance * 100) 
                    ? randomUtil.GetInt(1, ModConfig.Config.PmcMaxGroupSize - 1) 
                    : 0;

            var boss = new BossLocationSpawn
            {
                BossName = pmcType,
                BossChance = 100,
                BossZone = "", // In a full implementation, pick from pmcZones
                BossDifficulty = difficulty,
                BossEscortType = pmcType,
                BossEscortDifficulty = difficulty,
                BossEscortAmount = bossEscortAmount.ToString(),
                Time = (int)Math.Round(startTime),
                Delay = 0,
                Supports = []
            };
            
            // Advance the time for the next wave
            startTime += averageTime;
            
            if (mapConfig.PmcHotZones.Count > 0)
            {
                double botToZoneTotal = (double)mapConfig.PmcHotZones.Count / Math.Max(1, totalWaves);
                int zoneIndex = (int)Math.Floor(i * botToZoneTotal);
                if (zoneIndex < mapConfig.PmcHotZones.Count)
                {
                    boss.BossZone = mapConfig.PmcHotZones[zoneIndex];
                }
            }
            
            result.Add(boss);
        }
        
        return result;
    }
}
