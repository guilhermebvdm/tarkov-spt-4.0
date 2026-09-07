using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace UIFixes;

// A watcher simply watches for the next TraderControllerClass.RunNetworkTransaction and resolves its task when 
// that method completes by calling its callback.
// This class also manages the watchers, ensuring they are called in reverse order (LIFO).
// To use simply call NetworkTransactionWatch.WatchNext()
public class NetworkTransactionWatcher : IDisposable
{
    private static readonly List<NetworkTransactionWatcher> Watchers = [];

    public static void Enable()
    {
        new NetworkTransactionPatch().Enable();
    }

    public static NetworkTransactionWatcher WatchNext()
    {
        var watcher = new NetworkTransactionWatcher();
        Watchers.Add(watcher);
        return watcher;
    }

    private Callback _innerCallback;
    private readonly TaskCompletionSource _source = new();

    public Task Task => _source.Task;

    protected void Watch(ref Callback callback)
    {
        _innerCallback = callback;
        callback = Callback;
    }

    public bool Started()
    {
        return _innerCallback != null;
    }

    public void Dispose()
    {
        Watchers.Remove(this);
        _source.TrySetCanceled();
    }

    private void Callback(IResult result)
    {
        _innerCallback?.Invoke(result);
        _source.TryComplete();
    }

    public class NetworkTransactionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderControllerClass), nameof(TraderControllerClass.RunNetworkTransaction));
        }

        [PatchPrefix]
        [HarmonyPriority(Priority.First)]
        public static void Prefix(ref Callback callback)
        {
            var list = Watchers.ToArray();
            Watchers.Clear();
            for (int i = list.Length - 1; i >= 0; i--)
            {
                list[i].Watch(ref callback);
            }
        }
    }
}