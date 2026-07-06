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

        /// <summary>
        /// Item 022 (Grupo C / RN-4) — ConnectServer roda em Task.Run (pool) porque tem chamadas
        /// bloqueantes (PingServer, LoadDefaultServerAsync, TailscaleHelper). O modelo de notificação
        /// (ConnectServerModel / LauncherSettingsProvider) NÃO marshala sozinho, então toda escrita
        /// de propriedade bound tem de ir para a UI thread. Post = fire-and-forget (não bloqueia a
        /// pool nem arrisca deadlock — NUNCA usar InvokeAsync(...).Wait() aqui, R4). Escritas em
        /// Server.Url NÃO passam por aqui: não são bound e são lidas de volta de forma síncrona logo
        /// adiante (marshalar criaria corrida de leitura).
        /// </summary>
        private static void OnUi(Action action) => Dispatcher.UIThread.Post(action);

        public async Task ConnectServer()
        {
            try
            {
                OnUi(() => LauncherSettingsProvider.Instance.AllowSettings = false);
                LogManager.Instance.Info("[Connect] Iniciando conexão ao servidor...");

                // 1. Obtém a URL oficial do servidor via Pastebin (precisa ser antes da VPN para sabermos para onde mandar o registro)
                if (!LauncherSettingsProvider.Instance.IsDevMode)
                {
                    OnUi(() => connectModel.InfoText = "Obtendo URL do servidor...");
                    try
                    {
                        using var client = new System.Net.Http.HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(10);
                        string pastebinUrl = await client.GetStringAsync("https://gist.githubusercontent.com/rockettechnology-dev/1fe6a8a243ea568c07e46a84744dff41/raw/gistfile1.txt");
                        if (!string.IsNullOrWhiteSpace(pastebinUrl))
                        {
                            LauncherSettingsProvider.Instance.Server.Url = pastebinUrl.Trim();
                            LogManager.Instance.Info($"[Connect] URL obtida do Gist: {LauncherSettingsProvider.Instance.Server.Url}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Warning($"[Connect] Falha ao obter URL do Pastebin, usando fallback embutido: {ex.Message}");
                        LauncherSettingsProvider.Instance.Server.Url = "https://100.106.152.7:6969";
                    }
                }
                else
                {
                    LogManager.Instance.Info($"[Connect] Dev Mode Ativo: Ignorando Pastebin e usando URL atual: {LauncherSettingsProvider.Instance.Server.Url}");
                }

                // Automação do Tailscale: Verifica, instala, autentica e configura Fika (Apenas se o Path do jogo for válido)
                if (!string.IsNullOrEmpty(LauncherSettingsProvider.Instance.GamePath))
                {
                    OnUi(() => connectModel.InfoText = "Conectando na rede P2P (Tailscale)...");
                    LogManager.Instance.Info("[Connect] Verificando e conectando Tailscale...");
                    // Item 023 (Frente C / RN-7): resultado tipado para distinguir chave rejeitada de falha de rede.
                    TailscaleConnectResult tailscaleResult = await TailscaleHelper.EnsureTailscaleConnected();

                    if (tailscaleResult == TailscaleConnectResult.Connected)
                    {
                        OnUi(() => connectModel.InfoText = "Atualizando configurações de rede (Fika)...");
                        LogManager.Instance.Info("[Connect] Configurando IP do Fika...");
                        await TailscaleHelper.ConfigureFikaAsync(LauncherSettingsProvider.Instance.GamePath);
                    }
                    else if (LauncherSettingsProvider.Instance.IsDevMode)
                    {
                        // Dev Mode pode apontar para servidor local — VPN não é obrigatória; segue com aviso no log
                        LogManager.Instance.Warning("[Connect] Tailscale indisponível, mas Dev Mode ativo — prosseguindo sem VPN.");
                    }
                    else
                    {
                        // Falha de VPN é fatal fora do Dev Mode: sem IP Tailscale o servidor é inalcançável.
                        // Erro claro no launcher (nunca navegador) + botão de retry via ConnectionFailed.
                        // RN-7/CA-C1: chave rejeitada/esgotada tem mensagem específica (não é a internet do
                        // cliente); qualquer outra falha (rede/DNS/CLI ausente) mantém a mensagem genérica (CA-C2).
                        bool authKeyRejected = tailscaleResult == TailscaleConnectResult.AuthKeyRejected;
                        LogManager.Instance.Error($"[Connect] Falha ao autenticar/conectar no Tailscale (motivo={tailscaleResult}). Abortando conexão — ver logs acima.");
                        OnUi(() =>
                        {
                            connectModel.ConnectionFailed = true;
                            connectModel.InfoText = authKeyRejected
                                ? "Falha na rede P2P (Tailscale): a chave de acesso à rede foi rejeitada ou esgotada. Isso NÃO é problema da sua internet — avise o host para gerar/renovar a chave compartilhada."
                                : "Falha na rede P2P (Tailscale): não foi possível autenticar/conectar. Verifique sua internet e clique em tentar novamente.";
                            LauncherSettingsProvider.Instance.AllowSettings = true;
                        });
                        return;
                    }
                }

                if (!LauncherSettingsProvider.Instance.DisableUpdates)
                {
                    OnUi(() => connectModel.InfoText = "Verificando atualizações do launcher...");
                    LogManager.Instance.Info("[Connect] Verificando versão do launcher no servidor...");

                    // CC-6/C3: o Progress<int> é construído dentro do Task.Run, capturando o
                    // SynchronizationContext da pool (nulo) → o handler roda na pool. Marshalar as
                    // escritas bound via OnUi cobre isso independentemente do contexto capturado.
                    var progress = new Progress<int>(percent =>
                    {
                        OnUi(() =>
                        {
                            connectModel.IsDownloading = true;
                            connectModel.DownloadProgress = percent;
                            connectModel.InfoText = $"Baixando atualização do launcher... {percent}%";
                        });
                    });

                    // Item 018 (auto-update-security): resultado tipado de 3+1 estados. Só 'Restarting'
                    // interrompe o fluxo (o launcher vai reiniciar). 'VerificationFailed' é fail-closed —
                    // a versão atual segue JOGÁVEL, então mostramos um aviso não-bloqueante e CONTINUAMOS
                    // o login (CA-2/CA-3). 'UpToDate'/'NetworkError' seguem silenciosos (CA-5 / corner case).
                    UpdateOutcome updateOutcome = await LauncherUpdateHelper.CheckAndUpdateAsync(LauncherSettingsProvider.Instance.Server.Url, progress);
                    if (updateOutcome == UpdateOutcome.Restarting)
                    {
                        OnUi(() => connectModel.InfoText = "Atualizando o launcher! Reiniciando...");
                        return;
                    }
                    if (updateOutcome == UpdateOutcome.VerificationFailed)
                    {
                        LogManager.Instance.Error("[Connect] Atualização do launcher abortada: verificação de assinatura falhou (fail-closed). Seguindo na versão atual.");
                        OnUi(() =>
                        {
                            connectModel.IsDownloading = false;
                            connectModel.InfoText = "Atualização do launcher indisponível (verificação de segurança falhou). Seguindo na versão atual...";
                        });
                        // Pausa breve NÃO-bloqueante (rodamos na pool): sem ela a InfoText seria sobrescrita
                        // imediatamente pelo "server_connecting" abaixo e o aviso piscaria invisível. Só
                        // ocorre no caminho raro de fail-closed; o jogo segue jogável (CA-2).
                        await Task.Delay(2500);
                    }
                }
                else
                {
                    LogManager.Instance.Info("[Connect] Verificação de atualização bloqueada pelas configurações (Desativada).");
                }

                OnUi(() => connectModel.InfoText = LocalizationProvider.Instance.server_connecting);

                LogManager.Instance.Info($"[Connect] Carregando servidor: {LauncherSettingsProvider.Instance.Server.Url}");
                bool serverLoaded = await ServerManager.LoadDefaultServerAsync(LauncherSettingsProvider.Instance.Server.Url);
                
                // Retry automático — rede pode estar estabilizando após Tailscale reconnect
                if (!serverLoaded)
                {
                    LogManager.Instance.Warning("[Connect] Primeira tentativa falhou. Aguardando 2s e tentando novamente...");
                    OnUi(() => connectModel.InfoText = "Reconectando ao servidor...");
                    await Task.Delay(2000);
                    serverLoaded = await ServerManager.LoadDefaultServerAsync(LauncherSettingsProvider.Instance.Server.Url);
                }

                if (!serverLoaded)
                {
                    LogManager.Instance.Error("[Connect] Falha ao carregar servidor após 2 tentativas.");
                    OnUi(() =>
                    {
                        connectModel.ConnectionFailed = true;
                        connectModel.InfoText = string.Format(LocalizationProvider.Instance.server_unavailable_format_1,
                            LauncherSettingsProvider.Instance.Server.Name);
                        LauncherSettingsProvider.Instance.AllowSettings = true;
                    });
                    return;
                }
                
                LogManager.Instance.Info("[Connect] Servidor carregado com sucesso.");
                LogManager.Instance.Info("[Connect] Ping ao servidor...");
                bool connected = ServerManager.PingServer();
                LogManager.Instance.Info($"[Connect] Ping resultado: {(connected ? "OK" : "FALHOU")}");

                OnUi(() =>
                {
                    connectModel.ConnectionFailed = !connected;
                    connectModel.InfoText = connected ? LocalizationProvider.Instance.ok : string.Format(LocalizationProvider.Instance.server_unavailable_format_1, LauncherSettingsProvider.Instance.Server.Name);
                });

                if (connected)
                {
                    SPTVersion version = Locator.Current.GetService<SPTVersion>("sptversion");

                    version.ParseVersionInfo(ServerManager.GetVersion());
                    
                    LogManager.Instance.Info($"Connected to server: {ServerManager.SelectedServer.backendUrl} - SPT MatchingVersion: {version}");



                    LogManager.Instance.Info($"[Connect] Chamando NavigateTo LoginView...");
                    NavigateTo(new LoginViewModel(HostScreen, noAutoLogin));
                    LogManager.Instance.Info($"[Connect] Navegação disparada para LoginView.");
                }

                OnUi(() => LauncherSettingsProvider.Instance.AllowSettings = true);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[FATAL ERROR IN CONNECT SERVER]: {ex.Message}\n{ex.StackTrace}");
                OnUi(() =>
                {
                    connectModel.InfoText = "Erro Fatal Crítico: " + ex.Message;
                    connectModel.ConnectionFailed = true;
                });
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
