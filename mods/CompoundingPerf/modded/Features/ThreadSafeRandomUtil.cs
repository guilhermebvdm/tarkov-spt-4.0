using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S6 — Subclass of <see cref="RandomUtil"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/>. Replaces the unsafe instance
/// <c>System.Random</c> path with a lock-protected one for the four virtual methods
/// that still touch it. Sibling type <see cref="ThreadSafeRandomUtilPatches"/>
/// Harmony-patches the non-virtual <c>GetSecureRandomNumber</c> separately.
///
/// <para><b>Why this exists</b>: SPT 4.0.13 has partly migrated RandomUtil to use
/// <c>Random.Shared</c> (which is thread-safe by design) but <c>GetDouble</c>,
/// <c>GetBool</c>, <c>RandInt</c>, <c>RandNum</c>, and the misleadingly-named
/// <c>GetSecureRandomNumber</c> still call <c>this.Random</c> directly. A single
/// <c>System.Random</c> instance is shared across all callers. Per .NET docs,
/// concurrent calls produce undefined results — silent 0-returns, ArgumentExceptions,
/// or RNG-state corruption. In our case the corruption crashed the server when S5's
/// background loot regen ran concurrently with a foreground request handler.</para>
///
/// <para><b>Approach</b>: lock-wrap each unsafe override. The Random call itself is
/// nanoseconds; lock contention is irrelevant. The parallelism win comes from running
/// the surrounding (lock-free) algorithm in parallel, not from parallel Random calls.</para>
///
/// <para><b>Distribution preservation</b>: each override calls <c>base.X(...)</c> with
/// the same arguments — the seed source, range semantics, precision rounding, and
/// all other algorithm details are unchanged. Only the concurrent access pattern is
/// fixed.</para>
/// </summary>
// Lifetime must match vanilla RandomUtil ([Injectable(InjectionType.Singleton)]) —
// Injectable DEFAULTS to Scoped; see CoalescingSaveServer for the failure mode.
[Injectable(InjectionType.Singleton, TypeOverride = typeof(RandomUtil), TypePriority = 100)]
public class ThreadSafeRandomUtil : RandomUtil
{
    // One lock for the type. All concurrent calls to the unsafe-Random methods serialize
    // through here. The instance fields on RandomUtil itself (Random, _cloner) are shared
    // across all callers, so a single static lock matches the actual contention domain.
    internal static readonly object Gate = new();

    public ThreadSafeRandomUtil(ISptLogger<RandomUtil> logger, ICloner cloner)
        : base(logger, cloner)
    {
    }

    public override double GetDouble(double min, double max)
    {
        lock (Gate)
        {
            TelemetryHub.Increment("s6.random.get_double_calls");
            return base.GetDouble(min, max);
        }
    }

    public override bool GetBool()
    {
        lock (Gate)
        {
            TelemetryHub.Increment("s6.random.get_bool_calls");
            return base.GetBool();
        }
    }

    public override int RandInt(int low, int? high = null)
    {
        lock (Gate)
        {
            TelemetryHub.Increment("s6.random.rand_int_calls");
            return base.RandInt(low, high);
        }
    }

    public override double RandNum(double val1, double val2, int precision)
    {
        lock (Gate)
        {
            TelemetryHub.Increment("s6.random.rand_num_calls");
            return base.RandNum(val1, val2, precision);
        }
    }
}

/// <summary>
/// Harmony patch for <see cref="RandomUtil.GetSecureRandomNumber"/>. The method is
/// non-virtual, so DI override doesn't catch it — but Harmony patches at the JIT
/// detour level, which affects every call regardless of caller type. We replace the
/// body entirely: <c>Random.Shared.NextSingle()</c> is itself thread-safe by design.
///
/// <para>Despite the name, the original method has nothing to do with crypto-secure
/// randomness — it just calls <c>this.Random.NextSingle()</c>. It's used by
/// <c>GetGaussianRandom</c> and <c>GetNormallyDistributedRandomNumber</c>, both of
/// which need the patch for full thread-safety of the chain.</para>
/// </summary>
public static class ThreadSafeRandomUtilPatches
{
    public static void Apply(Harmony harmony, ISptLogger<CompoundingPerfMod> logger)
    {
        // GetSecureRandomNumber is private — string-literal name. AccessTools.Method
        // searches all visibilities via reflection.
        var target = AccessTools.Method(typeof(RandomUtil), "GetSecureRandomNumber");
        if (target is null)
        {
            logger.Warning("[CompoundingPerf/S6] RandomUtil.GetSecureRandomNumber not found — partial patch");
            return;
        }
        var prefix = new HarmonyMethod(typeof(ThreadSafeRandomUtilPatches), nameof(GetSecureRandomNumber_Prefix));
        harmony.Patch(target, prefix: prefix);
    }

    /// <summary>Return false to skip the original (unsafe) implementation. Sets
    /// <paramref name="__result"/> to a value drawn from the thread-safe Random.Shared.</summary>
    public static bool GetSecureRandomNumber_Prefix(ref double __result)
    {
        __result = Random.Shared.NextSingle();
        TelemetryHub.Increment("s6.random.get_secure_random_calls");
        return false;
    }
}
