using System;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TarkovRedLine.Server;

public record TarkovRedLineModMetadata : AbstractModMetadata, IModWebMetadata
{
    public override string Name { get; init; } = ModRouting.ModName;
    public override string Author { get; init; } = "Saraiva";
    public override string ModGuid { get; init; } = ModRouting.ModGuid;
    public override SemanticVersioning.Version Version { get; init; } = new(4, 1, 0);
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

        // Item 001: prepara o manifesto em background no boot (carrega do disco se a impressão do
        // mods_repo bate; senão gera). Fire-and-forget — não bloqueia o startup. O try/catch garante
        // que qualquer falha seja LOGADA, nunca uma unobserved task exception (PA-01-04).
        System.Threading.Tasks.Task.Run(() =>
        {
            try { Controllers.ModUpdaterController.EnsureManifestReady(); }
            catch (Exception ex) { Console.WriteLine($"[ModUpdater] boot warmup falhou: {ex.Message}"); }
        });
    }
}
