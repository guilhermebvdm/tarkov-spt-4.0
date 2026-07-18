using HarmonyLib;
using SPTarkov.Server.Core.Helpers;          // TraderHelper
using SPTarkov.Server.Core.Models.Common;    // MongoId

namespace TRLItemsManagement.Pricing;

/// <summary>
///     B-5 backstop — Harmony Postfix on <c>TraderHelper.GetHighestSellToTraderPrice(MongoId tpl)</c>, the
///     single seam that computes the SPT flea trader-price floor (handbook × trader buyback, loyalty 0).
///     For a tpl whitelisted in <see cref="FleaFloorOverrideStore"/>, this LOWERS the returned floor to
///     the configured value via <c>Math.Min</c> — it can never RAISE a floor, so a mis-set entry can't
///     break vanilla pricing. Untouched tpls keep their exact vanilla floor.
///     <para>
///     This one method feeds BOTH floor code paths — offer-time
///     (<c>RagfairPriceService.GetDynamicItemPrice</c>) and the boot base-price floor
///     (<c>RagfairPriceService.ReplaceFleaBasePrices</c>) — plus weapon-preset pricing, so a single
///     Postfix covers every place the floor is applied.
///     </para>
///     <para>
///     <c>GetHighestSellToTraderPrice</c> is public, non-virtual, instance — DI's <c>typeOverride</c>
///     can't intercept it, so Harmony is required (same rationale as <see cref="SellItemPatch"/>). The
///     vanilla result is computed (and cached in <c>HighestTraderPriceItems</c>) FIRST; the Postfix only
///     rewrites the return value, so the cache retains the true vanilla number and the override is
///     re-applied on every call (an O(1) dictionary lookup, no hot-path cost).
///     </para>
/// </summary>
[HarmonyPatch(typeof(TraderHelper), nameof(TraderHelper.GetHighestSellToTraderPrice))]
internal static class FleaFloorOverridePatch
{
    [HarmonyPostfix]
    private static void Postfix(MongoId tpl, ref double __result)
    {
        try
        {
            if (FleaFloorOverrideStore.Map is { } map && map.TryGetValue(tpl, out var floor))
            {
                __result = Math.Min(__result, floor);
            }
        }
        catch (Exception ex)
        {
            TraderPriceOnLoad.Log?.Error("[TRLItemsManagement] flea-floor override patch failed: " + ex);
        }
    }
}
