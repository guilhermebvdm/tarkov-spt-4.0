using Avalonia;
using Avalonia.Controls;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// TRL workhorse container: panel surface, hairline edge and optional
    /// uppercase display title header. Visuals live in the ControlTheme
    /// (Assets/Theme/Controls/TrlCustomControls.axaml).
    /// ContentControl-based so consumers can nest arbitrary content.
    /// </summary>
    public class TrlPanel : ContentControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<TrlPanel, string>(nameof(Title), string.Empty);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<bool> ShowHeaderProperty =
            AvaloniaProperty.Register<TrlPanel, bool>(nameof(ShowHeader), true);

        public bool ShowHeader
        {
            get => GetValue(ShowHeaderProperty);
            set => SetValue(ShowHeaderProperty, value);
        }
    }
}
