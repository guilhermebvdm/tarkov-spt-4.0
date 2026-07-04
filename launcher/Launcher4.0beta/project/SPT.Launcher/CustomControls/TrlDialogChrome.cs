using Avalonia;
using Avalonia.Controls;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// TRL dialog shell: raised surface, strong edge, 34px screen-bar style
    /// header with uppercase display title. Wrap DialogHost dialog contents
    /// with it. Visuals live in the ControlTheme (TrlCustomControls.axaml).
    /// </summary>
    public class TrlDialogChrome : ContentControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<TrlDialogChrome, string>(nameof(Title), string.Empty);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}
