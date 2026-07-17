using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record RefreshAllRequest(string? Source);

/// <summary>
///     Bulk catalog rebuild — <c>POST /api/refresh-all</c> (re-fetch everything from one external
///     source + re-merge) and <c>POST /api/rescan</c> (re-read the local SPT database only, e.g. after
///     installing/removing a mod, without hitting any external API). Both just orchestrate the EXISTING
///     pipeline scripts via <c>Process.Start</c> — ported from <c>serve.js</c>'s <c>handleRefreshAll</c>/
///     <c>handleRescan</c>, no pipeline logic reimplemented in C# (same reasoning as
///     <see cref="ItemRefreshController"/>).
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class CatalogRebuildController(
    PipelineConfigService pipelineConfig,
    ModPathsService modPaths,
    WriteLockService writeLock,
    AuditLogService auditLog) : ControllerBase
{
    [HttpPost("refresh-all")]
    public Task<IActionResult> RefreshAll([FromBody] RefreshAllRequest body)
    {
        if (body.Source is not ("dev" or "market"))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "source must be 'dev' or 'market'" }));
        }

        var toolPath = pipelineConfig.ResolveToolPath();
        if (toolPath is null)
        {
            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                error = "config/pipeline.json missing/invalid — set { \"toolPath\": \"<absolute path to tools/trl-items-management on this machine>\" }",
            }));
        }

        var source = body.Source;
        var fetchScript = source == "market" ? "fetch-tarkov-market.js" : "fetch-tarkov-dev.js";

        return writeLock.RunAsync<IActionResult>(async () =>
        {
            var t0 = DateTime.UtcNow;

            var fetchResult = await NodeScriptRunner.RunAsync(toolPath, fetchScript, "--force");
            if (fetchResult.ExitCode != 0)
            {
                return StatusCode(502, new { error = $"{fetchScript} exited {fetchResult.ExitCode}: {fetchResult.StderrTail}" });
            }

            var loadResult = await NodeScriptRunner.RunAsync(toolPath, "load-spt.js");
            if (loadResult.ExitCode != 0)
            {
                return StatusCode(502, new { error = $"load-spt.js exited {loadResult.ExitCode}: {loadResult.StderrTail}" });
            }

            var normalizeResult = await NodeScriptRunner.RunAsync(toolPath, "normalize.js");
            if (normalizeResult.ExitCode != 0)
            {
                return StatusCode(502, new { error = $"normalize.js exited {normalizeResult.ExitCode}: {normalizeResult.StderrTail}" });
            }

            CatalogSync.SyncFromPipeline(toolPath, modPaths.DataDir);

            var itemCount = CountTopLevelKeys(Path.Combine(modPaths.DataDir, "items.json"));
            var durationMs = (int)(DateTime.UtcNow - t0).TotalMilliseconds;

            auditLog.Append("catalog-rebuild", "refresh-all", null, after: new { itemCount }, extra: new { source, durationMs });

            return Ok(new { ok = true, source, itemCount, durationMs });
        });
    }

    [HttpPost("rescan")]
    public Task<IActionResult> Rescan()
    {
        var toolPath = pipelineConfig.ResolveToolPath();
        if (toolPath is null)
        {
            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                error = "config/pipeline.json missing/invalid — set { \"toolPath\": \"<absolute path to tools/trl-items-management on this machine>\" }",
            }));
        }

        return writeLock.RunAsync<IActionResult>(async () =>
        {
            var t0 = DateTime.UtcNow;

            var loadResult = await NodeScriptRunner.RunAsync(toolPath, "load-spt.js");
            if (loadResult.ExitCode != 0)
            {
                return StatusCode(502, new { error = $"load-spt.js exited {loadResult.ExitCode}: {loadResult.StderrTail}" });
            }

            var normalizeResult = await NodeScriptRunner.RunAsync(toolPath, "normalize.js");
            if (normalizeResult.ExitCode != 0)
            {
                return StatusCode(502, new { error = $"normalize.js exited {normalizeResult.ExitCode}: {normalizeResult.StderrTail}" });
            }

            CatalogSync.SyncFromPipeline(toolPath, modPaths.DataDir);

            var itemsPath = Path.Combine(modPaths.DataDir, "items.json");
            var itemCount = CountTopLevelKeys(itemsPath);
            var modCount = CountModItems(itemsPath);
            var durationMs = (int)(DateTime.UtcNow - t0).TotalMilliseconds;

            auditLog.Append("catalog-rebuild", "rescan", null, after: new { itemCount, modCount }, extra: new { durationMs });

            return Ok(new { ok = true, itemCount, modCount, durationMs });
        });
    }

    private static int CountTopLevelKeys(string itemsJsonPath)
    {
        if (!System.IO.File.Exists(itemsJsonPath))
        {
            return 0;
        }

        return (JsonNode.Parse(System.IO.File.ReadAllText(itemsJsonPath)) as JsonObject)?.Count ?? 0;
    }

    private static int CountModItems(string itemsJsonPath)
    {
        if (JsonNode.Parse(System.IO.File.ReadAllText(itemsJsonPath)) is not JsonObject root)
        {
            return 0;
        }

        var count = 0;
        foreach (var (_, item) in root)
        {
            if (item?["modSource"] is not null)
            {
                count++;
            }
        }

        return count;
    }
}
