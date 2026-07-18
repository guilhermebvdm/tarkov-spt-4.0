using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;

namespace TRLItemsManagement;

/// <summary>
///     Append-only audit trail for every mutating action across the mod's 7 write controllers (flea
///     price, ban, trader sell/buy price, flea-cap, flea-level, catalog rebuild/refresh) — one compact
///     JSON object per line in <c>ModPathsService.LogsDir/audit.jsonl</c>. JSONL (not one big JSON
///     array) so a crash mid-append corrupts at most the last line, and so writing never requires
///     re-serializing the whole file (unlike <see cref="StyleSensitiveJsonWriter"/>'s whole-file
///     tmp+rename, which exists for a different problem: an interrupted whole-file rewrite risking an
///     invalid document).
///     <para>
///     Callers append INSIDE the same <see cref="WriteLockService"/> critical section that already
///     protects their config write — after the mutation has succeeded, before returning the HTTP
///     response — so the log's line order is always causally identical to the true mutation order, and
///     validation failures / no-op deletes never produce a line.
///     </para>
///     <para>
///     <c>origin</c> is always <c>"live"</c> for entries this service writes. The reserved value
///     <c>"baseline-unknown-date"</c> is used only by the offline
///     <c>tools/trl-items-management/scripts/backfill-audit-log.js</c> seed script, which appends
///     directly to the file (this service is never invoked from that script).
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class AuditLogService(ModPathsService modPaths)
{
    private string LogPath => Path.Combine(modPaths.LogsDir, "audit.jsonl");

    public void Append(string feature, string action, string? tpl, object? before = null, object? after = null, object? extra = null)
    {
        try
        {
            var entry = new JsonObject
            {
                ["ts"] = DateTimeOffset.UtcNow.ToString("O"),
                ["feature"] = feature,
                ["action"] = action,
                ["tpl"] = tpl,
                ["before"] = ToNode(before),
                ["after"] = ToNode(after),
                ["extra"] = ToNode(extra),
                ["origin"] = "live",
            };

            Directory.CreateDirectory(modPaths.LogsDir);

            // FileShare.ReadWrite so a concurrent GET /audit-log reader never makes this append
            // fail (and get swallowed by the catch below): File.AppendAllText defaults to
            // FileShare.Read, which on Windows is mutually exclusive with a reader that opened
            // first — the append would throw and the entry would be silently lost. Writing the
            // whole line in a single Write keeps a concurrent reader from seeing a torn line (and
            // the reader's JSON parse skips the trailing partial line anyway).
            var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(entry.ToJsonString() + "\n");
            using var fs = new FileStream(LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            fs.Write(bytes, 0, bytes.Length);
        }
        catch
        {
            // Best-effort only — a failed audit-log write must never turn an already-successful
            // config mutation into a 500 for the caller (same reasoning as
            // FleaLevelController.MirrorIntoMeta).
        }
    }

    private static JsonNode? ToNode(object? value) => value switch
    {
        null => null,
        JsonNode node => node.DeepClone(),
        _ => JsonSerializer.SerializeToNode(value),
    };
}
