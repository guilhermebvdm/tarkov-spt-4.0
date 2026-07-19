using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record PatchStockRequest(string? Tpl, string? TraderId, double? Stock, int? BuyLimit);

public sealed record DeleteStockRequest(string? Tpl, string? TraderId);

/// <summary>
///     Browser-facing CRUD for the trader stock override config (<c>config/stock-overrides.json</c> =
///     <c>traderId → tpl → { stock?, buyLimit? }</c>, B-6). Reads/writes the RAW file — no MongoId/Fence
///     validation here (format-only regex), same intentional split as <see cref="TraderPriceController"/>:
///     the operator sees exactly what is saved. The boot-time <see cref="Pricing.StockApplier"/> mutates
///     the live assort from this file — so, like the price overrides, a save takes effect in-game on the
///     next server restart. <c>stock</c> = lifetime <c>StackObjectsCount</c> ceiling; <c>buyLimit</c> =
///     per-refresh-cycle <c>BuyRestrictionMax</c> allowance.
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class StockController(ModPathsService modPaths, WriteLockService writeLock, AuditLogService auditLog) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);

    private string StockConfigPath => Path.Combine(modPaths.ConfigDir, "stock-overrides.json");

    [HttpGet("trader-stock-overrides")]
    public IActionResult GetStockOverrides() => Ok(new { ok = true, overrides = RawConfigStore.ReadForResponse(StockConfigPath) });

    [HttpPatch("trader-stock")]
    public Task<IActionResult> PatchStock([FromBody] PatchStockRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id)" }));
        }

        if (body.TraderId is not { } traderId || !TplPattern.IsMatch(traderId))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid traderId (expected 24-char hex MongoId)" }));
        }

        if (body.Stock is null && body.BuyLimit is null)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "provide stock and/or buyLimit" }));
        }

        if (body.Stock is { } s && (s < 0 || s != Math.Floor(s)))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid stock (expected non-negative integer)" }));
        }

        if (body.BuyLimit is { } bl && bl < 0)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid buyLimit (expected non-negative integer)" }));
        }

        return writeLock.RunAsync(() =>
        {
            var overrides = RawConfigStore.ReadForWrite(StockConfigPath);
            if (overrides[traderId] is not JsonObject tplMap)
            {
                tplMap = new JsonObject();
                overrides[traderId] = tplMap;
            }

            var previous = tplMap[tpl] is JsonObject prev
                ? new { stock = prev["stock"]?.GetValue<double?>(), buyLimit = prev["buyLimit"]?.GetValue<int?>() }
                : null;

            var entry = new JsonObject();
            if (body.Stock is { } st) entry["stock"] = st;
            if (body.BuyLimit is { } lim) entry["buyLimit"] = lim;
            tplMap[tpl] = entry;

            RawConfigStore.Write(StockConfigPath, overrides);

            auditLog.Append(
                "trader-stock", "set", tpl,
                before: previous,
                after: new { stock = body.Stock, buyLimit = body.BuyLimit },
                extra: new { traderId });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true, tpl, traderId, stock = body.Stock, buyLimit = body.BuyLimit,
                note = "Restart SPT to apply in-game.",
            }));
        });
    }

    [HttpDelete("trader-stock")]
    public Task<IActionResult> DeleteStock([FromBody] DeleteStockRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id)" }));
        }

        if (body.TraderId is not { } traderId || !TplPattern.IsMatch(traderId))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid traderId (expected 24-char hex MongoId)" }));
        }

        return writeLock.RunAsync(() =>
        {
            var overrides = RawConfigStore.ReadForWrite(StockConfigPath);
            var removed = false;
            double? prevStock = null;
            int? prevLimit = null;
            if (overrides[traderId] is JsonObject tplMap)
            {
                if (tplMap[tpl] is JsonObject prev)
                {
                    prevStock = prev["stock"]?.GetValue<double?>();
                    prevLimit = prev["buyLimit"]?.GetValue<int?>();
                }

                removed = tplMap.Remove(tpl);
                if (tplMap.Count == 0)
                {
                    overrides.Remove(traderId);
                }
            }

            if (removed)
            {
                RawConfigStore.Write(StockConfigPath, overrides);
                auditLog.Append("trader-stock", "delete", tpl, before: new { stock = prevStock, buyLimit = prevLimit }, extra: new { traderId });
            }

            return Task.FromResult<IActionResult>(Ok(new { ok = true, tpl, traderId, removed, note = "Restart SPT to apply in-game." }));
        });
    }

    [HttpDelete("trader-stock/all")]
    public Task<IActionResult> DeleteAllStock()
    {
        return writeLock.RunAsync(() =>
        {
            var before = RawConfigStore.ReadForWrite(StockConfigPath);
            var cleared = before.Count > 0;
            if (cleared)
            {
                RawConfigStore.Write(StockConfigPath, new JsonObject());
                auditLog.Append("trader-stock", "delete-all", null, before: before);
            }

            return Task.FromResult<IActionResult>(Ok(new { ok = true, cleared }));
        });
    }

    // Raw config I/O is shared with TraderPriceController via RawConfigStore (code-review CR-03).
}
