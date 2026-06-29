using MOARServer.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace MOARServer.Controllers;

[Injectable(InjectionType = InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader + 500)]
public class MapSpawns(
    ISptLogger<MapSpawns> logger,
    ICloner cloner,
    DatabaseService databaseService,
    PmcSpawns pmcSpawns,
    ScavSpawns scavSpawns,
    BossSpawns bossSpawns,
    ZombieSpawns zombieSpawns)
    : IOnLoad
{
    private readonly List<string> _validMaps =
    [
        "bigmap",
        "factory4_day",
        "factory4_night",
        "interchange",
        "laboratory",
        "lighthouse",
        "rezervbase",
        "sandbox",
        "sandbox_high",
        "shoreline",
        "tarkovstreets",
        "woods",
        "sandbox",
        "sandbox_high"
    ];

    private readonly Dictionary<string, string> _mapToConfigKey = new()
    {
        { "bigmap", "customs" },
        { "factory4_day", "factoryDay" },
        { "factory4_night", "factoryNight" },
        { "interchange", "interchange" },
        { "laboratory", "laboratory" },
        { "lighthouse", "lighthouse" },
        { "rezervbase", "rezervbase" },
        { "shoreline", "shoreline" },
        { "tarkovstreets", "tarkovstreets" },
        { "woods", "woods" },
        { "sandbox", "gzLow" },
        { "sandbox_high", "gzHigh" }
    };

    public Task OnLoad()
    {
        if (ModConfig.Config.EnableBotSpawning)
        {
            logger.Info("[MOAR]: Starting up, may the bots ever be in your favour!");
            ConfigureInitialData();
        }
        return Task.CompletedTask;
    }
    
    // Global template for Boss Invasions
    public static Dictionary<string, BossLocationSpawn> AllBosses = new();

    private void ConfigureInitialData()
    {
        var _locationData = databaseService.GetLocations().GetDictionary();
        
        // Build AllBosses dictionary first
        foreach (var map in _validMaps)
        {
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            if (!_locationData.ContainsKey(actualKey)) continue;
            
            foreach (var boss in _locationData[actualKey].Base.BossLocationSpawn)
            {
                if (!AllBosses.ContainsKey(boss.BossName))
                {
                    AllBosses[boss.BossName] = cloner.Clone(boss);
                }
            }
        }
        
        foreach (var map in _validMaps)
        {
            var configKey = _mapToConfigKey[map];
            var actualKey = databaseService.GetLocations().GetMappedKey(map);
            if (!_locationData.ContainsKey(actualKey)) continue;
            
            var escapeTime = _locationData[actualKey].Base.EscapeTimeLimit.GetValueOrDefault();
            
            MOARServer.Helpers.SpawnZoneCulling.ApplyCullingAndInjection(_locationData[actualKey].Base, map, configKey);
            
            var pmcs = pmcSpawns.GetCustomMapData(map, escapeTime, configKey);
            var scavs = scavSpawns.GetCustomMapData(map, escapeTime, configKey);
            var zombies = zombieSpawns.GetCustomMapData(map, escapeTime, configKey);
            
            // Pass a clone of the original BossLocationSpawn so BossSpawns can filter and keep actual bosses
            var originalBosses = _locationData[actualKey].Base.BossLocationSpawn.ToList();
            var bosses = bossSpawns.GetCustomMapData(map, escapeTime, configKey, originalBosses);
            
            // Clear existing
            _locationData[actualKey].Base.BossLocationSpawn.Clear();
            _locationData[actualKey].Base.Waves.Clear();
            
            _locationData[actualKey].Base.BossLocationSpawn.AddRange(pmcs);
            _locationData[actualKey].Base.BossLocationSpawn.AddRange(zombies);
            _locationData[actualKey].Base.BossLocationSpawn.AddRange(scavs);
            _locationData[actualKey].Base.BossLocationSpawn.AddRange(bosses);
            
            double smoothing = ModConfig.MapConfig.TryGetValue(configKey, out var mc) ? mc.SmoothingDistribution : 1.0;
            MOARServer.Helpers.WaveBuilder.EnforceSmoothing(_locationData[actualKey].Base.BossLocationSpawn, smoothing);
        }
    }
}
