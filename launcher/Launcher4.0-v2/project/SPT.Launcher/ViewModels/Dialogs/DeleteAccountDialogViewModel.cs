using ReactiveUI;

namespace SPT.Launcher.ViewModels.Dialogs
{
    /// <summary>
    /// Strong confirmation dialog for permanent account deletion (item 010).
    /// The confirm button only enables when the user types the exact username.
    /// </summary>
    public class DeleteAccountDialogViewModel : ViewModelBase
    {
        /// <summary>Username that must be typed to enable confirmation.</summary>
        public string Username { get; }

        private string _typedUsername = "";
        public string TypedUsername
        {
            get => _typedUsername;
            set
            {
                this.RaiseAndSetIfChanged(ref _typedUsername, value);
                this.RaisePropertyChanged(nameof(CanConfirm));
            }
        }

        /// <summary>Case-sensitive match; leading/trailing whitespace is forgiven.</summary>
        public bool CanConfirm => TypedUsername?.Trim() == Username;

        /// <summary>
        /// </summary>
        /// <param name="Host">Set to null when <see cref="ViewModelBase.ShowDialog(object)"/> is used</param>
        /// <param name="username">Account username the player must type to confirm</param>
        public DeleteAccountDialogViewModel(IScreen Host, string username) : base(Host)
        {
            Username = username;
        }
    }
}
