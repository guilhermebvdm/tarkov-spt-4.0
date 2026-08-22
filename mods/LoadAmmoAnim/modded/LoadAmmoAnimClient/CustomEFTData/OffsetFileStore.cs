using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    /// <summary>
    /// Persists per-magazine calibration offsets to <c>offsets.json</c> next to the plugin DLL.
    /// Entries are keyed by magazine TemplateId (24-char MongoDB ObjectId string).
    ///
    /// Thread-safety model:
    ///   - <see cref="FileSystemWatcher"/> runs on a system thread → only sets a volatile flag.
    ///   - Actual disk I/O (Load / Save) always happens on the Unity Main Thread via
    ///     <see cref="TryGet"/> or <see cref="Save"/>, both called from LateUpdate.
    ///   - <see cref="ReaderWriterLockSlim"/> guards the in-memory dictionary against
    ///     concurrent reads during a save.
    /// </summary>
    public static class OffsetFileStore
    {
        // ── serialized shape ────────────────────────────────────────────────

        private class OffsetEntry
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("templateId")]
            public string TemplateId { get; set; }

            [JsonProperty("magPosition")]
            public Vec3 MagPosition { get; set; }

            [JsonProperty("magRotation")]
            public Vec3 MagRotation { get; set; }

            [JsonProperty("bulletPosition")]
            public Vec3 BulletPosition { get; set; }

            [JsonProperty("bulletRotation")]
            public Vec3 BulletRotation { get; set; }
        }

        private struct Vec3
        {
            [JsonProperty("x")] public float X;
            [JsonProperty("y")] public float Y;
            [JsonProperty("z")] public float Z;

            public static Vec3 From(Vector3 v) => new Vec3
            {
                X = (float)Math.Round(v.x, 4),
                Y = (float)Math.Round(v.y, 4),
                Z = (float)Math.Round(v.z, 4)
            };

            public static Vec3 FromEuler(Vector3 euler) => new Vec3
            {
                X = (float)Math.Round(NormalizeAngle(euler.x), 2),
                Y = (float)Math.Round(NormalizeAngle(euler.y), 2),
                Z = (float)Math.Round(NormalizeAngle(euler.z), 2)
            };

            private static float NormalizeAngle(float angle)
            {
                while (angle > 180f) angle -= 360f;
                while (angle < -180f) angle += 360f;
                if (Math.Abs(angle) < 0.001f) return 0f;
                return angle;
            }

            public Vector3 ToVector3() => new Vector3(X, Y, Z);
        }

        // ── state ────────────────────────────────────────────────────────────

        private static string _filePath;
        private static readonly Dictionary<string, OffsetEntry> _cache =
            new Dictionary<string, OffsetEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static FileSystemWatcher _watcher;

        // Set by the FileSystemWatcher thread; consumed on the Main Thread in TryGet.
        private static volatile bool _reloadPending;

        // ── public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Call once from <c>Plugin.Awake()</c>.
        /// </summary>
        public static void Initialize(string pluginDir)
        {
            _filePath = Path.Combine(pluginDir, "offsets.json");

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "{}");

            Load();
            StartWatcher(pluginDir);
        }

        /// <summary>
        /// Returns true and fills <paramref name="data"/> when a per-magazine entry exists for
        /// <paramref name="templateId"/>. Automatically hot-reloads the file if an external
        /// write was detected. Must be called on the Unity Main Thread.
        /// </summary>
        public static bool TryGet(string templateId, out OffsetData data)
        {
            // Hot-reload gate — always on Main Thread, so no race with Load().
            if (_reloadPending)
            {
                _reloadPending = false;
                Load();
            }

            if (string.IsNullOrEmpty(templateId))
            {
                data = default;
                return false;
            }

            _lock.EnterReadLock();
            try
            {
                if (_cache.TryGetValue(templateId, out var entry))
                {
                    data = EntryToOffsetData(entry);
                    return true;
                }
            }
            finally { _lock.ExitReadLock(); }

            data = default;
            return false;
        }

        /// <summary>
        /// Upserts the offset for <paramref name="templateId"/> and flushes to disk immediately.
        /// The values in <paramref name="absoluteData"/> must already be the final absolute
        /// world-space offsets (BasePos + caliberDelta + sliderDelta). Must be called on the
        /// Unity Main Thread.
        /// </summary>
        public static void Save(string templateId, string magName, OffsetData absoluteData)
        {
            if (string.IsNullOrEmpty(templateId)) return;

            var entry = new OffsetEntry
            {
                Name         = magName ?? templateId,
                TemplateId   = templateId,
                MagPosition  = Vec3.From(absoluteData.MagPosition),
                MagRotation  = Vec3.FromEuler(absoluteData.MagRotation.eulerAngles),
                BulletPosition = Vec3.From(absoluteData.BulletPosition),
                BulletRotation = Vec3.FromEuler(absoluteData.BulletRotation.eulerAngles),
            };

            _lock.EnterWriteLock();
            try { _cache[templateId] = entry; }
            finally { _lock.ExitWriteLock(); }

            Flush();
        }

        // ── private helpers ──────────────────────────────────────────────────

        private static void Load()
        {
            if (string.IsNullOrEmpty(_filePath) || !File.Exists(_filePath)) return;

            try
            {
                string json = File.ReadAllText(_filePath);
                var raw = JsonConvert.DeserializeObject<Dictionary<string, OffsetEntry>>(json);
                if (raw == null) return;

                _lock.EnterWriteLock();
                try
                {
                    _cache.Clear();
                    foreach (var kv in raw)
                        _cache[kv.Key] = kv.Value;
                }
                finally { _lock.ExitWriteLock(); }

                Plugin.LogSource?.LogInfo($"[LoadAmmoAnim] OffsetFileStore loaded {_cache.Count} entries from offsets.json");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] OffsetFileStore.Load failed: {ex.Message}");
            }
        }

        private static void Flush()
        {
            try
            {
                Dictionary<string, OffsetEntry> snapshot;
                _lock.EnterReadLock();
                try { snapshot = new Dictionary<string, OffsetEntry>(_cache); }
                finally { _lock.ExitReadLock(); }

                // Suppress watcher so our own write doesn't trigger a reload.
                if (_watcher != null) _watcher.EnableRaisingEvents = false;

                string json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);
                File.WriteAllText(_filePath, json);

                if (_watcher != null) _watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] OffsetFileStore.Flush failed: {ex.Message}");
            }
        }

        private static void StartWatcher(string dir)
        {
            try
            {
                _watcher = new FileSystemWatcher(dir, "offsets.json")
                {
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };
                // Only set the flag — no Unity API calls here (wrong thread).
                _watcher.Changed += (_, __) => { _reloadPending = true; };
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] OffsetFileStore watcher failed to start: {ex.Message}");
            }
        }

        private static OffsetData EntryToOffsetData(OffsetEntry e) => new OffsetData
        {
            MagPosition    = e.MagPosition.ToVector3(),
            MagRotation    = Quaternion.Euler(e.MagRotation.ToVector3()),
            BulletPosition = e.BulletPosition.ToVector3(),
            BulletRotation = Quaternion.Euler(e.BulletRotation.ToVector3()),
        };
    }
}
