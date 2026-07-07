using System;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 018 (auto-update-security / CA-8) — pure, testable version comparison for the auto-update.
    /// Extracted to .Base so it can be unit-tested and so it centralizes the "+hash" cleanup the client
    /// used to lack (the server already strips '+', the client did not — R-4). Fail-SAFE: any
    /// unparseable input is treated as "NOT newer" so garbage never triggers an update.
    /// Does NOT touch item 014's single-source <c>CurrentVersion</c> (still read from AssemblyVersion).
    /// </summary>
    public static class UpdateVersion
    {
        /// <summary>
        /// Trims whitespace and drops any build-metadata suffix (everything from the first '+'),
        /// e.g. "2.1.0+abc123" → "2.1.0". Returns "" for null/blank so the parse cleanly fails-safe.
        /// </summary>
        public static string Normalize(string version)
        {
            if (string.IsNullOrWhiteSpace(version)) return string.Empty;

            string v = version.Trim();

            int plus = v.IndexOf('+');
            if (plus >= 0)
            {
                v = v.Substring(0, plus);
            }

            return v.Trim();
        }

        /// <summary>
        /// <c>true</c> iff <paramref name="candidateVersion"/> parses to a strictly greater
        /// <see cref="Version"/> than <paramref name="currentVersion"/>. Either side unparseable
        /// (empty, "1.0" is fine, "2.1.0+hash" cleaned first, "abc" → false) → <c>false</c>
        /// (fail-safe: never trigger an update on ambiguous input, CA-8). Never throws.
        /// </summary>
        public static bool IsNewerVersion(string currentVersion, string candidateVersion)
        {
            if (Version.TryParse(Normalize(currentVersion), out var curr)
                && Version.TryParse(Normalize(candidateVersion), out var next))
            {
                return next > curr;
            }

            return false;
        }
    }
}
