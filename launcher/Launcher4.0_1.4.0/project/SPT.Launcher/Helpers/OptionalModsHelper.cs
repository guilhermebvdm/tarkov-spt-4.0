using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;

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
                return $"http://{uri.Host}:7075";
            }
            catch
            {
                return "http://127.0.0.1:7075";
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
                    string relativePath = file.path.Replace("/", Path.DirectorySeparatorChar.ToString());
                    string localPath = Path.Combine(GamePath, relativePath);

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

                    // Salvar no GamePath
                    string destDir = Path.GetDirectoryName(localPath);
                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                    File.WriteAllBytes(localPath, fileData);
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

                string relativePath = file.path.Replace("/", Path.DirectorySeparatorChar.ToString());
                string localPath = Path.Combine(GamePath, relativePath);
                DeleteFileIfExists(localPath);
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

                string baseDestPath = string.IsNullOrEmpty(targetSubDir) ? GamePath : Path.Combine(GamePath, targetSubDir);
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

                        string destPath = Path.Combine(baseDestPath, file.path.Replace("/", Path.DirectorySeparatorChar.ToString()));
                        string destDir = Path.GetDirectoryName(destPath);
                        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
                        File.WriteAllBytes(destPath, fileData);
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

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
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

        public class ManifestFile
        {
            public string path { get; set; }
            public string hash { get; set; }
            public long size { get; set; }
            public bool optional { get; set; }
            public string optionalGroup { get; set; }
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
