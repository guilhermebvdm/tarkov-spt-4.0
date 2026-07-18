using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

public sealed record PatchTraderPriceRequest(string? Tpl, string? TraderId, int? Count, string? Currency);

public sealed record DeleteTraderPriceRequest(string? Tpl, string? TraderId);

/// <summary>
///     Browser-facing CRUD for the two trader-price override configs (<c>config/overrides.json</c> =
///     sell, <c>config/buy-overrides.json</c> = buy). Reads/writes the RAW file — no MongoId/Fence
///     validation here, unlike the boot-time <see cref="Pricing.TraderOverrideConfigParser"/> — this is
///     intentional (see the plan's "O que NÃO deduplicar" section): the operator editing the UI needs to
///     see exactly what is saved, including an entry the next boot would still reject.
///     <para>
///     Sell and buy routes share the same private helpers (parametrized by config file path), the C#-side
///     equivalent of the frontend's <c>PRICE_KIND</c> dedup — no MongoId type needed here (format-only
///     regex validation), so this stays a single controller rather than a "kind" enum/generic.
///     </para>
///     <para>
///     No <c>checks.dat</c> recompute here — these two files live under the mod's own
///     <c>user/mods/TRL-ItemsManagement/config/</c>, outside <c>SPT_Data/</c>, so they are not part of
///     SPT's integrity manifest (same as <c>serve.js</c>).
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class TraderPriceController(ModPathsService modPaths, WriteLockService writeLock, AuditLogService auditLog) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase) { "RUB", "USD", "EUR", "GP" };

    private string SellConfigPath => Path.Combine(modPaths.ConfigDir, "overrides.json");
    private string BuyConfigPath => Path.Combine(modPaths.ConfigDir, "buy-overrides.json");

    [HttpGet("trader-overrides")]
    public IActionResult GetTraderOverrides() => Ok(new { ok = true, overrides = ReadRawOverridesForResponse(SellConfigPath) });

    [HttpGet("trader-buy-overrides")]
    public IActionResult GetTraderBuyOverrides() => Ok(new { ok = true, overrides = ReadRawOverridesForResponse(BuyConfigPath) });

    [HttpPatch("trader-price")]
    public Task<IActionResult> PatchTraderPrice([FromBody] PatchTraderPriceRequest body) => PatchPrice(SellConfigPath, "trader-sell-price", body);

    [HttpDelete("trader-price/all")]
    public Task<IActionResult> DeleteAllTraderPrices() => DeleteAll(SellConfigPath, "trader-sell-price");

    [HttpDelete("trader-price")]
    public Task<IActionResult> DeleteTraderPrice([FromBody] DeleteTraderPriceRequest body) => DeleteOne(SellConfigPath, "trader-sell-price", body);

    [HttpPatch("trader-buy-price")]
    public Task<IActionResult> PatchTraderBuyPrice([FromBody] PatchTraderPriceRequest body) => PatchPrice(BuyConfigPath, "trader-buy-price", body);

    [HttpDelete("trader-buy-price/all")]
    public Task<IActionResult> DeleteAllTraderBuyPrices() => DeleteAll(BuyConfigPath, "trader-buy-price");

    [HttpDelete("trader-buy-price")]
    public Task<IActionResult> DeleteTraderBuyPrice([FromBody] DeleteTraderPriceRequest body) => DeleteOne(BuyConfigPath, "trader-buy-price", body);

    private Task<IActionResult> PatchPrice(string configPath, string feature, PatchTraderPriceRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id)" }));
        }

        if (body.TraderId is not { } traderId || !TplPattern.IsMatch(traderId))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid traderId (expected 24-char hex MongoId)" }));
        }

        if (body.Count is not { } count || count <= 0)
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid count (expected positive integer)" }));
        }

        if (body.Currency is not { } currency || !ValidCurrencies.Contains(currency))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid currency" }));
        }

        return writeLock.RunAsync(() =>
        {
            var overrides = ReadRawOverridesForWrite(configPath);
            if (overrides[traderId] is not JsonObject tplMap)
            {
                tplMap = new JsonObject();
                overrides[traderId] = tplMap;
            }

            var previousCount = tplMap[tpl]?["count"]?.GetValue<double?>();
            var previousCurrency = tplMap[tpl]?["currency"]?.GetValue<string?>();
            if (previousCount == count && string.Equals(previousCurrency, currency, StringComparison.OrdinalIgnoreCase))
            {
                // Already at this exact override — no-op. Don't rewrite the config or log a
                // spurious "X → X" entry. "Only touch disk on a real mutation."
                return Task.FromResult<IActionResult>(Ok(new
                {
                    ok = true, tpl, traderId, count, currency, previousPrice = previousCount, changed = false,
                }));
            }

            tplMap[tpl] = new JsonObject { ["count"] = count, ["currency"] = currency };

            WriteRawOverrides(configPath, overrides);

            auditLog.Append(
                feature,
                "set",
                tpl,
                before: new { count = previousCount, currency = previousCurrency },
                after: new { count, currency },
                extra: new { traderId });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true,
                tpl,
                traderId,
                count,
                currency,
                previousPrice = previousCount,
            }));
        });
    }

    private Task<IActionResult> DeleteOne(string configPath, string feature, DeleteTraderPriceRequest body)
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
            var overrides = ReadRawOverridesForWrite(configPath);
            var removed = false;
            double? previousCount = null;
            string? previousCurrency = null;
            if (overrides[traderId] is JsonObject tplMap)
            {
                previousCount = tplMap[tpl]?["count"]?.GetValue<double?>();
                previousCurrency = tplMap[tpl]?["currency"]?.GetValue<string?>();
                removed = tplMap.Remove(tpl);
                if (tplMap.Count == 0)
                {
                    overrides.Remove(traderId);
                }
            }

            if (removed)
            {
                WriteRawOverrides(configPath, overrides);
                auditLog.Append(feature, "delete", tpl, before: new { count = previousCount, currency = previousCurrency }, extra: new { traderId });
            }

            return Task.FromResult<IActionResult>(Ok(new { ok = true, tpl, traderId, removed }));
        });
    }

    private Task<IActionResult> DeleteAll(string configPath, string feature)
    {
        return writeLock.RunAsync(() =>
        {
            var before = ReadRawOverridesForWrite(configPath);
            WriteRawOverrides(configPath, new JsonObject());
            if (before.Count > 0)
            {
                auditLog.Append(feature, "delete-all", null, before: before);
            }

            return Task.FromResult<IActionResult>(Ok(new { ok = true, cleared = true }));
        });
    }

    /// <summary>Same tolerate-missing / self-heal-corrupt behavior as the boot loader, for a plain response DTO.</summary>
    private Dictionary<string, Dictionary<string, JsonElement>> ReadRawOverridesForResponse(string path)
    {
        var root = ReadRawOverridesForWrite(path);
        var result = new Dictionary<string, Dictionary<string, JsonElement>>();
        foreach (var (traderId, tplMapNode) in root)
        {
            if (tplMapNode is not JsonObject tplMap)
            {
                continue;
            }

            var inner = new Dictionary<string, JsonElement>();
            foreach (var (tpl, ovr) in tplMap)
            {
                if (ovr is not null)
                {
                    inner[tpl] = JsonSerializer.Deserialize<JsonElement>(ovr.ToJsonString());
                }
            }

            result[traderId] = inner;
        }

        return result;
    }

    /// <summary>
    ///     Tolerates a missing file (returns an empty object, same as the boot loader). A corrupt
    ///     (unparseable) file is backed up to a <c>.corrupt.bak</c> sibling and treated as empty so the
    ///     viewer never 500s on a hand-edit gone wrong and the next write self-heals the config — mirrors
    ///     <c>serve.js</c>'s <c>readTraderOverrides</c>/<c>readBuyOverrides</c>.
    /// </summary>
    private JsonObject ReadRawOverridesForWrite(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(System.IO.File.ReadAllText(path)) as JsonObject ?? new JsonObject();
        }
        catch
        {
            try
            {
                var backup = path + ".corrupt.bak";
                if (System.IO.File.Exists(backup))
                {
                    System.IO.File.Delete(backup);
                }

                System.IO.File.Move(path, backup);
            }
            catch
            {
                // Best-effort backup — even if it fails, still fall through and serve {} below
                // rather than 500ing the viewer over a config file it can't read either way.
            }

            return new JsonObject();
        }
    }

    /// <summary>Atomic (tmp+rename) write. This file is owned entirely by the mod (not shipped/copied),
    /// so — unlike SPT_Data files — there's no "original style" to preserve; always 2-space + trailing \n.</summary>
    private static void WriteRawOverrides(string path, JsonObject overrides)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var serialized = overrides.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
        var tmp = path + ".tmp";
        System.IO.File.WriteAllText(tmp, serialized);
        System.IO.File.Move(tmp, path, overwrite: true);
    }
}
