using System.Collections.Concurrent;                 // locale cache
using System.Diagnostics;                            // item 037: Stopwatch (perf instrumentation)
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;                  // ItemHelper
using SPTarkov.Server.Core.Services;                 // DatabaseService, LocaleService
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Enums;             // BaseClasses
using SPTarkov.Server.Core.Models.Eft.Common;        // Preset
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // TemplateItem, CustomizationItem
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger (item 037)

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
    /// <summary>Ícone do handbook (ex.: <c>/files/handbook/icon_weapons.png</c>) — servido pelo próprio SPT.</summary>
    [JsonPropertyName("icon")] public string? Icon { get; init; }
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

/// <summary>One part (mod) that composes a preset — for the preset hover popover.</summary>
public sealed record CatalogPresetPart
{
    [JsonPropertyName("tpl")] public required string Tpl { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("count")] public int Count { get; init; }
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
public class CatalogService
{
    private const string SourceFlea = "flea";
    private const string SourceHandbook = "handbook";
    private const string SourceCurrency = "currency";
    private const string SourceMissing = "missing";

    private readonly DatabaseService databaseService;
    private readonly ItemHelper itemHelper;
    private readonly LocaleService localeService;
    private readonly ISptLogger<CatalogService> logger;   // item 037 (PA-R1-08): perf instrumentation

    // ── Item 037: lazy read-only indexes over the LIVE DB ──────────────────────
    // Premise PA-037-06: GetItems/GetHandbook/GetCustomization/locales are immutable AFTER boot. These
    // Lazy<T> are built on FIRST ACCESS (never eager in the ctor — see §7), i.e. after every mod's
    // PostDBModLoader has run, then read-only. Lazy<T> default mode (ExecutionAndPublication) is
    // thread-safe — fine for indexes built once and only read afterwards (PA-037-03). A mod mutating
    // the DB at runtime (unsupported) would see a stale index — accepted risk.
    private readonly Lazy<List<SearchIndexRow>> _searchIndex;
    private readonly Lazy<Dictionary<MongoId, string>> _handbookIndex;       // tpl → handbook category id
    private readonly Lazy<Dictionary<string, List<string>>> _childrenByParent; // category id → direct children
    private readonly Lazy<Dictionary<string, List<CatalogClothing>>> _clothingBySide; // "Usec"/"Bear" → pieces
    private readonly Lazy<Dictionary<MongoId, List<Preset>>> _presetsByRoot;  // root tpl → presets rooted at it

    public CatalogService(
        DatabaseService databaseService,
        ItemHelper itemHelper,
        LocaleService localeService,
        ISptLogger<CatalogService> logger)
    {
        this.databaseService = databaseService;
        this.itemHelper = itemHelper;
        this.localeService = localeService;
        this.logger = logger;

        // Index factories — invoked on first .Value access (post-boot), never here in the ctor.
        _handbookIndex = new Lazy<Dictionary<MongoId, string>>(BuildHandbookIndex);
        _searchIndex = new Lazy<List<SearchIndexRow>>(BuildSearchIndex);
        _childrenByParent = new Lazy<Dictionary<string, List<string>>>(BuildChildrenByParent);
        _clothingBySide = new Lazy<Dictionary<string, List<CatalogClothing>>>(BuildClothingBySide);
        _presetsByRoot = new Lazy<Dictionary<MongoId, List<Preset>>>(BuildPresetsByRoot);
    }

    /// <summary>Pre-computed search-index row (DB immutable post-boot — PA-037-06).</summary>
    private sealed record SearchIndexRow(
        string Tpl,
        string? EnNameLower,
        string? PtNameLower,
        string? ShortNameLower,
        string? TemplateNameLower,   // PA-R1-01: template.Name internal IS a match source in the current Search
        string EnNameDisplay,
        string? ShortNameDisplay,
        string? CategoryId);

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

