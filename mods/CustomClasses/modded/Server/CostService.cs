using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;                  // ItemHelper, InventoryHelper
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Enums;             // SkillTypes
using SPTarkov.Server.Core.Models.Eft.Common;        // Preset
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // Item, Grid

namespace CustomClasses;

/// <summary>Per-skill line of the weighted skill cost.</summary>
public sealed record SkillCostEntry
{
    [JsonPropertyName("skill")] public required string Skill { get; init; }
    [JsonPropertyName("level")] public int Level { get; init; }
    [JsonPropertyName("weight")] public double Weight { get; init; }
    /// <summary>Explicit | Derived | CategoryFallback | UnmappedFallback (see <see cref="SkillWeightOrigin"/>).</summary>
    [JsonPropertyName("origin")] public required string Origin { get; init; }
    [JsonPropertyName("cost")] public double Cost { get; init; }
}

/// <summary>Weighted skill cost of a class (RZ formula: Σ level × weight, budget 28–32).</summary>
public sealed record SkillCostBreakdown
{
    [JsonPropertyName("skills")] public required List<SkillCostEntry> Skills { get; init; }
    [JsonPropertyName("total")] public double Total { get; init; }
    [JsonPropertyName("withinBudget")] public bool WithinBudget { get; init; }
    /// <summary>Non-blocking design warnings (budget, category coverage, skill count, level cap).</summary>
    [JsonPropertyName("warnings")] public required List<string> Warnings { get; init; }
}

/// <summary>Per-item line of the loadout cost (one line per tpl per spec; presets are expanded).</summary>
public sealed record LoadoutCostEntry
{
    [JsonPropertyName("tpl")] public required string Tpl { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    /// <summary>Where the item comes from: "equipped:&lt;slot&gt;", "stash", "contents", "ammo".</summary>
    [JsonPropertyName("context")] public required string Context { get; init; }
    [JsonPropertyName("qty")] public double Qty { get; init; }
    [JsonPropertyName("unitPrice")] public double UnitPrice { get; init; }
    /// <summary>"flea" | "handbook" | "currency" | "missing" (see CatalogService.GetPrice).</summary>
    [JsonPropertyName("priceSource")] public required string PriceSource { get; init; }
    [JsonPropertyName("subtotal")] public double Subtotal { get; init; }
    /// <summary>True when no price was found — subtotal 0, surfaced as a badge in the UI (never silent).</summary>
    [JsonPropertyName("missingPrice")] public bool MissingPrice { get; init; }
}

/// <summary>Total rouble value of a class loadout (equipped + stash, mods/contents/ammo included).</summary>
public sealed record LoadoutCostBreakdown
{
    [JsonPropertyName("items")] public required List<LoadoutCostEntry> Items { get; init; }
    [JsonPropertyName("totalRub")] public double TotalRub { get; init; }
    [JsonPropertyName("warnings")] public required List<string> Warnings { get; init; }
}

/// <summary>Stash capacity dry-run outcome (item 028) — warning-grade, never blocks a save.</summary>
public sealed record StashCapacityResult
{
    /// <summary>False when the base edition / stash grid could not be resolved (details in Warnings).</summary>
    [JsonPropertyName("gridResolved")] public bool GridResolved { get; init; }

    /// <summary>Base edition whose stash template sized the dry-run grid (default applied when blank).</summary>
    [JsonPropertyName("baseEdition")] public required string BaseEdition { get; init; }

    /// <summary>Grid dimensions as "W×H" (stash templates declare a single grid; extras would be listed).</summary>
    [JsonPropertyName("gridSizes")] public required List<string> GridSizes { get; init; }

    [JsonPropertyName("totalCells")] public int TotalCells { get; init; }
    [JsonPropertyName("usedCells")] public int UsedCells { get; init; }
    [JsonPropertyName("occupancyPercent")] public double OccupancyPercent { get; init; }

    /// <summary>Root placements that fit (assembled trees / split stacks — matches the builder's adds).</summary>
    [JsonPropertyName("placedCount")] public int PlacedCount { get; init; }

    /// <summary>"Name ×units" per stash line that ran out of space — the server skips those with a warning.</summary>
    [JsonPropertyName("unplaced")] public required List<string> Unplaced { get; init; }

