using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MOARServer.Models;

public class MapConfigData
{
    [JsonPropertyName("sniperQuantity")]
    public int SniperQuantity { get; set; }

    [JsonPropertyName("initialSpawnDelay")]
    public int InitialSpawnDelay { get; set; }

    [JsonPropertyName("smoothingDistribution")]
    public double SmoothingDistribution { get; set; }

    [JsonPropertyName("mapCullingNearPointValuePlayer")]
    public int MapCullingNearPointValuePlayer { get; set; }

    [JsonPropertyName("mapCullingNearPointValuePmc")]
    public int MapCullingNearPointValuePmc { get; set; }

    [JsonPropertyName("mapCullingNearPointValueScav")]
    public int MapCullingNearPointValueScav { get; set; }

    [JsonPropertyName("spawnMinDistance")]
    public int SpawnMinDistance { get; set; }

    [JsonPropertyName("maxBotCapOverride")]
    public int? MaxBotCapOverride { get; set; }

    [JsonPropertyName("maxBotPerZoneOverride")]
    public int? MaxBotPerZoneOverride { get; set; }

    [JsonPropertyName("pmcWaveCount")]
    public int PmcWaveCount { get; set; }

    [JsonPropertyName("scavWaveCount")]
    public int ScavWaveCount { get; set; }

    [JsonPropertyName("zombieWaveCount")]
    public int ZombieWaveCount { get; set; }

    [JsonPropertyName("scavHotZones")]
    public List<string> ScavHotZones { get; set; } = new();

    [JsonPropertyName("pmcHotZones")]
    public List<string> PmcHotZones { get; set; } = new();
}
