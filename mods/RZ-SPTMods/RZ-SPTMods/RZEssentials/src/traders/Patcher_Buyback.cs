// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Traders;

[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 1)]
public class Patcher_Buyback(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    public Task OnLoad()
    {
        var buybackConfig = configLoader.Load<BuybackConfig>();
        if (buybackConfig.Rules.Count == 0)
            return Task.CompletedTask;

        var traders = databaseService.GetTraders();
        var itemTpls = databaseService.GetTables().Templates?.Items?.Keys.ToHashSet();

        foreach (var (traderId, rule) in buybackConfig.Rules)
        {
            if (!traders.TryGetValue(traderId, out var trader))
            {
                if (traderId != CustomTraderConfig.TraderId)
                    log.Warning(LogChannel.Traders, $"Buyback: trader '{traderId}' not found, skipping.");
                continue;
            }

            if (rule.Mode == BuybackMode.Default)
                continue;

            trader.Base.ItemsBuy = BuildItemBuyData(traderId, rule, itemTpls);
        }

        return Task.CompletedTask;
    }

    private ItemBuyData BuildItemBuyData(string traderId, BuybackRule rule, HashSet<MongoId>? itemTpls)
    {
        switch (rule.Mode)
        {
            case BuybackMode.Disabled:
                return new ItemBuyData { Category = new HashSet<MongoId>(), IdList = new HashSet<MongoId>() };

            case BuybackMode.Categories:
                var categories = rule.Categories.Select(c => new MongoId(c)).ToHashSet();
                return new ItemBuyData { Category = categories, IdList = new HashSet<MongoId>() };

            case BuybackMode.AllWithBlacklist:
                if (itemTpls is null)
                {
                    log.Warning(LogChannel.Traders, $"Buyback: {traderId}: Templates.Items is null, cannot build buyback whitelist.");
                    return new ItemBuyData { Category = new HashSet<MongoId>(), IdList = new HashSet<MongoId>() };
                }

                var blacklist = rule.Blacklist.Select(t => new MongoId(t)).ToHashSet();
                var idList = itemTpls.Where(tpl => !blacklist.Contains(tpl)).ToHashSet();
                return new ItemBuyData { Category = new HashSet<MongoId>(), IdList = idList };

            default:
                log.Warning(LogChannel.Traders, $"Buyback: {traderId}: unknown mode '{rule.Mode}', disabling.");
                return new ItemBuyData { Category = new HashSet<MongoId>(), IdList = new HashSet<MongoId>() };
        }
    }
}
