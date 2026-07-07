using HarmonyLib;
using SPTarkov.Server.Core.Helpers;                  // TradeHelper
using SPTarkov.Server.Core.Models.Eft.Common;        // PmcData
using SPTarkov.Server.Core.Models.Eft.Trade;         // ProcessSellTradeRequestData
using SPTarkov.Server.Core.Models.Enums;             // CurrencyType

namespace TRLItemsManagement.Pricing;

/// <summary>
///     Server backstop — Harmony Prefix on <c>TradeHelper.SellItem</c> (the sell-to-trader path). The
///     vanilla method credits the player with <c>sellRequest.Price</c> exactly AS SENT BY THE CLIENT
///     (PaymentService.GiveProfileMoney round-trips it through the TRADER's own currency — no
///     cross-currency conversion); the server never recomputes a buyback price itself.
///     <para>
///     If EVERY sold item has a matching (traderId, tpl) entry in
///     <see cref="TraderPriceOnLoad.BuyOverrides"/>, denominated in the trader's own currency, this
///     Prefix rewrites <c>sellRequest.Price</c> to the override total BEFORE the original method runs —
///     so the money credited matches what the client-side patch
///     (<c>modded/Client/Patches/GetUserItemPricePatch.cs</c>, <c>TraderClass.GetUserItemPrice</c>)
///     displayed on the sell screen.
///     </para>
///     <para>
///     Fallback (untouched, vanilla <c>sellRequest.Price</c>): any sold item without an override, a
///     currency mismatch, or trader == Fence (dynamic assort, excluded at config-load time — see
///     <see cref="TraderPriceOnLoad.BuyOverrides"/>). The client sends ONE aggregate price for the
///     whole request, so a partial override (some items covered, some not) has no reliable per-item
///     vanilla breakdown to fall back to — safer to leave the whole request vanilla.
///     </para>
///     Entry-time only: <see cref="Prefix"/> reads <c>profileWithItemsToSell.Inventory.Items</c> BEFORE
///     the original method removes them — reading after would find nothing. Harmony is required here:
///     <c>SellItem</c> is not virtual, so DI's <c>typeOverride</c> registration cannot intercept it.
///     <para>
///     Renamed from <c>TraderBuyPricePatch.cs</c> (TRLTraderPrices) — the client half's display patch
///     (<c>Client/Patches/GetUserItemPricePatch.cs</c>) was also renamed, for the same reason: two files
///     with the identical name in different halves of the same mod invited confusion during the merge.
///     </para>
/// </summary>
[HarmonyPatch(typeof(TradeHelper), nameof(TradeHelper.SellItem))]
internal static class SellItemPatch
{
    [HarmonyPrefix]
    private static void Prefix(PmcData profileWithItemsToSell, ProcessSellTradeRequestData sellRequest)
    {
        try
        {
            var overrides = TraderPriceOnLoad.BuyOverrides;
            if (overrides is null || overrides.Count == 0
                || sellRequest?.Items is null || sellRequest.Items.Count == 0
                || profileWithItemsToSell?.Inventory?.Items is null)
            {
                return;
            }

            if (!overrides.TryGetValue(sellRequest.TransactionId, out var tplMap) || tplMap.Count == 0)
            {
                return; // no buy-override configured for this trader
            }

            var trader = TraderPriceOnLoad.Db?.GetTraders()?.GetValueOrDefault(sellRequest.TransactionId);
            if (trader?.Base?.Currency is not { } traderCurrency)
            {
                return;
            }

            double total = 0;

            foreach (var soldItem in sellRequest.Items)
            {
                var invItem = profileWithItemsToSell.Inventory.Items.FirstOrDefault(x => x.Id == soldItem.Id);
                if (invItem is null)
                {
                    // Let the original method's own "not found in inventory" error path run untouched.
                    return;
                }

                if (!tplMap.TryGetValue(invItem.Template, out var ovr))
                {
                    return; // at least one sold item lacks an override -> vanilla for the whole request
                }

                if (!Enum.TryParse<CurrencyType>(ovr.Currency?.Trim(), ignoreCase: true, out var ovrCurrency)
                    || ovrCurrency != traderCurrency)
                {
                    return; // override denominated in a different currency than the trader's native one
                }

                total += ovr.Count * (soldItem.Count ?? 1);
            }

            sellRequest.Price = total;
            TraderPriceOnLoad.Log?.Debug(
                $"[TRLItemsManagement] buy price backstop: trader {sellRequest.TransactionId} -> {total} ({traderCurrency}) for {sellRequest.Items.Count} item(s).");
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] buy price backstop failed: " + ex);
        }
    }
}
