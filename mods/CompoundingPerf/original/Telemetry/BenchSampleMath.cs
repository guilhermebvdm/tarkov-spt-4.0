namespace CompoundingPerf.Telemetry;

/// <summary>
/// Pure delta math for the dev-build server bench sampler (see BenchRecorder, BENCH
/// builds only). Ungated so unit tests can pin the arithmetic without a BENCH-defined
/// test assembly — the type is inert in release builds (nothing constructs it).
/// </summary>
public sealed class BenchCounters
{
    public double TotalGcPauseMs { get; init; }
    public int Gen0 { get; init; }
    public int Gen1 { get; init; }
    public int Gen2 { get; init; }
    public long SavesExecuted { get; init; }
    public long SavesSkippedClean { get; init; }
    public long CacheHits { get; init; }
    public long SanitizerCalls { get; init; }
    public long CompressedResponses { get; init; }
    public long ForcedGcSkipped { get; init; }
    public long WsBroadcasts { get; init; }
    public long WsSends { get; init; }
    public long NotifierPolls { get; init; }
}

public sealed class BenchDelta
{
    public double IntervalSec { get; init; }
    public double GcPauseMs { get; init; }
    public double GcPausePct { get; init; }
    public int Gen0 { get; init; }
    public int Gen1 { get; init; }
    public int Gen2 { get; init; }
    public long SavesExecuted { get; init; }
    public long SavesSkippedClean { get; init; }
    public long CacheHits { get; init; }
    public long SanitizerCalls { get; init; }
    public long CompressedResponses { get; init; }
    public long ForcedGcSkipped { get; init; }
    public long WsBroadcasts { get; init; }
    public long WsSends { get; init; }
    public long NotifierPolls { get; init; }
}

public static class BenchSampleMath
{
    /// <summary>Per-interval deltas between two cumulative snapshots. Counters are
    /// monotonic, so negatives only happen on counter reset — clamp to zero rather
    /// than emit a misleading negative.</summary>
    public static BenchDelta Delta(BenchCounters prev, BenchCounters cur, double intervalSec)
    {
        var pauseMs = Math.Max(0, cur.TotalGcPauseMs - prev.TotalGcPauseMs);
        return new BenchDelta
        {
            IntervalSec = intervalSec,
            GcPauseMs = pauseMs,
            GcPausePct = intervalSec > 0 ? pauseMs / (intervalSec * 1000.0) * 100.0 : 0,
            Gen0 = Math.Max(0, cur.Gen0 - prev.Gen0),
            Gen1 = Math.Max(0, cur.Gen1 - prev.Gen1),
            Gen2 = Math.Max(0, cur.Gen2 - prev.Gen2),
            SavesExecuted = Math.Max(0, cur.SavesExecuted - prev.SavesExecuted),
            SavesSkippedClean = Math.Max(0, cur.SavesSkippedClean - prev.SavesSkippedClean),
            CacheHits = Math.Max(0, cur.CacheHits - prev.CacheHits),
            SanitizerCalls = Math.Max(0, cur.SanitizerCalls - prev.SanitizerCalls),
            CompressedResponses = Math.Max(0, cur.CompressedResponses - prev.CompressedResponses),
            ForcedGcSkipped = Math.Max(0, cur.ForcedGcSkipped - prev.ForcedGcSkipped),
            WsBroadcasts = Math.Max(0, cur.WsBroadcasts - prev.WsBroadcasts),
            WsSends = Math.Max(0, cur.WsSends - prev.WsSends),
            NotifierPolls = Math.Max(0, cur.NotifierPolls - prev.NotifierPolls),
        };
    }
}
