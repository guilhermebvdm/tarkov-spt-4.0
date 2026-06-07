using SPT.Launcher.ViewModels;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

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
