using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPT.Launcher.Views.Dialogs
{
    public partial class OnboardingDialogView : UserControl
    {
        public OnboardingDialogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
