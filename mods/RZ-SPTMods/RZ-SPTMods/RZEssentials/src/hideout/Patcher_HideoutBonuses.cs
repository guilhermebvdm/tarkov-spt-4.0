// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace RZEssentials.Hideout;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class BonusesPatcher(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<BonusesConfig>();
        if (!config.EnableBonusesConfig || config.Areas.Count == 0)
            return Task.CompletedTask;

        var areas = databaseService.GetTables().Hideout?.Areas;
        if (areas is null || areas.Count == 0)
        {
            log.Warning(LogChannel.Hideout, "No hideout areas found : skipping bonus overrides.");
            return Task.CompletedTask;
        }

        var patched = 0;

        foreach (var (areaName, stageOverrides) in config.Areas)
        {
            if (!Enum.TryParse<HideoutAreas>(areaName, ignoreCase: true, out var areaType))
            {
                log.Warning(LogChannel.Hideout, $"Unknown hideout area '{areaName}' : skipping.");
                continue;
            }

            var area = areas.FirstOrDefault(a => a.Type == areaType);
            if (area?.Stages is null)
            {
                log.Warning(LogChannel.Hideout, $"Area '{areaName}' not found in database : skipping.");
                continue;
            }

            foreach (var (levelStr, bonuses) in stageOverrides)
            {
                if (!area.Stages.TryGetValue(levelStr, out var stage))
                {
                    log.Warning(LogChannel.Hideout, $"Area '{areaName}': stage '{levelStr}' not found : skipping.");
                    continue;
                }

                foreach (var b in bonuses)
                {
                    var existing = stage.Bonuses?.FirstOrDefault(x => x.Id.ToString() == b.Id);
                    if (existing is null)
                    {
                        log.Warning(LogChannel.Hideout, $"Bonus Id '{b.Id}' not found in stage '{levelStr}' of '{areaName}' : skipping.");
                        continue;
                    }

                    if (Enum.TryParse<BonusType>(b.Type, ignoreCase: true, out var bonusType))
                        existing.Type = bonusType;

                    existing.Value = b.Value;
                    //existing.IsPassive = b.IsPassive;
                    //existing.IsProduction = b.IsProduction;
                    existing.IsVisible = b.IsVisible;

                    patched++;
                }
            }
        }

        log.Info(LogChannel.Hideout, $"Bonuses: {patched} stage(s) patched.");
        return Task.CompletedTask;
    }
}
