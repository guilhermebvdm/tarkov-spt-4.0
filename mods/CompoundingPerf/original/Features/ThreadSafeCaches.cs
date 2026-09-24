using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S10 — Thread-safety hardening for three SPT cache services whose plain
/// (non-concurrent) collections are written at runtime while other threads read them.
///
/// <para>All three follow the same pattern and the same fix. The pattern: a cache
/// backed by <c>Dictionary</c>/<c>HashSet</c>, populated lazily or appended on a
/// cache miss, with either no lock at all or a lock held only by writers (which
/// protects nothing — readers racing a writer is the hazard). The concurrency is
/// not hypothetical: ragfair offer generation fans out across
/// <c>Task.Factory.StartNew</c> tasks that price items via
/// <c>HandbookHelper.GetTemplatePrice</c> (which WRITES on each miss), and
/// <c>ItemBaseClassService.ItemHasBaseClass</c> appends to its cache whenever an
/// un-hydrated template shows up mid-session.</para>
///
/// <para>The fix: each service gets a DI-override subclass whose overrides wrap the
/// base implementation in a single reentrant gate. Because every external call
/// dispatches virtually to our override, every touch of the unsafe collection is
/// serialized — including the base methods' internal calls back into other
/// overridden members (C# locks are reentrant on the same thread, so the
/// read-miss → write-through path can't deadlock). No SPT logic is duplicated;
/// behavior is identical minus the data race.</para>
///
/// <para>This is the same hazard family S6 fixed in <c>RandomUtil</c>, and it
/// completes the groundwork for parallelizing bot generation in a future version.</para>
/// </summary>
public static class ThreadSafeCaches
{
    /// <summary>Shared kill-switch for all three overrides. False until OnLoad
    /// reads the config; while false every override defers straight to base.</summary>
    public static volatile bool IsEnabled;

    public static void Apply(ThreadSafeCachesOptions options, ISptLogger<CompoundingPerfMod> logger)
    {
        if (options.Enabled)
        {
            IsEnabled = true;
            logger.Success("[CompoundingPerf/S10] thread-safe caches ACTIVE — HandbookHelper, ItemBaseClassService, ItemFilterService gated");
        }
        else
        {
            IsEnabled = false;
            logger.Info("[CompoundingPerf/S10] thread-safe caches disabled in config");
        }
    }
}

/// <summary>
/// Guards <c>HandbookHelper</c>'s price cache: the lazy <c>??=</c> hydration has no
/// memory barrier, and <c>GetTemplatePrice</c> writes into a plain
/// <c>Dictionary&lt;MongoId, double&gt;</c> on every cache miss while parallel ragfair
/// tasks read it.
/// </summary>
// Lifetime must match vanilla HandbookHelper ([Injectable(InjectionType.Singleton)]) —
// Injectable DEFAULTS to Scoped; see CoalescingSaveServer for the failure mode.
[Injectable(InjectionType.Singleton, TypeOverride = typeof(HandbookHelper), TypePriority = 100)]
public class ThreadSafeHandbookHelper(
    ISptLogger<HandbookHelper> logger,
    DatabaseService            databaseService,
    ConfigServer               configServer,
    ICloner                    cloner)
    : HandbookHelper(logger, databaseService, configServer, cloner)
{
    private static readonly object Gate = new();

    protected override LookupCollection HandbookPriceCache
    {
        get
        {
            if (!ThreadSafeCaches.IsEnabled)
            {
                return base.HandbookPriceCache;
            }

            // base getter performs the lazy ??= hydration; under the gate it runs once.
            lock (Gate)
            {
                return base.HandbookPriceCache;
            }
        }
    }

    public override double GetTemplatePrice(MongoId tpl)
    {
        if (!ThreadSafeCaches.IsEnabled)
        {
            return base.GetTemplatePrice(tpl);
        }

        lock (Gate)
        {
            TelemetryHub.Increment("s10.handbook.price_calls");
            return base.GetTemplatePrice(tpl);
        }
    }

    // The three methods below reach the price cache through INTERNAL non-virtual calls
    // to GetTemplatePrice (Roslyn `call` — our override above doesn't intercept those),
    // so each externally-callable entry point gets its own gate. The lock is reentrant,
    // so the inner vanilla call running inside is fine.

    public override double GetTemplatePriceForItems(IEnumerable<Item> items)
    {
        if (!ThreadSafeCaches.IsEnabled)
        {
            return base.GetTemplatePriceForItems(items);
        }

        lock (Gate)
        {
            return base.GetTemplatePriceForItems(items);
        }
    }

    public override double InRoubles(double nonRoubleCurrencyCount, MongoId currencyTypeFrom)
    {
        if (!ThreadSafeCaches.IsEnabled)
        {
            return base.InRoubles(nonRoubleCurrencyCount, currencyTypeFrom);
        }

        lock (Gate)
        {
            return base.InRoubles(nonRoubleCurrencyCount, currencyTypeFrom);
        }
    }

    public override double FromRoubles(double roubleCurrencyCount, MongoId currencyTypeTo)
    {
        if (!ThreadSafeCaches.IsEnabled)
        {
            return base.FromRoubles(roubleCurrencyCount, currencyTypeTo);
        }

        lock (Gate)
        {
            return base.FromRoubles(roubleCurrencyCount, currencyTypeTo);
        }
    }
}

