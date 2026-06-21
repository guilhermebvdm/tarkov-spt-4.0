using SPTarkov.Server.Core.Models.Spt.Mod;   // AbstractModMetadata

namespace TRLTraderPrices;

/// <summary>
///     Mod metadata discovered by the SPT server (same contract as CustomClasses / OutfitPersistenceFix).
/// </summary>
public record TRLTraderPricesMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "trltraderprices.trl";
    public override string Name { get; init; } = "TRLTraderPrices";
    public override string Author { get; init; } = "trl";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
