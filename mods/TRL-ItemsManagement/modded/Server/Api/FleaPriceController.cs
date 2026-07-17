using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Spt.Config;   // RagfairConfig
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace TRLItemsManagement.Api;

public sealed record SetFleaPriceRequest(string? Tpl, double? Price);

public sealed record DeleteFleaPriceRequest(string? Tpl);

/// <summary>
///     Flea (SPT ragfair) price overrides. Reads <c>SPT_Data/configs/ragfair.json</c> directly from
///     disk (not the DI-cached <c>ConfigServer</c> snapshot) so the viewer always shows the CURRENT
///     on-disk truth, including a write this same mod made earlier in this boot — mirrors
///     <c>serve.js</c>'s <c>handleGetOverrides</c>/<c>handlePatchPrice</c>/<c>handleDeletePrice</c>.
///     <para>
///     Two mutually exclusive write paths, gated on <c>item.modSource</c> (ported verbatim from
///     <c>serve.js</c>, see class-doc of each private helper below): vanilla items get an ADDITIVE
///     override (<c>itemPriceOverrideRouble</c>); mod items get a MULTIPLICATIVE factor
///     (<c>itemPriceMultiplier</c>) because the mod's own <c>CustomItemService</c> resets
///     <c>Prices[tpl]</c> AFTER the additive override would have applied, silently erasing it.
///     </para>
///     <para>
///     <b>Known interim gap (registered assumption, not silently dropped):</b> unlike <c>serve.js</c>,
///     this write does NOT resync <c>data/items.json</c>'s cached <c>consolidated</c>/<c>spt.*</c> view
///     for the edited tpl — that requires either porting <c>recomputeConsolidated</c> to C# (duplicates
///     the canonical price-priority logic that also lives in <c>normalize.js</c>, the exact lockstep risk
///     the plan flags) or a new Node script Process.Start call (deferred to the same follow-up as
///     Estágio 4's refresh/rescan wiring). The actual game-facing write (this file) is correct and
///     complete; the viewer's list will show a stale cached price for this tpl until the next manual
///     "rescan" — cosmetic, not a correctness bug in what the game applies.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class FleaPriceController(
    SptDataPathsService sptPaths,
    ModPathsService modPaths,
    JsonUtil jsonUtil,
    WriteLockService writeLock,
    SptChecksService checksService,
    AuditLogService auditLog) : ControllerBase
{
    private static readonly Regex TplPattern = new("^[a-f0-9]{24}$", RegexOptions.IgnoreCase);

    [HttpGet("overrides")]
    public IActionResult GetOverrides()
    {
        if (!System.IO.File.Exists(sptPaths.RagfairConfigPath))
        {
            return StatusCode(500, new { error = "ragfair.json not found" });
        }

        var config = jsonUtil.DeserializeFromFile<RagfairConfig>(sptPaths.RagfairConfigPath);
        var overridesRaw = config?.Dynamic?.ItemPriceOverrideRouble;

        // String-keyed rebuild (not a direct Dictionary<MongoId, double> return) — MongoId as a JSON
        // dictionary KEY needs a converter capable of WriteAsPropertyName, which ASP.NET's controller
        // serializer is not guaranteed to have registered (same reasoning as TraderBuyOverridesRouter).
        var overrides = new Dictionary<string, double>();
        if (overridesRaw is not null)
        {
            foreach (var (tpl, value) in overridesRaw)
            {
                overrides[tpl.ToString()] = value;
            }
        }

        return Ok(new { ok = true, overrides });
    }

    [HttpPost("price")]
    public Task<IActionResult> SetPrice([FromBody] SetFleaPriceRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id)" }));
        }

        if (body.Price is not { } priceRaw || priceRaw <= 0 || priceRaw != Math.Floor(priceRaw))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid price (expected positive integer)" }));
        }

        var price = priceRaw;

        return writeLock.RunAsync(() =>
        {
            var itemsCatalogPath = Path.Combine(modPaths.DataDir, "items.json");
            var itemsRoot = JsonNode.Parse(System.IO.File.ReadAllText(itemsCatalogPath)) as JsonObject;
            if (itemsRoot?[tpl] is not JsonObject item)
            {
                return Task.FromResult<IActionResult>(NotFound(new { error = "tpl not in data/items.json — run rescan/normalize first" }));
            }

            var sptBlock = item["spt"] as JsonObject;
            var modSource = item["modSource"]?.GetValue<string?>();

            var ragfairRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.RagfairConfigPath)) as JsonObject;
            var dynamicNode = ragfairRoot?["dynamic"] as JsonObject;
            if (dynamicNode is null)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "ragfair.json missing .dynamic" }));
            }

            if (!string.IsNullOrEmpty(modSource))
            {
                return Task.FromResult(SetModItemPrice(tpl, price, item, sptBlock, ragfairRoot!, dynamicNode, itemsRoot!, itemsCatalogPath));
            }

            return Task.FromResult(SetVanillaItemPrice(tpl, price, sptBlock, ragfairRoot!, dynamicNode, itemsRoot!, itemsCatalogPath));
        });
    }

    [HttpDelete("price")]
    public Task<IActionResult> DeletePrice([FromBody] DeleteFleaPriceRequest body)
    {
        if (body.Tpl is not { } tpl || !TplPattern.IsMatch(tpl))
        {
            return Task.FromResult<IActionResult>(BadRequest(new { error = "invalid tpl (expected 24-char hex BSG id)" }));
        }

        return writeLock.RunAsync(() =>
        {
            var ragfairRoot = JsonNode.Parse(System.IO.File.ReadAllText(sptPaths.RagfairConfigPath)) as JsonObject;
            var dynamicNode = ragfairRoot?["dynamic"] as JsonObject;
            if (dynamicNode is null)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, new { error = "ragfair.json missing .dynamic" }));
            }

            var overrideMap = dynamicNode["itemPriceOverrideRouble"] as JsonObject;
            var multiplierMap = dynamicNode["itemPriceMultiplier"] as JsonObject;
            var previousOverride = overrideMap?[tpl]?.GetValue<double?>();
            var previousMultiplier = multiplierMap?[tpl]?.GetValue<double?>();
            var hadOverride = overrideMap?.Remove(tpl) ?? false;
            var hadMultiplier = multiplierMap?.Remove(tpl) ?? false;
            var (effectiveFleaPrice, previousEffectiveFleaPrice) = ComputeDefaultEffectiveFleaPrice(tpl);

            if (!hadOverride && !hadMultiplier)
            {
                return Task.FromResult<IActionResult>(Ok(new { ok = true, tpl, removed = false, effectiveFleaPrice }));
            }

            StyleSensitiveJsonWriter.Write(sptPaths.RagfairConfigPath, ragfairRoot!);
            var checksResult = checksService.Update("configs/ragfair.json");

            PatchCatalogFleaReset(tpl, effectiveFleaPrice);

            // Reports both raw values (not a hadOverride ? ... : ... pick) — a tpl can, in principle,
            // have both an override AND a multiplier set at once (data anomaly, not the normal state);
            // the API response's removedOverride/removedMultiplier flags already show both, so the log
            // shouldn't silently drop one.
            auditLog.Append(
                "flea-price",
                "delete",
                tpl,
                before: new { previousOverride, previousMultiplier, effectiveFleaPrice = previousEffectiveFleaPrice },
                after: new { effectiveFleaPrice });

            return Task.FromResult<IActionResult>(Ok(new
            {
                ok = true,
                tpl,
                removed = true,
                removedOverride = hadOverride,
                removedMultiplier = hadMultiplier,
                effectiveFleaPrice,
                checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
            }));
        });
    }

    /// <summary>
    ///     Default (no override/multiplier) effective flea price for the viewer to show right after a
    ///     restore — same value <c>SetModItemPrice</c> lands on when its multiplier is 1 (<c>fleaBaseRaw</c>,
    ///     clamped to floor/ceiling): before any edit, the catalog's own <c>effectiveFleaPrice</c> already
    ///     equals <c>fleaBaseRaw</c> (verified against <c>data/items.json</c>), so this mirrors that baseline
    ///     without needing a Node round-trip. Reads the cached catalog (<c>data/items.json</c>), not
    ///     SPT_Data directly — same source <see cref="SetPrice"/> validates floor/ceiling against.
    ///     Pure compute, no side effect — called unconditionally, including the no-op "nothing to
    ///     remove" branch (which still needs this value for its response body), so it must never write
    ///     anything. See <see cref="PatchCatalogFleaReset"/> for the write, called only when a real
    ///     removal happened.
    ///     <para>
    ///     Also returns the cache's PRE-restore <c>effectiveFleaPrice</c> (same field read as the
    ///     <c>fleaBaseRaw</c> fallback below, just also handed back to the caller) — the audit log's
    ///     "before" value for a delete/restore, so the log can show what the price actually was, not
    ///     just the raw override/multiplier delta that was removed.
    ///     </para>
    /// </summary>
    private (double? Computed, double? PreviousCached) ComputeDefaultEffectiveFleaPrice(string tpl)
    {
        var itemsCatalogPath = Path.Combine(modPaths.DataDir, "items.json");
        if (!System.IO.File.Exists(itemsCatalogPath))
        {
            return (null, null);
        }

        var itemsRoot = JsonNode.Parse(System.IO.File.ReadAllText(itemsCatalogPath)) as JsonObject;
        if (itemsRoot?[tpl] is not JsonObject item || item["spt"] is not JsonObject sptBlock)
        {
            return (null, null);
        }

        var previousCached = sptBlock["effectiveFleaPrice"]?.GetValue<double?>();
        var baseVal = sptBlock["fleaBaseRaw"]?.GetValue<double?>() ?? previousCached;
        if (baseVal is not { } eff)
        {
            return (null, previousCached);
        }

        var floor = sptBlock["fleaFloor"]?.GetValue<double?>() ?? 0;
        var ceiling = sptBlock["fleaCeiling"]?.GetValue<double?>();
        if (eff < floor)
        {
            eff = floor;
        }

        if (ceiling is { } ceilingVal && eff > ceilingVal)
        {
            eff = ceilingVal;
        }

        return (eff, previousCached);
    }

    /// <summary>
    ///     Resets the tpl's cached <c>spt.fleaOverride</c>/<c>fleaOfferMultiplier</c>/
    ///     <c>effectiveFleaPrice</c> in <c>data/items.json</c> to match a restore — closes the "Known
    ///     interim gap" from the class doc (viewer showed a stale price/override badge until the next
    ///     manual rescan). Called only from <see cref="DeletePrice"/>'s real-removal branch (never the
    ///     no-op "nothing to remove" one) — same "only touch on an actual mutation" rule the audit log
    ///     entry right next to this call follows. Best-effort (try/catch): a cache-write failure here
    ///     must never turn the already-successful ragfair.json restore above into an error response.
    /// </summary>
    private void PatchCatalogFleaReset(string tpl, double? effectiveFleaPrice)
    {
        try
        {
            var itemsCatalogPath = Path.Combine(modPaths.DataDir, "items.json");
            if (!System.IO.File.Exists(itemsCatalogPath))
            {
                return;
            }

            var itemsRoot = JsonNode.Parse(System.IO.File.ReadAllText(itemsCatalogPath)) as JsonObject;
            if (itemsRoot?[tpl] is not JsonObject item || item["spt"] is not JsonObject sptBlock)
            {
                return;
            }

            sptBlock["fleaOverride"] = null;
            sptBlock["fleaOfferMultiplier"] = null;
            if (effectiveFleaPrice is { } eff)
            {
                sptBlock["effectiveFleaPrice"] = eff;
            }

            ItemCatalogPatcher.WriteBack(itemsCatalogPath, itemsRoot);
        }
        catch
        {
            // best-effort — see ItemCatalogPatcher's class doc
        }
    }

    /// <summary>
    ///     Vanilla-item branch: ADDITIVE override. The boot does
    ///     <c>Templates.Prices[tpl] = override</c> (assign) THEN <c>+= handbook×M</c> (AddOrUpdate), so
    ///     writing <c>override = desiredPrice − bonus</c> lands the in-game offer base on
    ///     <c>desiredPrice</c> — see <c>Pricing/SellPriceApplier.cs</c>'s doc-comment sibling note and
    ///     <c>docs/flea-formula-validation.md</c> in the Node tool for the source-level derivation.
    /// </summary>
    private IActionResult SetVanillaItemPrice(string tpl, double price, JsonObject? sptBlock, JsonObject ragfairRoot, JsonObject dynamicNode, JsonObject itemsRoot, string itemsCatalogPath)
    {
        var bonus = sptBlock?["fleaPrice"]?.GetValue<double?>();
        if (bonus is null)
        {
            return UnprocessableEntity(new { error = "no handbook-derived bonus for this tpl (mod item / not in handbook) — override unsupported" });
        }

        var floor = sptBlock?["fleaFloor"]?.GetValue<double?>() ?? 0;
        var ceiling = sptBlock?["fleaCeiling"]?.GetValue<double?>();

        if (price < floor)
        {
            return UnprocessableEntity(new
            {
                error = $"price {price} is below the flea floor {floor} (= handbook × trader buyback). The flea cannot go below this for this item.",
                floor,
            });
        }

        if (ceiling is { } ceilingVal && price > ceilingVal)
        {
            return UnprocessableEntity(new
            {
                error = $"price {price} is above the flea ceiling {ceilingVal} (Weapon Mod / Electronics are capped by SPT's unreasonableModPrices). The flea cannot exceed this for this item.",
                ceiling = ceilingVal,
            });
        }

        var overrideVal = price - bonus.Value;
        var overrideMap = dynamicNode["itemPriceOverrideRouble"] as JsonObject ?? new JsonObject();
        var previousOverride = overrideMap[tpl]?.GetValue<double?>();
        var previousEffectiveFleaPrice = sptBlock?["effectiveFleaPrice"]?.GetValue<double?>();
        overrideMap[tpl] = overrideVal;
        dynamicNode["itemPriceOverrideRouble"] = overrideMap;

        StyleSensitiveJsonWriter.Write(sptPaths.RagfairConfigPath, ragfairRoot);
        var checksResult = checksService.Update("configs/ragfair.json");

        try
        {
            sptBlock!["fleaOverride"] = overrideVal;
            sptBlock["fleaOfferMultiplier"] = null;
            sptBlock["effectiveFleaPrice"] = price;
            ItemCatalogPatcher.WriteBack(itemsCatalogPath, itemsRoot);
        }
        catch
        {
            // best-effort — see ItemCatalogPatcher's class doc; the ragfair.json write above already
            // succeeded, a cache-refresh failure here must not turn this into an error response.
        }

        auditLog.Append(
            "flea-price",
            "set",
            tpl,
            before: new { mode = "override", value = previousOverride, effectiveFleaPrice = previousEffectiveFleaPrice },
            after: new { mode = "override", value = overrideVal, effectiveFleaPrice = price });

        return Ok(new
        {
            ok = true,
            tpl,
            mode = "override",
            @override = overrideVal,
            previousOverride,
            effectiveFleaPrice = price,
            bonus = bonus.Value,
            floor,
            ceiling,
            checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
        });
    }

    /// <summary>
    ///     Mod-item branch: MULTIPLICATIVE factor, applied at offer-generation time BEFORE the ceiling —
    ///     see <c>Pricing/SellPriceApplier.cs</c>'s sibling notes and the plan's "Escritas fora da pasta
    ///     do mod" section for the full derivation (<c>multiplier = desiredPrice / fleaBaseRaw</c>).
    /// </summary>
    private IActionResult SetModItemPrice(string tpl, double price, JsonObject item, JsonObject? sptBlock, JsonObject ragfairRoot, JsonObject dynamicNode, JsonObject itemsRoot, string itemsCatalogPath)
    {
        var baseVal = sptBlock?["fleaBaseRaw"]?.GetValue<double?>() ?? sptBlock?["effectiveFleaPrice"]?.GetValue<double?>();
        var floor = sptBlock?["fleaFloor"]?.GetValue<double?>() ?? 0;
        var ceiling = sptBlock?["fleaCeiling"]?.GetValue<double?>();

        if (baseVal is not { } baseValNonNull || baseValNonNull <= 0)
        {
            var name = item["shortName"]?.GetValue<string?>() ?? tpl;
            return UnprocessableEntity(new { error = $"no flea base for \"{name}\" — multiplier undefined (run rescan/normalize)" });
        }

        if (ceiling is { } ceilingVal && price > ceilingVal)
        {
            return UnprocessableEntity(new
            {
                error = $"price {price} is above the ceiling {ceilingVal} (unreasonableModPrices: Weapon Mod ×6 / Electronics ×11). The multiplier is applied before the ceiling, so the flea cannot exceed this.",
                ceiling = ceilingVal,
            });
        }

        var multiplier = Math.Round(price / baseValNonNull * 1_000_000) / 1_000_000; // 6-decimal precision
        var multiplierMap = dynamicNode["itemPriceMultiplier"] as JsonObject ?? new JsonObject();
        var previousMultiplier = multiplierMap[tpl]?.GetValue<double?>();
        var previousEffectiveFleaPrice = sptBlock?["effectiveFleaPrice"]?.GetValue<double?>();
        multiplierMap[tpl] = multiplier;
        dynamicNode["itemPriceMultiplier"] = multiplierMap;

        StyleSensitiveJsonWriter.Write(sptPaths.RagfairConfigPath, ragfairRoot);
        var checksResult = checksService.Update("configs/ragfair.json");

        var eff = Math.Round(baseValNonNull * multiplier);
        if (eff < floor)
        {
            eff = floor;
        }

        if (ceiling is { } ceilingVal2 && eff > ceilingVal2)
        {
            eff = ceilingVal2;
        }

        try
        {
            sptBlock!["fleaOfferMultiplier"] = multiplier;
            sptBlock["fleaOverride"] = null;
            sptBlock["effectiveFleaPrice"] = eff;
            ItemCatalogPatcher.WriteBack(itemsCatalogPath, itemsRoot);
        }
        catch
        {
            // best-effort — see ItemCatalogPatcher's class doc
        }

        auditLog.Append(
            "flea-price",
            "set",
            tpl,
            before: new { mode = "multiplier", value = previousMultiplier, effectiveFleaPrice = previousEffectiveFleaPrice },
            after: new { mode = "multiplier", value = multiplier, effectiveFleaPrice = eff });

        return Ok(new
        {
            ok = true,
            tpl,
            mode = "multiplier",
            multiplier,
            previousMultiplier,
            @base = baseValNonNull,
            effectiveFleaPrice = eff,
            ceiling,
            checks = new { ok = checksResult.Ok, updated = checksResult.Updated },
        });
    }
}
