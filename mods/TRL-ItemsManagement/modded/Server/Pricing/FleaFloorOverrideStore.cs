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
    /// </summary>
    internal static Dictionary<MongoId, double>? Map;

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
                if (floor > 0 && IsHex24(tpl))
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
            Map ??= new Dictionary<MongoId, double>();
            var key = new MongoId(tpl);
            if (Map.TryGetValue(key, out var existing) && existing == floor)
            {
                return;
            }

            Map[key] = floor;
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
            if (Map is null || !Map.Remove(new MongoId(tpl)))
            {
                return false;
            }

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

    private static bool IsHex24(string? s)
    {
        if (s is null || s.Length != 24)
        {
            return false;
        }

        foreach (var c in s)
        {
            if (!Uri.IsHexDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
