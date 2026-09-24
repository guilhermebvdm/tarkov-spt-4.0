using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using CompoundingPerf.Telemetry;

namespace CompoundingPerf.Features;

/// <summary>
/// S12 — Subclass of <see cref="BotHelper"/> registered via SPT.DI's
/// <see cref="Injectable.TypeOverride"/>. Returns a defensive clone from
/// <c>GetBotRandomizationDetails</c> instead of vanilla's live reference into the
/// shared <c>BotConfig</c>.
///
/// <para><b>The vanilla bug (source + IL verified)</b>: on night raids,
/// <c>BotInventoryGenerator.GenerateAndAddEquipmentToBot</c> applies nighttime
/// equipment-chance modifiers by writing them back into the object
/// <c>GetBotRandomizationDetails</c> returned — which is the live config record shared
/// by every bot, every raid, for the server's lifetime. Three consequences:</para>
/// <list type="number">
///   <item><b>Compounding</b> — the modifier is ADDED once per generated bot
///     (<c>newWeight = weight + currentValue</c>), so chances drift toward the 0/100
///     clamp bounds as the raid generates more bots.</item>
///   <item><b>Persistence</b> — the mutation is never reverted; one night raid leaves
///     the modifiers baked into config for every later raid, day or night, until
///     server restart.</item>
///   <item><b>Data race</b> — vanilla 4.0.13 generates bots in parallel
///     (<c>AsParallel</c> in <c>BotController.GenerateBotWave</c>), so those writes are
///     concurrent read-modify-writes on a plain <c>Dictionary</c>.</item>
/// </list>
///
/// <para><b>The fix</b>: every caller gets its own clone. The nighttime adjustment then
/// applies exactly once per bot to that bot's private copy — the evident intent of the
/// code — nothing persists across raids, and there is no shared object to race on.</para>
///
/// <para><b>Behavior note</b>: one downstream reader
/// (<c>BotEquipmentModGenerator</c>) previously observed the leaked night-modified
/// (and progressively compounded) values; with isolation it reads pristine config.
/// That is a deliberate change — the values it read in vanilla were corrupted by
/// design accident, not intent.</para>
/// </summary>
[Injectable(TypeOverride = typeof(BotHelper), TypePriority = 100)]
public class IsolatedBotRandomisationHelper(
    ISptLogger<BotHelper> logger,
    DatabaseService       databaseService,
    RandomUtil            randomUtil,
    ConfigServer          configServer,
    ICloner               cloner)
    : BotHelper(logger, databaseService, randomUtil, configServer)
{
    /// <summary>Kill-switch. While false, vanilla behavior (shared reference) applies.</summary>
    public static volatile bool IsEnabled;

    private readonly ICloner _cloner = cloner;

    public override RandomisationDetails? GetBotRandomizationDetails(int botLevel, EquipmentFilters botEquipConfig)
    {
        var details = base.GetBotRandomizationDetails(botLevel, botEquipConfig);

        if (!IsEnabled || details is null)
        {
            return details;
        }

        TelemetryHub.Increment("s12.randomisation.clones");
        return _cloner.Clone(details);
    }
}

public static class IsolatedBotRandomisation
{
    public static void Apply(IsolatedBotRandomisationOptions options, ISptLogger<CompoundingPerfMod> logger)
    {
        if (options.Enabled)
        {
            IsolatedBotRandomisationHelper.IsEnabled = true;
            logger.Success("[CompoundingPerf/S12] isolated bot randomisation ACTIVE — nighttime modifiers no longer compound, persist, or race on shared config");
        }
        else
        {
            IsolatedBotRandomisationHelper.IsEnabled = false;
            logger.Info("[CompoundingPerf/S12] isolated bot randomisation disabled in config");
        }
    }
}
