using System.Reflection;
using MOARServer.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace MOARServer.Globals;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PreSptModLoader)]
public class ModConfig : IOnLoad
{
    private static JsonUtil _jsonUtil;
    private static FileUtil _fileUtil;
    private static ISptLogger<ModConfig> _logger;
    
    public static MOARConfig Config { get; private set; } = null!;
    public static SpawnLocations PmcSpawns { get; private set; } = null!;
    public static SpawnLocations ScavSpawns { get; private set; } = null!;
    public static SpawnLocations SniperSpawns { get; private set; } = null!;
    public static SpawnLocations PlayerSpawns { get; private set; } = null!;
    public static Dictionary<string, MapConfigData> MapConfig { get; private set; } = null!;
    public static Dictionary<string, Dictionary<string, int>> BossConfig { get; private set; } = null!;
    
    private static string? _modPath;

    public ModConfig(
        JsonUtil jsonUtil,
        FileUtil fileUtil,
        ModHelper modHelper,
        ISptLogger<ModConfig> logger)
    {
        _modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;
        _logger = logger;
    }
    
    public async Task OnLoad()
    {
        var configPath = Path.Combine(_modPath!, "config", "config.json");
        
        if (!File.Exists(configPath))
        {
            _logger.Error($"[MOAR] config.json not found at {configPath}");
            throw new FileNotFoundException($"config.json not found at {configPath}");
        }
        
        var rawConfig = await _fileUtil.ReadFileAsync(configPath);
        Config = _jsonUtil.Deserialize<MOARConfig>(rawConfig) ?? throw new ArgumentNullException(nameof(Config));
        
        _logger.Success("[MOAR] Config loaded successfully.");

        var pmcSpawnsPath = Path.Combine(_modPath!, "config", "Spawns", "pmcSpawns.json");
        var scavSpawnsPath = Path.Combine(_modPath!, "config", "Spawns", "scavSpawns.json");
        var sniperSpawnsPath = Path.Combine(_modPath!, "config", "Spawns", "sniperSpawns.json");
        var playerSpawnsPath = Path.Combine(_modPath!, "config", "Spawns", "playerSpawns.json");

        PmcSpawns = _jsonUtil.Deserialize<SpawnLocations>(await _fileUtil.ReadFileAsync(pmcSpawnsPath)) ?? new SpawnLocations();
        ScavSpawns = _jsonUtil.Deserialize<SpawnLocations>(await _fileUtil.ReadFileAsync(scavSpawnsPath)) ?? new SpawnLocations();
        SniperSpawns = _jsonUtil.Deserialize<SpawnLocations>(await _fileUtil.ReadFileAsync(sniperSpawnsPath)) ?? new SpawnLocations();
        PlayerSpawns = _jsonUtil.Deserialize<SpawnLocations>(await _fileUtil.ReadFileAsync(playerSpawnsPath)) ?? new SpawnLocations();
        
        var mapConfigPath = Path.Combine(_modPath!, "config", "mapConfig.json");
        MapConfig = _jsonUtil.Deserialize<Dictionary<string, MapConfigData>>(await _fileUtil.ReadFileAsync(mapConfigPath)) ?? new Dictionary<string, MapConfigData>();
        
        var bossConfigPath = Path.Combine(_modPath!, "config", "bossConfig.json");
        BossConfig = _jsonUtil.Deserialize<Dictionary<string, Dictionary<string, int>>>(await _fileUtil.ReadFileAsync(bossConfigPath)) ?? new Dictionary<string, Dictionary<string, int>>();
        
        _logger.Success("[MOAR] Spawn configurations loaded.");
    }
}
