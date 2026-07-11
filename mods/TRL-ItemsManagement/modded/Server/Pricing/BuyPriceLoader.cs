using SPTarkov.Server.Core.Models.Common;   // MongoId
using SPTarkov.Server.Core.Models.Utils;    // ISptLogger
using SPTarkov.Server.Core.Utils;           // JsonUtil

namespace TRLItemsManagement.Pricing;

/// <summary>
///     Buyback (trader → player sell price) override loader. Parses <c>config/buy-overrides.json</c>
///     (traderId → tpl → override) into MongoId-keyed dictionaries so <see cref="SellItemPatch"/> (Harmony
///     backstop on <c>TradeHelper.SellItem</c>) and <see cref="TraderBuyOverridesRouter"/> (feed for the
///     client display patch) do zero string parsing per request. Ported from TRLTraderPrices B-3
///     (<c>ParseBuyOverrides</c> in TRLTraderPricesMod.cs) — unlike the sell side, the buy side never
///     validates against the live trader DB (no <c>TryGetValue(traders, ...)</c> check), so
///     <see cref="TraderOverrideConfigParser.TryResolveTrader{TCtx}"/> here always resolves.
/// </summary>
internal static class BuyPriceLoader
{
    internal static Dictionary<MongoId, Dictionary<MongoId, TraderOverride>> Load(
        string configPath,
        JsonUtil jsonUtil,
        ISptLogger<TraderPriceOnLoad> logger)
    {
        var result = new Dictionary<MongoId, Dictionary<MongoId, TraderOverride>>();

        var summary = TraderOverrideConfigParser.Parse<bool>(
            configPath,
            jsonUtil,
            (MongoId _, out bool ctx) =>
            {
                ctx = true; // buy side never validates against the trader DB — always resolves
                return true;
            },
            (traderId, _, tpl, ovr) =>
            {
                if (!result.TryGetValue(traderId, out var tplMap))
                {
                    tplMap = new Dictionary<MongoId, TraderOverride>();
                    result[traderId] = tplMap;
                }

                tplMap[tpl] = ovr;
            });

        logger.Info(
            $"[TRLItemsManagement] buy-overrides.json: {summary.Entries} entr{(summary.Entries == 1 ? "y" : "ies")} parsed (badTrader {summary.BadTrader}, badTpl {summary.BadTpl}, fenceSkip {summary.FenceSkip}).");

        return result;
    }
}
