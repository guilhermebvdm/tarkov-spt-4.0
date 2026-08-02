using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route(ModRouting.RoutePrefix + "launcher/mods")]
public class ModUpdaterController : ControllerBase
{
    private static string _manifestHash = string.Empty;
    private static object _manifestCache = null;
    private static int _manifestGenerating; // 0 = idle, 1 = gerando (gate atômico via Interlocked, CR-02-02)
    // ref: CR-01-06 — case-insensitive lookups: manifest-path casing may differ from the
    // client's request casing (merge preserves base casing); on a case-sensitive host the
    // miss would 404 instead of falling back.
    private static Dictionary<string, string> _fileMapCache = new(StringComparer.OrdinalIgnoreCase);
    // Item 008: performance overlay pack (Launcher-Updater/config-optional) — rel path -> physical path.
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
    // Item 030 (D-9): config-optional passa a viver DENTRO do mods_repo, junto das irmãs config-*.
    private static string GetOptionalConfigPath() => Path.Combine(GetModsRepoPath(), "BepInEx", "config-optional");

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
    /// sibling like "config-optional-bak" cannot pass by prefix).
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
            if (Volatile.Read(ref _manifestGenerating) == 0) { _ = GenerateManifestAsync(); }
            return StatusCode(503, new { error = "Manifesto ainda sendo gerado" });
        }
        return Ok(new { hash = _manifestHash });
    }

    [HttpGet("manifest")]
    public IActionResult GetManifest()
    {
        if (_manifestCache == null)
        {
            if (Volatile.Read(ref _manifestGenerating) == 0) { _ = GenerateManifestAsync(); }
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
    /// (id/name/description) para o manifesto, preenchendo <paramref name="optionalPrefixes"/> — lista de
    /// (prefixo normalizado → id do mod dono). Cada entrada de "paths" pode ser um ARQUIVO (`Foo.dll`) OU
    /// uma PASTA (`FooMod`): o matching no scan é por igualdade OU sob-a-pasta, então uma pasta cobre
    /// TODOS os arquivos dela (mod-pasta desligado → nada da pasta é sincronizado; ligado → tudo baixa).
    /// Validações S-5: recusa o mod inteiro se algum path está sob user/mods (client-only, D-15) ou se
    /// sobrepõe outro mod (D-19); o motivo vai pro log.
    /// </summary>
    private static object[] LoadOptionalDefs(string modsPath, out List<(string prefix, string id)> optionalPrefixes, out object[] categories, out List<string> rejectedPrefixes, List<string> warnings)
    {
        optionalPrefixes = new List<(string prefix, string id)>();
        rejectedPrefixes = new List<string>();
        categories = Array.Empty<object>();
        var mods = new List<object>();

        string defsPath = Path.Combine(modsPath, "BepInEx", "plugins-optional.json");
        if (!System.IO.File.Exists(defsPath)) return mods.ToArray();

        try
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(defsPath));
            categories = ReadCategories(doc.RootElement); // item 030: categorias para agrupar na UI
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

                // Coleta TODOS os prefixos do mod antes de validar — se recusado, todos são rastreados em
                // rejectedPrefixes p/ o scan pular (CR-02-03), inclusive os que vêm depois do que falhou.
                var modPrefixes = new List<string>();
                foreach (var p in pathsP.EnumerateArray())
                {
                    if (p.ValueKind != JsonValueKind.String) continue;
                    // Trim('/') (não TrimStart): aceita "FooMod/" (pasta com barra) e "Foo.dll" igual.
                    string prefixNorm = (p.GetString() ?? "").Replace("\\", "/").Trim('/').ToLowerInvariant();
                    if (prefixNorm.Length > 0) modPrefixes.Add(prefixNorm);
                }

                bool rejected = false;
                foreach (var prefixNorm in modPrefixes)
                {
                    if (prefixNorm == "user/mods" || prefixNorm.StartsWith("user/mods/", StringComparison.Ordinal))
                    {
                        warnings.Add($"mod opcional '{id}' referencia '{prefixNorm}' sob user/mods — RECUSADO (mod opcional é client-only, D-15)");
                        rejected = true; break;
                    }
                    // D-19: sobreposição com outro mod (igualdade OU um contém o outro como pasta).
                    var clash = optionalPrefixes.FirstOrDefault(e => PrefixOverlaps(prefixNorm, e.prefix));
                    if (clash.id != null)
                    {
                        warnings.Add($"'{prefixNorm}' se sobrepõe a '{clash.prefix}' (mod '{clash.id}') — mod '{id}' RECUSADO (D-19)");
                        rejected = true; break;
                    }
                }

                if (rejected)
                {
                    // CR-02-03: um mod recusado NÃO pode ter os arquivos emitidos como obrigatórios pra todos
                    // (seria o oposto de "opcional" — num coop empurra gameplay pra todo mundo). O scan pula
                    // qualquer arquivo sob esses prefixos.
                    rejectedPrefixes.AddRange(modPrefixes);
                    continue;
                }

                foreach (var pn in modPrefixes) optionalPrefixes.Add((pn, id));

                object name = mod.TryGetProperty("name", out var nP) ? nP.Clone() : (object)id;
                object description = mod.TryGetProperty("description", out var dP) ? dP.Clone() : null;
                string category = mod.TryGetProperty("category", out var cP) && cP.ValueKind == JsonValueKind.String ? cP.GetString() : null;
                mods.Add(new { id, name, description, category });
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"plugins-optional.json inválido: {ex.Message}");
        }

        return mods.ToArray();
    }

    /// <summary>
    /// Item 030: lê o array "categories" do JSON — [ { id, name (string ou {pt,en}), order } ] — para
    /// agrupar os itens na tela. Categoria ausente/inválida vira lista vazia (a UI cai num grupo default).
    /// </summary>
    private static object[] ReadCategories(JsonElement root)
    {
        var cats = new List<object>();
        if (root.TryGetProperty("categories", out var catsP) && catsP.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in catsP.EnumerateArray())
            {
                if (c.ValueKind != JsonValueKind.Object) continue;
                string cid = c.TryGetProperty("id", out var idP) && idP.ValueKind == JsonValueKind.String ? idP.GetString() : null;
                if (string.IsNullOrWhiteSpace(cid)) continue;
                object cname = c.TryGetProperty("name", out var nP) ? nP.Clone() : (object)cid;
                int order = c.TryGetProperty("order", out var oP) && oP.ValueKind == JsonValueKind.Number && oP.TryGetInt32(out var o) ? o : 0;
                cats.Add(new { id = cid, name = cname, order });
            }
        }
        return cats.ToArray();
    }

    /// <summary>Item 030: dois prefixos se sobrepõem se são iguais ou um é pasta-mãe do outro.</summary>
    private static bool PrefixOverlaps(string a, string b) =>
        a == b
        || a.StartsWith(b + "/", StringComparison.Ordinal)
        || b.StartsWith(a + "/", StringComparison.Ordinal);

    /// <summary>Item 030: um path pertence a um prefixo de mod se é igual a ele OU está sob a pasta.</summary>
    private static bool IsUnderOrEqual(string relNorm, string prefix) =>
        relNorm == prefix || relNorm.StartsWith(prefix + "/", StringComparison.Ordinal);

    /// <summary>
    /// Item 030 (S-4): lê mods_repo/BepInEx/config-optional/configs-optional.json e devolve optionalConfigs
    /// (id/name/description), preenchendo <paramref name="pathToOptionalConfigId"/> (path normalizado sob
    /// config-optional/ → id do item). "files" é relativo à pasta config-optional/. Validação S-5:
    /// arquivo em dois itens é recusado (D-19).
    /// </summary>
    private static object[] LoadOptionalConfigDefs(out Dictionary<string, string> pathToOptionalConfigId, out object[] categories, List<string> warnings)
    {
        pathToOptionalConfigId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        categories = Array.Empty<object>();
        var items = new List<object>();

        string defsPath = Path.Combine(GetOptionalConfigPath(), "configs-optional.json");
        if (!System.IO.File.Exists(defsPath)) return items.ToArray();

        try
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(defsPath));
            categories = ReadCategories(doc.RootElement);
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

                // CR-02-04: coleta e valida os arquivos ANTES de registrar. Um arquivo já pertencente a
                // outro item recusa o ITEM INTEIRO (paridade com o D-19 dos mods), em vez de deixar um item
                // parcialmente possuído na UI (o player ligava e parte do efeito não acontecia).
                var itemFiles = new List<string>();
                bool itemRejected = false;
                foreach (var f in filesP.EnumerateArray())
                {
                    if (f.ValueKind != JsonValueKind.String) continue;
                    string inner = (f.GetString() ?? "").Replace("\\", "/").TrimStart('/');
                    if (inner.Length == 0) continue;
                    string relNorm = ("bepinex/config-optional/" + inner).ToLowerInvariant();

                    if (pathToOptionalConfigId.TryGetValue(relNorm, out var owner) || itemFiles.Contains(relNorm))
                    {
                        warnings.Add($"arquivo '{relNorm}' já pertence ao item '{owner ?? id}' — item de config '{id}' RECUSADO inteiro (D-19)");
                        itemRejected = true; break;
                    }
                    itemFiles.Add(relNorm);
                }

                if (itemRejected) continue;
                foreach (var rel in itemFiles) pathToOptionalConfigId[rel] = id;

                object name = item.TryGetProperty("name", out var nP) ? nP.Clone() : (object)id;
                object description = item.TryGetProperty("description", out var dP) ? dP.Clone() : null;
                string category = item.TryGetProperty("category", out var cP) && cP.ValueKind == JsonValueKind.String ? cP.GetString() : null;
                items.Add(new { id, name, description, category });
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"configs-optional.json inválido: {ex.Message}");
        }

        return items.ToArray();
    }

    /// <summary>
    /// Item 030 (RN-2, lado servidor): avisa quando o mesmo relativo existe em config-force/ e em
    /// config-optional/. A performance vence (D-1), mas a config forçada — que existe para paridade
    /// de coop — é silenciosamente sobreposta em quem tiver o item ligado; o operador precisa enxergar.
    /// </summary>
    private static void DetectForceOptionalConfigCollisions(string modsPath, List<string> warnings)
    {
        string forceDir = Path.Combine(modsPath, "BepInEx", "config-force");
        string perfDir = GetOptionalConfigPath();
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
                warnings.Add($"'{rel}' está em config-force E config-optional — a performance sobrepõe a config forçada em quem tiver o item ligado (RN-2)");
            }
        }
    }

    // Item 001: wrapper com o gate. O corpo vive em GenerateManifestCore (sem gate), reusado pelo
    // EnsureManifestReady do boot. Não-async (corpo síncrono/CPU-bound) → sem CS1998; o
    // `_ = GenerateManifestAsync()` dos endpoints segue funcionando.
    private static Task GenerateManifestAsync()
    {
        // CR-02-02: gate ATÔMICO — dois requests concorrentes (ou request + /refresh) não entram ambos
        // na geração completa (scan + hash + swap). O test-and-set em bool não era atômico.
        if (Interlocked.CompareExchange(ref _manifestGenerating, 1, 0) != 0) return Task.CompletedTask;
        try { GenerateManifestCore(); }
        finally { Interlocked.Exchange(ref _manifestGenerating, 0); }
        return Task.CompletedTask;
    }

    // Item 001: geração propriamente dita (scan + hash + swap + persistência), SEM o gate. Mantém o
    // try/catch interno (log "Critical error") — nunca propaga para o caller (PA-01-04).
    private static void GenerateManifestCore()
    {
        try
        {
            var files = new List<object>();
            // 🟡 CR: dict LOCAL preenchido e trocado por referência atômica no fim — evita que um
            // /refresh concorrente com Clear()+repopular derrube o TryGetValue do /download (404 nos -ref,
            // que só existem via cache).
            var fileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Item 001: acúmulo da impressão leve do mods_repo (cobre TODOS os arquivos, inclusive os
            // 2 JSON de metadados que o loop pula só para o manifesto).
            int fpCount = 0; long fpSizeSum = 0; long fpMaxTicks = 0;
            var fpPaths = new List<string>();

            var modsPath = GetModsRepoPath();
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
            }

            // Item 030: lê as definições ANTES do scan, para taggear cada arquivo com optionalId/
            // optionalConfigId. Validações de conteúdo (S-5): recusa path sob user/mods (mod opcional é
            // client-only, D-15) e arquivo repetido em dois itens (D-19). As mensagens vão pro log.
            var contentWarnings = new List<string>();
            var optionalMods = LoadOptionalDefs(modsPath, out var optionalPrefixes, out var optionalModCategories, out var rejectedModPrefixes, contentWarnings);
            var optionalConfigs = LoadOptionalConfigDefs(out var pathToOptionalConfigId, out var optionalConfigCategories, contentWarnings);

            // CR-01-03: id deve ser único ENTRE os dois eixos — o motor do launcher compara JustToggledIds
            // contra optionalId E optionalConfigId, então um id repetido cross-eixo faz alternar um mod
            // marcar o config homônimo como "recém-tocado" (aplicando-o sobre a customização do player).
            var modIdSet = new HashSet<string>(optionalPrefixes.Select(e => e.id), StringComparer.OrdinalIgnoreCase);
            foreach (var dupId in pathToOptionalConfigId.Values.Distinct().Where(cid => modIdSet.Contains(cid)))
            {
                contentWarnings.Add($"id '{dupId}' em mod opcional E config opcional — ids devem ser únicos entre os dois eixos (o toggle de um pode aplicar o outro)");
            }

            const string PerfPrefix = "bepinex/config-optional/";
            const string PerfPrefixCased = "BepInEx/config-optional/";

            var allFiles = Directory.GetFiles(modsPath, "*.*", SearchOption.AllDirectories);

            foreach (var file in allFiles)
            {
                var relPath = Path.GetRelativePath(modsPath, file).Replace("\\", "/");
                var relNorm = relPath.ToLowerInvariant();

                // Item 001: impressão ANTES do continue — cobre TODO o mods_repo (editar as definições
                // também tem que invalidar). FileInfo é só stat (não abre o arquivo).
                var fi = new FileInfo(file);
                fpCount++;
                fpSizeSum += fi.Length;
                long fpTicks = fi.LastWriteTimeUtc.Ticks;
                if (fpTicks > fpMaxTicks) fpMaxTicks = fpTicks;
                fpPaths.Add(relPath);

                // S-2: os JSON de definição são METADADOS — nunca sincronizados no jogo do player.
                if (relNorm == "bepinex/plugins-optional.json"
                    || relNorm == "bepinex/config-optional/configs-optional.json")
                {
                    continue;
                }

                var hash = GetFileHash(file);
                var size = fi.Length;

                if (relNorm.StartsWith(PerfPrefix, StringComparison.Ordinal))
                {
                    // S-5: arquivo sob config-optional/ SEM optionalConfigId é erro de conteúdo — não emite
                    // (senão viraria config aplicada que o player não consegue desligar).
                    if (!pathToOptionalConfigId.TryGetValue(relNorm, out var perfId))
                    {
                        contentWarnings.Add($"'{relPath}' não está listado em nenhum item do configs-optional.json — ignorado (não emitido no manifesto)");
                        continue;
                    }

                    // Fonte: aplica em config/ quando o item está ligado (optional-config-to-config).
                    files.Add(new { path = relPath, hash, size, optionalConfigId = perfId });
                    fileMap[relPath] = file;

                    // D-18: 2º prefixo lógico — MESMO arquivo físico, espelhado no cliente (mirror-reference).
                    var refPath = "BepInEx/config-optional-ref/" + relPath.Substring(PerfPrefixCased.Length);
                    files.Add(new { path = refPath, hash, size });
                    fileMap[refPath] = file;
                    continue;
                }

                // Matching por PREFIXO: o arquivo pertence a um mod opcional se é igual a um path do mod OU
                // está sob uma pasta listada. Assim um mod-pasta cobre TODOS os seus arquivos (nada da pasta
                // vaza como obrigatório, e desligar leva a pasta inteira pra quarentena).
                string optId = null;
                foreach (var (prefix, oid) in optionalPrefixes)
                {
                    if (IsUnderOrEqual(relNorm, prefix)) { optId = oid; break; }
                }

                if (optId != null)
                {
                    files.Add(new { path = relPath, hash, size, optional = true, optionalId = optId });
                }
                else if (rejectedModPrefixes.Any(rp => IsUnderOrEqual(relNorm, rp)))
                {
                    // CR-02-03: arquivo de um mod opcional RECUSADO (config inválida) — não emite, senão
                    // viraria obrigatório pra todos os players. A recusa em si já está em contentWarnings.
                    contentWarnings.Add($"'{relPath}' pertence a um mod opcional recusado — não emitido no manifesto");
                    continue;
                }
                else
                {
                    files.Add(new { path = relPath, hash, size });
                }
                fileMap[relPath] = file;
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
                        ["BepInEx/config-optional"] = "optional-config-to-config",
                        ["BepInEx/config-optional-ref"] = "mirror-reference",
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
                using var doc = JsonDocument.Parse(configContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("managedPaths", out var mpProp) && mpProp.ValueKind == JsonValueKind.Array)
                    managedPaths = mpProp.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()).Cast<string>().ToArray();
                if (root.TryGetProperty("deleteFiles", out var dfProp) && dfProp.ValueKind == JsonValueKind.Array)
                    deleteFiles = dfProp.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()).Cast<string>().ToArray();
                if (root.TryGetProperty("ignoredFiles", out var ifProp) && ifProp.ValueKind == JsonValueKind.Array)
                    ignoredFiles = ifProp.EnumerateArray().Where(x => x.ValueKind == JsonValueKind.String).Select(x => x.GetString()).Cast<string>().ToArray();
                // Item 030: optionalGroups foi aposentado (ScanOptionalGroups removido) — mods opcionais e
                // configs de performance vêm de plugins-optional.json / configs-optional.json (LoadOptionalDefs/
                // LoadOptionalConfigDefs) e são taggeados por arquivo no scan acima.
                if (root.TryGetProperty("folderRules", out var frProp) && frProp.ValueKind == JsonValueKind.Object)
                    folderRules = JsonSerializer.Deserialize<Dictionary<string, string>>(frProp.GetRawText()) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ModUpdater] Erro ao ler config.json: {ex.Message}");
            }

            // S-6 / R-11: garante que o manifesto SEMPRE carregue as regras do item 030, mesmo que o
            // config.json de produção não as defina — cliente antigo que não conhece "optional-config-to-config"
            // ignora a regra (TryParse falha), então mover a pasta antes de todos atualizarem é seguro.
            folderRules["BepInEx/config-optional"] = "optional-config-to-config";
            folderRules["BepInEx/config-optional-ref"] = "mirror-reference";

            // RN-2 (lado servidor): avisa quando o mesmo arquivo está em config-force E config-optional.
            DetectForceOptionalConfigCollisions(modsPath, contentWarnings);
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
                optionalConfigs = optionalConfigs,
                optionalModCategories = optionalModCategories,
                optionalConfigCategories = optionalConfigCategories,
                folderRules = folderRules,
                // CR-02-05: expõe as recusas/colisões S-5 ao operador (antes só iam pro Console, invisível
                // em produção — "meu mod opcional não apareceu" exigia ler o log do servidor).
                contentWarnings = contentWarnings,
                files = files
            };

            var json = JsonSerializer.Serialize(manifestObj);
            
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
            
            // CR-02-06: publica na ordem mapa → manifesto → hash. Assim um cliente que veja o hash novo
            // (via /manifest-hash) sempre encontra o manifesto correspondente já buscável (sem janela 503),
            // e qualquer path do manifesto novo já é resolvível no /download (fileMap primeiro).
            _fileMapCache = fileMap; // troca atômica de referência
            _manifestCache = manifestObj;
            _manifestHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Item 001: monta a impressão (paths ordenados → digest barato pega rename/add/remove — CC-8)
            // e persiste em disco (best-effort — CC-7). A impressão é dos MESMOS arquivos hasheados (CC-2).
            fpPaths.Sort(StringComparer.Ordinal);
            var fingerprint = new Fingerprint
            {
                count = fpCount,
                sizeSum = fpSizeSum,
                maxMtimeUtcTicks = fpMaxTicks,
                pathsDigest = Md5Hex(string.Join("\n", fpPaths)),
            };
            PersistManifest(manifestObj, _manifestHash, fingerprint, fileMap, modsPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModUpdater] Critical error generating manifest: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    // ===================== Item 001: cache persistente do manifesto =====================

    private const int PersistedFormatVersion = 1;

    private static string GetManifestCachePath() =>
        Path.Combine(GetUpdaterBasePath(), $"manifest-cache{ModRouting.StateSuffix}.json");

    private static string Md5Hex(string s)
    {
        using var md5 = MD5.Create();
        return BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s)))
            .Replace("-", "").ToLowerInvariant();
    }

    private sealed class Fingerprint
    {
        public int count { get; set; }
        public long sizeSum { get; set; }
        public long maxMtimeUtcTicks { get; set; }
        public string pathsDigest { get; set; } = "";
    }

    private sealed class PersistedManifest
    {
        public int formatVersion { get; set; }
        public string hash { get; set; } = "";
        public Fingerprint fingerprint { get; set; } = new();
        public JsonElement manifest { get; set; }                       // o manifestObj serializado
        public Dictionary<string, string> fileMap { get; set; } = new(); // chaveLógica -> relativo ao mods_repo
    }

    /// <summary>
    /// Item 001: chamado proativamente no boot (Plugin static ctor). Carrega do disco se a impressão
    /// bate; senão regera. Gate próprio — não roda concorrente com uma geração em voo. GenerateManifestCore
    /// e TryLoadPersisted têm try/catch internos; o CALLER (Plugin) ainda envolve por robustez (PA-01-04).
    /// </summary>
    public static void EnsureManifestReady()
    {
        if (_manifestCache != null) return;
        if (Interlocked.CompareExchange(ref _manifestGenerating, 1, 0) != 0) return; // já cuidando
        try
        {
            if (TryLoadPersisted()) return; // carregou do disco (log dentro)
            GenerateManifestCore();          // regera + persiste
        }
        finally { Interlocked.Exchange(ref _manifestGenerating, 0); }
    }

    private static bool TryLoadPersisted()
    {
        try
        {
            string path = GetManifestCachePath();
            if (!System.IO.File.Exists(path)) return false;

            var p = JsonSerializer.Deserialize<PersistedManifest>(System.IO.File.ReadAllText(path));
            if (p == null || p.formatVersion != PersistedFormatVersion) return false; // CC-5
            // Guard (CR-01-01): manifest ausente/corrompido (adulteração externa do arquivo) — regera
            // em vez de servir um JsonElement Undefined que só estouraria na serialização do /manifest.
            if (p.manifest.ValueKind != JsonValueKind.Object) return false;

            var modsPath = GetModsRepoPath();
            var current = ComputeFingerprint(modsPath);
            if (!FingerprintEquals(current, p.fingerprint))
            {
                Console.WriteLine("[ModUpdater] impressão do mods_repo mudou — regerando manifesto.");
                return false;
            }

            // Reconstrói o fileMap absoluto a partir dos relativos ao mods_repo (R-3: cobre os -ref D-18).
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in p.fileMap)
                map[kv.Key] = Path.GetFullPath(Path.Combine(modsPath,
                    kv.Value.Replace("/", Path.DirectorySeparatorChar.ToString())));

            // Ordem de publicação map → manifesto → hash (CR-02-06). CC-9: serve o hash ORIGINAL.
            _fileMapCache = map;
            _manifestCache = p.manifest;
            _manifestHash = p.hash;
            Console.WriteLine("[ModUpdater] manifesto carregado do disco (sem regerar).");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModUpdater] cache persistido ilegível ({ex.Message}) — regerando.");
            return false;
        }
    }

    private static Fingerprint ComputeFingerprint(string modsPath)
    {
        if (!Directory.Exists(modsPath)) return new Fingerprint { pathsDigest = "" }; // CC-10
        int count = 0; long sizeSum = 0; long maxTicks = 0; var paths = new List<string>();
        foreach (var file in Directory.GetFiles(modsPath, "*.*", SearchOption.AllDirectories))
        {
            var fi = new FileInfo(file);
            count++; sizeSum += fi.Length;
            long t = fi.LastWriteTimeUtc.Ticks; if (t > maxTicks) maxTicks = t;
            paths.Add(Path.GetRelativePath(modsPath, file).Replace("\\", "/"));
        }
        paths.Sort(StringComparer.Ordinal);
        return new Fingerprint
        {
            count = count,
            sizeSum = sizeSum,
            maxMtimeUtcTicks = maxTicks,
            pathsDigest = Md5Hex(string.Join("\n", paths)),
        };
    }

    private static bool FingerprintEquals(Fingerprint a, Fingerprint b) =>
        a.count == b.count && a.sizeSum == b.sizeSum && a.maxMtimeUtcTicks == b.maxMtimeUtcTicks
        && string.Equals(a.pathsDigest, b.pathsDigest, StringComparison.Ordinal);

    private static void PersistManifest(object manifestObj, string hash, Fingerprint fp,
        Dictionary<string, string> fileMap, string modsPath)
    {
        try
        {
            // fileMap guardado RELATIVO ao mods_repo (robusto a mudança de pasta — R-3).
            var relMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in fileMap)
                relMap[kv.Key] = Path.GetRelativePath(modsPath, kv.Value).Replace("\\", "/");

            var payload = new PersistedManifest
            {
                formatVersion = PersistedFormatVersion,
                hash = hash,
                fingerprint = fp,
                manifest = JsonSerializer.SerializeToElement(manifestObj),
                fileMap = relMap,
            };

            string path = GetManifestCachePath();
            string tmp = path + ".tmp";
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(tmp, JsonSerializer.Serialize(payload));
            System.IO.File.Move(tmp, path, overwrite: true); // troca atômica (CC-4)
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModUpdater] persist manifest falhou (best-effort): {ex.Message}");
        }
    }
}
