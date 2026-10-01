using CompoundingPerf.Telemetry;
using Xunit;

namespace CompoundingPerf.Tests;

public class BenchSampleMathTests
{
    private static BenchCounters Counters(double pauseMs, int g0 = 0, int g2 = 0, long saves = 0, long skips = 0) =>
        new() { TotalGcPauseMs = pauseMs, Gen0 = g0, Gen2 = g2, SavesExecuted = saves, SavesSkippedClean = skips };

    [Fact]
    public void Deltas_are_differences_between_snapshots()
    {
        var d = BenchSampleMath.Delta(
            Counters(1000, g0: 50, g2: 2, saves: 10, skips: 100),
            Counters(1450, g0: 65, g2: 3, saves: 12, skips: 160),
            30.0);

        Assert.Equal(450, d.GcPauseMs, 1);
        Assert.Equal(1.5, d.GcPausePct, 2); // 450ms of 30,000ms = 1.5%
        Assert.Equal(15, d.Gen0);
        Assert.Equal(1, d.Gen2);
        Assert.Equal(2, d.SavesExecuted);
        Assert.Equal(60, d.SavesSkippedClean);
    }

    [Fact]
    public void Counter_reset_clamps_to_zero_instead_of_negative()
    {
        var d = BenchSampleMath.Delta(Counters(5000, g0: 100), Counters(10, g0: 3), 30.0);
        Assert.Equal(0, d.GcPauseMs);
        Assert.Equal(0, d.Gen0);
    }

    [Fact]
    public void Zero_interval_yields_zero_percentage_not_NaN()
    {
        var d = BenchSampleMath.Delta(Counters(0), Counters(100), 0);
        Assert.Equal(0, d.GcPausePct);
    }
}
