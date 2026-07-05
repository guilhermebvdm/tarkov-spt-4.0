using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using System.Collections.Generic;

namespace TRLDynamicSpawnServer;

public record ModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string ModGuid { get; init; } = "com.trl.dynamicspawn";
    public override string Name { get; init; } = "TRL Dynamic Spawn Manager";
    public override string Author { get; init; } = "TRL";
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.3");
    
    // Satisfy all AbstractModMetadata requirements
    public override string License { get; init; } = "MIT";
    public override Dictionary<string, SemanticVersioning.Range> ModDependencies { get; init; } = new Dictionary<string, SemanticVersioning.Range>();
    public override string Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; } = false;
    public override List<string> Contributors { get; init; } = new List<string>();
    public override List<string> Incompatibilities { get; init; } = new List<string>();
    
    // Configura a rota onde o Painel Web vai ficar hospedado!
    public string WebComponentPath => "trldynamicspawn";
}
