using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models;
using SPT.Launcher.Models.Launcher;
using Avalonia;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SPT.Launcher.Sync;
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

        // Carrossel do fundo da tela principal: fonte = Image_Cache/bg/ (server) ∪ Assets/Backgrounds
        // (bundlado, fallback), troca a cada 10s, dots na base. Criado no ctor (semeia o 1º frame,
        // sem flash); timer só roda enquanto a tela está ativa (Start/Stop no WhenActivated).
        public BackgroundCarousel Carousel { get; } = new BackgroundCarousel();

        public ModInfoCollection ModInfoCollection { get; set; } = new ModInfoCollection();

        // === Mods Opcionais Dinâmicos ===
        public ObservableCollection<OptionalModToggle> OptionalMods { get; } = new ObservableCollection<OptionalModToggle>();

        // Fonte canônica: GET /redline/server/version, buscada pelo ServerManager no connect (item 013)
        private string _serverVersion = ServerManager.TrlServerVersion;
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

        // === Item 007 — sync engine (SPT.Launcher.Base/Sync) ===

        private bool _canCancelUpdate;
        public bool CanCancelUpdate
        {
            get => _canCancelUpdate;
            set => this.RaiseAndSetIfChanged(ref _canCancelUpdate, value);
        }

        private string _lastUpdateText = "";
        public string LastUpdateText
        {
            get => _lastUpdateText;
            set => this.RaiseAndSetIfChanged(ref _lastUpdateText, value);
        }

        private bool _hasLastUpdate;
        public bool HasLastUpdate
        {
            get => _hasLastUpdate;
            set => this.RaiseAndSetIfChanged(ref _hasLastUpdate, value);
        }

        // === Item 016 — taxa de download na barra de update (média móvel, MB/s decimal, PT-BR) ===

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

        /// <summary>Cache hit / nada baixando → texto vazio → o rótulo da taxa fica oculto.</summary>
        public bool HasDownloadSpeed => !string.IsNullOrEmpty(DownloadSpeedText);

        public ICommand UpdateModsCommand { get; }
        public ICommand VerifyFilesCommand { get; }
        public ICommand CancelUpdateCommand { get; }

        private CancellationTokenSource _syncCts;

        // ref: CR-01-01 — gate de reentrância do sync (0 = idle, 1 = rodando). Interlocked fecha a
        // janela entre o auto-check do login e um clique quase simultâneo em VERIFICAR ARQUIVOS.
        private int _syncGate;

        private bool _isSyncRunning;
        public bool IsSyncRunning
        {
            get => _isSyncRunning;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSyncRunning, value);
                this.RaisePropertyChanged(nameof(CanVerifyFiles));
            }
        }

        /// <summary>ref: CR-01-01 — botão VERIFICAR ARQUIVOS: além de CanStartGame, nunca durante um sync.</summary>
        public bool CanVerifyFiles => CanStartGame && !IsSyncRunning;

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

            UpdateModsCommand = ReactiveCommand.CreateFromTask(async () => await ForceCheckForUpdates());
            VerifyFilesCommand = ReactiveCommand.CreateFromTask(async () => await ForceCheckForUpdates());
            CancelUpdateCommand = ReactiveCommand.CreateFromTask(async () => await CancelUpdate());

            LoadLastUpdateInfo();

            LauncherSettingsProvider.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LauncherSettingsProvider.Instance.CanStartGame))
                {
                    this.RaisePropertyChanged(nameof(CanStartGame));
                    this.RaisePropertyChanged(nameof(CanVerifyFiles)); // ref: CR-01-01
                }
            };

            // ref: CR-02-01 (013L) — se o fetch do connect falhou transitoriamente, a versão
            // fica "—" a sessão toda (read-once). Refetch async barato aqui: ServerVersion é
            // reativo, então a ProfileView atualiza; síncrono seria pior (até 15s de UI freeze
            // no timeout — exatamente o cenário de falha).
            if (_serverVersion == "—")
            {
                _ = Task.Run(() =>
                {
                    var refreshed = ServerManager.RefreshTrlServerVersionIfUnknown();
                    Dispatcher.UIThread.Post(() => ServerVersion = refreshed);
                });
            }

            // Carrossel: o timer só avança enquanto a ProfileView está ativa (para ao ir p/ Settings,
            // retoma ao voltar) — evita timer rodando fora de tela (leak/CPU). Dispose real do
            // carrossel ocorre no logout via GC (timer parado não fica ancorado no Dispatcher).
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Carousel.Start();
                Disposable.Create(() => Carousel.Stop()).DisposeWith(disposables);
            });

            // Auto-check for updates, depois aplica opcionais pendentes
            _ = InitializeAsync();
        }

        private static readonly System.Threading.SemaphoreSlim _optionalToggleSemaphore = new System.Threading.SemaphoreSlim(1, 1);

        /// <summary>
        /// Chamado quando um toggle de mod opcional muda na UI.
        /// </summary>
        private async Task OnOptionalToggled(OptionalModToggle toggle)
        {
            // R-3: disparo programático (revert de falha total, D-021.A) — consome o guard e sai,
            // sem re-entrar no fluxo (evita o loop "desativar" que tentaria baixar offFolders).
            if (toggle.SuppressToggleHandler)
            {
                toggle.SuppressToggleHandler = false;
                return;
            }

            await _optionalToggleSemaphore.WaitAsync();

            Action<double> progressHandler = p => Dispatcher.UIThread.Post(() => UpdateProgress = p);
            Action<string> statusHandler = msg => Dispatcher.UIThread.Post(() => UpdateStatusText = msg);
            bool enabling = toggle.IsEnabled;

            try
            {
                LauncherSettingsProvider.Instance.IsUpdating = true;
                UpdateMaxProgress = 100;
                UpdateProgress = 0;

                OptionalModsHelper.OnProgressChanged += progressHandler;
                OptionalModsHelper.OnStatusMessageChanged += statusHandler;

                // Item 021: a persistência do estado "ligado" saiu daqui (era feita ANTES do download,
                // fingindo sucesso) — agora é decidida pelo resultado, em ApplyOptionalResult.
                OptionalOpResult result;
                if (enabling)
                {
                    LogManager.Instance.Info($"[Profile] Ativando mod opcional '{toggle.Id}'...");
                    result = await OptionalModsHelper.DownloadOptionalGroupAsync(toggle.Id);
                }
                else
                {
                    LogManager.Instance.Info($"[Profile] Desativando mod opcional '{toggle.Id}'...");
                    result = await OptionalModsHelper.RemoveOptionalGroupAsync(toggle.Id);
                }

                ApplyOptionalResult(toggle, enabling, result);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Profile] Erro ao aplicar mod opcional '{toggle.Id}': {ex.Message}");
                Dispatcher.UIThread.Post(() => UpdateStatusText = $"Erro ao aplicar mod opcional: {ex.Message}");
            }
            finally
            {
                OptionalModsHelper.OnProgressChanged -= progressHandler;
                OptionalModsHelper.OnStatusMessageChanged -= statusHandler;
                LauncherSettingsProvider.Instance.IsUpdating = false;
                _optionalToggleSemaphore.Release();
            }
        }

        /// <summary>
        /// Item 021 (B / RN-2 / D-021.A): traduz o <see cref="OptionalOpResult"/> em estado persistido + UI.
        /// Enable pleno → persiste true + "atualizado". Enable parcial (algo aplicou, algo falhou) →
        /// persiste true + erro visível "N de M". Enable falha TOTAL (nada aplicou / grupo não resolvido)
        /// → reverte para false, desmarca o toggle (guard R-3) e mostra erro. Disable → persiste false
        /// (intenção honrada); erro visível se algum arquivo falhou. Nunca mostra o verde silencioso quando
        /// houve falha (CA-021.4/5/6). Toda mudança de UI é marshalada para a UI thread.
        /// </summary>
        private void ApplyOptionalResult(OptionalModToggle toggle, bool enabling, OptionalOpResult result)
        {
            if (enabling && result.TotalFailure)
            {
                // D-021.A — reverter: estado persistido volta a false e o toggle desmarca sem re-disparar.
                LauncherSettingsProvider.Instance.SetOptionalEnabled(toggle.Id, false);
                string msg = result.GroupResolved
                    ? $"Falha ao ativar '{toggle.Id}': {result.Failed} de {result.Total} arquivo(s) falharam. Não aplicado."
                    : $"Falha ao ativar '{toggle.Id}': grupo indisponível no servidor. Não aplicado.";
                LogManager.Instance.Error($"[Profile] {msg}");
                Dispatcher.UIThread.Post(() =>
                {
                    // R-3 fix: só armar o guard se a atribuição realmente muda o valor.
                    // RaiseAndSetIfChanged é no-op quando IsEnabled já é false (usuário desmarcou
                    // durante o download) → o Subscribe não dispararia e o guard ficaria preso,
                    // engolindo em silêncio o próximo enable legítimo daquele grupo.
                    if (toggle.IsEnabled)
                    {
                        toggle.SuppressToggleHandler = true;
                        toggle.IsEnabled = false;
                    }
                    UpdateStatusText = msg;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                });
                return;
            }

            // Persiste a intenção: enable-com-algo-aplicado → true; disable → false.
            LauncherSettingsProvider.Instance.SetOptionalEnabled(toggle.Id, enabling);

            if (result.Failed > 0)
            {
                // Sucesso parcial / disable com falhas — visível como erro, mantém o que aplicou (RN-2).
                string msg = enabling
                    ? $"'{toggle.Id}' ativado com falhas: {result.Failed} de {result.Total} arquivo(s) falharam."
                    : $"'{toggle.Id}' desativado com falhas: {result.Failed} arquivo(s) não aplicado(s).";
                LogManager.Instance.Warning($"[Profile] {msg}");
                Dispatcher.UIThread.Post(() =>
                {
                    UpdateStatusText = msg;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                });
                return;
            }

            // Sucesso pleno.
            Dispatcher.UIThread.Post(() =>
            {
                UpdateStatusText = LocalizationProvider.Instance.update_up_to_date;
                UpdateMaxProgress = 1;
                UpdateProgress = 1;
            });
            LogManager.Instance.Info($"[Profile] Mod opcional '{toggle.Id}' {(enabling ? "ativado" : "desativado")} com sucesso.");
        }

        public bool CanStartGame => LauncherSettingsProvider.Instance.CanStartGame;

        /// <summary>
        /// Item 009: enriquece os toggles com as descrições do optionals-list
        /// (description.json por pasta de grupo no server). Join tolerante (spec 009 D2):
        /// descriptor.id == group.id, OU group.folders contém descriptor.id, OU
        /// descriptor.name == group.name — tudo case-insensitive. Falha é silenciosa
        /// (fica a descrição do optionalGroups, comportamento atual).
        /// </summary>
        private async Task EnrichOptionalDescriptionsAsync(List<OptionalModsHelper.OptionalGroupInfo> optionalGroups)
        {
            try
            {
                var descriptors = await OptionalModsHelper.FetchOptionalsListAsync();
                if (descriptors.Count == 0) return;

                bool preferPt = (LauncherSettingsProvider.Instance.DefaultLocale ?? "")
                    .StartsWith("Portuguese", StringComparison.OrdinalIgnoreCase);

                Dispatcher.UIThread.Post(() =>
                {
                    foreach (var toggle in OptionalMods)
                    {
                        var group = optionalGroups.FirstOrDefault(g =>
                            string.Equals(g.id, toggle.Id, StringComparison.OrdinalIgnoreCase));

                        var descriptor = FindOptionalDescriptor(descriptors, toggle, group);
                        if (descriptor == null) continue;

                        var text = OptionalModsHelper.ResolveDescription(descriptor, preferPt);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            toggle.Description = text;
                        }

                        // O name do optionalGroups é curado pelo operador — o do descriptor
                        // só entra quando o grupo não tem nome (spec 009 D1).
                        if (string.IsNullOrWhiteSpace(toggle.Name) && !string.IsNullOrWhiteSpace(descriptor.Name))
                        {
                            toggle.Name = descriptor.Name;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Profile] Falha ao enriquecer descrições dos opcionais: {ex.Message}");
            }
        }

        // ref: CR-01-01 (009) — três passadas com precedência GLOBAL (id exato → pastas do
        // grupo → nome), não first-match por descriptor: a ordem alfabética das pastas no
        // disco do server não pode decidir o match (grupo multi-pasta herdava a descrição da
        // primeira pasta alfabética mesmo existindo match mais forte adiante). No desempate
        // por pastas, vale a ordem de group.folders (curada pelo operador no config.json).
        private static OptionalModsHelper.OptionalFolderDescriptor FindOptionalDescriptor(
            List<OptionalModsHelper.OptionalFolderDescriptor> descriptors,
            OptionalModToggle toggle,
            OptionalModsHelper.OptionalGroupInfo group)
        {
            static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

            var byId = descriptors.FirstOrDefault(d => Eq(d.Id, toggle.Id));
            if (byId != null) return byId;

            if (group?.folders != null)
            {
                foreach (var folder in group.folders)
                {
                    var byFolder = descriptors.FirstOrDefault(d => Eq(d.Id, folder));
                    if (byFolder != null) return byFolder;
                }
            }

            if (!string.IsNullOrWhiteSpace(group?.name))
            {
                return descriptors.FirstOrDefault(d => Eq(d.Name, group.name));
            }

            return null;
        }

        private async Task InitializeAsync()
        {
            // Item 013: versão do server agora vem do endpoint /redline/server/version
            // (populada no connect via ServerManager) — o read do config.json do
            // TarkovRedLine-ServerMod foi removido por ser fonte local defasável.
            await CheckForUpdates();
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
        /// Verificação manual — ignora versão local e faz scan completo.
        /// Em Dev Mode a verificação manual RODA o motor (com proteção R5 — divergentes do
        /// baseline são preservados), enquanto o auto-check do login continua pulando.
        /// </summary>
        private async Task ForceCheckForUpdates()
        {
            // ref: CR-01-01 — não deletar o manifest_hash no meio de um sync em andamento
            if (IsSyncRunning)
            {
                LogManager.Instance.Info("[Profile] Sync já em andamento — verificação manual ignorada.");
                return;
            }

            string gamePath = LauncherSettingsProvider.Instance.GamePath;
            string hashFilePath = Path.Combine(gamePath, "SPT", "user", "launcher", "manifest_hash.txt");

            // Deletar hash local para forçar scan completo
            if (File.Exists(hashFilePath))
                File.Delete(hashFilePath);

            LogManager.Instance.Info("[Profile] Verificação manual solicitada — forçando scan completo...");
            await CheckForUpdates(manual: true);
        }

        /// <summary>
        /// ref: CR-01-01 — ponto de entrada ÚNICO do sync, com guard de reentrância: auto-check do
        /// login, clique em VERIFICAR ARQUIVOS e navegar-sair-voltar não podem rodar dois motores
        /// destrutivos concorrentes sobre os mesmos arquivos/baseline. O corpo fica em
        /// <see cref="CheckForUpdatesCore"/> (sem guard) porque o retry recursivo interno é o MESMO
        /// fluxo lógico e deve manter o "lock".
        /// </summary>
        private async Task CheckForUpdates(bool manual = false)
        {
            if (Interlocked.CompareExchange(ref _syncGate, 1, 0) != 0)
            {
                LogManager.Instance.Info("[Profile] Sync já em andamento — disparo concorrente ignorado.");
                return;
            }

            IsSyncRunning = true;

            try
            {
                await CheckForUpdatesCore(manual);
            }
            finally
            {
                IsSyncRunning = false;
                Interlocked.Exchange(ref _syncGate, 0);
            }
        }

        /// <summary>
        /// Item 007: verificação + aplicação via motor de sync (SPT.Launcher.Base/Sync).
        /// Regras por pasta (config preserva divergentes do baseline, config-server espelha
        /// com delete, patchers/plugins espelham movendo removidos p/ -disabled), apply
        /// atômico, cancelamento com confirmação e manifesto de mudanças em last-update.json.
        /// NÃO chamar diretamente — sempre via <see cref="CheckForUpdates"/> (guard CR-01-01).
        /// </summary>
        private async Task CheckForUpdatesCore(bool manual)
        {
            // Sempre usa a pasta onde o jogo está configurado
            string gamePath = LauncherSettingsProvider.Instance.GamePath;

            bool manifestFailed = false;
            bool devMode = LauncherSettingsProvider.Instance.IsDevMode;

            try
            {
                if (devMode && !manual)
                {
                    // Auto-check do login continua pulando em Dev Mode (login rápido, server
                    // pode nem estar de pé). A verificação manual roda o motor com R5.
                    LogManager.Instance.Info("[Profile] DevMode ativo. Pulando varredura automática (verificação manual disponível)...");
                    LauncherSettingsProvider.Instance.IsUpdating = false;
                    IsUpdateVisible = false;
                    return;
                }

                LauncherSettingsProvider.Instance.IsUpdating = true;
                IsUpdateVisible = true;
                UpdateStatusText = LocalizationProvider.Instance.update_checking;
                UpdateProgress = 0;
                _rateMeter.Reset();      // item 016 — taxa limpa a cada verificação
                DownloadSpeedText = "";
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
                    // Retry recursivo — tenta novamente todo o fluxo. Chama o Core direto (sem
                    // guard): é o mesmo fluxo lógico e o "lock" do CR-01-01 permanece ativo.
                    await CheckForUpdatesCore(manual);
                    return;
                }

                var manifest = JObject.Parse(response);
                var allFiles = manifest["files"]?.ToObject<List<ManifestFile>>() ?? new List<ManifestFile>();
                var managedPaths = manifest["managedPaths"]?.ToObject<List<string>>() ?? new List<string>();
                var deleteFiles = manifest["deleteFiles"]?.ToObject<List<string>>() ?? new List<string>();
                var ignoredFiles = manifest["ignoredFiles"]?.ToObject<List<string>>() ?? new List<string>();
                var folderRules = manifest["folderRules"]?.ToObject<Dictionary<string, string>>();
                var optionalGroups = manifest["optionalGroups"]?.ToObject<List<OptionalModsHelper.OptionalGroupInfo>>() ?? new List<OptionalModsHelper.OptionalGroupInfo>();
                var performanceOverlayFiles = manifest["performanceOverlay"]?.ToObject<List<ManifestFile>>() ?? new List<ManifestFile>();

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

                // Item 009: descrições vêm do server (description.json por grupo, via
                // optionals-list) — enriquecimento assíncrono, não bloqueia a verificação.
                _ = EnrichOptionalDescriptionsAsync(optionalGroups);

                // Se as hashes são iguais, já terminamos o trabalho inicial (que era só montar a UI)
                if (skipFileScan)
                {
                    UpdateStatusText = LocalizationProvider.Instance.update_up_to_date;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                    return;
                }

                // deleteFiles: lista explícita do server — mantida fora do motor (guard + lixeira).
                // ref: item 019 (B2) — cada entrada passa pelo MESMO ResolveUnderRoot do motor antes
                // de tocar disco: um manifesto adulterado com ".." ou caminho absoluto é rejeitado +
                // logado (Warning), sem escrever/deletar fora da raiz e sem abortar o loop (RN-1).
                foreach (var deleteFile in deleteFiles)
                {
                    try
                    {
                        string localPath = SyncPathUtil.ResolveUnderRoot(gamePath, deleteFile);
                        if (File.Exists(localPath))
                        {
                            RecycleBinHelper.Delete(localPath);
                            LogManager.Instance.Info($"[Profile] Movido para lixeira (deleteFiles): {deleteFile}");
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        LogManager.Instance.Warning($"[Profile] deleteFiles rejeitado (fora da raiz): {deleteFile} — {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Error($"[Profile] Falha ao deletar {deleteFile}: {ex.Message}");
                    }
                }

                // === Item 007: planejamento via motor (regras por pasta + baseline) ===
                _syncCts = new CancellationTokenSource();
                CanCancelUpdate = true;

                var resolver = new SyncRuleResolver(folderRules);
                var baseline = SyncBaseline.Load(Path.Combine(SyncStateDir, "sync-state.json"));

                // === Item 008: overlay de configs performance como fonte extra do planner ===
                // Merge (não 2ª passada) — spec 008 D3: o pack sobrepõe o manifesto principal por
                // path e o engine grava o hash efetivo no baseline, o que torna o OFF reversível.
                IReadOnlyList<ManifestFile> effectiveFiles = allFiles;
                SyncManifestOverlay performanceOverlay = null;

                if (LauncherSettingsProvider.Instance.UsePerformanceConfigs && performanceOverlayFiles.Count > 0)
                {
                    performanceOverlay = SyncManifestOverlay.Merge(allFiles, performanceOverlayFiles);
                    effectiveFiles = performanceOverlay.Files;
                    LogManager.Instance.Info($"[Profile] Configs performance ON — overlay com {performanceOverlayFiles.Count} arquivo(s) sobreposto ao manifesto");
                }

                var plannerOptions = new SyncPlannerOptions
                {
                    GameRoot = gamePath,
                    DevMode = devMode,
                    IgnoredFiles = ignoredFiles,
                    ExcludeFromCleanup = LauncherSettingsProvider.Instance.ExcludeFromCleanup ?? Array.Empty<string>(),
                    ProtectedPaths = OptionalModsHelper.GetAllKnownOptionalPaths()
                        .Select(p => p.Replace(Path.DirectorySeparatorChar, '/'))
                        .ToList(),
                    ManagedPaths = managedPaths,
                    IsOptionalGroupEnabled = id => LauncherSettingsProvider.Instance.IsOptionalEnabled(id),
                };

                var planner = new SyncPlanner(resolver, baseline, plannerOptions);
                var planProgress = new Progress<SyncProgress>(p =>
                {
                    UpdateMaxProgress = Math.Max(1, p.Total);
                    UpdateProgress = p.Current;
                    UpdateStatusText = string.Format(LocalizationProvider.Instance.update_checking_file, p.Current, p.Total) + " - " + p.CurrentPath;
                });

                var plan = await planner.BuildPlanAsync(effectiveFiles, planProgress, _syncCts.Token);

                OutdatedFiles = plan.DownloadCount;

                // === Execução (auto-apply, como o fluxo antigo) — atômica e cancelável ===
                var engine = BuildSyncEngine(gamePath, baseline, performanceOverlay);
                SyncResult result;

                if (plan.IoActionCount > 0)
                {
                    LogManager.Instance.Info($"[Profile] Plano: {plan.DownloadCount} downloads, {plan.SeedCount} seeds, {plan.DeleteCount} remoções, {plan.MoveCount} p/ -disabled, {plan.PreserveCount} preservados. Aplicando...");
                    UpdateMaxProgress = Math.Max(1, plan.IoActionCount);
                    UpdateProgress = 0;

                    var applyProgress = new Progress<SyncProgress>(p =>
                    {
                        UpdateProgress = p.Current;
                        UpdateStatusText = string.Format(LocalizationProvider.Instance.update_downloading, p.CurrentPath, p.Current, p.Total);
                    });

                    result = await engine.ExecuteAsync(plan, ReportFilePath, applyProgress, _syncCts.Token);
                }
                else
                {
                    // Nada a aplicar — ainda persiste o seed do baseline + report de preservados
                    result = await engine.ExecuteAsync(plan, ReportFilePath, null, CancellationToken.None);
                }

                foreach (var warning in result.Warnings)
                {
                    LogManager.Instance.Warning($"[Profile] {warning}");
                }

                SetLastUpdate(result.Updated);
                OutdatedFiles = 0;

                if (result.Cancelled)
                {
                    UpdateStatusText = $"Atualização cancelada — estado parcial gravado ({result.Pending} ações pendentes). Verifique novamente para completar.";
                    LogManager.Instance.Warning($"[Profile] Atualização cancelada com {result.Pending} ações pendentes");
                }
                else if (result.Errors > 0)
                {
                    UpdateStatusText = string.Format(LocalizationProvider.Instance.update_completed_with_errors, result.Updated, result.Errors);
                    LogManager.Instance.Warning($"[Profile] Atualização concluída com {result.Errors} erros: {result.Summary}");
                }
                else if (plan.IoActionCount > 0)
                {
                    UpdateStatusText = result.Summary;
                    LogManager.Instance.Info($"[Profile] Atualização concluída: {result.Summary}");
                }
                else
                {
                    UpdateStatusText = result.Preserved + result.PreservedDevMode > 0
                        ? $"{LocalizationProvider.Instance.update_up_to_date} ({result.Preserved + result.PreservedDevMode} preservados)"
                        : LocalizationProvider.Instance.update_up_to_date;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                    LogManager.Instance.Info("[Profile] Todos os mods estão atualizados.");
                }

                // Salvar manifest hash local (não salvar se cancelado — força rescan no próximo login)
                try
                {
                    if (!string.IsNullOrEmpty(serverManifestHash) && !result.Cancelled)
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
            catch (OperationCanceledException)
            {
                // Cancelado durante o planejamento (nada foi escrito)
                UpdateStatusText = "Verificação cancelada pelo usuário.";
                LogManager.Instance.Info("[Profile] Verificação cancelada pelo usuário");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Profile] Erro ao verificar atualizações: {ex.Message}");
                UpdateStatusText = string.Format(LocalizationProvider.Instance.update_error, ex.Message);
            }
            finally
            {
                CanCancelUpdate = false;
                DownloadSpeedText = ""; // item 016 — some com a taxa ao terminar a verificação
                _syncCts?.Dispose();
                _syncCts = null;

                if (!manifestFailed)
                    LauncherSettingsProvider.Instance.IsUpdating = false;
            }
        }

        // === Item 007 — helpers do motor de sync ===

        private static string SyncStateDir =>
            Path.Combine(SPT.Launcher.Base.Helpers.SptPathHelper.SptRootPath, "user", "launcher");

        private static string ReportFilePath => Path.Combine(SyncStateDir, SyncReport.DefaultFileName);

        private SyncEngine BuildSyncEngine(string gamePath, SyncBaseline baseline, SyncManifestOverlay performanceOverlay = null)
        {
            SyncDownloader downloader = (path, ct) => Task.Run(() => RequestHandler.DownloadModFile(path), ct);

            if (performanceOverlay != null)
            {
                // Item 008: paths do pack baixam do endpoint performance-download
                downloader = performanceOverlay.CreateDownloader(
                    downloader,
                    (path, ct) => Task.Run(() => RequestHandler.DownloadPerformanceFile(path), ct));
            }

            // Item 016 — camada MAIS EXTERNA: mede a taxa de todos os downloads (base/overlay/seed).
            downloader = WithSpeedMeter(downloader);

            return new SyncEngine(
                gamePath,
                baseline,
                downloader,
                deleteFile: RecycleBinHelper.Delete, // item 019 — fonte única de deleção recuperável
                log: msg => LogManager.Instance.Info(msg));
        }

        /// <summary>
        /// Item 016: envolve o downloader para medir bytes/tempo por arquivo e empurrar a taxa
        /// suavizada pra barra de update via UI thread (o delegate roda em thread de Task).
        /// </summary>
        private SyncDownloader WithSpeedMeter(SyncDownloader inner)
        {
            return async (path, ct) =>
            {
                var stopwatch = Stopwatch.StartNew();
                byte[] data = await inner(path, ct);
                stopwatch.Stop();

                _rateMeter.AddSample(data?.LongLength ?? 0, stopwatch.Elapsed);
                double bytesPerSec = _rateMeter.BytesPerSecond;
                string text = DownloadRateMeter.Format(bytesPerSec);

                Dispatcher.UIThread.Post(() =>
                {
                    DownloadBytesPerSec = bytesPerSec;
                    DownloadSpeedText = text;
                });

                return data;
            };
        }

        /// <summary>
        /// Requisito 4.1.2: cancelar com confirmação + alerta de consequência.
        /// O arquivo em voo termina atômico; o run para entre arquivos.
        /// </summary>
        private async Task CancelUpdate()
        {
            if (!CanCancelUpdate || _syncCts == null) return;

            var confirm = await ShowDialog(new ConfirmationDialogViewModel(null,
                "Cancelar a sincronização agora pode deixar a instalação em estado parcial. " +
                "Uma nova verificação completará o processo depois. Cancelar mesmo assim?",
                "Sim, cancelar", "Continuar"));

            if (confirm is not (bool and true)) return;

            try
            {
                _syncCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // o run terminou enquanto o dialog estava aberto — nada a cancelar
            }
        }

        /// <summary>Requisito 4.1.3: link "X arquivos foram atualizados" abre a pasta do relatório.</summary>
        public void OpenLastUpdateFolderCommand()
        {
            try
            {
                SyncReport.OpenReportFolder(SyncStateDir);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Profile] Falha ao abrir pasta do relatório: {ex.Message}");
            }
        }

        /// <summary>Carrega a contagem do last-update.json anterior (link persiste entre sessões).</summary>
        private void LoadLastUpdateInfo()
        {
            try
            {
                if (!File.Exists(ReportFilePath)) return;

                var report = JObject.Parse(File.ReadAllText(ReportFilePath));
                int updated = report["counts"]?["updated"]?.Value<int>() ?? 0;
                SetLastUpdate(updated);
            }
            catch
            {
                // report ausente/corrompido — sem link
            }
        }

        private void SetLastUpdate(int updatedCount)
        {
            LastUpdateText = $"{updatedCount} arquivo(s) foram atualizados — ver detalhes";
            HasLastUpdate = updatedCount > 0;
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

        /// <summary>
        /// Item 022 (Grupo B / RN-3, AC-022.8/9) — envelope guardado para os comandos async Task
        /// ligados por nome. O binding do Avalonia invoca o método sem await, então uma exceção
        /// após o 1º await viraria unobserved task exception (some sem log nem toast) e poderia
        /// deixar flags de UI presas. Aqui a exceção é logada, o <paramref name="onError"/> restaura
        /// o estado específico do comando (só StartGame mexe em GameRunning — CC-2/CC-3) e o usuário
        /// recebe uma notificação de erro. Nunca uma falha 100% silenciosa.
        /// </summary>
        private async Task GuardedAsync(Func<Task> body, string context, Action onError = null)
        {
            try
            {
                await body();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[Profile] {context}: {ex.Message}\n{ex.StackTrace}");
                onError?.Invoke();
                SendNotification("", "Ocorreu um erro ao executar a ação. Tente novamente.",
                    Avalonia.Controls.Notifications.NotificationType.Error);
            }
        }

        public async Task StartGameCommand() => await GuardedAsync(StartGameCore, "StartGame", onError: () =>
        {
            // CC-2/CC-3/AC-022.5/6/7: só StartGame gerencia GameRunning e a restauração é SÓ no catch —
            // o caminho feliz mantém GameRunning=true até o GameExitCallback (um finally cego zeraria o
            // jogo iniciado com sucesso). AllowSettings volta a true para destravar a tela de Settings.
            LauncherSettingsProvider.Instance.GameRunning = false;
            LauncherSettingsProvider.Instance.AllowSettings = true;
        });

        private async Task StartGameCore()
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

                // Item 020 (A4/AC-020.12) — o perfil antigo já foi removido; sem o re-register a chave
                // do cofre vira órfã apontando pra uma conta inexistente (e um futuro re-register do
                // mesmo username herdaria a senha antiga, BR-020.4). Limpar best-effort; a varredura de
                // órfãos do server reconcilia se esta falhar também.
                AccountStatus orphanCleanup = await AccountManager.DeleteVaultEntryAsync(username);
                if (orphanCleanup != AccountStatus.OK)
                {
                    LogManager.Instance.Warning($"[Profile] Não foi possível apagar a chave órfã do cofre para '{username}' ({orphanCleanup}) — será reconciliada no próximo delete/wipe.");
                }

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

        public async Task ChangeEditionCommand() => await GuardedAsync(ChangeEditionCore, "ChangeEdition");

        private async Task ChangeEditionCore()
        {
            var result = await ShowDialog(new ChangeEditionDialogViewModel(null));

            // CC-1: confirmação já é segura por positive-match (só prossegue com um SPTEdition).
            if(result != null && result is SPTEdition edition)
            {
                await WipeProfile(edition.Name);
            }
        }

        public async Task WipeConfirmCommand() => await GuardedAsync(WipeConfirmCore, "WipeConfirm");

        private async Task WipeConfirmCore()
        {
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(null,
                "Tem certeza que deseja resetar sua conta? Esta ação não pode ser revertida. Todo seu progresso será perdido."
                // Item 023 (Frente B / RN-4 / CA-B2): aviso de coop — o gate local não prova que
                // ninguém está em raid; wipe dispara Remove/Register no server.
                + "\n\nAtenção coop: se houver uma sessão Fika Coop PVE ativa, resetar agora pode corromper a raid dos outros jogadores. Confirme que ninguém está em raid.",
                isDestructive: true);

            var result = await ShowDialog(confirmation);

            // RN-1/AC-022.2: abortar no ambíguo — só o bool true explícito prossegue para o wipe.
            if (!DialogConfirmation.IsConfirmed(result)) return;

            await WipeProfile(AccountManager.SelectedAccount.edition);
        }

        /// <summary>
        /// Item 010 — exclusão definitiva da conta (≠ wipe): confirmação forte
        /// digitando o username, remove no server, limpa auto-login e volta ao login.
        /// </summary>
        public async Task DeleteAccountCommand() => await GuardedAsync(DeleteAccountCore, "DeleteAccount");

        private async Task DeleteAccountCore()
        {
            string username = AccountManager.SelectedAccount.username;

            var dialog = new DeleteAccountDialogViewModel(null, username);
            var result = await ShowDialog(dialog);

            // RN-1: abortar no ambíguo — mesma regra pura das demais confirmações destrutivas.
            if (!DialogConfirmation.IsConfirmed(result)) return;

            LogManager.Instance.Info($"[Profile] Excluindo conta '{username}'...");

            // Item 020 (BR-020.2/BR-020.3, AC-020.5/AC-020.6) — ORDEM SEGURA: remove no server
            // (fonte de verdade) PRIMEIRO; só no sucesso limpa o cofre. Corrige o defeito herdado:
            // antes o cofre era zerado ANTES do remove, então um remove que falhasse (ex.:
            // NoConnection) deixava a conta viva com senha VAZIA no cofre (takeover livre). Além
            // disso a limpeza APAGA a chave (DeleteVaultEntry), não grava senha vazia — senha vazia
            // == gate aberto e indistinguível de "conta sem senha".
            AccountStatus status = await AccountManager.RemoveAsync();

            switch (status)
            {
                case AccountStatus.OK:
                    {
                        // Só agora que a conta não existe mais no server: apagar a chave do cofre.
                        // Best-effort — a falha aqui não ressuscita a conta (já removida); a varredura
                        // de órfãos do server reconcilia (A4). NUNCA grava senha vazia (BR-020.3).
                        AccountStatus vaultStatus = await AccountManager.DeleteVaultEntryAsync(username);
                        if (vaultStatus != AccountStatus.OK)
                        {
                            LogManager.Instance.Warning($"[Profile] Conta '{username}' removida, mas a chave do cofre não pôde ser apagada agora ({vaultStatus}) — será reconciliada como órfã no próximo delete/wipe.");
                        }

                        // Sem isso o auto-login tentaria uma conta que não existe mais
                        LauncherSettingsProvider.Instance.Server.AutoLoginCreds = null;

                        // ref: CR-01-02 — RememberUsername não pode ressuscitar credenciais
                        // da conta morta pré-preenchendo a LoginView
                        LauncherSettingsProvider.Instance.LastUsername = "";
                        LauncherSettingsProvider.Instance.LastPassword = "";

                        LauncherSettingsProvider.Instance.SaveSettings();

                        AccountManager.Logout(); // idempotente — Remove() já anulou a conta

                        LogManager.Instance.Info($"[Profile] Conta '{username}' excluída.");
                        SendNotification("", $"Conta '{username}' excluída definitivamente.",
                            Avalonia.Controls.Notifications.NotificationType.Success);

                        NavigateTo(new LoginViewModel(HostScreen, true));
                        break;
                    }
                case AccountStatus.NoConnection:
                    {
                        LogManager.Instance.Error($"[Profile] Falha ao excluir conta '{username}': sem conexão.");
                        SendNotification("", "Sem conexão com o servidor — a conta não foi excluída.",
                            Avalonia.Controls.Notifications.NotificationType.Error);

                        NavigateTo(new ConnectServerViewModel(HostScreen));
                        break;
                    }
                default:
                    {
                        LogManager.Instance.Error($"[Profile] Falha ao excluir conta '{username}': {status}.");
                        SendNotification("", "Falha ao excluir a conta no servidor. Tente novamente.",
                            Avalonia.Controls.Notifications.NotificationType.Error);
                        break;
                    }
            }
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

        public async Task RemoveProfileCommand() => await GuardedAsync(RemoveProfileCore, "RemoveProfile");

        private async Task RemoveProfileCore()
        {
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(null, string.Format(LocalizationProvider.Instance.profile_remove_question_format_1, AccountManager.SelectedAccount.username));

            var result = await ShowDialog(confirmation);

            // RN-1/AC-022.3: abortar no ambíguo — só o bool true explícito prossegue para o RemoveAsync.
            if (!DialogConfirmation.IsConfirmed(result)) return;

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
