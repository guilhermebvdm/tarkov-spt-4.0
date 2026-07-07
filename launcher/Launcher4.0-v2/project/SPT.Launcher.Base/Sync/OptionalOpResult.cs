using System.Collections.Generic;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Item 021 (B — falha visível): outcome of applying/removing an optional group. Replaces the old
    /// fire-and-forget <c>Task</c> return so the caller (ProfileViewModel) can surface a real error
    /// state instead of the silent "up to date" green when downloads fail (CA-021.4/5/6, RN-2).
    /// </summary>
    public sealed class OptionalOpResult
    {
        /// <summary>Number of files the operation considered (0 when the group was not resolved).</summary>
        public int Total;

        /// <summary>Files successfully downloaded+written (enable) or removed (disable).</summary>
        public int Ok;

        /// <summary>Files already present with a matching hash — not re-downloaded (RN-3, not a failure).</summary>
        public int Skipped;

        /// <summary>Files that failed to download/write/delete.</summary>
        public int Failed;

        /// <summary>Relative paths of every failed file (for the error status text and logs).</summary>
        public List<string> FailedPaths = new();

        /// <summary>
        /// False when the group id was missing from the manifest cache (CC-5): an empty
        /// <see cref="_cachedGroupFiles"/> entry used to log a Warning and silently "succeed".
        /// Now it is an explicit non-success so the UI shows an error.
        /// </summary>
        public bool GroupResolved = true;

        /// <summary>True only when the group resolved and nothing failed (RN-2: all-or-reported).</summary>
        public bool AllOk => GroupResolved && Failed == 0;

        /// <summary>True when at least one file landed on disk (download or skip) — used to tell partial from total failure.</summary>
        public bool AnyApplied => Ok > 0 || Skipped > 0;

        /// <summary>
        /// Total failure (D-021.A): the group resolved to files but not a single one landed — every
        /// file failed. Also true when the group could not be resolved at all. This is the case that
        /// reverts the toggle to <c>false</c>.
        /// </summary>
        public bool TotalFailure => !GroupResolved || (Total > 0 && !AnyApplied && Failed > 0);
    }
}
