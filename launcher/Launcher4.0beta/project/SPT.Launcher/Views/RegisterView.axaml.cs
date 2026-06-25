using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SPT.Launcher.ViewModels;

namespace SPT.Launcher.Views
{
    public partial class RegisterView : ReactiveUserControl<RegisterViewModel>
    {
        public RegisterView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
