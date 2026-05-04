// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Servers;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Models.Eft.Common;

namespace RZEssentials.Character;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class Patcher_Health(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    SaveServer saveServer,
    RzeLogger log
) : IOnLoad
{
    private readonly HealthConfig _healthConfig = configLoader.Load<HealthConfig>();

    public Task OnLoad()
    {
        if (!_healthConfig.Enabled)
            return Task.CompletedTask;

        var globals = databaseService.GetGlobals();
        globals.Configuration.Health.Effects.Regeneration.Energy = _healthConfig.BaseEnergyRegeneration;
        globals.Configuration.Health.Effects.Regeneration.Hydration = _healthConfig.BaseHydrationRegeneration;

        var profileTemplates = databaseService.GetProfileTemplates();
        foreach (var profile in profileTemplates.Values)
        {
            foreach (var side in new[] { profile.Usec, profile.Bear })
            {
                var bodyParts = side?.Character?.Health?.BodyParts;
                if (bodyParts is null) continue;
                ApplyHealthChanges(bodyParts);
            }
        }

        if (_healthConfig.ApplyToExistingSaves)
        {
            foreach (var profile in saveServer.GetProfiles().Values)
            {
                var bodyParts = profile.CharacterData?.PmcData?.Health?.BodyParts;
                if (bodyParts is null) continue;
                ApplyHealthChanges(bodyParts);
            }
        }

        log.Info(LogChannel.Character, $"Health config patched{(_healthConfig.ApplyToExistingSaves ? " (including existing saves)" : "")}.");
        return Task.CompletedTask;
    }

    private void ApplyHealthChanges(Dictionary<string, BodyPartHealth> bodyParts)
    {
        ApplyBodyPart(bodyParts, "Head", _healthConfig.Head);
        ApplyBodyPart(bodyParts, "Chest", _healthConfig.Chest);
        ApplyBodyPart(bodyParts, "Stomach", _healthConfig.Stomach);
        ApplyBodyPart(bodyParts, "LeftArm", _healthConfig.LeftArm);
        ApplyBodyPart(bodyParts, "RightArm", _healthConfig.RightArm);
        ApplyBodyPart(bodyParts, "LeftLeg", _healthConfig.LeftLeg);
        ApplyBodyPart(bodyParts, "RightLeg", _healthConfig.RightLeg);
    }

    private static void ApplyBodyPart(Dictionary<string, BodyPartHealth> bodyParts, string part, double? value)
    {
        if (!value.HasValue || !bodyParts.TryGetValue(part, out var bodyPart) || bodyPart.Health is null)
            return;

        bodyPart.Health.Current = value.Value;
        bodyPart.Health.Maximum = value.Value;
    }
}
