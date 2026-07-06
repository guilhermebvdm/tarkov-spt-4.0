using System.Collections.Generic;
using System.Linq;

namespace SPT.Launcher.Sync
{
    /// <summary>Progress callback payload for both planning ("checking") and execution ("applying") phases.</summary>
    public sealed class SyncProgress
    {
        public SyncProgress(string phase, string currentPath, int current, int total)
        {
            Phase = phase;
            CurrentPath = currentPath;
            Current = current;
            Total = total;
        }

        public string Phase { get; }
        public string CurrentPath { get; }
        public int Current { get; }
        public int Total { get; }
    }

    /// <summary>Output of <see cref="SyncPlanner"/> — pure data, nothing has been written to disk yet.</summary>
    public sealed class SyncPlan
    {
        public List<SyncAction> Actions { get; } = new List<SyncAction>();

        /// <summary>
        /// Files whose local hash equals the server hash. Seeded into the baseline before
        /// execution — safe by definition (local == server), lets the baseline converge (CC7).
        /// </summary>
        public List<KeyValuePair<string, string>> UpToDate { get; } = new List<KeyValuePair<string, string>>();

        /// <summary>Accumulated warnings (e.g. Dev Mode preservation notices).</summary>
        public List<string> Warnings { get; } = new List<string>();

        public int DownloadCount => Actions.Count(a => a.Kind == SyncActionKind.Download);
        public int PreserveCount => Actions.Count(a => a.Kind == SyncActionKind.PreserveCustomized || a.Kind == SyncActionKind.PreserveDevMode);
        public int DeleteCount => Actions.Count(a => a.Kind == SyncActionKind.DeleteExtra);
        public int MoveCount => Actions.Count(a => a.Kind == SyncActionKind.MoveToDisabled);

        /// <summary>Item 017: config-server → config seed copies (target absent by name).</summary>
        public int SeedCount => Actions.Count(a => a.Kind == SyncActionKind.SeedCopy);

        /// <summary>Actions that actually touch the disk (downloads, deletes, moves, seeds).</summary>
        public int IoActionCount => DownloadCount + DeleteCount + MoveCount + SeedCount;
    }
}
