using SPTarkov.DI.Annotations;

namespace TRLItemsManagement;

/// <summary>
///     Single mutual-exclusion lock shared by EVERY controller that mutates a file (flea price/ban/
///     flea-level/flea-cap/trader sell+buy in <c>Api/</c>, plus refresh/rescan in Estágio 4) — the C#
///     equivalent of <c>serve.js</c>'s <c>withWriteLock</c> (one module-level promise chain reused by
///     all 14+ write handlers there).
///     <para>
///     Must be a singleton (DI-registered here as <see cref="InjectionType.Singleton"/>): ASP.NET
///     instantiates a new controller PER REQUEST by default, so a <c>SemaphoreSlim</c> field on the
///     controller itself would give every request its own lock — zero mutual exclusion. Injecting this
///     singleton is what makes two concurrent <c>PATCH</c> calls actually serialize instead of racing a
///     read-modify-write against each other.
///     </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class WriteLockService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RunAsync(Func<Task> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
