using System;
using System.IO;
using Newtonsoft.Json;
using CompoundingPerf.Client.Telemetry;

namespace CompoundingPerf.Client;

/// <summary>
/// Client-side telemetry writer. Append-only JSONL — one snapshot per call. Best-effort:
/// failures are swallowed because telemetry must never break gameplay.
/// </summary>
internal static class TelemetryDumper
{
    public static void Dump(string sptUserLogsDir, string reason)
    {
        try
        {
            Directory.CreateDirectory(sptUserLogsDir);
            var path = Path.Combine(
                sptUserLogsDir,
                $"CompoundingPerf-client-telemetry-{DateTime.UtcNow:yyyyMMdd}.jsonl");

            var snap = ClientTelemetry.Snapshot();
            var line = JsonConvert.SerializeObject(new
            {
                ts = DateTime.UtcNow.ToString("O"),
                reason,
                counters = snap.counters,
                timings = ProjectTimings(snap.timings)
            });

            File.AppendAllText(path, line + Environment.NewLine);
        }
        catch
        {
            // best-effort
        }
    }

    private static System.Collections.Generic.Dictionary<string, object> ProjectTimings(
        System.Collections.Generic.Dictionary<string, (double meanMs, long n)> raw)
    {
        var result = new System.Collections.Generic.Dictionary<string, object>(raw.Count);
        foreach (var kv in raw)
            result[kv.Key] = new { meanMs = kv.Value.meanMs, n = kv.Value.n };
        return result;
    }
}
