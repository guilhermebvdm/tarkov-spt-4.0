using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record RefreshItemRequest(string? Tpl);

/// <summary>
///     Per-item re-fetch from tarkov.dev / tarkov-market. Ported from <c>serve.js</c>'s
///     <c>handleRefreshDev</c>/<c>handleRefreshMarket</c> — but unlike the Node original (which fetches
///     in-process via raw <c>https.request</c>), this runs <c>scripts/refresh-item.js</c> via
///     <c>Process.Start</c>: a C# reimplementation of tarkov.dev's GraphQL query + the canonical
///     price-priority recompute would duplicate logic that also has to stay in lockstep with
///     <c>normalize.js</c> — the exact risk the plan flags for keeping the data pipeline in Node.
///     <para>
///     Response contract MUST match the frontend exactly (<c>refreshDevCell</c>/<c>refreshMarketCell</c>
///     in wwwroot/index.html read <c>data.previous.avg24h</c>, <c>data.tarkovDev</c>/<c>data.tarkovMarket</c>,
///     <c>data.consolidated</c> as top-level fields — NOT a wrapped "item" object) — captured BEFORE
///     running the script (the script overwrites the on-disk "before" state), then re-read after.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class ItemRefreshController(
    PipelineConfigService pipelineConfig,
    ModPathsService modPaths,
    WriteLockService writeLock) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);

    [HttpPost("refresh-dev")]
    public Task<IActionResult> RefreshDev([FromBody] RefreshItemRequest body) => Refresh("dev", body);

    [HttpPost("refresh-market")]
    public Task<IActionResult> RefreshMarket([FromBody] RefreshItemRequest body) => Refresh("market", body);

    private Task<IActionResult> Refresh(string source, RefreshItemRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl" }));
        }

        var toolPath = pipelineConfig.ResolveToolPath();
        if (toolPath is null)
        {
            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                error = "config/pipeline.json missing/invalid — set { \"toolPath\": \"<absolute path to tools/trl-items-management on this machine>\" }",
            }));
        }

        var sourceField = source == "market" ? "tarkovMarket" : "tarkovDev";

        return writeLock.RunAsync<IActionResult>(async () =>
        {
            var itemsPath = Path.Combine(modPaths.DataDir, "items.json");

            var beforeRoot = JsonNode.Parse(System.IO.File.ReadAllText(itemsPath)) as JsonObject;
            if (beforeRoot?[tpl] is not JsonObject beforeItem)
            {
                return NotFound(new { error = "tpl not in data/items.json" });
            }

            // "previous" = the narrow {lastLow,avg24h,low24h,high24h,updated} pve snapshot BEFORE this
            // refresh — the script overwrites the on-disk state, so this MUST be captured first.
            var previous = beforeItem[sourceField]?["pve"];

            var result = await NodeScriptRunner.RunAsync(toolPath, "refresh-item.js", "--source", source, "--tpl", tpl);
            if (result.ExitCode != 0)
            {
                return StatusCode(502, new { error = $"refresh-item.js exited {result.ExitCode}: {result.StderrTail}" });
            }

            CatalogSync.SyncFromPipeline(toolPath, modPaths.DataDir);

            var afterRoot = JsonNode.Parse(System.IO.File.ReadAllText(itemsPath)) as JsonObject;
            if (afterRoot?[tpl] is not JsonObject afterItem)
            {
                return NotFound(new { error = "tpl not found in data/items.json after refresh" });
            }

            var response = new JsonObject
            {
                ["ok"] = true,
                ["tpl"] = tpl,
                ["previous"] = previous?.DeepClone(),
                [sourceField] = afterItem[sourceField]?.DeepClone(),
                ["consolidated"] = afterItem["consolidated"]?.DeepClone(),
            };

            return new ContentResult
            {
                Content = response.ToJsonString(),
                ContentType = "application/json; charset=utf-8",
                StatusCode = 200,
            };
        });
    }
}
