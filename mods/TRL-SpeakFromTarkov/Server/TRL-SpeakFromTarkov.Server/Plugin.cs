using System;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;

namespace TRL_SpeakFromTarkov.Server
{
    public record SftServerModMetadata : AbstractModMetadata, IModWebMetadata
    {
        public override string Name { get; init; } = "TRL-SpeakFromTarkov.Server";
        public override string Author { get; init; } = "TRL Team";
        public override string ModGuid { get; init; } = "com.trl.speakfromtarkov.server";
        public override SemanticVersioning.Version Version { get; init; } = new(1, 6, 0);
        public override SemanticVersioning.Range SptVersion { get; init; } = new(">=4.0.0");
        public override bool? IsBundleMod { get; init; } = false;
        public override System.Collections.Generic.List<string>? Contributors { get; init; } = [];
        public override System.Collections.Generic.List<string>? Incompatibilities { get; init; } = [];
        public override System.Collections.Generic.Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = [];
        public override string? Url { get; init; } = "https://github.com";
        public override string License { get; init; } = "MIT";
    }
}
