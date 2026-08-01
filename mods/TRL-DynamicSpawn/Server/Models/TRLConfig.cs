using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace TRLDynamicSpawnServer.Models;

public record TRLConfig
{
    [JsonPropertyName("activePreset")] 
    public string ActivePreset { get; set; } = "Balanced"; // "Equilibrado", "Guerra de PMCs", "Infestação de Scavs", "Aleatório"

    [JsonPropertyName("pmcDifficulty")] 
    public Dictionary<string, double> PmcDifficulty { get; set; } = new()
    {
        { "easy", 10 },
        { "normal", 50 },
        { "hard", 30 },
        { "impossible", 10 }
    };

    [JsonPropertyName("scavDifficulty")] 
    public Dictionary<string, double> ScavDifficulty { get; set; } = new()
    {
        { "easy", 10 },
        { "normal", 50 },
        { "hard", 30 },
        { "impossible", 10 }
    };

    [JsonPropertyName("bossDifficulty")] 
    public Dictionary<string, double> BossDifficulty { get; set; } = new()
    {
        { "easy", 0 },
        { "normal", 60 },
        { "hard", 30 },
        { "impossible", 10 }
    };

    [JsonPropertyName("mapTimers")] 
    public Dictionary<string, WaveTimerConfig> MapTimers { get; set; } = new();

    [JsonPropertyName("eliteConfig")] 
    public EliteConfig EliteConfig { get; set; } = new();

    [JsonPropertyName("customSpawnsConfig")]
    public CustomSpawnsConfig CustomSpawnsConfig { get; set; } = new();

    [JsonPropertyName("mapConfigs")]
    public Dictionary<string, MapSettings> MapConfigs { get; set; } = new();

    [JsonPropertyName("globalAntiOverlapDistance")]
    public double GlobalAntiOverlapDistance { get; set; } = 5.0;

    [JsonPropertyName("enableMapOverlapCulling")]
    public bool EnableMapOverlapCulling { get; set; } = true;
}

public record CustomSpawnsConfig
{
    [JsonPropertyName("enableCustomPmcSpawns")] public bool EnableCustomPmcSpawns { get; set; } = false;
    [JsonPropertyName("enableCustomScavSpawns")] public bool EnableCustomScavSpawns { get; set; } = false;
    [JsonPropertyName("enableCustomSniperSpawns")] public bool EnableCustomSniperSpawns { get; set; } = false;
    [JsonPropertyName("enableCustomPlayerSpawns")] public bool EnableCustomPlayerSpawns { get; set; } = false;
}

public record WaveTimerConfig
{
    [JsonPropertyName("delayBeforeFirstWave")] public int DelayBeforeFirstWave { get; set; } = 60;
    [JsonPropertyName("secondsBetweenWaves")] public int SecondsBetweenWaves { get; set; } = 360;
}

public record EliteConfig
{
    [JsonPropertyName("disableBosses")] public bool DisableBosses { get; set; } = false;
    [JsonPropertyName("bossOpenZones")] public bool BossOpenZones { get; set; } = false;
    [JsonPropertyName("bossInvasion")] public BossInvasionConfig BossInvasion { get; set; } = new();
    [JsonPropertyName("randomRaiderGroup")] public bool RandomRaiderGroup { get; set; } = false;
    [JsonPropertyName("randomRaiderGroupChance")] public int RandomRaiderGroupChance { get; set; } = 10;
    [JsonPropertyName("randomRogueGroup")] public bool RandomRogueGroup { get; set; } = false;
    [JsonPropertyName("randomRogueGroupChance")] public int RandomRogueGroupChance { get; set; } = 10;

