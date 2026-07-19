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

        // Ao trocar de classe, o painel de detalhe volta pro topo (senão herda o scroll da classe anterior).
        private void OnClassSelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (DetailScroll != null)
            {
                DetailScroll.Offset = new Avalonia.Vector(0, 0);
            }
        }
    }
}
