using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;     // RequestHandler

namespace TRLTraderPrices.Client;

/// <summary>
///     B-3 Rota B: lazy-loaded cache of <c>config/buy-overrides.json</c> (traderId → tpl → {count,
///     currency}), fetched once from the paired server mod's route (<c>/trltraderprices/buy-overrides</c>,
///     <c>modded/Server/TraderBuyPriceRouter.cs</c> — the SAME file the server-side Harmony backstop
///     <c>modded/Server/TraderBuyPricePatch.cs</c> reads), so the displayed sell-to-trader price and the
///     money actually credited always agree. Loading pattern mirrors CustomClasses'
///     <c>SkillMultipliers.cs</c> (lazy on first use — no profile-selection hook to depend on).
/// </summary>
internal static class BuyPriceOverrides
{
    // Currency code ("RUB"/"USD"/"EUR"/"GP") -> the client-side MongoID tpl of that money item.
    // Hardcoded: these are the same 4 fixed BSG ids the server's Money enum and the client's own
    // currency table (GClass3130) use — stable across EFT builds, no reflection needed to resolve them.
    private static readonly Dictionary<string, string> CurrencyTpl = new(StringComparer.OrdinalIgnoreCase)
    {
        ["RUB"] = "5449016a4bdc2d6f028b456f",
        ["USD"] = "5696686a4bdc2da3298b456a",
        ["EUR"] = "569668774bdc2da2298b4568",
        ["GP"] = "5d235b4d86f7742e017bc88a",
    };

    private static Dictionary<string, Dictionary<string, Entry>>? _byTraderThenTpl;
    private static bool _loaded;

    private readonly struct Entry
    {
        public readonly int Count;
        public readonly string CurrencyTplId;

        public Entry(int count, string currencyTplId)
        {
            Count = count;
            CurrencyTplId = currencyTplId;
        }
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true; // mark before the try: a failed fetch must not retry on every sell-price query

        try
        {
            var json = RequestHandler.GetJson("/trltraderprices/buy-overrides");
            var result = new Dictionary<string, Dictionary<string, Entry>>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(json))
            {
                // CR-03: parsed leaf-by-leaf via JObject/JToken (never a single strongly-typed
                // Dictionary<string, Dictionary<string, RawOverride>> deserialize) so ONE malformed
                // entry (wrong JSON type, a fractional "count", a future schema change — whatever a
                // hand-edit of buy-overrides.json might introduce) can only drop THAT entry, never
                // abort the whole parse and disable buy-price overrides for every other trader/item
                // for the rest of the game session. "count" is read as double (matches the server's
                // TraderOverride.Count type) and rounded, instead of binding straight into an int that
                // would throw on a fractional value.
                var root = JObject.Parse(json);
                foreach (var traderProp in root.Properties())
                {
                    var traderId = traderProp.Name;
                    if (string.IsNullOrWhiteSpace(traderId) || traderProp.Value is not JObject tplMap)
                    {
                        continue;
                    }

                    var inner = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
                    foreach (var tplProp in tplMap.Properties())
                    {
                        var tpl = tplProp.Name;
                        if (string.IsNullOrWhiteSpace(tpl) || tplProp.Value is not JObject node)
                        {
                            continue;
                        }

                        try
                        {
                            var currency = node["currency"]?.ToObject<string>();
                            var count = node["count"]?.ToObject<double?>();
                            if (count is null or <= 0 || string.IsNullOrWhiteSpace(currency)
                                || !CurrencyTpl.TryGetValue(currency.Trim(), out var currencyTplId))
                            {
                                continue;
                            }

                            inner[tpl] = new Entry((int)Math.Round(count.Value), currencyTplId);
                        }
                        catch
                        {
                            // Malformed leaf (wrong JSON type for count/currency, etc.) — skip just this entry.
                        }
                    }

                    if (inner.Count > 0)
                    {
                        result[traderId] = inner;
                    }
                }
            }

            _byTraderThenTpl = result;
            Plugin.Log?.LogInfo($"[TRLTraderPrices] {CountEntries(result)} buy-price override(s) loaded.");
        }
        catch (Exception ex)
        {
            _byTraderThenTpl = new Dictionary<string, Dictionary<string, Entry>>();
            Plugin.Log?.LogError($"[TRLTraderPrices] failed to fetch buy-overrides: {ex.Message}");
        }
    }

    private static int CountEntries(Dictionary<string, Dictionary<string, Entry>> map)
    {
        var n = 0;
        foreach (var tplMap in map.Values)
        {
            n += tplMap.Count;
        }

        return n;
    }

    /// <summary>Resolves the override for (traderId, tpl), if any. <paramref name="currencyTplId"/> is the client MongoID (as string) of the money item the amount is denominated in.</summary>
    internal static bool TryGetPrice(string traderId, string tpl, out string currencyTplId, out int amount)
    {
        EnsureLoaded();
        currencyTplId = string.Empty;
        amount = 0;

        if (_byTraderThenTpl is null || traderId is null || tpl is null)
        {
            return false;
        }

        if (!_byTraderThenTpl.TryGetValue(traderId, out var tplMap) || !tplMap.TryGetValue(tpl, out var entry))
        {
            return false;
        }

        currencyTplId = entry.CurrencyTplId;
        amount = entry.Count;
        return true;
    }
}
