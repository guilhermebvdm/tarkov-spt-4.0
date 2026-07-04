using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using SPT.Launcher.Sync;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Gerencia mods opcionais com API genérica — opera a partir do manifesto principal.
    /// Cada grupo é identificado por um ID ("gore", "grass", "hollywood").
    /// Server endpoints mantidos para compatibilidade: /launcher/mods/optionals-manifest, /launcher/mods/optional-download
    /// </summary>
    public static class OptionalModsHelper
    {
        private static string GamePath => LauncherSettingsProvider.Instance.GamePath;
        private static string OpcionaisLocalPath => Path.Combine(GamePath, "Opcionais");

        // === Cache estático atualizado a cada recebimento de manifesto ===

        /// <summary>
        /// Metadados dos grupos opcionais (do manifesto do servidor).
        /// </summary>
        private static List<OptionalGroupInfo> _cachedGroups = new List<OptionalGroupInfo>();

        /// <summary>
        /// Arquivos por grupo: groupId → lista de ManifestFile
        /// </summary>
        private static Dictionary<string, List<ManifestFile>> _cachedGroupFiles = new Dictionary<string, List<ManifestFile>>();

        /// <summary>
        /// TargetSubDir por grupo: groupId → subdir (pode ser "")
        /// </summary>
        private static Dictionary<string, string> _cachedGroupTargetSubDir = new Dictionary<string, string>();

        /// <summary>
        /// OffFolders por grupo: groupId → lista de nomes de pasta 
        /// </summary>
        private static Dictionary<string, List<string>> _cachedGroupOffFolders = new Dictionary<string, List<string>>();

        // Usa a URL base do server (porta 7075 do HwidManager)
        private static string GetServerBaseUrl()
        {
            var serverUrl = LauncherSettingsProvider.Instance.Server?.Url ?? "https://127.0.0.1:6969";
            try
            {
                var uri = new Uri(serverUrl);
                return $"http://{uri.Host}";
            }
            catch
            {
                return "http://127.0.0.1";
            }
        }

        /// <summary>
        /// Atualiza o cache de grupos e arquivos a partir do manifesto principal.
        /// Chamado pelo ProfileViewModel após receber o manifesto.
        /// </summary>
        public static void UpdateFromManifest(List<OptionalGroupInfo> groups, List<ManifestFile> allFiles)
        {
            _cachedGroups = groups ?? new List<OptionalGroupInfo>();
            _cachedGroupFiles.Clear();
            _cachedGroupTargetSubDir.Clear();
            _cachedGroupOffFolders.Clear();

            foreach (var group in _cachedGroups)
            {
                _cachedGroupTargetSubDir[group.id] = group.targetSubDir ?? "";

                if (group.offFolders != null && group.offFolders.Count > 0)
                {
                    _cachedGroupOffFolders[group.id] = group.offFolders;
                }
            }

            // Separar arquivos por grupo
            foreach (var file in allFiles)
            {
                if (!string.IsNullOrEmpty(file.optionalGroup))
                {
                    if (!_cachedGroupFiles.ContainsKey(file.optionalGroup))
                    {
                        _cachedGroupFiles[file.optionalGroup] = new List<ManifestFile>();
                    }
                    _cachedGroupFiles[file.optionalGroup].Add(file);
                }
            }

            LogManager.Instance.Info($"[OptionalMods] Cache atualizado: {_cachedGroups.Count} grupos, {_cachedGroupFiles.Values.Sum(l => l.Count)} arquivos opcionais");
        }

        /// <summary>
        /// Retorna a lista de grupos opcionais disponíveis.
        /// </summary>
        public static List<OptionalGroupInfo> GetCachedGroups() => _cachedGroups;

        // === Item 009 — descritores dos grupos (description.json por pasta em Opcionais/) ===

        /// <summary>
        /// Busca GET /launcher/mods/optionals-list e parseia com tolerância aos dois shapes:
        /// novo (item 009) { "folders": [ { "id", "name", "description": { "pt", "en" } } ] }
        /// e antigo { "folders": [ "PastaA", "PastaB" ] } (retrocompat com server antigo).
        /// Erro/timeout/shape inesperado ⇒ lista vazia (as descrições ficam como estão).
        /// </summary>
        public static async Task<List<OptionalFolderDescriptor>> FetchOptionalsListAsync()
        {
            var descriptors = new List<OptionalFolderDescriptor>();

            try
            {
                string response = await Task.Run(() => RequestHandler.RequestOptionalsList());
                if (string.IsNullOrWhiteSpace(response)) return descriptors;

                using var doc = JsonDocument.Parse(response);

                if (!doc.RootElement.TryGetProperty("folders", out var foldersProp)
                    || foldersProp.ValueKind != JsonValueKind.Array)
                {
                    return descriptors;
                }

                foreach (var entry in foldersProp.EnumerateArray())
                {
                    if (entry.ValueKind == JsonValueKind.String)
                    {
                        // shape antigo: só o nome da pasta
                        var folderName = entry.GetString();
                        if (!string.IsNullOrWhiteSpace(folderName))
                        {
                            descriptors.Add(new OptionalFolderDescriptor { Id = folderName, Name = folderName });
                        }

                        continue;
                    }

                    if (entry.ValueKind != JsonValueKind.Object) continue;

                    var descriptor = new OptionalFolderDescriptor();

                    if (entry.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                        descriptor.Id = idProp.GetString();
                    if (entry.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                        descriptor.Name = nameProp.GetString();

                    if (entry.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.Object)
                    {
                        if (descProp.TryGetProperty("pt", out var ptProp) && ptProp.ValueKind == JsonValueKind.String)
                            descriptor.DescriptionPt = ptProp.GetString();
                        if (descProp.TryGetProperty("en", out var enProp) && enProp.ValueKind == JsonValueKind.String)
                            descriptor.DescriptionEn = enProp.GetString();
                    }

                    if (string.IsNullOrWhiteSpace(descriptor.Id)) descriptor.Id = descriptor.Name;
                    if (string.IsNullOrWhiteSpace(descriptor.Name)) descriptor.Name = descriptor.Id;

                    if (!string.IsNullOrWhiteSpace(descriptor.Id))
                    {
                        descriptors.Add(descriptor);
                    }
                }

                LogManager.Instance.Info($"[OptionalMods] optionals-list: {descriptors.Count} grupo(s) com descritor");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[OptionalMods] Falha ao buscar optionals-list: {ex.Message}");
            }

            return descriptors;
        }

        /// <summary>Escolhe o texto no idioma preferido, com fallback pro outro (decisão D3 do item 009).</summary>
        public static string ResolveDescription(OptionalFolderDescriptor descriptor, bool preferPt)
        {
            if (descriptor == null) return null;

            return preferPt
                ? descriptor.DescriptionPt ?? descriptor.DescriptionEn
                : descriptor.DescriptionEn ?? descriptor.DescriptionPt;
        }

        /// <summary>
        /// Retorna TODOS os paths de arquivos opcionais conhecidos (ativos e inativos).
        /// Usado pela verificação de managedPaths para NÃO deletar opcionais.
        /// </summary>
        public static HashSet<string> GetAllKnownOptionalPaths()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in _cachedGroupFiles)
            {
                foreach (var file in kvp.Value)
                {
                    string normalized = file.path.Replace('/', Path.DirectorySeparatorChar).ToLowerInvariant();
                    paths.Add(normalized);
                }
            }

            return paths;
        }

        /// <summary>
        /// Baixa e instala todos os arquivos de um grupo opcional.
        /// </summary>
        public static async Task DownloadOptionalGroupAsync(string groupId)
        {
            if (!_cachedGroupFiles.ContainsKey(groupId))
            {
                LogManager.Instance.Warning($"[OptionalMods] Grupo '{groupId}' não encontrado no cache");
                return;
            }

            var files = _cachedGroupFiles[groupId];
            string baseUrl = GetServerBaseUrl();
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            int totalFiles = files.Count;
            int currentFile = 0;
            UpdateProgress(0, $"Ativando mod opcional...");

            foreach (var file in files)
            {
                currentFile++;
                UpdateProgress((currentFile / (double)totalFiles) * 100);

                try
                {
                    // ref: item 019 — resolve+valida sob a raiz ANTES de tocar disco: um path opcional
                    // adulterado com ".."/absoluto lança e cai no catch (Warning), sem escrever fora.
                    string localPath = SyncPathUtil.ResolveUnderRoot(GamePath, file.path);

                    // Verificar hash local antes de baixar
                    if (File.Exists(localPath))
                    {
                        string localHash = GetFileMd5(localPath);
                        if (localHash == file.hash)
                        {
                            continue; // Já atualizado
                        }
                    }

                    // Baixar do server (via endpoint de mods normal)
                    string encodedFile = Uri.EscapeDataString(file.path);
                    string downloadUrl = $"{baseUrl}/launcher/mods/download?file={encodedFile}";

                    var fileData = await client.GetByteArrayAsync(downloadUrl);

                    // item 019 — write atômico (temp+move), destino já validado sob a raiz
                    SyncFileOps.WriteAtomic(localPath, fileData);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[OptionalMods] Erro ao baixar {file.path}: {ex.Message}");
                }
            }

            LogManager.Instance.Info($"[OptionalMods] Grupo '{groupId}' ativado ({totalFiles} arquivos)");
        }

        /// <summary>
        /// Remove todos os arquivos de um grupo opcional. 
        /// Se o grupo tiver offFolders, baixa esses arquivos em vez de apenas deletar.
        /// </summary>
        public static async Task RemoveOptionalGroupAsync(string groupId)
        {
            // Se tem offFolders, baixar arquivos de desativação (ex: "Remover grama Off")
            if (_cachedGroupOffFolders.ContainsKey(groupId))
            {
                var offFolders = _cachedGroupOffFolders[groupId];
                foreach (var offFolder in offFolders)
                {
                    await DownloadFromOpcionaisFolder(offFolder, _cachedGroupTargetSubDir.GetValueOrDefault(groupId, ""));
                }
                LogManager.Instance.Info($"[OptionalMods] Grupo '{groupId}' desativado (offFolders aplicados)");
                return;
            }

            // Sem offFolders: deletar os arquivos
            if (!_cachedGroupFiles.ContainsKey(groupId))
            {
                LogManager.Instance.Warning($"[OptionalMods] Grupo '{groupId}' não encontrado no cache para remoção");
                return;
            }

            var files = _cachedGroupFiles[groupId];
            int total = files.Count;
            int count = 0;
            UpdateProgress(0, $"Desativando mod opcional...");

            foreach (var file in files)
            {
                count++;
                UpdateProgress((count / (double)total) * 100);

                try
                {
                    // ref: item 019 — resolve sob a raiz + lixeira (recuperável) em vez de File.Delete
                    // permanente; entrada adulterada com ".."/absoluto é rejeitada + logada, sem abortar.
                    string localPath = SyncPathUtil.ResolveUnderRoot(GamePath, file.path);
                    if (File.Exists(localPath)) RecycleBinHelper.Delete(localPath);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[OptionalMods] Erro ao remover {file.path}: {ex.Message}");
                }
            }

            LogManager.Instance.Info($"[OptionalMods] Grupo '{groupId}' desativado ({total} arquivos removidos)");
        }

        /// <summary>
        /// Baixa arquivos de uma subpasta de Opcionais do server (para offFolders).
        /// </summary>
        private static async Task DownloadFromOpcionaisFolder(string folderName, string targetSubDir)
        {
            string baseUrl = GetServerBaseUrl();
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);

            string encodedFolder = Uri.EscapeDataString(folderName);
            string manifestUrl = $"{baseUrl}/launcher/mods/optionals-manifest?folder={encodedFolder}";

            try
            {
                var response = await client.GetAsync(manifestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    LogManager.Instance.Warning($"[OptionalMods] Server retornou {response.StatusCode} para offFolder '{folderName}'");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var manifest = JsonSerializer.Deserialize<OptionalManifest>(json);

                if (manifest?.files == null || manifest.files.Length == 0) return;

                int total = manifest.files.Length;
                int count = 0;
                UpdateProgress(0, $"Aplicando configuração: {folderName}...");

                foreach (var file in manifest.files)
                {
                    count++;
                    UpdateProgress((count / (double)total) * 100);

                    try
                    {
                        string encodedFile = Uri.EscapeDataString(file.path);
                        string downloadUrl = $"{baseUrl}/launcher/mods/optional-download?folder={encodedFolder}&file={encodedFile}";
                        var fileData = await client.GetByteArrayAsync(downloadUrl);

                        // ref: item 019 (CA-6) — valida o destino FINAL (targetSubDir do grupo + file.path
                        // do offFolder, ambos vindos do server) sob a raiz numa só chamada; depois grava
                        // atômico. Traversal em qualquer um dos dois é rejeitado + logado, sem abortar.
                        string destPath = SyncPathUtil.ResolveUnderRoot(GamePath, Path.Combine(targetSubDir ?? "", file.path));
                        SyncFileOps.WriteAtomic(destPath, fileData);
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Warning($"[OptionalMods] Erro ao baixar {file.path}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[OptionalMods] Erro ao processar offFolder '{folderName}': {ex.Message}");
            }
        }

        private static string GetFileMd5(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static Action<double> OnProgressChanged;
        public static Action<string> OnStatusMessageChanged;

        private static void UpdateProgress(double percent, string message = null)
        {
            OnProgressChanged?.Invoke(percent);
            if (message != null)
                OnStatusMessageChanged?.Invoke(message);
        }

        // === Classes para desserialização ===

        public class OptionalGroupInfo
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public List<string> folders { get; set; }
            public List<string> offFolders { get; set; }
            public string targetSubDir { get; set; }
            public bool hasOffMode => offFolders != null && offFolders.Count > 0;
        }

        /// <summary>
        /// Item 009: entrada do optionals-list — descriptor (description.json) de um grupo
        /// em Launcher-Updater/Opcionais/&lt;grupo&gt;/ no server.
        /// </summary>
        public class OptionalFolderDescriptor
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DescriptionPt { get; set; }
            public string DescriptionEn { get; set; }
        }

        /// <summary>
        /// Shell kept for source compatibility (ProfileViewModel uses the nested name) —
        /// the canonical definition lives in SPT.Launcher.Models.Launcher.ManifestFile (item 007).
        /// </summary>
        public class ManifestFile : SPT.Launcher.Models.Launcher.ManifestFile
        {
        }

        private class OptionalManifest
        {
            public string folder { get; set; }
            public int totalFiles { get; set; }
            public ManifestFileSimple[] files { get; set; }
        }

        private class ManifestFileSimple
        {
            public string path { get; set; }
            public string hash { get; set; }
            public long size { get; set; }
        }
    }
}
