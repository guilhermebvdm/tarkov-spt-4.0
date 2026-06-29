using MOARServer.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Collections.Generic;

namespace MOARServer.Controllers;

[Injectable]
public class ScavSpawns(
    ISptLogger<ScavSpawns> logger,
    RandomUtil randomUtil)
{
    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit, string configKey)
    {
        var result = new List<BossLocationSpawn>();
        
        if (!ModConfig.MapConfig.TryGetValue(configKey, out var mapConfig))
            return result;
            
        double defaultEscapeTime = 40.0;
        if (configKey == "factoryDay" || configKey == "factoryNight") defaultEscapeTime = 20.0;
        
        double escapeTimeRatio = Math.Round(escapeTimeLimit / defaultEscapeTime);
        if (escapeTimeRatio < 1) escapeTimeRatio = 1;
        
        int totalWaves = (int)Math.Round(mapConfig.ScavWaveCount * ModConfig.Config.ScavWaveQuantity * escapeTimeRatio);
        
        if (mapConfig.ScavHotZones.Count > 0 && totalWaves > 0)
        {
            totalWaves += mapConfig.ScavHotZones.Count;
        }
        
        double botDistribution = ModConfig.Config.ScavWaveDistribution;
        double timeLimit = escapeTimeLimit * 60;
        double workingTimeLimit = timeLimit;
        
        bool pushToEnd = botDistribution > 1.0;
        bool pullFromEnd = botDistribution < 1.0;
        
        double spawnDelay = 0;
        double startTime = pushToEnd
            ? Math.Round((botDistribution - 1.0) * workingTimeLimit)
            : spawnDelay;

        workingTimeLimit = pullFromEnd
            ? Math.Round(workingTimeLimit * botDistribution)
            : Math.Round(workingTimeLimit - startTime);
            
        double averageTime = workingTimeLimit / Math.Max(1, totalWaves);
        
        if (ModConfig.Config.Debug)
        {
            logger.Info($"[MOAR] {configKey}: Generating {totalWaves} Scav waves (Ratio: {escapeTimeRatio})");
        }
        
        for (int i = 0; i < totalWaves; i++)
        {
            var scav = new BossLocationSpawn
            {
                BossName = "assault",
                BossChance = 100,
                BossZone = "",
                BossDifficulty = "normal",
                BossEscortType = "assault",
                BossEscortDifficulty = "normal",
                BossEscortAmount = (randomUtil.GetInt(1, 100) <= ModConfig.Config.ScavGroupChance * 100) 
                    ? randomUtil.GetInt(1, ModConfig.Config.ScavMaxGroupSize - 1).ToString() 
                    : "0",
                Time = (int)Math.Round(startTime),
                Delay = 0,
                Supports = []
            };
            
            if (mapConfig.ScavHotZones.Count > 0)
            {
                double botToZoneTotal = (double)mapConfig.ScavHotZones.Count / Math.Max(1, totalWaves);
                int zoneIndex = (int)Math.Floor(i * botToZoneTotal);
                if (zoneIndex < mapConfig.ScavHotZones.Count)
                {
                    scav.BossZone = mapConfig.ScavHotZones[zoneIndex];
                }
            }
            
            result.Add(scav);
        }
        
        // Snipers (Marksman)
        int sniperWaves = mapConfig.SniperQuantity;
        if (sniperWaves > 0)
        {
            double averageSniperTime = timeLimit / Math.Max(1, sniperWaves);
            double sniperStartTime = 0;
            
            for (int i = 0; i < sniperWaves; i++)
            {
                var sniper = new BossLocationSpawn
                {
                    BossName = "marksman",
                    BossChance = 100,
                    BossZone = "",
                    BossDifficulty = "normal",
                    BossEscortType = "marksman",
                    BossEscortDifficulty = "normal",
                    BossEscortAmount = "0",
                    Time = (int)Math.Round(sniperStartTime),
                    Delay = 0,
                    Supports = []
                };
                
                result.Add(sniper);
                sniperStartTime += averageSniperTime;
            }
        }
        
        return result;
    }
}
