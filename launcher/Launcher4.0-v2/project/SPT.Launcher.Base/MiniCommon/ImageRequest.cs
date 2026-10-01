/* ImageRequest.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 * waffle.lord
 */

using SPT.Launcher.Controllers;
using SPT.Launcher.Helpers;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace SPT.Launcher.MiniCommon
{

    public static class ImageRequest
    {
        public static string ImageCacheFolder = Path.Join(SPT.Launcher.Base.Helpers.SptPathHelper.SptRootPath, "SPT_Data", "Launcher", "Image_Cache");

        // ref: CR-01-04 — a infra virou multi-thread (ícones de classe baixados em paralelo pelo item
        // 004, em corrida potencial com CacheSideImage/CacheBackgroundImage): List sem lock trocada por
        // dicionários concorrentes + lock POR ROTA, para nunca haver dois escritores no mesmo arquivo.
        private static readonly ConcurrentDictionary<string, byte> CachedRoutes = new ConcurrentDictionary<string, byte>();
        private static readonly ConcurrentDictionary<string, object> RouteLocks = new ConcurrentDictionary<string, object>();

        private static string LauncherRoute = "/files/launcher/";
        public static void CacheBackgroundImage() => CacheImage($"{LauncherRoute}bg.png", Path.Combine(ImageCacheFolder, "bg.png"));
        public static void CacheSideImage(string Side)
        {
            if (Side == null || string.IsNullOrWhiteSpace(Side) || Side.ToLower() == "unknown") return;

            string SideImagePath = Path.Combine(ImageCacheFolder, $"side_{Side.ToLower()}.png");

            CacheImage($"{LauncherRoute}side_{Side.ToLower()}.png", SideImagePath);
        }

        /// <summary>
        /// Caches an image served by the CONNECTED backend (RequestHandler endpoint) under
        /// Image_Cache/<paramref name="fileName"/> and returns the local path, or null on failure.
        /// Used for CustomClasses class icons (item 004). Static files come raw (no zlib).
        /// </summary>
        public static string CacheServerImage(string route, string fileName)
        {
            if (string.IsNullOrWhiteSpace(route) || string.IsNullOrWhiteSpace(fileName)) return null;

            // ref: CR-01-06 — defesa em profundidade: API pública sanitiza aqui, sem confiar no caller
            // (basename apenas + rejeição de chars inválidos fecha traversal para qualquer caller futuro).
            fileName = Path.GetFileName(fileName);

            if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return null;

            // ref: CR-01-05 — chave de sessão prefixada pelo backend: troca de server na mesma sessão
            // (dev mode) re-baixa em vez de servir ícone stale do server anterior. O arquivo em disco
            // continua compartilhado — o re-download sobrescreve.
            string cacheKey = $"{RequestHandler.GetBackendUrl()}{route}";

            try
            {
                Directory.CreateDirectory(ImageCacheFolder);

                string filePath = Path.Combine(ImageCacheFolder, fileName);

                // ref: CR-01-04 — lock por rota: loads concorrentes (duplo-load entre instâncias do VM)
                // não disputam o File.Create do mesmo ícone; o perdedor espera e pega o cache hit.
                object routeLock = RouteLocks.GetOrAdd(cacheKey, _ => new object());

                lock (routeLock)
                {
                    if (CachedRoutes.ContainsKey(cacheKey) && File.Exists(filePath))
                    {
                        return filePath;
                    }

                    using Stream s = new Request(null, RequestHandler.GetBackendUrl()).Send(route, "GET", null, false);

                    if (s == null) return null;

                    using MemoryStream ms = new MemoryStream();

                    s.CopyTo(ms);

                    if (ms.Length == 0) return null;

                    using (FileStream fs = File.Create(filePath))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.CopyTo(fs);
                    }

                    CachedRoutes.TryAdd(cacheKey, 0);
                    return filePath;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                return null;
            }
        }

        private static void CacheImage(string route, string filePath)
        {
            try
            {
                Directory.CreateDirectory(ImageCacheFolder);

                if (String.IsNullOrWhiteSpace(route))
                {
                    return;
                }

                // ref: CR-01-04 — mesma disciplina de lock por rota da CacheServerImage (callers vanilla
                // preservados: assinatura, chave sem prefixo e semântica de sessão intactas).
                object routeLock = RouteLocks.GetOrAdd(route, _ => new object());

                lock (routeLock)
                {
                    if (CachedRoutes.ContainsKey(route)) //Don't want to request the image if it was already cached this session.
                    {
                        return;
                    }

                    using Stream s = new Request(null, LauncherSettingsProvider.Instance.Server.Url).Send(route, "GET", null, false);

                    using MemoryStream ms = new MemoryStream();

                    s.CopyTo(ms);

                    if (ms.Length == 0) return;

                    using (FileStream fs = File.Create(filePath))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.CopyTo(fs);
                    }

                    CachedRoutes.TryAdd(route, 0);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
            }
        }

        public static async System.Threading.Tasks.Task SyncCarouselImagesAsync(string serverUrl = null)
        {
            try
            {
                string baseUrl = !string.IsNullOrWhiteSpace(serverUrl) 
                    ? serverUrl 
                    : (ServerManager.SelectedServer?.backendUrl ?? LauncherSettingsProvider.Instance.Server?.Url);

                if (string.IsNullOrWhiteSpace(baseUrl)) return;

                string carouselDir = Path.Combine(ImageCacheFolder, "carrocel");
                if (!Directory.Exists(carouselDir))
                {
                    Directory.CreateDirectory(carouselDir);
                }

                string listUrl = $"{baseUrl.TrimEnd('/')}{RequestHandler.ModRoutePrefix}/redline/launcher/carousel";
                var handler = new System.Net.Http.HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };
                using var client = new System.Net.Http.HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync(listUrl);
                if (!response.IsSuccessStatusCode) return;

                string json = await response.Content.ReadAsStringAsync();
                var data = Newtonsoft.Json.Linq.JObject.Parse(json);
                var images = data["images"]?.ToObject<System.Collections.Generic.List<string>>();
                if (images == null) return;

                var serverFileSet = new System.Collections.Generic.HashSet<string>(images, StringComparer.OrdinalIgnoreCase);

                // 1. Baixar imagens novas
                foreach (string fileName in images)
                {
                    string safeName = Path.GetFileName(fileName);
                    string localPath = Path.Combine(carouselDir, safeName);

                    if (!File.Exists(localPath))
                    {
                        string downloadUrl = $"{baseUrl.TrimEnd('/')}{RequestHandler.ModRoutePrefix}/redline/launcher/carousel/{Uri.EscapeDataString(safeName)}";
                        byte[] bytes = await client.GetByteArrayAsync(downloadUrl);
                        if (bytes != null && bytes.Length > 0)
                        {
                            await File.WriteAllBytesAsync(localPath, bytes);
                            LogManager.Instance.Info($"[ImageRequest] Nova imagem do carrossel baixada: {safeName} ({bytes.Length / 1024} KB)");
                        }
                    }
                }

                // 2. Limpar imagens locais deletadas do servidor
                foreach (string localFile in Directory.EnumerateFiles(carouselDir))
                {
                    string localName = Path.GetFileName(localFile);
                    if (!serverFileSet.Contains(localName))
                    {
                        try
                        {
                            File.Delete(localFile);
                            LogManager.Instance.Info($"[ImageRequest] Imagem do carrossel removida (já não existe no servidor): {localName}");
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[ImageRequest] Falha na sincronização do carrossel: {ex.Message}");
            }
        }
    }
}
