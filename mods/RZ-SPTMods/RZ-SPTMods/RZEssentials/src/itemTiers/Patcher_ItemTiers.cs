// RemzDNB 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.ItemTiers;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 10)]
public class TiersPatcher(RzeLogger log, DatabaseService databaseService, ConfigLoader configLoader) : IOnLoad
{
    private readonly ItemTiersConfig _itemTiersConfig = configLoader.Load<ItemTiersConfig>();
    private readonly CategoryRulesConfig _categoryRules = configLoader.Load<CategoryRulesConfig>();
    private readonly PriceRulesConfig _priceRules = configLoader.Load<PriceRulesConfig>();

    public Task OnLoad()
    {
        if (!_itemTiersConfig.EnableItemTiers)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.ItemTiers, "Templates.Items is null — aborting.");
            return Task.CompletedTask;
        }

        var handbook = databaseService.GetTables().Templates?.Handbook;

        var tiers = _itemTiersConfig.GetTiers();
        if (tiers is null || tiers.Count == 0)
        {
            log.Error(LogChannel.ItemTiers, "itemTiersConfig.json is empty or missing — aborting.");
            return Task.CompletedTask;
        }

        var tplOverrides = LoadAndMergeOverrides();

        _itemTiersConfig.PriceThresholds.Sort((a, b) => b.MinPrice.CompareTo(a.MinPrice));

        var handbookPrices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (handbook?.Items is not null)
            foreach (var entry in handbook.Items)
                handbookPrices[entry.Id.ToString()] = (int)(entry.Price ?? 0);

        var defaultColor = tiers.GetValueOrDefault("Default", "default");

        var priceRuleCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (_priceRules?.Enabled == true)
            foreach (var cat in _priceRules.Categories)
                priceRuleCategories.Add(cat);

        // Pass 1 - Category assignments
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        foreach (var (mongoId, item) in items)
        {
            if (item.Properties is null) continue;

            if (item.Properties.QuestItem == true && tiers.ContainsKey("Quest"))
            {
                item.Properties.BackgroundColor = tiers["Quest"];
                continue;
            }

            var tier = _categoryRules?.Assignments is not null
                ? FindCategoryTier(mongoId.ToString(), items, _categoryRules.Assignments)
                : null;

            item.Properties.BackgroundColor = tier is not null && tiers.TryGetValue(tier, out var c) ? c : defaultColor;
        }

        // Pass 2 - Price-based tiering
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        if (priceRuleCategories.Count > 0 && _itemTiersConfig.PriceThresholds.Count > 0)
        {
            foreach (var (mongoId, item) in items)
            {
                if (item.Properties is null) continue;

                var tpl = mongoId.ToString();
                if (!handbookPrices.TryGetValue(tpl, out var price) || price <= 0) continue;

                var categoryId = FindMatchingCategory(tpl, items, priceRuleCategories);
                if (categoryId is null) continue;

                var tier = ResolvePriceTier(price, _itemTiersConfig.PriceThresholds) ?? "Default";
                if (!tiers.TryGetValue(tier, out var color)) continue;

                item.Properties.BackgroundColor = color;
            }
        }

        // Pass 3 - Per-TPL overrides
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        foreach (var (tpl, tierName) in tplOverrides)
        {
            if (!items.TryGetValue(tpl, out var item)) continue;
            if (item.Properties is null) continue;
            if (!tiers.TryGetValue(tierName, out var color)) continue;

            item.Properties.BackgroundColor = color;
        }

        // Pass 4 - Ammo boxes inherit tier from their cartridge
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        foreach (var (mongoId, item) in items)
        {
            if (item.Properties?.StackSlots is null) continue;

            var cartridgeTpl = item.Properties.StackSlots
                .FirstOrDefault(s => s.Name == "cartridges")?
                .Properties?.Filters?
                .FirstOrDefault()?
                .Filter?
                .FirstOrDefault()
                .ToString();

            if (cartridgeTpl is null) continue;
            if (!items.TryGetValue(cartridgeTpl, out var cartridge)) continue;
            if (cartridge.Properties?.BackgroundColor is null) continue;

            item.Properties.BackgroundColor = cartridge.Properties.BackgroundColor;
        }

        log.Info(LogChannel.ItemTiers, $"{tplOverrides.Count} TPL override(s) applied, theme '{_itemTiersConfig.Theme}'.");
        return Task.CompletedTask;
    }

    private static string? ResolvePriceTier(int price, List<PriceThreshold> thresholds)
    {
        foreach (var threshold in thresholds)
            if (price >= threshold.MinPrice)
                return threshold.Tier;
        return null;
    }

    private static string? FindCategoryTier(
        string tpl,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> assignments)
    {
        if (!items.TryGetValue(tpl, out var item))
            return null;

        var current = item.Parent.ToString();
        while (!string.IsNullOrEmpty(current))
        {
            if (assignments.TryGetValue(current, out var tier))
                return tier;
            if (!items.TryGetValue(current, out var parent))
                break;
            current = parent.Parent.ToString();
        }
        return null;
    }

    private static string? FindMatchingCategory(
        string tpl,
        Dictionary<MongoId, TemplateItem> items,
        HashSet<string> categories)
    {
        if (!items.TryGetValue(tpl, out var item))
            return null;

        var current = item.Parent.ToString();
        while (!string.IsNullOrEmpty(current))
        {
            if (categories.Contains(current))
                return current;
            if (!items.TryGetValue(current, out var parent))
                break;
            current = parent.Parent.ToString();
        }
        return null;
    }

    private Dictionary<string, string> LoadAndMergeOverrides()
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var files = configLoader.LoadAll<OverrideFile>(
            "itemTiers", exclude: [ItemTiersConfig.FileName, CategoryRulesConfig.FileName, PriceRulesConfig.FileName]
        );

        foreach (var file in files)
        {
            if (!file.Enabled) continue;
            foreach (var (tpl, tier) in file.Overrides)
            {
                if (merged.ContainsKey(tpl))
                    log.Warning(LogChannel.ItemTiers, $"Duplicate TPL override '{tpl}' : last file wins.");
                merged[tpl] = tier;
            }
        }

        return merged;
    }
}
