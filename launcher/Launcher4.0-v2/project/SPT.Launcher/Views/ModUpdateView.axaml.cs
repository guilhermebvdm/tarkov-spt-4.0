using SPT.Launcher.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace SPT.Launcher.Views;

public partial class ModUpdateView : ReactiveUserControl<ModUpdateViewModel>
{
    public ModUpdateView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
