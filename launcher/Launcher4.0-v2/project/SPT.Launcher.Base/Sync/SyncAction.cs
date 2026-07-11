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

        /// <summary>Item 017: seed a default from config-server → config when the target is absent by name (never overwrites).</summary>
        SeedCopy,

        /// <summary>
        /// config-force → config: SOBRESCREVE o alvo em 'config' sempre que divergir (ou faltar).
        /// Mesma mecânica do SeedCopy (baixa de <see cref="SyncAction.RelativePath"/>, grava em
        /// <see cref="SyncAction.SeedTargetRelative"/>), mas SEM o guard de "só se ausente".
        /// </summary>
        ForceCopy,
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

        /// <summary>
        /// Relative destination — set for <see cref="SyncActionKind.MoveToDisabled"/> e, no
        /// <see cref="SyncActionKind.ForceCopy"/>, o BACKUP da config do jogador em "&lt;pasta&gt;-disabled/&lt;rel&gt;"
        /// (null quando não há nada a preservar, ex.: o alvo não existia).
        /// </summary>
        public string MoveTargetRelative { get; set; }

        /// <summary>
        /// Item 017: relative WRITE destination under 'config' — set for <see cref="SyncActionKind.SeedCopy"/>.
        /// <see cref="RelativePath"/> stays the SERVER source (config-server/&lt;rel&gt;) used to download.
        /// </summary>
        public string SeedTargetRelative { get; set; }

        /// <summary>Human-readable reason (logged and written to last-update.json).</summary>
        public string Reason { get; set; }
    }
}
