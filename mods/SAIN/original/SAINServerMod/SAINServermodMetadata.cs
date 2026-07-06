using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SAINServerMod;

public sealed record SAINServermodMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "me.sol.sain";
    public override string Name { get; init; } = "SAIN";
    public override string Author { get; init; } = "Solarint";
    public override List<string>? Contributors { get; init; } = [];
    public override Version Version { get; init; } = new("4.4.3");
    public override Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; } = [];
    public override Dictionary<string, Range>? ModDependencies { get; init; } = [];
    public override string? Url { get; init; } = "https://github.com/ArchangelWTF/SAIN";
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "MIT";
}
