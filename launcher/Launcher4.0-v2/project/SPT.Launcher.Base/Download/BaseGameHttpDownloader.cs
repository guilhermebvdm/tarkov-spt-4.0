using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SPT.Launcher.Base.Helpers;
using SPT.Launcher.Controllers;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Download
{
    public class BaseGameManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("generatedAt")]
        public string GeneratedAt { get; set; }

        [JsonPropertyName("totalFiles")]
        public int TotalFiles { get; set; }

        [JsonPropertyName("totalBytes")]
        public long TotalBytes { get; set; }

        [JsonPropertyName("totalGigabytes")]
        public string TotalGigabytes { get; set; }

        [JsonPropertyName("files")]
        public List<BaseGameManifestFile> Files { get; set; } = new List<BaseGameManifestFile>();
    }

    public class BaseGameManifestFile
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }
    }

    /// <summary>
    /// Motor de download HTTP Multi-Thread direto da Cloudflare R2 para o jogo base de 64 GB.
    /// Suporta múltiplas conexões concorrentes, verificação prévia de arquivos locais, resume transparente e throttling de estado.
    /// </summary>
    public class BaseGameHttpDownloader : IDisposable
    {
        public const string DefaultPublicBaseUrl = "https://pub-ab477bb49e9646318e66742b8038b818.r2.dev/";
        public const string DefaultManifestUrl = "https://pub-ab477bb49e9646318e66742b8038b818.r2.dev/base-manifest.json";
        public const int DefaultConcurrency = 8;

        private readonly string _gamePath;
        private CancellationTokenSource _cts;
        private Timer _progressTicker;
        private readonly HttpClient _httpClient;

        private long _totalBytes;
        private long _downloadedBytes;
        private long _lastDownloadedBytesForSpeed;
        private DateTime _lastSpeedCheckTime;
        private double _currentSpeedBytesPerSec;
        private long _lastStateSaveTime = 0;

        private int _totalFileCount;
        private int _completedFileCount;
        private string _currentStatusText = "Iniciando...";
        private bool _isDownloading = false;
        private bool _isPaused = false;
        private bool _isCompleted = false;

        public event Action<BaseGameDownloadProgress> ProgressChanged;
        public event Action DownloadCompleted;
        public event Action<string> DownloadFailed;

        public bool IsDownloading => _isDownloading;
        public bool IsPaused => _isPaused;
        public bool IsCompleted => _isCompleted;

        public BaseGameHttpDownloader(string gamePath)
        {
            _gamePath = !string.IsNullOrWhiteSpace(gamePath) ? gamePath : AppDomain.CurrentDomain.BaseDirectory;
            
            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = 16,
                AutomaticDecompression = System.Net.DecompressionMethods.All
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<bool> InitializeAndStartAsync(string manifestUrl = null, string baseUrl = null)
        {
            try
            {
                manifestUrl ??= DefaultManifestUrl;
                baseUrl ??= DefaultPublicBaseUrl;
                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                _cts?.Dispose();
                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                _isDownloading = true;
                _isPaused = false;
                _isCompleted = false;

                LogManager.Instance.Info($"[HttpDownloader] Baixando manifesto da nuvem: {manifestUrl}");
                _currentStatusText = "Obtendo manifesto da nuvem Cloudflare R2...";
                NotifyProgress(0);

                // 1. Obter manifesto
                string manifestJson;
                try
                {
                    using var response = await _httpClient.GetAsync(manifestUrl, ct);
                    response.EnsureSuccessStatusCode();
                    manifestJson = await response.Content.ReadAsStringAsync(ct);
                }
                catch (Exception ex)
                {
                    LogError($"Falha ao obter manifesto da Cloudflare: {ex.Message}");
                    DownloadFailed?.Invoke($"Não foi possível conectar à Cloudflare R2: {ex.Message}");
                    _isDownloading = false;
                    return false;
                }

                BaseGameManifest manifest;
                try
                {
                    manifest = JsonSerializer.Deserialize<BaseGameManifest>(manifestJson);
                    if (manifest == null || manifest.Files == null || manifest.Files.Count == 0)
                    {
                        throw new InvalidOperationException("Manifesto vazio ou malformatado.");
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Manifesto inválido: {ex.Message}");
                    DownloadFailed?.Invoke("Manifesto da nuvem inválido.");
                    _isDownloading = false;
                    return false;
                }

                _totalBytes = manifest.TotalBytes;
                _totalFileCount = manifest.Files.Count;
                _downloadedBytes = 0;
                _completedFileCount = 0;

                LogManager.Instance.Info($"[HttpDownloader] Total no manifesto: {_totalFileCount} arquivos ({manifest.TotalGigabytes} GB). Verificando arquivos locais...");
                _currentStatusText = "Verificando arquivos locais já baixados...";
                NotifyProgress(0);

                // 2. Verificar arquivos locais existentes
                var downloadQueue = new List<BaseGameManifestFile>();

                foreach (var file in manifest.Files)
                {
                    string localFilePath = Path.Combine(_gamePath, file.Path.Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(localFilePath))
                    {
                        var info = new FileInfo(localFilePath);
                        if (info.Length == file.Size)
                        {
                            _downloadedBytes += file.Size;
                            _completedFileCount++;
                            continue;
                        }
                    }

                    downloadQueue.Add(file);
                }

                LogManager.Instance.Info($"[HttpDownloader] Verificação concluída: {_completedFileCount}/{_totalFileCount} arquivos prontos. Faltam {downloadQueue.Count} arquivos.");

                // Se já tiver tudo no disco
                if (downloadQueue.Count == 0)
                {
                    OnDownloadFinished();
                    return true;
                }

                // 3. Iniciar Ticker de Velocidade e Progresso
                _lastDownloadedBytesForSpeed = _downloadedBytes;
                _lastSpeedCheckTime = DateTime.UtcNow;
                _progressTicker?.Dispose();
                _progressTicker = new Timer(OnProgressTick, null, 500, 500);

                // 4. Iniciar Pool de Download Concorrente
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var semaphore = new SemaphoreSlim(DefaultConcurrency);
                        var tasks = new List<Task>();

                        foreach (var file in downloadQueue)
                        {
                            ct.ThrowIfCancellationRequested();
                            await semaphore.WaitAsync(ct);

                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await DownloadSingleFileAsync(file, baseUrl, ct);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }, ct));
                        }

                        await Task.WhenAll(tasks);

                        if (!ct.IsCancellationRequested)
                        {
                            OnDownloadFinished();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        LogManager.Instance.Info("[HttpDownloader] Download pausado pelo usuário.");
                        _isPaused = true;
                        _isDownloading = false;
                        _currentStatusText = "Download pausado.";
                        NotifyProgress(0);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Erro durante o download dos arquivos: {ex.Message}");
                        DownloadFailed?.Invoke($"Erro no download: {ex.Message}");
                        _isDownloading = false;
                    }
                }, ct);

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Falha ao inicializar HttpDownloader: {ex.Message}");
                DownloadFailed?.Invoke(ex.Message);
                _isDownloading = false;
                return false;
            }
        }

        private async Task DownloadSingleFileAsync(BaseGameManifestFile file, string baseUrl, CancellationToken ct)
        {
            string localFilePath = Path.Combine(_gamePath, file.Path.Replace('/', Path.DirectorySeparatorChar));
            string tempFilePath = localFilePath + ".tmp";
            string dir = Path.GetDirectoryName(localFilePath);

            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // Normalizar URL escapando caracteres especiais em cada segmento do caminho
            string[] segments = file.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string escapedPath = string.Join("/", Array.ConvertAll(segments, Uri.EscapeDataString));
            string fileUrl = baseUrl + escapedPath;

            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync(ct))
                    using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true))
                    {
                        var buffer = new byte[65536];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                        {
                            await fs.WriteAsync(buffer, 0, bytesRead, ct);
                            Interlocked.Add(ref _downloadedBytes, bytesRead);
                        }
                    }

                    // Mover atômico
                    if (File.Exists(localFilePath))
                    {
                        File.Delete(localFilePath);
                    }
                    File.Move(tempFilePath, localFilePath);

                    Interlocked.Increment(ref _completedFileCount);
                    _currentStatusText = $"Baixando arquivos ({_completedFileCount}/{_totalFileCount}): {Path.GetFileName(localFilePath)}";
                    return;
                }
                catch (OperationCanceledException)
                {
                    TryDelete(tempFilePath);
                    throw;
                }
                catch (Exception ex)
                {
                    TryDelete(tempFilePath);
                    if (attempt == maxRetries)
                    {
                        LogError($"Falha definitiva ao baixar {file.Path} após {maxRetries} tentativas: {ex.Message}");
                        throw;
                    }
                    await Task.Delay(1000 * attempt, ct);
                }
            }
        }

        private void OnProgressTick(object state)
        {
            if (!_isDownloading || _isPaused || _isCompleted) return;

            var now = DateTime.UtcNow;
            var elapsedSec = (now - _lastSpeedCheckTime).TotalSeconds;

            if (elapsedSec >= 0.5)
            {
                long currentDownloaded = Interlocked.Read(ref _downloadedBytes);
                long bytesDiff = currentDownloaded - _lastDownloadedBytesForSpeed;
                if (bytesDiff < 0) bytesDiff = 0;

                double instantSpeed = bytesDiff / elapsedSec;
                _currentSpeedBytesPerSec = _currentSpeedBytesPerSec > 0
                    ? (_currentSpeedBytesPerSec * 0.7) + (instantSpeed * 0.3)
                    : instantSpeed;

                _lastDownloadedBytesForSpeed = currentDownloaded;
                _lastSpeedCheckTime = now;

                NotifyProgress(_currentSpeedBytesPerSec);

                // Throttling de save de estado a cada 5s
                long currentUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (currentUnix - _lastStateSaveTime >= 5)
                {
                    _lastStateSaveTime = currentUnix;
                    double percent = _totalBytes > 0 ? (currentDownloaded / (double)_totalBytes) * 100.0 : 0;
                    GameStateDetector.SaveState(_gamePath, new BaseGameState
                    {
                        Status = "Downloading",
                        Completed = false,
                        ProgressPercentage = percent,
                        DownloadedBytes = currentDownloaded,
                        TotalBytes = _totalBytes,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }
        }

        private void NotifyProgress(double speed)
        {
            long downloaded = Interlocked.Read(ref _downloadedBytes);
            double percentage = _totalBytes > 0 ? (downloaded / (double)_totalBytes) * 100.0 : 0.0;
            if (percentage > 100.0) percentage = 100.0;

            TimeSpan? eta = null;
            if (speed > 1024)
            {
                long remainingBytes = _totalBytes - downloaded;
                if (remainingBytes > 0)
                {
                    double remainingSec = remainingBytes / speed;
                    if (remainingSec < 86400 * 7) // < 7 dias
                    {
                        eta = TimeSpan.FromSeconds(remainingSec);
                    }
                }
            }

            ProgressChanged?.Invoke(new BaseGameDownloadProgress
            {
                ProgressPercentage = percentage,
                DownloadSpeedBytesPerSec = speed,
                DownloadedBytes = downloaded,
                TotalBytes = _totalBytes,
                OpenConnections = DefaultConcurrency,
                IsWebSeedingActive = true,
                Eta = eta,
                StateDescription = _currentStatusText
            });
        }

        private void OnDownloadFinished()
        {
            _isDownloading = false;
            _isCompleted = true;
            _progressTicker?.Dispose();

            LogManager.Instance.Info("[HttpDownloader] Todos os arquivos do jogo base foram baixados com sucesso!");

            // Aplicar proteção Hidden | System no EscapeFromTarkov.exe
            ApplyGameExecutableProtection();

            // Marcar como instalado
            GameStateDetector.MarkAsInstalled(_gamePath);

            NotifyProgress(0);
            DownloadCompleted?.Invoke();
        }

        private void ApplyGameExecutableProtection()
        {
            try
            {
                string exePath = Path.Combine(_gamePath, "EscapeFromTarkov.exe");
                if (File.Exists(exePath))
                {
                    var attrs = File.GetAttributes(exePath);
                    if (!attrs.HasFlag(FileAttributes.Hidden) || !attrs.HasFlag(FileAttributes.System))
                    {
                        File.SetAttributes(exePath, attrs | FileAttributes.Hidden | FileAttributes.System);
                        LogManager.Instance.Info("[HttpDownloader] Proteção Hidden | System aplicada ao EscapeFromTarkov.exe");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[HttpDownloader] Não foi possível aplicar proteção no executável: {ex.Message}");
            }
        }

        public async Task PauseAsync()
        {
            if (!_isDownloading || _isPaused) return;

            _cts?.Cancel();
            _progressTicker?.Dispose();
            _progressTicker = null;
            _isPaused = true;
            _isDownloading = false;

            double percent = _totalBytes > 0 ? (Interlocked.Read(ref _downloadedBytes) / (double)_totalBytes) * 100.0 : 0;
            GameStateDetector.MarkAsPaused(_gamePath, percent);
            await Task.CompletedTask;
        }

        public async Task ResumeAsync()
        {
            if (_isDownloading) return;
            await InitializeAndStartAsync();
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }

        private static void LogError(string msg) =>
            LogManager.Instance.Error($"[HttpDownloader] {msg}");

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _progressTicker?.Dispose();
            _httpClient?.Dispose();
        }
    }
}
