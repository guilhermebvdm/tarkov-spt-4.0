using SPTarkov.Server.Core.Models.Common;   // MongoId
using SPTarkov.Server.Core.Utils;           // JsonUtil

namespace TRLItemsManagement.Pricing;

/// <summary>
///     Shared parser for the two trader-override config files (<c>overrides.json</c> for sell,
///     <c>buy-overrides.json</c> for buy) — same shape (traderId → tpl → {count, currency}), same
///     MongoId/Fence validation, different per-entry behavior. Deduplicates what used to be two
///     near-identical blocks in TRLTraderPricesMod.cs (CR-06).
///     <para>
///     Two-layer callback design: <see cref="TryResolveTrader{TCtx}"/> runs ONCE per traderId in the
///     file (lets the sell side build its per-trader <c>itemsById</c> index once, preserving the O(1)
///     lookup perf it already had — the buy side just returns <c>true</c> unconditionally, since it never
///     needed a DB check); <see cref="OnEntry{TCtx}"/> runs per (tpl, override) of an already-resolved
///     trader and owns 100% of the side-effect (mutate assort vs. populate a dict) plus any extra
///     counters that side needs (sell tracks applied/tplNotSold/barterSkip/etc.; buy doesn't).
///     </para>
///     Try-pattern (not <c>Func&lt;MongoId, TCtx?&gt;</c>) is deliberate: a nullable generic breaks when
///     <c>TCtx</c> is a non-nullable value type with no natural "not resolved" value (the buy side's
///     <c>TCtx = bool</c> — <c>bool</c> can never be null), which would force a boxing hack. <c>bool</c>
///     return + <c>out TCtx</c> avoids that entirely.
/// </summary>
internal static class TraderOverrideConfigParser
{
    internal static readonly MongoId FenceId = new("579dc571d53a0658a154fbec");

    internal readonly record struct ParseSummary(int Entries, int BadTrader, int BadTpl, int FenceSkip);

    internal delegate bool TryResolveTrader<TCtx>(MongoId traderId, out TCtx ctx);

    internal delegate void OnEntry<TCtx>(MongoId traderId, TCtx ctx, MongoId tpl, TraderOverride ovr);

    /// <summary>
    ///     Parses <paramref name="configPath"/> (traderId → tpl → override), dropping Fence and any
    ///     malformed traderId/tpl (counted, never thrown — a bad config entry must never abort the
    ///     caller's own try/catch, since a corrupt file is a expected, recoverable condition here).
    /// </summary>
    internal static ParseSummary Parse<TCtx>(
        string configPath,
        JsonUtil jsonUtil,
        TryResolveTrader<TCtx> resolveTrader,
        OnEntry<TCtx> onEntry)
    {
        var entries = 0;
        var badTrader = 0;
        var badTpl = 0;
        var fenceSkip = 0;

        var raw = jsonUtil.DeserializeFromFile<Dictionary<string, Dictionary<string, TraderOverride>>>(configPath);
        if (raw is null || raw.Count == 0)
        {
            return new ParseSummary(entries, badTrader, badTpl, fenceSkip);
        }

        foreach (var (traderIdStr, tplMap) in raw)
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
                // Fence: dynamic assort — an override here would be meaningless/unsafe for either side.
                fenceSkip++;
                continue;
            }

            if (tplMap is null)
            {
                continue;
            }

            if (!resolveTrader(traderId, out var ctx))
            {
                badTrader++;
                continue;
            }

            foreach (var (tplStr, ovr) in tplMap)
            {
                if (ovr is null || string.IsNullOrWhiteSpace(tplStr) || tplStr.Length != 24)
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

                onEntry(traderId, ctx, tpl, ovr);
                entries++;
            }
        }

        return new ParseSummary(entries, badTrader, badTpl, fenceSkip);
    }
}
