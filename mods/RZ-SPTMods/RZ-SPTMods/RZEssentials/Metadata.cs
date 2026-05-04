// RemzDNB - 2026

using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;

namespace RZEssentials;

public record ModMetadata : AbstractModMetadata
{
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override Range SptVersion { get; init; } = new("~4.0.0");

    public override string ModGuid { get; init; } = "com.rz.essentials";
    public override string Name { get; init; } = "RZEssentials";
    public override string Author { get; init; } = "RemzDNB";
    public override string License { get; init; } = "MIT";
    public override List<string>? Contributors { get; init; } = null;
    public override List<string>? Incompatibilities { get; init; } = null;
    public override Dictionary<string, Range>? ModDependencies { get; init; } = null;
    public override string? Url { get; init; } = null;
    public override bool? IsBundleMod { get; init; } = false;
}
