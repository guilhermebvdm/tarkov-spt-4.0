using System.Reflection;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using CompoundingPerf.Features;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf;

/// <summary>SPT 4.0 mod metadata. No package.json — this record replaces it.</summary>
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid    { get; init; } = "com.echostarz.compoundingperf";
    public override string Name       { get; init; } = "CompoundingPerf";
    public override string Author     { get; init; } = "EchoStarz";
    public override SemanticVersioning.Version Version    { get; init; } = new("1.3.0");
    public override SemanticVersioning.Range   SptVersion { get; init; } = new("~4.0.13");
    public override string License { get; init; } = "MIT";

    public override List<string>?                              Contributors      { get; init; }
    public override List<string>?                              Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url          { get; init; }
    public override bool?   IsBundleMod  { get; init; }
}

/// <summary>
/// Server-side entry for CompoundingPerf. Reads <c>config.json</c> and turns on the
/// individual features. S1 (profile-save coalescer) is implemented as a DI override
/// — see <see cref="CoalescingSaveServer"/> — so this method only needs to flip its
/// kill-switch flag. S6's GetSecureRandomNumber patch is the only Harmony use.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 50)]
public class CompoundingPerfMod(
    ISptLogger<CompoundingPerfMod> logger,
    ModHelper                      modHelper,
    HttpRouter                     httpRouter,
    SaveServer                     saveServer) : IOnLoad
{
    public const string ModGuid = "com.echostarz.compoundingperf";

    public Task OnLoad()
    {
        try
        {
            // Override self-check (same pattern as the S2 router check below): if another
            // mod displaced our SaveServer override, S1/S11 silently degrade to vanilla —
            // say so instead of pretending.
            if (saveServer is not CoalescingSaveServer)
            {
                logger.Warning($"[CompoundingPerf/S1] SaveServer DI override did NOT take — actual type is {saveServer.GetType().FullName}. Save coalescing and dirty-tracking inactive.");
            }

            var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            var config  = modHelper.GetJsonDataFromFile<CompoundingPerfConfig>(modPath, "config.json");

            TelemetryHub.TimingEnabled = config.Telemetry.TimingEnabled;

#if BENCH
            // Dev benchmark builds only: server-side GC/counter sampler. Runs on BOTH
            // sides of an A/B (it tags each line with masterEnabled), so start it
            // before the master-switch bail below.
            Features.BenchRecorder.Start(logger, config.MasterEnabled);
#endif

            // Master A/B switch: when off, leave every feature flag at its inert default
            // and bail — the DI overrides are all in the container but behave exactly
            // like their base classes while their flags are false.
            if (!config.MasterEnabled)
            {
                logger.Warning("[CompoundingPerf] MASTER SWITCH OFF — all optimizations disabled (benchmark baseline mode). Flip MasterEnabled to true to re-enable.");
                return Task.CompletedTask;
            }

            // S1: pure flag flip — CoalescingSaveServer is already in the DI container.
            ProfileSaveDebouncer.Apply(config.Server.ProfileSaveDebouncer, logger);

            // S2: configure the DI-overridden router. If our subclass is registered,
            // the DI container resolved `httpRouter` as a CachingHttpRouter and we can
            // turn caching on; otherwise the override didn't take and we log a warning.
            if (httpRouter is CachingHttpRouter cachingRouter)
            {
                cachingRouter.Configure(config.Server.ResponseCache);
                if (config.Server.ResponseCache.Enabled)
                    logger.Success($"[CompoundingPerf/S2] response cache ACTIVE — {CachingHttpRouter.DefaultCacheablePaths.Count} default paths + {config.Server.ResponseCache.AdditionalPaths.Count} additional");
                else
                    logger.Info("[CompoundingPerf/S2] response cache disabled in config");

            }
            else
            {
                logger.Warning($"[CompoundingPerf/S2] HttpRouter DI override did NOT take — actual type is {httpRouter.GetType().FullName}. Response caching inactive.");
            }

            var harmony = new Harmony(ModGuid);

            // S6: Harmony patch on the non-virtual GetSecureRandomNumber. The DI override
            // of RandomUtil (ThreadSafeRandomUtil) was wired up at container build — by
            // the time OnLoad runs, the container is already resolving RandomUtil
            // dependencies to our subclass automatically.
            if (config.Server.ThreadSafeRandom.Enabled)
            {
                ThreadSafeRandomUtilPatches.Apply(harmony, logger);
                logger.Success("[CompoundingPerf/S6] thread-safe RandomUtil ACTIVE — DI override + GetSecureRandomNumber patched");
            }
            else
            {
                logger.Info("[CompoundingPerf/S6] thread-safe RandomUtil disabled in config");
            }

            // S7/S9: the DI overrides only catch external virtual call sites — the real
            // callers are internal non-virtual `call` instructions, so the load-bearing
            // interception is a Harmony detour on the base method body (flag-gated).
            ResponseSanitizerPatch.Apply(harmony, logger);
            FastCompressionPatch.Apply(harmony, logger);

            // S7/S8/S9 flag flips.
            FastHttpResponseUtil.IsEnabled = config.Server.ResponseSanitizer.Enabled;
            logger.Success(config.Server.ResponseSanitizer.Enabled
                ? "[CompoundingPerf/S7] response sanitizer ACTIVE — single-pass ClearString replaces five regex passes"
                : "[CompoundingPerf/S7] response sanitizer disabled in config");

            CalmRagfairServer.IsEnabled = config.Server.RagfairCalmUpdates.Enabled;
            logger.Success(config.Server.RagfairCalmUpdates.Enabled
                ? "[CompoundingPerf/S8] calm ragfair updates ACTIVE — offer expiry runs without vanilla's forced blocking GC"
                : "[CompoundingPerf/S8] calm ragfair updates disabled in config");

            FastCompressionHttpListener.Level = FastCompressionHttpListener.ParseLevel(config.Server.FastCompression.Level);
            FastCompressionHttpListener.IsEnabled = config.Server.FastCompression.Enabled;
            logger.Success(config.Server.FastCompression.Enabled
                ? $"[CompoundingPerf/S9] fast response compression ACTIVE — zlib level {FastCompressionHttpListener.Level} (vanilla: SmallestSize)"
                : "[CompoundingPerf/S9] fast response compression disabled in config");

            // S10: pure flag flip — the three ThreadSafe* cache overrides are already
            // in the DI container; this turns their gates on.
            ThreadSafeCaches.Apply(config.Server.ThreadSafeCaches, logger);

            // S11: dirty-skip for profile saves. Marking happens in CachingHttpRouter,
            // skipping happens in CoalescingSaveServer — both already in the container.
            if (config.Server.SaveDirtyTracking.Enabled)
            {
                ProfileDirtyTracker.ForceSaveIntervalSeconds = Math.Max(10, config.Server.SaveDirtyTracking.ForceSaveIntervalSeconds);
                ProfileDirtyTracker.IsEnabled = true;
                logger.Success($"[CompoundingPerf/S11] save dirty-tracking ACTIVE — clean sessions skip serialization (force-save every {ProfileDirtyTracker.ForceSaveIntervalSeconds}s)");
            }
            else
            {
                ProfileDirtyTracker.IsEnabled = false;
                logger.Info("[CompoundingPerf/S11] save dirty-tracking disabled in config");
            }

            // S12: pure flag flip — IsolatedBotRandomisationHelper is already in the container.
            IsolatedBotRandomisation.Apply(config.Server.IsolatedBotRandomisation, logger);

            // S13: pure flag flip — CalmWebSocketHandler + CalmNotifierController are
            // already in the container.
            CalmNotifier.Apply(config.Server.CalmNotifier, logger);

            logger.Success("[CompoundingPerf] server-side features loaded");
        }
        catch (Exception ex)
        {
            logger.Error($"[CompoundingPerf] failed to load: {ex}");
        }
        return Task.CompletedTask;
    }
}
