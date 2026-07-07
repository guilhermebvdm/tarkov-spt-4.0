using System;
using System.Collections.Generic;
using System.IO;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Item 021 — pure, injectable core of the optional-group download loop, extracted from
    /// OptionalModsHelper so it can be unit-tested without an exe/network (the downloader is a
    /// <see cref="Func{T,TResult}"/>). Reuses the SAME primitives as the sync engine: the root guard
    /// (<see cref="SyncPathUtil.ResolveUnderRoot"/>), the hash skip (<see cref="SyncPathUtil.ComputeMd5"/>)
    /// and the atomic write (<see cref="SyncFileOps.WriteAtomic"/>). Never throws for a per-file error:
    /// it counts the failure, records the path and keeps going (RN-2, CA-021.4/5/8/10).
    /// </summary>
    public static class OptionalGroupApplier
    {
        /// <summary>One file to apply: where it lands (<see cref="TargetPath"/>), what key the downloader
        /// receives (<see cref="DownloadKey"/> — the group endpoint keys on the target path, the offFolders
        /// endpoint keys on the in-folder path), and the expected md5 for the skip (nullable).</summary>
        public readonly struct Entry
        {
            public readonly string TargetPath;
            public readonly string DownloadKey;
            public readonly string Hash;

            public Entry(string targetPath, string downloadKey, string hash)
            {
                TargetPath = targetPath;
                DownloadKey = downloadKey;
                Hash = hash;
            }
        }

        /// <summary>
        /// Downloads (via <paramref name="downloader"/>) and atomically writes every entry under
        /// <paramref name="gameRoot"/>. A file already present with a matching hash is skipped
        /// (not re-downloaded, not a failure). A path that escapes the root, a download that throws
        /// or a write that fails is counted as a failure and the loop continues.
        /// </summary>
        /// <param name="gameRoot">Absolute game root; every target is contained under it.</param>
        /// <param name="files">Entries to apply.</param>
        /// <param name="downloader">Fetches the bytes for a <see cref="Entry.DownloadKey"/>; may throw.</param>
        /// <param name="localHasher">Local file hasher (defaults to <see cref="SyncPathUtil.ComputeMd5"/>); injectable for tests.</param>
        /// <param name="onProgress">(current, total) after each file, for UI progress marshaling.</param>
        /// <param name="onError">(relativePath, exception) for logging — the log stops being the only channel.</param>
        public static OptionalOpResult Apply(
            string gameRoot,
            IReadOnlyList<Entry> files,
            Func<string, byte[]> downloader,
            Func<string, string> localHasher = null,
            Action<int, int> onProgress = null,
            Action<string, Exception> onError = null)
        {
            var result = new OptionalOpResult { Total = files?.Count ?? 0 };
            if (files == null || files.Count == 0) return result;

            localHasher ??= SyncPathUtil.ComputeMd5;

            int current = 0;
            foreach (var file in files)
            {
                current++;
                onProgress?.Invoke(current, result.Total);

                try
                {
                    // Resolve+validate under the root BEFORE touching disk: a tampered target with
                    // ".."/absolute path throws here and is counted as a failure, never written outside.
                    string localPath = SyncPathUtil.ResolveUnderRoot(gameRoot, file.TargetPath);

                    if (!string.IsNullOrEmpty(file.Hash) && File.Exists(localPath))
                    {
                        string localHash = localHasher(localPath);
                        if (string.Equals(localHash, file.Hash, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Skipped++;
                            continue; // RN-3: already present, idempotent — not a download, not a failure
                        }
                    }

                    byte[] data = downloader(file.DownloadKey);
                    SyncFileOps.WriteAtomic(localPath, data); // atomic temp+move (CA-021.10)
                    result.Ok++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.FailedPaths.Add(file.TargetPath);
                    onError?.Invoke(file.TargetPath, ex);
                }
            }

            return result;
        }
    }
}
