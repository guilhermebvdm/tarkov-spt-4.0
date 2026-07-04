using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

    // Raw JSON shape mirrors the server's TraderOverride record (config/buy-overrides.json keys are
    // lowercase "count"/"currency"). Newtonsoft matches property names case-insensitively by default,
    // but the explicit JsonProperty keeps this file self-documenting and immune to that default changing.
    private sealed class RawOverride
    {
        [JsonProperty("count")] public int Count { get; set; }
        [JsonProperty("currency")] public string? Currency { get; set; }
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
                var raw = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, RawOverride>>>(json);
                if (raw != null)
                {
                    foreach (var (traderId, tplMap) in raw)
                    {
                        if (string.IsNullOrWhiteSpace(traderId) || tplMap is null)
                        {
                            continue;
                        }

                        var inner = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
                        foreach (var (tpl, ovr) in tplMap)
                        {
                            if (ovr is null || string.IsNullOrWhiteSpace(tpl) || string.IsNullOrWhiteSpace(ovr.Currency)
                                || !CurrencyTpl.TryGetValue(ovr.Currency.Trim(), out var currencyTplId))
                            {
                                continue;
                            }

                            inner[tpl] = new Entry(ovr.Count, currencyTplId);
                        }

                        if (inner.Count > 0)
                        {
                            result[traderId] = inner;
                        }
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
