using SPT.Launcher.Models.SPT;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using SPT.Launcher.ViewModels.Dialogs;
using System.IO;
using System.Diagnostics;
using Avalonia.Threading;

namespace SPT.Launcher.ViewModels
{
    public class ConnectServerViewModel : ViewModelBase
    {
        private bool noAutoLogin = false;

        public ConnectServerModel connectModel { get; set; } = new ConnectServerModel()
        {
            InfoText = LocalizationProvider.Instance.server_connecting
        };

        public ConnectServerViewModel(IScreen Host, bool NoAutoLogin = false) : base(Host)
        {
            noAutoLogin = NoAutoLogin;

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                Task.Run(async () =>
                {
                   await ConnectServer();
                });
            });
        }

        public async Task ConnectServer()
        {
            try
            {
                LauncherSettingsProvider.Instance.AllowSettings = false;
                LogManager.Instance.Info("[Connect] Iniciando conexão ao servidor...");

                // 1. Obtém a URL oficial do servidor via Pastebin (precisa ser antes da VPN para sabermos para onde mandar o registro)
                if (!LauncherSettingsProvider.Instance.IsDevMode)
                {
                    connectModel.InfoText = "Obtendo URL do servidor...";
                    try
                    {
                        using var client = new System.Net.Http.HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(10);
                        string pastebinUrl = await client.GetStringAsync("https://pastebin.com/raw/PT4cMwLB");
                        if (!string.IsNullOrWhiteSpace(pastebinUrl))
                        {
                            LauncherSettingsProvider.Instance.Server.Url = pastebinUrl.Trim();
                            LogManager.Instance.Info($"[Connect] URL obtida do Pastebin: {LauncherSettingsProvider.Instance.Server.Url}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Warning($"[Connect] Falha ao obter URL do Pastebin, usando cache/fallback: {ex.Message}");
                    }
                }
                else
                {
                    LogManager.Instance.Info($"[Connect] Dev Mode Ativo: Ignorando Pastebin e usando URL atual: {LauncherSettingsProvider.Instance.Server.Url}");
                }

                // 2. Automação da VPN (WireGuard): Instala, gera chaves, conecta ao Hub e configura Fika
                if (!string.IsNullOrEmpty(LauncherSettingsProvider.Instance.GamePath))
                {
                    // Checar se Tailscale está instalado e sugerir desinstalação
                    if (File.Exists(@"C:\Program Files\Tailscale\tailscale.exe"))
                    {
                        var dialog = new ConfirmationDialogViewModel(null, "Detectamos que você ainda tem o TailScale VPN instalado. O Tarkov Red Line não utiliza mais este VPN.\n\nGostaria de desinstalá-lo agora para evitar conflitos de rede?");
                        var result = await Dispatcher.UIThread.InvokeAsync(async () => await ShowDialog(dialog));
                        
                        if (result is bool confirm && confirm)
                        {
                            connectModel.InfoText = "Desinstalando Tailscale...";
                            LogManager.Instance.Info("[Connect] Desinstalando Tailscale pelo Winget...");
                            try
                            {
                                var uninstallProc = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "powershell.exe",
                                        Arguments = "-Command \"winget uninstall Tailscale --silent --accept-source-agreements\"",
                                        UseShellExecute = true,
                                        Verb = "runas",
                                        CreateNoWindow = true
                                    }
                                };
                                uninstallProc.Start();
                                await uninstallProc.WaitForExitAsync();
                                LogManager.Instance.Info("[Connect] Comando de desinstalação do Tailscale concluído.");
                                
                                // Apagar atalhos de sobra (mesmo que falhe o winget)
                                try {
                                    foreach (var p in Process.GetProcessesByName("tailscale-ipn")) p.Kill();
                                    foreach (var p in Process.GetProcessesByName("tailscale")) p.Kill();
                                    
                                    string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                                    string tsShortcut = Path.Combine(startupFolder, "Tailscale.lnk");
                                    if (File.Exists(tsShortcut)) File.Delete(tsShortcut);
                                    
                                    string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                                    string commonTsShortcut = Path.Combine(commonStartup, "Tailscale.lnk");
                                    if (File.Exists(commonTsShortcut)) File.Delete(commonTsShortcut);
                                } catch { }
                            }
                            catch (Exception ex)
                            {
                                LogManager.Instance.Warning($"[Connect] Falha ao desinstalar Tailscale: {ex.Message}");
                            }
                        }
                    }

                    connectModel.InfoText = "Conectando na rede P2P (WireGuard)...";
                    LogManager.Instance.Info("[Connect] Verificando e conectando WireGuard...");
                    await WireGuardHelper.EnsureWireGuardConnected();

                    connectModel.InfoText = "Atualizando configurações de rede (Fika)...";
                    LogManager.Instance.Info("[Connect] Configurando IP do Fika...");
                    await WireGuardHelper.ConfigureFikaAsync(LauncherSettingsProvider.Instance.GamePath);
                }

                if (!LauncherSettingsProvider.Instance.DisableUpdates)
                {
                    connectModel.InfoText = "Verificando atualizações do launcher...";
                    LogManager.Instance.Info("[Connect] Verificando versão do launcher no servidor...");
                    
                    var progress = new Progress<int>(percent => 
                    {
                        connectModel.IsDownloading = true;
                        connectModel.DownloadProgress = percent;
                        connectModel.InfoText = $"Baixando atualização do launcher... {percent}%";
                    });

                    bool isUpdating = await LauncherUpdateHelper.CheckAndUpdateAsync(LauncherSettingsProvider.Instance.Server.Url, progress);
                    if (isUpdating)
                    {
                        connectModel.InfoText = "Atualizando o launcher! Reiniciando...";
                        return;
                    }
                }
                else
                {
                    LogManager.Instance.Info("[Connect] Verificação de atualização bloqueada pelas configurações (Desativada).");
                }

                connectModel.InfoText = LocalizationProvider.Instance.server_connecting;
                
                LogManager.Instance.Info($"[Connect] Carregando servidor: {LauncherSettingsProvider.Instance.Server.Url}");
                bool serverLoaded = await ServerManager.LoadDefaultServerAsync(LauncherSettingsProvider.Instance.Server.Url);
                
                // Retry automático — rede pode estar estabilizando após Tailscale reconnect
                if (!serverLoaded)
                {
                    LogManager.Instance.Warning("[Connect] Primeira tentativa falhou. Aguardando 2s e tentando novamente...");
                    connectModel.InfoText = "Reconectando ao servidor...";
                    await Task.Delay(2000);
                    serverLoaded = await ServerManager.LoadDefaultServerAsync(LauncherSettingsProvider.Instance.Server.Url);
                }

                if (!serverLoaded)
                {
                    LogManager.Instance.Error("[Connect] Falha ao carregar servidor após 2 tentativas.");
                    connectModel.ConnectionFailed = true;
                    connectModel.InfoText = string.Format(LocalizationProvider.Instance.server_unavailable_format_1,
                        LauncherSettingsProvider.Instance.Server.Name);
                    
                    LauncherSettingsProvider.Instance.AllowSettings = true;
                    return;
                }
                
                LogManager.Instance.Info("[Connect] Servidor carregado com sucesso.");
                LogManager.Instance.Info("[Connect] Ping ao servidor...");
                bool connected = ServerManager.PingServer();
                LogManager.Instance.Info($"[Connect] Ping resultado: {(connected ? "OK" : "FALHOU")}");

                connectModel.ConnectionFailed = !connected;

                connectModel.InfoText = connected ? LocalizationProvider.Instance.ok : string.Format(LocalizationProvider.Instance.server_unavailable_format_1, LauncherSettingsProvider.Instance.Server.Name);

                if (connected)
                {
                    SPTVersion version = Locator.Current.GetService<SPTVersion>("sptversion");

                    version.ParseVersionInfo(ServerManager.GetVersion());
                    
                    LogManager.Instance.Info($"Connected to server: {ServerManager.SelectedServer.backendUrl} - SPT MatchingVersion: {version}");



                    LogManager.Instance.Info($"[Connect] Chamando NavigateTo LoginView...");
                    NavigateTo(new LoginViewModel(HostScreen, noAutoLogin));
                    LogManager.Instance.Info($"[Connect] Navegação disparada para LoginView.");
                }
                
                LauncherSettingsProvider.Instance.AllowSettings = true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[FATAL ERROR IN CONNECT SERVER]: {ex.Message}\n{ex.StackTrace}");
                connectModel.InfoText = "Erro Fatal Crítico: " + ex.Message;
                connectModel.ConnectionFailed = true;
            }
        }

        public void RetryCommand()
        {
            connectModel.InfoText = LocalizationProvider.Instance.server_connecting;

            connectModel.ConnectionFailed = false;

            Task.Run(async () =>
            {
                await ConnectServer();
            });
        }
    }
}
