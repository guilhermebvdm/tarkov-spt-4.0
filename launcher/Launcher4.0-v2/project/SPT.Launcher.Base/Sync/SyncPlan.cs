using System.Collections.Generic;
using System.Linq;

namespace SPT.Launcher.Sync
{
    /// <summary>Progress callback payload for both planning ("checking") and execution ("applying") phases.</summary>
    public sealed class SyncProgress
    {
        public SyncProgress(string phase, string currentPath, int current, int total, SyncActionKind? kind = null)
        {
            Phase = phase;
            CurrentPath = currentPath;
            Current = current;
            Total = total;
            Kind = kind;
        }

        public string Phase { get; }
        public string CurrentPath { get; }
        public int Current { get; }
        public int Total { get; }
        /// <summary>Item 031: tipo da ação em curso (null na fase "checking") — a UI escolhe a frase.</summary>
        public SyncActionKind? Kind { get; }
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

        /// <summary>
        /// Item 030: entries informativas do relatório que NÃO são ações de disco (ex.:
        /// "optional-config-suppressed-force", RN-2). O engine as copia para o relatório sem executar nada.
        /// </summary>
        public List<SyncReportEntry> InfoEntries { get; } = new List<SyncReportEntry>();

        public int DownloadCount => Actions.Count(a => a.Kind == SyncActionKind.Download);
        public int PreserveCount => Actions.Count(a => a.Kind == SyncActionKind.PreserveCustomized || a.Kind == SyncActionKind.PreserveDevMode);
        public int DeleteCount => Actions.Count(a => a.Kind == SyncActionKind.DeleteExtra);
        public int MoveCount => Actions.Count(a => a.Kind == SyncActionKind.MoveToDisabled);

        /// <summary>Item 017: config-server → config seed copies (target absent by name).</summary>
        public int SeedCount => Actions.Count(a => a.Kind == SyncActionKind.SeedCopy);

        /// <summary>config-force → config: sobrescritas forçadas (ignoram customização do usuário).</summary>
        public int ForceCount => Actions.Count(a => a.Kind == SyncActionKind.ForceCopy);

        /// <summary>Item 030: config-optional → config aplicadas (item ligado).</summary>
        public int OptionalConfigCount => Actions.Count(a => a.Kind == SyncActionKind.OptionalConfigCopy);

        /// <summary>Item 034: mods movidos p/ quarentena como PASTA inteira (Directory.Move).</summary>
        public int MoveDirCount => Actions.Count(a => a.Kind == SyncActionKind.MoveDirToDisabled);

        /// <summary>
        /// Item 034: roots de espelho-com-quarentena a varrer para remover pastas vazias no fim do
        /// sync (rede de segurança da faxina). Vazio em Dev Mode (a faxina não roda). Populado pelo planner.
        /// </summary>
        public List<string> EmptyDirCleanupRoots { get; } = new List<string>();

        /// <summary>Actions that actually touch the disk (downloads, deletes, moves, seeds, forces, performance).</summary>
        public int IoActionCount => DownloadCount + DeleteCount + MoveCount + MoveDirCount + SeedCount + ForceCount + OptionalConfigCount;
    }
}
