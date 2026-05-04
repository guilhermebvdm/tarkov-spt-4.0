// RemzDNB - 2026

using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace RZEssentials.Raids;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class RaidsPatcher(
    ILogger<RaidsPatcher> logger,
    DatabaseService databaseService,
    ConfigLoader configLoader
) : IOnLoad
{
    private readonly RaidsConfig _raidsConfig = configLoader.Load<RaidsConfig>();

    public Task OnLoad()
    {
        PatchRaidTimes();
        PatchRaidRestrictions();
        PatchNoRunThrough();
        PatchSpecialExtracts();
        PatchMagazineSpeed();

        return Task.CompletedTask;
    }

    private void PatchRaidTimes()
    {
        if (!_raidsConfig.EnableRaidTimes)
            return;

        foreach (var location in databaseService.GetLocations().GetDictionary().Values)
        {
            if (!_raidsConfig.RaidTimes.TryGetValue(location.Base.Id, out var minutes))
                continue;

            location.Base.EscapeTimeLimit = minutes;
            location.Base.EscapeTimeLimitCoop = minutes;
            location.Base.EscapeTimeLimitPVE = minutes;
        }
    }

    private void PatchRaidRestrictions()
    {
        if (!_raidsConfig.RemoveRaidRestrictions)
            return;

        var globals = databaseService.GetGlobals();
        globals.Configuration.RestrictionsInRaid = [];
    }

    private void PatchNoRunThrough()
    {
        if (!_raidsConfig.NoRunThrough)
            return;

        var globals = databaseService.GetGlobals();
        globals.Configuration.Exp.MatchEnd.SurvivedExperienceRequirement = 0;
        globals.Configuration.Exp.MatchEnd.SurvivedSecondsRequirement = 0;
    }

    private void PatchSpecialExtracts()
    {
        if (!_raidsConfig.FreeSpecialExtracts)
            return;

        var locations = databaseService.GetLocations();
        var globals = databaseService.GetGlobals();

        var alpinists = globals.Configuration.RequirementReferences.Alpinists.ToList();
        alpinists.Clear();
        globals.Configuration.RequirementReferences.Alpinists = alpinists;

        foreach (var loc in locations.GetDictionary().Values)
        {
            foreach (var exit in loc.Base.Exits)
            {
                if (exit.Name == "Exfil_Train" || exit.Name == "Saferoom Exfil")
                    continue;

                if (exit.PassageRequirement == RequirementState.Reference ||
                    exit.PassageRequirement == RequirementState.Empty)
                {
                    exit.PassageRequirement = RequirementState.None;
                    exit.ExfiltrationType = ExfiltrationType.Individual;
                    exit.Id = null;
                    exit.Count = 0;
                    exit.PlayersCount = 0;
                    exit.RequirementTip = "";
                    exit.RequiredSlot = null;
                }
            }
        }
    }

    private void PatchMagazineSpeed()
    {
        var globals = databaseService.GetGlobals();
        globals.Configuration.BaseLoadTime /= _raidsConfig.MagazineLoadSpeedMultiplier;
        globals.Configuration.BaseUnloadTime /= _raidsConfig.MagazineUnloadSpeedMultiplier;
    }
}
