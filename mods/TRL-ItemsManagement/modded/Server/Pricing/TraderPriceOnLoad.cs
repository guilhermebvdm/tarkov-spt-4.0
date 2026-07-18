using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                       // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Helpers;                  // PaymentHelper
using SPTarkov.Server.Core.Models.Common;            // MongoId
using SPTarkov.Server.Core.Models.Utils;             // ISptLogger
using SPTarkov.Server.Core.Services;                 // DatabaseService
using SPTarkov.Server.Core.Utils;                    // JsonUtil

namespace TRLItemsManagement.Pricing;

/// <summary>
///     Boot-time orchestrator for both trader-price directions. Ported from TRLTraderPrices v1.1.0's
///     <c>TRLTraderPricesMod.OnLoad</c> — same 3 independent try/catch blocks (sell / Harmony bootstrap /
///     buy), same timing, now delegating the actual work to <see cref="SellPriceApplier"/> and
///     <see cref="BuyPriceLoader"/> (which share <see cref="TraderOverrideConfigParser"/>). The parser
///     deliberately swallows no exception, so keeping 3 separate try/catch blocks here (rather than
///     folding them into the shared helper) keeps failure-independence a visible property of this
///     orchestrator, not an accident of a shared helper's internals.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 1)]
public class TraderPriceOnLoad(
    ModPathsService modPaths,
    DatabaseService databaseService,
    PaymentHelper paymentHelper,
    JsonUtil jsonUtil,
    ISptLogger<TraderPriceOnLoad> logger
) : IOnLoad
{
    /// <summary>
    ///     B-3: parsed <c>buy-overrides.json</c> (traderId → tpl → override), read by the Harmony backstop
    ///     <see cref="SellItemPatch"/> at <c>TradeHelper.SellItem</c> time and by
    ///     <see cref="TraderBuyOverridesRouter"/> for the client feed. Pre-parsed into <see cref="MongoId"/>
    ///     keys at load (not raw strings) so neither consumer does string parsing on the request path.
    ///     Null until <see cref="OnLoad"/> runs; empty (not null) once loaded with no entries.
    /// </summary>
    internal static Dictionary<MongoId, Dictionary<MongoId, TraderOverride>>? BuyOverrides;

    /// <summary>Exposed to <see cref="SellItemPatch"/> (a static Harmony patch has no DI) to resolve <c>trader.Base.Currency</c>.</summary>
    internal static DatabaseService? Db;

    /// <summary>Exposed to <see cref="SellItemPatch"/> for diagnostics (same pattern as OutfitPersistenceFixMod).</summary>
    internal static ISptLogger<TraderPriceOnLoad>? Log;

    private static bool _harmonyPatched;

    public Task OnLoad()
    {
        // The SPT loader invokes IOnLoad.OnLoad without its own try/catch, so any exception here
        // (e.g. JsonUtil.DeserializeFromFile throwing JsonException on a corrupt overrides.json — it
        // only returns null for a MISSING file) would abort server boot. Guard the whole body so a bad
        // config can never brick the server.
        try
        {
            var configPath = Path.Combine(modPaths.ConfigDir, "overrides.json");
            SellPriceApplier.Apply(configPath, jsonUtil, databaseService, paymentHelper, logger);
        }
        catch (Exception ex)
        {
            logger.Error("[TRLItemsManagement] failed to load/apply sell overrides: " + ex);
        }

        // Expose Db/Log to the static Harmony patch (no DI in a HarmonyPatch class), patch
        // TradeHelper.SellItem once (idempotent — OnLoad can in principle run more than once), and
        // load the buy-side config. Kept in its own try/catch so a Harmony/config failure here can
        // never take down the sell-override logic above (or vice versa).
        Db = databaseService;
        Log = logger;

        if (!_harmonyPatched)
        {
            try
            {
                new Harmony("trlitemsmanagement.trl.buyprice").PatchAll(typeof(TraderPriceOnLoad).Assembly);
                _harmonyPatched = true;
                logger.Info("[TRLItemsManagement] Harmony patch applied — buy price backstop (TradeHelper.SellItem).");
            }
            catch (Exception ex)
            {
                logger.Error("[TRLItemsManagement] failed to apply Harmony patch (buy price backstop): " + ex.Message);
            }
        }

        try
        {
            var buyConfigPath = Path.Combine(modPaths.ConfigDir, "buy-overrides.json");
            BuyOverrides = BuyPriceLoader.Load(buyConfigPath, jsonUtil, logger);
        }
        catch (Exception ex)
        {
            logger.Error("[TRLItemsManagement] failed to load buy-overrides.json: " + ex.Message);
            BuyOverrides = new Dictionary<MongoId, Dictionary<MongoId, TraderOverride>>();
        }

        // B-5: per-tpl flea-floor overrides, read by the Harmony patch FleaFloorOverridePatch (which
        // PatchAll above already registered). Own try/catch so a bad floor config can't take down the
        // buy/sell logic (or vice versa).
        try
        {
            var floorConfigPath = Path.Combine(modPaths.ConfigDir, FleaFloorOverrideStore.FileName);
            FleaFloorOverrideStore.Load(floorConfigPath, jsonUtil, logger);
        }
        catch (Exception ex)
        {
            logger.Error("[TRLItemsManagement] failed to load " + FleaFloorOverrideStore.FileName + ": " + ex.Message);
            FleaFloorOverrideStore.Map = new Dictionary<MongoId, double>();
        }

        return Task.CompletedTask;
    }
}
