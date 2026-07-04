using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                       // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;                  // PaymentHelper
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Enums;             // Money
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Utils;                    // FileUtil, JsonUtil
using System.Text.Json.Serialization;                // JsonPropertyName
using Item = SPTarkov.Server.Core.Models.Eft.Common.Tables.Item;

namespace TRLTraderPrices;

/// <summary>
///     Overrides trader sell prices (the player's BUY cost) from a static config file.
///     Reads <c>config/overrides.json</c> mapping <c>traderId -> tpl -> { count, currency }</c> and rewrites
///     the first money-based barter tier of each matching assort entry. The count is stored in the offer's
///     NATIVE currency and applied DIRECTLY (no RUB conversion). The currency field identifies which money
///     tpl the count is denominated in, so a mismatched-currency assort entry is skipped instead of corrupted.
///
///     Timing: runs at RagfairCallbacks - 1, i.e. AFTER trader-assort injection (mods @ PostDBModLoader
///     400000, custom traders @ TraderRegistration 500000) and AFTER TraderController.Load @ TraderCallbacks
///     800000 (so the global traderPriceMultiplier is applied before our override) — but BEFORE
///     RagfairCallbacks 1000000. This is critical: RagfairOfferGenerator.GenerateFleaOffersForTrader clones
///     the LIVE trader.Assort at generation time (RagfairOfferGenerator.cs:532), so the flea-market trader
///     offers only reflect our override if we mutate the assort BEFORE ragfair generates them. (Running at
///     PostSptModLoader+1 = 1100001, after ragfair, left the flea showing base prices while the direct
///     trader buy showed the override — the bug this fixes.) The served assort is the live
///     <see cref="DatabaseService.GetTraders"/> object, cloned per request, so direct buy reflects it too.
///
///     Skips: Fence (dynamic assort), barter (non-money first tier) offers, unknown traders/tpls, mixed-
///     requirement first tiers, and entries whose currency does not match the override's currency.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 1)]
public class TRLTraderPricesMod(
    DatabaseService databaseService,
    PaymentHelper paymentHelper,
    FileUtil fileUtil,
    JsonUtil jsonUtil,
    ISptLogger<TRLTraderPricesMod> logger
) : IOnLoad
{
    private static readonly MongoId FenceId = new("579dc571d53a0658a154fbec");

    /// <summary>Max number of per-applied-entry sample lines emitted (the rest are summarised by count).</summary>
    private const int SampleCap = 5;

    /// <summary>
    ///     Per-tpl override: the native <c>Count</c> the game charges and the <c>Currency</c> it is
    ///     denominated in ("RUB" | "USD" | "EUR" | "GP"). Count is a double because BarterScheme.Count is
    ///     <c>double?</c>. Currency may be absent/empty, in which case the override applies to any money entry.
    ///     Shared shape between the sell (<c>overrides.json</c>) and buy (<c>buy-overrides.json</c>, B-3)
    ///     configs — <see cref="TraderBuyPricePatch"/> reuses this record.
    /// </summary>
    internal sealed record TraderOverride
    {
        // The overrides.json keys are lowercase ("count"/"currency"); SPT's JsonUtil binds
        // case-sensitively, so without these attributes Count deserialized to 0 (free items!)
        // and Currency to null. JsonPropertyName makes the lowercase JSON bind correctly.
        [JsonPropertyName("count")]    public double Count { get; init; }
        [JsonPropertyName("currency")] public string? Currency { get; init; }
    }

    /// <summary>
    ///     B-3: parsed <c>buy-overrides.json</c> (traderId → tpl → override), read by the Harmony backstop
    ///     <see cref="TraderBuyPricePatch"/> at <c>TradeHelper.SellItem</c> time. Pre-parsed into
    ///     <see cref="MongoId"/> keys at load (not raw strings) so the patch does zero string parsing on
    ///     the request path. Null until <see cref="OnLoad"/> runs; empty (not null) once loaded with no
    ///     entries.
    /// </summary>
    internal static Dictionary<MongoId, Dictionary<MongoId, TraderOverride>>? BuyOverrides;

    /// <summary>Exposed to <see cref="TraderBuyPricePatch"/> (a static Harmony patch has no DI) to resolve <c>trader.Base.Currency</c>.</summary>
    internal static DatabaseService? Db;

    /// <summary>Exposed to <see cref="TraderBuyPricePatch"/> for diagnostics (same pattern as OutfitPersistenceFixMod).</summary>
    internal static ISptLogger<TRLTraderPricesMod>? Log;

    private static bool _harmonyPatched;

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

    public Task OnLoad()
    {
        // The SPT loader invokes IOnLoad.OnLoad without its own try/catch, so any exception here
        // (e.g. JsonUtil.DeserializeFromFile throwing JsonException on a corrupt overrides.json — it
        // only returns null for a MISSING file) would abort server boot. Guard the whole body so a bad
        // config can never brick the server. Mirrors OutfitPersistenceFixMod's defensive try/catch.
        try
        {
            var configPath = Path.Combine(fileUtil.GetModPath("TRLTraderPrices"), "config", "overrides.json");

            // traderId -> (tpl -> { count, currency })
            var overrides = jsonUtil.DeserializeFromFile<Dictionary<string, Dictionary<string, TraderOverride>>>(configPath);
            if (overrides is null || overrides.Count == 0)
            {
                logger.Info("[TRLTraderPrices] No overrides configured (config/overrides.json empty or missing) — nothing to do.");
                return Task.CompletedTask;
            }

            var applied = 0;
            var badTrader = 0;            // malformed or unknown traderId (its tpls are short-circuited, not counted)
            var badTpl = 0;               // malformed tpl string
            var tplNotSold = 0;           // valid tpl not sold by that trader (no matching assort entry)
            var barterSkip = 0;
            var currencyMismatchSkip = 0; // money entry exists but in a different currency than the override
            var fenceSkip = 0;
            var mixedSkip = 0;            // first barter option is a multi-requirement combo (money + something)
            var samplesLogged = 0;

            var traders = databaseService.GetTraders();

            foreach (var (traderIdStr, tplMap) in overrides)
            {
                if (string.IsNullOrWhiteSpace(traderIdStr) || traderIdStr.Length != 24)
                {
                    // Malformed traderId: the all-zeros/empty MongoId does not throw, so guard explicitly.
                    badTrader++;
                    continue;
                }

                MongoId traderId;
                try
                {
                    traderId = new MongoId(traderIdStr);
                }
                catch
                {
                    badTrader++;
                    continue;
                }

                if (traderId == FenceId)
                {
                    fenceSkip++;
                    continue;
                }

                if (!traders.TryGetValue(traderId, out var trader) || trader.Assort is null)
                {
                    badTrader++;
                    continue;
                }

                var assort = trader.Assort;

                // Build an Id -> root Item index ONCE per trader so the inner tpl loop is O(1) per
                // assort key instead of an O(items) FirstOrDefault scan.
                var itemsById = new Dictionary<MongoId, Item>();
                foreach (var item in assort.Items)
                {
                    itemsById[item.Id] = item;
                }

                foreach (var (tplStr, ovr) in tplMap)
                {
                    if (ovr is null)
                    {
                        badTpl++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(tplStr) || tplStr.Length != 24)
                    {
                        // Malformed tpl: the all-zeros/empty MongoId does not throw, so guard explicitly.
                        badTpl++;
                        continue;
                    }

                    MongoId tpl;
                    try
                    {
                        tpl = new MongoId(tplStr);
                    }
                    catch
                    {
                        badTpl++;
                        continue;
                    }

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
                                $"[TRLTraderPrices] skipped mixed-requirement offer (trader {traderId} tpl {tpl}, {tiers[0].Count} requirements)");
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
                            logger.Debug($"[TRLTraderPrices] trader {traderId} tpl {tpl} -> {ovr.Count} {currencyLabel} ({bs.Template})");
                            samplesLogged++;
                        }
                    }

                    if (!hit)
                    {
                        tplNotSold++;
                    }
                }
            }

            var shown = applied > SampleCap
                ? $" (showing first {SampleCap} of {applied} applied)"
                : string.Empty;

            logger.Info(
                $"[TRLTraderPrices] applied {applied} entries{shown} (badTrader {badTrader}, badTpl {badTpl}, tplNotSold {tplNotSold}, barterSkip {barterSkip}, currencyMismatchSkip {currencyMismatchSkip}, fenceSkip {fenceSkip}, mixedSkip {mixedSkip})");
        }
        catch (Exception ex)
        {
            logger.Error("[TRLTraderPrices] failed to load/apply overrides: " + ex);
        }

        // B-3: expose Db/Log to the static Harmony patch (no DI in a HarmonyPatch class), patch
        // TradeHelper.SellItem once (idempotent — OnLoad can in principle run more than once), and
        // load the buy-side config. Kept in its own try/catch so a Harmony/config failure here can
        // never take down the sell-override logic above (or vice versa).
        Db = databaseService;
        Log = logger;

        if (!_harmonyPatched)
        {
            try
            {
                new Harmony("trltraderbuyprice.trl").PatchAll(typeof(TRLTraderPricesMod).Assembly);
                _harmonyPatched = true;
                logger.Info("[TRLTraderPrices] Harmony patch applied — buy price backstop (TradeHelper.SellItem).");
            }
            catch (Exception ex)
            {
                logger.Error("[TRLTraderPrices] failed to apply Harmony patch (buy price backstop): " + ex.Message);
            }
        }

        try
        {
            var buyConfigPath = Path.Combine(fileUtil.GetModPath("TRLTraderPrices"), "config", "buy-overrides.json");
            var rawBuyOverrides = jsonUtil.DeserializeFromFile<Dictionary<string, Dictionary<string, TraderOverride>>>(buyConfigPath);
            BuyOverrides = ParseBuyOverrides(rawBuyOverrides, logger);
        }
        catch (Exception ex)
        {
            logger.Error("[TRLTraderPrices] failed to load buy-overrides.json: " + ex.Message);
            BuyOverrides = new Dictionary<MongoId, Dictionary<MongoId, TraderOverride>>();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Validates + pre-parses the raw buy-overrides.json map into MongoId keys so
    ///     <see cref="TraderBuyPricePatch"/> does zero string parsing per sell request. Fence is dropped
    ///     at load (dynamic assort — a buyback override there would be meaningless/unsafe). Malformed
    ///     traderId/tpl entries are dropped and counted, mirroring the sell-side loader above.
    /// </summary>
    private static Dictionary<MongoId, Dictionary<MongoId, TraderOverride>> ParseBuyOverrides(
        Dictionary<string, Dictionary<string, TraderOverride>>? raw,
        ISptLogger<TRLTraderPricesMod> logger)
    {
        var result = new Dictionary<MongoId, Dictionary<MongoId, TraderOverride>>();
        if (raw is null || raw.Count == 0)
        {
            logger.Info("[TRLTraderPrices] No buy-overrides configured (config/buy-overrides.json empty or missing) — buyback backstop inert.");
            return result;
        }

        var badTrader = 0;
        var badTpl = 0;
        var entries = 0;

        foreach (var (traderIdStr, tplMap) in raw)
        {
            if (string.IsNullOrWhiteSpace(traderIdStr) || traderIdStr.Length != 24)
            {
                badTrader++;
                continue;
            }

            MongoId traderId;
            try
            {
                traderId = new MongoId(traderIdStr);
            }
            catch
            {
                badTrader++;
                continue;
            }

            if (traderId == FenceId || tplMap is null)
            {
                continue;
            }

            if (!result.TryGetValue(traderId, out var parsedTplMap))
            {
                parsedTplMap = new Dictionary<MongoId, TraderOverride>();
                result[traderId] = parsedTplMap;
            }

            foreach (var (tplStr, ovr) in tplMap)
            {
                if (ovr is null || string.IsNullOrWhiteSpace(tplStr) || tplStr.Length != 24)
                {
                    badTpl++;
                    continue;
                }

                MongoId tpl;
                try
                {
                    tpl = new MongoId(tplStr);
                }
                catch
                {
                    badTpl++;
                    continue;
                }

                parsedTplMap[tpl] = ovr;
                entries++;
            }
        }

        logger.Info($"[TRLTraderPrices] buy-overrides.json: {entries} entr{(entries == 1 ? "y" : "ies")} parsed (badTrader {badTrader}, badTpl {badTpl}).");
        return result;
    }
}
