using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// Brand laser divider (1px red gradient + glow). One per view, maximum (R1).
    /// </summary>
    public partial class TrlLaserDivider : UserControl
    {
        public TrlLaserDivider()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
