using System;
using System.Collections.Generic;
using System.Text;

namespace SPT.Launcher.Sync
{
    /// <summary>One line of the change manifest (last-update.json), requirement 4.1.3.</summary>
    public sealed class SyncReportEntry
    {
        public string path { get; set; }

        /// <summary>updated · preserved · preserved-devmode · deleted · moved-to-disabled · error</summary>
        public string action { get; set; }

        public string detail { get; set; }

        public DateTime timestamp { get; set; }
    }

    /// <summary>Outcome of a <see cref="SyncEngine.ExecuteAsync"/> run (complete, failed or cancelled).</summary>
    public sealed class SyncResult
    {
        public int Updated { get; set; }
        public int Preserved { get; set; }
        public int PreservedDevMode { get; set; }
        public int Deleted { get; set; }
        public int MovedToDisabled { get; set; }

        /// <summary>Item 017: default configs copied from config-server into config (target was absent by name).</summary>
        public int Seeded { get; set; }

        public int Errors { get; set; }

        /// <summary>Disk actions not attempted because the run was cancelled (C4).</summary>
        public int Pending { get; set; }

        public bool Cancelled { get; set; }

        public List<string> Warnings { get; } = new List<string>();

        public List<SyncReportEntry> Entries { get; } = new List<SyncReportEntry>();

        /// <summary>Item 007 minimum UI: "X atualizados · Y preservados · Z movidos p/ disabled" (+ extras when > 0).</summary>
        public string Summary
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append($"{Updated} atualizados · {Preserved + PreservedDevMode} preservados · {MovedToDisabled} movidos p/ disabled");

                if (Seeded > 0) sb.Append($" · {Seeded} semeados");
                if (Deleted > 0) sb.Append($" · {Deleted} removidos");
                if (Errors > 0) sb.Append($" · {Errors} erros");
                if (PreservedDevMode > 0) sb.Append($" · {PreservedDevMode} preservados por Dev Mode");
                if (Cancelled) sb.Append($" · cancelado ({Pending} pendentes)");

                return sb.ToString();
            }
        }
    }
}
