// RemzDNB - 2026

using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using RZEssentials._Shared;

namespace RZEssentials.Hideout;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// hideoutAreasConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class HideoutAreasConfig : IConfig
{
    public static string FileName => "hideout/hideoutAreasConfig.json";
    public bool Enabled { get; set; } = false;

    public Dictionary<string, HideoutAreaConfig> Areas { get; init; } = new();
    public BitcoinFarmConfig? BitcoinFarm { get; set; }

    public static readonly Dictionary<string, (string CategoryId, string CustomisationType)> HideoutCategories = new()
    {
        ["Wall"]              = ("67373f1e5a5ee73f2a081baf", CustomisationType.WALL),
        ["Floor"]             = ("67373f170eca6e03ab0d5391", CustomisationType.FLOOR),
        ["Ceiling"]           = ("673b3f595bf6b605c90fcdc2", CustomisationType.CEILING),
        ["Light"]             = ("67373f286cadad262309e862", CustomisationType.LIGHT),
        ["MannequinPose"]     = ("675ff48ce8d2356707079617", CustomisationType.MANNEQUIN_POSE),
        ["ShootingRangeMark"] = ("67373f330eca6e03ab0d5394", CustomisationType.SHOOTING_RANGE_MARK),
    };
}

public class HideoutAreaConfig
{
    public bool? RemoveFromDb { get; set; }
    public bool? Enabled { get; set; }
    public bool? DisplayLevel { get; set; }

    public bool UseCustomConstructionTime { get; set; } = false;
    public Dictionary<string, double> ConstructionTime { get; set; } = new();
    public bool UseCustomItemRequirements { get; set; } = false;
    public Dictionary<string, List<ItemRequirement>> ItemRequirements { get; set; } = new();
    public bool UseCustomAreaRequirements { get; set; } = false;
    public Dictionary<string, List<AreaRequirement>> AreaRequirements { get; set; } = new();
    public bool UseCustomSkillRequirements { get; set; } = false;
    public Dictionary<string, List<SkillRequirement>> SkillRequirements { get; set; } = new();
    public bool UseCustomTraderRequirements { get; set; } = false;
    public Dictionary<string, List<TraderRequirement>> TraderRequirements { get; set; } = new();
}

public class ItemRequirement
{
    public string ItemTpl { get; set; } = "";
    public int ItemCount { get; set; } = 1;
    public bool ItemFunctional { get; set; } = false;
}

public class AreaRequirement
{
    public string AreaName { get; set; } = "";
    public int AreaLevel { get; set; } = 1;
}

public class SkillRequirement
{
    public string SkillName { get; set; } = "";
    public int SkillLevel { get; set; } = 1;
}

public class TraderRequirement
{
    public string TraderName { get; set; } = "";
    public int TraderLoyalty { get; set; } = 1;
}

public class BitcoinFarmConfig
{
    public double? ProductionSpeedMultiplier { get; set; }
    public double? GpuBoostRate { get; set; }
    public int? MaxCapacity { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// hideoutBonusesConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class BonusesConfig : IConfig
{
    public static string FileName => "hideout/hideoutBonusesConfig.json";
    public bool EnableBonusesConfig { get; set; } = false;

    public Dictionary<string, Dictionary<string, List<BonusEntry>>> Areas { get; set; } = new();
}

public class BonusEntry
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public double? Value { get; set; }
    //public bool? IsPassive { get; set; }
    //public bool? IsProduction { get; set; }
    public bool? IsVisible { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// hideoutMiscConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class HideoutMiscConfig : IConfig
{
    public static string FileName => "hideout/hideoutMiscConfig.json";

    public bool? RequireFoundInRaid { get; set; } = null;
    public Dictionary<string, bool>? UnlockHideoutCustomizations { get; set; } = null;
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// craftingConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class CraftingConfig : IConfig
{
    public static string FileName => "hideout/craftingConfig.json";
    public bool EnableCraftingConfig { get; set; } = false;

    public List<HideoutAreas> ClearAreas { get; set; } = new();
    public Dictionary<HideoutAreas, List<CraftRecipe>> Recipes { get; set; } = new();
}

public class CraftRequirement
{
    public string Type { get; set; } = "Item"; // "Item" or "Area"
    public string? TemplateId { get; set; } // if Type == Item
    public HideoutAreas? AreaType { get; set; } // if Type == Area
    public int? RequiredLevel { get; set; } // if Type == Area
    public int Count { get; set; } = 1;
    public bool IsFunctional { get; set; } = false;
    public bool IsEncoded { get; set; } = false;
}

public class CraftRecipe
{
    public string EndProduct { get; set; } = "";
    public int Count { get; set; } = 1;
    public int ProductionTime { get; set; } = 60;
    public bool NeedFuelForAllProductionTime { get; set; } = false;
    public bool Locked { get; set; } = false;
    public bool Continuous { get; set; } = false;
    public List<CraftRequirement> Requirements { get; set; } = new();
}
