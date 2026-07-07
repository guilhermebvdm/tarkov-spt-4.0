using Microsoft.AspNetCore.Mvc;
using SPTarkov.Server.Core.Models.Spt.Config;   // RagfairConfig
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace TRLItemsManagement.Api;

/// <summary>
///     Flea (SPT ragfair) price overrides. Reads <c>SPT_Data/configs/ragfair.json</c> directly from
///     disk (not the DI-cached <c>ConfigServer</c> snapshot) so the viewer always shows the CURRENT
///     on-disk truth, including a write this same mod made earlier in this boot — mirrors
///     <c>serve.js</c>'s <c>handleGetOverrides</c>.
///     <para>
///     Estágio 2 scope: GET only. The write route (<c>POST /api/price</c>, with the vanilla-vs-mod-item
///     compensation-formula bifurcation, floor/ceiling validation, <c>checks.dat</c> recompute, and
///     catalog resync) lands in Estágio 3 — see the plan's "Escritas fora da pasta do mod" section.
///     </para>
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class FleaPriceController(SptDataPathsService sptPaths, JsonUtil jsonUtil) : ControllerBase
{
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
}
