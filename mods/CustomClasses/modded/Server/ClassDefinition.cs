using System.Text.Json.Serialization;

namespace CustomClasses;

/// <summary>
///     JSON schema for one class file (<c>config/classes/*.json[c]</c>). Item 002 covers
///     name/enabled/baseEdition/description/skills; the schema grows in items 003-008
///     (starting items, outfits, skill multipliers, …).
/// </summary>
public sealed record ClassDefinition
{
    /// <summary>Required. Becomes the launcher edition key + display label (PT identifier).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    ///     Optional. Item 008 (i18n): localized class name shown in-game (menu/tooltip). Object { en, pt }.
    ///     Falls back to <see cref="Name"/> when absent. The launcher edition key stays <see cref="Name"/> (PT).
    /// </summary>
    [JsonPropertyName("displayName")]
    public LocalizedText? DisplayName { get; init; }

    /// <summary>Optional (default true). When false the class is not registered.</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; init; } = true;

    /// <summary>Optional. Vanilla edition cloned as the base; default "SPT Zero to hero" (empty stash).</summary>
    [JsonPropertyName("baseEdition")]
    public string? BaseEdition { get; init; }

    /// <summary>Optional. Launcher description. Item 008: aceita string (legado=en) ou objeto { en, pt }.</summary>
    [JsonPropertyName("description")]
    public LocalizedText? Description { get; init; }

    /// <summary>Optional. Item 011: nome do PNG do ícone da classe (ex.: "cacador.png"), em BepInEx/plugins/CustomClasses/icons/.</summary>
    [JsonPropertyName("iconFile")]
    public string? IconFile { get; init; }

    /// <summary>Optional. Item 011: cor do nome da classe na UI, hex "#RRGGBB".</summary>
    [JsonPropertyName("nameColor")]
    public string? NameColor { get; init; }

    /// <summary>Optional. Skill name → starting level (0..51). Unknown names are ignored with a warning.</summary>
    [JsonPropertyName("skills")]
    public Dictionary<string, int>? Skills { get; init; }

    /// <summary>Optional. Skill name → XP-gain multiplier (1 = vanilla, 1.5 = +50%, 0.5 = −50%). Applied client-side. (Item 005)</summary>
    [JsonPropertyName("skillMultipliers")]
    public Dictionary<string, double>? SkillMultipliers { get; init; }

    /// <summary>Optional. Hideout station name (HideoutAreas) → starting level. (Item 003)</summary>
    [JsonPropertyName("hideout")]
    public Dictionary<string, int>? Hideout { get; init; }

    /// <summary>Optional. Starting items: equipped on the character + loose in the stash. (Item 003)</summary>
    [JsonPropertyName("loadout")]
    public Loadout? Loadout { get; init; }

    /// <summary>Optional. Per-side outfit (USEC/BEAR). (Item 004)</summary>
    [JsonPropertyName("outfit")]
    public Outfit? Outfit { get; init; }
}

/// <summary>Outfit por lado — roupas são específicas de facção. (Item 004)</summary>
public sealed record Outfit
{
    [JsonPropertyName("usec")] public OutfitSide? Usec { get; init; }
    [JsonPropertyName("bear")] public OutfitSide? Bear { get; init; }
}

/// <summary>IDs das peças de roupa (customization "Item"): upper (camisa/jaqueta) e lower (calça).</summary>
public sealed record OutfitSide
{
    [JsonPropertyName("upper")] public string? Upper { get; init; }
    [JsonPropertyName("lower")] public string? Lower { get; init; }
}

/// <summary>Starting items of a class: equipped (by slot) + loose stash items.</summary>
public sealed record Loadout
{
    /// <summary>EquipmentSlots name (Headwear, ArmorVest, TacticalVest, FirstPrimaryWeapon, …) → item.</summary>
    [JsonPropertyName("equipped")]
    public Dictionary<string, ItemSpec>? Equipped { get; init; }

    /// <summary>Loose items placed in the stash.</summary>
    [JsonPropertyName("stash")]
    public List<ItemSpec>? Stash { get; init; }
}

/// <summary>One item entry: a plain tpl, a weapon preset, or a manual tree (mods/contents).</summary>
public sealed record ItemSpec
{
    [JsonPropertyName("tpl")] public string? Tpl { get; init; }

    /// <summary>Weapon preset id OR weapon tpl. Resolved: GetPreset(id) → fallback GetDefaultPreset(tpl). (PA-01-04)</summary>
    [JsonPropertyName("preset")] public string? Preset { get; init; }

    /// <summary>When true (with `preset` = weapon tpl), use the most-kitted preset (premium build) instead of the default.</summary>
    [JsonPropertyName("premium")] public bool Premium { get; init; }

    [JsonPropertyName("count")] public int Count { get; init; } = 1;

    /// <summary>Cartridge tpl. REQUIRED when loadedMag/chambered is true. (PA-01-03)</summary>
    [JsonPropertyName("ammo")] public string? Ammo { get; init; }

    [JsonPropertyName("loadedMag")] public bool LoadedMag { get; init; }

    [JsonPropertyName("chambered")] public bool Chambered { get; init; }

    /// <summary>Items inside a container (rig/backpack).</summary>
    [JsonPropertyName("contents")] public List<ItemSpec>? Contents { get; init; }

    /// <summary>Manual mod tree (alternative to preset).</summary>
    [JsonPropertyName("mods")] public List<ModSpec>? Mods { get; init; }

    /// <summary>Stash grid column (0-based). Opt-in: when X+Y are set the builder places the item at that
    /// cell instead of first-fit (falls back to auto-pack if it no longer fits). Stash-only (item 038).</summary>
    [JsonPropertyName("x")] public int? X { get; init; }

    /// <summary>Stash grid row (0-based). See <see cref="X"/>.</summary>
    [JsonPropertyName("y")] public int? Y { get; init; }

    /// <summary>Explicit rotation for the pinned cell — true → ItemRotation.Vertical (item 038).</summary>
    [JsonPropertyName("rotated")] public bool? Rotated { get; init; }
}

/// <summary>A mod node in a manual item tree.</summary>
public sealed record ModSpec
{
    [JsonPropertyName("slotId")] public string? SlotId { get; init; }
    [JsonPropertyName("tpl")] public string? Tpl { get; init; }
    [JsonPropertyName("mods")] public List<ModSpec>? Mods { get; init; }
}
