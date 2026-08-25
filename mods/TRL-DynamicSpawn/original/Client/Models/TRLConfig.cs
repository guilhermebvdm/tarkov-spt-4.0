using Newtonsoft.Json;
using System.Collections.Generic;

namespace TRLDynamicSpawn.Models;

public record TRLConfig
{
    [JsonProperty("activePreset")] 
    public string ActivePreset { get; set; } = "Equilibrado"; // "Equilibrado", "Guerra de PMCs", "Infestacao de Scavs", "Aleatorio"

    [JsonProperty("pmcDifficulty")] 
    public Dictionary<string, double> PmcDifficulty { get; set; } = new()
    {
        { "easy", 10 },
        { "normal", 50 },
        { "hard", 30 },
        { "impossible", 10 }
    };

    [JsonProperty("scavDifficulty")] 
    public Dictionary<string, double> ScavDifficulty { get; set; } = new()
    {
        { "easy", 10 },
        { "normal", 50 },
        { "hard", 30 },
        { "impossible", 10 }
    };

    [JsonProperty("bossDifficulty")] 
    public Dictionary<string, double> BossDifficulty { get; set; } = new()
    {
        { "easy", 0 },
        { "normal", 60 },
        { "hard", 30 },
        { "impossible", 10 }
    };

    [JsonProperty("mapTimers")] 
    public Dictionary<string, WaveTimerConfig> MapTimers { get; set; } = new();

    [JsonProperty("eliteConfig")] 
    public EliteConfig EliteConfig { get; set; } = new();

    [JsonProperty("customSpawnsConfig")]
    public CustomSpawnsConfig CustomSpawnsConfig { get; set; } = new();

    [JsonProperty("mapConfigs")]
    public Dictionary<string, MapSettings> MapConfigs { get; set; } = new();
}

public record CustomSpawnsConfig
{
    [JsonProperty("enableCustomPmcSpawns")] public bool EnableCustomPmcSpawns { get; set; } = false;
    [JsonProperty("enableCustomScavSpawns")] public bool EnableCustomScavSpawns { get; set; } = false;
    [JsonProperty("enableCustomSniperSpawns")] public bool EnableCustomSniperSpawns { get; set; } = false;
    [JsonProperty("enableCustomPlayerSpawns")] public bool EnableCustomPlayerSpawns { get; set; } = false;
}

public record WaveTimerConfig
{
    [JsonProperty("delayBeforeFirstWave")] public int DelayBeforeFirstWave { get; set; } = 60;
    [JsonProperty("secondsBetweenWaves")] public int SecondsBetweenWaves { get; set; } = 360;
}

public record EliteConfig
{
    [JsonProperty("disableBosses")] public bool DisableBosses { get; set; } = false;
    [JsonProperty("bossOpenZones")] public bool BossOpenZones { get; set; } = false;
    [JsonProperty("bossInvasion")] public BossInvasionConfig BossInvasion { get; set; } = new();
    [JsonProperty("randomRaiderGroup")] public bool RandomRaiderGroup { get; set; } = false;
    [JsonProperty("randomRaiderGroupChance")] public int RandomRaiderGroupChance { get; set; } = 10;
    [JsonProperty("randomRogueGroup")] public bool RandomRogueGroup { get; set; } = false;
    [JsonProperty("randomRogueGroupChance")] public int RandomRogueGroupChance { get; set; } = 10;
    [JsonProperty("disableVanillaRogues")] public bool DisableVanillaRogues { get; set; } = false;
    [JsonProperty("disableVanillaRaiders")] public bool DisableVanillaRaiders { get; set; } = false;
    [JsonProperty("lighthouseRogueZoneFilter")] public bool LighthouseRogueZoneFilter { get; set; } = true;
    [JsonProperty("lighthouseRogueMaxCount")] public int LighthouseRogueMaxCount { get; set; } = -1;

