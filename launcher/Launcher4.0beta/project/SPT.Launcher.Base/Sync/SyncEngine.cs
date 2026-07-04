using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SPT.Launcher.Sync
{
    /// <summary>Downloads a manifest file's bytes. Abstracted so the engine is testable without HTTP.</summary>
    public delegate Task<byte[]> SyncDownloader(string relativePath, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a <see cref="SyncPlan"/>: atomic downloads (temp + move), extra deletion,
    /// move-to-disabled quarantine. Cancellation is honored BETWEEN files; the file in flight
    /// either lands atomically or is discarded. Baseline and last-update.json are persisted in
    /// the finally block — also on error and cancellation (E4/C4).
    /// </summary>
    public sealed class SyncEngine
    {
        private const string TempSuffix = ".sync-tmp";

        private readonly string _gameRoot;
        private readonly string _gameRootFullPrefix; // ref: CR-01-05 — raiz resolvida p/ validação de traversal
        private readonly SyncBaseline _baseline;
        private readonly SyncDownloader _downloader;
        private readonly Action<string> _deleteFile;
        private readonly Action<string> _log;

        /// <param name="deleteFile">
        /// Deletion strategy for extras. Default: <see cref="File.Delete"/>. The UI injects a
        /// recycle-bin deleter (Microsoft.VisualBasic FileIO) to keep the legacy safety net.
        /// </param>
        public SyncEngine(
            string gameRoot,
            SyncBaseline baseline,
            SyncDownloader downloader,
            Action<string> deleteFile = null,
            Action<string> log = null)
        {
            _gameRoot = !string.IsNullOrEmpty(gameRoot) ? gameRoot : throw new ArgumentException("gameRoot is required", nameof(gameRoot));
            _gameRootFullPrefix = Path.GetFullPath(_gameRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _deleteFile = deleteFile ?? File.Delete;
            _log = log ?? (_ => { });
        }

        public async Task<SyncResult> ExecuteAsync(
            SyncPlan plan,
            string reportFilePath = null,
            IProgress<SyncProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            var result = new SyncResult();
            result.Warnings.AddRange(plan.Warnings);

            // Seed the baseline with confirmed up-to-date files (CC7 — safe: local == server).
            foreach (var upToDate in plan.UpToDate)
            {
                _baseline.SetHash(upToDate.Key, upToDate.Value);
            }

            int ioTotal = plan.IoActionCount;
            int ioDone = 0;

            try
            {
                foreach (var action in plan.Actions)
                {
                    if (action.Kind == SyncActionKind.PreserveCustomized)
                    {
                        result.Preserved++;
                        AddEntry(result, action.RelativePath, "preserved", action.Reason);
                        continue;
                    }

                    if (action.Kind == SyncActionKind.PreserveDevMode)
                    {
                        result.PreservedDevMode++;
                        AddEntry(result, action.RelativePath, "preserved-devmode", action.Reason);
                        continue;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    progress?.Report(new SyncProgress("applying", action.RelativePath, ioDone + 1, ioTotal));

                    switch (action.Kind)
                    {
                        case SyncActionKind.Download:
                            try
                            {
                                // ref: CR-01-05 — valida ANTES de baixar: manifesto adulterado com ".."
                                // não pode escrever fora do GameRoot (conta como erro por-arquivo).
                                string destinationPath = ResolveUnderRoot(action.RelativePath);
                                byte[] data = await _downloader(action.RelativePath, cancellationToken);
                                ApplyAtomic(destinationPath, data);

                                // ref: CR-01-05 — baseline records the hash of the bytes actually
                                // written, NOT the manifest hash: a stale manifest (e.g. pack edited
                                // without /refresh) would poison the baseline (local != baseline
                                // forever => file wedged as "customized"; ON can't re-apply, OFF
                                // can't revert). Mismatch is logged — it means the manifest is stale.
                                string appliedHash = SyncPathUtil.ComputeMd5(data);
                                if (!string.IsNullOrEmpty(action.ServerHash)
                                    && !string.Equals(appliedHash, action.ServerHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    _log($"[Sync] Aviso: bytes baixados de {action.RelativePath} não batem com o hash do manifesto — manifesto desatualizado no server? (baseline gravado com o hash real do disco)");
                                }

                                _baseline.SetHash(action.RelativePath, appliedHash);
                                result.Updated++;
                                ioDone++;
                                AddEntry(result, action.RelativePath, "updated", action.Reason);
                            }
                            catch (OperationCanceledException)
                            {
                                throw; // in-flight download cancelled → nothing applied, counts as pending
                            }
                            catch (Exception ex)
                            {
                                result.Errors++;
                                ioDone++;
                                AddEntry(result, action.RelativePath, "error", ex.Message);
                                _log($"[Sync] Falha ao atualizar {action.RelativePath}: {ex.Message}");
                            }

                            break;

                        case SyncActionKind.DeleteExtra:
                            try
                            {
                                _deleteFile(ResolveUnderRoot(action.RelativePath)); // ref: CR-01-05
                                _baseline.Remove(action.RelativePath);
                                result.Deleted++;
                                ioDone++;
                                AddEntry(result, action.RelativePath, "deleted", action.Reason);
                            }
                            catch (Exception ex)
                            {
                                result.Errors++;
                                ioDone++;
                                AddEntry(result, action.RelativePath, "error", ex.Message);
                                _log($"[Sync] Falha ao remover {action.RelativePath}: {ex.Message}");
                            }

                            break;

                        case SyncActionKind.MoveToDisabled:
                            try
                            {
                                // ref: CR-01-05 — origem E destino (-disabled) validados sob o GameRoot
                                MoveWithOverwrite(
                                    ResolveUnderRoot(action.RelativePath),
                                    ResolveUnderRoot(action.MoveTargetRelative));
                                _baseline.Remove(action.RelativePath);
                                result.MovedToDisabled++;
                                ioDone++;
                                AddEntry(result, action.RelativePath, "moved-to-disabled", action.MoveTargetRelative);
                            }
                            catch (Exception ex)
                            {
                                result.Errors++;
                                ioDone++;
                                AddEntry(result, action.RelativePath, "error", ex.Message);
                                _log($"[Sync] Falha ao mover {action.RelativePath}: {ex.Message}");
                            }

                            break;

                        case SyncActionKind.SeedCopy:
                            try
                            {
                                // ref: CR-01-05 — destino validado sob o GameRoot ANTES de baixar.
                                string seedDestination = ResolveUnderRoot(action.SeedTargetRelative);

                                // Item 017: NUNCA sobrescreve. O planner já decidiu por ausência, mas
                                // re-checa aqui (TOCTOU) — se o alvo surgiu entre o plano e o apply,
                                // pula sem baixar e sem erro. Baseline intocado (seed sem memória).
                                if (File.Exists(seedDestination))
                                {
                                    ioDone++;
                                    AddEntry(result, action.SeedTargetRelative, "seed-skipped", "target already present");
                                    break;
                                }

                                byte[] seedData = await _downloader(action.RelativePath, cancellationToken); // baixa da FONTE (config-server)
                                ApplyAtomic(seedDestination, seedData);

                                result.Seeded++;
                                ioDone++;
                                AddEntry(result, action.SeedTargetRelative, "seeded", action.RelativePath);
                            }
                            catch (OperationCanceledException)
                            {
                                throw; // download em voo cancelado → nada aplicado, conta como pendente
                            }
                            catch (Exception ex)
                            {
                                result.Errors++;
                                ioDone++;
                                AddEntry(result, action.SeedTargetRelative, "error", ex.Message);
                                _log($"[Sync] Falha ao semear {action.SeedTargetRelative}: {ex.Message}");
                            }

                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                result.Cancelled = true;
                _log("[Sync] Execução cancelada pelo usuário — estado parcial persistido");
            }
            finally
            {
                result.Pending = ioTotal - ioDone;

                try
                {
                    _baseline.Save();
                }
                catch (Exception ex)
                {
                    _log($"[Sync] Falha ao salvar baseline: {ex.Message}");
                }

                if (!string.IsNullOrEmpty(reportFilePath))
                {
                    try
                    {
                        SyncReport.Write(reportFilePath, result);
                    }
                    catch (Exception ex)
                    {
                        _log($"[Sync] Falha ao gravar last-update.json: {ex.Message}");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// ref: CR-01-05 — defense in depth: resolves the relative path and requires the result to
        /// stay under the game root ("..", absolute paths, etc. throw → counted as per-file error).
        /// </summary>
        private string ResolveUnderRoot(string relativePath)
        {
            string fullPath = Path.GetFullPath(SyncPathUtil.ToLocalPath(_gameRoot, relativePath));

            if (!fullPath.StartsWith(_gameRootFullPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"path escapes game root: {relativePath}");
            }

            return fullPath;
        }

        /// <summary>E3: write to "&lt;dest&gt;.sync-tmp" in the same directory (same volume), then atomic move.</summary>
        private static void ApplyAtomic(string destinationPath, byte[] data)
        {
            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = destinationPath + TempSuffix;

            try
            {
                File.WriteAllBytes(tempPath, data);
                File.Move(tempPath, destinationPath, overwrite: true);
            }
            catch
            {
                try
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch
                {
                    // best effort — never mask the original exception
                }

                throw;
            }
        }

        /// <summary>R3.3: collision in the -disabled folder → the freshly moved file wins.</summary>
        private static void MoveWithOverwrite(string sourcePath, string destinationPath)
        {
            string directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Move(sourcePath, destinationPath, overwrite: true);
        }

        private static void AddEntry(SyncResult result, string path, string action, string detail)
        {
            result.Entries.Add(new SyncReportEntry
            {
                path = path,
                action = action,
                detail = detail,
                timestamp = DateTime.UtcNow,
            });
        }
    }
}
