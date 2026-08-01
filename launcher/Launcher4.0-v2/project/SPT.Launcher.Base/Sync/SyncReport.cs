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

        /// <summary>
        /// Prioridade de ordenação das entries no relatório — menor = mais importante (topo).
        /// "updated" fica no topo (é o que mais interessa ao usuário); "error" por último (o
        /// counts.errors + warnings já dão visibilidade). Ação fora do mapa → <see cref="int.MaxValue"/>
        /// (vai pro fim), NUNCA exceção: nenhuma ação nova pode crashar o sort.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, int> ActionPriority = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["updated"] = 1,
            ["optional-config-applied"] = 2,
            ["forced"] = 3,
            ["optional-config-suppressed-force"] = 4,
            ["reference-updated"] = 5,
            ["seeded"] = 6,
            ["seed-skipped"] = 7,
            ["preserved"] = 8,
            ["preserved-devmode"] = 9,
            ["optional-config-reverted"] = 10,
            ["moved-to-disabled"] = 11,
            ["deleted"] = 12,
            ["error"] = 13,
        };

        /// <summary>
        /// Frase humana (PT) por ação — o campo "descricao" do relatório. "detail" continua cru (pra
        /// máquina). Ação sem frase mapeada → cai no "detail" cru (nunca fica vazio).
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> ActionDescricao = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["updated"] = "baixada do servidor (estava desatualizada ou faltando)",
            ["optional-config-applied"] = "config de performance aplicada (item ligado; se você tinha uma, foi preservada na quarentena)",
            ["optional-config-reverted"] = "config de performance desligada — removida (movida para quarentena)",
            ["optional-config-suppressed-force"] = "config de performance sobrepôs uma config obrigatória (config-force) do mesmo arquivo",
            ["forced"] = "config obrigatória do servidor — se você tinha uma, ela foi preservada em uma pasta -disabled/",
            ["reference-updated"] = "biblioteca de referência atualizada (pasta config-server; não altera a sua config)",
            ["seeded"] = "config padrão copiada (não existia)",
            ["seed-skipped"] = "config padrão já existia — não foi sobrescrita",
            ["preserved"] = "você personalizou — sua versão foi mantida",
            ["preserved-devmode"] = "mantida pelo Modo desenvolvedor (não revertida)",
            ["moved-to-disabled"] = "movida para quarentena (pasta -disabled)",
            ["deleted"] = "removida (extra fora do manifesto)",
            ["error"] = "falha — ver detalhe",
        };

        public static void Write(string filePath, SyncResult result)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Ordena por prioridade de ação (updated no topo), com desempate ESTÁVEL pela ordem de
            // execução (índice original). Enriquece cada linha com "descricao" (PT) — "detail" fica cru.
            var orderedEntries = (result.Entries ?? new List<SyncReportEntry>())
                .Select((entry, index) => new { entry, index })
                .OrderBy(x => ActionPriority.TryGetValue(x.entry.action ?? string.Empty, out int p) ? p : int.MaxValue)
                .ThenBy(x => x.index)
                .Select(x => new
                {
                    x.entry.path,
                    x.entry.action,
                    descricao = ActionDescricao.TryGetValue(x.entry.action ?? string.Empty, out string frase)
                        ? frase
                        : x.entry.detail,
                    x.entry.detail,
                    x.entry.timestamp,
                })
                .ToList();

            // O mirror do config-server reusa Download → incrementa result.Updated, mas no relatório
            // sai com label próprio ("reference-updated"). Sem descontar, counts.updated ficaria MAIOR
            // que o nº de linhas "updated" (confuso). Deriva o nº de referências das entries (fonte de
            // verdade do relatório) e separa: updated = só a config do usuário; referenceUpdated = a
            // biblioteca. A subtração é sempre >= 0 (toda "reference-updated" incrementou Updated).
            int referenceUpdated = (result.Entries ?? new List<SyncReportEntry>())
                .Count(e => e.action == "reference-updated");

            var model = new
            {
                generatedAt = DateTime.UtcNow,
                cancelled = result.Cancelled,
                counts = new
                {
                    updated = result.Updated - referenceUpdated,
                    referenceUpdated = referenceUpdated,
                    forced = result.Forced,
                    preserved = result.Preserved,
                    preservedDevMode = result.PreservedDevMode,
                    seeded = result.Seeded,
                    configsBackedUp = result.ConfigsBackedUp,
                    movedToDisabled = result.MovedToDisabled,
                    deleted = result.Deleted,
                    errors = result.Errors,
                    pending = result.Pending,
                },
                warnings = result.Warnings,
                entries = orderedEntries,
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
