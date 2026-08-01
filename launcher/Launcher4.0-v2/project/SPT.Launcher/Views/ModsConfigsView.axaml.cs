using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SPT.Launcher.ViewModels;

namespace SPT.Launcher.Views
{
    public partial class ModsConfigsView : ReactiveUserControl<ModsConfigsViewModel>
    {
        public ModsConfigsView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
