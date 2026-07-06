using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using ReactiveUI;

namespace SPT.Launcher.ViewModels.Notifications
{
    public class SPTNotificationViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public IBrush BarColor { get; set; }

        public SPTNotificationViewModel(IScreen Host, string Title, string Message, NotificationType Type = NotificationType.Information) : base(Host)
        {
            this.Title = Title;
            this.Message = Message;
            BarColor = ResolveBarColor(Type);
        }

        // Item 025 (AC-8): seam de teste puro — mapeia o tipo da notificação para a chave
        // do token do tema. Sem dependência de Avalonia → roda headless trivialmente.
        internal static string MapTypeToToken(NotificationType type) => type switch
        {
            NotificationType.Information => "TrlAccentBrush",
            NotificationType.Warning => "TrlWarningBrush",
            NotificationType.Success => "TrlSuccessBrush",
            NotificationType.Error => "TrlDangerStrongBrush",
            _ => "TrlFgMutedBrush",
        };

        // Resolve o brush do token no tema atual. Fallback não-nulo (TrlFgMutedBrush embutido)
        // p/ design-time/headless quando Application.Current/o recurso não resolvem (CC-4).
        private static IBrush ResolveBarColor(NotificationType type)
        {
            string key = MapTypeToToken(type);

            if (Application.Current is { } app
                && app.TryFindResource(key, out var res)
                && res is IBrush brush)
            {
                return brush;
            }

            return new SolidColorBrush(Color.Parse("#9B9A96"));
        }
    }
}
