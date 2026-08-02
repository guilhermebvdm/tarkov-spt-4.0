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
    public class ProfileViewModel : ViewModelBase, ILauncherHome
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

        // Item 030: a lista de opcionais saiu daqui (era um painel na tela logada); agora vive na tela
        // "Mods e Configs". A tela logada mostra só o RESUMO (ModsConfigsSummary).

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
        // Item 031: run atual — guard contra um Progress<T> de um run anterior sobrescrever a msg final.
        private int _syncRunId;
        // Item 032: ticker que atualiza a taxa na UI em cadência fixa (~500ms), desacoplado dos chunks.
        private DispatcherTimer _speedTicker;

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

                // CR-03-04: o resumo "Mods e Configs" é materializado por string.Format (não é binding
                // reativo), então não reage sozinho à troca de idioma. Re-renderiza quando o locale muda
                // com o Profile ativo; o handler estático é removido ao desativar (evita leak).
                EventHandler onLocaleChanged = (_, _) =>
                    Dispatcher.UIThread.Post(RefreshModsConfigsSummary);
                LocalizationProvider.LocaleChanged += onLocaleChanged;
                Disposable.Create(() => LocalizationProvider.LocaleChanged -= onLocaleChanged).DisposeWith(disposables);

                // Item 030 (PA-01-03/§5.6): ao voltar da tela "Mods e Configs", se há intenção pendente
                // e o jogo está fechado (CA-030.23), dispara o sync — a tela NÃO sincroniza, quem aplica
                // é aqui, com o guard de concorrência existente. Se um sync já roda, a pendência persiste.
                RefreshModsConfigsSummary();
                if (LauncherSettingsProvider.Instance.PendingApply.Count > 0 && CanStartGame && !IsSyncRunning)
                {
                    _ = CheckForUpdates();
                }
            });

            // Auto-check for updates, depois aplica opcionais pendentes
            _ = InitializeAsync();
        }

        // === Item 030: resumo "Mods e Configs" na tela logada + navegação ===

        private string _modsConfigsSummary = "";
        public string ModsConfigsSummary
        {
            get => _modsConfigsSummary;
            set => this.RaiseAndSetIfChanged(ref _modsConfigsSummary, value);
        }

        private bool _hasModsConfigsNew;
        public bool HasModsConfigsNew
        {
            get => _hasModsConfigsNew;
            set => this.RaiseAndSetIfChanged(ref _hasModsConfigsNew, value);
        }

        public bool HasModsConfigsSummary => !string.IsNullOrEmpty(ModsConfigsSummary);

        /// <summary>
        /// Item 030 (CA-030.13 / §4.1): resumo a partir do catálogo + preferências. Nunca "0 de 0" por
        /// ausência de dado — sem itens conhecidos, o resumo fica oculto (string vazia).
        /// </summary>
        private void RefreshModsConfigsSummary()
        {
            var settings = LauncherSettingsProvider.Instance;
            var mods = ModsConfigCatalog.OptionalMods;
            var perf = ModsConfigCatalog.OptionalConfigs;

            if (mods.Count == 0 && perf.Count == 0)
            {
                ModsConfigsSummary = "";
                HasModsConfigsNew = false;
                this.RaisePropertyChanged(nameof(HasModsConfigsSummary));
                return;
            }

            int modsOn = mods.Count(m => settings.IsOptionalEnabled(m.Id));
            int perfOn = perf.Count(x => settings.IsOptionalConfigEnabled(x.Id));
            ModsConfigsSummary = string.Format(LocalizationProvider.Instance.mods_configs_summary_format,
                modsOn, mods.Count, perfOn, perf.Count);
            HasModsConfigsNew = mods.Concat(perf).Any(i => !settings.SeenItemIds.Contains(i.Id));
            this.RaisePropertyChanged(nameof(HasModsConfigsSummary));
        }

        /// <summary>Item 030: abre a tela "Mods e Configs" (resumo clicável / item do menu lateral).</summary>
        public void OpenModsConfigsCommand()
        {
            LauncherSettingsProvider.Instance.AllowSettings = false;
            NavigateMenu(new ModsConfigsViewModel(HostScreen));
        }

        /// <summary>
        /// Item 030 (CA-030.16/16b/CC-14): dispara o onboarding? Sim quando NÃO concluído (a marca é a
        /// fonte de verdade, D-17 — não repete se o plugins esvaziar depois) E o cliente está sem plugins
        /// (pasta inexistente ou sem nenhum .dll em qualquer profundidade). Dev Mode não dispara (CC-14).
        /// </summary>
        // Item 033: onboarding UNIVERSAL — dispara para todos na 1ª vez (novo jogador E quem atualiza,
        // qualquer versão, inclusive em Dev Mode), uma vez só. A fonte de verdade é o flag persistente;
        // o estado do disco (plugins) e o Dev Mode não decidem mais (CA-033.6/CA-033.7).
        private static bool ShouldTriggerOnboarding()
        {
            return !LauncherSettingsProvider.Instance.ModsConfigsOnboardingDone;
        }

        public bool CanStartGame => LauncherSettingsProvider.Instance.CanStartGame;

        private async Task InitializeAsync()
        {
            // Item 013: versão do server agora vem do endpoint /redline/server/version
            // (populada no connect via ServerManager) — o read do config.json do
            // TarkovRedLine-ServerMod foi removido por ser fonte local defasável.
            await CheckForUpdates();
        }

        public void OpenModsInfoCommand() =>
            NavigateTo(new ModInfoViewModel(HostScreen, ModInfoCollection));

        public void OpenSettingsCommand()
        {
            // Esconde o gear da topbar enquanto está em Configurações (senão abriria outra por cima).
            LauncherSettingsProvider.Instance.AllowSettings = false;
            NavigateMenu(new SettingsViewModel(HostScreen));
        }

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
                // Item 033: NÃO faz early-return de Dev Mode aqui — o manifesto precisa ser buscado para
                // popular o catálogo + semear (Mec.1) + disparar o onboarding universal (Mec.2). O skip do
                // Dev Mode (não mover/aplicar arquivos) foi movido para depois disso, antes do planner.
                LauncherSettingsProvider.Instance.IsUpdating = true;
                IsUpdateVisible = true;
                UpdateStatusText = LocalizationProvider.Instance.update_checking;
                UpdateProgress = 0;
                _rateMeter.Reset();      // item 016 — taxa limpa a cada verificação
                DownloadSpeedText = "";
                // Item 031: reset ÚNICO — o placar/link do run ANTERIOR não pode sobrar na tela (D-031.3).
                LastUpdateText = "";
                HasLastUpdate = false;
                int myRun = ++_syncRunId;   // Item 031: run atual; reports de runs anteriores são ignorados.
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
                    UpdateStatusText = LocalizationProvider.Instance.update_generating_list;
                    await Task.Delay(3000);
                    response = null;
                }

                if (string.IsNullOrEmpty(response))
                {
                    // Item 033: em Dev Mode auto-check, se o servidor não respondeu, desiste sem o countdown
                    // de 30s (preserva o "login rápido" que motivava o skip do Dev Mode). O onboarding/seed
                    // rodam no próximo login/verificação com o servidor de pé.
                    if (devMode && !manual)
                    {
                        LogManager.Instance.Info("[Profile] DevMode: manifesto indisponível — pulando este ciclo (sem retry longo).");
                        LauncherSettingsProvider.Instance.IsUpdating = false;
                        IsUpdateVisible = false;
                        return;
                    }

                    LogManager.Instance.Warning("[Profile] Manifesto não disponível após 5 tentativas. Reagendando em 30s...");
                    manifestFailed = true;

                    // Countdown de 30 segundos com retry automático
                    for (int s = 30; s > 0; s--)
                    {
                        UpdateStatusText = string.Format(LocalizationProvider.Instance.update_preparing_retry_countdown, s);
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

                // Item 030: catálogo dos itens da tela "Mods e Configs" (mods opcionais + configs de
                // performance). O resumo na tela logada e a própria tela leem daqui. Substitui o modelo
                // antigo (optionalGroups/performanceOverlay + toggles inline).
                ModsConfigCatalog.UpdateFromManifest(manifest["optionalMods"], manifest["optionalConfigs"], manifest["optionalModCategories"], manifest["optionalConfigCategories"]);
                Dispatcher.UIThread.Post(RefreshModsConfigsSummary);

                // Item 033: seed do disco — define o estado inicial dos mods opcionais NÃO-decididos ANTES do
                // planner (CC-7), para que um mod que o jogador já usa não seja quarentenado por nascer
                // desligado (D-6). Jogador com plugins → liga o instalado; sem plugins → liga as categorias.
                var seededDefaults = OptionalModSeeder.ComputeSeed(
                    allFiles, gamePath,
                    new HashSet<string>(LauncherSettingsProvider.Instance.EnabledOptionals.Keys),
                    ModsConfigCatalog.OptionalMods.ToDictionary(m => m.Id, m => m.Category));
                LauncherSettingsProvider.Instance.SeedOptionalDefaults(seededDefaults);

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
                        // R3.4: pastas "-disabled" são quarentena/backup intocável (inclui o backup do
                        // config-force em config-disabled/). deleteFiles roda FORA do motor, então
                        // precisa do MESMO guard do ScanExtras — senão um manifesto poderia apagar o
                        // backup e furar a garantia "nada excluído misteriosamente". Limpeza de
                        // quarentena é manual, por design.
                        if (SyncPathUtil.ContainsDisabledSegment(SyncPathUtil.Normalize(deleteFile)))
                        {
                            LogManager.Instance.Warning($"[Profile] deleteFiles ignorado (pasta -disabled, quarentena protegida): {deleteFile}");
                            continue;
                        }

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

                // Item 030: o canal config-optional agora é regra de pasta (optional-config-to-config),
                // não overlay de manifesto — o SyncManifestOverlay foi aposentado (D-13). Os discriminadores
                // vêm das preferências: mods opcionais e itens de performance ligados, mais os ids que o
                // player acabou de alternar (PendingApply) — para estes a aplicação é explícita.
                IReadOnlyList<ManifestFile> effectiveFiles = allFiles;
                var justToggled = LauncherSettingsProvider.Instance.PendingApply
                    .Where(id => id != SyncTriggers.PendingApplyMarker)
                    .ToList();

                var plannerOptions = new SyncPlannerOptions
                {
                    GameRoot = gamePath,
                    DevMode = devMode,
                    IgnoredFiles = ignoredFiles,
                    ExcludeFromCleanup = LauncherSettingsProvider.Instance.ExcludeFromCleanup ?? Array.Empty<string>(),
                    ManagedPaths = managedPaths,
                    IsOptionalModEnabled = id => LauncherSettingsProvider.Instance.IsOptionalEnabled(id),
                    IsOptionalConfigEnabled = id => LauncherSettingsProvider.Instance.IsOptionalConfigEnabled(id),
                    JustToggledIds = justToggled,
                };

                // Item 030 (CA-030.16 / R-10): onboarding do primeiro acesso. Cliente sem plugins e
                // onboarding não concluído vai direto pra tela "Mods e Configs" ANTES de aplicar — o
                // catálogo já foi populado acima. Ao sair da tela, a primeira ingestão roda com as
                // escolhas dele (a tela dispara via PendingApply). Só em full scan (não no skipFileScan).
                if (ShouldTriggerOnboarding())
                {
                    LogManager.Instance.Info("[Profile] Onboarding (item 033): 1º acesso — abrindo tela Mods e Configs antes do primeiro sync");
                    Dispatcher.UIThread.Post(() => NavigateTo(new ModsConfigsViewModel(HostScreen, onboarding: true)));
                    return; // não aplica agora; o apply vem quando o player sair da tela (CA-030.19)
                }

                // Item 033: skip do Dev Mode MOVIDO para cá — catálogo + seed + gatilho de onboarding já
                // rodaram; o Dev Mode continua NÃO movendo/aplicando arquivos (preserva builds locais do dev).
                if (devMode && !manual)
                {
                    LogManager.Instance.Info("[Profile] DevMode ativo — catálogo/seed/onboarding OK; pulando a aplicação do sync.");
                    LauncherSettingsProvider.Instance.IsUpdating = false;
                    IsUpdateVisible = false;
                    return;
                }

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
                var engine = BuildSyncEngine(gamePath, baseline);
                SyncResult result;

                if (plan.IoActionCount > 0)
                {
                    LogManager.Instance.Info($"[Profile] Plano: {plan.DownloadCount} downloads, {plan.SeedCount} seeds, {plan.DeleteCount} remoções, {plan.MoveCount} p/ -disabled, {plan.PreserveCount} preservados. Aplicando...");
                    UpdateMaxProgress = Math.Max(1, plan.IoActionCount);
                    UpdateProgress = 0;

                    var applyProgress = new Progress<SyncProgress>(p =>
                    {
                        if (myRun != _syncRunId) return;   // Item 031: report de run já encerrado → ignora (defensivo)
                        UpdateProgress = p.Current;
                        UpdateStatusText = SyncMessages.ProgressText(p.Kind, p.CurrentPath, p.Current, p.Total);
                    });

                    StartSpeedTicker();  // Item 032: taxa atualiza em cadência fixa durante o apply
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

                _syncRunId++;               // Item 031: invalida qualquer report de progresso ainda em voo
                SetLastUpdate(result);      // Item 031: link por TOTAL de ações, não só Updated
                OutdatedFiles = 0;

                if (result.Cancelled)
                {
                    UpdateStatusText = string.Format(LocalizationProvider.Instance.update_cancelled_partial_state, result.Pending);
                    LogManager.Instance.Warning($"[Profile] Atualização cancelada com {result.Pending} ações pendentes");
                }
                else if (result.Errors > 0)
                {
                    UpdateStatusText = string.Format(LocalizationProvider.Instance.update_completed_with_errors, result.Updated, result.Errors);
                    LogManager.Instance.Warning($"[Profile] Atualização concluída com {result.Errors} erros: {result.Summary}");
                }
                else if (plan.IoActionCount > 0)
                {
                    UpdateStatusText = SyncMessages.BuildSummary(result);   // Item 031: i18n, não result.Summary (PT)
                    LogManager.Instance.Info($"[Profile] Atualização concluída: {result.Summary}");  // log interno (PT ok)
                }
                else
                {
                    UpdateStatusText = result.Preserved + result.PreservedDevMode > 0
                        ? $"{LocalizationProvider.Instance.update_up_to_date} {string.Format(LocalizationProvider.Instance.update_up_to_date_preserved_suffix, result.Preserved + result.PreservedDevMode)}"
                        : LocalizationProvider.Instance.update_up_to_date;
                    UpdateMaxProgress = 1;
                    UpdateProgress = 1;
                    LogManager.Instance.Info("[Profile] Todos os mods estão atualizados.");
                }

                // Item 030 (PA-01-05 + 🟡 CR): a intenção pendente só é limpa quando o sync conclui SEM erro
                // e sem cancelamento. Remove APENAS o snapshot que ESTE sync aplicou (justToggled + marker),
                // não Clear() — um toggle registrado por outra tela DURANTE este sync (que ficou pendente
                // porque o guard de concorrência o pulou) não pode ser descartado sem ter sido aplicado.
                if (!result.Cancelled && result.Errors == 0)
                {
                    var pending = LauncherSettingsProvider.Instance.PendingApply;
                    int before = pending.Count;
                    foreach (var id in justToggled) pending.Remove(id);
                    pending.RemoveAll(x => x == SyncTriggers.PendingApplyMarker);
                    if (pending.Count != before) LauncherSettingsProvider.Instance.SaveSettings();
                }
                Dispatcher.UIThread.Post(RefreshModsConfigsSummary);

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
                UpdateStatusText = LocalizationProvider.Instance.update_check_cancelled_by_user;
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
                StopSpeedTicker();      // item 032 — para o ticker da taxa (sucesso/erro/cancelamento)
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

        private SyncEngine BuildSyncEngine(string gamePath, SyncBaseline baseline)
        {
            // Item 030: um downloader só — o canal config-optional virou regra de pasta (as duas
            // entradas lógicas config-optional/ e config-optional-ref/ baixam do /download comum,
            // servidas pelo _fileMapCache do servidor). O overlay/performance-download foi aposentado.
            // Item 032: o downloader alimenta o meter POR CHUNK (medição intra-arquivo). O ticker
            // (StartSpeedTicker) lê o meter em cadência fixa e atualiza a UI — não medimos mais por arquivo.
            SyncDownloader downloader = (path, ct) => Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                long last = 0;
                return RequestHandler.DownloadModFile(path, 30000, onProgress: total =>
                {
                    long delta = total - last;
                    last = total;
                    _rateMeter.AddSample(delta, sw.Elapsed); // (bytes do chunk, tempo do chunk)
                    sw.Restart();
                });
            }, ct);

            return new SyncEngine(
                gamePath,
                baseline,
                downloader,
                deleteFile: RecycleBinHelper.Delete, // item 019 — fonte única de deleção recuperável
                log: msg => LogManager.Instance.Info(msg));
        }

        // === Item 032: ticker da taxa — lê o meter em cadência fixa (~500ms), desacoplado dos chunks ===

        private void StartSpeedTicker()
        {
            _speedTicker ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _speedTicker.Tick -= OnSpeedTick;   // evita handler duplicado em runs repetidos
            _speedTicker.Tick += OnSpeedTick;
            _speedTicker.Start();
        }

        private void OnSpeedTick(object sender, EventArgs e)
        {
            var (has, text) = _rateMeter.Snapshot(); // leitura ATÔMICA (CR-01-01)
            DownloadSpeedText = has ? text : "";
        }

        private void StopSpeedTicker() => _speedTicker?.Stop();

        /// <summary>
        /// Requisito 4.1.2: cancelar com confirmação + alerta de consequência.
        /// O arquivo em voo termina atômico; o run para entre arquivos.
        /// </summary>
        private async Task CancelUpdate()
        {
            if (!CanCancelUpdate || _syncCts == null) return;

            var confirm = await ShowDialog(new ConfirmationDialogViewModel(null,
                LocalizationProvider.Instance.sync_cancel_confirm_question,
                LocalizationProvider.Instance.sync_cancel_confirm_yes, LocalizationProvider.Instance.sync_cancel_confirm_no));

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
                var c = report["counts"];
                int Count(string key) => c?[key]?.Value<int>() ?? 0;
                // Item 031: total de ações relevantes (mesmo critério do run atual, com os counts do JSON).
                SetLastUpdateTotal(Count("updated") + Count("movedToDisabled") + Count("deleted")
                                   + Count("forced") + Count("seeded"));
            }
            catch
            {
                // report ausente/corrompido — sem link
            }
        }

        // Item 031: o link "ver detalhes" liga por TOTAL de ações relevantes (não só downloads),
        // senão some justamente num run que só moveu/removeu arquivos (Updated=0 — CA-031.6).
        private void SetLastUpdate(SyncResult r)
            => SetLastUpdateTotal(r.Updated + r.MovedToDisabled + r.Deleted + r.Forced + r.Seeded + r.OptionalConfigApplied);

        private void SetLastUpdateTotal(int total)
        {
            LastUpdateText = string.Format(LocalizationProvider.Instance.last_update_files_updated, total);
            HasLastUpdate = total > 0;
            if (total > 0) IsUpdateVisible = true;   // Item 031: a área com o resumo+link fica visível (incl. no load)
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
                SendNotification("", LocalizationProvider.Instance.action_generic_error,
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
                    SendNotification("", LocalizationProvider.Instance.wipe_remove_old_profile_error);
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
                    SendNotification("", LocalizationProvider.Instance.wipe_create_new_profile_error);
                return registerStatus;
            }

            LogManager.Instance.Info("[Profile] Novo perfil criado com sucesso.");

            // 3. Atualizar UI
            LogManager.Instance.Info("[Profile] Etapa 3/3: Atualizando interface...");
            CurrentEdition = AccountManager.SelectedAccount.edition;
            UpdateProfileInfo();
            SendNotification("", string.Format(LocalizationProvider.Instance.wipe_success, edition));

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
            // Item 023 (Frente B / RN-4 / CA-B2): aviso de coop — o gate local não prova que
            // ninguém está em raid; wipe dispara Remove/Register no server.
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(null,
                LocalizationProvider.Instance.wipe_confirm_question,
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
                        SendNotification("", string.Format(LocalizationProvider.Instance.account_deleted_success, username),
                            Avalonia.Controls.Notifications.NotificationType.Success);

                        NavigateTo(new LoginViewModel(HostScreen, true));
                        break;
                    }
                case AccountStatus.NoConnection:
                    {
                        LogManager.Instance.Error($"[Profile] Falha ao excluir conta '{username}': sem conexão.");
                        SendNotification("", LocalizationProvider.Instance.account_delete_no_connection,
                            Avalonia.Controls.Notifications.NotificationType.Error);

                        NavigateTo(new ConnectServerViewModel(HostScreen));
                        break;
                    }
                default:
                    {
                        LogManager.Instance.Error($"[Profile] Falha ao excluir conta '{username}': {status}.");
                        SendNotification("", LocalizationProvider.Instance.account_delete_failed,
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
