// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Eft.Common;
using RZEssentials._Shared;

namespace RZEssentials.Character;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class Patcher_WeightLimits(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<WeightLimitsConfig>();
        if (!config.Enabled)
            return Task.CompletedTask;

        var stamina = databaseService.GetGlobals().Configuration.Stamina;

        if (config.BaseOverweightLimits is { } b)
            stamina.BaseOverweightLimits = new XYZ { X = b.X ?? stamina.BaseOverweightLimits.X, Y = b.Y ?? stamina.BaseOverweightLimits.Y, Z = 0 };

        if (config.WalkOverweightLimits is { } w)
            stamina.WalkOverweightLimits = new XYZ { X = w.X ?? stamina.WalkOverweightLimits.X, Y = w.Y ?? stamina.WalkOverweightLimits.Y, Z = 0 };

        if (config.SprintOverweightLimits is { } s)
            stamina.SprintOverweightLimits = new XYZ { X = s.X ?? stamina.SprintOverweightLimits.X, Y = s.Y ?? stamina.SprintOverweightLimits.Y, Z = 0 };

        if (config.WalkSpeedOverweightLimits is { } ws)
            stamina.WalkSpeedOverweightLimits = new XYZ { X = ws.X ?? stamina.WalkSpeedOverweightLimits.X, Y = ws.Y ?? stamina.WalkSpeedOverweightLimits.Y, Z = 0 };

        log.Info(LogChannel.Character, "Weight limits patched.");
        return Task.CompletedTask;
    }
}
