using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models;
using SPT.Launcher.Models.Launcher;
using Avalonia;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using SPT.Launcher.Attributes;
using SPT.Launcher.ViewModels.Dialogs;
using Avalonia.Threading;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Newtonsoft.Json.Linq;
using SPT.Launcher.Controllers;
using SPT.Launcher.Models.SPT;
using Splat;

namespace SPT.Launcher.ViewModels
{
    [RequireLoggedIn]
    public class ProfileViewModel : ViewModelBase
    {
        private string _CurrentEdition;
        public string CurrentEdition
        {
            get => _CurrentEdition;
            set => this.RaiseAndSetIfChanged(ref _CurrentEdition, value);
        }

        private bool _WipeProfileOnStart;
        public bool WipeProfileOnStart
        {
            get => _WipeProfileOnStart;
            set => this.RaiseAndSetIfChanged(ref _WipeProfileOnStart, value);
        }

        private bool _ProfileWipePending;
        public bool ProfileWipePending
        {
            get => _ProfileWipePending;
            set => this.RaiseAndSetIfChanged(ref _ProfileWipePending, value);
        }

        public string CurrentId { get; set; }

        public ProfileInfo ProfileInfo { get; set; } = AccountManager.SelectedProfileInfo;

        public ImageHelper SideImage { get; } = new ImageHelper();

        public ImageHelper Background => Splat.Locator.Current.GetService<ImageHelper>("bgimage");
        public SPTVersion VersionInfo => Splat.Locator.Current.GetService<SPTVersion>("sptversion");

        public ModInfoCollection ModInfoCollection { get; set; } = new ModInfoCollection();

        // === Mods Opcionais Dinâmicos ===
        public ObservableCollection<OptionalModToggle> OptionalMods { get; } = new ObservableCollection<OptionalModToggle>();

        private string _serverVersion = "1.5.7";
        public string ServerVersion
        {
            get => _serverVersion;
            set => this.RaiseAndSetIfChanged(ref _serverVersion, value);
        }

        private readonly GameStarter _gameStarter = new GameStarter(new GameStarterFrontend());

        private readonly ProcessMonitor _monitor;

        // === Update system properties ===

        private string _updateStatusText = "";
        public string UpdateStatusText
        {
            get => _updateStatusText;
            set => this.RaiseAndSetIfChanged(ref _updateStatusText, value);
        }

        private double _updateProgress = 0;
        public double UpdateProgress
        {
            get => _updateProgress;
            set => this.RaiseAndSetIfChanged(ref _updateProgress, value);
        }

        private double _updateMaxProgress = 100;
        public double UpdateMaxProgress
        {
            get => _updateMaxProgress;
            set => this.RaiseAndSetIfChanged(ref _updateMaxProgress, value);
        }

        private bool _isUpdateVisible = false;
        public bool IsUpdateVisible
        {
            get => _isUpdateVisible;
            set => this.RaiseAndSetIfChanged(ref _isUpdateVisible, value);
        }

        private bool _canUpdate = false;
        public bool CanUpdate
        {
            get => _canUpdate;
            set => this.RaiseAndSetIfChanged(ref _canUpdate, value);
        }

        private int _outdatedFiles = 0;
        public int OutdatedFiles
        {
            get => _outdatedFiles;
            set => this.RaiseAndSetIfChanged(ref _outdatedFiles, value);
        }

        public ICommand UpdateModsCommand { get; }
        public ICommand VerifyFilesCommand { get; }

        private List<ManifestFile> _filesToUpdate = new List<ManifestFile>();
        private List<string> _filesToDelete = new List<string>();

