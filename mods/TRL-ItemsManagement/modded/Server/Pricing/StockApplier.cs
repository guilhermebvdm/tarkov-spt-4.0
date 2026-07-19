using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Eft.Common.Tables; // Upd
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Utils;                    // JsonUtil

namespace TRLItemsManagement.Pricing;

/// <summary>
///     B-6: overrides trader assort STOCK (<c>Item.Upd.StackObjectsCount</c>) and per-cycle BUY LIMIT
///     (<c>Item.Upd.BuyRestrictionMax</c>) from <c>config/stock-overrides.json</c>
///     (<c>traderId → tpl → {stock, buyLimit}</c>).
///     <para>
///     Boot mutation of the live <see cref="DatabaseService.GetTraders"/> assort — NO Harmony patch. In
///     SPT 4.0 <c>TraderAssortHelper.ResetExpiredTrader</c> refreshes a trader by cloning the LIVE
///     <c>Assort.Items</c> (confirmed against the deployed DLL — the 3.x pristine-snapshot store was
///     dropped), so a boot mutation is carried forward on every refresh, not reverted. Runs at the
///     <c>RagfairCallbacks - 1</c> window (see <see cref="TraderPriceOnLoad"/>), after custom-trader assort
///     injection and before ragfair generates trader→flea offers, so the mirrored flea offer picks up the
///     capped stock too.
///     </para>
///     <para>
///     <b>Semantics.</b> <c>stock</c> is a hard lifetime ceiling that erodes with purchases and does NOT
///     restock per cycle; <c>buyLimit</c> is the per-cycle allowance that DOES reset each refresh
///     (<c>BuyRestrictionCurrent</c> is zeroed by <c>TraderController.Update</c>). When a <c>stock</c> cap
///     is applied, <c>UnlimitedCount</c> is also set <c>false</c> so the client "∞" flag matches the
///     now-finite stock (the server never reads <c>UnlimitedCount</c> — it's display-only — but the
///     trader→flea mirror re-syncs the offer stack from the live assort).
///     </para>
///     Skips: Fence (dynamic assort), malformed/unknown traderId/tpl, and a tpl the trader doesn't sell.
/// </summary>
internal static class StockApplier
{
    internal static void Apply(
        string configPath,
        JsonUtil jsonUtil,
        DatabaseService databaseService,
        ISptLogger<TraderPriceOnLoad> logger)
    {
        var raw = jsonUtil.DeserializeFromFile<Dictionary<string, Dictionary<string, StockOverride>>>(configPath);
        if (raw is null || raw.Count == 0)
        {
            return;
        }

        var traders = databaseService.GetTraders();
        int stockApplied = 0, limitApplied = 0, tplNotSold = 0, badTrader = 0, badTpl = 0;

        foreach (var (traderIdStr, tplMap) in raw)
        {
            if (!TplValidation.IsHex24(traderIdStr))
            {
                badTrader++;
                continue;
            }

            var traderId = new MongoId(traderIdStr);
            if (traderId == TraderOverrideConfigParser.FenceId)
            {
                continue; // Fence: dynamic assort — a stock cap here is meaningless
            }

            if (tplMap is null || !traders.TryGetValue(traderId, out var trader) || trader.Assort?.Items is null)
            {
                badTrader++;
                continue;
            }

            foreach (var (tplStr, ovr) in tplMap)
            {
                if (ovr is null || !TplValidation.IsHex24(tplStr))
                {
                    badTpl++;
                    continue;
                }

                var tpl = new MongoId(tplStr);
                var hit = false;

                // Every ROOT sellable assort entry of this tpl (SlotId "hideout"; children are mods) —
                // a tpl can appear across loyalty tiers. Flat stock per tpl per trader, same all-tiers
                // model SellPriceApplier uses.
                foreach (var item in trader.Assort.Items)
                {
                    if (item.SlotId != "hideout" || item.Template != tpl)
                    {
                        continue;
                    }

                    item.Upd ??= new Upd();

                    if (ovr.Stock is { } s)
                    {
                        item.Upd.StackObjectsCount = s;
                        item.Upd.UnlimitedCount = false; // a finite cap must override the client "∞" flag
                        stockApplied++;
                    }

                    if (ovr.BuyLimit is { } bl)
                    {
                        item.Upd.BuyRestrictionMax = bl;
                        limitApplied++;
                    }

                    hit = true;
                }

                if (!hit)
                {
                    tplNotSold++;
                }
            }
        }

        logger.Info(
            $"[TRLItemsManagement] stock: {stockApplied} stock + {limitApplied} buy-limit entr(ies) applied (badTrader {badTrader}, badTpl {badTpl}, tplNotSold {tplNotSold}).");
    }
}
