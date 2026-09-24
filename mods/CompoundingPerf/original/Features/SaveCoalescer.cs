using System.Collections.Concurrent;

namespace CompoundingPerf.Features;

/// <summary>
/// Generic trailing-edge save coalescer keyed by <typeparamref name="TKey"/>.
/// At most one save runs concurrently per key. While a save is in flight, additional
/// requests reserve a single "trailing" save that fires after the current one completes,
/// capturing whatever state is in memory at that point. All requests during the in-flight
/// window share the trailing save's Task.
///
/// <para>This guarantees: when a caller awaits the returned Task, a save whose serialized
/// state is at-least-as-recent as the caller's state-at-call has completed. No state
/// changes are silently dropped — they always land in either the current save (if it
/// hadn't started serializing yet) or the trailing save.</para>
///
/// <para>Designed for testability — the actual save action is injected as a delegate so
/// the state machine can be exercised without depending on SPT runtime types.</para>
/// </summary>
public sealed class SaveCoalescer<TKey, TResult> where TKey : notnull
{
    private readonly Func<TKey, Task<TResult>> _performSave;
    private readonly ConcurrentDictionary<TKey, SessionSlot> _slots = new();

    public SaveCoalescer(Func<TKey, Task<TResult>> performSave)
    {
        _performSave = performSave;
    }

    public Task<TResult> RequestSave(TKey key)
    {
        var slot = _slots.GetOrAdd(key, _ => new SessionSlot());

        lock (slot.Lock)
        {
            // No save is currently running — start one immediately.
            if (slot.Current is null || slot.Current.IsCompleted)
            {
                var task = StartSaveLocked(key, slot);
                return task;
            }

            // A save is in flight. Reserve (or re-use) a trailing save.
            slot.Pending ??= new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            return slot.Pending.Task;
        }
    }

    private Task<TResult> StartSaveLocked(TKey key, SessionSlot slot)
    {
        Task<TResult> task;
        try
        {
            task = _performSave(key);
        }
        catch (Exception ex)
        {
            // Synchronous throw from the save action — surface as a faulted task so callers
            // can observe via await. The slot's Current stays null so the next call retries.
            return Task.FromException<TResult>(ex);
        }

        slot.Current = task;
        // When this save finishes, drain the trailing reservation (if any).
        _ = task.ContinueWith(
            t => OnCurrentCompleted(key, slot, t),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        return task;
    }

    private void OnCurrentCompleted(TKey key, SessionSlot slot, Task<TResult> completedSave)
    {
        TaskCompletionSource<TResult>? trailing;
        lock (slot.Lock)
        {
            trailing = slot.Pending;
            slot.Pending = null;
            slot.Current = null;
        }

        if (trailing is null) return; // No trailing reservation — done.

        // Kick off the trailing save now. This call captures the latest in-memory state.
        Task<TResult> trailingTask;
        try
        {
            lock (slot.Lock) trailingTask = StartSaveLocked(key, slot);
        }
        catch (Exception ex)
        {
            trailing.TrySetException(ex);
            return;
        }

        // When the trailing save finishes, complete the TCS with its outcome.
        _ = trailingTask.ContinueWith(t =>
        {
            if (t.IsFaulted)        trailing.TrySetException(t.Exception!.InnerExceptions);
            else if (t.IsCanceled)  trailing.TrySetCanceled();
            else                    trailing.TrySetResult(t.Result);
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private sealed class SessionSlot
    {
        public readonly object Lock = new();
        public Task<TResult>? Current;
        public TaskCompletionSource<TResult>? Pending;
    }
}