        public ProfileViewModel(IScreen Host) : base(Host)
        {
            // cache and load side image if profile has a side
            if(AccountManager.SelectedProfileInfo != null && AccountManager.SelectedProfileInfo.Side != null)
            {
                ImageRequest.CacheSideImage(AccountManager.SelectedProfileInfo.Side);
                SideImage.Path = AccountManager.SelectedProfileInfo.SideImage;
                SideImage.Touch();
            }

            _monitor = new ProcessMonitor("EscapeFromTarkov", 1000, aliveCallback: GameAliveCallBack, exitCallback: GameExitCallback);

            CurrentEdition = AccountManager.SelectedAccount.edition;

            CurrentId = AccountManager.SelectedAccount.id;

            UpdateModsCommand = ReactiveCommand.CreateFromTask(async () => await DoUpdateMods());
            VerifyFilesCommand = ReactiveCommand.CreateFromTask(async () => await ForceCheckForUpdates());

            LauncherSettingsProvider.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LauncherSettingsProvider.Instance.CanStartGame))
                {
                    this.RaisePropertyChanged(nameof(CanStartGame));
                }
            };

            // Auto-check for updates, depois aplica opcionais pendentes
            _ = InitializeAsync();
        }

        private static readonly System.Threading.SemaphoreSlim _optionalToggleSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        /// <summary>
        /// Chamado quando um toggle de mod opcional muda na UI.
        /// </summary>
        private async Task OnOptionalToggled(OptionalModToggle toggle)
        {
            await _optionalToggleSemaphore.WaitAsync();
            try
            {
                LauncherSettingsProvider.Instance.SetOptionalEnabled(toggle.Id, toggle.IsEnabled);

                LauncherSettingsProvider.Instance.IsUpdating = true;
                UpdateMaxProgress = 100;
                UpdateProgress = 0;

                Action<double> progressHandler = p => Dispatcher.UIThread.Post(() => UpdateProgress = p);
                Action<string> statusHandler = msg => Dispatcher.UIThread.Post(() => UpdateStatusText = msg);
                OptionalModsHelper.OnProgressChanged += progressHandler;
                OptionalModsHelper.OnStatusMessageChanged += statusHandler;

                if (toggle.IsEnabled)
                {
                    LogManager.Instance.Info($"[Profile] Ativando mod opcional '{toggle.Id}'...");
                    await OptionalModsHelper.DownloadOptionalGroupAsync(toggle.Id);
                }
                else
                {
                    LogManager.Instance.Info($"[Profile] Desativando mod opcional '{toggle.Id}'...");
                    await OptionalModsHelper.RemoveOptionalGroupAsync(toggle.Id);
                }

                OptionalModsHelper.OnProgressChanged -= progressHandler;
                OptionalModsHelper.OnStatusMessageChanged -= statusHandler;

                Dispatcher.UIThread.Post(() =>
                {
                    UpdateStatusText = LocalizationProvider.Instance.update_up_to_date;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                });
                LogManager.Instance.Info($"[Profile] Mod opcional '{toggle.Id}' {(toggle.IsEnabled ? "ativado" : "desativado")} com sucesso.");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Profile] Erro ao aplicar mod opcional '{toggle.Id}': {ex.Message}");
                UpdateStatusText = $"Erro ao aplicar mod opcional: {ex.Message}";
            }
            finally
            {
                LauncherSettingsProvider.Instance.IsUpdating = false;
                _optionalToggleSemaphore.Release();
            }
        }

        public bool CanStartGame => LauncherSettingsProvider.Instance.CanStartGame;

        private async Task InitializeAsync()
        {
            try
            {
                string configPath = Path.Combine(LauncherSettingsProvider.Instance.GamePath, "SPT", "user", "mods", "TarkovRedLine-ServerMod", "config.json");
                if (File.Exists(configPath))
                {
                    var config = JObject.Parse(File.ReadAllText(configPath));
                    if (config["serverVersion"] != null)
                    {
                        ServerVersion = config["serverVersion"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Profile] Failed to parse ServerMod config.json: {ex.Message}");
            }

            await CheckForUpdates();
        }

        private async Task GameVersionCheck()
        {
            string compatibleGameVersion = ServerManager.GetCompatibleGameVersion();

            if (compatibleGameVersion == "") return;

            // get the product version of the exe
            string gameVersion = FileVersionInfo.GetVersionInfo(Path.Join(LauncherSettingsProvider.Instance.GamePath, "EscapeFromTarkov.exe")).FileVersion;

            if (gameVersion == null) return;

            // if the compatible version isn't the same as the game version show a warning dialog
            if(compatibleGameVersion != gameVersion)
            {
                WarningDialogViewModel warning = new WarningDialogViewModel(null,
                                                     string.Format(LocalizationProvider.Instance.game_version_mismatch_format_2, gameVersion, compatibleGameVersion),
                                                     LocalizationProvider.Instance.i_understand);
                Dispatcher.UIThread.InvokeAsync(async() =>
                {
                    await ShowDialog(warning);
                });
            }
        }

        public void OpenModsInfoCommand() =>
            NavigateTo(new ModInfoViewModel(HostScreen, ModInfoCollection));

        public void OpenSettingsCommand() =>
            NavigateTo(new SettingsViewModel(HostScreen));

        public void OpenLinkCommand(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
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
        /// Verificação manual — ignora versão local e faz scan completo
        /// </summary>
        private async Task ForceCheckForUpdates()
        {
            string gamePath = LauncherSettingsProvider.Instance.GamePath;
            string hashFilePath = Path.Combine(gamePath, "SPT", "user", "launcher", "manifest_hash.txt");

            // Deletar hash local para forçar scan completo
            if (File.Exists(hashFilePath))
                File.Delete(hashFilePath);

            LogManager.Instance.Info("[Profile] Verificação manual solicitada — forçando scan completo...");
            await CheckForUpdates();
        }

        /// <summary>
        /// Verifica atualizações comparando versão do servidor.
        /// Se a versão for a mesma, pula o scan completo (rápido).
        /// Se manifest hash igual, pula scan (mods atualizados).
        /// Se diferente, compara hashes com o manifesto do servidor.
        /// </summary>
        private async Task CheckForUpdates()
        {
            // Sempre usa a pasta onde o jogo está configurado
            string gamePath = LauncherSettingsProvider.Instance.GamePath;

            bool manifestFailed = false;

            try
            {
                if (LauncherSettingsProvider.Instance.IsDevMode)
                {
                    LogManager.Instance.Info("[Profile] DevMode ativo. Pulando varredura e download do manifesto...");
                    LauncherSettingsProvider.Instance.IsUpdating = false;
                    IsUpdateVisible = false;
                    return;
                }

                LauncherSettingsProvider.Instance.IsUpdating = true;
                IsUpdateVisible = true;
                UpdateStatusText = LocalizationProvider.Instance.update_checking;
                UpdateProgress = 0;
                _filesToUpdate.Clear();
                _filesToDelete.Clear();
                LogManager.Instance.Info("[Profile] Verificando atualizações de mods...");

                // 1. Buscar hash do manifesto do servidor (endpoint leve)
                string hashFilePath = Path.Combine(gamePath, "SPT", "user", "launcher", "manifest_hash.txt");
                string serverManifestHash = "";
                try
                {
                    string hashResponse = await Task.Run(() => RequestHandler.RequestManifestHash());
                    var hashData = JObject.Parse(hashResponse);
                    serverManifestHash = hashData["hash"]?.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[Profile] Não foi possível obter manifest hash: {ex.Message}. Fazendo scan completo...");
                }

                // 2. Comparar com hash local salvo
                string localManifestHash = "";
                if (File.Exists(hashFilePath))
                {
                    localManifestHash = File.ReadAllText(hashFilePath).Trim();
                }

                bool skipFileScan = false; // Desabilitado a pedido: força o launcher a sempre verificar integridade local

                if (skipFileScan)
                {
                    LogManager.Instance.Info($"[Profile] Manifest hash igual ({serverManifestHash.Substring(0, 8)}...). Mods atualizados, pulando scan no disco (carregando apenas UI).");
                }
                else
                {
                    LogManager.Instance.Info($"[Profile] Manifest hash diferente (local={localManifestHash}, servidor={serverManifestHash}). Fazendo scan completo...");
                }

                // 3. Buscar manifesto completo (com retry se servidor ainda gerando)
                string response = null;
                for (int attempt = 1; attempt <= 5; attempt++)
                {
                    try
                    {
                        response = await Task.Run(() => RequestHandler.RequestModsManifest());
                        var testParse = JObject.Parse(response);
                        if (testParse["files"] != null) break; // Manifesto válido
                    }
                    catch { }

                    // Servidor ainda gerando manifesto — aguardar e tentar novamente
                    LogManager.Instance.Info($"[Profile] Manifesto não disponível, tentativa {attempt}/5. Aguardando...");
                    UpdateStatusText = "Servidor está gerando a lista de atualização, aguarde...";
                    await Task.Delay(3000);
                    response = null;
                }

                if (string.IsNullOrEmpty(response))
                {
                    LogManager.Instance.Warning("[Profile] Manifesto não disponível após 5 tentativas. Reagendando em 30s...");
                    manifestFailed = true;

                    // Countdown de 30 segundos com retry automático
                    for (int s = 30; s > 0; s--)
                    {
                        UpdateStatusText = $"Servidor preparando a lista. Tentando novamente em {s}s...";
                        await Task.Delay(1000);
                    }

                    manifestFailed = false;
                    // Retry recursivo — tenta novamente todo o fluxo
                    await CheckForUpdates();
                    return;
                }

                var manifest = JObject.Parse(response);
                var allFiles = manifest["files"]?.ToObject<List<ManifestFile>>() ?? new List<ManifestFile>();
                var managedPaths = manifest["managedPaths"]?.ToObject<List<string>>() ?? new List<string>();
                var deleteFiles = manifest["deleteFiles"]?.ToObject<List<string>>() ?? new List<string>();
                var ignoredFiles = manifest["ignoredFiles"]?.ToObject<List<string>>() ?? new List<string>();
                var optionalGroups = manifest["optionalGroups"]?.ToObject<List<OptionalModsHelper.OptionalGroupInfo>>() ?? new List<OptionalModsHelper.OptionalGroupInfo>();

                // Atualizar cache de opcionais e popular toggles na UI
                var optionalManifestFiles = allFiles
                    .Where(f => f.optional)
                    .Select(f => new OptionalModsHelper.ManifestFile { path = f.path, hash = f.hash, size = f.size, optional = f.optional, optionalGroup = f.optionalGroup })
                    .ToList();
                OptionalModsHelper.UpdateFromManifest(optionalGroups, optionalManifestFiles);

                // Popular toggles dinâmicos na UI
                Dispatcher.UIThread.Post(() =>
                {
                    OptionalMods.Clear();
                    foreach (var group in optionalGroups)
                    {
                        var toggle = new OptionalModToggle
                        {
                            Id = group.id,
                            Name = group.name,
                            Description = group.description ?? "",
                            IsEnabled = LauncherSettingsProvider.Instance.IsOptionalEnabled(group.id)
                        };
                        toggle.WhenAnyValue(x => x.IsEnabled).Skip(1).Subscribe(val => { var _ = OnOptionalToggled(toggle); });
                        OptionalMods.Add(toggle);
                    }
                });

                // Se as hashes são iguais, já terminamos o trabalho inicial (que era só montar a UI)
                if (skipFileScan)
                {
                    UpdateStatusText = LocalizationProvider.Instance.update_up_to_date;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                    return;
                }

                // Separar arquivos: obrigatórios vs opcionais
                var mandatoryFiles = allFiles.Where(f => !f.optional).ToList();
                var optionalFiles = allFiles.Where(f => f.optional).ToList();

                // Criar set de TODOS os caminhos do manifesto (obrigatórios + opcionais) para proteção
                var manifestFilePaths = new HashSet<string>(
                    allFiles.Select(f => f.path.Replace('/', Path.DirectorySeparatorChar).ToLowerInvariant())
                );

                // Determinar quais arquivos opcionais precisam de update (só se grupo ativo)
                var activeOptionalFiles = optionalFiles
                    .Where(f => LauncherSettingsProvider.Instance.IsOptionalEnabled(f.optionalGroup))
                    .ToList();

                var filesToCheck = mandatoryFiles.Concat(activeOptionalFiles).ToList();

                UpdateMaxProgress = filesToCheck.Count;
                int checkedCount = 0;
                int outdated = 0;

                // 1. Verificar arquivos do manifesto (faltantes ou desatualizados)
                foreach (var file in filesToCheck)
                {
                    checkedCount++;
                    UpdateProgress = checkedCount;
                    UpdateStatusText = string.Format(LocalizationProvider.Instance.update_checking_file, checkedCount, filesToCheck.Count) + " - " + file.path;

                    string localPath = Path.Combine(gamePath, file.path.Replace('/', Path.DirectorySeparatorChar));

                    bool needsUpdate = false;
                    if (!File.Exists(localPath))
                    {
                        needsUpdate = true;
                        LogManager.Instance.Info($"[Profile] Arquivo faltando: {file.path}");
                    }
                    else
                    {
                        string localHash = await Task.Run(() => GetFileMD5(localPath));
                        if (localHash != file.hash)
                        {
                            needsUpdate = true;
                            LogManager.Instance.Info($"[Profile] Hash diferente: {file.path} (local={localHash}, servidor={file.hash})");
                        }
                    }

                    if (needsUpdate)
                    {
                        _filesToUpdate.Add(file);
                        outdated++;
                    }
                }

                // 2. Verificar arquivos extras nas pastas gerenciadas (managedPaths)
                // Proteger TODOS os opcionais conhecidos (ativos E inativos) contra deleção
                foreach (var managedPath in managedPaths)
                {
                    string localManagedDir = Path.Combine(gamePath, managedPath.Replace('/', Path.DirectorySeparatorChar));
                    if (!Directory.Exists(localManagedDir)) continue;

                    var localFiles = Directory.GetFiles(localManagedDir, "*", SearchOption.AllDirectories);
                    foreach (var localFile in localFiles)
                    {
                        string relativePath = Path.GetRelativePath(gamePath, localFile).ToLowerInvariant();
                        string relPathNormalized = relativePath.Replace('\\', '/');
                        
                        bool isIgnored = ignoredFiles.Any(ig => relPathNormalized.Contains(ig.Replace('\\', '/').ToLowerInvariant()));

                        // Proteger: se está no manifesto (obrigatório ou opcional) → não deletar
                        if (!manifestFilePaths.Contains(relativePath) && !isIgnored)
                        {
                            _filesToDelete.Add(localFile);
                        }
                    }
                }

                // 3. Deletar automaticamente arquivos da lista deleteFiles (não precisa de clique)
                foreach (var deleteFile in deleteFiles)
                {
                    string localPath = Path.Combine(gamePath, deleteFile.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(localPath))
                    {
                        try
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(localPath, 
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, 
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                            LogManager.Instance.Info($"[Profile] Movido para lixeira: {deleteFile}");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.Error($"[Profile] Falha ao deletar {deleteFile}: {ex.Message}");
                        }
                    }
                }

                OutdatedFiles = outdated;
                int totalActions = outdated + _filesToDelete.Count;

                if (totalActions > 0)
                {
                    LogManager.Instance.Info($"[Profile] {outdated} arquivos para atualizar, {_filesToDelete.Count} extras para deletar. Iniciando auto-update...");
                    // Atualização automática — sem botão
                    await DoUpdateMods();
                }
                else
                {
                    UpdateStatusText = LocalizationProvider.Instance.update_up_to_date;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                    LogManager.Instance.Info("[Profile] Todos os mods estão atualizados.");
                }

                // Salvar manifest hash local — na próxima abertura, pula scan se igual
                try
                {
                    if (!string.IsNullOrEmpty(serverManifestHash))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(hashFilePath));
                        File.WriteAllText(hashFilePath, serverManifestHash);
                        LogManager.Instance.Info($"[Profile] Manifest hash salvo: {serverManifestHash.Substring(0, 8)}...");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[Profile] Falha ao salvar manifest hash: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Profile] Erro ao verificar atualizações: {ex.Message}");
                UpdateStatusText = string.Format(LocalizationProvider.Instance.update_error, ex.Message);
            }
            finally
            {
                if (!manifestFailed)
                    LauncherSettingsProvider.Instance.IsUpdating = false;
            }
        }



        /// <summary>
        /// Baixa e instala todos os arquivos desatualizados
        /// </summary>
        private async Task DoUpdateMods()
        {
            if (_filesToUpdate.Count == 0 && _filesToDelete.Count == 0) return;

            LogManager.Instance.Info($"[Profile] Iniciando atualização: {_filesToUpdate.Count} arquivos para baixar, {_filesToDelete.Count} para deletar");
            string gamePath = LauncherSettingsProvider.Instance.GamePath;
            CanUpdate = false;
            LauncherSettingsProvider.Instance.IsUpdating = true;
            int totalActions = _filesToUpdate.Count + _filesToDelete.Count;
            UpdateMaxProgress = totalActions;
            UpdateProgress = 0;

            int completed = 0;
            int errors = 0;

            // 1. Baixar arquivos atualizados/faltantes
            foreach (var file in _filesToUpdate)
            {
                UpdateStatusText = string.Format(LocalizationProvider.Instance.update_downloading, file.path, completed + 1, totalActions);

                try
                {
                    string localPath = Path.Combine(gamePath, file.path.Replace('/', Path.DirectorySeparatorChar));
                    string directory = Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    byte[] fileData = await Task.Run(() => RequestHandler.DownloadModFile(file.path));
                    await File.WriteAllBytesAsync(localPath, fileData);
                }
                catch (Exception ex)
                {
                    errors++;
                    LogManager.Instance.Error($"[ModUpdate] Failed to update {file.path}: {ex.Message}");
                }

                completed++;
                UpdateProgress = completed;
            }

            // 2. Deletar arquivos extras dentro das pastas gerenciadas
            foreach (var fileToDelete in _filesToDelete)
            {
                string relativePath = Path.GetRelativePath(gamePath, fileToDelete);
                UpdateStatusText = string.Format(LocalizationProvider.Instance.update_deleting, relativePath, completed + 1, totalActions);

                try
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(fileToDelete, 
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, 
                        Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    LogManager.Instance.Info($"[ModUpdate] Movido para lixeira: {relativePath}");
                }
                catch (Exception ex)
                {
                    errors++;
                    LogManager.Instance.Error($"[ModUpdate] Failed to delete {relativePath}: {ex.Message}");
                }

                completed++;
                UpdateProgress = completed;
            }

            if (errors > 0)
            {
                LogManager.Instance.Warning($"[Profile] Atualização concluída com {errors} erros de {totalActions} ações");
                UpdateStatusText = string.Format(LocalizationProvider.Instance.update_completed_with_errors, totalActions - errors, errors);
            }
            else
            {
                LogManager.Instance.Info($"[Profile] Atualização concluída: {totalActions} ações sem erros");
                UpdateStatusText = string.Format(LocalizationProvider.Instance.update_completed, totalActions);
            }

            _filesToUpdate.Clear();
            _filesToDelete.Clear();
            OutdatedFiles = 0;
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

        public void LogoutCommand()
        {
            AccountManager.Logout();

            // Ir direto pro login — servidor já está conectado, não precisa refazer Tailscale
            NavigateTo(new LoginViewModel(HostScreen, true));
        }

        public void ChangeWindowState(Avalonia.Controls.WindowState? State, bool Close = false)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    if (Close)
                    {
                        desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
                        desktop.Shutdown();
                    }
                    else
                    {
                        desktop.MainWindow.WindowState = State ?? Avalonia.Controls.WindowState.Normal;
                    }
                }
            });
        }

        public async Task StartGameCommand()
        {
            LauncherSettingsProvider.Instance.AllowSettings = false;
            LogManager.Instance.Info("[Profile] Iniciando jogo...");

            AccountStatus status = await AccountManager.LoginAsync(AccountManager.SelectedAccount.username, AccountManager.SelectedAccount.password);
            LogManager.Instance.Info($"[Profile] Re-login para iniciar: {status}");

            LauncherSettingsProvider.Instance.AllowSettings = true;

            switch (status)
            {
                case AccountStatus.NoConnection:
                    NavigateTo(new ConnectServerViewModel(HostScreen));
                    return;
            }

            LauncherSettingsProvider.Instance.GameRunning = true;

            if (WipeProfileOnStart)
            {
                var wipeStatus = await WipeProfile(AccountManager.SelectedAccount.edition);

                if (wipeStatus != AccountStatus.OK)
                {
                    LauncherSettingsProvider.Instance.GameRunning = false;
                    return;
                }

                WipeProfileOnStart = false;
            }

            GameStarterResult gameStartResult = await _gameStarter.LaunchGame(ServerManager.SelectedServer, AccountManager.SelectedAccount, LauncherSettingsProvider.Instance.GamePath);

            if (gameStartResult.Succeeded)
            {
                LogManager.Instance.Info("[Profile] Jogo iniciado com sucesso!");
                _monitor.Start();

                switch (LauncherSettingsProvider.Instance.LauncherStartGameAction)
                {
                    case LauncherAction.MinimizeAction:
                        {
                            ChangeWindowState(Avalonia.Controls.WindowState.Minimized);
                            break;
                        }
                    case LauncherAction.ExitAction:
                        {
                            ChangeWindowState(null, true);
                            break;
                        }
                }
            }
            else
            {
                LogManager.Instance.Error($"[Profile] Falha ao iniciar jogo: {gameStartResult.Message}");
                SendNotification("", gameStartResult.Message, Avalonia.Controls.Notifications.NotificationType.Error);
                LauncherSettingsProvider.Instance.GameRunning = false;
            }
        }

        private async Task<AccountStatus> WipeProfile(string edition)
        {
            // Salvar credenciais antes de deletar
            string username = AccountManager.SelectedAccount.username;
            string password = AccountManager.SelectedAccount.password;

            LogManager.Instance.Info($"[Profile] Iniciando wipe: deletar + recriar com edição '{edition}'...");

            // 1. Deletar perfil antigo
            LogManager.Instance.Info("[Profile] Etapa 1/3: Removendo perfil antigo...");
            AccountStatus removeStatus = await AccountManager.RemoveAsync();

            if (removeStatus != AccountStatus.OK)
            {
                LogManager.Instance.Error($"[Profile] Falha ao remover perfil: {removeStatus}");
                if (removeStatus == AccountStatus.NoConnection)
                    NavigateTo(new ConnectServerViewModel(HostScreen));
                else
                    SendNotification("", "Erro ao remover perfil antigo.");
                return removeStatus;
            }

            LogManager.Instance.Info("[Profile] Perfil antigo removido com sucesso.");

            // 2. Recriar perfil com nova edição
            LogManager.Instance.Info($"[Profile] Etapa 2/3: Criando novo perfil com edição '{edition}'...");
            AccountStatus registerStatus = await AccountManager.RegisterAsync(username, password, edition);

            if (registerStatus != AccountStatus.OK)
            {
                LogManager.Instance.Error($"[Profile] Falha ao recriar perfil: {registerStatus}");
                if (registerStatus == AccountStatus.NoConnection)
                    NavigateTo(new ConnectServerViewModel(HostScreen));
                else
                    SendNotification("", "Erro ao criar novo perfil. Tente registrar manualmente.");
                return registerStatus;
            }

            LogManager.Instance.Info("[Profile] Novo perfil criado com sucesso.");

            // 3. Atualizar UI
            LogManager.Instance.Info("[Profile] Etapa 3/3: Atualizando interface...");
            CurrentEdition = AccountManager.SelectedAccount.edition;
            UpdateProfileInfo();
            SendNotification("", $"Perfil resetado com sucesso! Edição: {edition}");

            LogManager.Instance.Info($"[Profile] Wipe completo: {username} → {edition}");
            return AccountStatus.OK;
        }

        public async Task ChangeEditionCommand()
        {
            var result = await ShowDialog(new ChangeEditionDialogViewModel(null));

            if(result != null && result is SPTEdition edition)
            {
                await WipeProfile(edition.Name);
            }
        }

        public async Task WipeConfirmCommand()
        {
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(null,
                "Tem certeza que deseja resetar sua conta? Esta ação não pode ser revertida. Todo seu progresso será perdido.");

            var result = await ShowDialog(confirmation);

            if (result is bool b && !b) return;

            await WipeProfile(AccountManager.SelectedAccount.edition);
        }

        public async Task CopyCommand(object parameter)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && parameter is string text)
            {
                if (desktop?.MainWindow?.Clipboard == null)
                {
                    return;
                }
                
                await desktop.MainWindow.Clipboard.SetTextAsync(text);
                SendNotification("", $"{text} {LocalizationProvider.Instance.copied}", Avalonia.Controls.Notifications.NotificationType.Success);
            }
        }

        public async Task RemoveProfileCommand()
        {
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(null, string.Format(LocalizationProvider.Instance.profile_remove_question_format_1, AccountManager.SelectedAccount.username));

            var result = await ShowDialog(confirmation);

            if (result is bool b && !b) return;

            AccountStatus status = await AccountManager.RemoveAsync();

            switch(status)
            {
                case AccountStatus.OK:
                    {
                        SendNotification("", LocalizationProvider.Instance.profile_removed);

                        LauncherSettingsProvider.Instance.Server.AutoLoginCreds = null;

                        LauncherSettingsProvider.Instance.SaveSettings();

                        NavigateTo(new ConnectServerViewModel(HostScreen));
                        break;
                    }
                case AccountStatus.UpdateFailed:
                    {
                        SendNotification("", LocalizationProvider.Instance.profile_removal_failed);
                        break;
                    }
                case AccountStatus.NoConnection:
                    {
                        SendNotification("", LocalizationProvider.Instance.no_servers_available);
                        NavigateTo(new ConnectServerViewModel(HostScreen));
                        break;
                    }
            }
        }

        private void UpdateProfileInfo()
        {
            AccountManager.UpdateProfileInfo();
            ImageRequest.CacheSideImage(AccountManager.SelectedProfileInfo.Side);
            ProfileInfo.UpdateDisplayedProfile(AccountManager.SelectedProfileInfo);
            if (ProfileInfo.SideImage != SideImage.Path)
            {
                SideImage.Path = ProfileInfo.SideImage;
                SideImage.Touch();
            }
        }


        //pull profile every x seconds
        private int aliveCallBackCountdown = 60;
        private void GameAliveCallBack(ProcessMonitor monitor)
        {
            aliveCallBackCountdown--;

            if (aliveCallBackCountdown <= 0)
            {
                aliveCallBackCountdown = 60;
                UpdateProfileInfo();
            }
        }

        private void GameExitCallback(ProcessMonitor monitor)
        {
            monitor.Stop();

            LauncherSettingsProvider.Instance.GameRunning = false;

            //Make sure the call to MainWindow happens on the UI thread.
            switch (LauncherSettingsProvider.Instance.LauncherStartGameAction)
            {
                case LauncherAction.MinimizeAction:
                    {
                        ChangeWindowState(Avalonia.Controls.WindowState.Normal);
                        ProfileWipePending = false;

                        break;
                    }
            }

            UpdateProfileInfo();
        }
    }
}