    [JsonPropertyName("bossKnight")] public EliteLocationInfo BossKnight { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Customs = 30, Lighthouse = 30, Shoreline = 30, Woods = 30 },
        BossZone = new ValidLocationString { Customs = "ZoneScavBase", Lighthouse = "Zone_TreatmentContainers,Zone_Chalet", Shoreline = "ZoneMeteoStation", Woods = "ZoneScavBase2" }
    };
    [JsonPropertyName("bossTagilla")] public EliteLocationInfo BossTagilla { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Factory4Day = 30, Factory4Night = 30 },
        BossZone = new ValidLocationString { Factory4Day = "BotZone", Factory4Night = "BotZone" }
    };
    [JsonPropertyName("bossKilla")] public EliteLocationInfo BossKilla { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Interchange = 30 },
        BossZone = new ValidLocationString { Interchange = "ZoneCenterBot,ZoneCenter,ZoneOLI,ZoneIDEA,ZoneGoshan" }
    };
    [JsonPropertyName("bossZryachiy")] public EliteLocationInfo BossZryachiy { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Lighthouse = 100 },
        BossZone = new ValidLocationString { Lighthouse = "Zone_Island" }
    };
    [JsonPropertyName("bossGluhar")] public EliteLocationInfo BossGluhar { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Reserve = 30 },
        BossZone = new ValidLocationString { Reserve = "ZoneRailStrorage,ZonePTOR2,ZoneBarrack,ZoneSubStorage" }
    };
    [JsonPropertyName("bossSanitar")] public EliteLocationInfo BossSanitar { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Shoreline = 30 },
        BossZone = new ValidLocationString { Shoreline = "ZoneGreenHouses,ZoneSanatorium1,ZoneSanatorium2,ZonePort" }
    };
    [JsonPropertyName("bossKolontay")] public EliteLocationInfo BossKolontay { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { GroundZero = 30, TarkovStreets = 30 },
        BossZone = new ValidLocationString { GroundZero = "ZoneSandbox", TarkovStreets = "ZoneClimova,ZoneMvd" }
    };
    [JsonPropertyName("bossReshala")] public EliteLocationInfo BossReshala { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Customs = 30 },
        BossZone = new ValidLocationString { Customs = "ZoneDormitory,ZoneGasStation,ZoneScavBase" }
    };
    [JsonPropertyName("bossKaban")] public EliteLocationInfo BossKaban { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { TarkovStreets = 30 },
        BossZone = new ValidLocationString { TarkovStreets = "ZoneCarShowroom" }
    };
    [JsonPropertyName("bossShturman")] public EliteLocationInfo BossShturman { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Woods = 30 },
        BossZone = new ValidLocationString { Woods = "ZoneWoodCutter" }
    };
    [JsonPropertyName("bossPartisan")] public EliteLocationInfo BossPartisan { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Customs = 30, Lighthouse = 30, Shoreline = 30, Woods = 30 }
    };
    [JsonPropertyName("pmcBot")] public EliteLocationInfo Raiders { get; set; } = new() { SpawnChance = new ValidLocationInt { Laboratory = 40 }, BossZone = new ValidLocationString { Laboratory = "BotZoneBasement,BotZoneFloor1,BotZoneFloor2" } }; // Raiders
    [JsonPropertyName("exUsec")] public EliteLocationInfo Rogues { get; set; } = new(); // Rogues
    [JsonPropertyName("arenaFighterEvent")] public EliteLocationInfo Bloodhounds { get; set; } = new() { SpawnChance = new ValidLocationInt { Customs = 5, Woods = 5 }, BossZone = new ValidLocationString { Customs = "ZoneFactoryCenter,ZoneScavBase", Woods = "ZoneMiniHouse,ZoneClearVill,ZoneRoad,ZoneBrokenVill,ZoneScavBase2" } }; // Bloodhounds
    [JsonPropertyName("sectantPriest")] public EliteLocationInfo Cultists { get; set; } = new() { SpawnChance = new ValidLocationInt { Customs = 15, Factory4Night = 20, Shoreline = 15, Woods = 15, GroundZero = 44 }, BossZone = new ValidLocationString { Customs = "ZoneScavBase", Factory4Night = "BotZone", Shoreline = "ZoneSanatorium1,ZoneSanatorium2,ZoneForestSpawn", Woods = "ZoneMiniHouse,ZoneBrokenVill", GroundZero = "ZoneSandbox" } }; // Cultists
    [JsonPropertyName("gifter")] public EliteLocationInfo BossGifter { get; set; } = new(); // Santa

    // Regular Bots (For Hotzone configuration)
    [JsonPropertyName("sptBear")] public EliteLocationInfo Bear { get; set; } = new();
    [JsonPropertyName("sptUsec")] public EliteLocationInfo Usec { get; set; } = new();
    [JsonPropertyName("assault")] public EliteLocationInfo Scav { get; set; } = new();
}

