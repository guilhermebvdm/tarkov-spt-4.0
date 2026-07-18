using SPTarkov.Server.Core.Models.Spt.Mod;   // AbstractModMetadata
using SPTarkov.Server.Web;                    // IModWebMetadata (marker that opts the assembly into the web pipeline)

namespace TRLItemsManagement;

/// <summary>
///     Mod metadata for the unified TRL Items Management mod (server half). The <see cref="IModWebMetadata"/>
///     marker is what mounts wwwroot/ at /TRLItemsManagement-Server/ and registers this assembly's
///     controllers (SPTWeb.cs, confirmed with the B-2 spike). Without it, neither the UI nor the API
///     are served. Replaces TRLTraderPrices (trader sell/buy pricing) and the TRLItemsManagement spike
///     (proof that the web UI can be served from inside the SPT server process) — new GUID, does not
///     upgrade either.
/// </summary>
public record TRLItemsManagementMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "trlitemsmanagement.trl";
    public override string Name { get; init; } = "TRL Items Management";
    public override string Author { get; init; } = "trl";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.4");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}
