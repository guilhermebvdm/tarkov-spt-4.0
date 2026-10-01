using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace CompoundingPerf.Client.Telemetry;

/// <summary>
/// Client-side mirror of the server <c>TelemetryHub</c>. Same shape — diverged because
/// the server runs on net9.0 with System.Text.Json and the client runs on net471 with
/// Newtonsoft.Json. Counters always cheap; timings opt-in.
/// </summary>
public static class ClientTelemetry
{
    private static readonly ConcurrentDictionary<string, long> Counters = new();
    private static readonly ConcurrentDictionary<string, (long totalTicks, long n)> Timings = new();

    public static bool TimingEnabled { get; set; } = false;

    // Cached no-capture delegate — the hot path (by=1) must not allocate per call.
    private static readonly Func<string, long, long> AddOne = (_, prev) => prev + 1;

    public static void Increment(string key) =>
        Counters.AddOrUpdate(key, 1L, AddOne);

    /// <summary>Rare path for cumulative adds. Closure acceptable — per-raid call sites.</summary>
    public static void Increment(string key, long by) =>
        Counters.AddOrUpdate(key, by, (_, prev) => prev + by);

    public static long Get(string key) =>
        Counters.TryGetValue(key, out var v) ? v : 0;

    public static void RecordTiming(string key, long ticks)
    {
        if (!TimingEnabled) return;
        Timings.AddOrUpdate(key, (ticks, 1), (_, prev) => (prev.totalTicks + ticks, prev.n + 1));
    }

    public static T Time<T>(string key, Func<T> action)
    {
        if (!TimingEnabled) return action();
        var sw = Stopwatch.StartNew();
        try { return action(); }
        finally { sw.Stop(); RecordTiming(key, sw.ElapsedTicks); }
    }

    public static (Dictionary<string, long> counters, Dictionary<string, (double meanMs, long n)> timings) Snapshot()
    {
        var c = new Dictionary<string, long>(Counters);
        var t = new Dictionary<string, (double, long)>();
        foreach (var kv in Timings)
        {
            var meanTicks = kv.Value.n == 0 ? 0d : (double)kv.Value.totalTicks / kv.Value.n;
            var meanMs = meanTicks * 1000d / Stopwatch.Frequency;
            t[kv.Key] = (meanMs, kv.Value.n);
        }
        return (c, t);
    }
}
