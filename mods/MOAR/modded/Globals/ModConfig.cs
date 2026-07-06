using System.Reflection;
using MOARServer.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MOARServer.Globals;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PreSptModLoader)]
public class ModConfig : IOnLoad
{
    private static JsonUtil _jsonUtil;
    private static FileUtil _fileUtil;
    private static ISptLogger<ModConfig> _logger;
    
    public static MOARConfig Config { get; private set; } = null!;
    public static string BaseConfigRaw { get; private set; } = "";
    public static Dictionary<string, Dictionary<string, JsonElement>> Presets { get; private set; } = new();
    public static Dictionary<string, int> PresetWeights { get; private set; } = new();
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
        BaseConfigRaw = rawConfig;
        Config = _jsonUtil.Deserialize<MOARConfig>(rawConfig) ?? throw new ArgumentNullException(nameof(Config));
        
        _logger.Success("[MOAR] Config loaded successfully.");

        // Load Presets
        var presetsPath = Path.Combine(_modPath!, "config", "Presets.json");
        if (File.Exists(presetsPath))
        {
            var rawPresets = await _fileUtil.ReadFileAsync(presetsPath);
            Presets = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(rawPresets) ?? new();
        }

        var weightsPath = Path.Combine(_modPath!, "config", "PresetWeightings.json");
        if (File.Exists(weightsPath))
        {
            var rawWeights = await _fileUtil.ReadFileAsync(weightsPath);
            PresetWeights = JsonSerializer.Deserialize<Dictionary<string, int>>(rawWeights) ?? new();
        }

        ApplyPreset(Config.CurrentPreset);

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

    public static void SetPreset(string preset)
    {
        Config.CurrentPreset = preset;
        SaveConfig();
        ApplyPreset(preset);
    }

    public static void SaveConfig()
    {
        var configPath = Path.Combine(_modPath!, "config", "config.json");
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(configPath, JsonSerializer.Serialize(Config, options));
        BaseConfigRaw = File.ReadAllText(configPath);
    }

    public static void SetOverrideConfig(MOARConfig overrides)
    {
        // For custom preset, we just overwrite the config and do not save to disk 
        // to avoid permanently overwriting base config if we are in Custom mode.
        // Or we could serialize it into BaseConfigRaw...
        // For now, if "Custom" is selected, the client will send this override.
        Config = overrides;
        Config.CurrentPreset = "Custom";
        SaveConfig();
    }

    public static void ApplyPreset(string preset)
    {
        // Reset to base
        Config = _jsonUtil.Deserialize<MOARConfig>(BaseConfigRaw) ?? new MOARConfig();
        Config.CurrentPreset = preset; // restore the preset name after reset
        
        string activePreset = preset;
        if (preset.Equals("Random", StringComparison.OrdinalIgnoreCase))
        {
            activePreset = RollRandomPreset();
        }

        if (Presets.TryGetValue(activePreset, out var overrides))
        {
            var properties = typeof(MOARConfig).GetProperties();
            foreach (var kvp in overrides)
            {
                var prop = properties.FirstOrDefault(p => 
                    p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == kvp.Key);
                
                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        var value = JsonSerializer.Deserialize(kvp.Value.GetRawText(), prop.PropertyType);
                        prop.SetValue(Config, value);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"[MOAR] Failed to override {kvp.Key}: {ex.Message}");
                    }
                }
            }
            _logger.Success($"[MOAR] Applied Preset: {activePreset}");
        }
    }

    private static string RollRandomPreset()
    {
        if (PresetWeights.Count == 0) return "live-like";
        
        int totalWeight = PresetWeights.Values.Sum();
        int roll = Random.Shared.Next(0, totalWeight);
        
        int current = 0;
        foreach (var kvp in PresetWeights)
        {
            current += kvp.Value;
            if (roll < current) return kvp.Key;
        }
        
        return PresetWeights.Keys.First();
    }
}
