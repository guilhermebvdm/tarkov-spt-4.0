using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Common;    // MongoId
using SPTarkov.Server.Core.Models.Enums;     // CurrencyType, Money
using SPTarkov.Server.Core.Services;         // DatabaseService
using TRLItemsManagement.Pricing;            // TraderPriceOnLoad.BuyOverrides

namespace TRLItemsManagement.Api;

/// <summary>
///     Read-only diagnostic endpoint: confirms flea/trader/ban prices took effect at THIS boot without
///     needing to launch the game client. Everything sell-side and flea-side is genuinely computed at
///     boot (see <see cref="Pricing.SellPriceApplier"/> — mutates the live <c>Trader.Assort</c> — and
///     SPT's own <c>PostDbLoadService</c>/<c>RagfairPriceService</c> — mutate <c>Templates.Prices</c>), so
///     reading the SAME live <see cref="DatabaseService"/> objects those steps already wrote to is a
///     faithful post-boot check, not a guess.
///     <para>
///     Buyback is the one exception: it's applied by a Harmony Prefix on <c>TradeHelper.SellItem</c>,
///     which only runs during an actual sell transaction — there is no boot-time mutation to inspect. The
///     <c>traderBuy</c> section below is a DRY RUN of that Prefix's own currency-match logic (same
///     dict, same check), which confirms the DATA and LOGIC are correct; it does not prove the Harmony
///     patch itself fires (that still requires a real sale).
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class DebugController(
    DatabaseService databaseService,
    ModPathsService modPaths,
    SptDataPathsService sptPaths) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);

    [HttpGet("debug/verify-price")]
    public IActionResult VerifyPrice([FromQuery] string? tpl)
    {
        if (tpl is not { } tplStr || !TplPattern.IsMatch(tplStr))
        {
            return BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id, ?tpl=...)" });
        }

        var tplId = new MongoId(tplStr);

        // Our own catalog (data/items.json) already has the THEORETICAL/expected values the pipeline
        // computed (bonus, floor, ceiling, intended effective price, ban state) — reused here rather
        // than re-deriving the compensation formula a third time (Node pipeline + FleaPriceController's
        // write-time validation already have it).
        var catalogPath = Path.Combine(modPaths.DataDir, "items.json");
        var catalogItem = System.IO.File.Exists(catalogPath)
            ? (JsonNode.Parse(System.IO.File.ReadAllText(catalogPath)) as JsonObject)?[tplStr] as JsonObject
            : null;
        var catalogSpt = catalogItem?["spt"] as JsonObject;

        return Ok(new
        {
            ok = true,
            tpl = tplStr,
            name = catalogItem?["shortName"]?.GetValue<string?>() ?? catalogItem?["name"]?.GetValue<string?>(),
            ban = BuildBanCheck(tplId, catalogSpt),
            flea = BuildFleaCheck(tplId, catalogSpt),
            traderSell = BuildTraderSellRows(tplId),
            traderBuy = BuildTraderBuyRows(tplId),
        });
    }

    private object BuildBanCheck(MongoId tplId, JsonObject? catalogSpt)
    {
        var liveItems = databaseService.GetItems();
        bool? bannedLive = liveItems.TryGetValue(tplId, out var template)
            ? template.Properties?.CanSellOnRagfair == false
            : null;
        var bannedExpected = catalogSpt?["fleaBanned"]?.GetValue<bool?>();

        var ragfairRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.RagfairConfigPath)) as JsonObject;
        var enableBsgList = ragfairRoot?["dynamic"]?["blacklist"]?["enableBsgList"]?.GetValue<bool?>() ?? true;

        return new
        {
            bannedLive,
            bannedExpected,
            match = bannedLive == bannedExpected,
            enableBsgList,
            note = enableBsgList
                ? (string?)null
                : "enableBsgList is false in ragfair.json - CanSellOnRagfair is IGNORED by SPT regardless of its value here.",
        };
    }

    private object BuildFleaCheck(MongoId tplId, JsonObject? catalogSpt)
    {
        // Templates.Prices[tpl] post-boot = override (or vanilla base) + handbook bonus — BEFORE the
        // floor/ceiling clamp and the ±20% random variance RagfairPriceService applies at offer-generation
        // time. floor/ceiling below are clamp BOUNDS already computed by the pipeline, not a
        // re-derivation of the compensation formula — applying them here is display-only.
        var livePrices = databaseService.GetPrices();
        double? liveBase = livePrices.TryGetValue(tplId, out var p) ? p : null;

        var floor = catalogSpt?["fleaFloor"]?.GetValue<double?>();
        var ceiling = catalogSpt?["fleaCeiling"]?.GetValue<double?>();

        var liveClamped = liveBase;
        if (liveClamped is { } lc)
        {
            if (floor is { } f && lc < f)
            {
                liveClamped = f;
            }

            if (ceiling is { } c && liveClamped > c)
            {
                liveClamped = c;
            }
        }

        var expectedEffectivePrice = catalogSpt?["effectiveFleaPrice"]?.GetValue<double?>();

        return new
        {
            livePricesBase = liveBase,
            liveClampedApprox = liveClamped,
            floor,
            ceiling,
            expectedEffectivePrice,
            match = liveClamped != null && expectedEffectivePrice != null && Math.Abs(liveClamped.Value - expectedEffectivePrice.Value) < 1,
            note = "liveClampedApprox approximates the real flea offer price - SPT still applies +/-20% random variance on top of this at offer-generation time, so exact equality isn't expected, only closeness.",
        };
    }

    private List<object> BuildTraderSellRows(MongoId tplId)
    {
        var rows = new List<object>();
        var traders = databaseService.GetTraders();

        foreach (var (traderId, trader) in traders)
        {
            var assort = trader.Assort;
            if (assort?.Items is null || assort.BarterScheme is null)
            {
                continue;
            }

            foreach (var item in assort.Items)
            {
                if (item.Template != tplId)
                {
                    continue;
                }

                if (!assort.BarterScheme.TryGetValue(item.Id, out var tiers) || tiers is null || tiers.Count == 0
                    || tiers[0] is null || tiers[0].Count == 0)
                {
                    continue;
                }

                var bs = tiers[0][0];
                rows.Add(new
                {
                    traderId = traderId.ToString(),
                    traderName = trader.Base?.Nickname,
                    nativePrice = bs.Count,
                    currency = MoneyTplToLabel(bs.Template),
                });
            }
        }

        return rows;
    }

    private List<object> BuildTraderBuyRows(MongoId tplId)
    {
        var rows = new List<object>();
        var buyOverrides = TraderPriceOnLoad.BuyOverrides;
        if (buyOverrides is null)
        {
            return rows;
        }

        var traders = databaseService.GetTraders();

        foreach (var (traderId, tplMap) in buyOverrides)
        {
            if (!tplMap.TryGetValue(tplId, out var ovr))
            {
                continue;
            }

            traders.TryGetValue(traderId, out var trader);
            var traderCurrency = trader?.Base?.Currency;

            // Same currency-match guard as SellItemPatch.Prefix — dry run, not a real transaction.
            var currencyMatches = Enum.TryParse<CurrencyType>(ovr.Currency?.Trim(), ignoreCase: true, out var ovrCurrency)
                && ovrCurrency == traderCurrency;

            rows.Add(new
            {
                traderId = traderId.ToString(),
                traderName = trader?.Base?.Nickname,
                configuredCount = ovr.Count,
                configuredCurrency = ovr.Currency,
                traderNativeCurrency = traderCurrency?.ToString(),
                wouldApply = currencyMatches,
                dryRunCreditedAmount = currencyMatches ? ovr.Count : (double?)null,
            });
        }

        return rows;
    }

    private static string MoneyTplToLabel(MongoId tpl)
    {
        if (tpl == Money.ROUBLES)
        {
            return "RUB";
        }

        if (tpl == Money.DOLLARS)
        {
            return "USD";
        }

        if (tpl == Money.EUROS)
        {
            return "EUR";
        }

        if (tpl == Money.GP)
        {
            return "GP";
        }

        return tpl.ToString();
    }
}
