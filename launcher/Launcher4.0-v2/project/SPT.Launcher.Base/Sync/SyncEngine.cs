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
        private readonly string _gameRoot;
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
                                SyncFileOps.WriteAtomic(destinationPath, data);

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
                                // MirrorReference (biblioteca config-server) reusa Download, mas no relatório
                                // recebe label próprio: separa "a SUA config mudou" de "a referência mudou".
                                AddEntry(result, action.RelativePath,
                                    action.Rule == SyncFolderRule.MirrorReference ? "reference-updated" : "updated",
                                    action.Reason);
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
                                SyncFileOps.WriteAtomic(seedDestination, seedData);

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

                        case SyncActionKind.ForceCopy:
                            try
                            {
                                // config-force → config: SOBRESCREVE (o oposto do SeedCopy — sem guard de
                                // ausência), mas NUNCA sem preservar antes o que o jogador tinha.
                                string forceDestination = ResolveUnderRoot(action.SeedTargetRelative); // ref: CR-01-05

                                // ORDEM: baixa PRIMEIRO. Falha de rede → nada tocado, config intacta.
                                byte[] forceData = await _downloader(action.RelativePath, cancellationToken); // FONTE (config-force)

                                // CR-01 (TOCTOU): quem decide "existe?" é o APPLY, não o plano — o arquivo
                                // pode ter surgido no meio (BepInEx regenerando o default, o jogador
                                // restaurando um backup). O planner sempre deriva o destino do backup.
                                bool forceTargetExists = File.Exists(forceDestination);

                                // CR-02 — FAIL-SAFE: se há algo no disco e não temos onde preservá-lo, NÃO
                                // sobrescrevemos. Falhar alto e visível > destruir em silêncio. A config do
                                // jogador fica intacta e o force é re-tentado no próximo sync.
                                if (forceTargetExists && string.IsNullOrEmpty(action.MoveTargetRelative))
                                {
                                    throw new InvalidOperationException(
                                        $"force abortado: sem destino de backup para {action.SeedTargetRelative} — a config do jogador foi mantida intacta");
                                }

                                string forceBackupRel = null;
                                if (forceTargetExists)
                                {
                                    // CR-04 — COPY, não move: em NENHUMA ordem de falha o config/ fica sem
                                    // arquivo (falha na cópia → nada escrito; falha na escrita → config
                                    // intacta E backup já feito). Estado final feliz é o mesmo do move.
                                    // CR-03 — nunca clobbera um backup de conteúdo DIFERENTE (a customização
                                    // do jogador não tem segunda cópia): versiona .1, .2, …
                                    // Pastas "-disabled" nunca são re-sincronizadas nem deletadas (R3.4).
                                    forceBackupRel = ResolveFreeBackupRelative(action.MoveTargetRelative, forceDestination);
                                    string forceBackupAbs = ResolveUnderRoot(forceBackupRel); // ref: CR-01-05

                                    string backupDir = Path.GetDirectoryName(forceBackupAbs);
                                    if (!string.IsNullOrEmpty(backupDir)) Directory.CreateDirectory(backupDir);

                                    File.Copy(forceDestination, forceBackupAbs, overwrite: true);
                                    _log($"[Sync] Config do jogador preservada em {forceBackupRel} antes do force");
                                }

                                SyncFileOps.WriteAtomic(forceDestination, forceData);

                                // Manifesto stale (arquivo mexido no mods_repo sem /refresh): os bytes baixados
                                // nunca vão bater com o hash do manifesto → o planner re-força TODO sync, para
                                // sempre. Sem este aviso o loop seria silencioso (mesma guarda do Download).
                                string forceAppliedHash = SyncPathUtil.ComputeMd5(forceData);
                                if (!string.IsNullOrEmpty(action.ServerHash)
                                    && !string.Equals(forceAppliedHash, action.ServerHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    _log($"[Sync] Aviso: bytes de {action.RelativePath} não batem com o hash do manifesto — manifesto desatualizado no server (chame /launcher/mods/refresh). A config forçada será reaplicada em todo sync até corrigir.");
                                }

                                result.Forced++;
                                if (forceBackupRel != null) result.ConfigsBackedUp++;
                                ioDone++;
                                AddEntry(result, action.SeedTargetRelative, "forced",
                                    forceBackupRel != null ? $"{action.Reason} · backup: {forceBackupRel}" : action.Reason);
                                _log($"[Sync] Config forçada: {action.SeedTargetRelative} (de {action.RelativePath})");
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
                                _log($"[Sync] Falha ao forçar {action.SeedTargetRelative}: {ex.Message}");
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
        /// ref: CR-01-05 / item 019 — defense in depth: delegates to the shared guard
        /// (<see cref="SyncPathUtil.ResolveUnderRoot"/>) so the engine and the legacy FS paths
        /// reject the same escaping paths. "..", absolute paths, etc. throw → counted as per-file error.
        /// </summary>
        private string ResolveUnderRoot(string relativePath) =>
            SyncPathUtil.ResolveUnderRoot(_gameRoot, relativePath);

        /// <summary>
        /// CR-03 — o backup do config-force NUNCA clobbera conteúdo DIFERENTE: a customização do jogador
        /// não tem segunda cópia no mundo. Se já existe um backup com o MESMO conteúdo, reusa (idempotente);
        /// se existe com conteúdo divergente (ele customizou de novo e estamos no 2º force), versiona
        /// ".1", ".2", … preservando a customização ANTERIOR. Devolve o caminho RELATIVO efetivo — é ele
        /// que vai pro relatório, senão o last-update.json apontaria pra um arquivo com outro conteúdo.
        /// </summary>
        private string ResolveFreeBackupRelative(string backupRelative, string sourceAbsolutePath)
        {
            string absolute = ResolveUnderRoot(backupRelative);
            if (!File.Exists(absolute))
            {
                return backupRelative;
            }

            // Mesmo conteúdo → reaproveita (não duplica a cada sync que re-força o mesmo arquivo).
            if (string.Equals(SyncPathUtil.ComputeMd5(absolute), SyncPathUtil.ComputeMd5(sourceAbsolutePath),
                    StringComparison.OrdinalIgnoreCase))
            {
                return backupRelative;
            }

            for (int i = 1; i < 100; i++)
            {
                string candidate = $"{backupRelative}.{i}";
                if (!File.Exists(ResolveUnderRoot(candidate)))
                {
                    return candidate;
                }
            }

            // Escape hatch (100 backups divergentes do mesmo arquivo): timestamp, nunca clobber.
            return $"{backupRelative}.{DateTime.UtcNow:yyyyMMddHHmmss}";
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
