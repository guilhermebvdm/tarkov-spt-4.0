using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.ViewModels
{
    public class ModUpdateViewModel : ViewModelBase
    {
        // === Properties for UI Binding ===

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
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
            set => this.RaiseAndSetIfChanged(ref _isChecking, value);
        }

        private bool _isUpdating = false;
        public bool IsUpdating
        {
            get => _isUpdating;
            set => this.RaiseAndSetIfChanged(ref _isUpdating, value);
        }

        private bool _canUpdate = false;
        public bool CanUpdate
        {
            get => _canUpdate;
            set => this.RaiseAndSetIfChanged(ref _canUpdate, value);
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

        public ObservableCollection<ModFileStatus> FileStatuses { get; } = new ObservableCollection<ModFileStatus>();

        public ICommand UpdateCommand { get; }
        public ICommand CheckUpdatesCommand { get; }

        // Internal state
        private List<ManifestFile> _filesToUpdate = new List<ManifestFile>();
        private string _gamePath;

        public ModUpdateViewModel(IScreen Host) : base(Host)
        {
            _gamePath = LauncherSettingsProvider.Instance.GamePath ?? "";

            UpdateCommand = ReactiveCommand.CreateFromTask(async () => await UpdateMods());
            CheckUpdatesCommand = ReactiveCommand.CreateFromTask(async () => await CheckForUpdates());

            // Auto-check on load
            _ = CheckForUpdates();
        }

        /// <summary>
        /// Check for updates by fetching manifest and comparing local files
        /// </summary>
        public async Task CheckForUpdates()
        {
            IsChecking = true;
            CanUpdate = false;
            UpdateComplete = false;
            StatusText = LocalizationProvider.Instance.update_checking;
            Progress = 0;
            FileStatuses.Clear();
            _filesToUpdate.Clear();

            try
            {
                // Fetch manifest from server
                Controllers.LogManager.Instance.Info("[ModUpdateView] Buscando manifesto de mods...");
                string response = await Task.Run(() => RequestHandler.RequestModsManifest());
                var manifest = JObject.Parse(response);

                var files = manifest["files"]?.ToObject<List<ManifestFile>>() ?? new List<ManifestFile>();
                var ignoredFiles = manifest["ignoredFiles"]?.ToObject<List<string>>() ?? new List<string>();

                TotalFiles = files.Count;
                MaxProgress = files.Count;

                int checkedCount = 0;
                int outdated = 0;

                foreach (var file in files)
                {
                    checkedCount++;
                    Progress = checkedCount;
                    StatusText = string.Format(LocalizationProvider.Instance.update_checking_file, checkedCount, files.Count);

                    string relPathLower = file.path.Replace('\\', '/').ToLowerInvariant();
                    if (ignoredFiles.Any(ig => relPathLower.Contains(ig.Replace('\\', '/').ToLowerInvariant())))
                    {
                        continue;
                    }

                    string localPath = Path.Combine(_gamePath, file.path.Replace('/', Path.DirectorySeparatorChar));

                    string status;
                    if (!File.Exists(localPath))
                    {
                        // File doesn't exist locally — needs download
                        status = "missing";
                        _filesToUpdate.Add(file);
                        outdated++;
                    }
                    else
                    {
                        // Compare hash
                        string localHash = await Task.Run(() => GetFileMD5(localPath));
                        if (localHash != file.hash)
                        {
                            status = "outdated";
                            _filesToUpdate.Add(file);
                            outdated++;
                        }
                        else
                        {
                            status = "ok";
                        }
                    }

                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        FileStatuses.Add(new ModFileStatus
                        {
                            FileName = file.path,
                            Status = status,
                            Size = file.size
                        });
                    });
                }

                OutdatedFiles = outdated;

                if (outdated > 0)
                {
                    CanUpdate = true;
                    StatusText = string.Format(LocalizationProvider.Instance.update_available, outdated);
                }
                else
                {
                    StatusText = LocalizationProvider.Instance.update_up_to_date;
                    UpdateComplete = true;
                }
            }
            catch (Exception ex)
            {
                Controllers.LogManager.Instance.Error($"[ModUpdateView] Erro ao verificar atualizações: {ex.Message}");
                StatusText = string.Format(LocalizationProvider.Instance.update_error, ex.Message);
            }
            finally
            {
                IsChecking = false;
            }
        }

        /// <summary>
        /// Download and install all outdated files
        /// </summary>
        public async Task UpdateMods()
        {
            if (_filesToUpdate.Count == 0) return;

            IsUpdating = true;
            CanUpdate = false;
            Controllers.LogManager.Instance.Info($"[ModUpdateView] Iniciando download de {_filesToUpdate.Count} arquivos...");
            MaxProgress = _filesToUpdate.Count;
            Progress = 0;

            int completed = 0;
            int errors = 0;

            foreach (var file in _filesToUpdate)
            {
                completed++;
                StatusText = string.Format(LocalizationProvider.Instance.update_downloading, file.path, completed, _filesToUpdate.Count);
                Progress = completed;

                try
                {
                    string localPath = Path.Combine(_gamePath, file.path.Replace('/', Path.DirectorySeparatorChar));

                    // Ensure directory exists
                    string directory = Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Download file
                    byte[] fileData = await Task.Run(() => RequestHandler.DownloadModFile(file.path));
                    await File.WriteAllBytesAsync(localPath, fileData);

                    // Update status in UI
                    UpdateFileStatus(file.path, "updated");
                }
                catch (Exception ex)
                {
                    errors++;
                    UpdateFileStatus(file.path, "error");
                    Controllers.LogManager.Instance.Error($"[ModUpdate] Failed to update {file.path}: {ex.Message}");
                }
            }

            IsUpdating = false;
            UpdateComplete = true;

            if (errors > 0)
            {
                Controllers.LogManager.Instance.Warning($"[ModUpdateView] Atualização concluída com {errors} erros de {_filesToUpdate.Count}");
                StatusText = string.Format(LocalizationProvider.Instance.update_completed_with_errors, _filesToUpdate.Count - errors, errors);
            }
            else
            {
                Controllers.LogManager.Instance.Info($"[ModUpdateView] Atualização concluída: {_filesToUpdate.Count} arquivos sem erros");
                StatusText = string.Format(LocalizationProvider.Instance.update_completed, _filesToUpdate.Count);
            }
        }

        private void UpdateFileStatus(string filePath, string newStatus)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var item = FileStatuses.FirstOrDefault(f => f.FileName == filePath);
                if (item != null)
                {
                    item.Status = newStatus;
                }
            });
        }

        private static string GetFileMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public new void GoBackCommand() => NavigateTo(new ProfileViewModel(HostScreen));
    }

    // === Models ===

    public class ManifestFile
    {
        public string path { get; set; }
        public string hash { get; set; }
        public long size { get; set; }
        public bool optional { get; set; }
        public string optionalGroup { get; set; }
    }

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
            "error" => "❌",
            _ => "⏳"
        };
    }
}
