// RemzDNB - 2026

using RZEssentials._Shared;

namespace RZEssentials.ItemTiers;

public class ItemTiersConfig : IConfig
{
    public static string FileName => "itemTiers/itemTiersConfig.json";
    public bool EnableItemTiers { get; set; } = false;

    public string Theme { get; set; } = "dark";
    public Dictionary<string, Dictionary<string, string>> ThemePresets { get; set; } = new();
    public List<PriceThreshold> PriceThresholds { get; set; } = new();

    public Dictionary<string, string> GetTiers()
    {
        if (ThemePresets.TryGetValue(Theme, out var preset))
            return preset;

        return ThemePresets.Values.FirstOrDefault() ?? new Dictionary<string, string>();
    }
}

public class CategoryRulesConfig : IConfig
{
    public static string FileName => "itemTiers/categoryRules.json";

    public Dictionary<string, string> Assignments { get; set; } = new();
}

public class PriceRulesConfig : IConfig
{
    public static string FileName => "itemTiers/priceRules.json";

    public bool Enabled { get; set; } = true;
    public List<string> Categories { get; set; } = new();
}

public class OverrideFile
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, string> Overrides { get; set; } = new();
}

public class PriceThreshold
{
    public int    MinPrice { get; set; }
    public string Tier     { get; set; } = "";
}

public class StatThreshold
{
    public int    Min   { get; set; }
    public string Color { get; set; } = "";
}
