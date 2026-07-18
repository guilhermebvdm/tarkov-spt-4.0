using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record SetFleaMinLevelRequest(int? MinUserLevel);

/// <summary>
///     Global flea unlock level (<c>SPT_Data/database/globals.json:config.RagFair.minUserLevel</c>) —
///     the only knob SPT exposes for this; there is no per-item flea-level concept in BSG data. Ported
///     from <c>serve.js</c>'s <c>handleFleaMinLevel</c>.
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class FleaLevelController(
    SptDataPathsService sptPaths,
    ModPathsService modPaths,
    WriteLockService writeLock,
    SptChecksService checksService,
    AuditLogService auditLog) : ControllerBase
{
    [HttpPost("flea-min-level")]
    public Task<IActionResult> SetFleaMinLevel([FromBody] SetFleaMinLevelRequest body)
    {
        if (body.MinUserLevel is not { } lvl || lvl < 1 || lvl > 99)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "minUserLevel must be integer 1..99" }));
        }

        return writeLock.RunAsync(() =>
        {
            var globalsRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.GlobalsPath)) as JsonObject;
            if (globalsRoot?["config"]?["RagFair"] is not JsonObject ragFair)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "globals.json missing config.RagFair" }));
            }

            var previous = ragFair["minUserLevel"]?.GetValue<int?>();
            if (previous == lvl)
            {
                // Already at this level — no-op. Don't rewrite globals.json, recompute checks.dat,
                // mirror meta, or log. "Only touch disk on a real mutation."
                return Task.FromResult<IActionResult>(Ok(new
                {
                    ok = true, minUserLevel = lvl, previous, changed = false,
                }));
            }

            ragFair["minUserLevel"] = lvl;

            StyleSensitiveJsonWriter.Write(sptPaths.GlobalsPath, globalsRoot);
            var checksResult = checksService.Update("database/globals.json");

            MirrorIntoMeta(lvl);

            auditLog.Append("flea-min-level", "set", null, before: new { minUserLevel = previous }, after: new { minUserLevel = lvl });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true,
                minUserLevel = lvl,
                previous,
                checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
            }));
        });
    }

    /// <summary>
    ///     Best-effort mirror into <c>data/meta.json</c> so the viewer's cached META reflects the new
    ///     value on next load, without requiring a full rescan. Failure here is non-fatal (matches
    ///     <c>serve.js</c> — a small display-cache miss, never worth failing the actual config write over).
    /// </summary>
    private void MirrorIntoMeta(int lvl)
    {
        try
        {
            var metaPath = Path.Combine(modPaths.DataDir, "meta.json");
            var metaRoot = JsonNode.Parse(System.IO.File.ReadAllText(metaPath)) as JsonObject;
            if (metaRoot?["sources"]?["spt"] is JsonObject sptMeta)
            {
                sptMeta["fleaMinUserLevel"] = lvl;
                var serialized = metaRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
                System.IO.File.WriteAllText(metaPath, serialized);
            }
        }
        catch
        {
            // Best-effort only — the meta.json cache mirror is a display convenience, not load-bearing.
        }
    }
}
