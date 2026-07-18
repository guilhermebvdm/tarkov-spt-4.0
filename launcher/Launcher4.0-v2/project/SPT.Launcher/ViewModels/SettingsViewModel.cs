using SPT.Launcher.Controllers;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models;
using SPT.Launcher.Models.Launcher;
using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Input;
using Splat;

namespace SPT.Launcher.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public LocaleCollection Locales { get; set; } = new LocaleCollection();

        public ImageHelper Background => Splat.Locator.Current.GetService<ImageHelper>("bgimage");

        private GameStarter gameStarter = new GameStarter(new GameStarterFrontend());

        // === Dev Mode protegido por senha ===
        private const string DEV_PASSWORD = "Redline123";

        private string _devPassword = "";
        public string DevPassword
        {
            get => _devPassword;
            set => this.RaiseAndSetIfChanged(ref _devPassword, value);
        }

        private bool _devPasswordVisible = false;
        public char DevPasswordChar => _devPasswordVisible ? '\0' : '●';
        // Estado exposto p/ o XAML alternar os ícones traçados do DS (TrlIconEye / TrlIconEyeOff).
        public bool IsDevPasswordVisible => _devPasswordVisible;

        public void ToggleDevPasswordVisibilityCommand()
        {
            _devPasswordVisible = !_devPasswordVisible;
            this.RaisePropertyChanged(nameof(DevPasswordChar));
            this.RaisePropertyChanged(nameof(IsDevPasswordVisible));
        }

        // Cor do dot de status migrou para o XAML (Border.dev-dot + class binding em
        // IsDevMode, tokens TrlSuccessBrush/TrlFgFaintBrush). A VM só expõe o texto do tooltip.
        public string DevModeStatusText => LauncherSettingsProvider.Instance.IsDevMode ? "Dev Mode ATIVO" : "Dev Mode INATIVO";

        /// <summary>
        /// Item 008: toggle "USAR CONFIGS PERFORMANCE" — persiste imediatamente; o efeito
        /// acontece na próxima verificação de arquivos (overlay via motor de sync do 007).
        /// </summary>
        public bool UsePerformanceConfigs
        {
            get => LauncherSettingsProvider.Instance.UsePerformanceConfigs;
            set
            {
                if (LauncherSettingsProvider.Instance.UsePerformanceConfigs == value) return;

                LauncherSettingsProvider.Instance.UsePerformanceConfigs = value;
                LauncherSettingsProvider.Instance.SaveSettings();
                this.RaisePropertyChanged(nameof(UsePerformanceConfigs));
                LogManager.Instance.Info($"[Settings] Configs performance {(value ? "ativadas" : "desativadas")} — aplica na próxima verificação de arquivos");
            }
        }

        public SettingsViewModel(IScreen Host) : base(Host)
        {
            if(Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow.Closing += MainWindow_Closing;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            LauncherSettingsProvider.Instance.SaveSettings();
        }

        /// <summary>
        /// Alterna o modo dev: ativar requer senha, desativar é livre.
        /// </summary>
        public void ToggleDevModeCommand()
        {
            if (!LauncherSettingsProvider.Instance.IsDevMode)
            {
                // Tentando ativar — exige senha
                if (DevPassword == DEV_PASSWORD)
                {
                    LauncherSettingsProvider.Instance.IsDevMode = true;
                    LauncherSettingsProvider.Instance.SaveSettings();
                    SendNotification("", "🔓 Modo desenvolvedor ativado!", NotificationType.Success);
                    LogManager.Instance.Info("[Settings] Dev Mode ativado via senha");
                }
                else
                {
                    SendNotification("", "❌ Senha incorreta", NotificationType.Error);
                    LogManager.Instance.Warning("[Settings] Tentativa de ativar Dev Mode com senha incorreta");
                }
            }
            else
            {
                // Desativar — sem senha necessária
                LauncherSettingsProvider.Instance.IsDevMode = false;

                // Item 028: "Bloquear atualização de launcher" é ferramenta ESCOPADA ao Dev Mode.
                // Ao desligar o Dev Mode, resetar DisableUpdates → o bloqueio não fica preso/invisível
                // com o Dev Mode off (era o trap: usuário sem acesso ao painel dev não conseguia
                // reativar o auto-update). Só previne casos NOVOS — quem já está preso precisa editar
                // o config.json manualmente (não alcançável remotamente).
                if (LauncherSettingsProvider.Instance.DisableUpdates)
                {
                    LauncherSettingsProvider.Instance.DisableUpdates = false;
                    LogManager.Instance.Info("[Settings] DisableUpdates resetado ao desligar Dev Mode (item 028)");
                }

                LauncherSettingsProvider.Instance.SaveSettings();
                SendNotification("", "🔒 Modo desenvolvedor desativado", NotificationType.Information);
                LogManager.Instance.Info("[Settings] Dev Mode desativado");
            }

            DevPassword = "";
            this.RaisePropertyChanged(nameof(DevModeStatusText));
        }

        public void OpenKofiCommand()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://ko-fi.com/umbigopreto",
                UseShellExecute = true
            });
        }

        public async Task CopyLogsToClipboard()
        {
            LogManager.Instance.Info("[Settings] Copying logs to clipboard ...");

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow?.Clipboard == null)
                {
                    LogManager.Instance.Error("[Settings] Failed to get clipboard");
                    return;
                }

                var filesToCopy = new List<string> { LogManager.Instance.LogFile };
                
                var serverLog = Path.Join(LauncherSettingsProvider.Instance.GamePath, "SPT", @"user\logs",
                    $"server-{DateTime.Now:yyyy-MM-dd}.log");
                var bepinexLog = Path.Join(LauncherSettingsProvider.Instance.GamePath, @"BepInEx\LogOutput.log");

                if (AccountManager.SelectedAccount?.id != null)
                {
                    filesToCopy.Add(Path.Join(LauncherSettingsProvider.Instance.GamePath, "SPT", @"user\profiles",
                        $"{AccountManager.SelectedAccount.id}.json"));
                }

                if (File.Exists(serverLog))
                {
                    filesToCopy.Add(serverLog);
                }

                if (File.Exists(bepinexLog))
                {
                    filesToCopy.Add(bepinexLog);
                }

                var logsPath = Path.Join(LauncherSettingsProvider.Instance.GamePath, "Logs");
                if (Directory.Exists(logsPath))
                {
                    var traceLogs = Directory.GetFiles(logsPath, $"{DateTime.Now:yyyy.MM.dd}_* traces.log",
                        SearchOption.AllDirectories);

                    var log = traceLogs.Length > 0 ? traceLogs[0] : "";

                    if (!string.IsNullOrWhiteSpace(log))
                    {
                        filesToCopy.Add(log);
                    }
                }
                
                List<IStorageFile> files = new List<IStorageFile>();

                foreach (var logPath in filesToCopy)
                {
                    var file = await desktop.MainWindow.StorageProvider.TryGetFileFromPathAsync(logPath);

                    if (file != null)
                    {
                        LogManager.Instance.Debug($"file to copy :: {logPath}");
                        files.Add(file);
                        continue;
                    }
                    
                    LogManager.Instance.Warning($"failed to get file to copy :: {logPath}");
                }

                if (files.Count == 0)
                {
                    LogManager.Instance.Warning("[Settings] Failed to copy log files");
                    SendNotification("", LocalizationProvider.Instance.copy_failed);
                }

                var data = new DataObject();

                data.Set(DataFormats.Files, files.ToArray());
                
                await desktop.MainWindow.Clipboard.SetDataObjectAsync(data);
                
                LogManager.Instance.Info($"[Settings] {files.Count} log/s copied to clipboard");
                SendNotification("", $"{files.Count} {LocalizationProvider.Instance.copied}");
            }
        }

        public async void GoBackCommand()
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow.Closing -= MainWindow_Closing;
            }

            LauncherSettingsProvider.Instance.AllowSettings = true;

            if (!LauncherSettingsProvider.Instance.SaveSettings())
            {
                SendNotification("", LocalizationProvider.Instance.failed_to_save_settings, NotificationType.Error);
            }

            // Se tem opcionais pendentes, criar novo ProfileViewModel (roda InitializeAsync com progresso)
            bool hasPending = LauncherSettingsProvider.Instance.PendingOptionalChanges.Count > 0;
            if (hasPending)
            {
                NavigateTo(new ProfileViewModel(HostScreen));
            }
            else
            {
                NavigateBack();
            }
        }

        public void CleanTempFilesCommand()
        {
            LogManager.Instance.Info("[Settings] Clearing temp files ...");
            bool filesCleared = gameStarter.CleanTempFiles(LauncherSettingsProvider.Instance.GamePath);

            if (filesCleared)
            {
                LogManager.Instance.Info("[Settings] Temp files cleared");
                SendNotification("", LocalizationProvider.Instance.clean_temp_files_succeeded, NotificationType.Success);
            }
            else
            {
                LogManager.Instance.Info("[Settings] Temp files failed to clear");
                SendNotification("", LocalizationProvider.Instance.clean_temp_files_failed, NotificationType.Error);
            }
        }

        /* Desativado: Carregar configurações do jogo oficial
        public async Task ResetGameSettingsCommand()
        {
            LogManager.Instance.Info("[Settings] Reseting game settings ...");
            string EFTSettingsFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Battlestate Games", "Escape from Tarkov", "Settings");
            string SPTSettingsFolder = Path.Join(LauncherSettingsProvider.Instance.GamePath, "SPT", "user", "sptsettings");

            if (!Directory.Exists(EFTSettingsFolder))
            {
                LogManager.Instance.Warning($"[Settings] EFT settings folder not found, can't reset :: Path: {EFTSettingsFolder}");
                SendNotification("", LocalizationProvider.Instance.load_live_settings_failed, Avalonia.Controls.Notifications.NotificationType.Error);
                return;
            }

            try
            {
                Directory.CreateDirectory(SPTSettingsFolder);

                foreach (string dirPath in Directory.GetDirectories(EFTSettingsFolder, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(EFTSettingsFolder, SPTSettingsFolder));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(EFTSettingsFolder, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(EFTSettingsFolder, SPTSettingsFolder), true);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                SendNotification("", LocalizationProvider.Instance.load_live_settings_failed, Avalonia.Controls.Notifications.NotificationType.Error);
                return;
            }
            
            LogManager.Instance.Info("[Settings] Game settings reset to live settings");
            SendNotification("", LocalizationProvider.Instance.load_live_settings_succeeded, Avalonia.Controls.Notifications.NotificationType.Success);
        }
        */

        public async Task ClearGameSettingsCommand()
        {
            LogManager.Instance.Info("[Settings] Clearing game settings ...");
            var SPTSettingsDir = new DirectoryInfo(Path.Join(LauncherSettingsProvider.Instance.GamePath, "SPT", "user", "sptsettings"));

            try
            {
                SPTSettingsDir.Delete(true);

                Directory.CreateDirectory(SPTSettingsDir.FullName);
            }
            catch(Exception ex)
            {
                LogManager.Instance.Exception(ex);
                SendNotification("", LocalizationProvider.Instance.clear_game_settings_failed, NotificationType.Error);
                return;
            }
            
            LogManager.Instance.Info("[Settings] Game settings cleared");
            SendNotification("", LocalizationProvider.Instance.clear_game_settings_succeeded, NotificationType.Success);
        }

        public void OpenGameFolderCommand()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.EndsInDirectorySeparator(LauncherSettingsProvider.Instance.GamePath) ? LauncherSettingsProvider.Instance.GamePath : LauncherSettingsProvider.Instance.GamePath + Path.DirectorySeparatorChar,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public async Task SelectGameFolderCommand()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // AppContext.BaseDirectory em vez de Assembly.Location: este último é vazio
                // em publish single-file (IL3000), zerando a sugestão de pasta do picker (item 014).
                var startPath = await desktop.MainWindow.StorageProvider.TryGetFolderFromPathAsync(AppContext.BaseDirectory);
                
                var dir = await desktop.MainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select your SPT folder",
                    SuggestedStartLocation = startPath
                });

                if (dir == null || dir.Count == 0)
                {
                    return;
                }

                LauncherSettingsProvider.Instance.GamePath = dir[0].Path.LocalPath;
            }
        }
    }
}
