// RemzDNB - 2026

using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace RZEssentials.Character;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// healthConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public record HealthConfig : IConfig
{
    public static string FileName => "character/healthConfig.json";
    public bool Enabled { get; set; } = false;

    public bool ApplyToExistingSaves { get; set; } = false;
    public double? Head     { get; set; } = null;
    public double? Chest    { get; set; } = null;
    public double? Stomach  { get; set; } = null;
    public double? LeftArm  { get; set; } = null;
    public double? RightArm { get; set; } = null;
    public double? LeftLeg  { get; set; } = null;
    public double? RightLeg { get; set; } = null;

    public double BaseEnergyRegeneration { get; set; } = 0;
    public double BaseHydrationRegeneration { get; set; } = 0;
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// weightLimitsConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class WeightLimitsConfig : IConfig
{
    public static string FileName => "character/weightLimitsConfig.json";
    public bool Enabled { get; set; } = false;

    public XYZ? BaseOverweightLimits { get; set; }
    public XYZ? WalkOverweightLimits { get; set; }
    public XYZ? SprintOverweightLimits { get; set; }
    public XYZ? WalkSpeedOverweightLimits { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// skillsConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public record SkillsConfig : IConfig
{
    public static string FileName => "character/skillsConfig.json";
    public bool Enabled { get; set; } = false;

    public Dictionary<string, Dictionary<string, double>> Settings { get; set; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// equipmentConflictsConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class EquipmentConflictsConfig : IConfig
{
    public static string FileName => "character/equipmentConflictsConfig.json";
    public bool Enabled { get; set; } = false;

    public Dictionary<string, EquipmentBlockSettings> CategorySettings { get; set; } = new();
    public List<string> ExcludedTpls { get; set; } = [];
    public List<ConflictingItemsRule> ClearConflictingItems { get; set; } = [];
}

public class EquipmentBlockSettings
{
    public bool UnblockHeadwear  { get; set; } = false;
    public bool UnblockEarpiece  { get; set; } = false;
    public bool UnblockFaceCover { get; set; } = false;
    public bool UnblockEyewear   { get; set; } = false;
}

public class ConflictingItemsRule
{
    public List<string> FromTpls { get; set; } = [];
    public List<string> ToCategories { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// specialSlotsConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class SpecialSlotsConfig : IConfig
{
    public static string FileName => "character/specialSlotsConfig.json";
    public bool Enabled { get; set; } = false;

    public List<SpecialSlotEntry> Items { get; set; } = [];
}

public class SpecialSlotEntry
{
    public string Tpl { get; set; } = "";
    public bool Enabled { get; set; } = true;
}

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// holsterSlotConfig.json
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

public class HolsterSlotConfig : IConfig
{
    public static string FileName => "character/holsterSlotConfig.json";
    public bool Enabled { get; set; } = false;

    public HolsterCategoryToggles Categories { get; set; } = new();
    public List<string> AllowedTpls { get; set; } = [];
}

public class HolsterCategoryToggles
{
    public bool AssaultCarbine  { get; set; } = false;
    public bool AssaultRifle    { get; set; } = false;
    public bool GrenadeLauncher { get; set; } = false;
    public bool MachineGun      { get; set; } = false;
    public bool MarksmanRifle   { get; set; } = false;
    public bool Pistol          { get; set; } = true;
    public bool Revolver        { get; set; } = true;
    public bool RocketLauncher  { get; set; } = false;
    public bool Shotgun         { get; set; } = false;
    public bool Smg             { get; set; } = false;
    public bool SniperRifle     { get; set; } = false;
    public bool SpecialWeapon   { get; set; } = false;
    public bool Knife           { get; set; } = false;
}
