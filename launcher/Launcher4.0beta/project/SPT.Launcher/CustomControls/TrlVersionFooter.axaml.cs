using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// Launcher/server version footer. Defaults keep the values that were
    /// hardcoded in the views; item 013 (dynamic server version) binds real data.
    /// </summary>
    public partial class TrlVersionFooter : UserControl
    {
        public TrlVersionFooter()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> LauncherVersionProperty =
            AvaloniaProperty.Register<TrlVersionFooter, string>(nameof(LauncherVersion), "15.0");

        public string LauncherVersion
        {
            get => GetValue(LauncherVersionProperty);
            set => SetValue(LauncherVersionProperty, value);
        }

        public static readonly StyledProperty<string> ServerVersionProperty =
            AvaloniaProperty.Register<TrlVersionFooter, string>(nameof(ServerVersion), "0.10");

        public string ServerVersion
        {
            get => GetValue(ServerVersionProperty);
            set => SetValue(ServerVersionProperty, value);
        }
    }
}
