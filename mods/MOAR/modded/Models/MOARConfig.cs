using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace MOARServer.Models;

public class MOARConfig : IRequestData
{
    [JsonPropertyName("currentPreset")]
    public string CurrentPreset { get; set; } = "Random";

    [JsonPropertyName("enableBotSpawning")]
    public bool EnableBotSpawning { get; set; }

    [JsonPropertyName("spawnSmoothing")]
    public bool SpawnSmoothing { get; set; }

    [JsonPropertyName("pmcDifficulty")]
    public double PmcDifficulty { get; set; }

    [JsonPropertyName("scavDifficulty")]
    public double ScavDifficulty { get; set; }

    [JsonPropertyName("scavWaveDistribution")]
    public double ScavWaveDistribution { get; set; }

    [JsonPropertyName("scavWaveQuantity")]
    public double ScavWaveQuantity { get; set; }

    [JsonPropertyName("startingPmcs")]
    public bool StartingPmcs { get; set; }

    [JsonPropertyName("pmcWaveDistribution")]
    public double PmcWaveDistribution { get; set; }

    [JsonPropertyName("pmcWaveQuantity")]
    public double PmcWaveQuantity { get; set; }

    [JsonPropertyName("randomSpawns")]
    public bool RandomSpawns { get; set; }

    [JsonPropertyName("zombiesEnabled")]
    public bool ZombiesEnabled { get; set; }

    [JsonPropertyName("zombieWaveDistribution")]
    public double ZombieWaveDistribution { get; set; }

    [JsonPropertyName("zombieWaveQuantity")]
    public double ZombieWaveQuantity { get; set; }

    [JsonPropertyName("zombieHealth")]
    public double ZombieHealth { get; set; }

    [JsonPropertyName("maxBotCap")]
    public int MaxBotCap { get; set; }

    [JsonPropertyName("maxBotPerZone")]
    public int MaxBotPerZone { get; set; }

    [JsonPropertyName("sniperGroupChance")]
    public double SniperGroupChance { get; set; }

    [JsonPropertyName("scavGroupChance")]
    public double ScavGroupChance { get; set; }

    [JsonPropertyName("pmcGroupChance")]
    public double PmcGroupChance { get; set; }

    [JsonPropertyName("pmcMaxGroupSize")]
    public int PmcMaxGroupSize { get; set; }

    [JsonPropertyName("scavMaxGroupSize")]
    public int ScavMaxGroupSize { get; set; }

    [JsonPropertyName("sniperMaxGroupSize")]
    public double SniperMaxGroupSize { get; set; }

    [JsonPropertyName("bossOpenZones")]
    public bool BossOpenZones { get; set; }

    [JsonPropertyName("randomRaiderGroup")]
    public bool RandomRaiderGroup { get; set; }

    [JsonPropertyName("randomRaiderGroupChance")]
    public int RandomRaiderGroupChance { get; set; }

    [JsonPropertyName("randomRogueGroup")]
    public bool RandomRogueGroup { get; set; }

    [JsonPropertyName("randomRogueGroupChance")]
    public int RandomRogueGroupChance { get; set; }

    [JsonPropertyName("disableBosses")]
    public bool DisableBosses { get; set; }

    [JsonPropertyName("mainBossChanceBuff")]
    public int MainBossChanceBuff { get; set; }

    [JsonPropertyName("bossInvasion")]
    public bool BossInvasion { get; set; }

    [JsonPropertyName("bossInvasionSpawnChance")]
    public int BossInvasionSpawnChance { get; set; }

    [JsonPropertyName("gradualBossInvasion")]
    public bool GradualBossInvasion { get; set; }

    [JsonPropertyName("debug")]
    public bool Debug { get; set; }
}
