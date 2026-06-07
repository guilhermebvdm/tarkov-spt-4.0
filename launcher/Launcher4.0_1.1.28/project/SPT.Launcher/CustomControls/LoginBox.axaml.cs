using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SPT.Launcher.CustomControls;

public partial class LoginBox : UserControl
{
    private bool _passwordVisible = false;

    public LoginBox()
    {
        InitializeComponent();
    }

    private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        var passwordBox = this.FindControl<TextBox>("PasswordBox");
        if (passwordBox != null)
        {
            passwordBox.PasswordChar = _passwordVisible ? '\0' : '●';
        }
        if (sender is Button btn)
        {
            btn.Content = _passwordVisible ? "🙉" : "🙈";
        }
    }

    public static readonly StyledProperty<string> UsernameProperty = AvaloniaProperty.Register<LoginBox, string>(
        "Username");

    public string Username
    {
        get => GetValue(UsernameProperty);
        set => SetValue(UsernameProperty, value);
    }

    public static readonly StyledProperty<string> PasswordProperty = AvaloniaProperty.Register<LoginBox, string>(
        "Password");

    public string Password
    {
        get => GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public static readonly StyledProperty<ICommand> LoginCommandProperty = AvaloniaProperty.Register<LoginBox, ICommand>(
        "LoginCommand");

    public ICommand LoginCommand
    {
        get => GetValue(LoginCommandProperty);
        set => SetValue(LoginCommandProperty, value);
    }

    public static readonly StyledProperty<bool> IsLoggedInProperty = AvaloniaProperty.Register<LoginBox, bool>(
        "IsLoggedIn");

    public bool IsLoggedIn
    {
        get => GetValue(IsLoggedInProperty);
        set => SetValue(IsLoggedInProperty, value);
    }

    public static readonly StyledProperty<ICommand> ResetPasswordCommandProperty = AvaloniaProperty.Register<LoginBox, ICommand>(
        "ResetPasswordCommand");

    public ICommand ResetPasswordCommand
    {
        get => GetValue(ResetPasswordCommandProperty);
        set => SetValue(ResetPasswordCommandProperty, value);
    }
}