    [JsonProperty("bossKnight")] public EliteLocationInfo BossKnight { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Customs = 30, Lighthouse = 30, Shoreline = 30, Woods = 30 },
        BossZone = new ValidLocationString { Customs = "ZoneScavBase", Lighthouse = "Zone_TreatmentContainers,Zone_Chalet", Shoreline = "ZoneMeteoStation", Woods = "ZoneScavBase2" }
    };
    [JsonProperty("bossTagilla")] public EliteLocationInfo BossTagilla { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Factory4Day = 30, Factory4Night = 30 },
        BossZone = new ValidLocationString { Factory4Day = "BotZone", Factory4Night = "BotZone" }
    };
    [JsonProperty("bossKilla")] public EliteLocationInfo BossKilla { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Interchange = 30 },
        BossZone = new ValidLocationString { Interchange = "ZoneCenterBot,ZoneCenter,ZoneOLI,ZoneIDEA,ZoneGoshan" }
    };
    [JsonProperty("bossZryachiy")] public EliteLocationInfo BossZryachiy { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Lighthouse = 100 },
        BossZone = new ValidLocationString { Lighthouse = "Zone_Island" }
    };
    [JsonProperty("bossGluhar")] public EliteLocationInfo BossGluhar { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Reserve = 30 },
        BossZone = new ValidLocationString { Reserve = "ZoneRailStrorage,ZonePTOR2,ZoneBarrack,ZoneSubStorage" }
    };
    [JsonProperty("bossSanitar")] public EliteLocationInfo BossSanitar { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Shoreline = 30 },
        BossZone = new ValidLocationString { Shoreline = "ZoneGreenHouses,ZoneSanatorium1,ZoneSanatorium2,ZonePort" }
    };
    [JsonProperty("bossKolontay")] public EliteLocationInfo BossKolontay { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { GroundZero = 30, TarkovStreets = 30 },
        BossZone = new ValidLocationString { GroundZero = "ZoneSandbox", TarkovStreets = "ZoneClimova,ZoneMvd" }
    };
    [JsonProperty("bossReshala")] public EliteLocationInfo BossReshala { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Customs = 30 },
        BossZone = new ValidLocationString { Customs = "ZoneDormitory,ZoneGasStation,ZoneScavBase" }
    };
    [JsonProperty("bossKaban")] public EliteLocationInfo BossKaban { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { TarkovStreets = 30 },
        BossZone = new ValidLocationString { TarkovStreets = "ZoneCarShowroom" }
    };
    [JsonProperty("bossShturman")] public EliteLocationInfo BossShturman { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Woods = 30 },
        BossZone = new ValidLocationString { Woods = "ZoneWoodCutter" }
    };
    [JsonProperty("bossPartisan")] public EliteLocationInfo BossPartisan { get; set; } = new()
    {
        SpawnChance = new ValidLocationInt { Customs = 30, Lighthouse = 30, Shoreline = 30, Woods = 30 }
    };
    [JsonProperty("pmcBot")] public EliteLocationInfo Raiders { get; set; } = new() { SpawnChance = new ValidLocationInt { Laboratory = 40 }, BossZone = new ValidLocationString { Laboratory = "BotZoneBasement,BotZoneFloor1,BotZoneFloor2" } }; // Raiders
    [JsonProperty("exUsec")] public EliteLocationInfo Rogues { get; set; } = new(); // Rogues
    [JsonProperty("arenaFighterEvent")] public EliteLocationInfo Bloodhounds { get; set; } = new() { SpawnChance = new ValidLocationInt { Customs = 5, Woods = 5 }, BossZone = new ValidLocationString { Customs = "ZoneFactoryCenter,ZoneScavBase", Woods = "ZoneMiniHouse,ZoneClearVill,ZoneRoad,ZoneBrokenVill,ZoneScavBase2" } }; // Bloodhounds
    [JsonProperty("sectantPriest")] public EliteLocationInfo Cultists { get; set; } = new() { SpawnChance = new ValidLocationInt { Customs = 15, Factory4Night = 20, Shoreline = 15, Woods = 15, GroundZero = 44 }, BossZone = new ValidLocationString { Customs = "ZoneScavBase", Factory4Night = "BotZone", Shoreline = "ZoneSanatorium1,ZoneSanatorium2,ZoneForestSpawn", Woods = "ZoneMiniHouse,ZoneBrokenVill", GroundZero = "ZoneSandbox" } }; // Cultists
    [JsonProperty("gifter")] public EliteLocationInfo BossGifter { get; set; } = new(); // Santa

    // Regular Bots (For Hotzone configuration)
    [JsonProperty("sptBear")] public EliteLocationInfo Bear { get; set; } = new();
    [JsonProperty("sptUsec")] public EliteLocationInfo Usec { get; set; } = new();
    [JsonProperty("assault")] public EliteLocationInfo Scav { get; set; } = new();
}

