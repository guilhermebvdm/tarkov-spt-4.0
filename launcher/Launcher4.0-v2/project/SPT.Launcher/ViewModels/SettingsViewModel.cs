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
        public string DevModeStatusText => LauncherSettingsProvider.Instance.IsDevMode ? LocalizationProvider.Instance.dev_mode_status_active : LocalizationProvider.Instance.dev_mode_status_inactive;

        // Item 030 (D-12): UsePerformanceConfigs removido — a performance agora é por item na tela
        // "Mods e Configs", não um toggle global aqui.

        private const string LocalServerUrl = "https://127.0.0.1:6969";

        /// <summary>
        /// Checkbox "Usar servidor local": liga → guarda a URL atual em SavedServerUrl e troca Server.Url
        /// pro local (127.0.0.1); desliga → restaura a URL guardada. Não destrói a URL de produção.
        /// Ferramenta de Dev Mode; efeito na próxima conexão (ao sair de Configurações o launcher reconecta).
        /// </summary>
        public bool UseLocalServer
        {
            get => LauncherSettingsProvider.Instance.UseLocalServer;
            set
            {
                if (LauncherSettingsProvider.Instance.UseLocalServer == value) return;

                if (value)
                {
                    // Guarda a URL atual (a menos que já seja o local — evita perder a de produção).
                    string current = LauncherSettingsProvider.Instance.Server.Url;
                    if (!string.Equals(current, LocalServerUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        LauncherSettingsProvider.Instance.SavedServerUrl = current;
                    }
                    LauncherSettingsProvider.Instance.Server.Url = LocalServerUrl;
                }
                else
                {
                    string saved = LauncherSettingsProvider.Instance.SavedServerUrl;
                    if (!string.IsNullOrEmpty(saved))
                    {
                        LauncherSettingsProvider.Instance.Server.Url = saved;
                    }
                }

                LauncherSettingsProvider.Instance.UseLocalServer = value;
                LauncherSettingsProvider.Instance.SaveSettings();
                this.RaisePropertyChanged(nameof(UseLocalServer));
                LogManager.Instance.Info($"[Settings] Servidor local {(value ? "LIGADO (127.0.0.1)" : "desligado")} — URL efetiva: {LauncherSettingsProvider.Instance.Server.Url}");
            }
        }

        /// <summary>
        /// Modo enxuto (aberto a partir de um erro de conexão): esconde o menu lateral e mostra só um
        /// botão "salvar e voltar". Ao voltar (GoBackCommand → NavigateBack), a tela anterior
        /// (ConnectServer) re-ativa e reconecta. Default false = tela normal com sidebar.
        /// </summary>
        public bool SlimMode { get; }
        public bool ShowSidebar => !SlimMode;

        // Estado do servidor ao ABRIR Configurações — para detectar (ao sair) se o usuário trocou o
        // servidor (via campo de URL ou checkbox "usar servidor local") e então reconectar de verdade.
        private readonly string _initialServerUrl;
        private readonly bool _initialUseLocalServer;

        public SettingsViewModel(IScreen Host, bool slim = false) : base(Host)
        {
            SlimMode = slim;
            _initialServerUrl = LauncherSettingsProvider.Instance.Server?.Url;
            _initialUseLocalServer = LauncherSettingsProvider.Instance.UseLocalServer;

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
                    SendNotification("", LocalizationProvider.Instance.dev_mode_enabled_notification, NotificationType.Success);
                    LogManager.Instance.Info("[Settings] Dev Mode ativado via senha");
                }
                else
                {
                    SendNotification("", LocalizationProvider.Instance.wrong_password_notification, NotificationType.Error);
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
                SendNotification("", LocalizationProvider.Instance.dev_mode_disabled_notification, NotificationType.Information);
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

        /// <summary>
        /// Item 033: abre a tela "Mods e Configs" pelo menu lateral (item canônico do SettingsView, que
        /// antes tinha só um "Mod List" em construção). Espelha ProfileViewModel.OpenModsConfigsCommand —
        /// desliga AllowSettings (a ModsConfigsView restaura ao sair). Desregistra o handler de fechamento
        /// e persiste as configs antes de navegar (o cleanup que o GoBack faz, sem a lógica de reconexão).
        /// </summary>
        public void OpenModsConfigsCommand()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow.Closing -= MainWindow_Closing;
            }

            if (!LauncherSettingsProvider.Instance.SaveSettings())
            {
                SendNotification("", LocalizationProvider.Instance.failed_to_save_settings, NotificationType.Error);
            }

            LauncherSettingsProvider.Instance.AllowSettings = false;
            NavigateMenu(new ModsConfigsViewModel(HostScreen));
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

            // Trocou de servidor (campo URL OU checkbox "usar servidor local")? Precisa RECONECTAR de
            // verdade no novo IP — reexecutar o fluxo inicial (ConnectServer), não só NavigateBack (que
            // voltaria pra tela anterior ainda no servidor antigo). Troca ida-e-volta pro mesmo valor = sem efeito.
            bool serverChanged = !string.Equals(_initialServerUrl,
                LauncherSettingsProvider.Instance.Server?.Url, StringComparison.OrdinalIgnoreCase);

            if (serverChanged)
            {
                if (AccountManager.SelectedAccount != null)
                {
                    // Logado: confirma (avisando que desloga) antes de aplicar. i18n.
                    var confirm = await ShowDialog(new Dialogs.ConfirmationDialogViewModel(
                        null, LocalizationProvider.Instance.change_server_confirm));

                    if (confirm is bool and true)
                    {
                        // Desloga e RESETA a stack pro fluxo inicial (não empilha — evita acúmulo de
                        // ConnectServer/Login/Profile a cada troca). NoAutoLogin = cai no login, deslogado.
                        AccountManager.Logout();
                        HostScreen.Router.NavigateAndReset.Execute(new ConnectServerViewModel(HostScreen, NoAutoLogin: true));
                    }
                    else
                    {
                        // Cancelou: reverte a troca (URL + checkbox) e volta sem reconectar.
                        LauncherSettingsProvider.Instance.Server.Url = _initialServerUrl;
                        LauncherSettingsProvider.Instance.UseLocalServer = _initialUseLocalServer;
                        LauncherSettingsProvider.Instance.SaveSettings();
                        NavigateBack();
                    }
                }
                else
                {
                    // Não logado: sem confirmação. Reseta pro fluxo inicial no novo IP (idêntico ao boot).
                    HostScreen.Router.NavigateAndReset.Execute(new ConnectServerViewModel(HostScreen));
                }

                return;
            }

            // Sem troca de servidor → comportamento original.
            // Se tem opcionais pendentes, criar novo ProfileViewModel (roda InitializeAsync com progresso)
            bool hasPending = LauncherSettingsProvider.Instance.PendingOptionalChanges.Count > 0;
            if (hasPending)
            {
                // NavigateAndReset (não NavigateTo): substitui a pilha por um único Profile fresco que roda
                // o apply. Evita empilhar um 2º ProfileViewModel (dois ILauncherHome), que o NavigateMenu
                // depois descartaria — junto de um apply em curso, com risco de sync concorrente.
                HostScreen.Router.NavigateAndReset.Execute(new ProfileViewModel(HostScreen));
            }
            else if (SlimMode)
            {
                // Modo enxuto (aberto de um erro de conexão, sem Launcher na pilha): volta à tela que abriu.
                NavigateBack();
            }
            else
            {
                // Menu normal: vai DIRETO ao Launcher (pilha rasa e determinística) — é item de menu, não "voltar".
                NavigateMenu(null);
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
                    Title = LocalizationProvider.Instance.select_spt_folder,
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
