using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SPT.Launcher.ViewModels;

namespace SPT.Launcher.Views
{
    public partial class ClassSelectionView : ReactiveUserControl<ClassSelectionViewModel>
    {
        public ClassSelectionView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
