using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record SetFleaCapRequest(bool? Enabled);

/// <summary>
///     Toggles SPT's flea ceiling (<c>ragfair.json:dynamic.unreasonableModPrices</c> — Weapon Mod ×6 /
///     Electronics ×11 by default). Ported from <c>serve.js</c>'s <c>handleGetFleaCap</c>/
///     <c>handlePostFleaCap</c> — this is the C# reimplementation of B-1 (already shipped in the Node
///     viewer), NOT new functionality; <see cref="Api.FleaPriceController"/>'s floor/ceiling validation
///     reads the SAME <c>unreasonableModPrices</c> map this controller flips.
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class FleaCapController(
    SptDataPathsService sptPaths,
    WriteLockService writeLock,
    SptChecksService checksService,
    AuditLogService auditLog) : ControllerBase
{
    [HttpGet("flea-cap")]
    public IActionResult GetFleaCap()
    {
        var ragfairRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.RagfairConfigPath)) as JsonObject;
        var unreasonableModPrices = ragfairRoot?["dynamic"]?["unreasonableModPrices"] as JsonObject;

        var categories = new List<object>();
        var anyEnabled = false;
        if (unreasonableModPrices is not null)
        {
            foreach (var (tpl, node) in unreasonableModPrices)
            {
                if (node is not JsonObject cat)
                {
                    continue;
                }

                var enabled = cat["enabled"]?.GetValue<bool?>() ?? false;
                anyEnabled |= enabled;
                categories.Add(new
                {
                    tpl,
                    itemType = cat["itemType"]?.GetValue<string?>(),
                    overMult = cat["handbookPriceOverMultiplier"]?.GetValue<double?>(),
                    newMult = cat["newPriceHandbookMultiplier"]?.GetValue<double?>(),
                    enabled,
                });
            }
        }

        return Ok(new { ok = true, enabled = anyEnabled, categories });
    }

    [HttpPost("flea-cap")]
    public Task<IActionResult> SetFleaCap([FromBody] SetFleaCapRequest body)
    {
        if (body.Enabled is not { } enabled)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid body (expected { enabled: boolean })" }));
        }

        return writeLock.RunAsync(() =>
        {
            var ragfairRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.RagfairConfigPath)) as JsonObject;
            if (ragfairRoot?["dynamic"]?["unreasonableModPrices"] is not JsonObject unreasonableModPrices)
            {
                return Task.FromResult<IActionResult>(Ok(new { ok = true, enabled = false, changed = 0, note = "no unreasonableModPrices — no cap to toggle" }));
            }

            var changed = 0;
            foreach (var (_, node) in unreasonableModPrices)
            {
                if (node is not JsonObject cat)
                {
                    continue;
                }

                var current = cat["enabled"]?.GetValue<bool?>() ?? false;
                if (current != enabled)
                {
                    cat["enabled"] = enabled;
                    changed++;
                }
            }

            if (changed == 0)
            {
                // Every category is already in the requested state — no-op. Don't rewrite
                // ragfair.json or recompute checks.dat, and don't log. "Only touch disk on a real
                // mutation." (The write + checks + audit below all assumed changed > 0 anyway.)
                return Task.FromResult<IActionResult>(Ok(new
                {
                    ok = true,
                    enabled,
                    changed = 0,
                    note = "already in the requested state — nothing changed.",
                }));
            }

            StyleSensitiveJsonWriter.Write(sptPaths.RagfairConfigPath, ragfairRoot);
            var checksResult = checksService.Update("configs/ragfair.json");

            // before = the opposite of what we just set — this toggle is a simple boolean flip
            // applied uniformly to every category, so there's no per-category "previous" worth
            // logging individually; !enabled is exactly what "changed" categories held a moment
            // ago. Needed for the viewer's undo button (toggle back to before.enabled).
            auditLog.Append("flea-cap", "set", null, before: new { enabled = !enabled }, after: new { enabled }, extra: new { changed });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true,
                enabled,
                changed,
                checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
                note = "Restart SPT to apply in-game. Rebuild the catalog (or restart the viewer) to lift the editor cap in the UI.",
            }));
        });
    }
}
