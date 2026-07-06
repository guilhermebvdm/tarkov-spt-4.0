using SPT.Launcher.Attributes;
using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models;
using SPT.Launcher.Models.SPT;
using ReactiveUI;
using Splat;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace SPT.Launcher.ViewModels
{
    [RequireServerConnected]
    public class RegisterViewModel : ViewModelBase
    {
        public ImageHelper Background => Splat.Locator.Current.GetService<ImageHelper>("bgimage");

        private string _registerUsername;
        public string RegisterUsername
        {
            get => _registerUsername;
            set => this.RaiseAndSetIfChanged(ref _registerUsername, value);
        }

        private string _registerPassword;
        public string RegisterPassword
        {
            get => _registerPassword;
            set => this.RaiseAndSetIfChanged(ref _registerPassword, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }

        private string _registerErrorMsg;
        public string RegisterErrorMsg
        {
            get => _registerErrorMsg;
            set => this.RaiseAndSetIfChanged(ref _registerErrorMsg, value);
        }

        // Mostrar/ocultar senha por campo (senha + confirmar), mesmo padrão do Settings/Login.
        private bool _passwordVisible = false;
        public char PasswordChar => _passwordVisible ? '\0' : '●';
        public bool IsPasswordVisible => _passwordVisible;

        public void TogglePasswordVisibilityCommand()
        {
            _passwordVisible = !_passwordVisible;
            this.RaisePropertyChanged(nameof(PasswordChar));
            this.RaisePropertyChanged(nameof(IsPasswordVisible));
        }

        private bool _confirmVisible = false;
        public char ConfirmPasswordChar => _confirmVisible ? '\0' : '●';
        public bool IsConfirmVisible => _confirmVisible;

        public void ToggleConfirmVisibilityCommand()
        {
            _confirmVisible = !_confirmVisible;
            this.RaisePropertyChanged(nameof(ConfirmPasswordChar));
            this.RaisePropertyChanged(nameof(IsConfirmVisible));
        }

        // Item 022 (Grupo D / 013L, AC-022.13/14) — footer de versão reativo (idem LoginViewModel):
        // substitui o x:Static read-once que congelava "—" a sessão toda em caso de fetch falho.
        private string _serverVersion = ServerManager.TrlServerVersion;
        public string ServerVersion
        {
            get => _serverVersion;
            set => this.RaiseAndSetIfChanged(ref _serverVersion, value);
        }

        public ReactiveCommand<Unit, Unit> GoToLoginCommand { get; set; }
        public ReactiveCommand<Unit, Unit> GoToClassSelectionCommand { get; set; }

        public RegisterViewModel(IScreen Host, string initialUsername = "") : base(Host)
        {
            RegisterUsername = initialUsername;

            // Refetch barato quando a versão ficou desconhecida no connect (async: evita freeze de UI).
            if (_serverVersion == "—")
            {
                _ = Task.Run(() =>
                {
                    var refreshed = ServerManager.RefreshTrlServerVersionIfUnknown();
                    Dispatcher.UIThread.Post(() => ServerVersion = refreshed);
                });
            }

            GoToLoginCommand = ReactiveCommand.Create(() => 
            {
                NavigateTo(new LoginViewModel(HostScreen));
            });

            GoToClassSelectionCommand = ReactiveCommand.Create(() => 
            {
                RegisterErrorMsg = "";
                if (string.IsNullOrWhiteSpace(RegisterUsername) || RegisterUsername.Length > 15)
                {
                    RegisterErrorMsg = "Usuário inválido (vazio ou maior que 15 caracteres).";
                    return;
                }
                if (string.IsNullOrWhiteSpace(RegisterPassword) || RegisterPassword != ConfirmPassword)
                {
                    RegisterErrorMsg = "Senhas não são idênticas!";
                    return;
                }

                NavigateTo(new ClassSelectionViewModel(HostScreen, RegisterUsername, RegisterPassword));
            });
        }
    }
}
