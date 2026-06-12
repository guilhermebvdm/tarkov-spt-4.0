using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;                  // ItemHelper
using SPTarkov.Server.Core.Services;                 // DatabaseService, LocaleService
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Enums;             // BaseClasses
using SPTarkov.Server.Core.Models.Eft.Common;        // Preset
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // TemplateItem, CustomizationItem

namespace CustomClasses;

/// <summary>One item hit returned by <see cref="CatalogService.Search"/>.</summary>
public sealed record CatalogItem
{
    [JsonPropertyName("tpl")] public required string Tpl { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("shortName")] public string? ShortName { get; init; }
    [JsonPropertyName("price")] public double Price { get; init; }
    [JsonPropertyName("priceSource")] public required string PriceSource { get; init; }
    [JsonPropertyName("categoryId")] public string? CategoryId { get; init; }
}

/// <summary>One handbook category node (tree via ParentId).</summary>
public sealed record CatalogCategory
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("parentId")] public string? ParentId { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
}

/// <summary>One weapon/armor preset available for a tpl.</summary>
public sealed record CatalogPreset
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("itemCount")] public int ItemCount { get; init; }
    /// <summary>Encyclopedia preset — what InventoryBuilder resolves for a bare tpl.</summary>
    [JsonPropertyName("isDefault")] public bool IsDefault { get; init; }
    /// <summary>The preset InventoryBuilder picks for <c>premium: true</c> (most-kitted, thermal/NV avoided).</summary>
    [JsonPropertyName("isPremium")] public bool IsPremium { get; init; }
}

/// <summary>One ammo cartridge compatible with a weapon's caliber (item 023 — AmmoPicker).</summary>
public sealed record CatalogAmmo
{
    [JsonPropertyName("tpl")] public required string Tpl { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("shortName")] public string? ShortName { get; init; }
    [JsonPropertyName("price")] public double Price { get; init; }
    [JsonPropertyName("damage")] public double? Damage { get; init; }
    [JsonPropertyName("penetration")] public int? Penetration { get; init; }
    [JsonPropertyName("caliber")] public required string Caliber { get; init; }
}

/// <summary>One mod/equipment slot declared by an item template (item 026 — ItemSpecEditor).</summary>
public sealed record CatalogSlotInfo
{
    /// <summary>EFT slot id (<c>_props.Slots[]._name</c>, e.g. "mod_magazine", "FirstPrimaryWeapon").</summary>
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("required")] public bool Required { get; init; }
}

/// <summary>One clothing piece (customization "Item") selectable as outfit upper/lower.</summary>
public sealed record CatalogClothing
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    /// <summary>"upper" (Body) or "lower" (Feet).</summary>
    [JsonPropertyName("slot")] public required string Slot { get; init; }
}

