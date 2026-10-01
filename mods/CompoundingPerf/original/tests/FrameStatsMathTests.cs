using Xunit;

namespace CompoundingPerf.Tests;

public class FrameStatsMathTests
{
    [Fact]
    public void Too_few_samples_returns_null()
    {
        Assert.Null(FrameStatsMath.Compute(new float[10], 10));
        Assert.Null(FrameStatsMath.Compute(null!, 0));
    }

    [Fact]
    public void Steady_60fps_computes_correctly()
    {
        var frames = new float[600];
        Array.Fill(frames, 1f / 60f); // 10 seconds at exactly 60fps

        var s = FrameStatsMath.Compute(frames, 600)!;
        Assert.Equal(600, s.Frames);
        Assert.Equal(10.0, s.DurationSec, 1);
        Assert.Equal(60.0, s.AvgFps, 0);
        Assert.Equal(60.0, s.OnePercentLowFps, 0); // uniform — lows equal average
        Assert.Equal(0, s.HitchesOver50Ms);
        Assert.Equal(0, s.HitchesOver100Ms);
    }

    [Fact]
    public void Hitches_are_counted_and_lows_drop()
    {
        var frames = new float[1000];
        Array.Fill(frames, 1f / 100f);     // 100fps baseline
        frames[100] = 0.120f;              // one >100ms hitch
        frames[500] = 0.060f;              // one 50-100ms hitch

        var s = FrameStatsMath.Compute(frames, 1000)!;
        Assert.Equal(1, s.HitchesOver100Ms);
        Assert.Equal(2, s.HitchesOver50Ms);   // the 100ms one counts in both buckets
        Assert.Equal(120.0, s.MaxFrameMs, 0);
        Assert.True(s.OnePercentLowFps < s.AvgFps, "1% low must drop below average when hitches exist");
        Assert.True(s.PointOnePercentLowFps <= s.OnePercentLowFps);
    }

    [Fact]
    public void One_percent_low_is_average_of_worst_one_percent()
    {
        // 990 frames at 10ms + 10 frames at 50ms. Worst 1% (10 frames) are the 50ms ones.
        var frames = new float[1000];
        Array.Fill(frames, 0.010f);
        for (var i = 0; i < 10; i++) { frames[i * 17] = 0.050f; }

        var s = FrameStatsMath.Compute(frames, 1000)!;
        Assert.Equal(20.0, s.OnePercentLowFps, 0); // 1 / 0.050 = 20 fps
    }

    [Fact]
    public void Warmup_trim_excludes_loading_frames()
    {
        // 10 "loading" frames of 1s each, then 600 clean frames at 60fps.
        var frames = new float[610];
        for (var i = 0; i < 10; i++) { frames[i] = 1.0f; }
        for (var i = 10; i < 610; i++) { frames[i] = 1f / 60f; }

        // Without trim: lows wrecked by the 1s frames.
        var raw = FrameStatsMath.Compute(frames, 610)!;
        Assert.True(raw.OnePercentLowFps < 2);

        // With a 10s warmup trim, exactly the loading frames are dropped.
        var trimmed = FrameStatsMath.Compute(frames, 610, 10.0)!;
        Assert.Equal(600, trimmed.Frames);
        Assert.Equal(60.0, trimmed.AvgFps, 0);
        Assert.Equal(0, trimmed.HitchesOver100Ms);
    }

    [Fact]
    public void Warmup_trim_longer_than_raid_returns_null()
    {
        var frames = new float[120];
        Array.Fill(frames, 1f / 60f); // 2-second raid
        Assert.Null(FrameStatsMath.Compute(frames, 120, 60.0));
    }

    [Fact]
    public void Max_frame_position_is_reported_from_recording_start()
    {
        // 600 frames at 60fps with one 200ms spike 5 seconds in (frame index 300).
        var frames = new float[600];
        Array.Fill(frames, 1f / 60f);
        frames[300] = 0.200f;

        var s = FrameStatsMath.Compute(frames, 600)!;
        Assert.Equal(200.0, s.MaxFrameMs, 0);
        Assert.Equal(5.0, s.MaxFrameAtSec, 1); // 300 frames * (1/60)s before the spike

        // With a warmup trim, the offset still counts from recording start, not trim start:
        // 120 one-sixtieth frames = 2s warmup, spike unchanged at the same wall position.
        var trimmed = FrameStatsMath.Compute(frames, 600, 2.0)!;
        Assert.Equal(5.0, trimmed.MaxFrameAtSec, 1);
    }

    [Fact]
    public void Count_smaller_than_buffer_is_respected()
    {
        var frames = new float[10_000];
        Array.Fill(frames, 999f); // garbage beyond count must be ignored
        for (var i = 0; i < 120; i++) { frames[i] = 1f / 30f; }

        var s = FrameStatsMath.Compute(frames, 120)!;
        Assert.Equal(120, s.Frames);
        Assert.Equal(30.0, s.AvgFps, 0);
    }
}
