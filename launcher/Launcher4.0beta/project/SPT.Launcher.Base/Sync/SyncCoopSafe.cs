using System;
using System.Collections.Generic;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Item 023 (Frente A / RN-1..RN-3) — embedded coop-safe allowlist. Fika is a client-only plugin
    /// family essential to the Fika Coop PVE runtime. When the server does NOT ship Fika in its
    /// manifest (a purely local install), <see cref="SyncPlanner.ScanExtras"/> would treat any
    /// <c>Fika.*.dll</c> under a mirror prefix (plugins/patchers) as an "extra" and quarantine it to
    /// <c>&lt;prefix&gt;-disabled/</c>, silently breaking every non-host client's ability to join the
    /// host's raid (solo=host never feels it). This predicate lets the planner PRESERVE those files
    /// (with a Warning — never silent, RN-3).
    ///
    /// Defense-in-depth only: if the server DOES ship Fika in the manifest, the manifest skip already
    /// protects it and this allowlist never fires (RN-2). It can only SUPPRESS a quarantine — never
    /// revert an update, never re-enable a server-defined file. There is no path where it forces Fika
    /// to diverge from the server.
    ///
    /// Matching is by ASSEMBLY-NAME family (<c>Fika.*.dll</c>) under a plugins/patchers prefix, never
    /// a loose path substring, so unrelated third-party DLLs that merely contain "fika" in a folder
    /// name are not caught (CC-3).
    /// </summary>
    public static class SyncCoopSafe
    {
        /// <summary>
        /// Normalized (lower-case, forward-slash) prefixes whose extras are mirror-managed AND where a
        /// coop-essential client plugin can legitimately live. Kept independent of the resolver so this
        /// predicate is a self-contained guard: a <c>Fika.Core.dll</c> under <c>user/mods</c> (not a
        /// mirror) is NOT coop-safe.
        /// </summary>
        private static readonly string[] PluginPrefixes =
        {
            "bepinex/plugins",
            "bepinex/patchers",
            "plugins",
            "patchers",
        };

        /// <summary>Assembly-name globs (single '*'), matched against the file name only (lower-case).</summary>
        private static readonly string[] _patterns = { "fika.*.dll" };

        /// <summary>The coop-safe assembly-name patterns — exposed for tests and report/log copy.</summary>
        public static IReadOnlyList<string> Patterns => _patterns;

        /// <summary>
        /// True when <paramref name="path"/> is a coop-essential client plugin that must never be
        /// quarantined even when absent from the server manifest. The input is normalized defensively
        /// (see <see cref="SyncPathUtil.Normalize"/>) so both already-normalized and raw/mixed-case
        /// paths are accepted; only paths under a plugins/patchers prefix are considered.
        /// </summary>
        public static bool IsCoopEssentialPlugin(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string normalized = SyncPathUtil.Normalize(path); // idempotent — tolerates raw/mixed-case input

            bool underPluginPrefix = false;
            foreach (var prefix in PluginPrefixes)
            {
                if (SyncPathUtil.IsUnderPrefix(normalized, prefix))
                {
                    underPluginPrefix = true;
                    break;
                }
            }

            if (!underPluginPrefix) return false;

            // File name only (normalized uses '/'), matched against the assembly-name family — not a
            // path substring — so "plugins/FikaThirdPartyStuff/other.dll" would NOT match.
            int slash = normalized.LastIndexOf('/');
            string fileName = slash >= 0 ? normalized.Substring(slash + 1) : normalized;
            if (fileName.Length == 0) return false;

            foreach (var pattern in _patterns)
            {
                if (MatchesGlob(fileName, pattern)) return true;
            }

            return false;
        }

        /// <summary>Minimal single-'*' glob over an already-lower-cased file name.</summary>
        private static bool MatchesGlob(string fileNameLower, string patternLower)
        {
            int star = patternLower.IndexOf('*');
            if (star < 0)
            {
                return string.Equals(fileNameLower, patternLower, StringComparison.Ordinal);
            }

            string head = patternLower.Substring(0, star);
            string tail = patternLower.Substring(star + 1);

            return fileNameLower.Length >= head.Length + tail.Length
                   && fileNameLower.StartsWith(head, StringComparison.Ordinal)
                   && fileNameLower.EndsWith(tail, StringComparison.Ordinal);
        }
    }
}
