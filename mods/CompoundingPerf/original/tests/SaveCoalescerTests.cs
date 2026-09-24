using CompoundingPerf.Features;
using Xunit;

namespace CompoundingPerf.Tests;

/// <summary>
/// The whole point of <see cref="SaveCoalescer{TKey,TResult}"/> is the trailing-edge
/// guarantee: state changes during an in-flight save MUST be picked up by a subsequent
/// trailing save. These tests pin that contract — they're the difference between a
/// correctness-preserving coalescer and a state-dropping bug.
/// </summary>
public class SaveCoalescerTests
{
    [Fact]
    public async Task Single_call_with_no_in_flight_save_runs_save_directly()
    {
        var saves = 0;
        var coalescer = new SaveCoalescer<string, int>(_ => Task.FromResult(Interlocked.Increment(ref saves)));

        var result = await coalescer.RequestSave("p1");

        Assert.Equal(1, result);
        Assert.Equal(1, saves);
    }

    [Fact]
    public async Task Concurrent_calls_during_in_flight_save_share_one_trailing_save()
    {
        var saveCount = 0;
        var firstSaveGate = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<int> Save(string _)
        {
            var n = Interlocked.Increment(ref saveCount);
            // First save blocks on the gate so we can pile up trailing requests behind it.
            return n == 1 ? firstSaveGate.Task : Task.FromResult(n);
        }

        var coalescer = new SaveCoalescer<string, int>(Save);

        // First call kicks off save #1 (gated).
        var t1 = coalescer.RequestSave("p1");

        // While save #1 is in flight, fire 5 more requests. They should all share ONE trailing save.
        var trailing = Enumerable.Range(0, 5)
            .Select(_ => coalescer.RequestSave("p1"))
            .ToArray();

        // All trailing tasks are the same Task instance — the single reservation.
        Assert.All(trailing, t => Assert.Same(trailing[0], t));

        // Release save #1.
        firstSaveGate.SetResult(1);

        var firstResult = await t1;
        var trailingResult = await trailing[0];

        Assert.Equal(1, firstResult);
        Assert.Equal(2, trailingResult);  // exactly one trailing save ran for all 5 callers
        Assert.Equal(2, saveCount);       // total saves: 1 in-flight + 1 trailing
    }

    [Fact]
    public async Task Trailing_save_captures_state_after_current_completes()
    {
        // This test simulates the durability concern: subsystem B modifies state DURING
        // save #1, then calls SaveProfileAsync. The trailing save MUST observe B's state.
        var stateSnapshots = new List<int>();
        var sharedState = 0;
        var firstSaveStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstSave = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<int> Save(string _)
        {
            var captured = sharedState;
            stateSnapshots.Add(captured);
            // First save blocks until released so we can mutate state and request another save.
            if (stateSnapshots.Count == 1)
            {
                firstSaveStarted.SetResult(true);
                return releaseFirstSave.Task.ContinueWith(_ => captured);
            }
            return Task.FromResult(captured);
        }

        var coalescer = new SaveCoalescer<string, int>(Save);

        sharedState = 100;
        var t1 = coalescer.RequestSave("p");
        await firstSaveStarted.Task;

        // Mutate state (simulating subsystem B's change) and request a save.
        sharedState = 200;
        var t2 = coalescer.RequestSave("p");

        // Mutate again before save #1 finishes.
        sharedState = 300;
        var t3 = coalescer.RequestSave("p");

        releaseFirstSave.SetResult(true);

        var s1 = await t1;
        var s2 = await t2;
        var s3 = await t3;

        Assert.Equal(100, s1);             // save #1 captured pre-mutation state
        Assert.Equal(300, s2);             // trailing save captured the LATEST state (300)
        Assert.Equal(s2, s3);              // both trailing requests resolve to the same trailing save
        Assert.Equal(new[] { 100, 300 }, stateSnapshots); // exactly two saves ran
    }

    [Fact]
    public async Task Sequential_calls_each_run_their_own_save()
    {
        var saveCount = 0;
        var coalescer = new SaveCoalescer<string, int>(_ => Task.FromResult(Interlocked.Increment(ref saveCount)));

        // Awaiting between calls means each caller starts after the prior completes —
        // no in-flight overlap, so each call gets its own save.
        await coalescer.RequestSave("p1");
        await coalescer.RequestSave("p1");
        await coalescer.RequestSave("p1");

        Assert.Equal(3, saveCount);
    }

    [Fact]
    public async Task Different_keys_run_independently()
    {
        var saves = new System.Collections.Concurrent.ConcurrentBag<string>();
        var coalescer = new SaveCoalescer<string, int>(k =>
        {
            saves.Add(k);
            return Task.FromResult(0);
        });

        await Task.WhenAll(
            coalescer.RequestSave("a"),
            coalescer.RequestSave("b"),
            coalescer.RequestSave("c"));

        Assert.Contains("a", saves);
        Assert.Contains("b", saves);
        Assert.Contains("c", saves);
    }

    [Fact]
    public async Task Save_exception_propagates_to_awaiter()
    {
        var coalescer = new SaveCoalescer<string, int>(_ =>
            Task.FromException<int>(new InvalidOperationException("boom")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => coalescer.RequestSave("p"));
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task Synchronous_throw_in_save_action_surfaces_as_faulted_task()
    {
        var coalescer = new SaveCoalescer<string, int>(_ => throw new InvalidOperationException("sync-throw"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => coalescer.RequestSave("p"));
    }

    [Fact]
    public async Task Trailing_save_failure_propagates_to_all_waiters()
    {
        var saveCount = 0;
        var firstSaveGate = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<int> Save(string _)
        {
            var n = Interlocked.Increment(ref saveCount);
            if (n == 1) return firstSaveGate.Task;
            return Task.FromException<int>(new InvalidOperationException("trailing-failed"));
        }

        var coalescer = new SaveCoalescer<string, int>(Save);

        var t1 = coalescer.RequestSave("p");
        var t2 = coalescer.RequestSave("p");
        var t3 = coalescer.RequestSave("p");

        firstSaveGate.SetResult(1);

        Assert.Equal(1, await t1);
        var ex2 = await Assert.ThrowsAsync<InvalidOperationException>(() => t2);
        var ex3 = await Assert.ThrowsAsync<InvalidOperationException>(() => t3);
        Assert.Equal("trailing-failed", ex2.Message);
        Assert.Equal("trailing-failed", ex3.Message);
    }
}
