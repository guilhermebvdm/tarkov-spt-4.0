using SPT.Launcher.Attributes;
using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Models.TRL;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Controls.Notifications;

namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// UI item for one selectable class. Built from GET /customclasses/classes
    /// (SP0 contract, CustomClasses item 058) or from the vanilla editions fallback.
    /// </summary>
    public class ClassProfile
    {
        /// <summary>EXACT edition key expected by /launcher/profile/register.</summary>
        public string EditionKey { get; set; }

        /// <summary>Display name (pt -> en -> editionKey).</summary>
        public string Name { get; set; }

        /// <summary>Uppercase name for TrlScreenBar (consumer provides uppercase).</summary>
        public string NameUpper => Name?.ToUpperInvariant() ?? string.Empty;

        /// <summary>Description (pt -> en -> vanilla profileDescriptions -> empty).</summary>
        public string Description { get; set; }

        /// <summary>Local cached icon path (null = no icon).</summary>
        public string IconPath { get; set; }

        public bool HasIcon => !string.IsNullOrEmpty(IconPath);

        /// <summary>Bundled local icon (Assets/ClassIcons) resolved by name — only when the
        /// server did not provide an icon. Null when there is no name match.</summary>
        public Bitmap FallbackIcon { get; set; }

        public bool HasFallbackIcon => FallbackIcon != null;

        /// <summary>Parsed nameColor brush; null when absent/invalid (trl-nav default foreground applies).</summary>
        public IBrush NameBrush { get; set; }

        public bool HasNameColor => NameBrush != null;

        /// <summary>Kept for future use (kickoff 004: no render yet).</summary>
        public Dictionary<string, int> Skills { get; set; }

        /// <summary>Kept for future use (kickoff 004: no render yet).</summary>
        public Dictionary<string, double> SkillMultipliers { get; set; }
    }

    [RequireServerConnected]
    public class ClassSelectionViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private bool _loadStarted = false;

        public ObservableCollection<ClassProfile> AvailableClasses { get; set; } = new ObservableCollection<ClassProfile>();

        private ClassProfile _selectedClass;
        public ClassProfile SelectedClass
        {
            get => _selectedClass;
            set => this.RaiseAndSetIfChanged(ref _selectedClass, value);
        }

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private string _registerErrorMsg;
        public string RegisterErrorMsg
        {
            get => _registerErrorMsg;
            set => this.RaiseAndSetIfChanged(ref _registerErrorMsg, value);
        }

        public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; set; }
        public ReactiveCommand<Unit, Unit> FinalizeAccountCommand { get; set; }

        public ClassSelectionViewModel(IScreen Host, string username, string password) : base(Host)
        {
            _username = username;
            _password = password;

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                if (_loadStarted) return;
                _loadStarted = true;

                Task.Run(async () =>
                {
                    await LoadClassesAsync();
                });
            });

            GoToRegisterCommand = ReactiveCommand.Create(() =>
            {
                NavigateTo(new RegisterViewModel(HostScreen, _username));
            });

            FinalizeAccountCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                // ref: CR-01-02 — guard ANTES de limpar a mensagem: clique no estado vazio
                // não pode apagar a única orientação visível da tela.
                if (SelectedClass == null) return;

                RegisterErrorMsg = "";

                // Item 020 (DP-020.A = A2, BR-020.1, AC-020.4) — BLOQUEAR REGISTRO COLIDENTE.
                // Um username que colida case-insensitive com um perfil já existente (Bob vs bob)
                // colapsaria na mesma chave lowercase do cofre (redline_passwords.json) e uma senha
                // gravaria no perfil errado. Recusar aqui torna a colisão impossível na origem, sem
                // depender de o core casar case-sensitive. O critério é o único ponto VaultKeyMatcher.
                // (O wipe reusa o MESMO username via WipeProfile, não passa por este comando — imune.)
                try
                {
                    var existingProfiles = await Task.Run(() => AccountManager.GetExistingProfiles());
                    var existingUsernames = new List<string>();
                    if (existingProfiles != null)
                    {
                        foreach (var profile in existingProfiles)
                        {
                            existingUsernames.Add(profile?.username);
                        }
                    }

                    if (VaultKeyMatcher.CollidesWith(existingUsernames, _username))
                    {
                        RegisterErrorMsg = "Já existe uma conta com esse nome (ou uma variação de maiúsculas/minúsculas). Escolha outro nome.";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Fail-closed: sem conseguir listar os perfis, não arriscar criar uma colisão.
                    LogManager.Instance.Error($"[ClassSelection] Falha ao verificar colisão de username: {ex.Message}");
                    RegisterErrorMsg = "Não foi possível verificar o nome de usuário no servidor. Tente novamente.";
                    return;
                }

                // A edition enviada é a chave EXATA registrada no ProfileTemplates (contrato SP0),
                // nunca o displayName exibido na UI.
                string editionToUse = SelectedClass.EditionKey;

                AccountStatus registerResult = await AccountManager.RegisterAsync(_username, _password, editionToUse);

                if (registerResult == AccountStatus.OK)
                {
                    // ref: 005-D1 — o core cria a conta SEM senha (a senha do register é descartada
                    // pelo /launcher/profile/register). Semear o cofre com a senha digitada antes do
                    // auto-login; falha => segue o fluxo (usuário define no dialog do próximo login).
                    if (!string.IsNullOrEmpty(_password))
                    {
                        AccountStatus passwordResult = await AccountManager.ChangePasswordAsync(_password);

                        if (passwordResult != AccountStatus.OK)
                        {
                            LogManager.Instance.Warning($"[ClassSelection] Failed to set initial password ({passwordResult}) — user will be prompted on next login");
                            SendNotification("Senha", "Não foi possível salvar sua senha agora — você poderá defini-la no próximo login.", NotificationType.Warning);
                        }
                    }

                    SendNotification(LocalizationProvider.Instance.profile_created, _username, NotificationType.Success);

                    // Auto-Login e ir pro Profile
                    var loginModel = new LoginModel { Username = _username, Password = _password };
                    AccountStatus loginStatus = await AccountManager.LoginAsync(loginModel);

                    if (loginStatus == AccountStatus.OK)
                    {
                        NavigateTo(new ProfileViewModel(HostScreen));
                    }
                    else
                    {
                        NavigateTo(new LoginViewModel(HostScreen));
                    }
                }
                else
                {
                    RegisterErrorMsg = "Erro ao criar conta: " + registerResult.ToString();
                }
            });
        }

        /// <summary>
        /// Loads the class list from GET /customclasses/classes (zlib handled by Request.GetJson).
        /// Any failure or empty result degrades to the vanilla editions fallback — never crashes.
        /// Runs on a background thread; UI collections are mutated via the UI dispatcher.
        /// </summary>
        private async Task LoadClassesAsync()
        {
            // ref: CR-01-01 — try/catch de última instância + finally: o Post final NUNCA é pulado.
            // O chamador é Task.Run fire-and-forget (exceção seria engolida em silêncio); qualquer
            // falha inesperada fora do caminho feliz (ex.: IOException do LogManager sob escrita
            // concorrente) não pode deixar "Carregando classes..." eterno sem fallback.
            List<ClassProfile> classes = null;

            try
            {
                try
                {
                    // ref: CR-01-08.1 — chamada direta (sem Task.Run aninhado): este método já roda
                    // em thread de fundo (WhenActivated → Task.Run).
                    string json = RequestHandler.RequestClassList();
                    List<ClassInfo> serverClasses = Json.Deserialize<List<ClassInfo>>(json);

                    if (serverClasses != null && serverClasses.Count > 0)
                    {
                        classes = await BuildFromServerAsync(serverClasses);
                    }
                    else
                    {
                        LogManager.Instance.Warning("[ClassSelection] /customclasses/classes returned null/empty list");
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[ClassSelection] Failed to load /customclasses/classes: {ex.GetType().Name}: {ex.Message}");
                }

                if (classes == null || classes.Count == 0)
                {
                    LogManager.Instance.Warning("[ClassSelection] Falling back to vanilla editions + profileDescriptions");
                    classes = BuildFromEditionsFallback();
                }
            }
            catch (Exception ex)
            {
                // Última instância: nem o próprio log pode derrubar o load.
                try { LogManager.Instance.Error($"[ClassSelection] Unexpected failure loading classes: {ex}"); } catch { /* sem canal de log confiável aqui */ }
            }
            finally
            {
                List<ClassProfile> publishable = classes ?? new List<ClassProfile>();

                // Fallback local: classes sem ícone do server tentam um ícone bundlado por nome
                // (Assets/ClassIcons). Sem match (ex.: "SPT Developer", "Tanque") → segue sem ícone.
                // Decode fora da UI thread aqui (Bitmap não é controle) — visibilidade garantida
                // pelo Post subsequente.
                foreach (ClassProfile profile in publishable)
                {
                    if (!profile.HasIcon && profile.FallbackIcon == null)
                    {
                        profile.FallbackIcon = ResolveBundledIcon(profile.Name);
                    }
                }

                Dispatcher.UIThread.Post(() =>
                {
                    AvailableClasses.Clear();

                    foreach (ClassProfile profile in publishable)
                    {
                        AvailableClasses.Add(profile);
                    }

                    SelectedClass = AvailableClasses.Count > 0 ? AvailableClasses[0] : null;
                    IsLoading = false;

                    if (AvailableClasses.Count == 0)
                    {
                        RegisterErrorMsg = "Nenhuma classe disponível. Verifique a conexão com o servidor e tente novamente.";
                    }
                });
            }
        }

        /// <summary>Maps the SP0 payload to UI items. Dedupes by editionKey defensively (P-058.4).</summary>
        private async Task<List<ClassProfile>> BuildFromServerAsync(List<ClassInfo> serverClasses)
        {
            var result = new List<ClassProfile>();
            var iconTasks = new List<Task>();
            var seenKeys = new HashSet<string>(StringComparer.Ordinal);
            Dictionary<string, string> vanillaDescriptions = ServerManager.SelectedServer?.profileDescriptions;

            foreach (ClassInfo info in serverClasses)
            {
                if (info == null || string.IsNullOrWhiteSpace(info.EditionKey))
                {
                    LogManager.Instance.Warning("[ClassSelection] Skipping class entry without editionKey");
                    continue;
                }

                if (!seenKeys.Add(info.EditionKey))
                {
                    LogManager.Instance.Warning($"[ClassSelection] Duplicated editionKey '{info.EditionKey}' — keeping first occurrence");
                    continue;
                }

                string description = FirstNonEmpty(info.Description?.Pt, info.Description?.En);

                if (string.IsNullOrEmpty(description) && vanillaDescriptions != null && vanillaDescriptions.TryGetValue(info.EditionKey, out string vanillaDescription))
                {
                    description = vanillaDescription;
                }

                ClassProfile profile = new ClassProfile
                {
                    EditionKey = info.EditionKey,
                    Name = FirstNonEmpty(info.DisplayName?.Pt, info.DisplayName?.En) ?? info.EditionKey,
                    Description = description ?? string.Empty,
                    NameBrush = ParseNameColor(info.NameColor),
                    Skills = info.Skills,
                    SkillMultipliers = info.SkillMultipliers
                };

                result.Add(profile);

                // ref: CR-01-03 — ícones em PARALELO (Task.WhenAll): pior caso colapsa de ~7×15 s em
                // série para ~1 timeout (15 s). Opção de menor risco vs publicar-antes-e-preencher:
                // não exige ClassProfile reativo nem Post por item (perfis ainda não estão bound à UI
                // aqui — escrever IconPath em threads do pool antes do publish é seguro; o await do
                // WhenAll estabelece a visibilidade de memória antes do Post).
                if (!string.IsNullOrWhiteSpace(info.IconUrl))
                {
                    ClassInfo captured = info;
                    iconTasks.Add(Task.Run(() => profile.IconPath = CacheIcon(captured)));
                }
            }

            await Task.WhenAll(iconTasks);

            return result;
        }

        /// <summary>Fallback: vanilla editions[] + profileDescriptions{} (no icon/color). Never throws.</summary>
        private List<ClassProfile> BuildFromEditionsFallback()
        {
            var result = new List<ClassProfile>();
            ServerInfo server = ServerManager.SelectedServer;

            if (server?.editions == null)
            {
                return result;
            }

            foreach (string edition in server.editions)
            {
                if (string.IsNullOrWhiteSpace(edition)) continue;

                string description = null;
                server.profileDescriptions?.TryGetValue(edition, out description);

                result.Add(new ClassProfile
                {
                    EditionKey = edition,
                    Name = edition,
                    Description = description ?? string.Empty
                });
            }

            return result;
        }

        /// <summary>Downloads/caches the class icon via the launcher request infra; null on any failure.</summary>
        private static string CacheIcon(ClassInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.IconUrl)) return null;

            try
            {
                string fileName = Path.GetFileName(info.IconUrl);

                if (string.IsNullOrWhiteSpace(fileName)) return null;

                return ImageRequest.CacheServerImage(info.IconUrl, $"class_{fileName}");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[ClassSelection] Failed to cache icon '{info.IconUrl}': {ex.Message}");
                return null;
            }
        }

        private static IBrush ParseNameColor(string nameColor)
        {
            if (string.IsNullOrWhiteSpace(nameColor)) return null;

            if (Color.TryParse(nameColor.Trim(), out Color color))
            {
                // Immutable: criado em thread de fundo, lido pelo binding na UI thread.
                return new ImmutableSolidColorBrush(color);
            }

            LogManager.Instance.Warning($"[ClassSelection] Invalid nameColor '{nameColor}' — using theme default");
            return null;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            return null;
        }

        // === Fallback de ícone bundlado (Assets/ClassIcons) por match de nome ===

        // keyword normalizada (lowercase, sem acento/espaço) que DEVE aparecer no nome → arquivo.
        // Cobre nomes curtos ("Furtivo") e compostos ("Operador Furtivo") via Contains.
        private static readonly (string Keyword, string File)[] IconNameMap =
        {
            ("cacador", "cacador.png"),
            ("fuzileiro", "fuzileiro.png"),
            ("medico", "medicoDeCombate.png"),
            ("furtivo", "operadorFurtivo.png"),
            ("peladao", "peladao.png"),
            ("saqueador", "saqueador.png"),
            ("tatico", "operadorTatico.png"),
            ("batedor", "batedor.png"),
            ("armeiro", "armeiro.png"),
            ("gerente", "gerenteDeOperacoes.png"),
            ("sobreviv", "sobrevivencialista.png"),
        };

        private static readonly object BundledIconLock = new object();
        private static readonly Dictionary<string, Bitmap> BundledIconCache = new Dictionary<string, Bitmap>(StringComparer.Ordinal);

        /// <summary>Resolves a bundled icon from the class name; null when there is no keyword match.</summary>
        private static Bitmap ResolveBundledIcon(string name)
        {
            string normalized = NormalizeName(name);

            if (string.IsNullOrEmpty(normalized)) return null;

            foreach ((string keyword, string file) in IconNameMap)
            {
                if (normalized.Contains(keyword))
                {
                    return LoadBundledIcon(file);
                }
            }

            return null;
        }

        /// <summary>Loads (and memoizes) a bundled ClassIcons bitmap. Caches null on failure too.</summary>
        private static Bitmap LoadBundledIcon(string file)
        {
            lock (BundledIconLock)
            {
                if (BundledIconCache.TryGetValue(file, out Bitmap cached)) return cached;
            }

            Bitmap bitmap = null;

            try
            {
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                using Stream stream = AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/ClassIcons/{file}"));
                bitmap = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[ClassSelection] Falha ao carregar ícone bundlado '{file}': {ex.Message}");
            }

            lock (BundledIconLock)
            {
                BundledIconCache[file] = bitmap;
                return bitmap;
            }
        }

        /// <summary>Lowercase, strip accents and non-alphanumerics (spaces/punctuation).</summary>
        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            string decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (char c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(c)) builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
