using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;               // IOnLoad, OnLoadOrder
using SPTarkov.Server.Core.Models.Utils;     // ISptLogger
using SPTarkov.Server.Core.Services;         // DatabaseService

namespace TRLItemsManagement;

/// <summary>
///     Boot step: applies every banned mod-item tpl (<c>config/mod-item-bans.json</c>) to the live
///     database — see <see cref="ModItemBanService"/> for why this exists separately from
///     <see cref="Api.BanController"/>'s vanilla-item file write. Same timing as
///     <see cref="Pricing.TraderPriceOnLoad"/> (before Ragfair generates offers, which is what actually
///     reads <c>CanSellOnRagfair</c>) and same defensive try/catch-the-whole-body shape — a corrupt
///     config here must never abort server boot.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 1)]
public class ModItemBanOnLoad(
    ModItemBanService banService,
    DatabaseService databaseService,
    ISptLogger<ModItemBanOnLoad> logger
) : IOnLoad
{
    public Task OnLoad()
    {
        try
        {
            var bans = banService.ReadAll();
            var applied = ModItemBanService.Apply(databaseService, bans);
            logger.Info($"[TRLItemsManagement] mod-item-bans.json: {applied} mod item(s) banned (of {bans.Count} configured).");
        }
        catch (Exception ex)
        {
            logger.Error("[TRLItemsManagement] failed to apply mod-item-bans.json: " + ex);
        }

        return Task.CompletedTask;
    }
}
