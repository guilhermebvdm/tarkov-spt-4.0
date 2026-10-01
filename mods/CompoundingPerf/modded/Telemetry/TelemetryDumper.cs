using System.Text.Json;

namespace CompoundingPerf.Telemetry;

/// <summary>
/// Writes a one-shot JSON snapshot of <see cref="TelemetryHub"/> state to a file under the
/// SPT logs directory. Append-only — each call writes one line of JSON containing the full
/// snapshot at that moment, so downstream tools can diff successive entries.
/// </summary>
public static class TelemetryDumper
{
    public static void Dump(string sptUserLogsDir, string reason)
    {
        try
        {
            Directory.CreateDirectory(sptUserLogsDir);
            var path = Path.Combine(
                sptUserLogsDir,
                $"CompoundingPerf-telemetry-{DateTime.UtcNow:yyyyMMdd}.jsonl");

            var (counters, timings) = TelemetryHub.Snapshot();
            var line = JsonSerializer.Serialize(new
            {
                ts = DateTime.UtcNow.ToString("O"),
                reason,
                counters,
                timings = timings.ToDictionary(
                    kv => kv.Key,
                    kv => new { meanMs = kv.Value.meanMs, n = kv.Value.n })
            });

            File.AppendAllText(path, line + Environment.NewLine);
        }
        catch
        {
            // Telemetry is best-effort — never let a dump failure cascade.
        }
    }
}
