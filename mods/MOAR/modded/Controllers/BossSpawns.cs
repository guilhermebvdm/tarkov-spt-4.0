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
public class BossSpawns(
    ISptLogger<BossSpawns> logger,
    RandomUtil randomUtil)
{
    public List<BossLocationSpawn> GetCustomMapData(string location, double escapeTimeLimit, string configKey, List<BossLocationSpawn> originalBosses)
    {
        var result = new List<BossLocationSpawn>();
        
        if (ModConfig.Config.DisableBosses)
            return result;
            
        var bossesToRemove = new HashSet<string> { 
            "assault", "pmcBEAR", "pmcUSEC", "infectedAssault", 
            "infectedTagilla", "infectedLaborant", "infectedCivil" 
        };
        
        var mainBossNameList = new HashSet<string> {
            "bossKojaniy", "bossGluhar", "bossSanitar", "bossKilla",
            "bossTagilla", "bossKnight", "bossBoar", "bossKolontay",
            "bossPartisan", "bossBully"
        };
        
        // Filter original bosses
        foreach (var originalBoss in originalBosses)
        {
            if (bossesToRemove.Contains(originalBoss.BossName))
                continue;
                
            // Check if it's in bossConfig
            if (ModConfig.BossConfig.TryGetValue(configKey, out var mapBossConfig))
            {
                if (mapBossConfig.TryGetValue(originalBoss.BossName, out var newChance))
                {
                    originalBoss.BossChance = newChance;
                }
            }
            
            // Boss Chance Buff
            if (mainBossNameList.Contains(originalBoss.BossName) && ModConfig.Config.MainBossChanceBuff > 0)
            {
                originalBoss.BossChance += ModConfig.Config.MainBossChanceBuff;
                if (originalBoss.BossChance > 100) originalBoss.BossChance = 100;
            }
            
            // Open Zones
            if (mainBossNameList.Contains(originalBoss.BossName) && ModConfig.Config.BossOpenZones)
            {
                originalBoss.BossZone = "";
            }
            
            // Final Chance check
            if (originalBoss.BossChance > 0)
            {
                var roll = randomUtil.GetInt(1, 100);
                if (roll <= originalBoss.BossChance)
                {
                    originalBoss.BossChance = 100;
                    result.Add(originalBoss);
                }
            }
        }
        
        // Boss Invasions (Injecting bosses from other maps)
        if (ModConfig.Config.BossInvasion && ModConfig.Config.BossInvasionSpawnChance > 0)
        {
            foreach (var bossName in mainBossNameList)
            {
                if (bossName == "bossKnight") continue; // Knight doesn't invade individually usually
                
                // If the boss is already in this map natively, skip invasion
                if (result.Exists(b => b.BossName == bossName)) continue;
                
                if (MapSpawns.AllBosses.TryGetValue(bossName, out var templateBoss))
                {
                    var roll = randomUtil.GetInt(1, 100);
                    if (roll <= ModConfig.Config.BossInvasionSpawnChance)
                    {
                        var invadingBoss = new BossLocationSpawn
                        {
                            BossName = templateBoss.BossName,
                            BossChance = 100,
                            BossZone = "", // Invasions spawn anywhere
                            BossDifficulty = templateBoss.BossDifficulty,
                            BossEscortType = templateBoss.BossEscortType,
                            BossEscortDifficulty = templateBoss.BossEscortDifficulty,
                            BossEscortAmount = templateBoss.BossEscortAmount == "0" ? "0" : "1",
                            Time = -1,
                            Supports = templateBoss.Supports
                        };
                        result.Add(invadingBoss);
                    }
                }
            }
        }
        
        // Random Raider Group (Phase 10)
        if (ModConfig.Config.RandomRaiderGroup && ModConfig.Config.RandomRaiderGroupChance > 0)
        {
            var roll = randomUtil.GetInt(1, 100);
            if (roll <= ModConfig.Config.RandomRaiderGroupChance)
            {
                result.Add(new BossLocationSpawn
                {
                    BossName = "pmcBot",
                    BossChance = 100,
                    BossZone = "",
                    BossDifficulty = "normal",
                    BossEscortType = "pmcBot",
                    BossEscortDifficulty = "normal",
                    BossEscortAmount = "2,3,4",
                    Time = -1,
                    Supports = null
                });
            }
        }
        
        // Random Rogue Group (Phase 10)
        if (ModConfig.Config.RandomRogueGroup && ModConfig.Config.RandomRogueGroupChance > 0)
        {
            var roll = randomUtil.GetInt(1, 100);
            if (roll <= ModConfig.Config.RandomRogueGroupChance)
            {
                result.Add(new BossLocationSpawn
                {
                    BossName = "exUsec",
                    BossChance = 100,
                    BossZone = "",
                    BossDifficulty = "normal",
                    BossEscortType = "exUsec",
                    BossEscortDifficulty = "normal",
                    BossEscortAmount = "2,3,4",
                    Time = -1,
                    Supports = null
                });
            }
        }
        
        return result;
    }
}
