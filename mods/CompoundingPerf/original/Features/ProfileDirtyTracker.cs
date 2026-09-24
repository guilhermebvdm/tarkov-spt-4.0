using System.Collections.Concurrent;
using SPTarkov.Server.Core.Models.Common;

namespace CompoundingPerf.Features;

/// <summary>
/// S11 shared state — per-session dirty flags + last-real-save stamps.
///
/// <para><b>The problem (source-verified)</b>: vanilla's <c>SaveProfileAsync</c> serializes
/// the full profile to JSON and MD5-hashes it on every periodic save tick, and only THEN
/// compares hashes to skip the disk write. An idle player pays multi-megabyte
/// serialization + hashing every few seconds, forever, for nothing.</para>
///
/// <para><b>The fix</b>: skip the save call entirely when we know nothing changed.
/// "Know" is implemented conservatively: <see cref="CachingHttpRouter"/> marks a session
/// dirty for ANY request whose path is not in a small known-pure whitelist (keepalives,
/// pings, notifier long-polls, the static data endpoints S2 caches). Unknown paths are
/// assumed mutating. A clean session still gets a real save every
/// <c>ForceSaveIntervalSeconds</c> (default 60) to persist server-internal changes that
/// don't arrive via HTTP (hideout production progress, insurance returns) — so the
/// worst-case persistence window for purely passive changes is the force interval,
/// versus one vanilla tick. Player-driven changes always travel through a non-pure
/// request first and therefore can never be skipped.</para>
///
/// <para><b>Default must exceed the save tick.</b> SPT runs the periodic save every
/// <c>profileSaveIntervalSeconds</c> (60s by default). If the force interval equals the
/// tick, a clean session is always "due" at every tick and never actually skips — the
/// feature throttles itself to nothing. The default is therefore 300s (skip ~4 of every
/// 5 idle saves, force the 5th), trading a 5-minute worst-case passive-change window for
/// the optimization actually doing something.</para>
/// </summary>
public static class ProfileDirtyTracker
{
    public static volatile bool IsEnabled;
    public static volatile int ForceSaveIntervalSeconds = 300;

    private static readonly ConcurrentDictionary<MongoId, byte> Dirty = new();
    private static readonly ConcurrentDictionary<MongoId, DateTime> LastRealSaveUtc = new();

    /// <summary>Paths that never mutate profile state. Everything NOT here dirties the
    /// session — conservative by construction. Prefix match for the notifier/ping families.</summary>
    private static readonly string[] PurePrefixes =
    [
        "/client/game/keepalive",
        "/client/notifier",
        "/notifierServer",
        "/launcher/ping",
        "/launcher/server",
        "/fika/update/ping",
        "/client/checkVersion",
        "/client/game/version",
        "/client/putMetrics",
        "/client/getMetricsConfig",
        "/singleplayer/settings",
        "/files/",
    ];

    public static void MarkRequest(MongoId sessionId, string? path)
    {
        if (!IsEnabled || sessionId.IsEmpty || path is null)
        {
            return;
        }

        foreach (var pure in PurePrefixes)
        {
            if (path.StartsWith(pure, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        // S2's static data endpoints are global reads — also pure.
        if (CachingHttpRouter.DefaultCacheablePaths.Contains(path))
        {
            return;
        }

        Dirty[sessionId] = 1;
    }

    /// <summary>True when the save may be skipped: session is clean AND the last real
    /// save is fresher than the force interval.</summary>
    public static bool MaySkipSave(MongoId sessionId)
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (Dirty.ContainsKey(sessionId))
        {
            return false;
        }

        if (!LastRealSaveUtc.TryGetValue(sessionId, out var last))
        {
            return false; // never actually saved through us — don't skip
        }

        return (DateTime.UtcNow - last).TotalSeconds < ForceSaveIntervalSeconds;
    }

    /// <summary>Called when a real save is about to run: clears the dirty flag and stamps
    /// the time. Clearing BEFORE the save body runs is deliberate — anything that dirties
    /// the session mid-save will be picked up by the NEXT tick rather than lost.</summary>
    public static void OnRealSaveStarting(MongoId sessionId)
    {
        Dirty.TryRemove(sessionId, out _);
        LastRealSaveUtc[sessionId] = DateTime.UtcNow;
    }
}
