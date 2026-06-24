using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json.Linq;

namespace SPT.Launcher.CustomControls;

public partial class TotalModsCard : UserControl
{
    public TotalModsCard()
    {
        InitializeComponent();

        // Buscar versão do servidor em background
        Task.Run(() =>
        {
            try
            {
                string response = RequestHandler.RequestRedLineVersion();
                var json = JObject.Parse(response);
                string version = json["version"]?.ToString() ?? "?";

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ServerVersion = version;
                });
            }
            catch
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ServerVersion = "?";
                });
            }
        });
    }

    public static readonly StyledProperty<string> ServerVersionProperty = AvaloniaProperty.Register<TotalModsCard, string>(
        "ServerVersion", defaultValue: "...");

    public string ServerVersion
    {
        get => GetValue(ServerVersionProperty);
        set => SetValue(ServerVersionProperty, value);
    }

    public static readonly StyledProperty<int> ActiveModsCountProperty = AvaloniaProperty.Register<TotalModsCard, int>(
        "ActiveModsCount");

    public int ActiveModsCount
    {
        get => GetValue(ActiveModsCountProperty);
        set => SetValue(ActiveModsCountProperty, value);
    }

    public static readonly StyledProperty<ICommand> OpenModsInfoCommandProperty = AvaloniaProperty.Register<TotalModsCard, ICommand>(
        "OpenModsInfoCommand");

    public ICommand OpenModsInfoCommand
    {
        get => GetValue(OpenModsInfoCommandProperty);
        set => SetValue(OpenModsInfoCommandProperty, value);
    }
}