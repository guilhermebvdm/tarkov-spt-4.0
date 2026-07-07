using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Writes the change manifest (user/launcher/last-update.json, requirement 4.1.3)
    /// and exposes helpers for the future "X arquivos foram atualizados" UI link.
    /// </summary>
    public static class SyncReport
    {
        public const string DefaultFileName = "last-update.json";

        public static void Write(string filePath, SyncResult result)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var model = new
            {
                generatedAt = DateTime.UtcNow,
                cancelled = result.Cancelled,
                counts = new
                {
                    updated = result.Updated,
                    preserved = result.Preserved,
                    preservedDevMode = result.PreservedDevMode,
                    deleted = result.Deleted,
                    movedToDisabled = result.MovedToDisabled,
                    seeded = result.Seeded,
                    errors = result.Errors,
                    pending = result.Pending,
                },
                warnings = result.Warnings,
                entries = result.Entries,
            };

            // ref: CR-01-04 — escrita atômica (temp + move), como o baseline e os applies.
            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, JsonConvert.SerializeObject(model, Formatting.Indented));
            File.Move(tempPath, filePath, overwrite: true);
        }

        /// <summary>Entry count grouped by the path's top-level folder — for the future per-folder UI summary.</summary>
        public static IReadOnlyDictionary<string, int> CountByTopFolder(IEnumerable<SyncReportEntry> entries)
        {
            return (entries ?? Enumerable.Empty<SyncReportEntry>())
                .GroupBy(e =>
                {
                    string normalized = SyncPathUtil.Normalize(e.path ?? string.Empty);
                    int slash = normalized.IndexOf('/');
                    return slash > 0 ? normalized.Substring(0, slash) : normalized;
                })
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        }

        /// <summary>Opens the folder containing the report in the OS file explorer (4.1.3 "clicar abre a pasta").</summary>
        public static void OpenReportFolder(string folderPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
            });
        }
    }
}
