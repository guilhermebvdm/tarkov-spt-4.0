using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace TrlWeatherSync.SeasonCycle;

// ref: mods/Skills-Extended/modded/Server/Metadata.cs:10-24 (padrão real já usado no repo).
// Contributors/Incompatibilities/ModDependencies/Url/IsBundleMod são abstratos em AbstractModMetadata
// (confirmado por erro CS0534 numa tentativa de build sem eles) — não são opcionais apesar de nullable.
public record SeasonCycleModMetadata : AbstractModMetadata
{
    public static SeasonCycleModMetadata Instance { get; } = new();
    public override string ModGuid { get; init; } = "trl.weathersync.seasoncycle";
    public override string Name { get; init; } = "TRL-WeatherSync — Season Cycle";
    public override string Author { get; init; } = "TRL";
    public override List<string>? Contributors { get; init; } = [];
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = false;
    public override Version Version { get; init; } = new("1.0.2");
    public override Range SptVersion { get; init; } = new("~4.0.0");
    public override string License { get; init; } = "All Rights Reserved";
}
