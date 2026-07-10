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
[Route(ModRouting.RoutePrefix + "launcher/mods")]
public class ModUpdaterController : ControllerBase
{
    private static string _manifestHash = string.Empty;
    private static object _manifestCache = null;
    private static bool _manifestGenerating = false;
    // ref: CR-01-06 — case-insensitive lookups: manifest-path casing may differ from the
    // client's request casing (merge preserves base casing); on a case-sensitive host the
    // miss would 404 instead of falling back.
    private static Dictionary<string, string> _fileMapCache = new(StringComparer.OrdinalIgnoreCase);
    // Item 008: performance overlay pack (Launcher-Updater/config-performance) — rel path -> physical path.
    private static Dictionary<string, string> _performanceFileMapCache = new(StringComparer.OrdinalIgnoreCase);

    private static string GetUpdaterBasePath()
    {
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // Procurar a pasta Launcher-Updater subindo até 4 níveis
        for (int i = 0; i < 4; i++)
        {
            string testPath = Path.Combine(currentDir, ModRouting.UpdaterFolderName);
            if (Directory.Exists(testPath))
            {
                return Path.GetFullPath(testPath);
            }
            
            string parent = Path.GetDirectoryName(currentDir);
            if (string.IsNullOrEmpty(parent) || parent == currentDir) break;
            currentDir = parent;
        }

        // Fallback (o que estava antes) caso não exista ainda
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ModRouting.UpdaterFolderName));
    }

    private static string GetModsRepoPath() => Path.Combine(GetUpdaterBasePath(), "mods_repo");
    private static string GetOptionalsPath() => Path.Combine(GetUpdaterBasePath(), "Opcionais");
    private static string GetPerformancePath() => Path.Combine(GetUpdaterBasePath(), "config-performance");

    /// <summary>
    /// Versão do server/mods, de Launcher-Updater/server-version.txt (paridade com ServerVersionController).
    /// Substitui o "1.4.1" hardcoded que antes ia no manifesto.
    /// </summary>
    private static string GetServerVersionString()
    {
        try
        {
            var path = Path.Combine(GetUpdaterBasePath(), "server-version.txt");
            if (System.IO.File.Exists(path))
            {
                var v = System.IO.File.ReadAllText(path).Trim();
                if (!string.IsNullOrEmpty(v)) return v;
            }
        }
        catch { }
        return ServerVersionController.DefaultServerVersion; // fonte única do default
    }

    /// <summary>
    /// Versão do launcher, do ProductVersion do exe servido (paridade com LauncherUpdaterController),
    /// removendo o sufixo "+commit". Fallback = versão do server.
    /// </summary>
    private static string GetLauncherVersionString()
    {
        try
        {
            var exe = Path.Combine(GetUpdaterBasePath(), LauncherUpdaterController.LauncherExeFileName);
            if (System.IO.File.Exists(exe))
            {
                var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(exe);
                var v = info.ProductVersion ?? info.FileVersion;
                if (!string.IsNullOrEmpty(v))
                {
                    int plus = v.IndexOf('+');
                    if (plus >= 0) v = v.Substring(0, plus);
                    return v.Trim();
                }
            }
        }
        catch { }
        return GetServerVersionString();
    }

    /// <summary>
    /// ref: CR-01-01 (008) — containment guard shared by all download/list endpoints.
    /// Path.Combine with a ROOTED second argument discards the base dir, and
    /// .Replace("..", "") is a no-op for rooted inputs — so the resolved path MUST be
    /// re-checked against the base prefix (with trailing separator, ref: CR-01-06, so a
    /// sibling like "config-performance-bak" cannot pass by prefix).
    /// </summary>
    private static bool TryResolveUnder(string baseDir, string relativeInput, out string fullPath)
    {
        fullPath = null;

        if (string.IsNullOrEmpty(relativeInput))
        {
            return false;
        }

        try
        {
            var basePrefix = Path.GetFullPath(baseDir)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var resolved = Path.GetFullPath(Path.Combine(baseDir, relativeInput.Replace("..", "")));

            if (!resolved.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            fullPath = resolved;
            return true;
        }
        catch
        {
            return false; // invalid path characters etc. — treat as not found
        }
    }

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

    // B2 (paridade TS): o launcher chama GET /launcher/mods/version (RequestHandler.GetModVersion).
    // O TS devolvia config.serverVersion; aqui devolvemos a versão do server-version.txt.
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new { version = GetServerVersionString() });
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

        // ref: CR-01-01 — the old fallback served ANY rooted path (Path.Combine discards the
        // base for rooted args; ".." replace is a no-op there): profile/config exfiltration
        // over the Tailscale network. Same containment guard as performance-download.
        if (TryResolveUnder(GetModsRepoPath(), file, out var fallbackPath) && System.IO.File.Exists(fallbackPath))
        {
            return PhysicalFile(fallbackPath, "application/octet-stream");
        }

        return NotFound(new { error = "File not found" });
    }

    /// <summary>
    /// Item 008: serves a file from the performance overlay pack (Launcher-Updater/config-performance).
    /// Separate from /download because pack paths intentionally collide with mods_repo paths (overrides).
    /// </summary>
    [HttpGet("performance-download")]
    public IActionResult DownloadPerformanceFile([FromQuery] string file)
    {
        if (string.IsNullOrEmpty(file))
        {
            return BadRequest(new { error = "Missing 'file'" });
        }

        var normalizedFile = file.Replace("\\", "/").TrimStart('/');

        if (_performanceFileMapCache.TryGetValue(normalizedFile, out string physicalPath) && System.IO.File.Exists(physicalPath))
        {
            return PhysicalFile(physicalPath, "application/octet-stream");
        }

        // ref: CR-01-06 — trailing-separator prefix check via the shared guard (a sibling
        // folder like "config-performance-bak" must not pass by raw StartsWith prefix).
        if (TryResolveUnder(GetPerformancePath(), file, out var fallbackPath) && System.IO.File.Exists(fallbackPath))
        {
            return PhysicalFile(fallbackPath, "application/octet-stream");
        }

        return NotFound(new { error = "File not found" });
    }

    /// <summary>
    /// Item 009 (contrato SP0): each optional group folder may carry a description.json
    /// descriptor: { "name": "...", "description": { "pt": "...", "en": "..." } }.
    /// Response: { folders: [ { id, name, description } ] }.
    /// Retrocompat: folder without (or with invalid) descriptor -> name = folder name, description = null.
    /// </summary>
    [HttpGet("optionals-list")]
    public IActionResult GetOptionalsList()
    {
        var optsPath = GetOptionalsPath();
        if (!Directory.Exists(optsPath))
        {
            return Ok(new { folders = Array.Empty<object>() });
        }

        var folders = new List<object>();

        foreach (var dir in Directory.GetDirectories(optsPath))
        {
            var id = Path.GetFileName(dir);
            var name = id;
            object description = null;

            var descriptorPath = Path.Combine(dir, "description.json");
            if (System.IO.File.Exists(descriptorPath))
            {
                try
                {
                    using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(descriptorPath));
                    var root = doc.RootElement;

                    if (root.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(nameProp.GetString()))
                    {
                        name = nameProp.GetString();
                    }

                    if (root.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.Object)
                    {
                        string pt = descProp.TryGetProperty("pt", out var ptProp) && ptProp.ValueKind == JsonValueKind.String
                            ? ptProp.GetString() : null;
                        string en = descProp.TryGetProperty("en", out var enProp) && enProp.ValueKind == JsonValueKind.String
                            ? enProp.GetString() : null;

                        if (pt != null || en != null)
                        {
                            description = new { pt, en };
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Invalid descriptor -> treated as absent (retrocompat), just logged.
                    Console.WriteLine($"[ModUpdater] description.json inválido em '{id}': {ex.Message}");
                }
            }

            folders.Add(new { id, name, description });
        }

        return Ok(new { folders });
    }

    [HttpGet("optionals-manifest")]
    public IActionResult GetOptionalsManifest([FromQuery] string folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return BadRequest(new { error = "Missing 'folder'" });
        }

        // ref: CR-01-01 — same containment guard as the download endpoints (rooted input
        // to Path.Combine would discard the base dir; separator-suffixed prefix check).
        if (!TryResolveUnder(GetOptionalsPath(), folder, out var targetFolder) || !Directory.Exists(targetFolder))
        {
            return NotFound(new { error = "Folder not found" });
        }

        var filesList = new List<object>();
        var allFiles = Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            var relPath = Path.GetRelativePath(targetFolder, file).Replace("\\", "/");

            // Item 009: the group descriptor is metadata, never synced into the game folder.
            if (string.Equals(relPath, "description.json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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

        // ref: CR-01-01 — folder+file resolved through the shared containment guard
        // (rooted 'file' would discard base+folder in Path.Combine; suffixed prefix check).
        if (!TryResolveUnder(GetOptionalsPath(), folder + "/" + file, out var fullPath) || !System.IO.File.Exists(fullPath))
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

    /// <summary>
    /// Item 021 (D-021.B): tag optional-group files into the manifest so the launcher's group download
    /// actually fetches them (before, GenerateManifest only scanned mods_repo, so optionalGroup files
    /// were never emitted → the client's per-group cache was empty → "activate" downloaded nothing).
    /// Mirrors the legacy TS scan (modUpdater.ts): for each group, walk every Opcionais/&lt;folder&gt;,
    /// emit { path = targetSubDir + relPath, optional = true, optionalGroup = id }, and map the FINAL
    /// path to the physical file so the existing /download?file= endpoint serves it (via _fileMapCache).
    /// The group folders + their contents are operator-deployed content (gate G-5): a missing folder is
    /// logged and skipped, leaving the group with 0 files — which the launcher now surfaces as an error.
    /// </summary>
    private static void ScanOptionalGroups(JsonElement optionalGroups, List<object> files)
    {
        var optionalsRoot = GetOptionalsPath();
        if (!Directory.Exists(optionalsRoot))
        {
            Console.WriteLine($"[ModUpdater] Pasta Opcionais inexistente: {optionalsRoot} — nenhum opcional taggeado (gate de conteúdo G-5).");
            return;
        }

        foreach (var group in optionalGroups.EnumerateArray())
        {
            if (group.ValueKind != JsonValueKind.Object) continue;

            string id = group.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
                ? idProp.GetString() : null;
            if (string.IsNullOrWhiteSpace(id)) continue;

            string targetSubDir = group.TryGetProperty("targetSubDir", out var tsProp) && tsProp.ValueKind == JsonValueKind.String
                ? tsProp.GetString() : "";

            if (!group.TryGetProperty("folders", out var foldersProp) || foldersProp.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            int groupFileCount = 0;
            foreach (var folderEl in foldersProp.EnumerateArray())
            {
                if (folderEl.ValueKind != JsonValueKind.String) continue;
                var folder = folderEl.GetString();
                if (string.IsNullOrWhiteSpace(folder)) continue;

                // Same containment guard as the download/list endpoints (ref: CR-01-01).
                if (!TryResolveUnder(optionalsRoot, folder, out var folderPath) || !Directory.Exists(folderPath))
                {
                    Console.WriteLine($"[ModUpdater] Pasta opcional não encontrada para grupo '{id}': {folder}");
                    continue;
                }

                foreach (var file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
                {
                    var relPath = Path.GetRelativePath(folderPath, file).Replace("\\", "/");

                    // Item 009: the group descriptor is metadata, never synced into the game folder.
                    if (string.Equals(relPath, "description.json", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Trim('/') (não só TrimEnd): barra INICIAL no targetSubDir furaria o
                    // _fileMapCache — o endpoint /download normaliza a request com TrimStart('/').
                    var finalPath = string.IsNullOrEmpty(targetSubDir)
                        ? relPath
                        : $"{targetSubDir.Replace("\\", "/").Trim('/')}/{relPath}";

                    files.Add(new
                    {
                        path = finalPath,
                        hash = GetFileHash(file),
                        size = new FileInfo(file).Length,
                        optional = true,
                        optionalGroup = id
                    });
                    _fileMapCache[finalPath] = file;
                    groupFileCount++;
                }
            }

            Console.WriteLine($"[ModUpdater] Opcional '{id}': {groupFileCount} arquivo(s) taggeado(s)");
        }
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

            // Item 008: performance overlay pack — paths relative to the game root, mirroring
            // mods_repo layout. Exposed in the manifest so pack changes also change the manifest
            // hash (clients rescan). Downloaded via /launcher/mods/performance-download.
            var performanceFiles = new List<object>();
            _performanceFileMapCache.Clear();

            var perfPath = GetPerformancePath();
            if (!Directory.Exists(perfPath))
            {
                Directory.CreateDirectory(perfPath);
            }

            foreach (var file in Directory.GetFiles(perfPath, "*.*", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(perfPath, file).Replace("\\", "/");
                performanceFiles.Add(new { path = relPath, hash = GetFileHash(file), size = new FileInfo(file).Length });
                _performanceFileMapCache[relPath] = file;
            }

            string[] managedPaths = Array.Empty<string>();
            string[] deleteFiles = Array.Empty<string>();
            string[] ignoredFiles = Array.Empty<string>();
            object optionalGroupsArray = Array.Empty<object>();
            // Item 007: optional per-folder sync rules (prefix -> rule name), pass-through to the manifest.
            // Rule names: "default" | "preserve-divergent" | "mirror-delete" | "mirror-move-disabled".
            // Absent -> launcher falls back to its built-in prefix table.
            Dictionary<string, string> folderRules = new();

            string configPath = Path.Combine(GetUpdaterBasePath(), "config.json");
            if (!System.IO.File.Exists(configPath))
            {
                var defaultConfig = new
                {
                    managedPaths = new[] { "BepInEx/plugins", "user/mods" },
                    deleteFiles = Array.Empty<string>(),
                    ignoredFiles = new[] { "BepInEx/plugins/spt", "user/mods/spt" },
                    optionalGroups = Array.Empty<object>(),
                    folderRules = new Dictionary<string, string>
                    {
                        ["BepInEx/config"] = "preserve-divergent",
                        // config-server DUAL (seed-and-mirror): os arquivos em mods_repo/BepInEx/config-server/
                        //  1) SEED em BepInEx/config/ só se ausente (nunca deleta/sobrescreve as vivas);
                        //  2) MIRROR em BepInEx/config-server/ (última versão sempre; NÃO deleta extras),
                        //     de onde o usuário copia manualmente ao atualizarmos a config de um mod.
                        // Servers existentes dependem do fallback built-in do client; este default só vale
                        // num config.json novo.
                        ["BepInEx/config-server"] = "seed-and-mirror",
                        ["BepInEx/patchers"] = "mirror-move-disabled",
                        ["BepInEx/plugins"] = "mirror-move-disabled"
                    }
                };
                System.IO.File.WriteAllText(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
            }

            try
            {
                var configContent = System.IO.File.ReadAllText(configPath);
                var doc = JsonDocument.Parse(configContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("managedPaths", out var mpProp) && mpProp.ValueKind == JsonValueKind.Array)
                    managedPaths = mpProp.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).Cast<string>().ToArray();
                if (root.TryGetProperty("deleteFiles", out var dfProp) && dfProp.ValueKind == JsonValueKind.Array)
                    deleteFiles = dfProp.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).Cast<string>().ToArray();
                if (root.TryGetProperty("ignoredFiles", out var ifProp) && ifProp.ValueKind == JsonValueKind.Array)
                    ignoredFiles = ifProp.EnumerateArray().Select(x => x.GetString()).Where(x => x != null).Cast<string>().ToArray();
                if (root.TryGetProperty("optionalGroups", out var ogProp) && ogProp.ValueKind == JsonValueKind.Array)
                {
                    optionalGroupsArray = JsonSerializer.Deserialize<object[]>(ogProp.GetRawText()) ?? Array.Empty<object>();
                    // Item 021 (D-021.B): tag optional files so the launcher's group download fetches them.
                    ScanOptionalGroups(ogProp, files);
                }
                if (root.TryGetProperty("folderRules", out var frProp) && frProp.ValueKind == JsonValueKind.Object)
                    folderRules = JsonSerializer.Deserialize<Dictionary<string, string>>(frProp.GetRawText()) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ModUpdater] Erro ao ler config.json: {ex.Message}");
            }

            var manifestObj = new
            {
                serverVersion = GetServerVersionString(),
                launcherVersion = GetLauncherVersionString(),
                generatedAt = DateTime.UtcNow.ToString("O"),
                totalFiles = files.Count,
                managedPaths = managedPaths,
                deleteFiles = deleteFiles,
                ignoredFiles = ignoredFiles,
                optionalGroups = optionalGroupsArray,
                folderRules = folderRules,
                performanceOverlay = performanceFiles,
                files = files
            };

            var json = JsonSerializer.Serialize(manifestObj);
            
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
            
            _manifestHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            _manifestCache = manifestObj;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModUpdater] Critical error generating manifest: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            _manifestGenerating = false;
        }
    }
}
