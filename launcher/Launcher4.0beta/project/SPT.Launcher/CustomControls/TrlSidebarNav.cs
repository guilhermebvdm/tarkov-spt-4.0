using Avalonia;
using Avalonia.Controls;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// TRL sidebar chrome: 280px column, panel surface, hairline right edge and
    /// optional uppercase group label. Host nav items (e.g. a ListBox with
    /// Classes="trl-nav") as content. Visuals live in the ControlTheme.
    /// </summary>
    public class TrlSidebarNav : ContentControl
    {
        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<TrlSidebarNav, string>(nameof(Header), string.Empty);

        public string Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
    }
}
