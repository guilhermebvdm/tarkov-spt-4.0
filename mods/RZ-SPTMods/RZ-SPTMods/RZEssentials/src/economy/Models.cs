// RemzDNB - 2026

using RZEssentials._Shared;

namespace RZEssentials.Economy;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// fleaMarketConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public record FleaMarketConfig : IConfig
{
    public static string FileName => "economy/fleaMarketConfig.json";

    public bool DisableFleaMarket { get; set; } = false;
    public List<string> DynamicForceDisable { get; set; } = [];
    public List<string> DynamicForceEnable { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// insuranceConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public record InsuranceConfig : IConfig
{
    public static string FileName => "economy/insuranceConfig.json";

    public bool DisableAll { get; set; } = false;
    public List<CategoryBlacklistEntry> CategoryBlacklist { get; set; } = [];
    public List<string> TplBlacklist { get; set; } = [];
}

public record CategoryBlacklistEntry
{
    public bool Enabled { get; set; } = false;
    public string CategoryId { get; set; } = "";
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// handbookPricesConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class HandbookPricesConfig : IConfig
{
    public static string FileName => "economy/handbookPricesConfig.json";
    public bool Enabled { get; set; } = false;

    public Dictionary<string, int> Prices { get; set; } = new();
}
