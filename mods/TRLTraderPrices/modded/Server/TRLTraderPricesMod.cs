using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                 // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;            // HandbookHelper, PaymentHelper
using SPTarkov.Server.Core.Models.Common;      // MongoId
using SPTarkov.Server.Core.Models.Enums;       // Money
using SPTarkov.Server.Core.Models.Utils;       // ISptLogger
using SPTarkov.Server.Core.Services;           // DatabaseService
using SPTarkov.Server.Core.Utils;              // FileUtil, JsonUtil

namespace TRLTraderPrices;

/// <summary>
///     Overrides trader sell prices (the player's BUY cost) from a static config file.
///     Reads <c>config/overrides.json</c> mapping <c>traderId -> tpl -> priceRUB</c> and rewrites the
///     first money-based barter tier of each matching assort entry. Prices given in RUB are converted to
///     the offer's currency via <see cref="HandbookHelper.FromRoubles"/>.
///
///     Runs LAST (after RagfairCallbacks / PostSptModLoader) so it mutates the live
///     <see cref="DatabaseService.GetTraders"/> dictionary after every other loader. The served assort is
///     cloned from this live object per request (TraderAssortHelper), so the change is visible to clients
///     with no pristine snapshot fighting it.
///
///     Skips: Fence (dynamic assort), barter (non-money first tier) offers, unknown traders/tpls, and any
///     conversion that yields a non-positive count.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class TRLTraderPricesMod(
    DatabaseService databaseService,
    HandbookHelper handbookHelper,
    PaymentHelper paymentHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    ISptLogger<TRLTraderPricesMod> logger
) : IOnLoad
{
    private static readonly MongoId FenceId = new("579dc571d53a0658a154fbec");

    public Task OnLoad()
    {
        var configPath = Path.Combine(fileUtil.GetModPath("TRLTraderPrices"), "config", "overrides.json");

        // traderId -> (tpl -> priceRUB)
        var overrides = jsonUtil.DeserializeFromFile<Dictionary<string, Dictionary<string, double>>>(configPath);
        if (overrides is null || overrides.Count == 0)
        {
            logger.Info("[TRLTraderPrices] No overrides configured (config/overrides.json empty or missing) — nothing to do.");
            return Task.CompletedTask;
        }

        var applied = 0;
        var notFound = 0;
        var barterSkip = 0;
        var convFail = 0;
        var fenceSkip = 0;
        var samplesLogged = 0;

        var traders = databaseService.GetTraders();

        foreach (var (traderIdStr, tplMap) in overrides)
        {
            MongoId traderId;
            try
            {
                traderId = new MongoId(traderIdStr);
            }
            catch
            {
                notFound++;
                continue;
            }

            if (traderId == FenceId)
            {
                fenceSkip++;
                continue;
            }

            if (!traders.TryGetValue(traderId, out var trader) || trader.Assort is null)
            {
                notFound++;
                continue;
            }

            var assort = trader.Assort;

            foreach (var (tplStr, priceRub) in tplMap)
            {
                MongoId tpl;
                try
                {
                    tpl = new MongoId(tplStr);
                }
                catch
                {
                    notFound++;
                    continue;
                }

                var hit = false;

                // Snapshot of keys to avoid mutation-during-iteration concerns.
                foreach (var assortId in assort.BarterScheme.Keys.ToList())
                {
                    var item = assort.Items.FirstOrDefault(i => i.Id == assortId);
                    if (item is null || item.Template != tpl)
                    {
                        continue;
                    }

                    var tiers = assort.BarterScheme[assortId];
                    if (tiers is null || tiers.Count == 0 || tiers[0] is null || tiers[0].Count == 0)
                    {
                        continue;
                    }

                    var bs = tiers[0][0];
                    if (!paymentHelper.IsMoneyTpl(bs.Template))
                    {
                        barterSkip++;
                        continue;
                    }

                    var newCount = bs.Template == Money.ROUBLES
                        ? priceRub
                        : handbookHelper.FromRoubles(priceRub, bs.Template);

                    if (newCount <= 0)
                    {
                        convFail++;
                        continue;
                    }

                    bs.Count = newCount;
                    applied++;
                    hit = true;

                    if (samplesLogged < 5)
                    {
                        logger.Info($"[TRLTraderPrices] trader {traderId} tpl {tpl} -> {newCount} ({bs.Template})");
                        samplesLogged++;
                    }
                }

                if (!hit)
                {
                    notFound++;
                }
            }
        }

        logger.Info(
            $"[TRLTraderPrices] applied {applied} entries (notFound {notFound}, barterSkip {barterSkip}, convFail {convFail}, fenceSkip {fenceSkip})");

        return Task.CompletedTask;
    }
}