public record BossInvasionConfig
{
    [JsonPropertyName("enable")] public bool Enable { get; set; } = false;
    [JsonPropertyName("bossChance")] public int BossChance { get; set; } = 15;
    [JsonPropertyName("mapChance")] public int MapChance { get; set; } = 15;
    [JsonPropertyName("selectedBosses")] public List<string> SelectedBosses { get; set; } = new() { "Random" };
    [JsonPropertyName("selectedMaps")] public List<string> SelectedMaps { get; set; } = new() { "Random" };
}

public record EliteLocationInfo
{
    [JsonPropertyName("enable")] public bool Enable { get; set; } = true;
    [JsonPropertyName("disableFollowers")] public bool DisableFollowers { get; set; } = false;
    [JsonPropertyName("groupChance")] public int GroupChance { get; set; } = 30;
    [JsonPropertyName("maxGroupSize")] public int MaxGroupSize { get; set; } = 3;
    [JsonPropertyName("spawnChance")] public ValidLocationInt SpawnChance { get; set; } = new();
    [JsonPropertyName("bossZone")] public ValidLocationString BossZone { get; set; } = new();
}

public record ValidLocationInt
{
    [JsonPropertyName("customs")] public int Customs { get; set; } = 0;
    [JsonPropertyName("factory4_day")] public int Factory4Day { get; set; } = 0;
    [JsonPropertyName("factory4_night")] public int Factory4Night { get; set; } = 0;
    [JsonPropertyName("interchange")] public int Interchange { get; set; } = 0;
    [JsonPropertyName("laboratory")] public int Laboratory { get; set; } = 0;
    [JsonPropertyName("lighthouse")] public int Lighthouse { get; set; } = 0;
    [JsonPropertyName("rezervbase")] public int Reserve { get; set; } = 0;
    [JsonPropertyName("sandbox")] public int GroundZero { get; set; } = 0;
    [JsonPropertyName("sandbox_high")] public int GroundZeroHigh { get; set; } = 0;
    [JsonPropertyName("shoreline")] public int Shoreline { get; set; } = 0;
    [JsonPropertyName("tarkovstreets")] public int TarkovStreets { get; set; } = 0;
    [JsonPropertyName("woods")] public int Woods { get; set; } = 0;
    [JsonPropertyName("labyrinth")] public int Labyrinth { get; set; } = 0;
}

public record ValidLocationString
{
    [JsonPropertyName("customs")] public string Customs { get; set; } = "";
    [JsonPropertyName("factory4_day")] public string Factory4Day { get; set; } = "";
    [JsonPropertyName("factory4_night")] public string Factory4Night { get; set; } = "";
    [JsonPropertyName("interchange")] public string Interchange { get; set; } = "";
    [JsonPropertyName("laboratory")] public string Laboratory { get; set; } = "";
    [JsonPropertyName("lighthouse")] public string Lighthouse { get; set; } = "";
    [JsonPropertyName("rezervbase")] public string Reserve { get; set; } = "";
    [JsonPropertyName("sandbox")] public string GroundZero { get; set; } = "";
    [JsonPropertyName("sandbox_high")] public string GroundZeroHigh { get; set; } = "";
    [JsonPropertyName("shoreline")] public string Shoreline { get; set; } = "";
    [JsonPropertyName("tarkovstreets")] public string TarkovStreets { get; set; } = "";
    [JsonPropertyName("woods")] public string Woods { get; set; } = "";
    [JsonPropertyName("labyrinth")] public string Labyrinth { get; set; } = "";
}

public record MapSettings
{
    [JsonPropertyName("secondsBetweenWaves")] public int SecondsBetweenWaves { get; set; } = 360;
    [JsonPropertyName("sniperChance")] public int SniperChance { get; set; } = 30;
    [JsonPropertyName("safeZoneDistance")] public double SafeZoneDistance { get; set; } = 30.0;
    
    [JsonPropertyName("enableDespawn")] public bool EnableDespawn { get; set; } = false;
    [JsonPropertyName("despawnPMCs")] public bool DespawnPMCs { get; set; } = false;
    [JsonPropertyName("despawnDistance")] public float DespawnDistance { get; set; } = 300f;
    [JsonPropertyName("despawnInterval")] public float DespawnInterval { get; set; } = 20f;
    [JsonPropertyName("spawnBubbleDistance")] public float SpawnBubbleDistance { get; set; } = 300f;
    [JsonPropertyName("enableSpawnBubble")] public bool EnableSpawnBubble { get; set; } = true;
    [JsonPropertyName("teleportMinDistance")] public float TeleportMinDistance { get; set; } = 100f;
}