    // localeService.GetLocaleDb re-materializes the whole locale dict on every call (the core Locales.Global
    // is a LazyLoad whose transformers — e.g. Fika's — re-run per .Value access; ~5ms, thousands of keys).
    // The cost walker resolves a name per loadout line and called GetLocale TWICE per item, so this scan
    // dominated ComputeLoadoutCost (~900ms/heavy class → GetItemName). Cache the dict per code (immutable
    // post-boot — PA-037-06, same premise as the lazy indexes above).
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _localeCache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>EFT locale dict for a language ("pt" → EFT code "po"); LocaleService falls back to en. Cached.</summary>
    private Dictionary<string, string> GetLocale(string lang)
    {
        var code = string.Equals(lang, "pt", StringComparison.OrdinalIgnoreCase) ? "po" : lang;
        return _localeCache.GetOrAdd(code, c => localeService.GetLocaleDb(c));   // ref: LocaleService.cs:20 (en fallback built in)
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

        // Item 037 instrumentation: differentiate the cold build (first access) from the hot scan.
        var cold = !_searchIndex.IsValueCreated;
        var sw = Stopwatch.StartNew();

        var q = query.Trim();
        var qLower = q.ToLowerInvariant();
        var scope = parentCategoryId is null ? null : CollectCategoryWithDescendants(parentCategoryId);

        // Order is IDENTICAL to the pre-037 Search (PA-R1-06): category → match(5 sources) →
        // filter(tpl) → GetPrice → add → cap@limit. GetPrice is computed only for the ≤limit matches
        // (PA-037-08: price is NOT frozen in the index — flea overrides at runtime — but it is cheap).
        foreach (var row in _searchIndex.Value)   // HOT: scan a compact in-memory list
        {
            if (scope is not null && (row.CategoryId is null || !scope.Contains(row.CategoryId)))
            {
                continue;
            }

            // PA-R1-01: the same 5 match sources as the original Search (tpl exact, en, pt, short,
            // internal template.Name) — TemplateNameLower preserves the internal-name match.
            var match = string.Equals(row.Tpl, q, StringComparison.OrdinalIgnoreCase)
                || (row.EnNameLower?.Contains(qLower, StringComparison.Ordinal) ?? false)
                || (row.PtNameLower?.Contains(qLower, StringComparison.Ordinal) ?? false)
                || (row.ShortNameLower?.Contains(qLower, StringComparison.Ordinal) ?? false)
                || (row.TemplateNameLower?.Contains(qLower, StringComparison.Ordinal) ?? false);
            if (!match)
            {
                continue;
            }

            if (filter is not null && !filter(row.Tpl))
            {
                continue;   // CR-EP-07: incompatible hit must not consume the result cap
            }

            var (price, source) = GetPrice(new MongoId(row.Tpl));
            results.Add(new CatalogItem
            {
                Tpl = row.Tpl,
                Name = row.EnNameDisplay,
                ShortName = row.ShortNameDisplay,
                Price = price,
                PriceSource = source,
                CategoryId = row.CategoryId,
            });

            if (results.Count >= limit)
            {
                break;
            }
        }

        logger.Debug($"[CustomClasses] [perf] Search '{q}': {results.Count} hit(s) over {_searchIndex.Value.Count} rows "
            + $"({(cold ? "cold/index-built" : "hot")}) in {sw.ElapsedMilliseconds} ms");
        return results;
    }

    /// <summary>
    ///     Item #6: lista os itens que passam num <paramref name="filter"/> (sem texto de busca) — usado
    ///     pra PRÉ-LISTAR os itens compatíveis com um slot ao abrir o picker. Mesma fonte/forma do
    ///     <see cref="Search"/>; só troca a condição de match (filtro do slot) pela busca textual.
    /// </summary>
    public List<CatalogItem> ListByFilter(Func<string, bool> filter, int limit = 100, string? parentCategoryId = null)
    {
        var results = new List<CatalogItem>();
        if (filter is null || limit <= 0)
        {
            return results;
        }

        var scope = parentCategoryId is null ? null : CollectCategoryWithDescendants(parentCategoryId);
        foreach (var row in _searchIndex.Value)
        {
            if (scope is not null && (row.CategoryId is null || !scope.Contains(row.CategoryId)))
            {
                continue;
            }

            if (!filter(row.Tpl))
            {
                continue;
            }

            var (price, source) = GetPrice(new MongoId(row.Tpl));
            results.Add(new CatalogItem
            {
                Tpl = row.Tpl,
                Name = row.EnNameDisplay,
                ShortName = row.ShortNameDisplay,
                Price = price,
                PriceSource = source,
                CategoryId = row.CategoryId,
            });

            if (results.Count >= limit)
            {
                break;
            }
        }

        return results;
    }

