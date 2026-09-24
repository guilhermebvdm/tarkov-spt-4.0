using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Routers;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S2 — Subclass of <see cref="HttpRouter"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/>. Caches responses for a whitelist of
/// endpoints whose payloads are static after server load (item DB, handbook,
/// hideout recipes, globals, etc.). Subsequent identical requests skip the entire
/// router → static-router → controller chain and return the cached JSON directly.
///
/// <para><b>Cacheability rules</b>: a request is cacheable iff
/// <list type="number">
///   <item>its path is in <see cref="DefaultCacheablePaths"/> (or the user's
///         <see cref="ResponseCacheOptions.AdditionalPaths"/>), AND</item>
///   <item>its body is empty or whitespace.</item>
/// </list>
/// We deliberately keep this conservative — anything with a non-empty body
/// (POSTs carrying session-specific arguments) is treated as dynamic and
/// passes through to the base router untouched.</para>
///
/// <para><b>Cache lifetime</b>: entries live until server shutdown. The whitelist
/// is restricted to paths whose payloads are computed once at OnLoad and never
/// mutated. If a path's payload can change at runtime, it must not be on the
/// whitelist.</para>
///
/// <para><b>Other-mod compatibility</b>: the first request for a cacheable path
/// runs through <c>base.GetResponse</c> exactly like vanilla, so any other mod's
/// Harmony patches or router-chain modifications are observed and baked into the
/// cached response. Subsequent requests return that same response. The order of
/// mod loading doesn't change cache contents.</para>
/// </summary>
[Injectable(TypeOverride = typeof(HttpRouter), TypePriority = 100)]
public class CachingHttpRouter : HttpRouter
{
    /// <summary>
    /// Paths whose responses are static after server load. Conservative list —
    /// every entry is a global-data endpoint (same response for every session
    /// regardless of caller). Per-session endpoints (profile/info, quest/list,
    /// customization/storage, etc.) are excluded.
    /// </summary>
    public static readonly HashSet<string> DefaultCacheablePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/client/items",
        "/client/items/templates",
        "/client/handbook/templates",
        "/client/globals",
        "/client/customization",
        "/client/languages",
        "/client/locale/en",
        "/client/menu/locale/en",
        "/client/settings",
        "/client/hideout/areas",
        "/client/hideout/production/recipes",
        "/client/hideout/qte/list",
        "/client/locations",
        "/client/trading/api/traderSettings",
        "/client/prestige/list",
    };

    // Case-insensitive comparer to match _activePaths semantics. Without it, different
    // case variants of the same path (e.g. /client/items vs /client/ITEMS) would each
    // get their own cache entry — small memory bloat + cache miss on variant casing.
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _activePaths = DefaultCacheablePaths;
    private bool _enabled = false;

    public CachingHttpRouter(
        IEnumerable<StaticRouter>  staticRouters,
        IEnumerable<DynamicRouter> dynamicRoutes)
        : base(staticRouters, dynamicRoutes)
    {
    }

    /// <summary>Called by <see cref="CompoundingPerfMod.OnLoad"/> to wire up config.
    /// Until this runs the cache is inert — every call passes through to base.</summary>
    public void Configure(ResponseCacheOptions options)
    {
        _enabled = options.Enabled;
        if (options.AdditionalPaths is { Count: > 0 })
        {
            _activePaths = new HashSet<string>(DefaultCacheablePaths, StringComparer.OrdinalIgnoreCase);
            foreach (var p in options.AdditionalPaths) _activePaths.Add(p);
        }
    }

    // S14 (memoized route dispatch) lived here and was REMOVED before release: routing
    // through a behavior-mirror of GetResponse/HandleRoute changed the
    // /launcher/server/connect response under FIKA (raw 0.0.0.0 backendUrl instead of
    // the rewritten one → the game client cannot connect). The mechanism behind the
    // rewrite was never fully identified, which is exactly why a feature whose entire
    // contract is "behavior-identical routing" cannot ship.

    public override async ValueTask<string> GetResponse(HttpRequest req, MongoId sessionID, string body)
    {
        // S11 hook: this override sees every routed request, which makes it the natural
        // place to track per-session dirtiness for the save-skip feature. Pure-read
        // paths are filtered inside MarkRequest; everything else flags the session.
        ProfileDirtyTracker.MarkRequest(sessionID, req.Path.Value);

        if (!_enabled || !IsCacheable(req, body))
        {
            return await base.GetResponse(req, sessionID, body);
        }

        var key = req.Path.Value!;

        if (_cache.TryGetValue(key, out var cached))
        {
            TelemetryHub.Increment("s2.response_cache.hits");
            return cached;
        }

        TelemetryHub.Increment("s2.response_cache.misses");
        var response = await base.GetResponse(req, sessionID, body);

        // Cache only non-null, non-empty responses — error responses are typically empty
        // strings and would poison the cache if stored.
        if (!string.IsNullOrEmpty(response))
        {
            _cache.TryAdd(key, response);
        }
        return response;
    }

    private bool IsCacheable(HttpRequest req, string body)
    {
        if (!string.IsNullOrWhiteSpace(body)) return false;
        var path = req.Path.Value;
        return path is not null && _activePaths.Contains(path);
    }

    /// <summary>Test seam — lets unit tests verify the decision logic without spinning up DI.</summary>
    public bool IsCacheableForTesting(string path, string body) =>
        string.IsNullOrWhiteSpace(body) && _activePaths.Contains(path);

    /// <summary>Test seam — count entries currently cached.</summary>
    public int CachedEntryCount => _cache.Count;
}
