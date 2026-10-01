using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S1 — Subclass of <see cref="SaveServer"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/> mechanism. Replaces the built-in SaveServer at
/// container resolution time, so every consumer that depends on <c>SaveServer</c> gets
/// our coalescing version instead.
///
/// <para>Why DI override instead of Harmony: <see cref="SaveServer.SaveProfileAsync"/> is
/// an <c>async</c>-keyword method whose IL references a compiler-generated state machine
/// internal to SPT.Server.Core. Harmony's reverse-patch can't safely copy that IL into
/// our assembly. Normal C# virtual dispatch via <c>override</c> + <c>base</c> sidesteps
/// the entire problem — the runtime does the right thing automatically.</para>
///
/// <para>Behavior: when <see cref="ProfileSaveDebouncer.IsEnabled"/> is true, calls are
/// routed through <see cref="SaveCoalescer{TKey,TResult}"/> — at most one save in flight
/// + one trailing per profile. The trailing save captures the latest in-memory state when
/// it fires, so no profile mutations are ever dropped. When disabled, calls pass through
/// directly to <c>base.SaveProfileAsync</c> with zero overhead.</para>
/// </summary>
// InjectionType MUST match the vanilla class being overridden (SaveServer is
// [Injectable(InjectionType.Singleton)]). Injectable's DEFAULT is Scoped — omitting
// it here replaced the singleton with a per-request-scope service, so ASP.NET MVC
// consumers (FIKA 2.3's API controllers, the SPT dashboard) got a FRESH EMPTY
// SaveServer per request: "no profiles found in saveServer" on /fika/api/players.
[Injectable(InjectionType.Singleton, TypeOverride = typeof(SaveServer), TypePriority = 100)]
public class CoalescingSaveServer : SaveServer
{
    private readonly SaveCoalescer<MongoId, long> _coalescer;
    private readonly ISptLogger<SaveServer> _logger;

    public CoalescingSaveServer(
        FileUtil                                   fileUtil,
        IEnumerable<SaveLoadRouter>                saveLoadRouters,
        JsonUtil                                   jsonUtil,
        HashUtil                                   hashUtil,
        ServerLocalisationService                  serverLocalisationService,
        ProfileValidatorService                    profileValidatorService,
        BackupService                              backupService,
        ISptLogger<SaveServer>                     logger,
        ConfigServer                               configServer)
        : base(fileUtil, saveLoadRouters, jsonUtil, hashUtil,
               serverLocalisationService, profileValidatorService,
               backupService, logger, configServer)
    {
        _coalescer = new SaveCoalescer<MongoId, long>(InvokeBaseSave);
        _logger = logger;
    }

    /// <summary>
    /// The periodic save tick. The shipped <c>base.SaveAsync</c> loop calls
    /// <c>SaveProfileAsync</c> NON-virtually (Roslyn emitted <c>call</c>; the
    /// virtualizer doesn't rewrite call sites) — without this override, every periodic
    /// tick ran vanilla's full serialize+hash per profile and S11's skip-clean logic
    /// never saw the exact traffic it exists for. This mirrors the vanilla loop but the
    /// per-profile call dispatches virtually into our coalesce/skip pipeline.
    /// </summary>
    public override async Task SaveAsync()
    {
        var totalTime = 0L;
        var profiles = GetProfiles();
        foreach (var sessionID in profiles)
        {
            totalTime += await SaveProfileAsync(sessionID.Key);
        }

        if (profiles.Count > 0 && _logger.IsLogEnabled(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Debug))
        {
            _logger.Debug($"Saved {profiles.Count} profiles, took: {totalTime}ms");
        }
    }

    public override Task<long> SaveProfileAsync(MongoId sessionID)
    {
        // S11: skip the save outright when the session is provably clean and the last
        // real save is fresh. Vanilla would serialize + MD5 the full profile just to
        // discover there's nothing to write; this skips that work entirely.
        if (ProfileDirtyTracker.MaySkipSave(sessionID))
        {
            TelemetryHub.Increment("s11.profile_save.skipped_clean");
            return Task.FromResult(0L);
        }

        if (!ProfileSaveDebouncer.IsEnabled)
        {
            // Kill-switch: behave exactly like the built-in SaveServer.
            ProfileDirtyTracker.OnRealSaveStarting(sessionID);
            return base.SaveProfileAsync(sessionID);
        }

        TelemetryHub.Increment("s1.profile_save.requested");
        return _coalescer.RequestSave(sessionID);
    }

    private Task<long> InvokeBaseSave(MongoId sessionID)
    {
        TelemetryHub.Increment("s1.profile_save.executed");
        // Clear the dirty flag BEFORE the save body runs — mutations landing mid-save
        // re-dirty the session and get persisted by the next tick rather than lost.
        ProfileDirtyTracker.OnRealSaveStarting(sessionID);
        return base.SaveProfileAsync(sessionID);
    }
}
