// Patcher_ItemStats.cs — RemzDNB 2026

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.ItemStats;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class ItemPatcher(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    private static readonly Dictionary<string, PropertyInfo> _propCache =
        typeof(TemplateItemProperties)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    public Task OnLoad()
    {
        var merged = LoadAndMergeConfigs();

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.ItemStats, "Templates.Items is null.");
            return Task.CompletedTask;
        }

        var patched = 0;

        // Pass 1 - Category overrides
        if (merged.CategoryOverrides.Count > 0)
        {
            foreach (var (mongoId, item) in items)
            {
                if (item.Properties is null) continue;

                var tpl = mongoId.ToString();
                var matchedCategory = FindMatchingCategory(tpl, items, merged.CategoryOverrides);
                if (matchedCategory is null) continue;

                var patchedAny = false;
                patchedAny |= ApplyOps(tpl, item.Properties, matchedCategory.Ops, category: true);
                patchedAny |= ApplyGridOverrides(tpl, item.Properties, matchedCategory, category: true);

                if (patchedAny) patched++;
            }
        }

        // Pass 2 - TPL overrides
        foreach (var (tpl, ov) in merged.Overrides)
        {
            if (!items.TryGetValue(tpl, out var item))
            {
                log.Warning(LogChannel.ItemStats, $"TPL '{tpl}' not found, skipping.");
                continue;
            }

            if (item.Properties is null)
            {
                log.Warning(LogChannel.ItemStats, $"TPL '{tpl}' has no properties, skipping.");
                continue;
            }

            var patchedAny = false;
            patchedAny |= ApplyOps(tpl, item.Properties, ov.Ops, category: false);
            patchedAny |= ApplyGridOverrides(tpl, item.Properties, ov, category: false);

            if (patchedAny) patched++;
        }

        if (patched > 0)
            log.Info(LogChannel.ItemStats, $"{patched} item(s) patched.");

        return Task.CompletedTask;
    }

    private ItemStatsConfig LoadAndMergeConfigs()
    {
        var configs = configLoader.LoadAll<ItemStatsConfig>("itemStats", exclude: ["masterConfig.json"]);
        var merged = new ItemStatsConfig();

        foreach (var config in configs)
        {
            if (!config.Enabled) continue;

            foreach (var (key, ov) in config.CategoryOverrides)
            {
                if (merged.CategoryOverrides.ContainsKey(key))
                    log.Warning(LogChannel.ItemStats, $"Duplicate CategoryOverride key '{key}' : last file wins.");
                merged.CategoryOverrides[key] = ov;
            }

            foreach (var (key, ov) in config.Overrides)
            {
                if (merged.Overrides.ContainsKey(key))
                    log.Warning(LogChannel.ItemStats, $"Duplicate Override key '{key}' : last file wins.");
                merged.Overrides[key] = ov;
            }
        }

        return merged;
    }

    private static ItemOverride? FindMatchingCategory(
        string tpl,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, ItemOverride> categoryOverrides)
    {
        if (!items.TryGetValue(tpl, out var item)) return null;

        var current = item.Parent.ToString();
        while (!string.IsNullOrEmpty(current))
        {
            if (categoryOverrides.TryGetValue(current, out var ov)) return ov;
            if (!items.TryGetValue(current, out var parent)) break;
            current = parent.Parent.ToString();
        }

        return null;
    }

    private bool ApplyOps(
        string tpl,
        TemplateItemProperties props,
        Dictionary<string, StatOperation>? ops,
        bool category)
    {
        if (ops is null || ops.Count == 0) return false;

        var applied = false;

        foreach (var (key, op) in ops)
        {
            if (!_propCache.TryGetValue(key, out var propInfo))
            {
                if (!category)
                    log.Warning(LogChannel.ItemStats, $"'{tpl}': unknown property '{key}' : skipped.");
                continue;
            }

            try
            {
                var current = propInfo.GetValue(props);
                var result = ApplyOperation(current, op, propInfo.PropertyType);

                if (result is null)
                {
                    if (!category)
                        log.Warning(LogChannel.ItemStats, $"'{tpl}': could not apply op '{op.Op}' on '{key}' : skipped.");
                    continue;
                }

                propInfo.SetValue(props, result);
                applied = true;
            }
            catch (Exception ex)
            {
                if (!category)
                    log.Warning(LogChannel.ItemStats, $"'{tpl}': failed to apply op on '{key}': {ex.Message}");
            }
        }

        return applied;
    }

    private static object? ApplyOperation(object? current, StatOperation op, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (op.Op == "set")
        {
            return underlying switch
            {
                _ when underlying == typeof(double)  => op.Value,
                _ when underlying == typeof(float)   => (float)op.Value,
                _ when underlying == typeof(int)     => (int)op.Value,
                _ when underlying == typeof(long)    => (long)op.Value,
                _ when underlying == typeof(bool)    => op.Value != 0,
                _ => null,
            };
        }

        // add / multiply require a numeric current value
        if (current is null) return null;

        double currentDouble;
        try { currentDouble = Convert.ToDouble(current); }
        catch { return null; }

        var resultDouble = op.Op switch
        {
            "add"      => currentDouble + op.Value,
            "multiply" => currentDouble * op.Value,
            _ => (double?)null,
        };

        if (resultDouble is null) return null;

        return underlying switch
        {
            _ when underlying == typeof(double)  => resultDouble.Value,
            _ when underlying == typeof(float)   => (float)resultDouble.Value,
            _ when underlying == typeof(int)     => (int)Math.Round(resultDouble.Value),
            _ when underlying == typeof(long)    => (long)Math.Round(resultDouble.Value),
            _ => null,
        };
    }

    private bool ApplyGridOverrides(
        string tpl,
        TemplateItemProperties props,
        ItemOverride ov,
        bool category)
    {
        if (ov.Grids is null || ov.Grids.Count == 0) return false;

        var grids = props.Grids?.ToList();
        if (grids is null || grids.Count == 0)
        {
            if (!category)
                log.Warning(LogChannel.ItemStats, $"'{tpl}': no grids found — grid overrides skipped.");
            return false;
        }

        var result = new List<Grid>();

        foreach (var (indexStr, gridOv) in ov.Grids)
        {
            if (!int.TryParse(indexStr, out var index) || index < 0 || index >= grids.Count)
            {
                if (!category)
                    log.Warning(LogChannel.ItemStats, $"'{tpl}': invalid grid index '{indexStr}' : skipped.");
                continue;
            }

            var grid = grids[index];
            if (grid.Properties is null)
            {
                if (!category)
                    log.Warning(LogChannel.ItemStats, $"'{tpl}': grid[{index}] has no properties : skipped.");
                continue;
            }

            if (gridOv.CellsH.HasValue) grid.Properties.CellsH = gridOv.CellsH.Value;
            if (gridOv.CellsV.HasValue) grid.Properties.CellsV = gridOv.CellsV.Value;
            if (gridOv.MaxWeight.HasValue) grid.Properties.MaxWeight = gridOv.MaxWeight.Value;

            result.Add(grid);
        }

        props.Grids = result;
        return result.Count > 0;
    }
}
