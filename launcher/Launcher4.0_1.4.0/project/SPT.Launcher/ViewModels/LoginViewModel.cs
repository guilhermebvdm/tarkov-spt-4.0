using SPT.Launcher.Attributes;
using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models;
using SPT.Launcher.Models.SPT;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.ViewModels.Dialogs;
using Avalonia.Controls.Notifications;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.ViewModels
{
    [RequireServerConnected]
    public class LoginViewModel : ViewModelBase
    {
        public ObservableCollection<ProfileInfo> ExistingProfiles { get; set; } = new ObservableCollection<ProfileInfo>();

        public LoginModel Login { get; set; } = new LoginModel();

        public ImageHelper Background => Splat.Locator.Current.GetService<ImageHelper>("bgimage");

        public ReactiveCommand<Unit, Unit> LoginCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ResetPasswordCommand { get; set; }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
        }

        public LoginViewModel(IScreen Host, bool NoAutoLogin = false) : base(Host)
        {
            //setup reactive commands
            LoginCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                LogManager.Instance.Info($"[Login] Tentando login: {Login.Username}");
                AccountStatus status = await AccountManager.LoginAsync(Login);
                LogManager.Instance.Info($"[Login] Resultado: {status}");

                switch (status)
                {
                    case AccountStatus.OK:
                        {
                            string storedPassword = AccountManager.SelectedAccount?.password ?? "";

                            // Client-side password validation (SPT server does NOT validate passwords)
                            if (!string.IsNullOrEmpty(storedPassword) && Login.Password != storedPassword)
                            {
                                AccountManager.Logout();
                                SendNotification("", LocalizationProvider.Instance.incorrect_login, NotificationType.Error);
                                return;
                            }

                            // Check if SERVER-SIDE password is empty and force password creation
                            if (string.IsNullOrEmpty(storedPassword))
                            {
                                var createPwdVm = new CreatePasswordDialogViewModel(null);
                                var result = await ShowDialog(createPwdVm);

                                if (result is string newPassword && !string.IsNullOrWhiteSpace(newPassword))
                                {
                                    AccountStatus changePwdStatus = await AccountManager.ChangePasswordAsync(newPassword);

                                    if (changePwdStatus == AccountStatus.OK)
                                    {
                                        Login.Password = newPassword;
                                        SendNotification("", LocalizationProvider.Instance.create_password_success, NotificationType.Success);
                                    }
                                    else
                                    {
                                        SendNotification("", LocalizationProvider.Instance.edit_account_update_error, NotificationType.Error);
                                        AccountManager.Logout();
                                        return;
                                    }
                                }
                                else
                                {
                                    // User cancelled - logout
                                    AccountManager.Logout();
                                    return;
                                }
                            }

                            // Registrar HWID silenciosamente após login bem-sucedido
                            _ = Task.Run(() =>
                            {
                                try
                                {
                                    string hwid = HwidHelper.GetHwid();
                                    var hwidData = new HwidRegisterRequestData(Login.Username, Login.Password, hwid);
                                    RequestHandler.RequestHwidRegister(hwidData);
                                }
                                catch (Exception ex)
                                {
                                    Controllers.LogManager.Instance.Warning($"[HWID] Failed to register: {ex.Message}");
                                }
                            });

                            if (LauncherSettingsProvider.Instance.UseAutoLogin && LauncherSettingsProvider.Instance.Server.AutoLoginCreds != Login)
                            {
                                LauncherSettingsProvider.Instance.Server.AutoLoginCreds = Login;
                            }

                            // Salvar último username se "Lembrar Usuário" ativo
                            if (LauncherSettingsProvider.Instance.RememberUsername)
                            {
                                LauncherSettingsProvider.Instance.LastUsername = Login.Username;
                                LauncherSettingsProvider.Instance.LastPassword = Login.Password;
                            }
                            else
                            {
                                LauncherSettingsProvider.Instance.LastPassword = "";
                            }

                            LauncherSettingsProvider.Instance.SaveSettings();
                            IsLoggedIn = true;
                            NavigateTo(new ProfileViewModel(HostScreen));
                            break;
                        }
                    case AccountStatus.LoginFailed:
                        {
                            // Create account if it doesn't exist
                            if (!string.IsNullOrWhiteSpace(Login.Username))
                            {
                                if (Login.Username.Length > 15)
                                {
                                    SendNotification(LocalizationProvider.Instance.registration_failed, LocalizationProvider.Instance.register_failed_name_limit, NotificationType.Error);
                                    return;
                                }
                                
                                var result = await ShowDialog(new RegisterDialogViewModel(null, Login.Username));

                                if (result != null && result is RegisterDialogViewModel regDialog)
                                {
                                    var edition = regDialog.Editions.SelectedEdition;
                                    var registerPassword = regDialog.Password;

                                    if (edition == null || string.IsNullOrWhiteSpace(registerPassword)) 
                                        return;

                                    AccountStatus registerResult = await AccountManager.RegisterAsync(Login.Username, registerPassword, edition.Name);
                                    LogManager.Instance.Info($"[Login] Registro resultado: {registerResult}");

                                    switch (registerResult)
                                    {
                                        case AccountStatus.OK:
                                            {
                                                if (LauncherSettingsProvider.Instance.UseAutoLogin && LauncherSettingsProvider.Instance.Server.AutoLoginCreds != Login)
                                                {
                                                    LauncherSettingsProvider.Instance.Server.AutoLoginCreds = Login;
                                                }

                                                LauncherSettingsProvider.Instance.SaveSettings();
                                                SendNotification(LocalizationProvider.Instance.profile_created, Login.Username, NotificationType.Success);
                                                NavigateTo(new ProfileViewModel(HostScreen));
                                                break;
                                            }
                                        case AccountStatus.RegisterFailed:
                                            {
                                                SendNotification("", LocalizationProvider.Instance.registration_failed, NotificationType.Error);
                                                break;
                                            }
                                        case AccountStatus.NoConnection:
                                            {
                                                NavigateTo(new ConnectServerViewModel(HostScreen));
                                                break;
                                            }
                                        default:
                                            {
                                                SendNotification("", registerResult.ToString(), NotificationType.Error);
                                                break;
                                            }
                                    }

                                    return;
                                }
                            }

                            SendNotification("", LocalizationProvider.Instance.login_failed, NotificationType.Error);

                            break;
                        }
                    case AccountStatus.NoConnection:
                        {
                            // Se estiver em auto-login e der falha de conexão,
                            // quebra o loop para não ficar voltando infinitamente para o ConnectServerViewModel.
                            if (LauncherSettingsProvider.Instance.UseAutoLogin)
                            {
                                LauncherSettingsProvider.Instance.UseAutoLogin = false;
                                LauncherSettingsProvider.Instance.SaveSettings();
                                SendNotification("", "Erro de conexão ao tentar fazer o Auto-Login. Auto-Login desativado.", NotificationType.Error);
                            }
                            else
                            {
                                NavigateTo(new ConnectServerViewModel(HostScreen));
                            }
                            break;
                        }
                }
            });

            // Comando para resetar senha via HWID
            ResetPasswordCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (string.IsNullOrWhiteSpace(Login.Username))
                {
                    SendNotification("", LocalizationProvider.Instance.reset_password_failed, NotificationType.Error);
                    return;
                }

                try
                {
                    // Step 1: Verify HWID with the mod
                    string hwid = HwidHelper.GetHwid();
                    var resetData = new HwidResetPasswordRequestData(Login.Username, hwid);
                    string response = RequestHandler.RequestHwidResetPassword(resetData);

                    var json = JObject.Parse(response);
                    string status = json["status"]?.ToString() ?? "";

                    if (status == "HWID_MISMATCH")
                    {
                        SendNotification("", LocalizationProvider.Instance.reset_password_hwid_mismatch, NotificationType.Error);
                        return;
                    }
                    else if (status == "NO_HWID_REGISTERED")
                    {
                        SendNotification("", LocalizationProvider.Instance.reset_password_no_hwid, NotificationType.Error);
                        return;
                    }
                    else if (status != "OK")
                    {
                        SendNotification("", LocalizationProvider.Instance.reset_password_failed, NotificationType.Error);
                        return;
                    }

                    // Step 2: Login silently (SPT server doesn't validate passwords)
                    AccountStatus loginStatus = await AccountManager.LoginAsync(Login.Username, "");
                    if (loginStatus != AccountStatus.OK)
                    {
                        SendNotification("", LocalizationProvider.Instance.reset_password_failed, NotificationType.Error);
                        return;
                    }

                    // Step 3: Clear password via SPT's official API (updates memory + disk)
                    AccountStatus changePwdStatus = await AccountManager.ChangePasswordAsync("");
                    AccountManager.Logout();

                    if (changePwdStatus != AccountStatus.OK)
                    {
                        SendNotification("", LocalizationProvider.Instance.reset_password_failed, NotificationType.Error);
                        return;
                    }

                    // Step 4: Re-login to trigger password creation dialog
                    Login.Password = "";
                    SendNotification("", LocalizationProvider.Instance.reset_password_success, NotificationType.Success);
                    await Task.Delay(500);
                    LoginCommand.Execute();
                }
                catch (Exception)
                {
                    SendNotification("", LocalizationProvider.Instance.reset_password_failed, NotificationType.Error);
                }
            });

            //cache and touch background image
            var backgroundImage = Locator.Current.GetService<ImageHelper>("bgimage");

            ImageRequest.CacheBackgroundImage();

            backgroundImage.Touch();

            //handle auto-login
            if (LauncherSettingsProvider.Instance.UseAutoLogin && LauncherSettingsProvider.Instance.Server.AutoLoginCreds != null && !NoAutoLogin)
            {
                Login = LauncherSettingsProvider.Instance.Server.AutoLoginCreds;
                LogManager.Instance.Info($"[Login] Auto-login ativado para: {Login.Username}");
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LoginCommand.Execute();
                });
                return;
            }

            Task.Run(() =>
            {
                GetExistingProfiles();
            });

            // Preencher username salvo se "Lembrar Usuário" ativo (e não é auto-login)
            if (LauncherSettingsProvider.Instance.RememberUsername 
                && !string.IsNullOrEmpty(LauncherSettingsProvider.Instance.LastUsername))
            {
                Login.Username = LauncherSettingsProvider.Instance.LastUsername;
                Login.Password = LauncherSettingsProvider.Instance.LastPassword;
            }

            // Salvar settings quando checkboxes de login mudam
            LauncherSettingsProvider.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "UseAutoLogin" || e.PropertyName == "RememberUsername")
                {
                    LauncherSettingsProvider.Instance.SaveSettings();
                }
            };
        }

        public void LoginProfileCommand(object parameter)
        {
            if (parameter == null) return;

            Task.Run(() =>
            {
                if (parameter is string username)
                {
                    Login.Username = username;
                    LoginCommand.Execute();
                }
            });
        }

        public async Task GetExistingProfiles()
        {
            await Task.Delay(200);
            
            ServerProfileInfo[] existingProfiles = AccountManager.GetExistingProfiles();

            if(existingProfiles != null)
            {
                ExistingProfiles.Clear();

                foreach(ServerProfileInfo profile in existingProfiles)
                {
                    ProfileInfo profileInfo = new ProfileInfo(profile);

                    ExistingProfiles.Add(profileInfo);

                    ImageRequest.CacheSideImage(profileInfo.Side);

                    ImageHelper sideImage = new ImageHelper() { Path = profileInfo.SideImage };
                    sideImage.Touch();

                    await Task.Delay(100);
                }
            }
        }
    }
}
