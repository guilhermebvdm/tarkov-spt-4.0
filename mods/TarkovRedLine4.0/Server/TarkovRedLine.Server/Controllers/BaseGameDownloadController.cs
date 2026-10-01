using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route(ModRouting.RoutePrefix + "redline")]
public class BaseGameDownloadController : ControllerBase
{
    private static string GetBaseClientPath()
    {
        return Path.Combine(LauncherUpdaterController.GetUpdaterBasePath(), "base-client");
    }

    private static string GetTorrentFilePath()
    {
        return Path.Combine(LauncherUpdaterController.GetUpdaterBasePath(), "base-game.torrent");
    }

    [HttpGet("base-game.torrent")]
    public IActionResult GetBaseGameTorrent()
    {
        string torrentPath = GetTorrentFilePath();
        if (!System.IO.File.Exists(torrentPath))
        {
            return NotFound(new { error = "Torrent do jogo base ainda não foi gerado no servidor." });
        }

        return PhysicalFile(torrentPath, "application/x-bittorrent", "base-game.torrent");
    }

    [HttpGet("base-game/{*relativePath}")]
    public IActionResult StreamBaseGameFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return BadRequest(new { error = "Caminho relativo inválido" });
        }

        string baseDir = Path.GetFullPath(GetBaseClientPath());
        string cleanRelative = relativePath.TrimStart('/', '\\');

        // Se o cliente MonoTorrent enviar com prefixo do torrent ('base-client/' ou 'SPT/'), remove
        if (cleanRelative.StartsWith("base-client/", StringComparison.OrdinalIgnoreCase))
        {
            cleanRelative = cleanRelative.Substring("base-client/".Length);
        }
        else if (cleanRelative.StartsWith("base-client\\", StringComparison.OrdinalIgnoreCase))
        {
            cleanRelative = cleanRelative.Substring("base-client\\".Length);
        }
        else if (cleanRelative.StartsWith("SPT/", StringComparison.OrdinalIgnoreCase))
        {
            cleanRelative = cleanRelative.Substring("SPT/".Length);
        }
        else if (cleanRelative.StartsWith("SPT\\", StringComparison.OrdinalIgnoreCase))
        {
            cleanRelative = cleanRelative.Substring("SPT\\".Length);
        }

        string fullPath = Path.GetFullPath(Path.Combine(baseDir, cleanRelative));

        // Anti-Path Traversal (CR-01-01)
        if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
        {
            return NotFound(new { error = "Arquivo não encontrado", path = cleanRelative });
        }

        // Suporte HTTP Range 206 nativo do ASP.NET Core (CR-01-02)
        return PhysicalFile(fullPath, "application/octet-stream", enableRangeProcessing: true);
    }
}
