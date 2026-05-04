// RemzDNB - 2026

using RZEssentials._Shared;

namespace RZEssentials.ItemContext;

public class ItemContextConfig : IConfig
{
    public static string FileName => "itemContext/itemContextConfig.json";
    public bool EnableItemContext { get; set; } = false;

    public AmmoNameEnrichmentConfig AmmoNameEnrichment { get; set; } = new();
    public HandbookPriceDisplayConfig BasePriceDisplay { get; set; } = new();
    public DescriptionCleanupConfig DescriptionCleanup { get; set; } = new();

    public bool EnableHideoutInfo { get; set; } = true;
    public List<string> HideoutExcludedAreas { get; set; } = new();
    public bool EnableBarterInfo { get; set; } = true;
    public bool ShowBarterLoyaltyLevel { get; set; } = true;
    public bool EnableCraftingInfo { get; set; } = true;
    public bool EnableCraftingToolInfo { get; set; } = true;
    public bool EnableQuestInfo { get; set; } = true;
    public bool EnableContainerInfo { get; set; } = true;
    public bool EnableHeadsetInfo { get; set; } = true;
    public bool EnableArmorPlateInfo { get; set; } = true;
    public bool EnableKeyInfo { get; set; } = false;
    public Dictionary<string, string> KeyStats { get; set; } = new();
}

public class AmmoNameEnrichmentConfig
{
    public bool Enabled { get; set; } = true;
    public bool ShowPrefixes { get; set; } = true;
    public List<StatThreshold> DamageThresholds { get; set; } = new();
    public List<StatThreshold> PenetrationThresholds { get; set; } = new();
}

public class HandbookPriceDisplayConfig
{
    public bool Enabled { get; set; } = false;
    public Dictionary<string, bool> Categories { get; set; } = new();
    public List<StatThreshold> PriceThresholds { get; set; } = new();
}

public class DescriptionCleanupConfig
{
    public bool Enabled { get; set; } = true;
    public List<string> Categories { get; set; } = new();
}

public class StatThreshold
{
    public int Min { get; set; }
    public string Color { get; set; } = "";
}
