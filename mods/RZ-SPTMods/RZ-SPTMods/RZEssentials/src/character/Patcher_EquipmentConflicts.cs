// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Character;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class EquipmentConflictsPatcher(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<EquipmentConflictsConfig>();
        if (!config.Enabled)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.MiscSettings, "Templates.Items is null, skipping.");
            return Task.CompletedTask;
        }

        var categoryMap = config.CategorySettings
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

        if (categoryMap.Count == 0 && config.ClearConflictingItems.Count == 0)
            return Task.CompletedTask;

        var excluded = config.ExcludedTpls.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Resolve category TPL sets once upfront — avoids rebuilding per item in the main loop
        List<(ConflictingItemsRule Rule, HashSet<string> CategoryTpls, HashSet<string> FromTplsSet)>? resolvedRules = null;
        if (config.ClearConflictingItems.Count > 0)
        {
            resolvedRules = config.ClearConflictingItems.Select(rule => (
                Rule: rule,
                CategoryTpls: rule.ToCategories.Count > 0
                    ? BuildCategoryTplSet(rule.ToCategories, items)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                FromTplsSet: new HashSet<string>(rule.FromTpls, StringComparer.OrdinalIgnoreCase)
            )).ToList();
        }

        var patched = 0;

        // Forward pass — Unblock flags + remove conflicts from FromTpls pointing to ToCategories
        foreach (var (mongoId, item) in items)
        {
            if (item.Properties is null)
                continue;

            // ExcludedTpls only applies to CategorySettings
            if (!excluded.Contains(mongoId.ToString()) && categoryMap.Count > 0)
            {
                var settings = FindMatchingCategorySettings(mongoId.ToString(), items, categoryMap);
                if (settings is not null)
                {
                    if (settings.UnblockHeadwear)
                        item.Properties.BlocksHeadwear = false;
                    if (settings.UnblockEarpiece)
                        item.Properties.BlocksEarpiece = false;
                    if (settings.UnblockFaceCover)
                        item.Properties.BlocksFaceCover = false;
                    if (settings.UnblockEyewear)
                        item.Properties.BlocksEyewear = false;

                    patched++;
                }
            }

            if (resolvedRules is not null)
                ApplyClearConflictingItems(mongoId.ToString(), item.Properties, resolvedRules);
        }

        // Reverse pass — remove conflicts from ToCategories items pointing back to FromTpls
        if (resolvedRules is not null)
            ApplyReverseConflictClearing(items, resolvedRules);

        log.Info(LogChannel.MiscSettings, $"{patched} item(s) patched.");
        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Forward pass : on FromTpls, remove conflicts pointing to ToCategories
    // ─────────────────────────────────────────────────────────────────────────

    private static void ApplyClearConflictingItems(
        string tpl,
        TemplateItemProperties props,
        List<(ConflictingItemsRule Rule, HashSet<string> CategoryTpls, HashSet<string> FromTplsSet)> resolvedRules)
    {
        if (props.ConflictingItems is null || props.ConflictingItems.Count == 0)
            return;

        foreach (var (_, categoryTpls, fromTplsSet) in resolvedRules)
        {
            if (fromTplsSet.Count > 0 && !fromTplsSet.Contains(tpl))
                continue;

            if (categoryTpls.Count == 0)
            {
                props.ConflictingItems = [];
                return;
            }

            var toRemove = props.ConflictingItems
                .Where(conflictTpl => categoryTpls.Contains(conflictTpl.ToString()))
                .ToList();

            foreach (var entry in toRemove)
                props.ConflictingItems.Remove(entry);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Reverse pass : on ToCategories items, remove conflicts pointing to FromTpls
    // ─────────────────────────────────────────────────────────────────────────

    private static void ApplyReverseConflictClearing(
        Dictionary<MongoId, TemplateItem> items,
        List<(ConflictingItemsRule Rule, HashSet<string> CategoryTpls, HashSet<string> FromTplsSet)> resolvedRules)
    {
        foreach (var (_, categoryTpls, fromTplsSet) in resolvedRules)
        {
            if (categoryTpls.Count == 0)
                continue;

            foreach (var tpl in categoryTpls)
            {
                if (!items.TryGetValue(tpl, out var item) || item.Properties?.ConflictingItems is null)
                    continue;

                if (fromTplsSet.Count == 0)
                {
                    item.Properties.ConflictingItems = [];
                    continue;
                }

                var toRemove = item.Properties.ConflictingItems
                    .Where(conflictTpl => fromTplsSet.Contains(conflictTpl.ToString()))
                    .ToList();

                foreach (var entry in toRemove)
                    item.Properties.ConflictingItems.Remove(entry);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static HashSet<string> BuildCategoryTplSet(
        List<string> categoryIds,
        Dictionary<MongoId, TemplateItem> items)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var categorySet = new HashSet<string>(categoryIds, StringComparer.OrdinalIgnoreCase);

        foreach (var (mongoId, item) in items)
        {
            var current = item.Parent.ToString();
            while (!string.IsNullOrEmpty(current))
            {
                if (categorySet.Contains(current))
                {
                    result.Add(mongoId.ToString());
                    break;
                }
                if (!items.TryGetValue(current, out var parent))
                    break;
                current = parent.Parent.ToString();
            }
        }

        return result;
    }

    private static EquipmentBlockSettings? FindMatchingCategorySettings(
        string tpl,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, EquipmentBlockSettings> categoryMap)
    {
        if (!items.TryGetValue(tpl, out var item))
            return null;

        var current = item.Parent.ToString();

        while (!string.IsNullOrEmpty(current))
        {
            if (categoryMap.TryGetValue(current, out var settings))
                return settings;

            if (!items.TryGetValue(current, out var parent))
                break;

            current = parent.Parent.ToString();
        }

        return null;
    }
}
