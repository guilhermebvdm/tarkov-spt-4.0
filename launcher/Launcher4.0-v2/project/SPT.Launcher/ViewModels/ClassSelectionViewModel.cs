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
using System.Linq;
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

        /// <summary>Full-body class art (Assets/ClassImages, fundo removido) resolvida por nome. Null sem match.</summary>
        public Bitmap ClassImage { get; set; }

        public bool HasClassImage => ClassImage != null;

        /// <summary>Parsed nameColor brush; null when absent/invalid (trl-nav default foreground applies).</summary>
        public IBrush NameBrush { get; set; }

        public bool HasNameColor => NameBrush != null;

        /// <summary>Kept for future use (kickoff 004: no render yet).</summary>
        public Dictionary<string, int> Skills { get; set; }

        /// <summary>Raw skill → XP factor (as served). Kept for reference; the UI binds <see cref="MultiplierRows"/>.</summary>
        public Dictionary<string, double> SkillMultipliers { get; set; }

        /// <summary>Skill XP multipliers formatted for display (name + colored token). Empty when none in effect.</summary>
        public List<SkillMultiplierRow> MultiplierRows { get; set; } = new List<SkillMultiplierRow>();

        public bool HasMultipliers => MultiplierRows != null && MultiplierRows.Count > 0;

        /// <summary>Metades da lista (column-major) para renderizar os multiplicadores em 2 colunas, mantendo a ordem.</summary>
        public List<SkillMultiplierRow> MultiplierRowsLeft { get; set; } = new List<SkillMultiplierRow>();
        public List<SkillMultiplierRow> MultiplierRowsRight { get; set; } = new List<SkillMultiplierRow>();

        /// <summary>Perks (isPerk=true) da classe — coluna esquerda dos cards. Item 029.</summary>
        public List<PerkEffectRow> Perks { get; set; } = new List<PerkEffectRow>();

        /// <summary>Drawbacks (isPerk=false) — coluna direita dos cards. Item 029.</summary>
        public List<PerkEffectRow> Drawbacks { get; set; } = new List<PerkEffectRow>();

        public bool HasPerks => Perks != null && Perks.Count > 0;
        public bool HasDrawbacks => Drawbacks != null && Drawbacks.Count > 0;
        public bool HasAnyEffects => HasPerks || HasDrawbacks;
    }

    /// <summary>
    /// UI de um card de perk/drawback (item 029), espelhando o painel in-game (PerksPanelView): barra de
    /// acento colorida + título esmaecido + chip de valor + label. Cores = MultiplierFormat do mod.
    /// </summary>
    public class PerkEffectRow
    {
        public string Title { get; set; }
        public string Label { get; set; }
        public string ValueToken { get; set; }
        public bool HasToken => !string.IsNullOrEmpty(ValueToken);
        public bool Pending { get; set; }

        /// <summary>Cor do acento/token: verde (perk) · vermelho (drawback) · âmbar (pending).</summary>
        public IBrush AccentBrush { get; set; }

        /// <summary>Fundo escuro tingido do card (mesma tinta do painel in-game).</summary>
        public IBrush BgBrush { get; set; }
    }

    /// <summary>One XP-multiplier line: humanized skill name + signed token ("+50%" / "−20%"), colored buff/debuff.
    /// Mirrors the in-game CustomClasses color language (verde #9ad27a / vermelho #d27a7a) for visual continuity
    /// with the future perks/drawbacks cards.</summary>
    public class SkillMultiplierRow
    {
        public string Name { get; set; }

        /// <summary>Signed percent token relative to vanilla (factor 1.5 → "+50%", 0.8 → "−20%").</summary>
        public string Token { get; set; }

        /// <summary>Green when the multiplier is a buff (&gt; 1), red when a debuff (&lt; 1).</summary>
        public IBrush Brush { get; set; }
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
                if (SelectedClass == null)
                {
                    RegisterErrorMsg = IsLoading 
                        ? LocalizationProvider.Instance.class_selection_loading 
                        : LocalizationProvider.Instance.class_selection_none_available;
                    return;
                }

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
                        RegisterErrorMsg = LocalizationProvider.Instance.register_username_collision_error;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Fail-closed: sem conseguir listar os perfis, não arriscar criar uma colisão.
                    LogManager.Instance.Error($"[ClassSelection] Falha ao verificar colisão de username: {ex.Message}");
                    RegisterErrorMsg = LocalizationProvider.Instance.register_username_verify_failed;
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
                            SendNotification(LocalizationProvider.Instance.notification_password_title, LocalizationProvider.Instance.register_password_save_failed, NotificationType.Warning);
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
                    RegisterErrorMsg = LocalizationProvider.Instance.register_create_account_error + registerResult.ToString();
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

                    profile.ClassImage = ResolveClassImage(profile.Name); // arte full-body no painel de detalhe

                    SplitMultiplierColumns(profile);    // 2 colunas de multiplicadores (column-major)
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
                        RegisterErrorMsg = LocalizationProvider.Instance.class_selection_none_available;
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

                string description = Pick(info.Description?.En, info.Description?.Pt);

                if (string.IsNullOrEmpty(description) && vanillaDescriptions != null && vanillaDescriptions.TryGetValue(info.EditionKey, out string vanillaDescription))
                {
                    description = vanillaDescription;
                }

                ClassProfile profile = new ClassProfile
                {
                    EditionKey = info.EditionKey,
                    Name = Pick(info.DisplayName?.En, info.DisplayName?.Pt) ?? info.EditionKey,
                    Description = description ?? string.Empty,
                    NameBrush = ParseNameColor(info.NameColor),
                    Skills = info.Skills,
                    SkillMultipliers = info.SkillMultipliers,
                    MultiplierRows = BuildMultiplierRows(info.SkillMultipliers)
                };

                PopulateEffects(profile, info.Effects);
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

        // Mesma linguagem de cor do mod in-game (MultiplierFormat.cs): verde = buff, vermelho = debuff.
        // Immutable: criados em thread de fundo, lidos pelo binding na UI thread.
        private static readonly IBrush BuffBrush = new ImmutableSolidColorBrush(Color.Parse("#9ad27a"));
        private static readonly IBrush DebuffBrush = new ImmutableSolidColorBrush(Color.Parse("#d27a7a"));

        /// <summary>
        /// Formata os multiplicadores de XP crus ({skill → fator}) em linhas exibíveis. Fatores == 1 (sem
        /// efeito) são omitidos. Token = variação percentual assinada vs vanilla (1.5 → "+50%", 0.8 → "−20%"),
        /// na cor de buff/debuff. Ordenado por magnitude do efeito (maior desvio primeiro).
        /// </summary>
        private static List<SkillMultiplierRow> BuildMultiplierRows(Dictionary<string, double> multipliers)
        {
            var rows = new List<SkillMultiplierRow>();
            if (multipliers == null || multipliers.Count == 0) return rows;

            foreach (var pair in multipliers)
            {
                double factor = pair.Value;
                if (double.IsNaN(factor) || Math.Abs(factor - 1.0) < 1e-4) continue; // sem efeito → não exibe

                int percent = (int)Math.Round((factor - 1.0) * 100.0);
                if (percent == 0) continue; // arredonda p/ 0 (ex.: ×1.004) → não vira linha "+0%"
                bool buff = factor > 1.0;
                string token = (buff ? "+" : "−") + Math.Abs(percent) + "%"; // −: U+2212 (minus), igual ao mod

                rows.Add(new SkillMultiplierRow
                {
                    Name = HumanizeSkillName(pair.Key),
                    Token = token,
                    Brush = buff ? BuffBrush : DebuffBrush,
                });
            }

            // Ordena pelo valor COM SINAL, desc: buffs (positivos) primeiro em ordem decrescente, depois
            // os debuffs (negativos). Ex.: +50, +40, +20, −30 (não +50, +40, −30, +20).
            return rows
                .OrderByDescending(r => ParseSignedPercent(r.Token))
                .ToList();
        }

        /// <summary>"−20%"/"+50%" → inteiro COM SINAL para ordenação (U+2212 e '-' contam como negativo).</summary>
        private static int ParseSignedPercent(string token)
        {
            if (string.IsNullOrEmpty(token)) return 0;
            string digits = new string(token.Where(char.IsDigit).ToArray());
            if (!int.TryParse(digits, out int value)) return 0;
            bool negative = token.Contains('−') || token.Contains('-'); // U+2212 (menu do token) ou hífen ASCII
            return negative ? -value : value;
        }

        /// <summary>Divide MultiplierRows (já ordenado) em 2 colunas column-major: esquerda leva a 1ª metade
        /// (com o extra quando ímpar), direita a 2ª — a leitura de cima→baixo da esquerda segue na direita.</summary>
        private static void SplitMultiplierColumns(ClassProfile profile)
        {
            profile.MultiplierRowsLeft.Clear();
            profile.MultiplierRowsRight.Clear();

            var rows = profile.MultiplierRows;
            if (rows == null || rows.Count == 0) return;

            int half = (rows.Count + 1) / 2;
            for (int i = 0; i < rows.Count; i++)
            {
                if (i < half) profile.MultiplierRowsLeft.Add(rows[i]);
                else profile.MultiplierRowsRight.Add(rows[i]);
            }
        }

        /// <summary>Nome de skill PascalCase → legível ("StressResistance" → "Stress Resistance").</summary>
        private static string HumanizeSkillName(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill)) return skill ?? string.Empty;

            var builder = new StringBuilder(skill.Length + 4);
            for (int i = 0; i < skill.Length; i++)
            {
                char c = skill[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(skill[i - 1])) builder.Append(' ');
                builder.Append(c);
            }

            return builder.ToString();
        }

        // === Perks/drawbacks (item 029): cores dos cards espelhando o painel in-game (PerksPanelView) ===
        // Acentos = MultiplierFormat (verde perk / vermelho drawback / âmbar pending).
        private static readonly IBrush PerkAccent = new ImmutableSolidColorBrush(Color.Parse("#9ad27a"));
        private static readonly IBrush DrawbackAccent = new ImmutableSolidColorBrush(Color.Parse("#d27a7a"));
        private static readonly IBrush PendingAccent = new ImmutableSolidColorBrush(Color.Parse("#cc9a3e"));
        // Fundos escuros tingidos (AARRGGBB) — mesmas tintas do card in-game (~0.55 alpha).
        private static readonly IBrush PerkBg = new ImmutableSolidColorBrush(Color.Parse("#8C121A14"));
        private static readonly IBrush DrawbackBg = new ImmutableSolidColorBrush(Color.Parse("#8C1C1313"));
        private static readonly IBrush PendingBg = new ImmutableSolidColorBrush(Color.Parse("#8C1A170E"));

        /// <summary>Separa os efeitos servidos em perks (esquerda) e drawbacks (direita), preservando a ordem.</summary>
        private static void PopulateEffects(ClassProfile profile, List<ClassEffect> effects)
        {
            if (effects == null) return;

            foreach (ClassEffect effect in effects)
            {
                if (effect == null) continue;

                PerkEffectRow row = BuildEffectRow(effect);
                if (effect.IsPerk) profile.Perks.Add(row);
                else profile.Drawbacks.Add(row);
            }
        }

        /// <summary>Monta um card a partir de um efeito do contrato. pt → en no title/label; token vem pronto.</summary>
        private static PerkEffectRow BuildEffectRow(ClassEffect effect)
        {
            bool pending = effect.Pending;
            IBrush accent = pending ? PendingAccent : (effect.IsPerk ? PerkAccent : DrawbackAccent);
            IBrush bg = pending ? PendingBg : (effect.IsPerk ? PerkBg : DrawbackBg);

            return new PerkEffectRow
            {
                Title = Pick(effect.Title?.En, effect.Title?.Pt) ?? string.Empty,
                Label = Pick(effect.Label?.En, effect.Label?.Pt) ?? string.Empty,
                ValueToken = effect.ValueToken ?? string.Empty,
                Pending = pending,
                AccentBrush = accent,
                BgBrush = bg,
            };
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

        /// <summary>Idioma atual da UI = inglês? (ietf_tag do locale). Só en/pt são suportados.</summary>
        private static bool IsEnglish =>
            string.Equals(LocalizationProvider.Instance?.ietf_tag, "en", StringComparison.OrdinalIgnoreCase);

        /// <summary>Escolhe en/pt do conteúdo servido pelo CustomClasses conforme o idioma da UI (com fallback).</summary>
        private static string Pick(string en, string pt) => IsEnglish ? FirstNonEmpty(en, pt) : FirstNonEmpty(pt, en);

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
            ("tanque", "tanque.png"),
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

        // === Arte full-body da classe (Assets/ClassImages, fundo removido): resolvida por keyword ===
        // Um subconjunto de classes tem arte; sem match → painel sem imagem (info ocupa a largura toda).
        // Keywords em PT e EN (o nome exibido segue o idioma da UI) → mesmo arquivo. Sem isso, a arte
        // sumia em inglês (o nome virava "Scavenger"/"Hunter" e não casava com as keywords só-PT).
        private static readonly (string Keyword, string File)[] ClassImageNameMap =
        {
            ("cacador", "cacador.png"),   ("hunter", "cacador.png"),
            ("furtivo", "furtivo.png"),   ("stealth", "furtivo.png"),
            ("fuzileiro", "fuzileiro.png"), ("rifleman", "fuzileiro.png"),
            ("medico", "medico.png"),     ("medic", "medico.png"),
            ("peladao", "peladao.png"),   ("naked", "peladao.png"),
            ("saqueador", "saqueador.png"), ("scavenger", "saqueador.png"),
            ("tanque", "tanque.png"),     ("tank", "tanque.png"),
        };

        private static readonly object ClassImageLock = new object();
        private static readonly Dictionary<string, Bitmap> ClassImageCache = new Dictionary<string, Bitmap>(StringComparer.Ordinal);

        /// <summary>Resolve a arte full-body da classe pelo nome (keyword). Null quando não há match.</summary>
        private static Bitmap ResolveClassImage(string name)
        {
            string normalized = NormalizeName(name);
            if (string.IsNullOrEmpty(normalized)) return null;

            foreach ((string keyword, string file) in ClassImageNameMap)
            {
                if (normalized.Contains(keyword)) return LoadClassImage(file);
            }

            return null;
        }

        /// <summary>Carrega (e memoiza) uma arte de Assets/ClassImages. Cacheia null na falha também.</summary>
        private static Bitmap LoadClassImage(string file)
        {
            lock (ClassImageLock)
            {
                if (ClassImageCache.TryGetValue(file, out Bitmap cached)) return cached;
            }

            Bitmap bitmap = null;

            try
            {
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                using Stream stream = AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/ClassImages/{file}"));
                bitmap = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[ClassSelection] Falha ao carregar arte de classe '{file}': {ex.Message}");
            }

            lock (ClassImageLock)
            {
                ClassImageCache[file] = bitmap;
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
