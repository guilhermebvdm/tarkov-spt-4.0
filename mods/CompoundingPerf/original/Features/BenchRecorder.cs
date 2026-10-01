#if BENCH
using System.Text.Json;
using CompoundingPerf.Telemetry;
using SPTarkov.Server.Core.Models.Utils;

namespace CompoundingPerf.Features;

/// <summary>
/// Server-side benchmark sampler. DEV BUILDS ONLY (-p:Bench=true) — release packages
/// contain no sampler. Every 30s, appends one JSON line to
/// <c>user/logs/CompoundingPerf-serverstats.jsonl</c> with per-interval deltas:
///
/// <list type="bullet">
///   <item>GC pause milliseconds + pause percentage (GC.GetTotalPauseDuration deltas) —
///         the direct measure of the stalls S8/S11 claim to remove,</item>
///   <item>gen0/1/2 collection counts,</item>
///   <item>managed heap size,</item>
///   <item>feature counters: saves executed vs skipped-clean (S1/S11), response-cache
///         hits (S2), sanitizer calls (S7), compressed responses (S9).</item>
/// </list>
///
/// Lines carry the masterEnabled flag, so the report script can split server-side
/// samples by A/B side the same way it splits client frame stats.
/// </summary>
public static class BenchRecorder
{
    private const int IntervalSec = 30;
    private static BenchCounters? _prev;
    private static DateTime _prevAt;
    private static bool _masterEnabled;

    public static void Start(ISptLogger<CompoundingPerfMod> logger, bool masterEnabled)
    {
        _masterEnabled = masterEnabled;
        var logsDir = Path.Combine(Directory.GetCurrentDirectory(), "user", "logs");
        var outFile = Path.Combine(logsDir, "CompoundingPerf-serverstats.jsonl");
        logger.Info($"[CompoundingPerf/BENCH] server sampler armed — GC + counter deltas every {IntervalSec}s to {outFile}");

        _prev = Snapshot();
        _prevAt = DateTime.UtcNow;

        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(IntervalSec));
                try
                {
                    var cur = Snapshot();
                    var now = DateTime.UtcNow;
                    var delta = BenchSampleMath.Delta(_prev!, cur, (now - _prevAt).TotalSeconds);
                    _prev = cur;
                    _prevAt = now;

                    var line = JsonSerializer.Serialize(new
                    {
                        ts = now.ToString("O"),
                        masterEnabled = _masterEnabled,
                        intervalSec = Math.Round(delta.IntervalSec, 1),
                        gcPauseMs = Math.Round(delta.GcPauseMs, 1),
                        gcPausePct = Math.Round(delta.GcPausePct, 3),
                        gen0 = delta.Gen0,
                        gen1 = delta.Gen1,
                        gen2 = delta.Gen2,
                        heapMb = Math.Round(GC.GetTotalMemory(false) / 1048576.0, 1),
                        savesExecuted = delta.SavesExecuted,
                        savesSkippedClean = delta.SavesSkippedClean,
                        cacheHits = delta.CacheHits,
                        sanitizerCalls = delta.SanitizerCalls,
                        compressedResponses = delta.CompressedResponses,
                        forcedGcSkipped = delta.ForcedGcSkipped,
                        wsBroadcasts = delta.WsBroadcasts,
                        wsSends = delta.WsSends,
                        notifierPolls = delta.NotifierPolls,
                    });
                    File.AppendAllText(outFile, line + Environment.NewLine);
                }
                catch
                {
                    // Sampler is observability, never load-bearing — swallow and keep going.
                }
            }
        });
    }

    private static BenchCounters Snapshot() => new()
    {
        TotalGcPauseMs = GC.GetTotalPauseDuration().TotalMilliseconds,
        Gen0 = GC.CollectionCount(0),
        Gen1 = GC.CollectionCount(1),
        Gen2 = GC.CollectionCount(2),
        SavesExecuted = TelemetryHub.Get("s1.profile_save.executed"),
        SavesSkippedClean = TelemetryHub.Get("s11.profile_save.skipped_clean"),
        CacheHits = TelemetryHub.Get("s2.response_cache.hits"),
        SanitizerCalls = TelemetryHub.Get("s7.sanitizer.calls"),
        CompressedResponses = TelemetryHub.Get("s9.compression.responses"),
        ForcedGcSkipped = TelemetryHub.Get("s8.ragfair.forced_gc_skipped"),
        WsBroadcasts = TelemetryHub.Get("s13.ws.broadcasts"),
        WsSends = TelemetryHub.Get("s13.ws.sends"),
        NotifierPolls = TelemetryHub.Get("s13.notifier.polls"),
    };
}
#endif
