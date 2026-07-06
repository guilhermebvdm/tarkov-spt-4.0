using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement;

/// <summary>
///     B-2 spike: proves an ASP.NET controller in an SPT web mod answers a browser REST call on the
///     shared Kestrel (https://127.0.0.1:6969). Controllers (unlike SPT StaticRouter) support all verbs
///     incl. PATCH/DELETE and return uncompressed JSON — the right tool for the eventual TRL API port.
///     Route is under the AssemblyName prefix so it shares origin with the static UI (no CORS).
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class PingController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, mod = "TRLItemsManagement", stage = "b2-spike" });
}
