using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;   // JsonUtil

namespace TRLItemsManagement;

/// <summary>
///     Resolves <c>config/pipeline.json</c> (<c>{ "toolPath": "&lt;absolute path&gt;" }</c>) — where the
///     Node data pipeline (<c>tools/trl-items-management/</c> in the repo clone on THIS machine) lives,
///     so the refresh/rescan controllers know what to pass to <c>Process.Start</c>. Per-machine value,
///     never shipped/committed (same "user data, not code" reasoning as <c>overrides.json</c>).
/// </summary>
[Injectable(InjectionType.Singleton)]
public class PipelineConfigService(ModPathsService modPaths, JsonUtil jsonUtil)
{
    private sealed record PipelineConfig
    {
        [JsonPropertyName("toolPath")] public string? ToolPath { get; init; }
    }

    /// <summary>Null if <c>config/pipeline.json</c> is missing/corrupt/empty, or the path it names doesn't exist.</summary>
    public string? ResolveToolPath()
    {
        var path = Path.Combine(modPaths.ConfigDir, "pipeline.json");
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var toolPath = jsonUtil.DeserializeFromFile<PipelineConfig>(path)?.ToolPath;
            return !string.IsNullOrWhiteSpace(toolPath) && Directory.Exists(toolPath) ? toolPath : null;
        }
        catch
        {
            return null;
        }
    }
}
