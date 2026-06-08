using SPTarkov.Server.Core.Models.Spt.Mod;   // ref: SkillDistributionMetadata.cs:1

namespace CustomClasses;

/// <summary>
///     Mod metadata discovered by the SPT server. Mirrors the shape of the reference mod
///     SkillDistributionMetadata (same SPT 4.0 server contract).
/// </summary>
public record CustomClassesMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "customclasses.mdj";
    public override string Name { get; init; } = "CustomClasses";
    public override string Author { get; init; } = "mdj";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("0.1.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
