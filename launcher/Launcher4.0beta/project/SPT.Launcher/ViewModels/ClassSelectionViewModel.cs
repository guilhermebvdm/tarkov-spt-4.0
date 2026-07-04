using SPT.Launcher.Attributes;
using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Models.TRL;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using Avalonia.Media;
using Avalonia.Media.Immutable;
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
                RegisterErrorMsg = "";

                if (SelectedClass == null) return;

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
            List<ClassProfile> classes = null;

            try
            {
                string json = await Task.Run(() => RequestHandler.RequestClassList());
                List<ClassInfo> serverClasses = Json.Deserialize<List<ClassInfo>>(json);

                if (serverClasses != null && serverClasses.Count > 0)
                {
                    classes = BuildFromServer(serverClasses);
                }
                else
                {
                    LogManager.Instance.Warning("[ClassSelection] /customclasses/classes returned null/empty list");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[ClassSelection] Failed to load /customclasses/classes: {ex.Message}");
            }

            if (classes == null || classes.Count == 0)
            {
                LogManager.Instance.Warning("[ClassSelection] Falling back to vanilla editions + profileDescriptions");
                classes = BuildFromEditionsFallback();
            }

            Dispatcher.UIThread.Post(() =>
            {
                AvailableClasses.Clear();

                foreach (ClassProfile profile in classes)
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

        /// <summary>Maps the SP0 payload to UI items. Dedupes by editionKey defensively (P-058.4).</summary>
        private List<ClassProfile> BuildFromServer(List<ClassInfo> serverClasses)
        {
            var result = new List<ClassProfile>();
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

                result.Add(new ClassProfile
                {
                    EditionKey = info.EditionKey,
                    Name = FirstNonEmpty(info.DisplayName?.Pt, info.DisplayName?.En) ?? info.EditionKey,
                    Description = description ?? string.Empty,
                    IconPath = CacheIcon(info),
                    NameBrush = ParseNameColor(info.NameColor),
                    Skills = info.Skills,
                    SkillMultipliers = info.SkillMultipliers
                });
            }

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
    }
}
