using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// TRL status tag (dot + uppercase display text). Dot color carries the
    /// status: TrlAccentBrush (default), TrlSuccessBrush, TrlWarningBrush,
    /// TrlDangerBrush, TrlBrandBrush (brand moments only).
    /// </summary>
    public partial class TrlTag : UserControl
    {
        public TrlTag()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<TrlTag, string>(nameof(Text), string.Empty);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<IBrush?> DotBrushProperty =
            AvaloniaProperty.Register<TrlTag, IBrush?>(nameof(DotBrush));

        public IBrush? DotBrush
        {
            get => GetValue(DotBrushProperty);
            set => SetValue(DotBrushProperty, value);
        }
    }
}
