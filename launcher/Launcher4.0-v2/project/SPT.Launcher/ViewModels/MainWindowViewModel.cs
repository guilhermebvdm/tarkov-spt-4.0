using System;
using Avalonia;
using ReactiveUI;
using System.Reactive.Disposables;
using SPT.Launcher.Models;
using SPT.Launcher.MiniCommon;
using System.IO;
using Splat;
using SPT.Launcher.Models.SPT;
using SPT.Launcher.Helpers;
using SPT.Launcher.ViewModels.Dialogs;
using Avalonia.Threading;
using DialogHostAvalonia;


namespace SPT.Launcher.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IActivatableViewModel, IScreen
    {
        public string WindowTitle => $"Launcher Tarkov Red Line v{LauncherUpdateHelper.CurrentVersion}";
        
        public SPTVersion VersionInfo { get; set; } = new SPTVersion();
        public RoutingState Router { get; } = new RoutingState();
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public ImageHelper Background { get; } = new ImageHelper()
        {
            Path = Path.Join(ImageRequest.ImageCacheFolder, "bg.png")
        };

        // Item 001 — recuo do laser: telas de auth (Login/Cadastro) mantêm a linha vermelha só
        // sobre o painel (janela fixa 1140, painel 450 → inset esquerdo 690); demais telas full-width.
        private Thickness _laserMargin;
        public Thickness LaserMargin
        {
            get => _laserMargin;
            set => this.RaiseAndSetIfChanged(ref _laserMargin, value);
        }

        public MainWindowViewModel()
        {
            Locator.CurrentMutable.RegisterConstant<ImageHelper>(Background, "bgimage");

            Locator.CurrentMutable.RegisterConstant<SPTVersion>(VersionInfo, "sptversion");
            
            LauncherSettingsProvider.Instance.ResetDefaults();

            LauncherSettingsProvider.Instance.AllowSettings = true;

            if (LauncherSettingsProvider.Instance.FirstRun)
            {
                LauncherSettingsProvider.Instance.FirstRun = false;
                LocalizationProvider.TryAutoSetLocale();
                LauncherSettingsProvider.Instance.SaveSettings();

                /* Desativado: Diálogo de copiar configs do jogo oficial
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var viewModel = new ConfirmationDialogViewModel(this,
                        LocalizationProvider.Instance.copy_live_settings_question,
                        LocalizationProvider.Instance.yes,
                        LocalizationProvider.Instance.no);

                    var confirmCopySettings = await DialogHost.Show(viewModel);

                    if (confirmCopySettings is bool and true)
                    {
                        var settingsVm = new SettingsViewModel(this);
                        await settingsVm.ResetGameSettingsCommand();
                    }
                    
                    LauncherSettingsProvider.Instance.SaveSettings();
                });
                */
            }

            // Item 001 — laser (linha vermelha) só sobre o painel nas telas de auth.
            Router.CurrentViewModel.Subscribe(vm =>
                LaserMargin = vm is LoginViewModel or RegisterViewModel ? new Thickness(690, 0, 0, 0) : default);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Router.Navigate.Execute(new ConnectServerViewModel(this));
            });
        }

        public void CloseCommand()
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.MainWindow.Close();
            }
        }

        public void MinimizeCommand()
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.MainWindow.WindowState = Avalonia.Controls.WindowState.Minimized;
            }
        }

        public void GoToSettingsCommand()
        {
            // Já em Configurações? Não empilhar outra por cima (o gear fica visível na tela de conexão,
            // mas se o usuário já está em Settings o clique deve ser no-op).
            int count = Router.NavigationStack.Count;
            if (count > 0 && Router.NavigationStack[count - 1] is SettingsViewModel)
            {
                return;
            }

            LauncherSettingsProvider.Instance.AllowSettings = false;

            Router.Navigate.Execute(new SettingsViewModel(this));
        }
    }
}
