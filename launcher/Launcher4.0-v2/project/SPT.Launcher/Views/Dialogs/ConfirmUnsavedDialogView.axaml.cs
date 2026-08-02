using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPT.Launcher.Views.Dialogs
{
    public partial class ConfirmUnsavedDialogView : UserControl
    {
        public ConfirmUnsavedDialogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
