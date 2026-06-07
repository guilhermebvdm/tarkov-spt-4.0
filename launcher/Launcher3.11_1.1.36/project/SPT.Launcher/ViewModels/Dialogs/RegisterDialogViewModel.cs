using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;
using ReactiveUI;

namespace SPT.Launcher.ViewModels.Dialogs
{
    public class RegisterDialogViewModel : ViewModelBase
    {
        public string Question { get; set; }
        public string RegisterButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public string ComboBoxPlaceholderText { get; set; }
        public string PasswordPlaceholderText { get; set; }
        
        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                this.RaiseAndSetIfChanged(ref _password, value);
                this.RaisePropertyChanged(nameof(CanRegister));
            }
        }

        public bool CanRegister => Editions.HasSelection && !string.IsNullOrWhiteSpace(Password);

        public EditionCollection Editions { get; set; } = new EditionCollection();

        /// <summary>
        /// A registration dialog
        /// </summary>
        /// <param name="Host">Set to null when <see cref="ViewModelBase.ShowDialog(object)"/> is used, since the dialog host is handling routing</param>
        /// <param name="Username"></param>
        public RegisterDialogViewModel(IScreen Host, string Username) : base(Host)
        {
            Question = string.Format(LocalizationProvider.Instance.registration_question_format_1, Username);

            RegisterButtonText = LocalizationProvider.Instance.register;
            CancelButtonText = LocalizationProvider.Instance.cancel;

            ComboBoxPlaceholderText = LocalizationProvider.Instance.select_edition;
            PasswordPlaceholderText = LocalizationProvider.Instance.password ?? "Senha";

            Editions.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(Editions.HasSelection))
                    this.RaisePropertyChanged(nameof(CanRegister));
            };
        }
    }
}
