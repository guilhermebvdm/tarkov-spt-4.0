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
    }
}
