using SPTarkov.Server.Core.Models.Spt.Mod;   // AbstractModMetadata

namespace CustomizationPersistenceFix;

/// <summary>
///     Mod metadata discovered by the SPT server (same contract as CustomClasses / SkillDistribution).
/// </summary>
public record CustomizationPersistenceFixMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "customizationpersistencefix.mdj";
    public override string Name { get; init; } = "CustomizationPersistenceFix";
    public override string Author { get; init; } = "mdj";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
