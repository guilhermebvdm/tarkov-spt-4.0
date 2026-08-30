using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route(ModRouting.RoutePrefix + "redline/launcher")]
public class CarouselController : ControllerBase
{
    private static string GetCarouselBasePath()
    {
        return Path.Combine(LauncherUpdaterController.GetUpdaterBasePath(), "carrocel");
    }

    [HttpGet("carousel")]
    public IActionResult GetCarouselImages()
    {
        try
        {
            string folder = GetCarouselBasePath();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var validExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
            var images = Directory.EnumerateFiles(folder)
                .Where(f => validExts.Contains(Path.GetExtension(f)))
                .Select(Path.GetFileName)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Ok(new { images });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("carousel/{fileName}")]
    public IActionResult GetCarouselImage(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { error = "Nome de arquivo inválido" });
        }

        string folder = Path.GetFullPath(GetCarouselBasePath());
        string safeName = Path.GetFileName(fileName);
        string filePath = Path.GetFullPath(Path.Combine(folder, safeName));

        // Anti-Path Traversal (CR-01-01)
        if (!filePath.StartsWith(folder, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(filePath))
        {
            return NotFound(new { error = "Imagem não encontrada" });
        }

        // Mapeamento de MIME type (CR-02-03)
        string contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType);
    }
}
