using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Utils;   // JsonUtil

namespace TRLItemsManagement.Api;

/// <summary>
///     Browser-facing CRUD for the two trader-price override configs (<c>config/overrides.json</c> =
///     sell, <c>config/buy-overrides.json</c> = buy). Reads/writes the RAW file — no MongoId/Fence
///     validation here, unlike the boot-time <see cref="Pricing.TraderOverrideConfigParser"/> — this is
///     intentional (see the plan's "O que NÃO deduplicar" section): the operator editing the UI needs to
///     see exactly what is saved, including an entry the next boot would still reject.
///     <para>
///     Estágio 2 scope: GET only (round-trip the current file to the viewer). Write routes
///     (PATCH/DELETE, "kind"-parametrized to also cover buy) land in Estágio 3 alongside the
///     deduplicated <c>PRICE_KIND</c> frontend.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class TraderPriceController(ModPathsService modPaths, JsonUtil jsonUtil) : ControllerBase
{
    [HttpGet("trader-overrides")]
    public IActionResult GetTraderOverrides()
    {
        var path = Path.Combine(modPaths.ConfigDir, "overrides.json");
        return Ok(new { ok = true, overrides = ReadRawOverrides(path) });
    }

    [HttpGet("trader-buy-overrides")]
    public IActionResult GetTraderBuyOverrides()
    {
        var path = Path.Combine(modPaths.ConfigDir, "buy-overrides.json");
        return Ok(new { ok = true, overrides = ReadRawOverrides(path) });
    }

    /// <summary>
    ///     Tolerates a missing file (returns <c>{}</c>, same as the boot loader). A corrupt (unparseable)
    ///     file is backed up to a <c>.corrupt.bak</c> sibling and treated as <c>{}</c> so the viewer never
    ///     500s on a hand-edit gone wrong and the next write self-heals the config — mirrors
    ///     <c>serve.js</c>'s <c>readTraderOverrides</c>/<c>readBuyOverrides</c>.
    /// </summary>
    private Dictionary<string, Dictionary<string, object>> ReadRawOverrides(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return new Dictionary<string, Dictionary<string, object>>();
        }

        try
        {
            return jsonUtil.DeserializeFromFile<Dictionary<string, Dictionary<string, object>>>(path)
                ?? new Dictionary<string, Dictionary<string, object>>();
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

            return new Dictionary<string, Dictionary<string, object>>();
        }
    }
}
