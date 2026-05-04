// RemzDNB - 2026

namespace RZEssentials._Shared;

public record ClientConfig : IConfig
{
    public static string FileName => "ui/clientConfig.json";

    public bool SkipSideSelectionScreen { get; set; } = true;
    public bool SkipInsuranceScreen { get; set; } = true;
    public bool SkipRaidSettingsScreen { get; set; } = true;
    public bool SkipExperienceScreen { get; set; } = true;

    public bool EnableCategorySort { get; set; } = true;
    public bool EnableItemSort { get; set; } = true;
    public bool EnableAlphabeticalSort { get; set; } = true;
    public List<string> CategoryOrder { get; set; } = [];
    public List<string> ItemOrder { get; set; } = [];

    public bool EnableVersionLabelOverride { get; set; } = false;
    public string VersionLabelText { get; set; } = "";
}

public class LogConfig : IConfig
{
    public static string FileName => "miscSettings/logConfig.json";

    public Dictionary<string, bool> Channels { get; set; } = new();
}

public enum TradeCurrency { Rub, Eur, Usd }

public class ItemEntry
{
    public string Tpl { get; set; } = "";
    public int Count { get; set; } = 1;
    public string? SlotId { get; set; } = null;
}

public class StatOperation
{
    public string Op { get; set; } = "set";
    public double Value { get; set; }
}