/// <summary>
///     Item 022: read-only item/price/preset/customization catalog over the LIVE server DB
///     (<see cref="DatabaseService"/>) for the class editor (item 023). Includes items added by
///     other installed mods automatically, since it reads the post-load tables on demand.
///
///     Price decision (registered in 022-catalog-e-custo-02-spec-tech.md): the effective flea table
///     (<c>DatabaseService.GetPrices()</c> via <c>ItemHelper.GetDynamicItemPrice</c>) is the primary
///     source — it is the server analogue of the RZ avg24h feed and already reflects this repo's
///     flea price overrides (additive override + floor + ceiling, see memory project_flea_price_formula).
///     Items absent from the flea table fall back to the handbook (<c>ItemHelper.GetStaticItemPrice</c>).
///     Currency is face value (rouble handbook = 1 ₽). No price at all → 0 + "missing" source (never silent).
/// </summary>
[Injectable(InjectionType.Singleton)]
public class CatalogService(
    DatabaseService databaseService,
    ItemHelper itemHelper,
    LocaleService localeService)
{
    private const string SourceFlea = "flea";
    private const string SourceHandbook = "handbook";
    private const string SourceCurrency = "currency";
    private const string SourceMissing = "missing";

    // ── Prices ───────────────────────────────────────────────────────────────

    /// <summary>Unit price in roubles + source. Flea-first with handbook fallback; currency = face value.</summary>
    public (double Price, string Source) GetPrice(MongoId tpl)
    {
        if (itemHelper.IsOfBaseclass(tpl, BaseClasses.MONEY))
        {
            // face value: rouble handbook price is 1; USD/EUR handbook prices are the rouble conversion
            var face = itemHelper.GetStaticItemPrice(tpl);   // ref: ItemHelper.cs:454
            return (face >= 1 ? face : 1, SourceCurrency);
        }

        var flea = itemHelper.GetDynamicItemPrice(tpl);      // ref: ItemHelper.cs:470 → DatabaseService.GetPrices()
        if (flea is > 0)
        {
            return (flea.Value, SourceFlea);
        }

        var handbook = itemHelper.GetStaticItemPrice(tpl);
        if (handbook >= 1)
        {
            return (handbook, SourceHandbook);
        }

        return (0, SourceMissing);
    }

    // ── Names / locales ──────────────────────────────────────────────────────

    /// <summary>
    ///     Localized item name. <paramref name="lang"/> = "en" | "pt" (EFT locale code for Portuguese is
    ///     "po" — mapped here). Falls back: requested locale → en → template internal name → tpl.
    /// </summary>
    public string GetItemName(MongoId tpl, string lang = "en")
    {
        var locale = GetLocale(lang);
        if (locale.TryGetValue($"{tpl} Name", out var name) && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var en = GetLocale("en");
        if (en.TryGetValue($"{tpl} Name", out var enName) && !string.IsNullOrWhiteSpace(enName))
        {
            return enName;
        }

        return GetTemplate(tpl)?.Name ?? tpl.ToString();
    }

    /// <summary>EFT locale dict for a language ("pt" → EFT code "po"); LocaleService falls back to en.</summary>
    private Dictionary<string, string> GetLocale(string lang)
    {
        var code = string.Equals(lang, "pt", StringComparison.OrdinalIgnoreCase) ? "po" : lang;
        return localeService.GetLocaleDb(code);   // ref: LocaleService.cs:20 (en fallback built in)
    }

    // ── Search ───────────────────────────────────────────────────────────────

    /// <summary>
    ///     Searches item templates (<c>_type == "Item"</c>) by name (en + pt locale), short name,
    ///     internal template name or exact tpl. Optional <paramref name="parentCategoryId"/> filters by
    ///     handbook category (the category itself or any descendant). Optional <paramref name="filter"/>
    ///     (tpl predicate, e.g. slot compatibility) is applied BEFORE the cap so filtered-out hits never
    ///     consume the limit (CR-EP-07). Results capped at <paramref name="limit"/>.
    /// </summary>
    public List<CatalogItem> Search(string query, string? parentCategoryId = null, int limit = 50, Func<string, bool>? filter = null)
    {
        var results = new List<CatalogItem>();
        if (string.IsNullOrWhiteSpace(query) || limit <= 0)
        {
            return results;
        }

        var q = query.Trim();
        var en = GetLocale("en");
        var pt = GetLocale("pt");
        var handbookIndex = BuildHandbookIndex();
        var categoryScope = parentCategoryId is null ? null : CollectCategoryWithDescendants(parentCategoryId);

        foreach (var (tpl, template) in databaseService.GetItems())   // ref: DatabaseService.cs:129
        {
            if (!string.Equals(template.Type, "Item", StringComparison.Ordinal))
            {
                continue;   // skip "Node" type rows (category nodes inside items.json)
            }

            handbookIndex.TryGetValue(tpl, out var categoryId);
            if (categoryScope is not null && (categoryId is null || !categoryScope.Contains(categoryId)))
            {
                continue;
            }

            var key = tpl.ToString();
            en.TryGetValue($"{key} Name", out var enName);
            en.TryGetValue($"{key} ShortName", out var shortName);
            pt.TryGetValue($"{key} Name", out var ptName);

            var match = string.Equals(key, q, StringComparison.OrdinalIgnoreCase)
                || Contains(enName, q)
                || Contains(ptName, q)
                || Contains(shortName, q)
                || Contains(template.Name, q);
            if (!match)
            {
                continue;
            }

            if (filter is not null && !filter(key))
            {
                continue;   // CR-EP-07: incompatible hit must not consume the result cap
            }

            var (price, source) = GetPrice(tpl);
            results.Add(new CatalogItem
            {
                Tpl = key,
                Name = !string.IsNullOrWhiteSpace(enName) ? enName! : template.Name ?? key,
                ShortName = shortName,
                Price = price,
                PriceSource = source,
                CategoryId = categoryId,
            });

            if (results.Count >= limit)
            {
                break;
            }
        }

        return results;
    }

    private static bool Contains(string? haystack, string needle) =>
        haystack is not null && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);

    /// <summary>tpl → handbook category id (handbook.Items[].ParentId). Built per call — DB is live (mods).</summary>
    private Dictionary<MongoId, string> BuildHandbookIndex()
    {
        var index = new Dictionary<MongoId, string>();
        foreach (var entry in databaseService.GetHandbook().Items)   // ref: DatabaseService.cs:123, HandbookBase.cs
        {
            index[entry.Id] = entry.ParentId.ToString();
        }

        return index;
    }

    /// <summary>Set with a category id + all its descendants (handbook.Categories tree by ParentId).</summary>
    private HashSet<string> CollectCategoryWithDescendants(string rootId)
    {
        var children = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var cat in databaseService.GetHandbook().Categories)
        {
            var parent = cat.ParentId?.ToString();
            if (parent is null)
            {
                continue;
            }

            if (!children.TryGetValue(parent, out var list))
            {
                children[parent] = list = [];
            }

            list.Add(cat.Id.ToString());
        }

        var result = new HashSet<string>(StringComparer.Ordinal) { rootId };
        var queue = new Queue<string>();
        queue.Enqueue(rootId);
        while (queue.Count > 0)
        {
            if (!children.TryGetValue(queue.Dequeue(), out var kids))
            {
                continue;
            }

            foreach (var kid in kids)
            {
                if (result.Add(kid))
                {
                    queue.Enqueue(kid);
                }
            }
        }

        return result;
    }

    // ── Categories ───────────────────────────────────────────────────────────

    /// <summary>Handbook category tree (id → parent + localized name; locale key is the category id itself).</summary>
    public List<CatalogCategory> GetCategories(string lang = "en")
    {
        var locale = GetLocale(lang);
        var en = GetLocale("en");
        var categories = new List<CatalogCategory>();
        foreach (var cat in databaseService.GetHandbook().Categories)
        {
            var id = cat.Id.ToString();
            var name = locale.TryGetValue(id, out var localized) && !string.IsNullOrWhiteSpace(localized)
                ? localized
                : en.TryGetValue(id, out var enName) ? enName : id;
            categories.Add(new CatalogCategory { Id = id, ParentId = cat.ParentId?.ToString(), Name = name });
        }

        return categories;
    }

    // ── Presets ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Presets whose root item is <paramref name="tpl"/>, flagging which one InventoryBuilder would
    ///     resolve as default (encyclopedia) and as premium (most-kitted, thermal/NV avoided).
    /// </summary>
    public List<CatalogPreset> GetPresetsFor(MongoId tpl)
    {
        var matches = PresetsFor(tpl);
        var premium = PickPremium(matches);
        var results = new List<CatalogPreset>(matches.Count);
        foreach (var p in matches)
        {
            results.Add(new CatalogPreset
            {
                Id = p.Id.ToString(),
                Name = p.Name,
                ItemCount = p.Items?.Count() ?? 0,
                IsDefault = p.Encyclopedia is not null,
                IsPremium = ReferenceEquals(p, premium),
            });
        }

        return results;
    }

    /// <summary>All presets rooted at a tpl (root = first item — ref: PresetController.cs:32).</summary>
    private List<Preset> PresetsFor(MongoId tpl)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;   // ref: Globals.cs:23
        return itemPresets.Values
            .Where(p => p.Items?.FirstOrDefault()?.Template == tpl)
            .ToList();
    }

    /// <summary>
    ///     Mirrors InventoryBuilder.ResolvePreset (default = encyclopedia, else first match; key may be a
    ///     preset id). Duplicated on purpose: InventoryBuilder's resolver is private and that file is owned
    ///     by another work item (021) — do not edit it.
    /// </summary>
    internal Preset? ResolveDefaultPreset(MongoId key)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;
        if (itemPresets.TryGetValue(key, out var byId))
        {
            return byId;
        }

        var matches = PresetsFor(key);
        return matches.FirstOrDefault(p => p.Encyclopedia is not null) ?? matches.FirstOrDefault();
    }

    /// <summary>
    ///     Mirrors InventoryBuilder.ResolvePremiumPreset (most-kitted; prefers builds without thermal/NV —
    ///     CR-02-01). Same duplication rationale as <see cref="ResolveDefaultPreset"/>.
    /// </summary>
    internal Preset? ResolvePremiumPreset(MongoId key)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;
        if (itemPresets.TryGetValue(key, out var byId))
        {
            return byId;
        }

        return PickPremium(PresetsFor(key));
    }

    /// <summary>
    ///     Mirrors InventoryBuilder.ResolveStashPreset (smallest preset that already carries a real optic,
    ///     else default) — used for stash/contents items so cost matches what the builder actually spawns.
    /// </summary>
    internal Preset? ResolveStashPreset(MongoId key)
    {
        var itemPresets = databaseService.GetGlobals().ItemPresets;
        if (itemPresets.TryGetValue(key, out var byId))
        {
            return byId;
        }

        var matches = PresetsFor(key);
        if (matches.Count == 0)
        {
            return null;
        }

        var scoped = matches
            .Where(p => p.Items!.Any(i => IsRealOptic(i.Template)))
            .OrderBy(p => p.Items!.Count())
            .FirstOrDefault();
        if (scoped is not null)
        {
            return scoped;
        }

        return matches.FirstOrDefault(p => p.Encyclopedia is not null) ?? matches[0];
    }

    private Preset? PickPremium(List<Preset> matches)
    {
        Preset? best = null;
        var bestCount = -1;
        Preset? bestNoThermal = null;
        var bestNoThermalCount = -1;
        foreach (var p in matches)
        {
            var count = p.Items?.Count() ?? 0;
            if (count > bestCount)
            {
                bestCount = count;
                best = p;
            }

            if (count > bestNoThermalCount && !(p.Items?.Any(i => IsThermalOrNv(i.Template)) ?? false))
            {
                bestNoThermalCount = count;
                bestNoThermal = p;
            }
        }

        return bestNoThermal ?? best;
    }

    /// <summary>Real-optic baseclasses — mirrors InventoryBuilder.OpticBaseclasses (excludes iron sights).</summary>
    private static readonly MongoId[] OpticBaseclasses =
    [
        BaseClasses.ASSAULT_SCOPE, BaseClasses.COLLIMATOR, BaseClasses.COMPACT_COLLIMATOR,
        BaseClasses.OPTIC_SCOPE, BaseClasses.SPECIAL_SCOPE,
    ];

    /// <summary>Thermal/NV — mirrors InventoryBuilder.ThermalNvBaseclasses.</summary>
    private static readonly MongoId[] ThermalNvBaseclasses = [BaseClasses.THERMAL_VISION, BaseClasses.NIGHT_VISION];

    private bool IsRealOptic(MongoId tpl) => itemHelper.IsOfBaseclasses(tpl, OpticBaseclasses);

    private bool IsThermalOrNv(MongoId tpl) => itemHelper.IsOfBaseclasses(tpl, ThermalNvBaseclasses);

    // ── Ammo (item 023 — AmmoPicker) ─────────────────────────────────────────

    /// <summary>
    ///     Caliber string for a tpl. Weapons carry it as <c>_props.ammoCaliber</c>; cartridges as
    ///     <c>_props.Caliber</c>. Returns null when the template has neither (not a weapon/ammo).
    /// </summary>
    public string? GetCaliber(MongoId tpl)
    {
        var props = GetTemplate(tpl)?.Properties;
        return props?.AmmoCaliber ?? props?.Caliber;
    }

    /// <summary>Cartridges loadable into <paramref name="weaponTpl"/> (matched by caliber).</summary>
    public List<CatalogAmmo> GetAmmoForWeapon(MongoId weaponTpl)
    {
        var caliber = GetCaliber(weaponTpl);
        return caliber is null ? [] : GetAmmoByCaliber(caliber);
    }

    /// <summary>
    ///     All AMMO-baseclass cartridges of a caliber, sorted by penetration desc. Damage/penetration
    ///     come straight from <c>_props</c> (cheap — no ballistics math).
    /// </summary>
    public List<CatalogAmmo> GetAmmoByCaliber(string caliber)
    {
        var en = GetLocale("en");
        var results = new List<CatalogAmmo>();
        foreach (var (tpl, template) in databaseService.GetItems())
        {
            if (!string.Equals(template.Type, "Item", StringComparison.Ordinal))
            {
                continue;
            }

            var props = template.Properties;
            if (props?.Caliber is null || !string.Equals(props.Caliber, caliber, StringComparison.Ordinal))
            {
                continue;
            }

            if (!itemHelper.IsOfBaseclass(tpl, BaseClasses.AMMO))
            {
                continue;   // exclude ammo boxes / grenade "calibers" piggybacking the prop
            }

            var key = tpl.ToString();
            en.TryGetValue($"{key} Name", out var name);
            en.TryGetValue($"{key} ShortName", out var shortName);
            var (price, _) = GetPrice(tpl);
            results.Add(new CatalogAmmo
            {
                Tpl = key,
                Name = !string.IsNullOrWhiteSpace(name) ? name! : template.Name ?? key,
                ShortName = shortName,
                Price = price,
                Damage = props.Damage,
                Penetration = props.PenetrationPower,
                Caliber = caliber,
            });
        }

        return results.OrderByDescending(a => a.Penetration ?? 0).ToList();
    }

    // ── Customization (outfit upper/lower) ───────────────────────────────────

    /// <summary>
    ///     Clothing pieces selectable as outfit for a faction ("Usec" | "Bear"), following the
    ///     OutfitBuilder acceptance rules: upper = _props.Body ref or BodyPart=="Body"; lower = _props.Feet
    ///     ref or BodyPart=="Feet"; empty Side list = usable by both factions (lenient, PA-01-03).
    ///     UI-02: human-readable names live in the locale under <c>"{id} Name"</c> (e.g.
    ///     64ef3fa81a5f313cb144bf89 → "USEC Predator"); <c>_props.Name</c>/<c>_name</c> are internal keys
    ///     ("Blackknight_Suite", "pmc_kit_blackknight_quest") — locale first (en, pt), template names last.
    /// </summary>
    public List<CatalogClothing> GetClothing(string sideName)
    {
        var en = GetLocale("en");
        var pt = GetLocale("pt");
        var pieces = new List<CatalogClothing>();
        foreach (var (id, item) in databaseService.GetCustomization())   // ref: DatabaseService.cs:117
        {
            var props = item.Properties;
            if (props is null || !string.Equals(item.Type, "Item", StringComparison.Ordinal))
            {
                continue;   // skip "Node" rows and malformed mod data
            }

            if (props.Side is { Count: > 0 } sides && !sides.Contains(sideName))
            {
                continue;
            }

            var isUpper = props.Body is not null
                || string.Equals(props.BodyPart, "Body", StringComparison.OrdinalIgnoreCase);
            var isLower = props.Feet is not null
                || string.Equals(props.BodyPart, "Feet", StringComparison.OrdinalIgnoreCase);
            if (!isUpper && !isLower)
            {
                continue;   // hands/heads/voices are not configurable as outfit (item 004)
            }

            var key = id.ToString();
            var name = en.TryGetValue($"{key} Name", out var enName) && !string.IsNullOrWhiteSpace(enName) ? enName
                : pt.TryGetValue($"{key} Name", out var ptName) && !string.IsNullOrWhiteSpace(ptName) ? ptName
                : props.Name ?? item.Name;

            pieces.Add(new CatalogClothing
            {
                Id = key,
                Name = name,
                Slot = isUpper ? "upper" : "lower",
            });
        }

        return pieces;
    }

    // ── Slots (item 026) ─────────────────────────────────────────────────────

    /// <summary>
    ///     "Default Inventory" template — the character itself as an item. Its <c>_props.Slots</c> ARE the
    ///     equipment slots: slot <c>_name</c> matches the <c>EquipmentSlots</c> enum names exactly and
    ///     <c>filters[0].Filter</c> lists the allowed baseclasses (verified against
    ///     SPT_Data/database/templates/items.json — e.g. TacticalVest → 5448e5284bdc2dcb718b4567 = VEST).
    ///     Same mechanism as mod slots on weapons — one code path covers both.
    /// </summary>
    public const string DefaultInventoryTpl = "55d7217a4bdc2d86028b456d";

    /// <summary>Slots declared by a template (<c>_props.Slots</c>). Unknown/malformed tpl → empty.</summary>
    public List<CatalogSlotInfo> GetSlotsOf(string tpl)
    {
        var id = TryParseMongoId(tpl);
        var slots = id is null ? null : GetTemplate(id.Value)?.Properties?.Slots;
        if (slots is null)
        {
            return [];
        }

        var result = new List<CatalogSlotInfo>();
        foreach (var slot in slots)
        {
            if (!string.IsNullOrEmpty(slot.Name))
            {
                result.Add(new CatalogSlotInfo { Id = slot.Name!, Required = slot.Required ?? false });
            }
        }

        return result;
    }

    /// <summary>
    ///     Allowed tpls/baseclasses of one slot (<c>_props.Slots[slotId]._props.filters[0].Filter</c> —
    ///     same source InventoryBuilder.EnsureMinimumOptic reads). Missing slot/template → empty.
    /// </summary>
    public List<string> GetSlotFilter(string parentTpl, string slotId)
    {
        var id = TryParseMongoId(parentTpl);
        var filter = id is null ? null : SlotFilterOf(id.Value, slotId);
        return filter?.Select(f => f.ToString()).ToList() ?? [];
    }

    /// <summary>
    ///     Whether <paramref name="candidateTpl"/> fits (parentTpl, slotId): the filter contains the tpl
    ///     itself OR one of its baseclasses (resolved via <c>ItemHelper.IsOfBaseclass</c> — walks the
    ///     <c>_parent</c> chain). LENIENT by design (warning-grade, mirrors the loader): returns true when
    ///     it cannot evaluate (malformed ids, unknown/modded parent template) — only a slot that EXISTS
    ///     and whose filter excludes the candidate returns false. The Save dry-run has the final word.
    /// </summary>
    public bool IsAllowedInSlot(string parentTpl, string slotId, string candidateTpl)
    {
        var parent = TryParseMongoId(parentTpl);
        var candidate = TryParseMongoId(candidateTpl);
        if (parent is null || candidate is null || GetTemplate(parent.Value) is null)
        {
            return true;   // cannot evaluate — unresolved ids surface elsewhere
        }

        var filter = SlotFilterOf(parent.Value, slotId);
        if (filter is null)
        {
            return false;   // slot absent on the template — nothing fits there
        }

        if (filter.Count == 0)
        {
            return true;    // declared but unconstrained (rare) — allow
        }

        return filter.Contains(candidate.Value)
            || filter.Any(f => itemHelper.IsOfBaseclass(candidate.Value, f));   // ref: ItemHelper.cs (parent chain)
    }

    /// <summary>Equipment-slot variant of <see cref="GetSlotFilter"/> (Default Inventory template).</summary>
    public List<string> GetEquipmentSlotFilter(string slotName) =>
        GetSlotFilter(DefaultInventoryTpl, slotName);

    /// <summary>Equipment-slot variant of <see cref="IsAllowedInSlot"/> (Default Inventory template).</summary>
    public bool IsAllowedInEquipmentSlot(string slotName, string candidateTpl) =>
        IsAllowedInSlot(DefaultInventoryTpl, slotName, candidateTpl);

    /// <summary>True when the tpl resolves to a live item template (mods included — live DB).</summary>
    public bool TemplateExists(string tpl)
    {
        var id = TryParseMongoId(tpl);
        return id is not null && GetTemplate(id.Value) is not null;
    }

    /// <summary>True when the template declares container grids (<c>_props.Grids</c>) — rig/backpack/case.</summary>
    public bool HasGrids(string tpl)
    {
        var id = TryParseMongoId(tpl);
        var grids = id is null ? null : GetTemplate(id.Value)?.Properties?.Grids;
        return grids?.Any() ?? false;
    }

    /// <summary>True when the tpl descends from the WEAPON baseclass.</summary>
    public bool IsWeapon(string tpl)
    {
        var id = TryParseMongoId(tpl);
        return id is not null && itemHelper.IsOfBaseclass(id.Value, BaseClasses.WEAPON);
    }

    /// <summary>First filter of one named slot on a template (case-insensitive slot match).</summary>
    private HashSet<MongoId>? SlotFilterOf(MongoId parentTpl, string slotId)
    {
        var slots = GetTemplate(parentTpl)?.Properties?.Slots;
        var slot = slots?.FirstOrDefault(s => string.Equals(s.Name, slotId, StringComparison.OrdinalIgnoreCase));
        return slot?.Properties?.Filters?.FirstOrDefault()?.Filter;   // ref: TemplateItem.cs:1776
    }

    /// <summary>MongoId.IsValidMongoId-guarded parse — razor-friendly (raw strings, never throws).</summary>
    private static MongoId? TryParseMongoId(string? raw) =>
        raw is not null && MongoId.IsValidMongoId(raw) ? new MongoId(raw) : (MongoId?)null;

    // ── Editions ─────────────────────────────────────────────────────────────

    /// <summary>
    ///     All edition keys currently registered in the profile templates (vanilla + classes + other mods).
    ///     The caller filters mod classes out if it needs vanilla-only — this service stays registry-agnostic
    ///     (ClassVisualRegistry is owned by work item 021).
    /// </summary>
    public List<string> GetEditionKeys()
    {
        return databaseService.GetProfileTemplates().Keys.ToList();   // ref: DatabaseService.cs:141
    }

    private TemplateItem? GetTemplate(MongoId tpl)
    {
        var ci = itemHelper.GetItem(tpl);   // ref: ItemHelper.cs:494
        return ci is { Key: true, Value: not null } ? ci.Value : null;
    }
}
