// RemzDNB - 2026

using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Helpers;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace RZEssentials.MiscSettings;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class MiscSettingsPatcher(
    DatabaseService databaseService,
    ConfigServer configServer,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    private readonly MiscSettingsConfig _config = configLoader.Load<MiscSettingsConfig>();

    public Task OnLoad()
    {
        PatchFreeHeal();
        UnlockOutfits();
        PatchPmcMailResponses();
        DisableStartingGifts();

        return Task.CompletedTask;
    }

    private void PatchFreeHeal()
    {
        if (!_config.FreePostRaidHeal)
            return;

        var globals = databaseService.GetGlobals();
        globals.Configuration.Health.HealPrice.TrialLevels = 999;
        globals.Configuration.Health.HealPrice.TrialRaids = 999999999;

        log.Info(LogChannel.MiscSettings, "FreePostRaidHeal enabled.");
    }

    private void UnlockOutfits()
    {
        if (!_config.UnlockAllOutfits)
            return;

        var suits = databaseService.GetTraders().GetValueOrDefault(SPTarkov.Server.Core.Models.Enums.Traders.RAGMAN)?.Suits;
        if (suits is null)
        {
            log.Warning(LogChannel.MiscSettings, "Ragman suits is null : skipping UnlockAllOutfits.");
            return;
        }

        var storage = databaseService.GetTemplates().CustomisationStorage;
        var storageIds = storage.Select(s => s.Id).ToHashSet();

        foreach (var suit in suits)
        {
            if (!storageIds.Add(suit.SuiteId))
                continue;

            storage.Add(new CustomisationStorage
            {
                Id     = suit.SuiteId,
                Source = CustomisationSource.UNLOCKED_IN_GAME,
                Type   = CustomisationType.SUITE,
            });
        }

        log.Info(LogChannel.MiscSettings, "All outfits unlocked.");
    }

    private void PatchPmcMailResponses()
    {
        if (!_config.DisablePmcMailResponses)
            return;

        var chatConfig = configServer.GetConfig<PmcChatResponse>();
        chatConfig.Victim.ResponseChancePercent = 0;
        chatConfig.Killer.ResponseChancePercent = 0;

        log.Info(LogChannel.MiscSettings, "PMC mail responses disabled.");
    }

    private void DisableStartingGifts()
    {
        if (!_config.DisableStartingGifts)
            return;

        var harmony = new Harmony("com.rz.customprofiles.gifts");

        harmony.Patch(
            AccessTools.Method(typeof(GiftService), nameof(GiftService.SendPraporStartingGift)),
            prefix: new HarmonyMethod(typeof(GiftPatches), nameof(GiftPatches.SkipGift))
        );

        harmony.Patch(
            AccessTools.Method(typeof(GiftService), nameof(GiftService.SendGiftWithSilentReceivedCheck)),
            prefix: new HarmonyMethod(typeof(GiftPatches), nameof(GiftPatches.SkipGift))
        );

        log.Info(LogChannel.MiscSettings, "Starting gifts disabled.");
    }

    private static class GiftPatches
    {
        public static bool SkipGift() => false;
    }
}
