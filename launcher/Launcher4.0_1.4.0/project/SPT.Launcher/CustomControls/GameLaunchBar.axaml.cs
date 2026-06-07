using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.CustomControls;

public partial class GameLaunchBar : UserControl
{
    public GameLaunchBar()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<ProfileInfo> ProfileInfoProperty = AvaloniaProperty.Register<GameLaunchBar, ProfileInfo>(
        "ProfileInfo");

    public ProfileInfo ProfileInfo
    {
        get => GetValue(ProfileInfoProperty);
        set => SetValue(ProfileInfoProperty, value);
    }

    public static readonly StyledProperty<ICommand> StartGameCommandProperty = AvaloniaProperty.Register<GameLaunchBar, ICommand>(
        "StartGameCommand");

    public ICommand StartGameCommand
    {
        get => GetValue(StartGameCommandProperty);
        set => SetValue(StartGameCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> LogoutCommandProperty = AvaloniaProperty.Register<GameLaunchBar, ICommand>(
        "LogoutCommand");

    public ICommand LogoutCommand
    {
        get => GetValue(LogoutCommandProperty);
        set => SetValue(LogoutCommandProperty, value);
    }

    // === Update properties ===

    public static readonly StyledProperty<string> UpdateStatusTextProperty = AvaloniaProperty.Register<GameLaunchBar, string>(
        "UpdateStatusText", "");

    public string UpdateStatusText
    {
        get => GetValue(UpdateStatusTextProperty);
        set => SetValue(UpdateStatusTextProperty, value);
    }

    public static readonly StyledProperty<double> UpdateProgressProperty = AvaloniaProperty.Register<GameLaunchBar, double>(
        "UpdateProgress", 0);

    public double UpdateProgress
    {
        get => GetValue(UpdateProgressProperty);
        set => SetValue(UpdateProgressProperty, value);
    }

    public static readonly StyledProperty<double> UpdateMaxProgressProperty = AvaloniaProperty.Register<GameLaunchBar, double>(
        "UpdateMaxProgress", 100);

    public double UpdateMaxProgress
    {
        get => GetValue(UpdateMaxProgressProperty);
        set => SetValue(UpdateMaxProgressProperty, value);
    }

    public static readonly StyledProperty<bool> CanUpdateProperty = AvaloniaProperty.Register<GameLaunchBar, bool>(
        "CanUpdate", false);

    public bool CanUpdate
    {
        get => GetValue(CanUpdateProperty);
        set => SetValue(CanUpdateProperty, value);
    }

    public static readonly StyledProperty<ICommand> UpdateModsCommandProperty = AvaloniaProperty.Register<GameLaunchBar, ICommand>(
        "UpdateModsCommand");

    public ICommand UpdateModsCommand
    {
        get => GetValue(UpdateModsCommandProperty);
        set => SetValue(UpdateModsCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> VerifyFilesCommandProperty = AvaloniaProperty.Register<GameLaunchBar, ICommand>(
        "VerifyFilesCommand");

    public ICommand VerifyFilesCommand
    {
        get => GetValue(VerifyFilesCommandProperty);
        set => SetValue(VerifyFilesCommandProperty, value);
    }
}