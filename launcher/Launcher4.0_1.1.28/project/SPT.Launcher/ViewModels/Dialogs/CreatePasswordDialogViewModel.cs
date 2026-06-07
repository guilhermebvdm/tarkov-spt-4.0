using SPT.Launcher.Helpers;
using SPT.Launcher.Utilities;
using ReactiveUI;

namespace SPT.Launcher.ViewModels.Dialogs
{
    public class CreatePasswordDialogViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ConfirmButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public string PasswordWatermark { get; set; }
        public string RepeatPasswordText { get; set; }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                this.RaiseAndSetIfChanged(ref _password, value);
                this.RaisePropertyChanged(nameof(CanConfirm));
            }
        }

        private string _confirmPassword = "";
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                this.RaiseAndSetIfChanged(ref _confirmPassword, value);
                this.RaisePropertyChanged(nameof(CanConfirm));
            }
        }

        public bool CanConfirm => !string.IsNullOrWhiteSpace(Password)
                                  && Password.Length >= 3
                                  && Password == ConfirmPassword;

        /// <summary>
        /// Dialog to force password creation for accounts without passwords
        /// </summary>
        /// <param name="Host">Set to null when <see cref="ViewModelBase.ShowDialog(object)"/> is used</param>
        public CreatePasswordDialogViewModel(IScreen Host) : base(Host)
        {
            Title = LocalizationProvider.Instance.create_password_title;
            Message = LocalizationProvider.Instance.create_password_message;
            ConfirmButtonText = LocalizationProvider.Instance.create_password_confirm;
            CancelButtonText = LocalizationProvider.Instance.cancel;
            PasswordWatermark = LocalizationProvider.Instance.password;
            RepeatPasswordText = LocalizationProvider.Instance.repeat_password;
        }
    }
}
