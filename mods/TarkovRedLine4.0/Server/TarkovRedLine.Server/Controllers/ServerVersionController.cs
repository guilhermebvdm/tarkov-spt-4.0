using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route("redline/server")]
public class ServerVersionController : ControllerBase
{
    /// <summary>
    /// Fallback used when Launcher-Updater/server-version.txt is missing or empty.
    /// </summary>
    internal const string DefaultServerVersion = "0.1.0-beta";

    private const string VersionFileName = "server-version.txt";

    /// <summary>
    /// GET /redline/server/version → { "version": "0.1.0-beta" }
    /// Reads the TRL server version from Launcher-Updater/server-version.txt (trimmed).
    /// Not to be confused with /launcher/server/version (SPT core version).
    /// </summary>
    [HttpGet("version")]
    public IActionResult GetServerVersion()
    {
        string version = DefaultServerVersion;

        try
        {
            string versionFilePath = Path.Combine(
                LauncherUpdaterController.GetUpdaterBasePath(),
                VersionFileName);

            if (System.IO.File.Exists(versionFilePath))
            {
                string fileContent = System.IO.File.ReadAllText(versionFilePath).Trim();
                if (!string.IsNullOrEmpty(fileContent))
                {
                    version = fileContent;
                }
            }
        }
        catch
        {
            // Unreadable file (permissions/IO) → fall back to the embedded default
        }

        return Ok(new { version });
    }
}
