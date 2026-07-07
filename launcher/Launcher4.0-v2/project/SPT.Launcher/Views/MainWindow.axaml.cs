using System;
using System.Runtime.InteropServices;
using SPT.Launcher.ViewModels;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Splat;

namespace SPT.Launcher.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        // DS é agudo: o Windows 11 arredonda os cantos e desenha a borda de acento via DWM.
        // Removemos os dois na abertura da janela (no-op em Windows < 11 / não-Windows).
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWCP_DONOTROUND = 1;
        private const uint DWMWA_COLOR_NONE = 0xFFFFFFFE;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            WindowNotificationManager notificationManager = new WindowNotificationManager(this);

            Locator.CurrentMutable.RegisterConstant(notificationManager);

            Opened += OnWindowOpened;
        }

        private void OnWindowOpened(object? sender, EventArgs e)
        {
            if (!OperatingSystem.IsWindows())
                return;

            IntPtr hwnd = TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hwnd == IntPtr.Zero)
                return;

            int corner = DWMWCP_DONOTROUND;
            DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));

            int border = unchecked((int)DWMWA_COLOR_NONE);
            DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref border, sizeof(int));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
