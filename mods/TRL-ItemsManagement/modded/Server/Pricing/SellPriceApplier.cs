using SPTarkov.Server.Core.Helpers;                  // PaymentHelper
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Enums;             // Money
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // TraderAssort
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Utils;                    // JsonUtil
using Item = SPTarkov.Server.Core.Models.Eft.Common.Tables.Item;

namespace TRLItemsManagement.Pricing;

/// <summary>
///     Overrides trader sell prices (the player's BUY cost) from a static config file.
///     Reads <c>config/overrides.json</c> mapping <c>traderId -> tpl -> { count, currency }</c> and rewrites
///     the first money-based barter tier of each matching assort entry. The count is stored in the offer's
///     NATIVE currency and applied DIRECTLY (no RUB conversion). The currency field identifies which money
///     tpl the count is denominated in, so a mismatched-currency assort entry is skipped instead of corrupted.
///     Ported from TRLTraderPrices v1.1.0 (TRLTraderPricesMod.cs) — logic unchanged, only the config
///     parsing/validation scaffold now goes through <see cref="TraderOverrideConfigParser"/>.
///
///     Timing: must run at RagfairCallbacks - 1 (see <see cref="TraderPriceOnLoad"/>), i.e. AFTER
///     trader-assort injection (mods @ PostDBModLoader 400000, custom traders @ TraderRegistration 500000)
///     and AFTER TraderController.Load @ TraderCallbacks 800000 (so the global traderPriceMultiplier is
///     applied before our override) — but BEFORE RagfairCallbacks 1000000. This is critical:
///     RagfairOfferGenerator.GenerateFleaOffersForTrader clones the LIVE trader.Assort at generation time,
///     so the flea-market trader offers only reflect our override if we mutate the assort BEFORE ragfair
///     generates them. The served assort is the live <see cref="DatabaseService.GetTraders"/> object,
///     cloned per request, so direct buy reflects it too.
///
///     Skips: Fence (dynamic assort, handled by the shared parser), barter (non-money first tier) offers,
///     unknown traders/tpls, mixed-requirement first tiers, and entries whose currency does not match the
///     override's currency.
/// </summary>
internal static class SellPriceApplier
{
    /// <summary>Max number of per-applied-entry sample lines emitted (the rest are summarised by count).</summary>
    private const int SampleCap = 5;

    private readonly record struct SellCtx(TraderAssort Assort, Dictionary<MongoId, Item> ItemsById);

    /// <summary>Maps an override currency code to its Money tpl. Returns null for unknown/empty (match any money entry).</summary>
    private static MongoId? CurrencyToMoneyTpl(string? currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            return null;
        }

        return currency.Trim().ToUpperInvariant() switch
        {
            "RUB" => (MongoId?)Money.ROUBLES,
            "USD" => Money.DOLLARS,
            "EUR" => Money.EUROS,
            "GP" => Money.GP,
            _ => null,
        };
    }

    internal static void Apply(
        string configPath,
        JsonUtil jsonUtil,
        DatabaseService databaseService,
        PaymentHelper paymentHelper,
        ISptLogger<TraderPriceOnLoad> logger)
    {
        var applied = 0;
        var tplNotSold = 0;           // valid tpl not sold by that trader (no matching assort entry)
        var barterSkip = 0;
        var currencyMismatchSkip = 0; // money entry exists but in a different currency than the override
        var mixedSkip = 0;            // first barter option is a multi-requirement combo (money + something)
        var samplesLogged = 0;

        var traders = databaseService.GetTraders();

        var summary = TraderOverrideConfigParser.Parse<SellCtx>(
            configPath,
            jsonUtil,
            (MongoId traderId, out SellCtx ctx) =>
            {
                if (!traders.TryGetValue(traderId, out var trader) || trader.Assort is null)
                {
                    ctx = default;
                    return false;
                }

                var assort = trader.Assort;

                // Build an Id -> root Item index ONCE per trader so the inner tpl loop is O(1) per
                // assort key instead of an O(items) FirstOrDefault scan.
                var itemsById = new Dictionary<MongoId, Item>();
                foreach (var item in assort.Items)
                {
                    itemsById[item.Id] = item;
                }

                ctx = new SellCtx(assort, itemsById);
                return true;
            },
            (traderId, ctx, tpl, ovr) =>
            {
                var assort = ctx.Assort;
                var itemsById = ctx.ItemsById;

                // Resolve the override currency to a Money tpl. null = unknown/empty -> match any money entry.
                var wantMoneyTpl = CurrencyToMoneyTpl(ovr.Currency);
                var currencyLabel = string.IsNullOrWhiteSpace(ovr.Currency) ? "?" : ovr.Currency!.Trim().ToUpperInvariant();

                var hit = false;

                // An override applies the SAME price to ALL assort entries (loyalty tiers) of this
                // tpl at this trader — intended "flat price per item per trader". Per-loyalty-level
                // pricing is not supported. BarterScheme.Keys is only read here (bs.Count is the sole
                // mutation), so iterating the live key collection is safe (no .ToList() snapshot needed).
                foreach (var assortId in assort.BarterScheme.Keys)
                {
                    if (!itemsById.TryGetValue(assortId, out var item) || item.Template != tpl)
                    {
                        continue;
                    }

                    var tiers = assort.BarterScheme[assortId];
                    if (tiers is null || tiers.Count == 0 || tiers[0] is null || tiers[0].Count == 0)
                    {
                        continue;
                    }

                    if (tiers[0].Count > 1)
                    {
                        // Mixed-requirement first option (money + something). Editing only [0][0]
                        // would produce an incoherent offer, so skip rather than silently corrupt it.
                        mixedSkip++;
                        logger.Debug(
                            $"[TRLItemsManagement] skipped mixed-requirement offer (trader {traderId} tpl {tpl}, {tiers[0].Count} requirements)");
                        continue;
                    }

                    var bs = tiers[0][0];
                    if (!paymentHelper.IsMoneyTpl(bs.Template))
                    {
                        barterSkip++;
                        continue;
                    }

                    // If the override names a known currency, only touch an entry already in that
                    // currency. Writing the native count onto a different-currency entry would corrupt it.
                    if (wantMoneyTpl is not null && bs.Template != wantMoneyTpl.Value)
                    {
                        currencyMismatchSkip++;
                        continue;
                    }

                    bs.Count = ovr.Count;
                    applied++;
                    hit = true;

                    if (samplesLogged < SampleCap)
                    {
                        logger.Debug($"[TRLItemsManagement] trader {traderId} tpl {tpl} -> {ovr.Count} {currencyLabel} ({bs.Template})");
                        samplesLogged++;
                    }
                }

                if (!hit)
                {
                    tplNotSold++;
                }
            });

        var shown = applied > SampleCap
            ? $" (showing first {SampleCap} of {applied} applied)"
            : string.Empty;

        logger.Info(
            $"[TRLItemsManagement] sell: applied {applied} entries{shown} (badTrader {summary.BadTrader}, badTpl {summary.BadTpl}, tplNotSold {tplNotSold}, barterSkip {barterSkip}, currencyMismatchSkip {currencyMismatchSkip}, fenceSkip {summary.FenceSkip}, mixedSkip {mixedSkip})");
    }
}
