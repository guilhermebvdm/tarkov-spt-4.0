using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPT.Launcher.Views.Dialogs
{
    public partial class DeleteAccountDialogView : UserControl
    {
        public DeleteAccountDialogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
