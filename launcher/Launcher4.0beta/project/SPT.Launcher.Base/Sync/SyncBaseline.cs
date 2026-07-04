using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Persisted last-sync state (user/launcher/sync-state.json): MD5 per relative path,
    /// recorded after each successful apply. "Equal to baseline" = the user did not touch the
    /// file since the last sync; divergence from baseline = user customization (item 007).
    /// Missing or corrupt file → empty baseline (conservative first run).
    /// </summary>
    public sealed class SyncBaseline
    {
        private sealed class PersistedModel
        {
            public int version { get; set; } = 1;
            public Dictionary<string, string> files { get; set; } = new Dictionary<string, string>();
        }

        private readonly Dictionary<string, string> _files;

        public string FilePath { get; }

        public int Count => _files.Count;

        private SyncBaseline(string filePath, Dictionary<string, string> files)
        {
            FilePath = filePath;
            _files = files;
        }

        public static SyncBaseline Load(string filePath)
        {
            var files = new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                if (File.Exists(filePath))
                {
                    var model = JsonConvert.DeserializeObject<PersistedModel>(File.ReadAllText(filePath));

                    if (model?.files != null)
                    {
                        foreach (var kvp in model.files)
                        {
                            files[SyncPathUtil.Normalize(kvp.Key)] = kvp.Value;
                        }
                    }
                }
            }
            catch
            {
                // Corrupt baseline → treated as first run (conservative: preserve divergent files).
                files.Clear();
            }

            return new SyncBaseline(filePath, files);
        }

        public bool TryGetHash(string relativePath, out string hash)
        {
            return _files.TryGetValue(SyncPathUtil.Normalize(relativePath), out hash);
        }

        public void SetHash(string relativePath, string hash)
        {
            if (string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(hash)) return;
            _files[SyncPathUtil.Normalize(relativePath)] = hash.ToLowerInvariant();
        }

        public void Remove(string relativePath)
        {
            _files.Remove(SyncPathUtil.Normalize(relativePath));
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var model = new PersistedModel { files = new Dictionary<string, string>(_files) };
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(model, Formatting.Indented));
        }
    }
}