/// <summary>
/// Guards <c>ItemBaseClassService</c>: vanilla locks its writers
/// (<c>AddItemToCache</c>) but readers (<c>ItemHasBaseClass</c>,
/// <c>GetItemBaseClasses</c>) touch the same plain Dictionary lock-free — and the
/// read path itself triggers <c>AddItemToCache</c> on a miss. Routing every entry
/// point through one reentrant gate serializes the lot; the miss-then-write path
/// re-enters our overridden <c>AddItemToCache</c> on the same thread, which is fine.
/// </summary>
// Lifetime must match vanilla ItemBaseClassService ([Injectable(InjectionType.Singleton)]).
[Injectable(InjectionType.Singleton, TypeOverride = typeof(ItemBaseClassService), TypePriority = 100)]
public class ThreadSafeItemBaseClassService(
    ISptLogger<ItemBaseClassService> logger,
    DatabaseService                  databaseService,
    ServerLocalisationService        serverLocalisationService)
    : ItemBaseClassService(logger, databaseService, serverLocalisationService)
{
    private static readonly object Gate = new();

    public override void HydrateItemBaseClassCache()
    {
        if (!ThreadSafeCaches.IsEnabled) { base.HydrateItemBaseClassCache(); return; }
        lock (Gate) { base.HydrateItemBaseClassCache(); }
    }

    public override void AddItemToCache(MongoId itemTpl)
    {
        if (!ThreadSafeCaches.IsEnabled) { base.AddItemToCache(itemTpl); return; }
        lock (Gate) { base.AddItemToCache(itemTpl); }
    }

    public override bool ItemHasBaseClass(MongoId itemTpl, MongoId baseClass)
    {
        if (!ThreadSafeCaches.IsEnabled) { return base.ItemHasBaseClass(itemTpl, baseClass); }
        lock (Gate) { return base.ItemHasBaseClass(itemTpl, baseClass); }
    }

    public override bool ItemHasBaseClass(MongoId itemTpl, IEnumerable<MongoId> baseClasses)
    {
        if (!ThreadSafeCaches.IsEnabled) { return base.ItemHasBaseClass(itemTpl, baseClasses); }
        lock (Gate) { return base.ItemHasBaseClass(itemTpl, baseClasses); }
    }

    public override HashSet<MongoId> GetItemBaseClasses(MongoId itemTpl)
    {
        if (!ThreadSafeCaches.IsEnabled) { return base.GetItemBaseClasses(itemTpl); }
        lock (Gate) { return base.GetItemBaseClasses(itemTpl); }
    }
}

/// <summary>
/// Guards <c>ItemFilterService</c>'s blacklist HashSets: vanilla lazily populates them
/// with a check-then-act (<c>if (!cache.Any()) cache.UnionWith(...)</c>) and exposes
/// append methods, all lock-free on plain HashSets.
/// </summary>
// Lifetime must match vanilla ItemFilterService ([Injectable(InjectionType.Singleton)]).
[Injectable(InjectionType.Singleton, TypeOverride = typeof(ItemFilterService), TypePriority = 100)]
public class ThreadSafeItemFilterService(
    ISptLogger<ItemFilterService> logger,
    ConfigServer                  configServer)
    : ItemFilterService(logger, configServer)
{
    private static readonly object Gate = new();

    public override bool IsLootableItemBlacklisted(MongoId itemKey)
    {
        if (!ThreadSafeCaches.IsEnabled) { return base.IsLootableItemBlacklisted(itemKey); }
        lock (Gate) { return base.IsLootableItemBlacklisted(itemKey); }
    }

    public override bool IsItemBlacklisted(MongoId tpl)
    {
        if (!ThreadSafeCaches.IsEnabled) { return base.IsItemBlacklisted(tpl); }
        lock (Gate) { return base.IsItemBlacklisted(tpl); }
    }

    public override void AddItemToLootableBlacklistCache(IEnumerable<MongoId> itemTplsToBlacklist)
    {
        if (!ThreadSafeCaches.IsEnabled) { base.AddItemToLootableBlacklistCache(itemTplsToBlacklist); return; }
        lock (Gate) { base.AddItemToLootableBlacklistCache(itemTplsToBlacklist); }
    }

    public override void AddItemToBlacklistCache(IEnumerable<MongoId> itemTplsToBlacklist)
    {
        if (!ThreadSafeCaches.IsEnabled) { base.AddItemToBlacklistCache(itemTplsToBlacklist); return; }
        lock (Gate) { base.AddItemToBlacklistCache(itemTplsToBlacklist); }
    }
}
