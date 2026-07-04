using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Item 021: retorna <see cref="OptionalOpResult"/> (Total/Ok/Skipped/Failed) para a UI
        /// mostrar falha visível (CA-021.4/5); download+hash+escrita rodam off-thread (CA-021.7)
        /// via <see cref="OptionalGroupApplier"/>, que reusa guard de raiz + escrita atômica do motor.
        /// A via de rede é o <see cref="RequestHandler"/> (WebRequest — honra o bypass TLS e usa o
        /// esquema+porta reais do backend), não mais HttpClient cru com URL http:80 (CA-021.1/2/3).
        /// </summary>
        public static async Task<OptionalOpResult> DownloadOptionalGroupAsync(string groupId)
        {
            if (!_cachedGroupFiles.TryGetValue(groupId, out var files) || files == null || files.Count == 0)
            {
                // CC-5: cache vazio para o grupo — antes logava Warning e "sucesso" silencioso;
                // agora sinaliza não-resolvido para virar estado de erro na UI.
                LogManager.Instance.Warning($"[OptionalMods] Grupo '{groupId}' não encontrado no cache");
                return new OptionalOpResult { GroupResolved = false };
            }

            UpdateProgress(0, "Ativando mod opcional...");

            var entries = files
                .Select(f => new OptionalGroupApplier.Entry(f.path, f.path, f.hash))
                .ToList();

            string gamePath = GamePath;

            var result = await Task.Run(() => OptionalGroupApplier.Apply(
                gamePath,
                entries,
                // R-1: timeout largo (5 min) p/ binários grandes, não os 30 s default do sync.
                downloadKey => RequestHandler.DownloadModFile(downloadKey, 300000),
                onProgress: (current, total) => UpdateProgress((current / (double)total) * 100),
                onError: (path, ex) => LogManager.Instance.Warning($"[OptionalMods] Erro ao baixar {path}: {ex.Message}")));

            LogManager.Instance.Info(
                $"[OptionalMods] Grupo '{groupId}' — ok:{result.Ok} skip:{result.Skipped} falha:{result.Failed} (de {result.Total})");
            return result;
        }

        /// <summary>
        /// Remove todos os arquivos de um grupo opcional.
        /// Se o grupo tiver offFolders, baixa esses arquivos em vez de apenas deletar (CC-2).
        /// Item 021: retorna <see cref="OptionalOpResult"/> e roda a exclusão off-thread (CA-021.7);
        /// a exclusão vai para a lixeira (item 019) e o guard de raiz continua (CA-021.8/9).
        /// </summary>
        public static async Task<OptionalOpResult> RemoveOptionalGroupAsync(string groupId)
        {
            // Se tem offFolders, baixar arquivos de desativação (ex: "Remover grama Off") — CC-2.
            if (_cachedGroupOffFolders.TryGetValue(groupId, out var offFolders))
            {
                var aggregate = new OptionalOpResult();
                string targetSubDir = _cachedGroupTargetSubDir.GetValueOrDefault(groupId, "");
                foreach (var offFolder in offFolders)
                {
                    Merge(aggregate, await DownloadFromOpcionaisFolder(offFolder, targetSubDir));
                }
                LogManager.Instance.Info(
                    $"[OptionalMods] Grupo '{groupId}' desativado (offFolders) — ok:{aggregate.Ok} skip:{aggregate.Skipped} falha:{aggregate.Failed}");
                return aggregate;
            }

            // Sem offFolders: deletar os arquivos
            if (!_cachedGroupFiles.TryGetValue(groupId, out var files) || files == null || files.Count == 0)
            {
                LogManager.Instance.Warning($"[OptionalMods] Grupo '{groupId}' não encontrado no cache para remoção");
                return new OptionalOpResult { GroupResolved = false };
            }

            UpdateProgress(0, "Desativando mod opcional...");
            string gamePath = GamePath;

            var result = await Task.Run(() =>
            {
                var r = new OptionalOpResult { Total = files.Count };
                int count = 0;
                foreach (var file in files)
                {
                    count++;
                    UpdateProgress((count / (double)files.Count) * 100);

                    try
                    {
                        // ref: item 019 — resolve sob a raiz + lixeira (recuperável) em vez de File.Delete
                        // permanente; entrada adulterada com ".."/absoluto é rejeitada + contada como falha.
                        string localPath = SyncPathUtil.ResolveUnderRoot(gamePath, file.path);
                        if (File.Exists(localPath)) RecycleBinHelper.Delete(localPath);
                        r.Ok++;
                    }
                    catch (Exception ex)
                    {
                        r.Failed++;
                        r.FailedPaths.Add(file.path);
                        LogManager.Instance.Warning($"[OptionalMods] Erro ao remover {file.path}: {ex.Message}");
                    }
                }
                return r;
            });

            LogManager.Instance.Info(
                $"[OptionalMods] Grupo '{groupId}' desativado — ok:{result.Ok} falha:{result.Failed} (de {result.Total})");
            return result;
        }

        /// <summary>Soma um resultado parcial (offFolder) no agregado do grupo.</summary>
        private static void Merge(OptionalOpResult into, OptionalOpResult from)
        {
            into.Total += from.Total;
            into.Ok += from.Ok;
            into.Skipped += from.Skipped;
            into.Failed += from.Failed;
            into.FailedPaths.AddRange(from.FailedPaths);
            if (!from.GroupResolved) into.GroupResolved = false;
        }

        /// <summary>
        /// Baixa arquivos de uma subpasta de Opcionais do server (para offFolders).
        /// Item 021: via <see cref="RequestHandler"/> (WebRequest — CA-021.1/2/3, CC-2), off-thread,
        /// retornando <see cref="OptionalOpResult"/>. Manifesto/deserialização com falha viram falha
        /// visível (não silêncio). Escrita atômica + guard de raiz do destino final preservados.
        /// </summary>
        private static async Task<OptionalOpResult> DownloadFromOpcionaisFolder(string folderName, string targetSubDir)
        {
            string gamePath = GamePath;

            return await Task.Run(() =>
            {
                string json;
                try
                {
                    json = RequestHandler.RequestOptionalsManifest(folderName);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[OptionalMods] Erro ao buscar manifesto do offFolder '{folderName}': {ex.Message}");
                    return new OptionalOpResult { Total = 1, Failed = 1, FailedPaths = { folderName } };
                }

                OptionalManifest manifest;
                try
                {
                    manifest = JsonSerializer.Deserialize<OptionalManifest>(json);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[OptionalMods] Manifesto inválido do offFolder '{folderName}': {ex.Message}");
                    return new OptionalOpResult { Total = 1, Failed = 1, FailedPaths = { folderName } };
                }

                if (manifest?.files == null || manifest.files.Length == 0)
                {
                    return new OptionalOpResult();
                }

                UpdateProgress(0, $"Aplicando configuração: {folderName}...");

                // ref: item 019 (CA-6) — o destino FINAL (targetSubDir do grupo + file.path do offFolder,
                // ambos do server) é validado sob a raiz dentro do Apply; a chave de download é o file.path
                // dentro da pasta (endpoint optional-download?folder=&file=).
                var entries = manifest.files
                    .Select(f => new OptionalGroupApplier.Entry(
                        Path.Combine(targetSubDir ?? "", f.path),
                        f.path,
                        f.hash))
                    .ToList();

                return OptionalGroupApplier.Apply(
                    gamePath,
                    entries,
                    downloadKey => RequestHandler.DownloadOptionalFile(folderName, downloadKey, 300000),
                    onProgress: (current, total) => UpdateProgress((current / (double)total) * 100),
                    onError: (path, ex) => LogManager.Instance.Warning($"[OptionalMods] Erro ao baixar {path}: {ex.Message}"));
            });
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
