using Microsoft.AspNetCore.Mvc;

namespace TRLItemsManagement.Api;

/// <summary>
///     Proxies SPT's own image assets (trader avatars, handbook/category icons — item icons themselves
///     are external <c>assets.tarkov.dev</c> URLs, never local) so the browser viewer can show them
///     without reaching into <c>SPT_Data/</c> directly. Mirrors the Node viewer's <c>/spt-images/*</c>
///     route, mounted under the same full prefix as every other controller in this mod (confirmed
///     empirically: unlike <c>wwwroot/</c> static files, SPTWeb does NOT auto-prefix ASP.NET controller
///     routes with the assembly name — each controller bakes <c>TRLItemsManagement-Server/</c> into its
///     own <c>[Route]</c>, same as <see cref="PingController"/>/<see cref="DataController"/>).
/// </summary>
[ApiController]
[Route("TRLItemsManagement-Server/api/spt-images")]
public sealed class ImagesController(SptDataPathsService sptPaths) : ControllerBase
{
    private static readonly Dictionary<string, string> MimeByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
    };

    [HttpGet("{**path}")]
    public IActionResult GetImage(string path)
    {
        if (!TryResolveImagePath(path, out var fullPath))
        {
            return BadRequest(new { error = "invalid image path" });
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound(new { error = $"image not found: {path}" });
        }

        var ext = Path.GetExtension(fullPath);
        var contentType = MimeByExtension.GetValueOrDefault(ext, "application/octet-stream");
        Response.Headers.CacheControl = "public, max-age=3600";
        return PhysicalFile(fullPath, contentType);
    }

    /// <summary>
    ///     <paramref name="requestedPath"/> comes straight from the client — resolve then verify the
    ///     canonical result still lives under <see cref="SptDataPathsService.ImagesDir"/> before
    ///     touching disk (mirrors the Node viewer's own guard: join, then check the prefix survived).
    ///     A bare-filename check (like ClassEditorService's) is too strict here — legitimate requests
    ///     carry subdirectories (<c>trader/avatar/&lt;id&gt;.png</c>, <c>handbook/&lt;icon&gt;.png</c>).
    /// </summary>
    private bool TryResolveImagePath(string requestedPath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(requestedPath))
        {
            return false;
        }

        var baseDir = Path.GetFullPath(sptPaths.ImagesDir);
        var candidate = Path.GetFullPath(Path.Combine(baseDir, requestedPath));

        var baseWithSeparator = baseDir.EndsWith(Path.DirectorySeparatorChar)
            ? baseDir
            : baseDir + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(baseWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = candidate;
        return true;
    }
}
