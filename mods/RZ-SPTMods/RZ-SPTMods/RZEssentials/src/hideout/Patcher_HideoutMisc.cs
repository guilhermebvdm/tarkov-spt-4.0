// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using RZEssentials._Shared;

namespace RZEssentials.Hideout;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class Patcher_HideoutMisc(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    private readonly HideoutMiscConfig _hideoutMiscConfig = configLoader.Load<HideoutMiscConfig>();

    public Task OnLoad()
    {
        PatchFoundInRaid();
        UnlockCustomizations();

        return Task.CompletedTask;
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // FIR
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void PatchFoundInRaid()
    {
        if (!_hideoutMiscConfig.RequireFoundInRaid.HasValue)
            return;

        var areas = databaseService.GetTables().Hideout?.Areas;
        if (areas is null)
            return;

        foreach (var area in areas)
        foreach (var (_, stage) in area.Stages ?? [])
        foreach (var req in stage.Requirements ?? [])
        {
            if (req.Type != "Item")
                continue;

            req.IsSpawnedInSession = _hideoutMiscConfig.RequireFoundInRaid;
        }

        log.Info(LogChannel.Hideout, $"Hideout's FIR item requirements patched.");
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Hideout customizations
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void UnlockCustomizations()
    {
        if (_hideoutMiscConfig.UnlockHideoutCustomizations is null || _hideoutMiscConfig.UnlockHideoutCustomizations.Count <= 0)
            return;

        var storage = databaseService.GetTemplates().CustomisationStorage;

        // Build categoryId : customisationType for enabled entries only.
        var categoryTypeMap = _hideoutMiscConfig.UnlockHideoutCustomizations
            .Where(kvp => kvp.Value && HideoutAreasConfig.HideoutCategories.TryGetValue(kvp.Key, out _))
            .ToDictionary(
                kvp => HideoutAreasConfig.HideoutCategories[kvp.Key].CategoryId,
                kvp => HideoutAreasConfig.HideoutCategories[kvp.Key].CustomisationType
            );

        if (categoryTypeMap.Count == 0)
            return;

        var customizations = databaseService.GetTemplates().Customization;
        var storageIds = storage.Select(s => s.Id).ToHashSet();
        var added = 0;

        foreach (var (id, item) in customizations)
        {
            if (storageIds.Contains(id))
                continue;

            var type = FindCategoryType(item, customizations, categoryTypeMap);
            if (type is null)
                continue;

            storage.Add(new CustomisationStorage
            {
                Id = id,
                Source = CustomisationSource.UNLOCKED_IN_GAME,
                Type = type,
            });
            added++;
        }

        log.Info(LogChannel.Hideout, $"{added} hideout customization(s) patched.");
    }

    private static string? FindCategoryType(
        CustomizationItem item,
        Dictionary<MongoId, CustomizationItem> all,
        Dictionary<string, string> categoryTypeMap)
    {
        var current = item.Parent;
        while (!string.IsNullOrEmpty(current))
        {
            if (categoryTypeMap.TryGetValue(current, out var type))
                return type;

            if (!all.TryGetValue(current, out var parent))
                break;

            current = parent.Parent;
        }
        return null;
    }
}
