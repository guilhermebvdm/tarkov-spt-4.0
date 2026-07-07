using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

/// <summary>
///     Serves the mod's own catalog files (<c>data/*.json</c> — items/categories/traders/meta/hideout-
///     crafts, produced by the Node pipeline in <c>tools/trl-items-management/</c> and copied/refreshed
///     into <c>user/mods/TRL-ItemsManagement/data/</c>) to the browser viewer.
///     <para>
///     <c>data/</c> is deliberately OUTSIDE <c>wwwroot/</c> (which <c>compile-mod.sh</c> clobbers on
///     every deploy) so it survives a code redeploy — see the plan's "Layout de pastas" section.
///     </para>
///     <c>items.json</c> alone is ~17 MB; streamed straight from disk via <see cref="ControllerBase.PhysicalFile(string,string)"/>
///     rather than deserialized+reserialized (would burn CPU/memory on every request for no reason —
///     the file is already the exact JSON the browser wants).
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api/data")]
public sealed class DataController(ModPathsService modPaths) : ControllerBase
{
    // Fixed allowlist, not a generic path-traversal guard: the set of valid catalog files is small
    // and known — simpler AND safer than trying to sanitize an arbitrary "..\"-laden path.
    private static readonly HashSet<string> AllowedFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "items.json", "categories.json", "traders.json", "meta.json", "hideout-crafts.json",
    };

    [HttpGet("{fileName}")]
    public IActionResult GetDataFile(string fileName)
    {
        if (!AllowedFiles.Contains(fileName))
        {
            return NotFound(new { error = $"unknown data file: {fileName}" });
        }

        var path = Path.Combine(modPaths.DataDir, fileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound(new { error = $"{fileName} not found — run rescan/refresh-all first" });
        }

        return PhysicalFile(Path.GetFullPath(path), "application/json; charset=utf-8");
    }
}
