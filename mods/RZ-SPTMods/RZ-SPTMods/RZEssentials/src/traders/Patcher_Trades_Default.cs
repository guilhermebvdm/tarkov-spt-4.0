// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Traders;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// DEFAULT TRADES PATCHER  (RagfairCallbacks - 3)
//
// Must run at RagfairCallbacks - 3 so the slate is clean before injection patchers run at - 2.
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 3)]
public class Patcher_Trades_Default(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    AssortUtilities assortUtilities,
    RzeLogger log
) : IOnLoad
{
    private readonly DefaultTradesConfig _defaultTradesConfig = configLoader.Load<DefaultTradesConfig>();

    public Task OnLoad()
    {
        if (_defaultTradesConfig.ClearAllDefaultTrades)
        {
            ClearDefaultAssorts();
            return Task.CompletedTask;
        }

        ApplyBlacklist();
        ReplaceBarterTrades(assortUtilities);
        ApplyPriceMultipliers();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ClearDefaultAssorts
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void ClearDefaultAssorts()
    {
        foreach ((MongoId id, Trader trader) in databaseService.GetTraders())
        {
            if (id == SPTarkov.Server.Core.Models.Enums.Traders.FENCE)
                continue;

            trader.Assort = new TraderAssort {
                Items           = new List<Item>(),
                BarterScheme    = new Dictionary<MongoId, List<List<BarterScheme>>>(),
                LoyalLevelItems = new Dictionary<MongoId, int>(),
                NextResupply    = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
            };
        }

        log.Info(LogChannel.Traders, "DefaultTrades: all trader assorts cleared.");
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ApplyBlacklist
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void ApplyBlacklist()
    {
        if (_defaultTradesConfig.Blacklist.Count == 0)
            return;

        var blacklistTpls = _defaultTradesConfig.Blacklist.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var traders = databaseService.GetTraders();
        var removed = 0;

        foreach (var (_, trader) in traders)
        {
            var assort = trader.Assort;
            if (assort?.Items is null)
                continue;

            var toRemove = assort.Items
                .Where(i => i.ParentId == "hideout" && blacklistTpls.Contains(i.Template.ToString()))
                .ToList();

            foreach (var root in toRemove)
            {
                assort.Items.RemoveAll(i => i.Id == root.Id || i.ParentId == root.Id);
                assort.BarterScheme.Remove(root.Id);
                assort.LoyalLevelItems.Remove(root.Id);
                removed++;
            }
        }

        log.Info(LogChannel.Traders, $"DefaultTrades/Blacklist: {removed} item(s) removed from trader assorts.");
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ApplyPriceMultipliers
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void ApplyPriceMultipliers()
    {
        if (_defaultTradesConfig.PriceMultipliers.Count == 0)
            return;

        HashSet<string> currencyTpls = [
            ItemTpl.MONEY_ROUBLES,
            ItemTpl.MONEY_DOLLARS,
            ItemTpl.MONEY_EUROS,
        ];

        var traders = databaseService.GetTraders();
        var patched = 0;

        foreach (var (traderId, multiplier) in _defaultTradesConfig.PriceMultipliers)
        {
            if (multiplier == 1.0)
                continue;

            if (!traders.TryGetValue(traderId, out var trader))
            {
                log.Warning(LogChannel.Traders, $"DefaultTrades/PriceMultipliers: trader '{traderId}' not found, skipping.");
                continue;
            }

            var assort = trader.Assort;
            if (assort?.BarterScheme is null)
                continue;

            foreach (var schemes in assort.BarterScheme.Values)
            {
                foreach (var scheme in schemes)
                {
                    // Only touch schemes where every entry is a currency (pure cash offers)
                    if (!scheme.All(s => currencyTpls.Contains(s.Template.ToString())))
                        continue;

                    foreach (var entry in scheme)
                        entry.Count = Math.Max(1, Math.Round((entry.Count ?? 0) * multiplier));

                    patched++;
                }
            }
        }

        if (patched > 0)
            log.Info(LogChannel.Traders, $"DefaultTrades/PriceMultipliers: {patched} offer(s) repriced.");
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ReplaceBarterTrades
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void ReplaceBarterTrades(AssortUtilities assortUtilities)
    {
        if (_defaultTradesConfig.NoBarterTraders.Count == 0)
            return;

        var traders = databaseService.GetTraders();
        var handbook = databaseService.GetTables().Templates?.Handbook;

        if (handbook is null)
        {
            log.Warning(LogChannel.Traders, "DefaultTrades/NoBarterTrades: handbook is null, skipping.");
            return;
        }

        HashSet<string> currencyTpls = [
            ItemTpl.MONEY_ROUBLES,
            ItemTpl.MONEY_DOLLARS,
            ItemTpl.MONEY_EUROS,
        ];

        var handbookItems = handbook.Items.ToDictionary(e => e.Id.ToString(), e => (double)(e.Price ?? 0));
        var dollarPriceInRub = handbookItems.GetValueOrDefault(ItemTpl.MONEY_DOLLARS.ToString(), 0.0);
        var euroPriceInRub = handbookItems.GetValueOrDefault(ItemTpl.MONEY_EUROS.ToString(), 0.0);

        if (dollarPriceInRub <= 0)
            log.Warning(LogChannel.Traders, "DefaultTrades/NoBarterTrades: handbook price for USD is 0 or missing, USD conversion will fallback to roubles.");
        if (euroPriceInRub <= 0)
            log.Warning(LogChannel.Traders, "DefaultTrades/NoBarterTrades: handbook price for EUR is 0 or missing, EUR conversion will fallback to roubles.");

        var converted = 0;
        var skipped = 0;

        foreach (var (traderId, traderEntry) in _defaultTradesConfig.NoBarterTraders)
        {
            if (!traderEntry.Enabled)
                continue;

            if (!traders.TryGetValue(traderId, out var trader))
            {
                log.Warning(LogChannel.Traders, $"DefaultTrades/NoBarterTrades: trader '{traderId}' not found, skipping.");
                continue;
            }

            var excludedTpls = traderEntry.ExcludedBarterTpls.ToHashSet();
            var assort = trader.Assort;

            if (assort?.BarterScheme is null)
                continue;

            foreach (var (assortId, schemes) in assort.BarterScheme)
            {
                foreach (var scheme in schemes)
                {
                    if (scheme.All(s => currencyTpls.Contains(s.Template.ToString())))
                        continue;

                    if (excludedTpls.Count > 0 && scheme.All(s => excludedTpls.Contains(s.Template.ToString())))
                        continue;

                    var rootItem = assort.Items.FirstOrDefault(i => i.Id == assortId);
                    if (rootItem is null)
                    {
                        skipped++;
                        continue;
                    }

                    var priceRub = assortUtilities.GetTotalHandbookPrice(rootItem.Template, handbookItems);
                    if (priceRub <= 0)
                    {
                        skipped++;
                        log.Warning(LogChannel.Traders, $"DefaultTrades/NoBarterTrades: no handbook price for '{rootItem.Template}', skipping.");
                        continue;
                    }

                    MongoId currencyTpl;
                    double finalPrice;

                    switch (traderEntry.Currency)
                    {
                        case TradeCurrency.Usd when dollarPriceInRub > 0:
                            currencyTpl = ItemTpl.MONEY_DOLLARS;
                            finalPrice = Math.Max(1, Math.Round(priceRub / dollarPriceInRub));
                            break;

                        case TradeCurrency.Eur when euroPriceInRub > 0:
                            currencyTpl = ItemTpl.MONEY_EUROS;
                            finalPrice = Math.Max(1, Math.Round(priceRub / euroPriceInRub));
                            break;

                        default:
                            currencyTpl = ItemTpl.MONEY_ROUBLES;
                            finalPrice = priceRub;
                            break;
                    }

                    scheme.Clear();
                    scheme.Add(new BarterScheme {
                        Template = currencyTpl,
                        Count = finalPrice,
                    });
                    converted++;
                }
            }
        }

        if (converted > 0)
            log.Info(LogChannel.Traders, $"DefaultTrades/NoBarterTrades: {converted} barter(s) converted to cash, {skipped} skipped.");
    }
}
