using System.Collections.Concurrent;
using CompoundingPerf.Features;
using SPTarkov.Server.Core.Utils;
using Xunit;

namespace CompoundingPerf.Tests;

/// <summary>
/// Pins S6's actual guarantee: many threads hammering the unsafe-Random methods of
/// <see cref="ThreadSafeRandomUtil"/> must not throw, must not return zero, must
/// produce reasonable distributions.
///
/// <para>Each test uses a fresh subclass instance because RandomUtil's ctor takes
/// SPT-specific DI types we can't easily mock — but the public surface we exercise
/// works regardless.</para>
/// </summary>
public class ThreadSafeRandomUtilTests
{
    private static ThreadSafeRandomUtil NewInstance() => new(logger: null!, cloner: null!);

    [Fact]
    public void Concurrent_RandInt_calls_never_throw_and_produce_unique_values()
    {
        // System.Random's documented failure mode for concurrent access is "may return
        // zero indefinitely or throw ArgumentException". We verify neither happens.
        var util = NewInstance();
        var observed = new ConcurrentBag<int>();
        const int threads = 16;
        const int callsPerThread = 1000;

        Parallel.For(0, threads, _ =>
        {
            for (var i = 0; i < callsPerThread; i++)
                observed.Add(util.RandInt(0, 10_000));
        });

        Assert.Equal(threads * callsPerThread, observed.Count);
        // We expect at least a few hundred distinct values — far more than the
        // "always zero" failure mode would produce.
        var distinct = new HashSet<int>(observed).Count;
        Assert.True(distinct > 500, $"only {distinct} distinct values across {threads * callsPerThread} calls — suspicious");
    }

    [Fact]
    public void Concurrent_GetDouble_never_throws_and_stays_in_range()
    {
        var util = NewInstance();
        var anyOutOfRange = false;
        Parallel.For(0, 16, _ =>
        {
            for (var i = 0; i < 500; i++)
            {
                var v = util.GetDouble(0.0, 1.0);
                if (v < 0.0 || v > 1.0) anyOutOfRange = true;
            }
        });
        Assert.False(anyOutOfRange);
    }

    [Fact]
    public void Concurrent_GetBool_returns_both_values()
    {
        var util = NewInstance();
        var trueCount = 0;
        var falseCount = 0;
        var lockObj = new object();
        Parallel.For(0, 16, _ =>
        {
            var localTrue = 0;
            var localFalse = 0;
            for (var i = 0; i < 1000; i++)
            {
                if (util.GetBool()) localTrue++; else localFalse++;
            }
            lock (lockObj) { trueCount += localTrue; falseCount += localFalse; }
        });
        // 16 * 1000 = 16,000 calls; we'd expect ~8000 of each. Allow huge slack but
        // both must be > 0 (failure mode "always returns false" would put trueCount at 0).
        Assert.True(trueCount > 1000,  $"trueCount={trueCount} suspiciously low");
        Assert.True(falseCount > 1000, $"falseCount={falseCount} suspiciously low");
    }

    [Fact]
    public void Concurrent_RandNum_concurrent_calls_stay_within_bounds()
    {
        var util = NewInstance();
        var anyOutOfRange = false;
        Parallel.For(0, 8, _ =>
        {
            for (var i = 0; i < 500; i++)
            {
                var v = util.RandNum(10.0, 20.0, 2);
                if (v < 10.0 || v > 20.0) anyOutOfRange = true;
            }
        });
        Assert.False(anyOutOfRange);
    }
}
