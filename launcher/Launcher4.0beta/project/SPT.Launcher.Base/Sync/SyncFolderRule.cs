using System;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Per-folder synchronization strategy (item 007).
    /// </summary>
    public enum SyncFolderRule
    {
        /// <summary>Current launcher behavior: download outdated/missing; extras inside managedPaths are deleted.</summary>
        Default = 0,

        /// <summary>config: replace files equal to the last-sync baseline; preserve user-customized ones.</summary>
        PreserveDivergent = 1,

        /// <summary>config-server: full mirror; local extras are deleted.</summary>
        MirrorDelete = 2,

        /// <summary>patchers/plugins: full mirror; local extras are moved to a sibling "&lt;folder&gt;-disabled" directory.</summary>
        MirrorMoveDisabled = 3,
    }

    public static class SyncFolderRuleParser
    {
        /// <summary>
        /// Parses the canonical rule names used by the server manifest ("default",
        /// "preserve-divergent", "mirror-delete", "mirror-move-disabled"). Case-insensitive.
        /// </summary>
        public static bool TryParse(string value, out SyncFolderRule rule)
        {
            switch (value?.Trim().ToLowerInvariant())
            {
                case "default":
                    rule = SyncFolderRule.Default;
                    return true;
                case "preserve-divergent":
                    rule = SyncFolderRule.PreserveDivergent;
                    return true;
                case "mirror-delete":
                    rule = SyncFolderRule.MirrorDelete;
                    return true;
                case "mirror-move-disabled":
                    rule = SyncFolderRule.MirrorMoveDisabled;
                    return true;
                default:
                    rule = SyncFolderRule.Default;
                    return false;
            }
        }
    }
}
