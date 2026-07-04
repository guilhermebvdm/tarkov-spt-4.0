using System;
using System.IO;
using System.Security.Cryptography;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Path normalization + hashing helpers shared by the sync engine.
    /// Canonical relative-path form: forward slashes, no leading slash, lower-case.
    /// </summary>
    public static class SyncPathUtil
    {
        /// <summary>Normalizes a relative path to the canonical comparison form.</summary>
        public static string Normalize(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return string.Empty;
            return relativePath.Replace('\\', '/').TrimStart('/').TrimEnd('/').ToLowerInvariant();
        }

        /// <summary>Converts a manifest-style relative path to an absolute local path under <paramref name="root"/>.</summary>
        public static string ToLocalPath(string root, string relativePath)
        {
            return Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
        }

        /// <summary>True when <paramref name="normalizedPath"/> is equal to or nested under <paramref name="normalizedPrefix"/>.</summary>
        public static bool IsUnderPrefix(string normalizedPath, string normalizedPrefix)
        {
            if (string.IsNullOrEmpty(normalizedPrefix)) return false;
            return normalizedPath.Equals(normalizedPrefix, StringComparison.Ordinal)
                   || normalizedPath.StartsWith(normalizedPrefix + "/", StringComparison.Ordinal);
        }

        /// <summary>
        /// Item 017: maps a SERVER seed-source path (e.g. "BepInEx/config-server/a/x.cfg") to the
        /// USER target under the sibling folder without the "-server" suffix ("BepInEx/config/a/x.cfg").
        /// "Same name" = relative path within the folder, so subfolders are preserved and keep their
        /// original casing. Returns null when there is no file remainder after the prefix, or when
        /// the inputs are empty.
        /// </summary>
        /// <param name="originalPath">Manifest path, original casing (may use back- or forward slashes).</param>
        /// <param name="normalizedMatchedPrefix">The seed-rule prefix returned by <see cref="SyncRuleResolver.Resolve(string, out string)"/> (normalized/lower-case, length-preserving vs the original).</param>
        public static string DeriveSeedTarget(string originalPath, string normalizedMatchedPrefix)
        {
            if (string.IsNullOrEmpty(originalPath) || string.IsNullOrEmpty(normalizedMatchedPrefix))
            {
                return null;
            }

            string forward = originalPath.Replace('\\', '/').TrimStart('/');
            if (forward.Length < normalizedMatchedPrefix.Length)
            {
                return null;
            }

            // Normalize() only lower-cases + swaps slashes (length-preserving), so the prefix span
            // aligns byte-for-byte with the original — Substring recovers the original casing.
            string originalPrefix = forward.Substring(0, normalizedMatchedPrefix.Length);
            string remainder = forward.Substring(normalizedMatchedPrefix.Length).TrimStart('/');
            if (remainder.Length == 0)
            {
                return null;
            }

            const string serverSuffix = "-server";
            string targetPrefix = originalPrefix.EndsWith(serverSuffix, StringComparison.OrdinalIgnoreCase)
                ? originalPrefix.Substring(0, originalPrefix.Length - serverSuffix.Length)
                : originalPrefix; // non "-server" seed prefix (operator misconfig): seeds into itself

            return targetPrefix + "/" + remainder;
        }

        /// <summary>True when any segment of the path ends with "-disabled" (quarantine folders are never re-synced).</summary>
        public static bool ContainsDisabledSegment(string normalizedPath)
        {
            foreach (var segment in normalizedPath.Split('/'))
            {
                if (segment.EndsWith("-disabled", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>MD5 hex (lower-case) of a file — same format used by the server manifest.</summary>
        public static string ComputeMd5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// MD5 hex (lower-case) of an in-memory buffer.
        /// ref: CR-01-05 (008) — the engine hashes the bytes it actually writes so the baseline
        /// reflects the DISK, not a possibly stale manifest hash.
        /// </summary>
        public static string ComputeMd5(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data ?? Array.Empty<byte>());
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
