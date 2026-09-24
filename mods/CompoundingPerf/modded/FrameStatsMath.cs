// Compile-included by both server (net9.0, for unit tests) and client (net471, for the
// in-raid recorder). Pure math — no Unity, no SPT types.

namespace CompoundingPerf;

/// <summary>
/// Frame-time statistics for benchmark dumps. All inputs are per-frame durations in
/// SECONDS (Unity's unscaledDeltaTime); outputs use FPS and milliseconds, matching how
/// benchmarking tools report (avg FPS, 1% low FPS, hitch counts).
/// </summary>
public static class FrameStatsMath
{
    // Plain class, not a positional record — this file compile-includes into the net471
    // client, which lacks the IsExternalInit type that record init-setters require.
    public sealed class FrameStatsResult
    {
        public FrameStatsResult(
            int frames, double durationSec, double avgFps, double onePercentLowFps,
            double pointOnePercentLowFps, int hitchesOver50Ms, int hitchesOver100Ms,
            double maxFrameMs, double maxFrameAtSec)
        {
            Frames = frames;
            DurationSec = durationSec;
            AvgFps = avgFps;
            OnePercentLowFps = onePercentLowFps;
            PointOnePercentLowFps = pointOnePercentLowFps;
            HitchesOver50Ms = hitchesOver50Ms;
            HitchesOver100Ms = hitchesOver100Ms;
            MaxFrameMs = maxFrameMs;
            MaxFrameAtSec = maxFrameAtSec;
        }

        public int    Frames                { get; }
        public double DurationSec           { get; }
        public double AvgFps                { get; }
        public double OnePercentLowFps      { get; }
        public double PointOnePercentLowFps { get; }
        public int    HitchesOver50Ms       { get; }
        public int    HitchesOver100Ms      { get; }
        public double MaxFrameMs            { get; }

        /// <summary>Seconds from recording start (warmup included) to the start of the
        /// worst frame. A value near warmup+DurationSec means the spike is the
        /// end-of-raid teardown, not a mid-raid freeze.</summary>
        public double MaxFrameAtSec         { get; }
    }

    /// <summary>
    /// Compute summary stats over per-frame durations (seconds). Returns null when there
    /// are too few samples to be meaningful (under 2 seconds' worth at 30fps).
    /// <paramref name="warmupSkipSeconds"/> discards the leading samples covering that
    /// much wall time — the spawn-in/asset-streaming window produces multi-second frames
    /// that say nothing about gameplay performance and would dominate the lows.
    /// </summary>
    public static FrameStatsResult? Compute(float[] frameSeconds, int count, double warmupSkipSeconds = 0)
    {
        if (frameSeconds is null)
        {
            return null;
        }

        // Trim warmup: advance past leading frames until we've consumed the skip window.
        var start = 0;
        if (warmupSkipSeconds > 0)
        {
            double consumed = 0;
            while (start < count && consumed < warmupSkipSeconds)
            {
                consumed += frameSeconds[start];
                start++;
            }
        }

        if (count - start < 60)
        {
            return null;
        }

        // Re-sum the warmup window so MaxFrameAtSec is an offset from recording start,
        // which matches how a human remembers the raid ("it froze ~2 minutes in").
        double warmupConsumed = 0;
        for (var i = 0; i < start; i++)
        {
            warmupConsumed += frameSeconds[i];
        }

        double total = 0;
        var hitch50 = 0;
        var hitch100 = 0;
        double maxSec = 0;
        double maxAtSec = warmupConsumed;
        for (var i = start; i < count; i++)
        {
            var s = frameSeconds[i];
            if (s > 0.100) { hitch100++; hitch50++; }
            else if (s > 0.050) { hitch50++; }
            if (s > maxSec) { maxSec = s; maxAtSec = warmupConsumed + total; }
            total += s;
        }

        if (total <= 0)
        {
            return null;
        }

        // Sort a copy descending to find the worst tails. "1% low FPS" is the standard
        // benchmark metric: the average frame rate across the slowest 1% of frames.
        var kept = count - start;
        var sorted = new float[kept];
        Array.Copy(frameSeconds, start, sorted, 0, kept);
        Array.Sort(sorted);
        Array.Reverse(sorted); // index 0 = slowest frame

        return new FrameStatsResult(
            kept,
            total,
            kept / total,
            TailFps(sorted, kept, 0.01),
            TailFps(sorted, kept, 0.001),
            hitch50,
            hitch100,
            maxSec * 1000.0,
            maxAtSec);
    }

    /// <summary>Average FPS across the slowest <paramref name="fraction"/> of frames.</summary>
    private static double TailFps(float[] sortedDesc, int count, double fraction)
    {
        var n = Math.Max(1, (int)(count * fraction));
        double sum = 0;
        for (var i = 0; i < n; i++)
        {
            sum += sortedDesc[i];
        }
        return n / sum;
    }
}
