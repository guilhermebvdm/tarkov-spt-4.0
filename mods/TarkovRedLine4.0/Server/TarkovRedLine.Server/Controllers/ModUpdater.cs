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
    // Item 030: _performanceFileMapCache removido — o pack de performance vive no mods_repo e entra no
    // _fileMapCache comum (servido pelo /download). Ver S-7.

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
    // Item 030 (D-9): config-performance passa a viver DENTRO do mods_repo, junto das irmãs config-*.
    private static string GetPerformancePath() => Path.Combine(GetModsRepoPath(), "BepInEx", "config-performance");

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

    // Item 030 (S-7): removidas as rotas do modelo antigo — performance-download (o pack de performance
    // agora vive no mods_repo e é servido pelo /download comum via _fileMapCache) e optionals-list/
    // optionals-manifest/optional-download (mods opcionais vêm de plugins-optional.json, sem pasta
    // Opcionais/). D-13 aposentou o overlay; o launcher da Fase 3 não chama mais nenhuma delas.

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
    /// Item 030 (S-4): lê mods_repo/BepInEx/plugins-optional.json e devolve o array optionalMods
    /// (id/name/description) para o manifesto, preenchendo <paramref name="pathToOptionalId"/>
    /// (path normalizado → id do mod dono). Validações S-5: recusa o mod inteiro se algum path está
    /// sob user/mods (client-only, D-15) ou já pertence a outro mod (D-19); o motivo vai pro log.
    /// </summary>
    private static object[] LoadOptionalDefs(string modsPath, out Dictionary<string, string> pathToOptionalId, List<string> warnings)
    {
        pathToOptionalId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var mods = new List<object>();

        string defsPath = Path.Combine(modsPath, "BepInEx", "plugins-optional.json");
        if (!System.IO.File.Exists(defsPath)) return mods.ToArray();

        try
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(defsPath));
            if (!doc.RootElement.TryGetProperty("mods", out var modsProp) || modsProp.ValueKind != JsonValueKind.Array)
            {
                return mods.ToArray();
            }

            foreach (var mod in modsProp.EnumerateArray())
            {
                if (mod.ValueKind != JsonValueKind.Object) continue;
                string id = mod.TryGetProperty("id", out var idP) && idP.ValueKind == JsonValueKind.String ? idP.GetString() : null;
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (!mod.TryGetProperty("paths", out var pathsP) || pathsP.ValueKind != JsonValueKind.Array) continue;

                var validPaths = new List<string>();
                bool rejected = false;
                foreach (var p in pathsP.EnumerateArray())
                {
                    if (p.ValueKind != JsonValueKind.String) continue;
                    string relNorm = (p.GetString() ?? "").Replace("\\", "/").TrimStart('/').ToLowerInvariant();
                    if (relNorm.Length == 0) continue;

                    if (relNorm.StartsWith("user/mods/", StringComparison.Ordinal))
                    {
                        warnings.Add($"mod opcional '{id}' referencia '{relNorm}' sob user/mods — RECUSADO (mod opcional é client-only, D-15)");
                        rejected = true; break;
                    }
                    if (pathToOptionalId.TryGetValue(relNorm, out var owner))
                    {
                        warnings.Add($"arquivo '{relNorm}' está em dois mods opcionais ('{owner}' e '{id}') — mod '{id}' RECUSADO (D-19)");
                        rejected = true; break;
                    }
                    validPaths.Add(relNorm);
                }
                if (rejected) continue;

                foreach (var rn in validPaths) pathToOptionalId[rn] = id;

                object name = mod.TryGetProperty("name", out var nP) ? nP.Clone() : (object)id;
                object description = mod.TryGetProperty("description", out var dP) ? dP.Clone() : null;
                mods.Add(new { id, name, description });
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"plugins-optional.json inválido: {ex.Message}");
        }

        return mods.ToArray();
    }

    /// <summary>
    /// Item 030 (S-4): lê mods_repo/BepInEx/config-performance/performance.json e devolve performanceItems
    /// (id/name/description), preenchendo <paramref name="pathToPerformanceId"/> (path normalizado sob
    /// config-performance/ → id do item). "files" é relativo à pasta config-performance/. Validação S-5:
    /// arquivo em dois itens é recusado (D-19).
    /// </summary>
    private static object[] LoadPerformanceDefs(out Dictionary<string, string> pathToPerformanceId, List<string> warnings)
    {
        pathToPerformanceId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var items = new List<object>();

        string defsPath = Path.Combine(GetPerformancePath(), "performance.json");
        if (!System.IO.File.Exists(defsPath)) return items.ToArray();

        try
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(defsPath));
            if (!doc.RootElement.TryGetProperty("items", out var itemsProp) || itemsProp.ValueKind != JsonValueKind.Array)
            {
                return items.ToArray();
            }

            foreach (var item in itemsProp.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                string id = item.TryGetProperty("id", out var idP) && idP.ValueKind == JsonValueKind.String ? idP.GetString() : null;
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (!item.TryGetProperty("files", out var filesP) || filesP.ValueKind != JsonValueKind.Array) continue;

                foreach (var f in filesP.EnumerateArray())
                {
                    if (f.ValueKind != JsonValueKind.String) continue;
                    string inner = (f.GetString() ?? "").Replace("\\", "/").TrimStart('/');
                    if (inner.Length == 0) continue;
                    string relNorm = ("bepinex/config-performance/" + inner).ToLowerInvariant();

                    if (pathToPerformanceId.TryGetValue(relNorm, out var owner))
                    {
                        warnings.Add($"arquivo '{relNorm}' está em dois itens de performance ('{owner}' e '{id}') — ignorado (D-19)");
                        continue;
                    }
                    pathToPerformanceId[relNorm] = id;
                }

                object name = item.TryGetProperty("name", out var nP) ? nP.Clone() : (object)id;
                object description = item.TryGetProperty("description", out var dP) ? dP.Clone() : null;
                items.Add(new { id, name, description });
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"performance.json inválido: {ex.Message}");
        }

        return items.ToArray();
    }

    /// <summary>
    /// Item 030 (RN-2, lado servidor): avisa quando o mesmo relativo existe em config-force/ e em
    /// config-performance/. A performance vence (D-1), mas a config forçada — que existe para paridade
    /// de coop — é silenciosamente sobreposta em quem tiver o item ligado; o operador precisa enxergar.
    /// </summary>
    private static void DetectForcePerformanceCollisions(string modsPath, List<string> warnings)
    {
        string forceDir = Path.Combine(modsPath, "BepInEx", "config-force");
        string perfDir = GetPerformancePath();
        if (!Directory.Exists(forceDir) || !Directory.Exists(perfDir)) return;

        var perfRels = new HashSet<string>(
            Directory.GetFiles(perfDir, "*.*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(perfDir, f).Replace("\\", "/").ToLowerInvariant()),
            StringComparer.Ordinal);

        foreach (var f in Directory.GetFiles(forceDir, "*.*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(forceDir, f).Replace("\\", "/").ToLowerInvariant();
            if (perfRels.Contains(rel))
            {
                warnings.Add($"'{rel}' está em config-force E config-performance — a performance sobrepõe a config forçada em quem tiver o item ligado (RN-2)");
            }
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

            // Item 030: lê as definições ANTES do scan, para taggear cada arquivo com optionalId/
            // performanceId. Validações de conteúdo (S-5): recusa path sob user/mods (mod opcional é
            // client-only, D-15) e arquivo repetido em dois itens (D-19). As mensagens vão pro log.
            var contentWarnings = new List<string>();
            var optionalMods = LoadOptionalDefs(modsPath, out var pathToOptionalId, contentWarnings);
            var performanceItems = LoadPerformanceDefs(out var pathToPerformanceId, contentWarnings);

            const string PerfPrefix = "bepinex/config-performance/";
            const string PerfPrefixCased = "BepInEx/config-performance/";

            var allFiles = Directory.GetFiles(modsPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                var relPath = Path.GetRelativePath(modsPath, file).Replace("\\", "/");
                var relNorm = relPath.ToLowerInvariant();

                // S-2: os JSON de definição são METADADOS — nunca sincronizados no jogo do player.
                if (relNorm.EndsWith("plugins-optional.json", StringComparison.Ordinal)
                    || relNorm.EndsWith("config-performance/performance.json", StringComparison.Ordinal))
                {
                    continue;
                }

                var hash = GetFileHash(file);
                var size = new FileInfo(file).Length;

                if (relNorm.StartsWith(PerfPrefix, StringComparison.Ordinal))
                {
                    // S-5: arquivo sob config-performance/ SEM performanceId é erro de conteúdo — não emite
                    // (senão viraria config aplicada que o player não consegue desligar).
                    if (!pathToPerformanceId.TryGetValue(relNorm, out var perfId))
                    {
                        contentWarnings.Add($"'{relPath}' não está listado em nenhum item do performance.json — ignorado (não emitido no manifesto)");
                        continue;
                    }

                    // Fonte: aplica em config/ quando o item está ligado (performance-to-config).
                    files.Add(new { path = relPath, hash, size, performanceId = perfId });
                    _fileMapCache[relPath] = file;

                    // D-18: 2º prefixo lógico — MESMO arquivo físico, espelhado no cliente (mirror-reference).
                    var refPath = "BepInEx/config-performance-ref/" + relPath.Substring(PerfPrefixCased.Length);
                    files.Add(new { path = refPath, hash, size });
                    _fileMapCache[refPath] = file;
                    continue;
                }

                if (pathToOptionalId.TryGetValue(relNorm, out var optId))
                {
                    files.Add(new { path = relPath, hash, size, optional = true, optionalId = optId });
                }
                else
                {
                    files.Add(new { path = relPath, hash, size });
                }
                _fileMapCache[relPath] = file;
            }

            string[] managedPaths = Array.Empty<string>();
            string[] deleteFiles = Array.Empty<string>();
            string[] ignoredFiles = Array.Empty<string>();
            // Item 007: optional per-folder sync rules (prefix -> rule name), pass-through to the manifest.
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
                    folderRules = new Dictionary<string, string>
                    {
                        ["BepInEx/config"] = "preserve-divergent",
                        // Item 030: config de performance aplica em config/ quando ligada (vence force e
                        // config); a pasta-espelho -ref é biblioteca de referência no cliente (D-18).
                        ["BepInEx/config-performance"] = "performance-to-config",
                        ["BepInEx/config-performance-ref"] = "mirror-reference",
                        // config-server = MIRROR-REFERENCE (biblioteca de referência). Os arquivos em
                        // mods_repo/BepInEx/config-server/ só são espelhados em BepInEx/config-server/ do
                        // cliente (última versão sempre; NÃO deleta extras; NUNCA toca BepInEx/config/).
                        // Quem distribui defaults é o canal 'config' (preserve-divergent). O usuário copia
                        // manualmente de config-server/ ao atualizarmos a config de um mod.
                        // Servers existentes dependem do fallback built-in do client; este default só vale
                        // num config.json novo (sem efeito nesta release — só higiene p/ redeploy futuro).
                        ["BepInEx/config-server"] = "mirror-reference",
                        // config-force: canal "essa config vai pra TODO MUNDO". Arquivos em
                        // mods_repo/BepInEx/config-force/ SOBRESCREVEM o BepInEx/config/ do usuário
                        // sempre que divergirem — ignoram customização (ao contrário do config normal,
                        // que é preserve-divergent). Usar para corrigir configs que quebram o coop.
                        ["BepInEx/config-force"] = "force-to-config",
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
                // Item 030: optionalGroups foi aposentado (ScanOptionalGroups removido) — mods opcionais e
                // configs de performance vêm de plugins-optional.json / performance.json (LoadOptionalDefs/
                // LoadPerformanceDefs) e são taggeados por arquivo no scan acima.
                if (root.TryGetProperty("folderRules", out var frProp) && frProp.ValueKind == JsonValueKind.Object)
                    folderRules = JsonSerializer.Deserialize<Dictionary<string, string>>(frProp.GetRawText()) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ModUpdater] Erro ao ler config.json: {ex.Message}");
            }

            // S-6 / R-11: garante que o manifesto SEMPRE carregue as regras do item 030, mesmo que o
            // config.json de produção não as defina — cliente antigo que não conhece "performance-to-config"
            // ignora a regra (TryParse falha), então mover a pasta antes de todos atualizarem é seguro.
            folderRules["BepInEx/config-performance"] = "performance-to-config";
            folderRules["BepInEx/config-performance-ref"] = "mirror-reference";

            // RN-2 (lado servidor): avisa quando o mesmo arquivo está em config-force E config-performance.
            DetectForcePerformanceCollisions(modsPath, contentWarnings);
            foreach (var w in contentWarnings) Console.WriteLine($"[ModUpdater] item030: {w}");

            var manifestObj = new
            {
                serverVersion = GetServerVersionString(),
                launcherVersion = GetLauncherVersionString(),
                generatedAt = DateTime.UtcNow.ToString("O"),
                totalFiles = files.Count,
                managedPaths = managedPaths,
                deleteFiles = deleteFiles,
                ignoredFiles = ignoredFiles,
                optionalMods = optionalMods,
                performanceItems = performanceItems,
                folderRules = folderRules,
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
