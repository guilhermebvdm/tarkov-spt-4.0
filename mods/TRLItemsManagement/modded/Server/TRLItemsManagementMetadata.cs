using SPTarkov.Server.Core.Models.Spt.Mod;   // AbstractModMetadata
using SPTarkov.Server.Web;                    // IModWebMetadata (marker that opts the assembly into the web pipeline)

namespace TRLItemsManagement;

/// <summary>
///     Mod metadata for the TRL Items Management web mod (B-2 spike). The <see cref="IModWebMetadata"/>
///     marker is what mounts wwwroot/ at /TRLItemsManagement-Server/ and registers this assembly's
///     controllers (SPTWeb.cs:16,23-26,53-69). Without it, neither the UI nor the API are served.
/// </summary>
public record TRLItemsManagementMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "trl.items.management";
    public override string Name { get; init; } = "TRLItemsManagement";
    public override string Author { get; init; } = "trl";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("0.1.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