public record BossInvasionConfig
{
    [JsonProperty("enable")] public bool Enable { get; set; } = false;
    [JsonProperty("invasionChance")] public int InvasionChance { get; set; } = 5;
    [JsonProperty("selectedBosses")] public List<string> SelectedBosses { get; set; } = new() { "Random" };
    [JsonProperty("selectedMaps")] public List<string> SelectedMaps { get; set; } = new() { "All" };
}

public record EliteLocationInfo
{
    [JsonProperty("enable")] public bool Enable { get; set; } = true;
    [JsonProperty("disableFollowers")] public bool DisableFollowers { get; set; } = false;
    [JsonProperty("groupChance")] public int GroupChance { get; set; } = 30;
    [JsonProperty("maxGroupSize")] public int MaxGroupSize { get; set; } = 3;
    [JsonProperty("spawnChance")] public ValidLocationInt SpawnChance { get; set; } = new();
    [JsonProperty("bossZone")] public ValidLocationString BossZone { get; set; } = new();
}

public record ValidLocationInt
{
    [JsonProperty("customs")] public int Customs { get; set; } = 0;
    [JsonProperty("factory4_day")] public int Factory4Day { get; set; } = 0;
    [JsonProperty("factory4_night")] public int Factory4Night { get; set; } = 0;
    [JsonProperty("interchange")] public int Interchange { get; set; } = 0;
    [JsonProperty("laboratory")] public int Laboratory { get; set; } = 0;
    [JsonProperty("lighthouse")] public int Lighthouse { get; set; } = 0;
    [JsonProperty("rezervbase")] public int Reserve { get; set; } = 0;
    [JsonProperty("sandbox")] public int GroundZero { get; set; } = 0;
    [JsonProperty("sandbox_high")] public int GroundZeroHigh { get; set; } = 0;
    [JsonProperty("shoreline")] public int Shoreline { get; set; } = 0;
    [JsonProperty("tarkovstreets")] public int TarkovStreets { get; set; } = 0;
    [JsonProperty("woods")] public int Woods { get; set; } = 0;
}

public record ValidLocationString
{
    [JsonProperty("customs")] public string Customs { get; set; } = "";
    [JsonProperty("factory4_day")] public string Factory4Day { get; set; } = "";
    [JsonProperty("factory4_night")] public string Factory4Night { get; set; } = "";
    [JsonProperty("interchange")] public string Interchange { get; set; } = "";
    [JsonProperty("laboratory")] public string Laboratory { get; set; } = "";
    [JsonProperty("lighthouse")] public string Lighthouse { get; set; } = "";
    [JsonProperty("rezervbase")] public string Reserve { get; set; } = "";
    [JsonProperty("sandbox")] public string GroundZero { get; set; } = "";
    [JsonProperty("sandbox_high")] public string GroundZeroHigh { get; set; } = "";
    [JsonProperty("shoreline")] public string Shoreline { get; set; } = "";
    [JsonProperty("tarkovstreets")] public string TarkovStreets { get; set; } = "";
    [JsonProperty("woods")] public string Woods { get; set; } = "";
}

public class MapSettings
{
    [JsonProperty("secondsBetweenWaves")] public int SecondsBetweenWaves { get; set; } = 360;
    [JsonProperty("sniperChance")] public int SniperChance { get; set; } = 30;
    [JsonProperty("safeZoneDistance")] public double SafeZoneDistance { get; set; } = 30.0;
    
    [JsonProperty("enableDespawn")] public bool EnableDespawn { get; set; } = false;
    [JsonProperty("despawnPMCs")] public bool DespawnPMCs { get; set; } = false;
    [JsonProperty("despawnDistance")] public float DespawnDistance { get; set; } = 300f;
    [JsonProperty("despawnInterval")] public float DespawnInterval { get; set; } = 20f;
    [JsonProperty("spawnBubbleDistance")] public float SpawnBubbleDistance { get; set; } = 300f;
    [JsonProperty("enableSpawnBubble")] public bool EnableSpawnBubble { get; set; } = true;
    [JsonProperty("teleportMinDistance")] public float TeleportMinDistance { get; set; } = 100f;
}

public class Vector3Model
{
    [JsonProperty("x")] public float X { get; set; }
    [JsonProperty("y")] public float Y { get; set; }
    [JsonProperty("z")] public float Z { get; set; }
}




