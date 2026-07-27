using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record PatchStockRequest(string? Tpl, string? TraderId, double? Stock, int? BuyLimit, int? LoyaltyLevel, bool? Disabled);

public sealed record DeleteStockRequest(string? Tpl, string? TraderId);

/// <summary>
///     Browser-facing CRUD for the trader stock override config (<c>config/stock-overrides.json</c> =
///     <c>traderId → tpl → { stock?, buyLimit?, loyaltyLevel?, disabled? }</c>, B-6/B-11). Reads/writes the
///     RAW file — no MongoId/Fence validation here (format-only regex), same intentional split as
///     <see cref="TraderPriceController"/>: the operator sees exactly what is saved. The boot-time
///     <see cref="Pricing.StockApplier"/> mutates the live assort from this file — so, like the price
///     overrides, a save takes effect in-game on the next server restart. <c>stock</c> = lifetime
///     <c>StackObjectsCount</c> ceiling; <c>buyLimit</c> = per-refresh-cycle <c>BuyRestrictionMax</c>
///     allowance; <c>loyaltyLevel</c> = the <c>LoyalLevelItems</c> tier (1-4) that unlocks the offer.
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class StockController(ModPathsService modPaths, WriteLockService writeLock, AuditLogService auditLog) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);

    private string StockConfigPath => Path.Combine(modPaths.ConfigDir, "stock-overrides.json");

    // CR-B-06: tolerate type-mismatched hand-edits (e.g. a "3" string or 3.5 where an int is expected).
    // JsonNode.GetValue<T> throws on a mismatched JSON kind; RawConfigStore only auto-heals UNPARSEABLE
    // files, not bad value types, so an unguarded read would 500 the whole read-modify-write. Degrade to
    // null (treated as "no previous value") instead.
    private static double? ReadDouble(JsonObject o, string k) { try { return o[k]?.GetValue<double?>(); } catch { return null; } }
    private static int?    ReadInt(JsonObject o, string k)    { try { return o[k]?.GetValue<int?>(); }    catch { return null; } }
    private static bool?   ReadBool(JsonObject o, string k)   { try { return o[k]?.GetValue<bool?>(); }   catch { return null; } }

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

        // disabled:false carries no state (use DELETE to clear an override) — only disabled:true, or a
        // stock/buyLimit/loyaltyLevel edit, is a meaningful write.
        if (body.Disabled != true && body.Stock is null && body.BuyLimit is null && body.LoyaltyLevel is null)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "provide stock, buyLimit, loyaltyLevel and/or disabled:true" }));
        }

        if (body.Stock is { } s && (s < 0 || s != Math.Floor(s)))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid stock (expected non-negative integer)" }));
        }

        if (body.BuyLimit is { } bl && bl < 0)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid buyLimit (expected non-negative integer)" }));
        }

        // B-11: loyalty level is the trader tier (1-4) that unlocks the offer — SPT traders top out at LL4.
        if (body.LoyaltyLevel is { } lvl && (lvl < 1 || lvl > 4))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid loyaltyLevel (expected 1-4)" }));
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
                ? new { stock = ReadDouble(prev, "stock"), buyLimit = ReadInt(prev, "buyLimit"), loyaltyLevel = ReadInt(prev, "loyaltyLevel"), disabled = ReadBool(prev, "disabled") }
                : null;

            // Normalize (CR-U-01): a disabled offer has nothing to cap → store { disabled:true } alone;
            // otherwise store the stock/buyLimit/loyaltyLevel edits and never persist a disabled:false key
            // (inert noise).
            var entry = new JsonObject();
            if (body.Disabled == true)
            {
                entry["disabled"] = true;
            }
            else
            {
                if (body.Stock is { } st) entry["stock"] = st;
                if (body.BuyLimit is { } lim) entry["buyLimit"] = lim;
                if (body.LoyaltyLevel is { } lvl) entry["loyaltyLevel"] = lvl;
            }
            tplMap[tpl] = entry;

            RawConfigStore.Write(StockConfigPath, overrides);

            auditLog.Append(
                "trader-stock", "set", tpl,
                before: previous,
                // CR-B-05: log what was actually PERSISTED (the normalized entry), not the raw request —
                // e.g. a {disabled:true, loyaltyLevel:3} request stores only {disabled:true}, so the audit
                // must not claim a loyalty change that was dropped.
                after: new { stock = ReadDouble(entry, "stock"), buyLimit = ReadInt(entry, "buyLimit"), loyaltyLevel = ReadInt(entry, "loyaltyLevel"), disabled = ReadBool(entry, "disabled") },
                extra: new { traderId });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true, tpl, traderId, stock = body.Stock, buyLimit = body.BuyLimit, loyaltyLevel = body.LoyaltyLevel, disabled = body.Disabled,
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
            int? prevLoyalty = null;
            bool? prevDisabled = null;
            if (overrides[traderId] is JsonObject tplMap)
            {
                if (tplMap[tpl] is JsonObject prev)
                {
                    prevStock = ReadDouble(prev, "stock");
                    prevLimit = ReadInt(prev, "buyLimit");
                    prevLoyalty = ReadInt(prev, "loyaltyLevel");
                    prevDisabled = ReadBool(prev, "disabled");
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
                auditLog.Append("trader-stock", "delete", tpl, before: new { stock = prevStock, buyLimit = prevLimit, loyaltyLevel = prevLoyalty, disabled = prevDisabled }, extra: new { traderId });
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
