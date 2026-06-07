using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route("launcher/mods")]
public class ModUpdaterController : ControllerBase
{
    private static string _manifestHash = string.Empty;
    private static object _manifestCache = null;
    private static bool _manifestGenerating = false;
    private static Dictionary<string, string> _fileMapCache = new();

    private static string GetUpdaterBasePath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Launcher-Updater");
    }

    private static string GetModsRepoPath() => Path.Combine(GetUpdaterBasePath(), "mods_repo");
    private static string GetOptionalsPath() => Path.Combine(GetUpdaterBasePath(), "Opcionais");

    [HttpGet("manifest-hash")]
    public IActionResult GetManifestHash()
    {
        if (string.IsNullOrEmpty(_manifestHash))
        {
            if (!_manifestGenerating) { _ = GenerateManifestAsync(); }
            return StatusCode(503, new { error = "Manifesto ainda sendo gerado" });
        }
        return Ok(new { hash = _manifestHash });
    }

    [HttpGet("manifest")]
    public IActionResult GetManifest()
    {
        if (_manifestCache == null)
        {
            if (!_manifestGenerating) { _ = GenerateManifestAsync(); }
            return StatusCode(503, new { error = "Manifesto ainda sendo gerado" });
        }
        return Ok(_manifestCache);
    }

    [HttpGet("download")]
    public IActionResult DownloadFile([FromQuery] string file)
    {
        if (string.IsNullOrEmpty(file))
        {
            return BadRequest(new { error = "Missing 'file'" });
        }

        var normalizedFile = file.Replace("\\", "/").TrimStart('/');
        
        if (_fileMapCache.TryGetValue(normalizedFile, out string physicalPath))
        {
            if (System.IO.File.Exists(physicalPath))
            {
                return PhysicalFile(physicalPath, "application/octet-stream");
            }
        }

        var fallbackPath = Path.Combine(GetModsRepoPath(), file.Replace("..", ""));
        if (System.IO.File.Exists(fallbackPath))
        {
            return PhysicalFile(fallbackPath, "application/octet-stream");
        }

        return NotFound(new { error = "File not found" });
    }

    [HttpGet("optionals-list")]
    public IActionResult GetOptionalsList()
    {
        var optsPath = GetOptionalsPath();
        if (!Directory.Exists(optsPath))
        {
            return Ok(new { folders = Array.Empty<string>() });
        }

        var folders = Directory.GetDirectories(optsPath).Select(Path.GetFileName).ToArray();
        return Ok(new { folders });
    }

    [HttpGet("optionals-manifest")]
    public IActionResult GetOptionalsManifest([FromQuery] string folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return BadRequest(new { error = "Missing 'folder'" });
        }

        var optsPath = GetOptionalsPath();
        var targetFolder = Path.Combine(optsPath, folder.Replace("..", ""));

        if (!Directory.Exists(targetFolder) || !targetFolder.StartsWith(optsPath))
        {
            return NotFound(new { error = "Folder not found" });
        }

        var filesList = new List<object>();
        var allFiles = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            var relPath = Path.GetRelativePath(targetFolder, file).Replace("\\", "/");
            var hash = GetFileHash(file);
            var size = new FileInfo(file).Length;
            filesList.Add(new { path = relPath, hash, size });
        }

        return Ok(new { folder, totalFiles = filesList.Count, files = filesList });
    }

    [HttpGet("optional-download")]
    public IActionResult DownloadOptional([FromQuery] string folder, [FromQuery] string file)
    {
        if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(file))
        {
            return BadRequest(new { error = "Missing params" });
        }

        var optsPath = GetOptionalsPath();
        var fullPath = Path.Combine(optsPath, folder.Replace("..", ""), file.Replace("..", ""));

        if (!fullPath.StartsWith(optsPath) || !System.IO.File.Exists(fullPath))
        {
            return NotFound(new { error = "File not found" });
        }

        return PhysicalFile(fullPath, "application/octet-stream");
    }

    [HttpGet("refresh")]
    public IActionResult Refresh()
    {
        _manifestCache = null;
        _manifestHash = string.Empty;
        _ = GenerateManifestAsync();

        return Ok(new { status = "OK", message = "Regenerando manifesto em background" });
    }

    private static string GetFileHash(string path)
    {
        using var md5 = MD5.Create();
        using var stream = System.IO.File.OpenRead(path);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static async Task GenerateManifestAsync()
    {
        if (_manifestGenerating) return;
        _manifestGenerating = true;

        try
        {
            var files = new List<object>();
            _fileMapCache.Clear();

            var modsPath = GetModsRepoPath();
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
            }

            var allFiles = Directory.GetFiles(modsPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                var relPath = Path.GetRelativePath(modsPath, file).Replace("\\", "/");
                var hash = GetFileHash(file);
                var size = new FileInfo(file).Length;

                files.Add(new { path = relPath, hash, size });
                _fileMapCache[relPath] = file;
            }

            var manifestObj = new
            {
                serverVersion = "1.4.0",
                generatedAt = DateTime.UtcNow.ToString("O"),
                totalFiles = files.Count,
                managedPaths = Array.Empty<string>(),
                deleteFiles = Array.Empty<string>(),
                ignoredFiles = Array.Empty<string>(),
                optionalGroups = Array.Empty<object>(),
                files = files
            };

            var json = JsonSerializer.Serialize(manifestObj);
            
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
            
            _manifestHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            _manifestCache = manifestObj;
        }
        catch (Exception)
        {
        }
        finally
        {
            _manifestGenerating = false;
        }
    }
}
