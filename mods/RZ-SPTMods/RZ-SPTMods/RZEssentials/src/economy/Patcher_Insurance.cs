// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Economy;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class InsurancePatcher(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    public Task OnLoad()
    {
        var insuranceConfig = configLoader.Load<InsuranceConfig>();

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Warning(LogChannel.Economy, "Templates.Items is null : skipping insurance patch.");
            return Task.CompletedTask;
        }

        // Disable all
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        if (insuranceConfig.DisableAll)
        {
            foreach (var (_, item) in items)
            {
                if (item.Properties is not null) {
                    item.Properties.InsuranceDisabled = true;
                }
            }

            log.Info(LogChannel.Economy, "Insurance disabled on all items.");
            return Task.CompletedTask;
        }

        // Category + TPL blacklist
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        var enabledCategories = insuranceConfig.CategoryBlacklist
            .Where(e => e.Enabled)
            .Select(e => e.CategoryId)
            .ToList();

        var blacklistedTpls = ItemUtils.BuildCategoryTplSet(enabledCategories, items);

        foreach (var tpl in insuranceConfig.TplBlacklist) {
            blacklistedTpls.Add(tpl);
        }

        if (blacklistedTpls.Count == 0) {
            return Task.CompletedTask;
        }

        foreach (var (tpl, item) in items)
        {
            if (item.Properties is not null && blacklistedTpls.Contains(tpl.ToString())) {
                item.Properties.InsuranceDisabled = true;
            }
        }

        log.Info(LogChannel.Economy, $"Insurance disabled on {blacklistedTpls.Count} item(s).");
        return Task.CompletedTask;
    }
}
