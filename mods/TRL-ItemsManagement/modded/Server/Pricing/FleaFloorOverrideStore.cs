using SPTarkov.Server.Core.Models.Common;   // MongoId
using SPTarkov.Server.Core.Models.Utils;    // ISptLogger
using SPTarkov.Server.Core.Utils;           // JsonUtil

namespace TRLItemsManagement.Pricing;

/// <summary>
///     B-5: per-tpl flea-floor overrides. SPT imposes a hard floor on every dynamic flea offer — an item
///     can't list below its "highest sell-to-trader price" (handbook × trader buyback, loyalty 0),
///     recomputed at offer time in <c>TraderHelper.GetHighestSellToTraderPrice</c> and re-applied in
///     <c>RagfairPriceService.GetDynamicItemPrice</c>. That recompute is handbook-based and ignores our
///     <c>itemPriceOverrideRouble</c>, so an operator can't push a price below it — hence B-5.
///     <para>
///     <see cref="FleaFloorOverridePatch"/> lowers that floor to the configured value for WHITELISTED
///     tpls only (via <c>Math.Min</c> — it can never RAISE a floor, so a bad entry can't break vanilla
///     pricing). Every other item keeps its exact vanilla floor.
///     </para>
///     <para>
///     Persisted to <c>config/flea-floor-overrides.json</c> (<c>{ "&lt;tpl&gt;": &lt;floorRouble&gt; }</c>):
///     loaded once at boot by <see cref="TraderPriceOnLoad"/>, mutated at runtime by
///     <c>FleaPriceController</c> when the operator sets a flea price below the vanilla floor. The runtime
///     mutation updates <see cref="Map"/> immediately (so the patch sees it without a restart), but SPT
///     caches generated flea offers — the below-floor price only fully shows in-game once offers
///     regenerate (server restart or flea refresh), the same "restart to apply" caveat as the other
///     price writes.
///     </para>
/// </summary>
internal static class FleaFloorOverrideStore
{
    internal const string FileName = "flea-floor-overrides.json";

    /// <summary>
    ///     MongoId-keyed for O(1) lookup by the Harmony patch on the offer-generation path. Null until
    ///     <see cref="Load"/> runs; empty (not null) once loaded with no entries.
    ///     <para>
    ///     Treated as immutable once published: <see cref="Set"/>/<see cref="Remove"/> swap in a fresh
    ///     copy under <see cref="Gate"/> rather than mutating in place, so <see cref="FleaFloorOverridePatch"/>
    ///     — which reads this field WITHOUT the lock, on the parallel ragfair offer-generation threads
    ///     (<c>RagfairOfferGenerator</c> spawns <c>Task.Factory.StartNew</c> per assort) — never observes a
    ///     half-mutated dictionary. <c>volatile</c> so the reference swap is promptly visible cross-thread.
    ///     </para>
    /// </summary>
    internal static volatile Dictionary<MongoId, double>? Map;

    private static readonly object Gate = new();

    internal static void Load(string configPath, JsonUtil jsonUtil, ISptLogger<TraderPriceOnLoad> logger)
    {
        var map = new Dictionary<MongoId, double>();

        // DeserializeFromFile returns null for a MISSING file and throws on a corrupt one — the caller
        // (TraderPriceOnLoad.OnLoad) wraps this so a bad config can't brick boot.
        var raw = jsonUtil.DeserializeFromFile<Dictionary<string, double>>(configPath);
        if (raw is not null)
        {
            foreach (var (tpl, floor) in raw)
            {
                if (floor > 0 && TplValidation.IsHex24(tpl))
                {
                    map[new MongoId(tpl)] = floor;
                }
            }
        }

        lock (Gate)
        {
            Map = map;
        }

        logger.Info($"[TRLItemsManagement] {FileName}: {map.Count} flea-floor override(s) loaded.");
    }

    /// <summary>
    ///     Whitelist a tpl at the given floor and persist. Idempotent — a no-op (no disk write) when the
    ///     tpl already has this exact floor, honoring the mod's "only touch disk on a real mutation" rule.
    /// </summary>
    internal static void Set(string configDir, JsonUtil jsonUtil, string tpl, double floor)
    {
        lock (Gate)
        {
            var key = new MongoId(tpl);
            // CR-04: tolerant compare — floor is always an integer rouble here, but exact double == is
            // fragile if the source ever changes.
            if (Map is { } cur && cur.TryGetValue(key, out var existing) && Math.Abs(existing - floor) < 0.5)
            {
                return;
            }

            // CR-01 copy-on-write: mutate a fresh copy and swap the reference, never edit Map in place —
            // the lockless patch reader runs on parallel offer-gen threads (see Map's remarks).
            var next = Map is null ? new Dictionary<MongoId, double>() : new Dictionary<MongoId, double>(Map);
            next[key] = floor;
            Map = next;
            Save(configDir, jsonUtil);
        }
    }

    /// <summary>
    ///     Drop a tpl's floor override (item returns to the vanilla floor) and persist. Returns false and
    ///     writes nothing when the tpl wasn't whitelisted.
    /// </summary>
    internal static bool Remove(string configDir, JsonUtil jsonUtil, string tpl)
    {
        lock (Gate)
        {
            var key = new MongoId(tpl);
            if (Map is null || !Map.ContainsKey(key))
            {
                return false;
            }

            // CR-01 copy-on-write: same rationale as Set — swap a fresh copy, never mutate in place.
            var next = new Dictionary<MongoId, double>(Map);
            next.Remove(key);
            Map = next;
            Save(configDir, jsonUtil);
            return true;
        }
    }

    // Caller holds Gate.
    private static void Save(string configDir, JsonUtil jsonUtil)
    {
        // String-keyed on disk: a MongoId as a JSON property NAME needs a WriteAsPropertyName-capable
        // converter that isn't guaranteed registered (same reasoning as FleaPriceController.GetOverrides).
        var raw = Map!.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);
        var json = jsonUtil.Serialize(raw, indented: true) ?? "{}";

        // tmp + rename so a crash mid-write can't leave a truncated config (matches the other writers).
        var path = Path.Combine(configDir, FileName);
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, path, overwrite: true);
    }
}
