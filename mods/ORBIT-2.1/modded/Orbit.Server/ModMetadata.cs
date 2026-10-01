using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using SemVerVersion = SemanticVersioning.Version;
using SemVerRange = SemanticVersioning.Range;

namespace Orbit.Server;

// SPT 4.0: metadata is the AbstractModMetadata record (4.1 turned it into the IModMetadata interface) and the
// Blazor marker is IModWebMetadata (4.1: IModBlazorMetadata, which also carries the HomePage card properties).
// The config UI is still served at /orbit: its route comes from the @page directives, not from the metadata.
public record OrbitServerMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.chazut.orbit.server";
    public override string Name { get; init; } = "ORBIT Server";
    public override string Author { get; init; } = "Chazut";
    public override List<string>? Contributors { get; init; }
    public override SemVerVersion Version { get; init; } = new("2.1.0");
    public override SemVerRange SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemVerRange>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "https://github.com/Chazut/ORBIT";
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