    [JsonPropertyName("warnings")] public required List<string> Warnings { get; init; }
}

/// <summary>
///     Item 022: class cost computation — faithful port of the RZCustomProfiles model.
///     Skills: weighted cost Σ level × weight (<see cref="SkillWeights"/>, budget target 28–32,
///     informative rules as non-blocking warnings). Loadout: total ₽ walking equipped + stash
///     (presets expanded, mods/contents/ammo included), flea-first pricing with handbook fallback
///     via <see cref="CatalogService.GetPrice"/>. XP multipliers stay OUT of the cost by design
///     (user decision — display only). Preset/tree resolution mirrors InventoryBuilder so the cost
///     matches what the builder actually spawns. Item 028 adds the stash capacity dry-run
///     (<see cref="CheckStashCapacity"/>) — same packing pipeline, warning-grade.
///     Known accepted gap (CR2-EP-05): the minimum optic the builder injects on optic-less weapons
///     (<c>InventoryBuilder.EnsureMinimumOptic</c>, equipped and stash/contents) is NOT priced —
///     undercost of roughly 4–30k ₽ per weapon without a factory optic. Documented limitation:
///     mirroring it would duplicate the builder's optic resolution for marginal display value.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class CostService(
    CatalogService catalog,
    ItemHelper itemHelper,
    InventoryHelper inventoryHelper,
    DatabaseService databaseService)
{
    // ── Skills ───────────────────────────────────────────────────────────────

    /// <summary>Weighted skill cost of a class (RZ weightedCost + validateProfile rules as warnings).</summary>
    public SkillCostBreakdown ComputeSkillCost(ClassDefinition def)
    {
        var entries = new List<SkillCostEntry>();
        var warnings = new List<string>();
        var coveredCategories = new HashSet<string>(StringComparer.Ordinal);
        double total = 0;
        var skillsWithPoints = 0;

        foreach (var (skillName, level) in def.Skills ?? new Dictionary<string, int>())
        {
            double weight;
            SkillWeightOrigin origin;

            // mirrors CustomClassesMod.ApplySkills name handling (PA-01-01: reject undefined enum values)
            if (Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var skill)
                && Enum.IsDefined(typeof(SkillTypes), skill))
            {
                (weight, origin) = SkillWeights.ResolveWeight(skill);
                if (origin == SkillWeightOrigin.CategoryFallback)
                {
                    warnings.Add($"Skill '{skill}' has no explicit weight — category median {weight:0.00} used.");
                }
                else if (origin == SkillWeightOrigin.UnmappedFallback)
                {
                    warnings.Add($"Skill '{skill}' is outside every weight/category map — neutral 1.00 used.");
                }

                if (level > 0 && SkillWeights.Categories.TryGetValue(skill, out var category))
                {
                    coveredCategories.Add(category);
                }
            }
            else
            {
                // unknown name: still costed (neutral 1.00) so the total is never silently underestimated
                weight = 1.00;
                origin = SkillWeightOrigin.UnmappedFallback;
                warnings.Add($"Unknown skill '{skillName}' — neutral weight 1.00 used (loader will ignore it).");
            }

            var cost = Math.Round(level * weight, 2);
            total += level * weight;
            if (level > 0)
            {
                skillsWithPoints++;
            }

            if (level > SkillWeights.SuggestedLevelCap)
            {
                warnings.Add($"Skill '{skillName}' = {level} exceeds the suggested cap of {SkillWeights.SuggestedLevelCap}.");
            }

            entries.Add(new SkillCostEntry
            {
                Skill = skillName,
                Level = level,
                Weight = weight,
                Origin = origin.ToString(),
                Cost = cost,
            });
        }

        total = Math.Round(total, 2);
        var withinBudget = total >= SkillWeights.BudgetMin && total <= SkillWeights.BudgetMax;

        // informative rules (RZ validateProfile) — only meaningful when the class actually assigns points
        // (an intentionally empty class like Peladão reports total 0 without warning noise)
        if (skillsWithPoints > 0)
        {
            if (!withinBudget)
            {
                warnings.Add($"Weighted cost {total:0.00} outside the [{SkillWeights.BudgetMin:0}, {SkillWeights.BudgetMax:0}] budget.");
            }

            var missing = new[] { "Ph", "M", "C", "P" }.Where(c => !coveredCategories.Contains(c)).ToList();
            if (missing.Count > 0)
            {
                warnings.Add($"Categories without coverage (≥1 point each expected): {string.Join(", ", missing)}.");
            }

            if (skillsWithPoints > SkillWeights.MaxSkillsWithPoints)
            {
                warnings.Add($"{skillsWithPoints} skills with points — design rule suggests at most {SkillWeights.MaxSkillsWithPoints}.");
            }
        }

        return new SkillCostBreakdown
        {
            Skills = entries,
            Total = total,
            WithinBudget = withinBudget,
            Warnings = warnings,
        };
    }

    // ── Loadout ──────────────────────────────────────────────────────────────

    /// <summary>
    ///     Total rouble value of the class loadout (RZ loadoutTotalRub analogue). Walks equipped + stash,
    ///     expanding presets (auto-completion mirrors InventoryBuilder: equipped bare tpl → default preset;
    ///     stash/contents → stash preset), manual mod trees, container contents and magazine/chamber ammo.
    ///     CR-EP-01: stash/contents lines honor the FULL spec semantics in the builder (explicit preset /
    ///     mods / ammo / contents), so they are costed identically to equipped lines; equipped count&gt;1 is
    ///     costed as 1 (CR-EP-04 — the builder spawns 1 per slot).
    /// </summary>
    public LoadoutCostBreakdown ComputeLoadoutCost(ClassDefinition def)
    {
        var items = new List<LoadoutCostEntry>();
        var warnings = new List<string>();

        foreach (var (slotName, spec) in def.Loadout?.Equipped ?? new Dictionary<string, ItemSpec>())
        {
            // CR2-EP-07: mirror InventoryBuilder.Apply — an unknown slot name is skipped with a
            // warning there, so it must not be priced here either.
            if (!Enum.TryParse<EquipmentSlots>(slotName, ignoreCase: true, out var slot)
                || !Enum.IsDefined(typeof(EquipmentSlots), slot))
            {
                warnings.Add($"equipped:{slotName}: unknown equipment slot — the builder ignores it; line not costed.");
                continue;
            }

            AddSpec(spec, equipped: true, $"equipped:{slotName}", items, warnings);
        }

        foreach (var spec in def.Loadout?.Stash ?? [])
        {
            AddSpec(spec, equipped: false, "stash", items, warnings);
        }

        var missing = items.Count(i => i.MissingPrice);
        if (missing > 0)
        {
            warnings.Add($"{missing} item line(s) without any price (flea + handbook) — counted as 0 ₽.");
        }

        return new LoadoutCostBreakdown
        {
            Items = items,
            TotalRub = Math.Round(items.Sum(i => i.Subtotal)),
            Warnings = warnings,
        };
    }

    /// <param name="multiplier">
    ///     CR2-EP-01: contents are packed into EVERY placed unit of the parent line (builder's
    ///     PlaceSpecTrees/PackSpecsIntoGrids recurse per unit/stack) — the parent's count multiplies
    ///     everything below it, recursively.
    /// </param>
    private void AddSpec(ItemSpec spec, bool equipped, string context, List<LoadoutCostEntry> items, List<string> warnings, int multiplier = 1)
    {
        // CR-EP-04: the builder spawns exactly 1 item per equipped slot (rule 17 / PA-02-05) —
        // price 1× and surface the discrepancy instead of multiplying.
        var count = Math.Max(1, spec.Count) * multiplier;
        if (equipped && count > 1)
        {
            warnings.Add($"{context}: count {count} on an equipped slot — the builder spawns 1 (schema rule 17); costed as 1.");
            count = 1;
        }

        Preset? preset = null;
        var hasManualTree = false;

        if (!string.IsNullOrWhiteSpace(spec.Preset))
        {
            var key = TryMongoId(spec.Preset!, context, warnings);
            if (key is null)
            {
                return;
            }

            preset = spec.Premium ? catalog.ResolvePremiumPreset(key.Value) : catalog.ResolveDefaultPreset(key.Value);
            if (preset?.Items is null || !preset.Items.Any())
            {
                warnings.Add($"{context}: preset/weapon '{spec.Preset}' not found — line skipped (builder skips it too).");
                return;
            }
        }
        else if (spec.Mods is not null)   // mirrors BuildItemTree/PackSpecsIntoGrids: `mods: []` = bare item, no auto-preset
        {
            if (string.IsNullOrWhiteSpace(spec.Tpl))
            {
                warnings.Add($"{context}: 'mods' without a root 'tpl' — line skipped (builder skips it too).");
                return;
            }

            hasManualTree = true;
            AddSimple(spec.Tpl!, count, context, items, warnings);
            AddModTree(spec.Mods, count, context, items, warnings);
        }
        else if (!string.IsNullOrWhiteSpace(spec.Tpl))
        {
            var tpl = TryMongoId(spec.Tpl!, context, warnings);
            if (tpl is null)
            {
                return;
            }

            // auto-completion parity with InventoryBuilder: equipped bare tpl resolves the default
            // preset (BuildItemTree); stash/contents resolve the stash preset (PackSpecsIntoGrids)
            preset = equipped ? catalog.ResolveDefaultPreset(tpl.Value) : catalog.ResolveStashPreset(tpl.Value);
            if (preset?.Items is null || !preset.Items.Any())
            {
                preset = null;
                AddSimple(spec.Tpl!, count, context, items, warnings);
            }
        }
        else
        {
            warnings.Add($"{context}: item without tpl/preset/mods — line skipped (builder skips it too).");
            return;
        }

        if (preset is not null)
        {
            AddPresetItems(preset, count, context, items);
        }

        AddAmmo(spec, preset, hasManualTree, equipped, count, context, items, warnings);

        foreach (var content in spec.Contents ?? [])
        {
            // CR2-EP-01: one copy of the contents goes into EACH unit of this line (builder parity).
            AddSpec(content, equipped: false, "contents", items, warnings, multiplier: count);
        }
    }

    /// <summary>One line per tpl of the resolved preset (stacks like preset cartridges respected).</summary>
    private void AddPresetItems(Preset preset, int count, string context, List<LoadoutCostEntry> items)
    {
        // aggregate by tpl so a 10-part preset stays readable in the breakdown
        var qtyByTpl = new Dictionary<MongoId, double>();
        foreach (var item in preset.Items!)
        {
            var qty = item.Upd?.StackObjectsCount ?? 1;
            qtyByTpl[item.Template] = qtyByTpl.GetValueOrDefault(item.Template) + qty;
        }

        foreach (var (tpl, qty) in qtyByTpl)
        {
            AddLine(tpl, qty * count, context, items);
        }
    }

    private void AddModTree(List<ModSpec> mods, int count, string context, List<LoadoutCostEntry> items, List<string> warnings)
    {
        foreach (var mod in mods)
        {
            if (string.IsNullOrWhiteSpace(mod.Tpl))
            {
                warnings.Add($"{context}: mod without tpl — sub-line skipped.");
                continue;
            }

            AddSimple(mod.Tpl!, count, context, items, warnings);
            if (mod.Mods is { Count: > 0 })
            {
                AddModTree(mod.Mods, count, context, items, warnings);
            }
        }
    }

    /// <summary>
    ///     Magazine fill + chambered round (mirrors InventoryBuilder.LoadAmmo): loadedMag fills the
    ///     preset's magazine to capacity (skipped when the preset already ships cartridges — CR-01-03,
    ///     those are already counted as preset items); chambered adds 1 round only when the weapon
    ///     template declares a chamber slot (CR-01-02 / CR-EP-08). The builder only loads ammo into an
    ///     ASSEMBLED tree: any equipped line, or a stash/contents line with a resolved preset / manual
    ///     mods — a plain stash item never gets ammo, so it is not costed either (CR-EP-01 parity).
    /// </summary>
    private void AddAmmo(ItemSpec spec, Preset? preset, bool hasManualTree, bool equipped, int count, string context, List<LoadoutCostEntry> items, List<string> warnings)
    {
        if (!spec.LoadedMag && !spec.Chambered)
        {
            return;
        }

        if (!equipped && preset is null && !hasManualTree)
        {
            warnings.Add($"{context}: loadedMag/chambered on a plain stash item — the builder spawns it without ammo; no rounds counted.");
            return;
        }

        if (string.IsNullOrWhiteSpace(spec.Ammo))
        {
            warnings.Add($"{context}: loadedMag/chambered without 'ammo' — no rounds counted (builder skips it too).");
            return;
        }

        var ammoTpl = TryMongoId(spec.Ammo!, context, warnings);
        if (ammoTpl is null)
        {
            return;
        }

        double rounds = 0;
        if (spec.LoadedMag)
        {
            // magazine lives in the resolved preset OR, for manual trees, anywhere in spec.Mods
            // (slotId mod_magazine — recursive, mirroring the builder's whole-tree scan; CR-EP-08b)
            var presetMag = preset?.Items?.FirstOrDefault(i => string.Equals(i.SlotId, "mod_magazine", StringComparison.Ordinal));
            var manualMag = FindMagazine(spec.Mods);
            var magTplRaw = presetMag?.Template.ToString() ?? manualMag?.Tpl;

            if (magTplRaw is null)
            {
                warnings.Add($"{context}: loadedMag without a 'mod_magazine' in the resolved tree — magazine fill not counted.");
            }
            else if (presetMag is not null
                     && preset!.Items!.Any(i => i.ParentId == presetMag.Id.ToString() && string.Equals(i.SlotId, "cartridges", StringComparison.Ordinal)))
            {
                // preset already ships cartridges — counted in AddPresetItems, do not double count (CR-01-03)
            }
            else if (presetMag is null
                     && manualMag?.Mods?.Any(m => string.Equals(m.SlotId, "cartridges", StringComparison.Ordinal)) == true)
            {
                // CR2-EP-06: manual tree declares an explicit 'cartridges' node — the builder skips the
                // fill (LoadAmmo's CR-01-03 check covers manual trees too); the node itself is already
                // counted by AddModTree, so no fill is added here either.
            }
            else
            {
                var magTpl = TryMongoId(magTplRaw, context, warnings);
                if (magTpl is not null)
                {
                    // capacity: _props.Cartridges[0]._max_count (ref: TemplateItem.cs:560/1733)
                    var magTemplate = itemHelper.GetItem(magTpl.Value);
                    var capacity = magTemplate is { Key: true, Value: not null }
                        ? magTemplate.Value.Properties?.Cartridges?.FirstOrDefault()?.MaxCount ?? 0
                        : 0;
                    if (capacity <= 0)
                    {
                        warnings.Add($"{context}: magazine '{magTplRaw}' has no cartridge capacity — fill not counted.");
                    }

                    rounds += capacity;
                }
            }
        }

        if (spec.Chambered)
        {
            // CR-EP-08a: the builder only chambers when the root template declares _props.Chambers
            // (CR-01-02) — root = preset root for preset lines, spec.Tpl for manual/simple trees.
            var rootTplRaw = preset is not null
                ? (preset.Items!.FirstOrDefault(i => string.IsNullOrEmpty(i.ParentId)) ?? preset.Items!.First()).Template.ToString()
                : spec.Tpl;
            string? chamberSlot = null;
            var rootTpl = string.IsNullOrWhiteSpace(rootTplRaw) ? null : TryMongoId(rootTplRaw!, context, warnings);
            if (rootTpl is not null)
            {
                var weapon = itemHelper.GetItem(rootTpl.Value);
                chamberSlot = weapon is { Key: true, Value: not null }
                    ? weapon.Value.Properties?.Chambers?.FirstOrDefault()?.Name
                    : null;
            }

            if (string.IsNullOrEmpty(chamberSlot))
            {
                warnings.Add($"{context}: chambered but the weapon has no chamber slot — round not counted (builder skips it too).");
            }
            else
            {
                rounds += 1;
            }
        }

        if (rounds > 0)
        {
            AddLine(ammoTpl.Value, rounds * count, "ammo", items);
        }
    }

    /// <summary>Depth-first search for the magazine node anywhere in a manual mod tree (CR-EP-08b).</summary>
    private static ModSpec? FindMagazine(List<ModSpec>? mods)
    {
        foreach (var mod in mods ?? [])
        {
            if (string.Equals(mod.SlotId, "mod_magazine", StringComparison.Ordinal))
            {
                return mod;
            }

            if (FindMagazine(mod.Mods) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    private void AddSimple(string tplRaw, int count, string context, List<LoadoutCostEntry> items, List<string> warnings)
    {
        var tpl = TryMongoId(tplRaw, context, warnings);
        if (tpl is not null)
        {
            AddLine(tpl.Value, count, context, items);
        }
    }

    private void AddLine(MongoId tpl, double qty, string context, List<LoadoutCostEntry> items)
    {
        var (unitPrice, source) = catalog.GetPrice(tpl);
        items.Add(new LoadoutCostEntry
        {
            Tpl = tpl.ToString(),
            Name = catalog.GetItemName(tpl),
            Context = context,
            Qty = qty,
            UnitPrice = unitPrice,
            PriceSource = source,
            Subtotal = Math.Round(unitPrice * qty),
            MissingPrice = string.Equals(source, "missing", StringComparison.Ordinal),
        });
    }

    // ── Stash capacity (item 028) ────────────────────────────────────────────

    /// <summary>
    ///     Dry-run of the stash packing (item 028): resolves the base edition's stash grid the same way
    ///     <c>InventoryBuilder.Apply</c> does at registration time (profile template character →
    ///     <c>Inventory.Stash</c> id → that item's template → <c>_props.Grids</c>), materializes
    ///     <c>loadout.stash</c> exactly like <c>PackSpecsIntoGrids</c> (CR-EP-01: explicit preset /
    ///     manual mod tree / stash-preset auto-completion, StackMaxSize stack-split, assembled size via
    ///     <c>InventoryHelper.GetItemSize</c>) and first-fit places every root through
    ///     <see cref="GridPacker"/>. Contents/ammo do not change the stash footprint (they live inside
    ///     the placed item). WARNING-grade by design: the loader already skips overflow items with a
    ///     log line — this never blocks a save.
    /// </summary>
    public StashCapacityResult CheckStashCapacity(ClassDefinition def)
    {
        var warnings = new List<string>();
        var unplaced = new List<string>();
        var baseKey = (string.IsNullOrWhiteSpace(def.BaseEdition)
            ? ClassRegistrar.DefaultBaseEdition
            : def.BaseEdition!).Trim();

        var grids = ResolveBaseStashGrids(baseKey, warnings);
        var packers = grids
            .Select(g => new GridPacker(g.Properties?.CellsH ?? 0, g.Properties?.CellsV ?? 0))
            .Where(p => p.Width > 0 && p.Height > 0)
            .ToList();
        var totalCells = packers.Sum(p => p.Width * p.Height);
        if (totalCells == 0)
        {
            return new StashCapacityResult
            {
                GridResolved = false,
                BaseEdition = baseKey,
                GridSizes = [],
                Unplaced = [],
                Warnings = warnings,
            };
        }

        var usedCells = 0;
        var placedCount = 0;

        foreach (var spec in def.Loadout?.Stash ?? [])
        {
            try
            {
                var count = Math.Max(1, spec.Count);

                // CR-EP-01 parity: PackSpecsIntoGrids honors the FULL spec semantics — explicit preset
                // (premium-aware), manual mod tree, then bare tpl (stash-preset auto-completion / simple).
                // Contents/ammo never change the stash footprint (they live INSIDE the placed item).
                List<Item>? tree = null;
                MongoId? labelTpl = null;

                if (!string.IsNullOrWhiteSpace(spec.Preset))
                {
                    var key = TryMongoId(spec.Preset!, "stash", warnings);
                    if (key is null)
                    {
                        continue;
                    }

                    var explicitPreset = spec.Premium ? catalog.ResolvePremiumPreset(key.Value) : catalog.ResolveDefaultPreset(key.Value);
                    if (explicitPreset?.Items is null || !explicitPreset.Items.Any())
                    {
                        warnings.Add($"stash: preset/weapon '{spec.Preset}' not found — not packed (the builder skips it too).");
                        continue;
                    }

                    tree = explicitPreset.Items.ToList();
                }
                else if (spec.Mods is not null)
                {
                    if (string.IsNullOrWhiteSpace(spec.Tpl))
                    {
                        warnings.Add("stash: 'mods' without a root 'tpl' — not packed (the builder skips it too).");
                        continue;
                    }

                    tree = BuildProbeTree(spec.Tpl!, spec.Mods);   // manual tree — ExtraSize of mods counts
                }
                else if (string.IsNullOrWhiteSpace(spec.Tpl))
                {
                    warnings.Add("stash: line without tpl/preset/mods — not packed (the builder skips it too).");
                    continue;
                }
                else
                {
                    var bareTpl = TryMongoId(spec.Tpl!, "stash", warnings);
                    if (bareTpl is null)
                    {
                        continue;
                    }

                    labelTpl = bareTpl;
                    var preset = catalog.ResolveStashPreset(bareTpl.Value);   // same resolution PackSpecsIntoGrids uses
                    if (preset?.Items is not null && preset.Items.Any())
                    {
                        tree = preset.Items.ToList();
                    }
                }

                if (tree is not null)
                {
                    // Assembled tree, one placement per unit — mirrors InventoryBuilder.PackSpecsIntoGrids.
                    // The builder also injects a minimum optic on optic-less stash weapons
                    // (EnsureMinimumOptic) before measuring; optics carry no ExtraSize, so the footprint
                    // is identical — that step is intentionally not mirrored here.
                    var root = tree.FirstOrDefault(i => string.IsNullOrEmpty(i.ParentId)) ?? tree[0];
                    labelTpl ??= root.Template;
                    var (w, h) = inventoryHelper.GetItemSize(root.Template, root.Id, tree);   // ref: InventoryHelper.cs:609
                    for (var i = 0; i < count; i++)
                    {
                        if (TryPlace(packers, w, h))
                        {
                            placedCount++;
                            usedCells += w * h;
                        }
                        else
                        {
                            unplaced.Add($"{catalog.GetItemName(labelTpl.Value)} ×{count - i}");
                            break;
                        }
                    }

                    continue;
                }

                // Simple item, stack-aware — mirrors the StackMaxSize split in PackSpecsIntoGrids.
                var tpl = labelTpl!.Value;
                var template = itemHelper.GetItem(tpl);   // ref: ItemHelper.cs:494
                var stackMax = (template is { Key: true, Value: not null } ? template.Value.Properties?.StackMaxSize : null) ?? 1;
                var probe = new Item { Id = new MongoId(), Template = tpl };
                var (sw, sh) = inventoryHelper.GetItemSize(tpl, probe.Id, new List<Item> { probe });
                var remaining = count;
                while (remaining > 0)
                {
                    var thisStack = stackMax > 1 ? Math.Min(remaining, stackMax) : 1;
                    if (TryPlace(packers, sw, sh))
                    {
                        placedCount++;
                        usedCells += sw * sh;
                        remaining -= thisStack;
                    }
                    else
                    {
                        unplaced.Add($"{catalog.GetItemName(tpl)} ×{remaining}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"stash: '{spec.Tpl ?? spec.Preset}' failed to measure ({ex.Message}) — not packed (the builder skips it too).");
            }
        }

        return new StashCapacityResult
        {
            GridResolved = true,
            BaseEdition = baseKey,
            GridSizes = packers.Select(p => $"{p.Width}×{p.Height}").ToList(),
            TotalCells = totalCells,
            UsedCells = usedCells,
            OccupancyPercent = Math.Round(100.0 * usedCells / totalCells, 1),
            PlacedCount = placedCount,
            Unplaced = unplaced,
            Warnings = warnings,
        };
    }

    /// <summary>First-fit across the stash grids (rotation handled inside <see cref="GridPacker.Place"/>).</summary>
    private static bool TryPlace(List<GridPacker> packers, int w, int h) =>
        packers.Any(p => p.Place(w, h) is not null);

    /// <summary>
    ///     Measurement probe for a manual mod tree (CR-EP-01 parity with InventoryBuilder.BuildManualTree):
    ///     root + recursive mods with real ParentId/SlotId links so GetItemSize sees the mods' ExtraSize.
    ///     Malformed tpls throw — the caller's per-spec try/catch degrades that to a warning.
    /// </summary>
    private static List<Item> BuildProbeTree(string rootTpl, List<ModSpec> mods)
    {
        var list = new List<Item> { new() { Id = new MongoId(), Template = new MongoId(rootTpl) } };
        AddProbeMods(list, list[0].Id.ToString(), mods);
        return list;
    }

    private static void AddProbeMods(List<Item> list, string parentId, List<ModSpec>? mods)
    {
        foreach (var mod in mods ?? [])
        {
            if (string.IsNullOrWhiteSpace(mod.Tpl) || string.IsNullOrWhiteSpace(mod.SlotId))
            {
                continue;   // the builder ignores these nodes with a warning — no footprint either
            }

            var child = new Item { Id = new MongoId(), Template = new MongoId(mod.Tpl!), ParentId = parentId, SlotId = mod.SlotId };
            list.Add(child);
            AddProbeMods(list, child.Id.ToString(), mod.Mods);
        }
    }

    /// <summary>
    ///     Stash grids of a base edition's profile template — the exact chain InventoryBuilder.Apply walks:
    ///     <c>character.Inventory.Stash</c> (container item id) → that item in <c>Inventory.Items</c> →
    ///     its template's <c>_props.Grids</c>. Every unresolvable step degrades to a warning + empty list
    ///     (the UI shows "capacity not checked", never an error).
    /// </summary>
    private List<Grid> ResolveBaseStashGrids(string baseKey, List<string> warnings)
    {
        if (!databaseService.GetProfileTemplates().TryGetValue(baseKey, out var sides) || sides is null)   // ref: DatabaseService.cs:141
        {
            warnings.Add($"Base edition '{baseKey}' not found among the profile templates — capacity not checked.");
            return [];
        }

        var character = sides.Usec?.Character ?? sides.Bear?.Character;
        var inv = character?.Inventory;
        if (inv?.Items is null || inv.Stash is null)
        {
            warnings.Add($"Base edition '{baseKey}' has no Inventory.Stash/Items — capacity not checked.");
            return [];
        }

        var stashItem = inv.Items.FirstOrDefault(i => i.Id == inv.Stash.Value);
        if (stashItem is null)
        {
            warnings.Add($"Base edition '{baseKey}': stash container item not found in Inventory.Items — capacity not checked.");
            return [];
        }

        // The packer starts from EMPTY grids — exactly like the builder. A base edition that already
        // ships stash items (e.g. "Standard") can therefore overlap; flag it so the author knows.
        var preExisting = inv.Items.Count(i => i.ParentId == inv.Stash.Value.ToString());
        if (preExisting > 0)
        {
            warnings.Add($"Base edition '{baseKey}' already ships {preExisting} stash item(s) — the packer ignores them (mirrors the builder), overlap is possible.");
        }

        var template = itemHelper.GetItem(stashItem.Template);
        List<Grid> stashGrids = template is { Key: true, Value: not null }
            ? template.Value.Properties?.Grids?.ToList() ?? []
            : [];
        if (stashGrids.Count == 0)
        {
            warnings.Add($"Base edition '{baseKey}': stash template '{stashItem.Template}' declares no grids — capacity not checked.");
        }

        return stashGrids;
    }

    /// <summary>
    ///     Item 038: dimensões (colunas × linhas) da grade-raiz do stash da baseEdition, para o editor
    ///     desenhar a grade 2D. Premissa: o stash do player é single-grid — usamos a 1ª grade. Retorna
    ///     null se a base/stash/grade não resolver (o editor cai num default e mostra aviso).
    /// </summary>
    public (int Width, int Height)? GetStashGridSize(string baseKey)
    {
        var grids = ResolveBaseStashGrids(baseKey, []);
        var props = grids.FirstOrDefault()?.Properties;
        if (props is null)
        {
            return null;
        }

        var w = props.CellsH ?? 0;
        var h = props.CellsV ?? 0;
        return w > 0 && h > 0 ? (w, h) : null;
    }

    /// <summary>Malformed ids throw in <c>new MongoId</c> — degrade to a warning, never break the breakdown.</summary>
    private static MongoId? TryMongoId(string raw, string context, List<string> warnings)
    {
        try
        {
            return new MongoId(raw);
        }
        catch (Exception ex)
        {
            warnings.Add($"{context}: malformed id '{raw}' ({ex.Message}) — line skipped.");
            return null;
        }
    }
}
