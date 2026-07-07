using System;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 016 — download-rate meter: deterministic bytes/elapsed math, short moving average,
    /// zero-time / cache-hit safety, and PT-BR decimal formatting (MB/s decimal, KB/s under 1 MB/s).
    /// </summary>
    public class DownloadRateMeterTests
    {
        [Fact]
        public void Single_sample_rate_is_bytes_over_seconds()
        {
            var meter = new DownloadRateMeter();
            meter.AddSample(1_000_000, TimeSpan.FromSeconds(1));

            Assert.True(meter.HasRate);
            Assert.Equal(1_000_000d, meter.BytesPerSecond, 3);
        }

        [Fact]
        public void Moving_average_is_sum_bytes_over_sum_seconds()
        {
            var meter = new DownloadRateMeter(windowSize: 5);
            meter.AddSample(2_000_000, TimeSpan.FromSeconds(1)); // 2 MB/s
            meter.AddSample(4_000_000, TimeSpan.FromSeconds(1)); // 4 MB/s

            // (2MB + 4MB) / (1s + 1s) = 3 MB/s — not the arithmetic mean of the per-file rates
            Assert.Equal(3_000_000d, meter.BytesPerSecond, 3);
        }

        [Fact]
        public void Window_evicts_oldest_samples()
        {
            var meter = new DownloadRateMeter(windowSize: 2);
            meter.AddSample(10_000_000, TimeSpan.FromSeconds(1)); // evicted
            meter.AddSample(1_000_000, TimeSpan.FromSeconds(1));
            meter.AddSample(3_000_000, TimeSpan.FromSeconds(1));

            // only the last two: (1MB + 3MB) / 2s = 2 MB/s
            Assert.Equal(2_000_000d, meter.BytesPerSecond, 3);
        }

        [Fact]
        public void Zero_elapsed_sample_is_ignored_no_division_by_zero()
        {
            var meter = new DownloadRateMeter();
            meter.AddSample(5_000_000, TimeSpan.Zero); // instantaneous file

            Assert.False(meter.HasRate);
            Assert.Equal(0d, meter.BytesPerSecond);
            Assert.Equal(string.Empty, meter.FormattedRate);
        }

        [Fact]
        public void Zero_byte_sample_cache_hit_is_ignored()
        {
            var meter = new DownloadRateMeter();
            meter.AddSample(0, TimeSpan.FromSeconds(1));

            Assert.False(meter.HasRate);
            Assert.Equal(0d, meter.BytesPerSecond);
        }

        [Fact]
        public void Reset_clears_all_samples()
        {
            var meter = new DownloadRateMeter();
            meter.AddSample(1_000_000, TimeSpan.FromSeconds(1));
            Assert.True(meter.HasRate);

            meter.Reset();
            Assert.False(meter.HasRate);
            Assert.Equal(0d, meter.BytesPerSecond);
        }

        [Theory]
        [InlineData(12_400_000, "12,4 MB/s")] // decimal MB, PT-BR comma
        [InlineData(1_000_000, "1,0 MB/s")]
        [InlineData(300_000, "300 KB/s")]     // under 1 MB/s → KB/s (no "0,0 MB/s")
        [InlineData(1_500, "2 KB/s")]         // rounds to nearest KB
        public void Format_uses_decimal_units_and_ptbr_separator(long bytesPerSecond, string expected)
        {
            Assert.Equal(expected, DownloadRateMeter.Format(bytesPerSecond));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Format_non_positive_is_empty(double bytesPerSecond)
        {
            Assert.Equal(string.Empty, DownloadRateMeter.Format(bytesPerSecond));
        }

        [Fact]
        public void Format_non_finite_is_empty()
        {
            Assert.Equal(string.Empty, DownloadRateMeter.Format(double.PositiveInfinity));
            Assert.Equal(string.Empty, DownloadRateMeter.Format(double.NaN));
        }
    }
}
