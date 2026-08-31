using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Helpers
{
    public sealed class ServerHeartbeatMonitor : IDisposable
    {
        private static readonly Lazy<ServerHeartbeatMonitor> _instance =
            new Lazy<ServerHeartbeatMonitor>(() => new ServerHeartbeatMonitor());

        public static ServerHeartbeatMonitor Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cts;
        private Task _monitorTask;
        private bool _isServerOnline = true;
        private int _consecutiveFailures = 0;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

        public bool IsServerOnline => _isServerOnline;
        public bool IsMonitoring => _cts != null && !_cts.IsCancellationRequested;

        public event Action<bool> ServerStatusChanged;
        public event Action ServerReconnected;

        private ServerHeartbeatMonitor()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(4)
            };
        }

        public void Start(string customUrl = null)
        {
            if (IsMonitoring)
                return;

            _cts = new CancellationTokenSource();
            _monitorTask = Task.Run(() => MonitorLoopAsync(_cts.Token));
            LogManager.Instance.Info("[Heartbeat] Monitor de servidor iniciado (intervalo: 15s).");
        }

        public void Stop()
        {
            if (!IsMonitoring)
                return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            LogManager.Instance.Info("[Heartbeat] Monitor de servidor finalizado.");
        }

        public ServerBandwidthStatus CurrentBandwidthStatus { get; private set; }
        public event Action<ServerBandwidthStatus> BandwidthStatusChanged;

        private async Task MonitorLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, ct);
                    if (ct.IsCancellationRequested) break;

                    bool currentStatus = await PingServerCoreAsync();
                    UpdateStatus(currentStatus);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[Heartbeat] Exceção no ciclo de monitoramento: {ex.Message}");
                    UpdateStatus(false);
                }
            }
        }

        public async Task<ServerBandwidthStatus> FetchBandwidthStatusAsync()
        {
            string url = ServerManager.SelectedServer?.backendUrl ?? LauncherSettingsProvider.Instance.Server?.Url;
            if (string.IsNullOrWhiteSpace(url)) return null;

            try
            {
                string statusUrl = $"{url.TrimEnd('/')}{RequestHandler.ModRoutePrefix}/redline/server/bandwidth-status";
                using var res = await _httpClient.GetAsync(statusUrl);
                if (res.IsSuccessStatusCode)
                {
                    string json = await res.Content.ReadAsStringAsync();
                    var status = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerBandwidthStatus>(json);
                    if (status != null)
                    {
                        CurrentBandwidthStatus = status;
                        BandwidthStatusChanged?.Invoke(status);
                        return status;
                    }
                }
            }
            catch
            {
                // Silencioso se rota não responder
            }

            return null;
        }

        public async Task<bool> CheckNowAsync()
        {
            bool online = await PingServerCoreAsync();
            UpdateStatus(online);
            return online;
        }

        private async Task<bool> PingServerCoreAsync()
        {
            string url = ServerManager.SelectedServer?.backendUrl ?? LauncherSettingsProvider.Instance.Server?.Url;
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                string pingUrl = url.TrimEnd('/') + "/launcher/ping";
                using var response = await _httpClient.GetAsync(pingUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Fallback para raiz do servidor se /launcher/ping não responder
                try
                {
                    using var response = await _httpClient.GetAsync(url.TrimEnd('/'));
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }

        private void UpdateStatus(bool online)
        {
            bool previous = _isServerOnline;
            _isServerOnline = online;

            if (online)
            {
                _consecutiveFailures = 0;
                if (!previous)
                {
                    LogManager.Instance.Info("[Heartbeat] SERVIDOR VOLTOU ONLINE! Disparando evento ServerReconnected.");
                    ServerStatusChanged?.Invoke(true);
                    ServerReconnected?.Invoke();
                }
            }
            else
            {
                _consecutiveFailures++;
                if (previous)
                {
                    LogManager.Instance.Warning($"[Heartbeat] SERVIDOR FICOU OFFLINE (falhas: {_consecutiveFailures}).");
                    ServerStatusChanged?.Invoke(false);
                }
            }
        }

        public async Task<bool> WaitForServerOnlineAsync(TimeSpan timeout, CancellationToken ct = default)
        {
            if (_isServerOnline)
            {
                // Confirmação rápida
                if (await CheckNowAsync())
                    return true;
            }

            var stopwatch = Stopwatch.StartNew();
            LogManager.Instance.Info($"[Heartbeat] Aguardando servidor voltar online (timeout: {timeout.TotalSeconds}s)...");

            while (stopwatch.Elapsed < timeout && !ct.IsCancellationRequested)
            {
                await Task.Delay(1500, ct);
                if (await PingServerCoreAsync())
                {
                    UpdateStatus(true);
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Stop();
            _httpClient?.Dispose();
        }
    }
}
