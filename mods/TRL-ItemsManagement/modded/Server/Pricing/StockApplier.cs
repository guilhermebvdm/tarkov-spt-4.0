using SPTarkov.Server.Core.Extensions;               // RemoveItemFromAssort (disable-sale)
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
        int stockApplied = 0, limitApplied = 0, loyaltyApplied = 0, disabledApplied = 0, tplNotSold = 0, badTrader = 0, badTpl = 0;
        int badLoyalty = 0, loyaltySkipped = 0; // CR-B-01/03: out-of-range LL rejected / trader has no loyalty dict

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

                // Disable-sale: remove every ROOT offer of this tpl (+ its children, barter & loyalty
                // entries) via SPT's own helper, so the trader stops listing it. Persists across refresh
                // (ResetExpiredTrader clones the mutated live Items). A removed offer has no stock to set.
                if (ovr.Disabled == true)
                {
                    var rootIds = new List<MongoId>();
                    foreach (var item in trader.Assort.Items)
                    {
                        if (item.SlotId == "hideout" && item.Template == tpl)
                        {
                            rootIds.Add(item.Id);
                        }
                    }

                    if (rootIds.Count == 0)
                    {
                        tplNotSold++;
                        continue;
                    }

                    foreach (var id in rootIds)
                    {
                        trader.Assort.RemoveItemFromAssort(id);
                    }

                    disabledApplied++;
                    continue;
                }

                var hit = false;

                // B-11: validate the loyalty override ONCE per tpl (not per root offer, so the counters
                // stay meaningful). The config is hand-editable and StockApplier trusts the raw file, so an
                // out-of-range level (e.g. 99) must be rejected here — it would otherwise gate the offer
                // behind a tier no player can reach, removing it for everyone (in the shop AND the flea
                // mirror) with no `disabled` flag, i.e. a silent phantom-bug. Mirrors StockController's 1-4
                // guard. A trader without a LoyalLevelItems dict (some custom traders) has nothing to retarget.
                int? applyLoyalty = null;
                if (ovr.LoyaltyLevel is { } rawLl)
                {
                    if (rawLl < 1 || rawLl > 4)
                    {
                        badLoyalty++;
                    }
                    else if (trader.Assort.LoyalLevelItems is null)
                    {
                        loyaltySkipped++;
                    }
                    else
                    {
                        applyLoyalty = rawLl;
                    }
                }

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

                    // B-11: retarget the loyalty tier that unlocks this root offer. LoyalLevelItems is keyed
                    // by the ROOT item.Id (a MongoId → int LL). The mutation persists across refresh cycles
                    // because TraderAssortHelper.ResetExpiredTrader only reassigns Assort.Items and never
                    // touches LoyalLevelItems — the cloned Items keep the same root Ids, so the keys match.
                    if (applyLoyalty is { } ll)
                    {
                        trader.Assort.LoyalLevelItems![item.Id] = ll;
                        loyaltyApplied++;
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
            $"[TRLItemsManagement] stock: {stockApplied} stock + {limitApplied} buy-limit + {loyaltyApplied} loyalty-level + {disabledApplied} disabled-sale entr(ies) applied (badTrader {badTrader}, badTpl {badTpl}, tplNotSold {tplNotSold}, badLoyalty {badLoyalty}, loyaltyNoDict {loyaltySkipped}).");
    }
}
