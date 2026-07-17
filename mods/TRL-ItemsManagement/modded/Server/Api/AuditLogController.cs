using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

/// <summary>
///     Read-only view over the audit trail <see cref="AuditLogService"/> writes to
///     <c>logs/audit.jsonl</c> — one line per mutating action across all 7 write controllers, plus the
///     offline backfill script's <c>baseline-unknown-date</c> seed entries.
///     <para>
///     V1: reads the whole file per request (no index/cache) — fine at the expected volume
///     (administrative actions, not a hot path); if this ever needs to scale past that, revisit then.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class AuditLogController(ModPathsService modPaths) : ControllerBase
{
    [HttpGet("audit-log")]
    public IActionResult GetAuditLog(
        [FromQuery] string? tpl,
        [FromQuery] string? feature,
        [FromQuery] string? action,
        [FromQuery] DateTimeOffset? since,
        [FromQuery] DateTimeOffset? until,
        [FromQuery] int limit = 100,
        [FromQuery] int offset = 0)
    {
        limit = Math.Clamp(limit, 1, 1000);
        offset = Math.Max(offset, 0);

        // Comma-separated ("tpl=a,b,c") — same convention as the main table's URL filters
        // (group=A,B,C etc.) — lets the viewer's name-based search match multiple resolved tpls
        // in one request instead of firing one request per candidate.
        var tplSet = tpl?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

        var logPath = Path.Combine(modPaths.LogsDir, "audit.jsonl");
        if (!System.IO.File.Exists(logPath))
        {
            return Ok(new { ok = true, total = 0, entries = Array.Empty<object>() });
        }

        var matches = new List<JsonNode>();
        foreach (var line in System.IO.File.ReadLines(logPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            JsonNode? entry;
            try
            {
                entry = JsonNode.Parse(line);
            }
            catch
            {
                // Tolerates a truncated/corrupt last line (e.g. a crash mid-append) — skip it
                // rather than failing the whole request. This is the payoff of JSONL over one big
                // JSON array.
                continue;
            }

            if (entry is null)
            {
                continue;
            }

            if (tplSet is { Count: > 0 })
            {
                var entryTpl = entry["tpl"]?.GetValue<string?>();
                if (entryTpl is null || !tplSet.Contains(entryTpl))
                {
                    continue;
                }
            }

            if (feature is not null && entry["feature"]?.GetValue<string?>() != feature)
            {
                continue;
            }

            if (action is not null && entry["action"]?.GetValue<string?>() != action)
            {
                continue;
            }

            if (since is not null || until is not null)
            {
                if (entry["ts"]?.GetValue<string?>() is not { } tsRaw || !DateTimeOffset.TryParse(tsRaw, out var ts))
                {
                    continue;
                }

                if (since is { } sinceVal && ts < sinceVal)
                {
                    continue;
                }

                if (until is { } untilVal && ts > untilVal)
                {
                    continue;
                }
            }

            matches.Add(entry);
        }

        matches.Reverse(); // newest first
        var page = matches.Skip(offset).Take(limit).ToList();

        return Ok(new { ok = true, total = matches.Count, entries = page });
    }
}
