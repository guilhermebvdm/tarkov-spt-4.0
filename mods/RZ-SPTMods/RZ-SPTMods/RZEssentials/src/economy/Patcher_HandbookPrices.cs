// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Economy;

[Injectable(TypePriority = OnLoadOrder.HandbookCallbacks - 1)]
public class Patcher_HandbookPrices(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    private readonly HandbookPricesConfig _handbookPricesConfig = configLoader.Load<HandbookPricesConfig>();

    public Task OnLoad()
    {
        if (!_handbookPricesConfig.Enabled || _handbookPricesConfig.Prices.Count == 0)
            return Task.CompletedTask;

        var handbook = databaseService.GetTables().Templates?.Handbook;
        if (handbook is null)
        {
            log.Warning(LogChannel.Economy, "Handbook is null : skipping price overrides.");
            return Task.CompletedTask;
        }

        var patched = 0;
        foreach (var (tpl, price) in _handbookPricesConfig.Prices)
        {
            var entry = handbook.Items.FirstOrDefault(i => i.Id.ToString() == tpl);
            if (entry is null)
            {
                log.Warning(LogChannel.Economy, $"Handbook entry '{tpl}' not found : skipping.");
                continue;
            }

            entry.Price = price;
            patched++;
        }

        log.Info(LogChannel.Economy, $"{patched} handbook price(s) patched.");
        return Task.CompletedTask;
    }
}
