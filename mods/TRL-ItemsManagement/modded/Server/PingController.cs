using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement;

/// <summary>
///     Estágio 0 gate: proves the rename (AssemblyName/GUID/namespace) landed correctly before any real
///     pricing/UI logic is ported. Removed once the Api/ controllers exist (Estágio 3) — this is
///     scaffolding, not part of the final layout.
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api")]
public sealed class PingController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, mod = "TRLItemsManagement", stage = "0-skeleton" });
}
