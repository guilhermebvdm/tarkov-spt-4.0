using SPT.Launcher.ViewModels;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace SPT.Launcher.Views
{
    public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Forward activation to the ViewModel (drives the background carousel Start/Stop).
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }

        private async void CopyIdToClipboard(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm && !string.IsNullOrEmpty(vm.CurrentId))
            {
                await vm.CopyCommand(vm.CurrentId);
            }
        }
    }
}
