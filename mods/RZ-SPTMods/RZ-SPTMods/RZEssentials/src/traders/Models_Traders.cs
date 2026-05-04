// RemzDNB - 2026

using SPTarkov.Server.Core.Models.Common;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace RZEssentials.Traders;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// defaultTradesConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public record DefaultTradesConfig : IConfig
{
    public static string FileName => "traders/defaultTradesConfig.json";

    public bool ClearAllDefaultTrades { get; set; } = false;
    public Dictionary<string, double> PriceMultipliers { get; set; } = new();
    public Dictionary<string, NoBarterTraderEntry> NoBarterTraders { get; set; } = new();
    public List<string> Blacklist { get; set; } = [];
}

public record NoBarterTraderEntry
{
    public bool Enabled { get; set; } = false;
    public TradeCurrency Currency { get; set; } = TradeCurrency.Rub;
    public List<string> ExcludedBarterTpls { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// autoRoutingConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class RoutedTradesConfig : IConfig
{
    public static string FileName => "traders/routedTradesConfig.json";

    public bool ForceRouteAll { get; set; } = false;
    public bool RouteModdedItemsOnly { get; set; } = false;
    public bool RouteVanillaItemsOnly { get; set; } = false;
    public Dictionary<string, TradeCurrency> TraderCurrencies { get; set; } = new();
    public string? FallbackTrader { get; set; } = null;
    public List<string> Blacklist { get; set; } = [];

    public Dictionary<string, List<CategoryRoute>> CategoryRoutes { get; set; } = new();
}

public class CategoryRoute
{
    public bool Enabled { get; set; } = true;
    public string CategoryId { get; set; } = "";
    public double PriceMultiplier { get; set; } = 1.0;
    public int LoyaltyLevel { get; set; } = 1;
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// manualTradesConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class ManualTradesConfig : IConfig
{
    public static string FileName => "traders/manualTradesConfig.json";

    public List<ManualTraderOffers> ManualOffers { get; set; } = new();
}

public class TradeOffer
{
    public string Tpl { get; set; } = "";
    public int StackCount { get; set; } = -1;
    public int LoyaltyLevel { get; set; } = 1;
    public int Durability { get; set; } = 100;
    public int PriceRoubles { get; set; } = 0;
    public List<ItemEntry> Children { get; set; } = new();
    public List<ItemEntry> BarterItems { get; set; } = new();
}

public class ManualTraderOffers
{
    public string Id { get; set; } = ""; // trader id
    public List<TradeOffer> Offers { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
// fenceConfig.json
// ─────────────────────────────────────────────────────────────────────────────

public class FenceConfig : IConfig
{
    public static string FileName => "traders/fenceConfig.json";
    public bool EnableFenceConfig { get; set; } = false;

    // Default item pool settings.
    public bool EnableDefaultItemPool { get; set; } = false;

    public int? AssortSize { get; set; } = null;
    public MinMax<int>? WeaponPresetMinMax { get; set; } = null;
    public MinMax<int>? EquipmentPresetMinMax { get; set; } = null;
    public double? ItemPriceMultiplier { get; set; } = 1.0;
    public double? PresetPriceMultiplier { get; set; } = 1.0;

    public int? DiscountAssortSize { get; set; } = null;
    public MinMax<int>? DiscountWeaponPresetMinMax { get; set; } = null;
    public MinMax<int>? DiscountEquipmentPresetMinMax { get; set; } = null;
    public double? DiscountItemPriceMultiplier { get; set; } = 1.0;
    public double? DiscountPresetPriceMultiplier { get; set; } = 1.0;

    // Default item pool additions settings.
    public bool EnableDefaultItemPoolAdditions { get; set; } = false;
    public List<FencePoolItem> DefaultItemPoolAdditions { get; set; } = new();

    // Custom item pool.
    public bool EnableCustomItemPool { get; set; } = false;
    public int CustomItemCount { get; set; } = 10;
    public double CustomItemPriceMultiplier { get; set; } = 1.0;
    public List<FenceOffer> CustomItemPool { get; set; } = new();
}

public class FencePoolItem
{
    public string Tpl { get; set; } = "";
    public int RoublePrice { get; set; } = 0;
    public int LoyaltyLevel { get; set; } = 1;
}

public class FenceOffer
{
    public string Tpl { get; set; } = "";
    public int Weight { get; set; } = 1;
    public int StackSize { get; set; } = 1;
    public string CurrencyTpl { get; set; } = ItemTpl.MONEY_ROUBLES;
    public int Price { get; set; } = 1;
    public int LoyaltyLevel { get; set; } = 1;
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// buybackConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class BuybackConfig : IConfig
{
    public static string FileName => "traders/buybackConfig.json";

    public Dictionary<string, BuybackRule> Rules { get; set; } = new();
}

public enum BuybackMode
{
    Default,            // Leave the trader's vanilla buyback config untouched.
    Disabled,           // Trader does not buy anything from the player.
    Categories,         // Trader buys items belonging to the specified handbook category IDs.
    AllWithBlacklist,   // Trader buys all handbook items except those in the blacklist.
}

public class BuybackRule
{
    public BuybackMode Mode { get; set; } = BuybackMode.Default;

    // Handbook category IDs. Used when Mode is Categories.
    public List<string> Categories { get; set; } = new();

    // Item TPLs the trader will not buy. Used when Mode is AllWithBlacklist.
    public List<string> Blacklist { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// supplyConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class SupplyConfig : IConfig
{
    public static string FileName => "traders/supplyConfig.json";

    public bool EnableRestockTimes { get; set; } = true;
    public Dictionary<string, int> RestockTimes { get; set; } = new();
    public StockMultipliersConfig StockMultipliers { get; set; } = new();
}

public class StockMultipliersConfig
{
    public bool EnableByTrader { get; set; } = false;
    public Dictionary<string, double> ByTrader { get; set; } = new();
    public bool EnableByCategory { get; set; } = false;
    public Dictionary<string, double> ByCategory { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// miscSettingsConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class TraderMiscSettingsConfig : IConfig
{
    public static string FileName => "traders/traderMiscSettingsConfig.json";

    public bool UnlockJaeger { get; set; } = false;
    public bool UnlockRef { get; set; } = false;
    public bool EnableLoyaltyLevels { get; set; } = false;
    public bool EnableRepairOverrides { get; set; } = false;

    public Dictionary<string, List<TraderLoyaltyLevel>> LoyaltyLevelsOverrides { get; set; } = new();
    public Dictionary<string, TraderRepair> RepairOverrides { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// customTrader.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public record CustomTraderConfig : IConfig
{
    public static string FileName => "traders/customTraderConfig.json";
    public static string TraderId => "fa363fc015e07b5f7828c9f7";

    public bool EnableCustomTrader { get; set; } = false;
    public string AvatarFile { get; set; } = "sable_01.png";

    public string FullName    { get; set; } = "Sable";
    public string FirstName   { get; set; } = "Sable";
    public string Nickname    { get; set; } = "Sable";
    public string Location    { get; set; } = "";
    public string Description { get; set; } = "";
}
