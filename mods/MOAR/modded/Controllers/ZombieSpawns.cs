using MOARServer.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using System;
using System.Collections.Generic;

namespace MOARServer.Controllers;

[Injectable]
public class ZombieSpawns(
    ISptLogger<ZombieSpawns> logger,
    RandomUtil randomUtil,
    DatabaseService databaseService)
{
    private bool _healthModified = false;

    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit, string configKey)
    {
        var result = new List<BossLocationSpawn>();
        
        if (!ModConfig.Config.ZombiesEnabled)
            return result;
            
        // Modify zombie health profiles globally (only once)
        if (!_healthModified)
        {
            ModifyZombieHealth();
            _healthModified = true;
        }

        if (!ModConfig.MapConfig.TryGetValue(configKey, out var mapConfig))
            return result;

        int baseZombieWaveCount = mapConfig.ZombieWaveCount;
        if (baseZombieWaveCount <= 0) return result;

        // Ratio logic based on default escape times
        double defaultEscapeTime = 40.0;
        switch (configKey)
        {
            case "factoryDay": defaultEscapeTime = 20.0; break;
            case "factoryNight": defaultEscapeTime = 25.0; break;
            case "laboratory":
            case "gzLow":
            case "gzHigh": defaultEscapeTime = 35.0; break;
            case "shoreline": defaultEscapeTime = 45.0; break;
            case "tarkovstreets": defaultEscapeTime = 50.0; break;
            default: defaultEscapeTime = 40.0; break;
        }

        double escapeTimeLimitRatio = (escapeTimeLimit / 60.0) / defaultEscapeTime;
        int zombieTotalWaveCount = (int)Math.Round(baseZombieWaveCount * ModConfig.Config.ZombieWaveQuantity * escapeTimeLimitRatio);

        if (zombieTotalWaveCount <= 0) return result;

        logger.Info($"[MOAR] {configKey}: Generating {zombieTotalWaveCount} Zombie waves");

        double botDistribution = ModConfig.Config.ZombieWaveDistribution;
        
        // Simplified pushToEnd/pullFromEnd logic
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
            
        double averageTime = workingTimeLimit / Math.Max(1, zombieTotalWaveCount);

        string[] zombieTypes = { "infectedassault", "infectedpmc", "infectedlaborant", "infectedcivil" };

        for (int i = 0; i < zombieTotalWaveCount; i++)
        {
            var allowGroup = randomUtil.GetInt(1, 100) < 20;
            var bossEscortAmount = allowGroup ? randomUtil.GetInt(0, 4) : 0;
            
            var zombieGroup = new BossLocationSpawn
            {
                BossName = zombieTypes[randomUtil.GetInt(0, zombieTypes.Length - 1)],
                BossChance = 100,
                BossZone = "",
                BossDifficulty = "normal",
                BossEscortType = zombieTypes[randomUtil.GetInt(0, zombieTypes.Length - 1)],
                BossEscortDifficulty = "normal",
                BossEscortAmount = "0",
                Delay = 0,
                IgnoreMaxBots = false,
                Time = (int)Math.Round(startTime),
                TriggerId = "",
                TriggerName = ""
            };
            
            var supportsList = new List<BossSupport>();
            for (int j = 0; j < bossEscortAmount; j++)
            {
                supportsList.Add(new BossSupport
                {
                    BossEscortType = zombieTypes[randomUtil.GetInt(0, zombieTypes.Length - 1)],
                    BossEscortAmount = "1"
                });
            }
            zombieGroup.Supports = supportsList;
            
            result.Add(zombieGroup);
            
            startTime += averageTime;
        }

        return result;
    }

    private void ModifyZombieHealth()
    {
        // TODO: Figure out correct BotType health property in C# SPT Server
        /*
        var bots = databaseService.GetBots().Types;
        string[] zombieTypes = { "infectedassault", "infectedpmc", "infectedlaborant", "infectedcivil" };
        
        float num = (float)ModConfig.Config.ZombieHealth;
        int num35 = (int)Math.Round(35 * num);
        int num100 = (int)Math.Round(100 * num);
        int num70 = (int)Math.Round(70 * num);
        int num80 = (int)Math.Round(80 * num);
        
        foreach (var type in zombieTypes)
        {
            if (bots.TryGetValue(type, out var botType))
            {
                // Health property modification goes here
            }
        }
        */
    }
}
