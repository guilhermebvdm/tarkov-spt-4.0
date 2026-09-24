using CompoundingPerf.Telemetry;
using Xunit;

namespace CompoundingPerf.Tests;

public class TelemetryHubTests
{
    public TelemetryHubTests()
    {
        // Each test starts from a clean slate.
        TelemetryHub.Reset();
        TelemetryHub.TimingEnabled = false;
    }

    [Fact]
    public void Increment_starts_at_zero_and_accumulates()
    {
        Assert.Equal(0, TelemetryHub.Get("foo"));
        TelemetryHub.Increment("foo");
        TelemetryHub.Increment("foo", 5);
        Assert.Equal(6, TelemetryHub.Get("foo"));
    }

    [Fact]
    public void Time_records_when_timing_enabled_only()
    {
        TelemetryHub.TimingEnabled = false;
        var result = TelemetryHub.Time("k", () => 42);
        var (_, timings) = TelemetryHub.Snapshot();
        Assert.Equal(42, result);
        Assert.False(timings.ContainsKey("k"));

        TelemetryHub.TimingEnabled = true;
        TelemetryHub.Time("k", () => 7);
        var (_, timings2) = TelemetryHub.Snapshot();
        Assert.True(timings2.ContainsKey("k"));
        Assert.Equal(1, timings2["k"].n);
    }

    [Fact]
    public void Snapshot_is_an_independent_copy()
    {
        TelemetryHub.Increment("a", 3);
        var (counters, _) = TelemetryHub.Snapshot();
        TelemetryHub.Increment("a", 100);
        Assert.Equal(3, counters["a"]);
        Assert.Equal(103, TelemetryHub.Get("a"));
    }
}
