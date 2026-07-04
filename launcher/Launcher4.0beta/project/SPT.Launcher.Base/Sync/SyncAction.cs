namespace SPT.Launcher.Sync
{
    public enum SyncActionKind
    {
        /// <summary>Download from server and apply atomically (temp + move).</summary>
        Download,

        /// <summary>Divergent file kept as-is (PreserveDivergent rule / first run without baseline).</summary>
        PreserveCustomized,

        /// <summary>Divergent file kept as-is because Dev Mode is on (local dev build protection).</summary>
        PreserveDevMode,

        /// <summary>Local extra removed (MirrorDelete rule or Default rule inside managedPaths).</summary>
        DeleteExtra,

        /// <summary>Local extra moved to the sibling "&lt;folder&gt;-disabled" directory (MirrorMoveDisabled rule).</summary>
        MoveToDisabled,
    }

    /// <summary>One planned action produced by <see cref="SyncPlanner"/> and executed by <see cref="SyncEngine"/>.</summary>
    public sealed class SyncAction
    {
        /// <summary>Relative path (manifest casing, forward slashes) from the game root.</summary>
        public string RelativePath { get; set; }

        public SyncActionKind Kind { get; set; }

        public SyncFolderRule Rule { get; set; }

        /// <summary>Server MD5 — set for <see cref="SyncActionKind.Download"/> (recorded in the baseline after apply).</summary>
        public string ServerHash { get; set; }

        /// <summary>Relative destination — set for <see cref="SyncActionKind.MoveToDisabled"/>.</summary>
        public string MoveTargetRelative { get; set; }

        /// <summary>Human-readable reason (logged and written to last-update.json).</summary>
        public string Reason { get; set; }
    }
}
