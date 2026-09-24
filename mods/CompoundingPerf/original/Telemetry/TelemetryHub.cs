using System.Collections.Concurrent;
using System.Diagnostics;

namespace CompoundingPerf.Telemetry;

/// <summary>
/// Thread-safe counter and timing store for server-side features. Counters are always
/// cheap (interlocked increment); timings are opt-in because Stopwatch on a hot path
/// can cost more than the patch saves.
/// </summary>
public static class TelemetryHub
{
    private static readonly ConcurrentDictionary<string, long> Counters = new();
    private static readonly ConcurrentDictionary<string, (long totalTicks, long n)> Timings = new();

    public static bool TimingEnabled { get; set; } = false;

    // Cached no-capture delegate: the hot path (by=1) must not allocate. A lambda that
    // captures `by` allocates a closure per call — on the log-filter drop path that's
    // an allocation per suppressed line, which would undercut the feature's own point.
    private static readonly Func<string, long, long> AddOne = static (_, prev) => prev + 1;

    public static void Increment(string key) =>
        Counters.AddOrUpdate(key, 1L, AddOne);

    /// <summary>Rare path for cumulative adds (sizes, durations). The closure here is
    /// acceptable — call sites are per-raid or per-dump, not per-message.</summary>
    public static void Increment(string key, long by) =>
        Counters.AddOrUpdate(key, by, (_, prev) => prev + by);

    public static long Get(string key) =>
        Counters.TryGetValue(key, out var v) ? v : 0;

    /// <summary>Record an elapsed-ticks observation. No-op when <see cref="TimingEnabled"/> is false.</summary>
    public static void RecordTiming(string key, long ticks)
    {
        if (!TimingEnabled) return;
        Timings.AddOrUpdate(key, (ticks, 1), (_, prev) => (prev.totalTicks + ticks, prev.n + 1));
    }

    /// <summary>Helper: time a block and record under the given key. Returns the result.
    /// When timing is disabled, executes the block with zero overhead beyond the call itself.</summary>
    public static T Time<T>(string key, Func<T> action)
    {
        if (!TimingEnabled) return action();
        var sw = Stopwatch.StartNew();
        try { return action(); }
        finally { sw.Stop(); RecordTiming(key, sw.ElapsedTicks); }
    }

    /// <summary>Snapshot for dumping. Returns counters and per-key (mean ms, n) for timings.</summary>
    public static (Dictionary<string, long> counters, Dictionary<string, (double meanMs, long n)> timings) Snapshot()
    {
        var c = new Dictionary<string, long>(Counters);
        var t = new Dictionary<string, (double, long)>();
        foreach (var (k, v) in Timings)
        {
            var meanTicks = v.n == 0 ? 0d : (double)v.totalTicks / v.n;
            var meanMs = meanTicks * 1000d / Stopwatch.Frequency;
            t[k] = (meanMs, v.n);
        }
        return (c, t);
    }

    public static void Reset()
    {
        Counters.Clear();
        Timings.Clear();
    }
}
