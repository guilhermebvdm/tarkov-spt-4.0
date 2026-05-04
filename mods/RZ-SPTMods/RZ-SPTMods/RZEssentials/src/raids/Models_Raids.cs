// RemzDNB - 2026

using RZEssentials._Shared;

namespace RZEssentials.Raids;

public class RaidsConfig : IConfig
{
    public static string FileName => "raids/raidsConfig.json";

    public bool EnableRaidTimes { get; set; } = false;
    public Dictionary<string, int> RaidTimes { get; set; } = new();

    public bool NoRunThrough { get; set; } = false;
    public bool RemoveRaidRestrictions { get; set; }
    public bool FreeSpecialExtracts { get; set; } = false;
    public double MagazineLoadSpeedMultiplier { get; set; } = 1.0;
    public double MagazineUnloadSpeedMultiplier { get; set; } = 1.0;
}

public class WeatherConfig : IConfig
{
    public static string FileName => "raids/weatherConfig.json";

    public bool Enabled { get; set; } = false;
    public SeasonConfig Season { get; set; } = new();
    public PresetsConfig Presets { get; set; } = new();
}

public class SeasonConfig
{
    public bool Enabled { get; set; } = false;
    public Dictionary<string, int> Weights { get; set; } = new()
    {
        ["Winter"]      = 5,
        ["EarlySpring"] = 10,
        ["Spring"]      = 15,
        ["Storm"]       = 0,
        ["Summer"]      = 30,
        ["Autumn"]      = 20,
        ["LateAutumn"]  = 10,
    };
}

public class PresetsConfig
{
    public bool Enabled { get; set; } = false;

    // true  = one preset drawn per raid, weather stays consistent throughout
    // false = one preset drawn per weather period, vanilla-style variation within the raid
    public bool FixedForRaid { get; set; } = false;

    public List<WeatherPresetEntry> Pool { get; set; } = [];
}

public class WeatherPresetEntry
{
    public string Name { get; set; } = "";
    public int Weight { get; set; } = 1;

    public double? Rain { get; set; }
    public double? RainIntensity { get; set; }
    public double? Fog { get; set; }
    public double? Cloud { get; set; }
    public double? WindSpeed { get; set; }
    public double? WindGustiness { get; set; }
}
