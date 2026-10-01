using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S8 — Subclass of <see cref="RagfairServer"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/>. Vanilla's flea-offer expiry pass ends with
/// <c>GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: true,
/// compacting: true)</c> — a forced, blocking, compacting full collection that runs
/// every time enough offers have expired, for the lifetime of the server. On large
/// heaps that is a recurring multi-hundred-millisecond pause for every connected
/// player (we measured ~1s for this exact GC shape during this mod's own development —
/// it's why our post-raid GC feature was removed).
///
/// <para>The .NET server GC promotes and compacts on its own schedule; an explicit
/// blocking collect after offer churn trades a tiny, deferred reclamation benefit for
/// a guaranteed, immediate stall. This override reproduces vanilla's
/// <c>Update</c> sequence exactly — trader-offer refresh, expired-offer flagging,
/// regeneration, cache invalidation — and simply omits the forced collect.</para>
///
/// <para>The expiry logic is reimplemented (vanilla's is a private method) against the
/// same public services vanilla calls, in the same order, with the same semantics:
/// flag expired → check threshold → clone expired items BEFORE removal purges them →
/// remove → regenerate from the clone.</para>
/// </summary>
[Injectable(TypeOverride = typeof(RagfairServer), TypePriority = 100)]
public class CalmRagfairServer(
    ISptLogger<RagfairServer>   logger,
    TimeUtil                    timeUtil,
    RagfairOfferService         ragfairOfferService,
    RagfairCategoriesService    ragfairCategoriesService,
    RagfairRequiredItemsService ragfairRequiredItemsService,
    ServerLocalisationService   serverLocalisationService,
    RagfairOfferGenerator       ragfairOfferGenerator,
    RagfairOfferHolder          ragfairOfferHolder,
    ConfigServer                configServer,
    ICloner                     cloner)
    : RagfairServer(
        logger, timeUtil, ragfairOfferService, ragfairCategoriesService,
        ragfairRequiredItemsService, serverLocalisationService,
        ragfairOfferGenerator, ragfairOfferHolder, configServer, cloner)
{
    /// <summary>Kill-switch. False until OnLoad reads the config; while false,
    /// <see cref="Update"/> defers to vanilla (including its forced GC).</summary>
    public static volatile bool IsEnabled;

    private readonly TimeUtil                    _timeUtil = timeUtil;
    private readonly RagfairOfferService         _offerService = ragfairOfferService;
    private readonly RagfairRequiredItemsService _requiredItemsService = ragfairRequiredItemsService;
    private readonly RagfairOfferGenerator       _offerGenerator = ragfairOfferGenerator;
    private readonly RagfairOfferHolder          _offerHolder = ragfairOfferHolder;
    private readonly ICloner                     _cloner = cloner;

    public override void Update()
    {
        if (!IsEnabled)
        {
            base.Update();
            return;
        }

        // Mirror of vanilla RagfairServer.Update():
        RefreshTraderOffers();
        ProcessExpiredFleaOffersWithoutForcedGC();
        _requiredItemsService.InvalidateCache();
    }

    /// <summary>
    /// Vanilla's private ProcessExpiredFleaOffers, minus the forced blocking GC.Collect.
    /// Sequence and semantics otherwise identical — including cloning the expired offer
    /// items BEFORE RemoveExpiredOffers purges them.
    /// </summary>
    private void ProcessExpiredFleaOffersWithoutForcedGC()
    {
        _offerHolder.FlagExpiredOffersAfterDate(_timeUtil.GetTimeStamp());

        if (!_offerService.EnoughExpiredOffersExistToProcess())
        {
            return;
        }

        var expiredOfferItemsClone = _cloner.Clone(_offerHolder.GetExpiredOfferItems());

        _offerService.RemoveExpiredOffers();

        // Vanilla forces GC.Collect(MaxGeneration, Optimized, blocking: true, compacting: true)
        // here. Deliberately omitted — see class doc.
        TelemetryHub.Increment("s8.ragfair.forced_gc_skipped");

        if (expiredOfferItemsClone is not null)
        {
            _offerGenerator.GenerateDynamicOffers(expiredOfferItemsClone);
        }
    }
}