    /// <summary>
    ///     Item 037: pre-computed search index over item templates (<c>_type == "Item"</c>). Resolves
    ///     en/pt/short locale + handbook category ONCE; the hot <see cref="Search"/> scans this compact
    ///     list (the expensive part the index eliminates is per-item locale resolution over the whole DB).
    /// </summary>
    private List<SearchIndexRow> BuildSearchIndex()
    {
        var sw = Stopwatch.StartNew();
        var en = GetLocale("en");
        var pt = GetLocale("pt");
        var handbook = _handbookIndex.Value;
        var rows = new List<SearchIndexRow>();

        foreach (var (tpl, template) in databaseService.GetItems())   // ref: DatabaseService.cs:129
        {
            if (!string.Equals(template.Type, "Item", StringComparison.Ordinal))
            {
                continue;   // skip "Node" type rows (category nodes inside items.json)
            }

            var key = tpl.ToString();
            en.TryGetValue($"{key} Name", out var enName);
            en.TryGetValue($"{key} ShortName", out var shortName);
            pt.TryGetValue($"{key} Name", out var ptName);
            handbook.TryGetValue(tpl, out var categoryId);

            // PA-037-07: nothing drops from search for lack of locale — the display name falls back to
            // template.Name/tpl and template.Name stays a match source.
            rows.Add(new SearchIndexRow(
                key,
                enName?.ToLowerInvariant(),
                ptName?.ToLowerInvariant(),
                shortName?.ToLowerInvariant(),
                template.Name?.ToLowerInvariant(),
                !string.IsNullOrWhiteSpace(enName) ? enName! : template.Name ?? key,
                shortName,
                categoryId));
        }

        logger.Debug($"[CustomClasses] [perf] BuildSearchIndex: {rows.Count} rows in {sw.ElapsedMilliseconds} ms");
        return rows;
    }

    /// <summary>tpl → handbook category id (handbook.Items[].ParentId). Item 037: built once (lazy).</summary>
    private Dictionary<MongoId, string> BuildHandbookIndex()
    {
        var index = new Dictionary<MongoId, string>();
        foreach (var entry in databaseService.GetHandbook().Items)   // ref: DatabaseService.cs:123, HandbookBase.cs
        {
            index[entry.Id] = entry.ParentId.ToString();
        }

        return index;
    }

    /// <summary>
    ///     category id → direct children (handbook.Categories tree by ParentId). Item 037 (PA-R1-04):
    ///     the EXPENSIVE part (building this parent→children map) is cached once; the per-root BFS in
    ///     <see cref="CollectCategoryWithDescendants"/> is cheap and stays per-call over this read-only map.
    /// </summary>
    private Dictionary<string, List<string>> BuildChildrenByParent()
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

        return children;
    }

    /// <summary>Set with a category id + all its descendants (BFS over the cached parent→children map).</summary>
    private HashSet<string> CollectCategoryWithDescendants(string rootId)
    {
        var children = _childrenByParent.Value;   // item 037: cached tree (built once)
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
            categories.Add(new CatalogCategory { Id = id, ParentId = cat.ParentId?.ToString(), Name = name, Icon = cat.Icon });
        }

