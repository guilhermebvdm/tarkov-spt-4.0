using System;
using System.Collections.Generic;
using System.Globalization;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Item 016: smoothed download-rate meter. Feed it (bytes, elapsed) samples at the download
    /// point; read a short moving-average rate = sum(bytes) / sum(seconds) over the last N samples.
    /// Decimal units (1 MB = 1_000_000 bytes). Pure and testable — no UI, no clock of its own.
    /// </summary>
    public sealed class DownloadRateMeter
    {
        // Custom formatter so the PT-BR decimal comma is used regardless of the host culture /
        // InvariantGlobalization mode (no CultureInfo lookup that could fail or vary).
        private static readonly NumberFormatInfo PtBrNumbers = new NumberFormatInfo
        {
            NumberDecimalSeparator = ",",
            NumberGroupSeparator = ".",
        };

        private readonly int _windowSize;
        private readonly Queue<(long bytes, double seconds)> _samples = new Queue<(long, double)>();
        private long _sumBytes;
        private double _sumSeconds;
        // Item 032 (CR-01-01): AddSample roda na thread do download; Snapshot no ticker da UI → lock.
        private readonly object _gate = new object();

        /// <param name="windowSize">How many recent samples are averaged together (clamped to >= 1).</param>
        public DownloadRateMeter(int windowSize = 5)
        {
            _windowSize = Math.Max(1, windowSize);
        }

        /// <summary>
        /// Records one completed transfer. Samples with non-positive bytes or elapsed are ignored —
        /// this covers cache hits (0 bytes) and instantaneous files (0 elapsed) without ever dividing
        /// by zero or producing an infinite rate.
        /// </summary>
        public void AddSample(long bytes, TimeSpan elapsed)
        {
            double seconds = elapsed.TotalSeconds;
            if (bytes <= 0 || seconds <= 0)
            {
                return;
            }

            lock (_gate)
            {
                _samples.Enqueue((bytes, seconds));
                _sumBytes += bytes;
                _sumSeconds += seconds;

                while (_samples.Count > _windowSize)
                {
                    var evicted = _samples.Dequeue();
                    _sumBytes -= evicted.bytes;
                    _sumSeconds -= evicted.seconds;
                }
            }
        }

        /// <summary>True once at least one usable sample has been recorded.</summary>
        public bool HasRate => _sumSeconds > 0 && _sumBytes > 0;

        /// <summary>Smoothed transfer rate in bytes per second (0 when there is no usable sample yet).</summary>
        public double BytesPerSecond => HasRate ? _sumBytes / _sumSeconds : 0d;

        /// <summary>Current rate already formatted for display (empty string when there is no rate).</summary>
        public string FormattedRate => Format(BytesPerSecond);

        /// <summary>Clears every sample — call at the start of a new sync/verification run.</summary>
        public void Reset()
        {
            lock (_gate)
            {
                _samples.Clear();
                _sumBytes = 0;
                _sumSeconds = 0;
            }
        }

        /// <summary>
        /// Item 032 (CR-01-01): leitura ATÔMICA para o ticker da UI (thread diferente do AddSample) —
        /// o par (tem-taxa, texto formatado) sob um único lock, evitando torn read / somas incoerentes.
        /// </summary>
        public (bool has, string formatted) Snapshot()
        {
            lock (_gate)
            {
                bool has = _sumSeconds > 0 && _sumBytes > 0;
                return (has, has ? Format(_sumBytes / _sumSeconds) : string.Empty);
            }
        }

        /// <summary>
        /// Formats a bytes/second rate for display with the PT-BR decimal separator (comma). Headline
        /// unit is MB/s (decimal, 1e6); rates under 1 MB/s fall back to KB/s (decimal, 1e3) so slow
        /// links never read "0,0 MB/s". Non-positive / non-finite input → empty string (the caller
        /// hides the label: cache hit or nothing downloaded).
        /// </summary>
        public static string Format(double bytesPerSecond)
        {
            if (bytesPerSecond <= 0 || double.IsNaN(bytesPerSecond) || double.IsInfinity(bytesPerSecond))
            {
                return string.Empty;
            }

            double megabytesPerSecond = bytesPerSecond / 1_000_000d;
            if (megabytesPerSecond >= 1d)
            {
                return megabytesPerSecond.ToString("0.0", PtBrNumbers) + " MB/s";
            }

            double kilobytesPerSecond = bytesPerSecond / 1_000d;
            return kilobytesPerSecond.ToString("0", PtBrNumbers) + " KB/s";
        }
    }
}
