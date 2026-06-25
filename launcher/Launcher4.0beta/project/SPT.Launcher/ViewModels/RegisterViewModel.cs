using SPT.Launcher.Attributes;
using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models;
using SPT.Launcher.Models.SPT;
using ReactiveUI;
using Splat;
using System.Reactive;

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

        public ReactiveCommand<Unit, Unit> GoToLoginCommand { get; set; }
        public ReactiveCommand<Unit, Unit> GoToClassSelectionCommand { get; set; }

        public RegisterViewModel(IScreen Host, string initialUsername = "") : base(Host)
        {
            RegisterUsername = initialUsername;

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