        return categories;
    }

    // ── Dimensions / category resolution (item 034 — gear/stash visual cells) ──

    /// <summary>
    ///     Item dimensions in inventory cells (<c>_props.Width × _props.Height</c>). Unknown/malformed tpl
    ///     or missing props default to 1×1 (corner case 034 — never zero, never throws). Read-only over the
    ///     live DB via the same <see cref="GetTemplate"/> path as the rest of the service.
    /// </summary>
    public (int Width, int Height) GetItemDimensions(string tpl)
    {
        var id = TryParseMongoId(tpl);
        var props = id is null ? null : GetTemplate(id.Value)?.Properties;
        var w = props?.Width ?? 1;
        var h = props?.Height ?? 1;
        return (w > 0 ? w : 1, h > 0 ? h : 1);
    }

    /// <summary>
    ///     tpl → handbook category id (null when the tpl is absent from the handbook). Cheap O(1) over the
    ///     lazy <see cref="_handbookIndex"/> (item 037) — the StashPanel resolves N lines with this + a
    ///     local id→name map (see CR-034-03), avoiding the per-line GetCategories rebuild.
    /// </summary>
    public string? GetCategoryId(string tpl)
    {
        var id = TryParseMongoId(tpl);
        return id is not null && _handbookIndex.Value.TryGetValue(id.Value, out var catId) ? catId : null;
    }

    /// <summary>
    ///     Localized handbook category NAME for a tpl, or null when the tpl is not in the handbook
    ///     (corner case 034 — caller falls back to an "Other" group). Reuses <see cref="GetCategoryId"/>
    ///     (tpl → category id) + <see cref="GetCategories"/> (id → localized name). lang: "en" | "pt".
    ///     Single-tpl convenience for GearPanel/ItemTooltip (few calls). The StashPanel does NOT use this
    ///     per-line — it builds an id→name map once from GetCategories (CR-034-03).
    /// </summary>
    public string? GetCategoryName(string tpl, string lang = "en")
    {
        var catId = GetCategoryId(tpl);
        if (catId is null)
        {
            return null;
        }

        var cats = GetCategories(lang);
        return cats.FirstOrDefault(c => string.Equals(c.Id, catId, StringComparison.Ordinal))?.Name;
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

    /// <summary>
    ///     The mods that compose a preset (for the hover popover): every item below the root, grouped by
    ///     tpl (qty summed — e.g. loaded cartridges), named and sorted. The root (the weapon itself) is
    ///     skipped — ref: PresetController.cs:32, first item is the root. Unknown/missing id → empty.
    /// </summary>
    public List<CatalogPresetPart> GetPresetParts(string presetId)
    {
        if (TryParseMongoId(presetId) is not { } id
            || !databaseService.GetGlobals().ItemPresets.TryGetValue(id, out var preset)
            || preset.Items is not { Count: > 0 } items)
        {
            return [];
        }

        var byTpl = new Dictionary<MongoId, int>();
        foreach (var it in items.Skip(1))   // skip the root weapon/armor
        {
            var qty = (int)Math.Max(1, it.Upd?.StackObjectsCount ?? 1);
            byTpl[it.Template] = byTpl.GetValueOrDefault(it.Template) + qty;
        }

        return byTpl
            .Select(kv => new CatalogPresetPart { Tpl = kv.Key.ToString(), Name = GetItemName(kv.Key), Count = kv.Value })
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    ///     All presets rooted at a tpl (root = first item — ref: PresetController.cs:32). O(1) lookup over
    ///     the <see cref="_presetsByRoot"/> index: the cost walker resolves a preset for almost every
    ///     equipped/stash line, and the old per-call linear scan over the thousands-strong
    ///     globals.ItemPresets dominated ComputeLoadoutCost (~1s/heavy class). Returns the shared cached
    ///     list — callers only read it (FirstOrDefault/Where/OrderBy/indexing), never mutate.
    /// </summary>
    private List<Preset> PresetsFor(MongoId tpl) =>
        _presetsByRoot.Value.TryGetValue(tpl, out var list) ? list : [];

    /// <summary>
    ///     Indexes every <c>globals.ItemPresets</c> entry by its root tpl (first item's Template) so
    ///     <see cref="PresetsFor"/> is a dictionary read instead of a full scan. Built once on first access
    ///     (post-boot, DB immutable — PA-037-06), same premise/lifecycle as the other lazy indexes.
    /// </summary>
    private Dictionary<MongoId, List<Preset>> BuildPresetsByRoot()
    {
        var index = new Dictionary<MongoId, List<Preset>>();
        foreach (var preset in databaseService.GetGlobals().ItemPresets.Values)   // ref: Globals.cs:23
        {
            if (preset.Items?.FirstOrDefault()?.Template is not { } root)
            {
                continue;
            }

            if (!index.TryGetValue(root, out var list))
            {
                list = [];
                index[root] = list;
            }

            list.Add(preset);
        }

        return index;
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
        // Item 037: clothing per side is pre-computed once (lazy). Returns the cached list for the side
        // ("Usec"/"Bear"); unknown side → empty. The list is treated as read-only by callers.
        return _clothingBySide.Value.TryGetValue(sideName, out var pieces) ? pieces : [];
    }

    /// <summary>
    ///     Item 037: pre-computes the outfit-eligible clothing pieces for "Usec" and "Bear" in ONE pass
    ///     over the customization table (was one full scan per GetClothing call, twice per editor load).
    ///     Same OutfitBuilder acceptance rules and locale-first naming as before — a piece with an empty
    ///     Side list is usable by both factions (lenient, PA-01-03), so it lands in both lists.
    /// </summary>
    private Dictionary<string, List<CatalogClothing>> BuildClothingBySide()
    {
        var sw = Stopwatch.StartNew();
        var en = GetLocale("en");
        var pt = GetLocale("pt");
        var bySide = new Dictionary<string, List<CatalogClothing>>(StringComparer.Ordinal)
        {
            ["Usec"] = [],
            ["Bear"] = [],
        };

        foreach (var (id, item) in databaseService.GetCustomization())   // ref: DatabaseService.cs:117
        {
            var props = item.Properties;
            if (props is null || !string.Equals(item.Type, "Item", StringComparison.Ordinal))
            {
                continue;   // skip "Node" rows and malformed mod data
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

            var piece = new CatalogClothing { Id = key, Name = name, Slot = isUpper ? "upper" : "lower" };

            // Empty Side list = both factions; otherwise only the listed sides.
            var sides = props.Side is { Count: > 0 } declared ? declared : (IEnumerable<string>)["Usec", "Bear"];
            foreach (var side in sides)
            {
                if (bySide.TryGetValue(side, out var list))
                {
                    list.Add(piece);
                }
            }
        }

        logger.Debug($"[CustomClasses] [perf] BuildClothingBySide: "
            + $"{bySide["Usec"].Count} Usec / {bySide["Bear"].Count} Bear in {sw.ElapsedMilliseconds} ms");
        return bySide;
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

    /// <summary>
    ///     The slot's SINGLE concrete option, when there is exactly one — the editor auto-selects it instead
    ///     of opening a picker with one choice. Returns the tpl only when the slot filter
    ///     (<see cref="GetSlotFilter"/>) has exactly ONE entry AND that entry is a concrete item
    ///     (<c>_type == "Item"</c>); null otherwise: zero options, several, or the lone entry is a
    ///     baseclass/Node (which would expand into many items — NOT expanded here, that would scan the
    ///     catalog). Conservative on purpose: a null just means "let the user pick", never a wrong auto-fill.
    /// </summary>
    public string? SingleSlotOption(string parentTpl, string slotId)
    {
        var filter = GetSlotFilter(parentTpl, slotId);
        if (filter.Count != 1)
        {
            return null;
        }

        var only = filter[0];
        var id = TryParseMongoId(only);
        if (id is null)
        {
            return null;
        }

        var template = GetTemplate(id.Value);
        return template is not null && string.Equals(template.Type, "Item", StringComparison.Ordinal) ? only : null;
    }

    /// <summary>
    ///     True when an item is built MANUALLY (explicit <c>mods</c>, no <c>preset</c>) and at least one
    ///     REQUIRED slot in its tree is empty — i.e. the builder will NOT auto-complete the default preset,
    ///     so the mandatory part genuinely is missing. Mirrors the editor's required-slot validation; lets
    ///     the silhouette / stash flag a pending item WITHOUT opening it. Preset / auto (empty mods) items
    ///     never report a pending required (the preset fills them at build).
    /// </summary>
    public bool HasMissingRequiredMods(ItemSpec? spec)
    {
        if (spec is null
            || !string.IsNullOrWhiteSpace(spec.Preset)   // preset mode — builder completes the tree
            || spec.Mods is not { Count: > 0 }            // empty mods — default preset auto-completes
            || string.IsNullOrWhiteSpace(spec.Tpl))
        {
            return false;
        }

        return CountMissingRequiredInTree(spec.Tpl!, spec.Mods, 0) > 0;
    }

    /// <summary>Recursive count of empty REQUIRED slots in a manual mod tree (depth-capped like the editor).</summary>
    private int CountMissingRequiredInTree(string parentTpl, List<ModSpec>? mods, int depth)
    {
        if (depth >= 6)
        {
            return 0;
        }

        var missing = 0;
        foreach (var slot in GetSlotsOf(parentTpl))
        {
            var mod = mods?.FirstOrDefault(m => string.Equals(m.SlotId, slot.Id, StringComparison.OrdinalIgnoreCase));
            if (slot.Required && string.IsNullOrWhiteSpace(mod?.Tpl))
            {
                missing++;
            }

            if (mod?.Tpl is { Length: > 0 } subTpl && TemplateExists(subTpl))
            {
                missing += CountMissingRequiredInTree(subTpl, mod.Mods, depth + 1);
            }
        }

        return missing;
    }

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
