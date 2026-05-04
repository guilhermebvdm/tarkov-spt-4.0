// RemzDNB - 2026

using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Common;

namespace RZEssentials.MiscSettings;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// miscSettingsConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class MiscSettingsConfig : IConfig
{
    public static string FileName => "miscSettings/miscSettingsConfig.json";

    public bool FreePostRaidHeal { get; set; } = false;
    public bool UnlockAllOutfits { get; set; } = false;
    public bool DisablePmcMailResponses { get; set; } = false;
    public bool DisableStartingGifts { get; set; } = false;
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// repairConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class RepairConfig : IConfig
{
    public static string FileName => "miscSettings/repairConfig.json";

    public bool? NoRepairDegradation { get; set; } = null;
    public RepairKitConfig? RepairKit { get; set; } = null;
}

public class RepairKitConfig
{
    public bool Enabled { get; set; } = false;

    public RepairKitBonusSettings? Armor    { get; set; }
    public RepairKitBonusSettings? Weapon   { get; set; }
    public RepairKitBonusSettings? Vest     { get; set; }
    public RepairKitBonusSettings? Headwear { get; set; }
}

public class RepairKitBonusSettings
{
    public Dictionary<string, double>?            RarityWeight    { get; set; }
    public Dictionary<string, double>?            BonusTypeWeight { get; set; }
    public Dictionary<string, RepairBonusValues>? Common          { get; set; }
    public Dictionary<string, RepairBonusValues>? Rare            { get; set; }
}

public class RepairBonusValues
{
    public MinMax<double>? ValuesMinMax                  { get; set; }
    public MinMax<int>?    ActiveDurabilityPercentMinMax { get; set; }
}

