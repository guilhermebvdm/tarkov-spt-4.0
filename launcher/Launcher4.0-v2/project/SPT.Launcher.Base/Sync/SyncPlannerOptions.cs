using System;
using System.Collections.Generic;

namespace SPT.Launcher.Sync
{
    /// <summary>Inputs for <see cref="SyncPlanner"/> beyond the manifest itself.</summary>
    public sealed class SyncPlannerOptions
    {
        /// <summary>Absolute game root (files in the manifest are relative to it).</summary>
        public string GameRoot { get; set; }

        /// <summary>
        /// Dev Mode: files diverging from BOTH manifest and baseline are preserved with a warning
        /// (never revert local dev builds — repo memory lesson). Extras in mirrored folders are
        /// preserved too. Off → folder rules apply fully.
        /// </summary>
        public bool DevMode { get; set; }

        /// <summary>Manifest "ignoredFiles" — substring match on the normalized path (legacy semantics).</summary>
        public IReadOnlyList<string> IgnoredFiles { get; set; } = Array.Empty<string>();

        /// <summary>Launcher settings ExcludeFromCleanup — root file/folder names never deleted or moved.</summary>
        public IReadOnlyList<string> ExcludeFromCleanup { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Extra protected relative paths (any normalization) — e.g.
        /// OptionalModsHelper.GetAllKnownOptionalPaths(). Never deleted or moved.
        /// </summary>
        public IReadOnlyCollection<string> ProtectedPaths { get; set; } = Array.Empty<string>();

        /// <summary>Manifest "managedPaths" — Default-rule folders whose extras are deleted (legacy behavior).</summary>
        public IReadOnlyList<string> ManagedPaths { get; set; } = Array.Empty<string>();

        /// <summary>Returns whether an optional group is enabled (its files are then checked/downloaded).</summary>
        public Func<string, bool> IsOptionalGroupEnabled { get; set; } = _ => false;
    }
}
