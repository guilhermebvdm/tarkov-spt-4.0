using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ReactiveUI;
using SPT.Launcher.Controllers;
using SPT.Launcher.MiniCommon;

namespace SPT.Launcher.Models
{
    /// <summary>
    /// One clickable indicator (dot) of the background carousel.
    /// </summary>
    public class CarouselDot : ReactiveObject
    {
        public int Index { get; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        public ReactiveCommand<Unit, Unit> SelectCommand { get; }

        public CarouselDot(int index, Action<int> onSelect)
        {
            Index = index;
            SelectCommand = ReactiveCommand.Create(() => onSelect(index));
        }
    }

    /// <summary>
    /// Rotating background for the main (Profile) screen. Sources are, in order:
    /// (1) images cached under <c>Image_Cache/bg/</c> (server-provided, .png/.jpg/.jpeg) and
    /// (2) the bundled fallbacks <c>Assets/Backgrounds/bg_01..03.jpg</c>. Both are united and
    /// deduped by file name — when the folder is empty/missing only the bundled ones remain.
    /// Advances every 10s (wrap-around); the dots jump to a specific image and reset the timer.
    /// Images are decoded lazily (only when first shown) and capped in width to bound memory.
    /// </summary>
    public sealed class BackgroundCarousel : ReactiveObject, IDisposable
    {
        // Cap decode width: the hero fills a column (window minus sidebar) and is zoomed 1.12x.
        // 2560 keeps fidelity on large displays while avoiding full-res decodes of multi-MB art.
        private const int DecodeWidth = 2560;
        private const double IntervalSeconds = 10;

        private List<string> _descriptors;
        private Bitmap[] _decoded;
        private readonly object _lock = new object();

        private DispatcherTimer _timer;
        private EventHandler _tickHandler;
        private int _currentIndex;
        private bool _disposed;

        public ObservableCollection<CarouselDot> Dots { get; } = new ObservableCollection<CarouselDot>();

        /// <summary>True when there is more than one image (drives the dots + timer).</summary>
        public bool HasMultiple => _descriptors.Count > 1;

        private Bitmap _currentBackground;
        public Bitmap CurrentBackground
        {
            get => _currentBackground;
            private set => this.RaiseAndSetIfChanged(ref _currentBackground, value);
        }

        public BackgroundCarousel()
        {
            _descriptors = BuildDescriptors();
            _decoded = new Bitmap[_descriptors.Count];

            for (int i = 0; i < _descriptors.Count; i++)
            {
                int captured = i;
                Dots.Add(new CarouselDot(captured, Select) { IsActive = i == 0 });
            }

            // Seed the first image synchronously so the view never paints a blank hero.
            if (_descriptors.Count > 0)
            {
                _currentIndex = 0;
                Bitmap first = GetDecoded(0);
                if (first != null)
                {
                    CurrentBackground = first;
                }
            }
        }

        /// <summary>Starts auto-advancing (no-op with ≤1 image). Safe to call on re-activation.</summary>
        public void Start()
        {
            if (_disposed || !HasMultiple) return;

            if (_timer == null)
            {
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(IntervalSeconds) };
                _tickHandler = (_, _) => Advance();
                _timer.Tick += _tickHandler;
            }

            _timer.Start();
        }

        /// <summary>Stops auto-advancing. A stopped DispatcherTimer is not rooted, so no leak.</summary>
        public void Stop() => _timer?.Stop();

        private void Advance()
        {
            if (_disposed || _descriptors.Count <= 1) return;

            int next = (_currentIndex + 1) % _descriptors.Count;
            ShowIndex(next, restartTimer: false);
        }

        /// <summary>Dot click: jump to <paramref name="index"/> and restart the interval.</summary>
        public void Select(int index)
        {
            if (_disposed || index < 0 || index >= _descriptors.Count) return;

            ShowIndex(index, restartTimer: true);
        }

