using System;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TarkovRedLine.Server;

public record TarkovRedLineModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string Name { get; init; } = "TarkovRedLine-ServerMod";
    public override string Author { get; init; } = "Saraiva";
    public override string ModGuid { get; init; } = "com.saraiva.tarkovredline";
    public override SemanticVersioning.Version Version { get; init; } = new(4, 0, 0);
    public override SemanticVersioning.Range SptVersion { get; init; } = new(">=4.0.0");
    public override bool? IsBundleMod { get; init; } = false;
    public override System.Collections.Generic.List<string>? Contributors { get; init; } = [];
    public override System.Collections.Generic.List<string>? Incompatibilities { get; init; } = [];
    public override System.Collections.Generic.Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = [];
    public override string? Url { get; init; } = "https://github.com";
    public override string License { get; init; } = "MIT";

    static TarkovRedLineModMetadata()
    {
        // Register Harmony Patches
        Patches.FikaProfilePatch.Enable();
    }
}
