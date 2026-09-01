using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MonoTorrent;
using MonoTorrent.Client;
using SPT.Launcher.Base.Helpers;
using SPT.Launcher.Controllers;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Download
{
    public class BaseGameDownloadProgress
    {
        public double ProgressPercentage { get; set; }
        public double DownloadSpeedBytesPerSec { get; set; }
        public long DownloadedBytes { get; set; }
        public long TotalBytes { get; set; }
        public int OpenConnections { get; set; }
        public bool IsWebSeedingActive { get; set; }
        public TimeSpan? Eta { get; set; }
        public string StateDescription { get; set; } = "Iniciando...";
    }

    public class BaseGameTorrentDownloader : IDisposable
    {
        private ClientEngine _engine;
        private TorrentManager _manager;
        private Timer _progressTicker;
        private Timer _bandwidthTicker;
        private CancellationTokenSource _cts;
        private readonly string _gamePath;
        private readonly string _torrentCacheDir;
        private bool _isPaused = false;
        private bool _isCompleted = false;
        private long _lastStateSaveTime = 0;

        public event Action<BaseGameDownloadProgress> ProgressChanged;
        public event Action DownloadCompleted;
        public event Action<string> DownloadFailed;

        public bool IsDownloading => _manager != null && _manager.State == TorrentState.Downloading;
        public bool IsPaused => _isPaused;
        public bool IsCompleted => _isCompleted;
        public TorrentState CurrentState => _manager?.State ?? TorrentState.Stopped;

        public BaseGameTorrentDownloader(string gamePath)
        {
            _gamePath = !string.IsNullOrWhiteSpace(gamePath) ? gamePath : AppDomain.CurrentDomain.BaseDirectory;
            _torrentCacheDir = Path.Combine(SptPathHelper.SptRootPath, "user", "launcher", "torrent-cache");

            if (!Directory.Exists(_torrentCacheDir))
            {
                Directory.CreateDirectory(_torrentCacheDir);
            }

            // Escutar reconexão do Heartbeat para autorefresh dos WebSeeds
            ServerHeartbeatMonitor.Instance.ServerReconnected += OnServerReconnected;
        }

        private async Task CheckBandwidthAsync()
        {
            if (_manager == null || _isPaused || _isCompleted) return;

            try
            {
                var status = await ServerHeartbeatMonitor.Instance.FetchBandwidthStatusAsync();
                if (status != null && status.MaxDownloadRateBytesSec > 0)
                {
                    if (_manager != null && _manager.Settings.MaximumDownloadRate != status.MaxDownloadRateBytesSec)
                    {
                        var newSettings = new TorrentSettingsBuilder(_manager.Settings)
                        {
                            MaximumDownloadRate = status.MaxDownloadRateBytesSec
                        }.ToSettings();

                        await _manager.UpdateSettingsAsync(newSettings);
                        LogManager.Instance.Info($"[TorrentDownloader] Limite adaptativo atualizado pelo servidor (QoS 60s): {status.Mode} ({status.MaxDownloadRateMBps} MB/s)");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[TorrentDownloader] Falha ao verificar status de banda: {ex.Message}");
            }
        }

        public async Task<bool> InitializeAndStartAsync(string torrentSource, string webSeedFallbackUrl = null)
        {
            try
            {
                _cts = new CancellationTokenSource();
                LogManager.Instance.Info($"[TorrentDownloader] Inicializando motor MonoTorrent para: {_gamePath}");

                // 1. Configurar EngineSettings
                var engineSettings = new EngineSettingsBuilder
                {
                    AllowPortForwarding = true,
                    AutoSaveLoadFastResume = true,
                    CacheDirectory = _torrentCacheDir,
                    MaximumConnections = 150,
                    MaximumHalfOpenConnections = 10,
                    MaximumDownloadRate = 0, // Ilimitado
                    MaximumUploadRate = 5 * 1024 * 1024 // 5 MB/s upload padrão
                }.ToSettings();

                var factories = Factories.Default.WithHttpClientCreator((address) => CreateBypassHttpClient());
                _engine = new ClientEngine(engineSettings, factories);

                // 2. Carregar o Torrent (com injeção dinâmica de WebSeed url-list)
                Torrent torrent = await LoadTorrentAsync(torrentSource, webSeedFallbackUrl);
                if (torrent == null)
                {
                    DownloadFailed?.Invoke("Não foi possível carregar os metadados do torrent do jogo base.");
                    return false;
                }

                // 3. Configurar TorrentSettings
                var torrentSettings = new TorrentSettingsBuilder
                {
                    MaximumConnections = 100
                }.ToSettings();

                // 4. Adicionar ao Engine
                _manager = await _engine.AddAsync(torrent, _gamePath, torrentSettings);

                // Mapear cada arquivo para salvar diretamente na raiz de _gamePath sem subpasta de torrent
                foreach (var file in _manager.Files)
                {
                    string relativePath = file.Path;
                    if (!string.IsNullOrEmpty(torrent.Name))
                    {
                        if (relativePath.StartsWith(torrent.Name + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                            relativePath.StartsWith(torrent.Name + "/", StringComparison.OrdinalIgnoreCase) ||
                            relativePath.StartsWith(torrent.Name + "\\", StringComparison.OrdinalIgnoreCase))
                        {
                            relativePath = relativePath.Substring(torrent.Name.Length + 1);
                        }
                    }

                    string targetFullPath = Path.GetFullPath(Path.Combine(_gamePath, relativePath));
                    if (!string.Equals(file.FullPath, targetFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        await _manager.MoveFileAsync(file, targetFullPath);
                    }
                }

                // Configurar eventos
                _manager.TorrentStateChanged += OnTorrentStateChanged;

                // 5. Iniciar Download
                await _manager.StartAsync();
                LogManager.Instance.Info("[TorrentDownloader] Download iniciado diretamente na raiz do jogo.");

                // 6. Iniciar Tickers de Progresso (500ms) e Limite de Banda QoS (60s)
                _progressTicker = new Timer(OnTickProgress, null, 500, 500);
                _bandwidthTicker = new Timer(async _ => await CheckBandwidthAsync(), null, 0, 60000);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[TorrentDownloader] Erro crítico ao iniciar download: {ex.Message}\n{ex.StackTrace}");
                DownloadFailed?.Invoke($"Erro ao iniciar download: {ex.Message}");
                return false;
            }
        }

        private static HttpClient CreateBypassHttpClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            return new HttpClient(handler);
        }

        private async Task<Torrent> LoadTorrentAsync(string source, string webSeedFallbackUrl)
        {
            byte[] data = null;

            if (File.Exists(source))
            {
                data = await File.ReadAllBytesAsync(source);
            }
            else if (Uri.TryCreate(source, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                using var client = CreateBypassHttpClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                data = await client.GetByteArrayAsync(uri);
            }
            else
            {
                string localFallback = Path.Combine(_gamePath, "base-game.torrent");
                if (File.Exists(localFallback))
                {
                    data = await File.ReadAllBytesAsync(localFallback);
                }
            }

            if (data == null || data.Length == 0)
            {
                return null;
            }

            // Injetar ou garantir url-list e httpseeds com o WebSeed do servidor
            if (!string.IsNullOrEmpty(webSeedFallbackUrl))
            {
                try
                {
                    string safeWebSeed = webSeedFallbackUrl.EndsWith("/") ? webSeedFallbackUrl : webSeedFallbackUrl + "/";
                    var dict = MonoTorrent.BEncoding.BEncodedValue.Decode<MonoTorrent.BEncoding.BEncodedDictionary>(data);
                    var list = new MonoTorrent.BEncoding.BEncodedList
                    {
                        new MonoTorrent.BEncoding.BEncodedString(safeWebSeed)
                    };
                    dict[new MonoTorrent.BEncoding.BEncodedString("url-list")] = list;
                    dict[new MonoTorrent.BEncoding.BEncodedString("httpseeds")] = list;
                    data = dict.Encode();
                    LogManager.Instance.Info($"[TorrentDownloader] WebSeed injetado com sucesso (url-list e httpseeds): {safeWebSeed}");
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[TorrentDownloader] Falha ao injetar WebSeed no torrent: {ex.Message}");
                }
            }

            return await Torrent.LoadAsync(data);
        }

        public async Task PauseAsync()
        {
            if (_manager == null || _isPaused) return;

            try
            {
                _bandwidthTicker?.Dispose();
                _bandwidthTicker = null;

                await _manager.PauseAsync();
                _isPaused = true;
                GameStateDetector.MarkAsPaused(_gamePath, _manager.Progress);
                LogManager.Instance.Info("[TorrentDownloader] Download pausado pelo usuário.");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[TorrentDownloader] Falha ao pausar: {ex.Message}");
            }
        }

        public async Task ResumeAsync()
        {
            if (_manager == null || !_isPaused) return;

            try
            {
                await _manager.StartAsync();
                _isPaused = false;
                _bandwidthTicker?.Dispose();
                _bandwidthTicker = new Timer(async _ => await CheckBandwidthAsync(), null, 0, 60000);
                LogManager.Instance.Info("[TorrentDownloader] Download retomado.");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[TorrentDownloader] Falha ao retomar: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            if (_manager == null) return;

            try
            {
                _progressTicker?.Dispose();
                _progressTicker = null;
                _bandwidthTicker?.Dispose();
                _bandwidthTicker = null;

                await _manager.StopAsync();
                LogManager.Instance.Info("[TorrentDownloader] Motor parado com sucesso.");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[TorrentDownloader] Erro ao parar: {ex.Message}");
            }
        }

        private void OnServerReconnected()
        {
            if (_manager != null && !_isCompleted && !_isPaused)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        LogManager.Instance.Info("[TorrentDownloader] Servidor reconectado. Forçando refresh dos WebSeeds...");
                        // Se o download estiver ativo, faz refresh de conexões de WebSeeds
                        if (_manager.State == TorrentState.Downloading || _manager.State == TorrentState.Starting)
                        {
                            await _manager.StartAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Instance.Warning($"[TorrentDownloader] Falha no refresh pós-reconexão: {ex.Message}");
                    }
                });
            }
        }

        private void OnTorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {
            LogManager.Instance.Info($"[TorrentDownloader] Estado mudou: {e.OldState} -> {e.NewState}");

            if (e.NewState == TorrentState.Error)
            {
                string errorReason = _manager?.Error?.Exception?.Message ?? "Erro desconhecido durante a checagem/download do torrent";
                LogManager.Instance.Error($"[TorrentDownloader] MonoTorrent entrou em estado de erro: {errorReason}\n{_manager?.Error?.Exception?.StackTrace}");
                DownloadFailed?.Invoke($"Erro no download: {errorReason}");
                return;
            }

            if (e.NewState == TorrentState.Seeding || (_manager != null && _manager.Progress >= 99.99))
            {
                if (!_isCompleted)
                {
                    _isCompleted = true;
                    _isPaused = false;
                    _progressTicker?.Dispose();
                    _progressTicker = null;
                    _bandwidthTicker?.Dispose();
                    _bandwidthTicker = null;

                    string hash = _manager.Torrent?.InfoHashes.V1OrV2.ToHex() ?? "";
                    long totalSize = _manager.Torrent?.Size ?? 0;

                    // Ocultar executável do jogo com flags de Sistema (Hidden + System)
                    string eftExe = Path.Combine(_gamePath, "EscapeFromTarkov.exe");
                    if (File.Exists(eftExe))
                    {
                        try
                        {
                            var currentAttrs = File.GetAttributes(eftExe);
                            File.SetAttributes(eftExe, currentAttrs | FileAttributes.Hidden | FileAttributes.System);
                            LogManager.Instance.Info("[TorrentDownloader] Atributos (Hidden + System) aplicados com sucesso em EscapeFromTarkov.exe.");
                        }
                        catch (Exception ex)
                        {
                            LogManager.Instance.Warning($"[TorrentDownloader] Falha ao aplicar atributos ao executável: {ex.Message}");
                        }
                    }

                    GameStateDetector.MarkAsInstalled(_gamePath, hash, totalSize);
                    DownloadCompleted?.Invoke();
                }
            }
        }

        private void OnTickProgress(object state)
        {
            if (_manager == null) return;

            try
            {
                double speed = _manager.Monitor.DownloadRate; // bytes/sec
                long downloaded = _manager.Monitor.DataBytesReceived;
                long total = _manager.Torrent?.Size ?? 0;
                double progress = _manager.Progress;
                int connections = _manager.OpenConnections;

                // Salvar estado em disco periodicamente (a cada 5s) durante o download ativo
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (now - _lastStateSaveTime >= 5000 && (_manager.State == TorrentState.Downloading || _manager.State == TorrentState.Starting))
                {
                    _lastStateSaveTime = now;
                    GameStateDetector.MarkAsDownloading(
                        _gamePath,
                        _manager.Torrent?.InfoHashes.V1OrV2.ToHex() ?? "",
                        progress,
                        downloaded,
                        total
                    );
                }

                TimeSpan? eta = null;
                if (speed > 1024 && total > downloaded)
                {
                    long remainingBytes = total - downloaded;
                    double secondsLeft = remainingBytes / speed;
                    if (secondsLeft > 0 && secondsLeft < 3600 * 24 * 7)
                    {
                        eta = TimeSpan.FromSeconds(secondsLeft);
                    }
                }

                string stateDesc = _manager.State switch
                {
                    TorrentState.Hashing => "Verificando integridade dos arquivos...",
                    TorrentState.Starting => "Conectando às fontes...",
                    TorrentState.Downloading => _isPaused ? "Pausado" : "Baixando arquivos do jogo...",
                    TorrentState.Seeding => "Download concluído!",
                    TorrentState.Paused => "Pausado",
                    _ => _manager.State.ToString()
                };

                if (!ServerHeartbeatMonitor.Instance.IsServerOnline && !_isPaused && progress < 100.0)
                {
                    stateDesc = "Servidor reiniciando — aguardando reconexão...";
                }

                ProgressChanged?.Invoke(new BaseGameDownloadProgress
                {
                    ProgressPercentage = progress,
                    DownloadSpeedBytesPerSec = speed,
                    DownloadedBytes = downloaded,
                    TotalBytes = total,
                    OpenConnections = connections,
                    IsWebSeedingActive = ServerHeartbeatMonitor.Instance.IsServerOnline,
                    Eta = eta,
                    StateDescription = stateDesc
                });
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[TorrentDownloader] Erro no ticker de progresso: {ex.Message}");
            }
        }

        public void Dispose()
        {
            ServerHeartbeatMonitor.Instance.ServerReconnected -= OnServerReconnected;
            _progressTicker?.Dispose();
            _bandwidthTicker?.Dispose();
            _engine?.Dispose();
        }
    }
}