        private void ShowIndex(int index, bool restartTimer)
        {
            if (_disposed) return;

            _currentIndex = index;

            for (int i = 0; i < Dots.Count; i++)
            {
                Dots[i].IsActive = i == index;
            }

            if (restartTimer && _timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }

            Bitmap cached;
            lock (_lock)
            {
                cached = _decoded[index];
            }

            if (cached != null)
            {
                CurrentBackground = cached;
                return;
            }

            // First view of this image: decode off the UI thread to avoid a hitch.
            _ = DecodeAndSetAsync(index);
        }

        private async Task DecodeAndSetAsync(int index)
        {
            Bitmap bmp = await Task.Run(() => GetDecoded(index)).ConfigureAwait(true);

            if (_disposed || bmp == null) return;

            // Only publish if the user hasn't moved on to another image meanwhile.
            if (_currentIndex == index)
            {
                CurrentBackground = bmp;
            }
        }

        private Bitmap GetDecoded(int index)
        {
            lock (_lock)
            {
                if (_decoded[index] != null) return _decoded[index];
            }

            Bitmap bmp = DecodeDescriptor(_descriptors[index]);

            lock (_lock)
            {
                if (_decoded[index] == null)
                {
                    _decoded[index] = bmp;
                }

                return _decoded[index];
            }
        }

        private static Bitmap DecodeDescriptor(string descriptor)
        {
            try
            {
                if (descriptor.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
                {
                    // avares scheme is registered by the running Avalonia app, so the Uri parses.
                    using Stream assetStream = AssetLoader.Open(new Uri(descriptor));
                    return Bitmap.DecodeToWidth(assetStream, DecodeWidth);
                }

                using FileStream fileStream = File.OpenRead(descriptor);
                return Bitmap.DecodeToWidth(fileStream, DecodeWidth);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Carousel] Falha ao decodificar '{descriptor}': {ex.Message}");
                return null;
            }
        }

        private void DisposeDecodedBitmaps()
        {
            lock (_lock)
            {
                if (_decoded != null)
                {
                    for (int i = 0; i < _decoded.Length; i++)
                    {
                        try
                        {
                            _decoded[i]?.Dispose();
                        }
                        catch { }
                        _decoded[i] = null;
                    }
                }
            }
        }

        public void Reload()
        {
            DisposeDecodedBitmaps();
            var newDescriptors = BuildDescriptors();
            _descriptors = newDescriptors;
            _decoded = new Bitmap[_descriptors.Count];

            Dots.Clear();
            for (int i = 0; i < _descriptors.Count; i++)
            {
                int captured = i;
                Dots.Add(new CarouselDot(captured, Select) { IsActive = i == 0 });
            }

            this.RaisePropertyChanged(nameof(HasMultiple));

            if (_descriptors.Count > 0)
            {
                _currentIndex = 0;
                Bitmap first = GetDecoded(0);
                if (first != null)
                {
                    CurrentBackground = first;
                }
                Start();
            }
            else
            {
                CurrentBackground = null;
                Stop();
            }
        }

        /// <summary>
        /// Scans cached image folders (carrocel/ and legacy bg/).
        /// Supports 0, 1 or N images dynamically. Never throws.
        /// </summary>
        private static List<string> BuildDescriptors()
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg" };

            string[] searchFolders = new[]
            {
                Path.Combine(ImageRequest.ImageCacheFolder, "carrocel"),
                Path.Combine(ImageRequest.ImageCacheFolder, "bg")
            };

            foreach (string folder in searchFolders)
            {
                try
                {
                    if (Directory.Exists(folder))
                    {
                        IEnumerable<string> files = Directory
                            .EnumerateFiles(folder)
                            .Where(f => validExtensions.Contains(Path.GetExtension(f)))
                            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase);

                        foreach (string file in files)
                        {
                            if (seen.Add(Path.GetFileName(file)))
                            {
                                result.Add(file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Warning($"[Carousel] Falha ao varrer a pasta {folder}: {ex.Message}");
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            DisposeDecodedBitmaps();

            if (_timer != null)
            {
                _timer.Stop();

                if (_tickHandler != null)
                {
                    _timer.Tick -= _tickHandler;
                }

                _timer = null;
            }
        }
    }
}
