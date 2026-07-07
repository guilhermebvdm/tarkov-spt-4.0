using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace SPT.Launcher.CustomControls
{
    /// <summary>
    /// TRL screen-bar (signature military window chrome, 34px).
    /// Title/Meta should be provided uppercase by the consumer.
    /// DotBrush: TrlSuccessBrush = synced, TrlWarningBrush = attention,
    /// TrlBrandBrush = live/dirty (the legitimate red moment).
    /// </summary>
    public partial class TrlScreenBar : UserControl
    {
        public TrlScreenBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<TrlScreenBar, string>(nameof(Title), string.Empty);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly StyledProperty<string> MetaProperty =
            AvaloniaProperty.Register<TrlScreenBar, string>(nameof(Meta), string.Empty);

        public string Meta
        {
            get => GetValue(MetaProperty);
            set => SetValue(MetaProperty, value);
        }

        public static readonly StyledProperty<bool> ShowDotProperty =
            AvaloniaProperty.Register<TrlScreenBar, bool>(nameof(ShowDot), false);

        public bool ShowDot
        {
            get => GetValue(ShowDotProperty);
            set => SetValue(ShowDotProperty, value);
        }

        public static readonly StyledProperty<IBrush?> DotBrushProperty =
            AvaloniaProperty.Register<TrlScreenBar, IBrush?>(nameof(DotBrush));

        public IBrush? DotBrush
        {
            get => GetValue(DotBrushProperty);
            set => SetValue(DotBrushProperty, value);
        }
    }
}
