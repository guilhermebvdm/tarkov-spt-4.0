using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    /// <summary>
    /// Gerencia a lista de carregadores excluídos/banidos de executar animação 3D de recarga (ex: tambores de revólver).
    /// Persiste em <c>BanAnimation.json</c> ao lado da DLL do plugin.
    ///
    /// Thread-safety e Hot-reload:
    ///   - <see cref="FileSystemWatcher"/> monitora alterações em tempo de execução.
    ///   - Disk I/O e consultas sincronizadas via <see cref="ReaderWriterLockSlim"/>.
    /// </summary>
    public static class BanAnimationStore
    {
        public class BanEntry
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("comment")]
            public string Comment { get; set; }
        }

        // Parent IDs do EFT referentes a tambores de revólver e cilindros
        private const string CylinderMagazineParentId = "610720f290b75a49ff2e5e25";
        private const string SpringDrivenCylinderParentId = "627a137bf21bc425b06ab944";

        private static string _filePath;
        private static readonly Dictionary<string, BanEntry> _cache =
            new Dictionary<string, BanEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static FileSystemWatcher _watcher;
        private static volatile bool _reloadPending;

        /// <summary>
        /// Inicializa a store e cria o arquivo BanAnimation.json com os padrões caso não exista.
        /// </summary>
        public static void Initialize(string pluginDir)
        {
            _filePath = Path.Combine(pluginDir, "BanAnimation.json");

            if (!File.Exists(_filePath))
            {
                CreateDefaultFile();
            }

            Load();
            StartWatcher(pluginDir);
        }

        /// <summary>
        /// Verifica se um determinado carregador está banido de executar animação.
        /// </summary>
        public static bool IsBanned(string templateId, MagazineItemClass mag = null)
        {
            if (_reloadPending)
            {
                _reloadPending = false;
                Load();
            }

            if (!string.IsNullOrEmpty(templateId))
            {
                _lock.EnterReadLock();
                try
                {
                    if (_cache.ContainsKey(templateId))
                        return true;
                }
                finally { _lock.ExitReadLock(); }
            }

            // Checagem heurística defensiva nativa para tambores de revólver não mapeados
            if (mag != null && mag.Template != null)
            {
                string parentId = mag.Template.ParentId;
                if (string.Equals(parentId, CylinderMagazineParentId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(parentId, SpringDrivenCylinderParentId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                string name = mag.Template.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    if (name.IndexOf("cylinder", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("speedloader", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("ks23", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("ks_23", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Adiciona ou atualiza um carregador na lista de banidos e salva no disco.
        /// </summary>
        public static void Ban(string templateId, string name, string comment = null)
        {
            if (string.IsNullOrEmpty(templateId)) return;

            var entry = new BanEntry
            {
                Name = name ?? templateId,
                Comment = comment ?? "Banned magazine"
            };

            _lock.EnterWriteLock();
            try
            {
                _cache[templateId] = entry;
            }
            finally { _lock.ExitWriteLock(); }

            Flush();
        }

        /// <summary>
        /// Remove um carregador da lista de banidos e salva no disco.
        /// </summary>
        public static void Unban(string templateId)
        {
            if (string.IsNullOrEmpty(templateId)) return;

            _lock.EnterWriteLock();
            try
            {
                _cache.Remove(templateId);
            }
            finally { _lock.ExitWriteLock(); }

            Flush();
        }

        private static void CreateDefaultFile()
        {
            try
            {
                var defaults = new Dictionary<string, BanEntry>(StringComparer.OrdinalIgnoreCase)
                {
                    ["6a82c653a2f1c403c8978aa7"] = new BanEntry
                    {
                        Name = "mag_ks23_toz_ks23_std_23x75_3_cap",
                        Comment = "KS-23M 3-round 23x75mm magazine tube cap"
                    },
                    ["5f647d9f8499b57dc40ddb93"] = new BanEntry
                    {
                        Name = "mag_ks23_toz_ks23_std_23x75_3",
                        Comment = "KS-23M 3-round 23x75mm magazine tube"
                    },
                    ["60dc519adf4c47305f6d410d"] = new BanEntry
                    {
                        Name = "mag_mc255_ckib_mc255_cylinder_std_12g_5",
                        Comment = "MTs-255-12 12ga 5-round cylinder"
                    },
                    ["619f54a1d25cbd424731fb99"] = new BanEntry
                    {
                        Name = "mag_rhino_chiappa_rhino_cylinder_9x33r_6",
                        Comment = "Chiappa Rhino .357 6-round cylinder"
                    },
                    ["61a4cda622af7f4f6a3ce617"] = new BanEntry
                    {
                        Name = "mag_rhino_chiappa_rhino_speedloader_9x33R_6",
                        Comment = "Chiappa Rhino .357 6-round speedloader"
                    },
                    ["624c3074dbbd335e8e6becf3"] = new BanEntry
                    {
                        Name = "mag_rhino_chiappa_rhino_cylinder_9x19",
                        Comment = "Chiappa Rhino 9x19 6-round cylinder"
                    },
                    ["627bce33f21bc425b06ab967"] = new BanEntry
                    {
                        Name = "mag_msgl_milkor_cylinder_mag_std_40x46_6",
                        Comment = "Milkor M32A1 MSGL 40x46mm 6-round cylinder"
                    },
                    ["633ec6ee025b096d320a3b15"] = new BanEntry
                    {
                        Name = "mag_rsh12_kbp_rsh12_cylinder_127x55_5",
                        Comment = "RSh-12 12.7x55 5-round cylinder"
                    }
                };

                string json = JsonConvert.SerializeObject(defaults, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] BanAnimationStore.CreateDefaultFile failed: {ex.Message}");
            }
        }

        private static void Load()
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath)) return;

            try
            {
                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json)) return;

                JToken token = JToken.Parse(json);
                var loaded = new Dictionary<string, BanEntry>(StringComparer.OrdinalIgnoreCase);

                if (token.Type == JTokenType.Object)
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);
                    if (dict != null)
                    {
                        foreach (var kv in dict)
                        {
                            if (kv.Value.Type == JTokenType.Object)
                            {
                                var entry = kv.Value.ToObject<BanEntry>() ?? new BanEntry { Name = kv.Key };
                                loaded[kv.Key] = entry;
                            }
                            else if (kv.Value.Type == JTokenType.Boolean && kv.Value.Value<bool>())
                            {
                                loaded[kv.Key] = new BanEntry { Name = kv.Key, Comment = "Banned" };
                            }
                            else if (kv.Value.Type == JTokenType.String)
                            {
                                loaded[kv.Key] = new BanEntry { Name = kv.Key, Comment = kv.Value.Value<string>() };
                            }
                        }
                    }
                }
                else if (token.Type == JTokenType.Array)
                {
                    var list = JsonConvert.DeserializeObject<List<string>>(json);
                    if (list != null)
                    {
                        foreach (var id in list)
                        {
                            if (!string.IsNullOrEmpty(id))
                                loaded[id] = new BanEntry { Name = id, Comment = "Banned in array" };
                        }
                    }
                }

                _lock.EnterWriteLock();
                try
                {
                    _cache.Clear();
                    foreach (var kv in loaded)
                        _cache[kv.Key] = kv.Value;
                }
                finally { _lock.ExitWriteLock(); }

                Plugin.LogSource?.LogInfo($"[LoadAmmoAnim] BanAnimationStore loaded {_cache.Count} banned magazine entries from BanAnimation.json");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] BanAnimationStore.Load failed: {ex.Message}");
            }
        }

        private static void Flush()
        {
            try
            {
                Dictionary<string, BanEntry> snapshot;
                _lock.EnterReadLock();
                try { snapshot = new Dictionary<string, BanEntry>(_cache); }
                finally { _lock.ExitReadLock(); }

                if (_watcher != null) _watcher.EnableRaisingEvents = false;

                string json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);
                File.WriteAllText(_filePath, json);

                if (_watcher != null) _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] BanAnimationStore.Flush failed: {ex.Message}");
            }
        }

        private static void StartWatcher(string dir)
        {
            try
            {
                _watcher = new FileSystemWatcher(dir, "BanAnimation.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };
                _watcher.Changed += (_, __) => { _reloadPending = true; };
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] BanAnimationStore watcher failed to start: {ex.Message}");
            }
        }
    }
}
