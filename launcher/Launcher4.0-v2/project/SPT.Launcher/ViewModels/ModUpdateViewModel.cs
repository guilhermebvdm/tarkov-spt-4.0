using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;
using SPT.Launcher.ViewModels.Dialogs;

namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// Standalone update screen backed by the item-007 sync engine (SPT.Launcher.Base/Sync),
    /// rendered by Views/ModUpdateView.axaml. Folder rules: config = preserve divergent ·
    /// config-server = mirror delete · patchers/plugins = mirror move to -disabled · rest =
    /// legacy behavior. The login flow (ProfileViewModel) uses the same engine (P-007.1).
    /// </summary>
    public class ModUpdateViewModel : ViewModelBase
    {
        // === Properties for UI Binding ===

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        private string _summaryText = "";
        public string SummaryText
        {
            get => _summaryText;
            set => this.RaiseAndSetIfChanged(ref _summaryText, value);
        }

        private double _progress = 0;
        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private double _maxProgress = 100;
        public double MaxProgress
        {
            get => _maxProgress;
            set => this.RaiseAndSetIfChanged(ref _maxProgress, value);
        }

        private bool _isChecking = false;
        public bool IsChecking
        {
            get => _isChecking;
            set
            {
                this.RaiseAndSetIfChanged(ref _isChecking, value);
                this.RaisePropertyChanged(nameof(IsBusy));
            }
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                this.RaiseAndSetIfChanged(ref _isUpdating, value);
                this.RaisePropertyChanged(nameof(IsBusy));
            }
        }

        /// <summary>True while checking or applying — drives the view's disabled states.</summary>
        public bool IsBusy => IsChecking || IsUpdating;

        private bool _canUpdate = false;
        public bool CanUpdate
        {
            get => _canUpdate;
            set => this.RaiseAndSetIfChanged(ref _canUpdate, value);
        }

        private bool _canCancel = false;
        public bool CanCancel
        {
            get => _canCancel;
            set => this.RaiseAndSetIfChanged(ref _canCancel, value);
        }

        private bool _updateComplete = false;
        public bool UpdateComplete
        {
            get => _updateComplete;
            set => this.RaiseAndSetIfChanged(ref _updateComplete, value);
        }

        private int _totalFiles = 0;
        public int TotalFiles
        {
            get => _totalFiles;
            set => this.RaiseAndSetIfChanged(ref _totalFiles, value);
        }

        private int _outdatedFiles = 0;
        public int OutdatedFiles
        {
            get => _outdatedFiles;
            set => this.RaiseAndSetIfChanged(ref _outdatedFiles, value);
        }

        // === Item 016 — taxa de download (média móvel, MB/s decimal, separador PT-BR) ===

        private readonly DownloadRateMeter _rateMeter = new DownloadRateMeter();

        private string _downloadSpeedText = "";
        public string DownloadSpeedText
        {
            get => _downloadSpeedText;
            set
            {
                this.RaiseAndSetIfChanged(ref _downloadSpeedText, value);
                this.RaisePropertyChanged(nameof(HasDownloadSpeed));
            }
        }

        private double _downloadBytesPerSec;
        public double DownloadBytesPerSec
        {
            get => _downloadBytesPerSec;
            set => this.RaiseAndSetIfChanged(ref _downloadBytesPerSec, value);
        }

        /// <summary>Cache hit / nothing downloading → empty text → the speed label is hidden.</summary>
        public bool HasDownloadSpeed => !string.IsNullOrEmpty(DownloadSpeedText);

        public ObservableCollection<ModFileStatus> FileStatuses { get; } = new ObservableCollection<ModFileStatus>();

        public ICommand UpdateCommand { get; }
        public ICommand CheckUpdatesCommand { get; }
        public ICommand CancelCommand { get; }

        // Internal state
        private SyncPlan _plan;
        private SyncBaseline _baseline;
        // Item 030: _performanceOverlay removido — config-performance virou regra de pasta (D-13).
        private CancellationTokenSource _cts;
        private readonly string _gamePath;

        private static string LauncherStateDir =>
            Path.Combine(SPT.Launcher.Base.Helpers.SptPathHelper.SptRootPath, "user", "launcher");

        public ModUpdateViewModel(IScreen Host) : base(Host)
        {
            _gamePath = LauncherSettingsProvider.Instance.GamePath ?? "";

            UpdateCommand = ReactiveCommand.CreateFromTask(async () => await UpdateMods());
            CheckUpdatesCommand = ReactiveCommand.CreateFromTask(async () => await CheckForUpdates());
            CancelCommand = ReactiveCommand.CreateFromTask(async () => await CancelSync());

            // Auto-check on load
            _ = CheckForUpdates();
        }

        /// <summary>
        /// Fetches the manifest and builds the sync plan (per-folder rules + baseline).
        /// Cancellable (requirement 4.1.2 — "Verificação de arquivo").
        /// </summary>
        public async Task CheckForUpdates()
        {
            if (IsChecking || IsUpdating) return;

            IsChecking = true;
            CanUpdate = false;
            UpdateComplete = false;
            SummaryText = "";
            StatusText = LocalizationProvider.Instance.update_checking;
            Progress = 0;
            FileStatuses.Clear();
            _plan = null;
            _rateMeter.Reset();       // item 016 — taxa começa limpa a cada verificação
            DownloadSpeedText = "";

            _cts = new CancellationTokenSource();
            CanCancel = true;

            try
            {
                Controllers.LogManager.Instance.Info("[ModUpdateView] Buscando manifesto de mods...");
                string response = await Task.Run(() => RequestHandler.RequestModsManifest());
                var manifest = JObject.Parse(response);

                var files = manifest["files"]?.ToObject<List<ManifestFile>>() ?? new List<ManifestFile>();
                var ignoredFiles = manifest["ignoredFiles"]?.ToObject<List<string>>() ?? new List<string>();
                var managedPaths = manifest["managedPaths"]?.ToObject<List<string>>() ?? new List<string>();
                var folderRules = manifest["folderRules"]?.ToObject<Dictionary<string, string>>();

                // Item 030: catálogo dos itens da tela "Mods e Configs" (mesmo modelo do ProfileViewModel).
                ModsConfigCatalog.UpdateFromManifest(manifest["optionalMods"], manifest["performanceItems"]);

                TotalFiles = files.Count;
                MaxProgress = Math.Max(1, files.Count);

                // Item 030: sem overlay — config-performance virou regra de pasta. Discriminadores das prefs.
                IReadOnlyList<ManifestFile> effectiveFiles = files;
                var justToggled = LauncherSettingsProvider.Instance.PendingApply
                    .Where(id => id != SyncTriggers.PendingApplyMarker)
                    .ToList();

                var resolver = new SyncRuleResolver(folderRules);
                _baseline = SyncBaseline.Load(Path.Combine(LauncherStateDir, "sync-state.json"));

                var options = new SyncPlannerOptions
                {
                    GameRoot = _gamePath,
                    DevMode = LauncherSettingsProvider.Instance.IsDevMode,
                    IgnoredFiles = ignoredFiles,
                    ExcludeFromCleanup = LauncherSettingsProvider.Instance.ExcludeFromCleanup ?? Array.Empty<string>(),
                    ManagedPaths = managedPaths,
                    IsOptionalModEnabled = id => LauncherSettingsProvider.Instance.IsOptionalEnabled(id),
                    IsPerformanceItemEnabled = id => LauncherSettingsProvider.Instance.IsPerformanceItemEnabled(id),
                    JustToggledIds = justToggled,
                };

                var planner = new SyncPlanner(resolver, _baseline, options);

                var progress = new Progress<SyncProgress>(p =>
                {
                    Progress = p.Current;
                    MaxProgress = Math.Max(1, p.Total);
                    StatusText = string.Format(LocalizationProvider.Instance.update_checking_file, p.Current, p.Total)
                                 + " - " + p.CurrentPath;
                });

                var plan = await planner.BuildPlanAsync(effectiveFiles, progress, _cts.Token);
                _plan = plan;

                PopulateFileStatuses(plan, effectiveFiles);
                OutdatedFiles = plan.DownloadCount;

                SummaryText = BuildPlanSummary(plan);

                if (plan.IoActionCount > 0)
                {
                    CanUpdate = true;
                    StatusText = string.Format(LocalizationProvider.Instance.update_available, plan.DownloadCount);
                }
                else
                {
                    StatusText = LocalizationProvider.Instance.update_up_to_date;
                    UpdateComplete = true;

                    // Nothing to apply, but persist the converged baseline seed + report of preserved files
                    var engine = CreateEngine();
                    var result = await engine.ExecuteAsync(plan, ReportFilePath, null, CancellationToken.None);
                    SummaryText = result.Summary;
                }
            }
            catch (OperationCanceledException)
            {
                StatusText = LocalizationProvider.Instance.mod_update_check_canceled;
                Controllers.LogManager.Instance.Info("[ModUpdateView] Verificação cancelada pelo usuário");
            }
            catch (Exception ex)
            {
                Controllers.LogManager.Instance.Error($"[ModUpdateView] Erro ao verificar atualizações: {ex.Message}");
                StatusText = string.Format(LocalizationProvider.Instance.update_error, ex.Message);
            }
            finally
            {
                IsChecking = false;
                CanCancel = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <summary>
        /// Executes the current plan through the sync engine: atomic downloads, mirror deletes,
        /// moves to -disabled. Baseline + last-update.json are persisted even on cancel/error.
        /// </summary>
        public async Task UpdateMods()
        {
            var plan = _plan;
            if (plan == null || _baseline == null || plan.IoActionCount == 0 || IsUpdating) return;

            IsUpdating = true;
            CanUpdate = false;
            Controllers.LogManager.Instance.Info(
                $"[ModUpdateView] Aplicando plano: {plan.DownloadCount} downloads, {plan.DeleteCount} remoções, {plan.MoveCount} movimentos p/ -disabled");
            MaxProgress = Math.Max(1, plan.IoActionCount);
            Progress = 0;
            _rateMeter.Reset(); // item 016
            DownloadSpeedText = "";

            _cts = new CancellationTokenSource();
            CanCancel = true;

            try
            {
                var engine = CreateEngine();

                var progress = new Progress<SyncProgress>(p =>
                {
                    Progress = p.Current;
                    StatusText = string.Format(LocalizationProvider.Instance.update_downloading, p.CurrentPath, p.Current, p.Total);
                    UpdateFileStatus(p.CurrentPath, "updating");
                });

                var result = await engine.ExecuteAsync(plan, ReportFilePath, progress, _cts.Token);

                foreach (var entry in result.Entries)
                {
                    UpdateFileStatus(entry.path, MapEntryStatus(entry.action));
                }

                SummaryText = result.Summary;
                OutdatedFiles = 0;
                _plan = null; // always re-check before another apply

                if (result.Cancelled)
                {
                    StatusText = string.Format(LocalizationProvider.Instance.mod_update_canceled_partial, result.Pending);
                    Controllers.LogManager.Instance.Warning($"[ModUpdateView] Atualização cancelada com {result.Pending} ações pendentes");
                }
                else if (result.Errors > 0)
                {
                    UpdateComplete = true;
                    StatusText = string.Format(LocalizationProvider.Instance.update_completed_with_errors, result.Updated, result.Errors);
                    Controllers.LogManager.Instance.Warning($"[ModUpdateView] Atualização concluída com {result.Errors} erros");
                }
                else
                {
                    UpdateComplete = true;
                    StatusText = string.Format(LocalizationProvider.Instance.update_completed, result.Updated);
                    Controllers.LogManager.Instance.Info($"[ModUpdateView] Atualização concluída: {result.Summary}");
                }
            }
            catch (Exception ex)
            {
                Controllers.LogManager.Instance.Error($"[ModUpdateView] Erro na atualização: {ex.Message}");
                StatusText = string.Format(LocalizationProvider.Instance.update_error, ex.Message);
            }
            finally
            {
                IsUpdating = false;
                CanCancel = false;
                DownloadSpeedText = ""; // item 016 — some com a taxa ao terminar o apply
                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <summary>
        /// Requirement 4.1.2: cancel with confirmation + consequence warning.
        /// The in-flight file finishes atomically; the run stops between files.
        /// </summary>
        public async Task CancelSync()
        {
            if (!CanCancel || _cts == null) return;

            var confirm = await ShowDialog(new ConfirmationDialogViewModel(null,
                LocalizationProvider.Instance.mod_update_cancel_confirm_question,
                LocalizationProvider.Instance.mod_update_cancel_confirm_yes,
                LocalizationProvider.Instance.mod_update_cancel_confirm_deny));

            if (confirm is not (bool and true)) return;

            try
            {
                _cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // run finished while the dialog was open — nothing to cancel
            }
        }

        /// <summary>Opens the folder with last-update.json (requirement 4.1.3 — future UI link uses this too).</summary>
        public void OpenLastUpdateFolder()
        {
            try
            {
                SyncReport.OpenReportFolder(LauncherStateDir);
            }
            catch (Exception ex)
            {
                Controllers.LogManager.Instance.Warning($"[ModUpdateView] Falha ao abrir pasta do relatório: {ex.Message}");
            }
        }

        private static string ReportFilePath => Path.Combine(LauncherStateDir, SyncReport.DefaultFileName);

        private SyncEngine CreateEngine()
        {
            // Item 030: um downloader só (overlay/performance-download aposentado — D-13).
            SyncDownloader downloader = (path, ct) => Task.Run(() => RequestHandler.DownloadModFile(path), ct);

            // Item 016 — mede a taxa como camada MAIS EXTERNA: captura todos os downloads
            // (base, overlay do 008 e seeds do 017), independente da origem.
            downloader = WithSpeedMeter(downloader);

            return new SyncEngine(
                _gamePath,
                _baseline,
                downloader,
                deleteFile: Helpers.RecycleBinHelper.Delete, // item 019 — fonte única de deleção recuperável
                log: msg => Controllers.LogManager.Instance.Info(msg));
        }

        /// <summary>
        /// Item 016: wraps a downloader to measure bytes/time per file and push the smoothed rate to
        /// the VM via the UI thread (the delegate runs on a Task thread). No coupling to the engine.
        /// </summary>
        private SyncDownloader WithSpeedMeter(SyncDownloader inner)
        {
            return async (path, ct) =>
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                byte[] data = await inner(path, ct);
                stopwatch.Stop();

                _rateMeter.AddSample(data?.LongLength ?? 0, stopwatch.Elapsed);
                double bytesPerSec = _rateMeter.BytesPerSecond;
                string text = DownloadRateMeter.Format(bytesPerSec);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    DownloadBytesPerSec = bytesPerSec;
                    DownloadSpeedText = text;
                });

                return data;
            };
        }

        private static string BuildPlanSummary(SyncPlan plan)
        {
            var parts = new List<string>
            {
                string.Format(LocalizationProvider.Instance.plan_summary_to_update, plan.DownloadCount),
                string.Format(LocalizationProvider.Instance.plan_summary_preserved, plan.PreserveCount),
            };

            if (plan.SeedCount > 0) parts.Add(string.Format(LocalizationProvider.Instance.plan_summary_to_seed, plan.SeedCount));
            // config-force: a config do usuário é SUBSTITUÍDA — mas preservada em -disabled. O preview
            // tem que dizer as duas coisas (senão o backup existe e ninguém sabe).
            if (plan.ForceCount > 0) parts.Add(string.Format(LocalizationProvider.Instance.plan_summary_forced_configs, plan.ForceCount));
            if (plan.MoveCount > 0) parts.Add(string.Format(LocalizationProvider.Instance.plan_summary_to_move, plan.MoveCount));
            if (plan.DeleteCount > 0) parts.Add(string.Format(LocalizationProvider.Instance.plan_summary_to_remove, plan.DeleteCount));
            if (plan.Warnings.Count > 0) parts.Add(string.Format(LocalizationProvider.Instance.plan_summary_dev_mode_warnings, plan.Warnings.Count));

            return string.Join(" · ", parts);
        }

        private void PopulateFileStatuses(SyncPlan plan, IReadOnlyList<ManifestFile> manifestFiles)
        {
            var sizeByPath = manifestFiles
                .GroupBy(f => f.path, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().size, StringComparer.OrdinalIgnoreCase);

            var statuses = plan.Actions.Select(action => new ModFileStatus
            {
                // Item 017 / config-force: seed E force mostram o alvo do USUÁRIO (config/<rel>) — é o que
                // cai no disco. A FONTE (config-server/, config-force/) nunca existe na máquina dele.
                FileName = action.Kind is SyncActionKind.SeedCopy or SyncActionKind.ForceCopy
                    ? action.SeedTargetRelative
                    : action.RelativePath,
                Status = action.Kind switch
                {
                    SyncActionKind.Download => "outdated",
                    SyncActionKind.PreserveCustomized => "preserved",
                    SyncActionKind.PreserveDevMode => "preserved",
                    SyncActionKind.DeleteExtra => "to-delete",
                    SyncActionKind.MoveToDisabled => "to-move",
                    SyncActionKind.SeedCopy => "to-seed",
                    SyncActionKind.ForceCopy => "to-force",
                    _ => "outdated",
                },
                Size = sizeByPath.TryGetValue(action.RelativePath, out long size) ? size : 0,
            }).ToList();

            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                FileStatuses.Clear();
                foreach (var status in statuses)
                {
                    FileStatuses.Add(status);
                }
            });
        }

        private static string MapEntryStatus(string action) => action switch
        {
            "updated" => "updated",
            "deleted" => "deleted",
            "moved-to-disabled" => "moved",
            "seeded" => "seeded",
            "seed-skipped" => "preserved",
            "forced" => "forced",
            "preserved" => "preserved",
            "preserved-devmode" => "preserved",
            "error" => "error",
            _ => "ok",
        };

        private void UpdateFileStatus(string filePath, string newStatus)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var item = FileStatuses.FirstOrDefault(f =>
                    string.Equals(f.FileName, filePath, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    item.Status = newStatus;
                }
            });
        }

        public new void GoBackCommand() => NavigateTo(new ProfileViewModel(HostScreen));
    }

    // === Models ===
    // ManifestFile agora é único em SPT.Launcher.Models.Launcher (SPT.Launcher.Base) — item 007.

    public class ModFileStatus : ReactiveObject
    {
        public string FileName { get; set; }

        private string _status;
        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public long Size { get; set; }

        public string SizeFormatted => Size < 1024 ? $"{Size} B"
            : Size < 1048576 ? $"{Size / 1024.0:F1} KB"
            : $"{Size / 1048576.0:F1} MB";

        public string StatusIcon => Status switch
        {
            "ok" => "✅",
            "outdated" => "⬇️",
            "missing" => "⬇️",
            "updating" => "📦",
            "updated" => "✅",
            "preserved" => "🛡️",
            "to-delete" => "🗑️",
            "deleted" => "🗑️",
            "to-move" => "📁",
            "moved" => "📁",
            "to-seed" => "🌱",
            "seeded" => "🌱",
            "to-force" => "🔨",
            "forced" => "🔨",
            "error" => "❌",
            _ => "⏳"
        };
    }
}
