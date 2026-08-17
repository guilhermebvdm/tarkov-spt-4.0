using UnityEngine;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;

namespace TRLImmersiveCombatMedicine.Helpers
{
    /// <summary>
    /// Carrega PNGs do disco e converte para Sprites Unity.
    /// Cache automático — cada imagem é carregada apenas uma vez.
    /// </summary>
    public static class ImageLoader
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("BandAid_Img");
        private static Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private static string _assetsPath;
        private static bool _initialized = false;

        /// <summary>
        /// Inicializa o caminho da pasta de assets.
        /// Deve ser chamado uma vez no Awake do plugin.
        /// </summary>
        public static void Init(string pluginPath)
        {
            _assetsPath = Path.Combine(pluginPath, "Silhueta");
            if (!Directory.Exists(_assetsPath))
            {
                Directory.CreateDirectory(_assetsPath);
                Logger.LogInfo($"Pasta Assets criada: {_assetsPath}");
            }
            _initialized = true;
            Logger.LogInfo($"ImageLoader inicializado. Path: {_assetsPath}");
        }

        /// <summary>
        /// Carrega um sprite PNG pelo nome do arquivo (sem extensão).
        /// Retorna null se o arquivo não existir.
        /// </summary>
        public static Sprite Load(string imageName)
        {
            if (!_initialized) return null;

            // Retorna do cache se já carregou
            if (_cache.TryGetValue(imageName, out Sprite cached))
                return cached;

            string filePath = Path.Combine(_assetsPath, imageName + ".png");
            if (!File.Exists(filePath))
                return null;

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;

                if (ImageConversion.LoadImage(tex, bytes))
                {
                    Sprite sprite = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), 100f);

                    _cache[imageName] = sprite;
                    Logger.LogInfo($"Imagem carregada: {imageName} ({tex.width}x{tex.height})");
                    return sprite;
                }
                else
                {
                    Logger.LogWarning($"Falha ao decodificar: {imageName}");
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Erro ao carregar {imageName}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Verifica se uma imagem existe na pasta Assets.
        /// </summary>
        public static bool Exists(string imageName)
        {
            if (!_initialized) return false;
            return File.Exists(Path.Combine(_assetsPath, imageName + ".png"));
        }

        /// <summary>
        /// Retorna o caminho completo da pasta Assets.
        /// </summary>
        public static string AssetsPath => _assetsPath;

        /// <summary>
        /// Limpa o cache de sprites (uso em reload).
        /// </summary>
        public static void ClearCache()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value != null && kvp.Value.texture != null)
                    Object.Destroy(kvp.Value.texture);
                if (kvp.Value != null)
                    Object.Destroy(kvp.Value);
            }
            _cache.Clear();
        }
    }
}
