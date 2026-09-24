using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CompoundingPerf.Client;

/// <summary>
/// Locates and parses the same <c>config.json</c> the server mod uses. Tries the canonical
/// SPT mods path first, falls back to a sidecar <c>config.json</c> next to the plugin DLL.
/// Returns the parsed config and the SPT user-logs directory (used by telemetry dumper).
/// </summary>
internal static class ConfigLoader
{
    public static (CompoundingPerfConfig config, string? sptUserLogsDir) Load()
    {
        var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        // BepInEx\plugins\<this>\..\..\..  → SPT root
        var sptRoot = Path.GetFullPath(Path.Combine(pluginDir, "..", "..", ".."));
        var serverModConfig = Path.Combine(sptRoot, "SPT", "user", "mods", "CompoundingPerf", "config.json");
        var sidecarConfig   = Path.Combine(pluginDir, "config.json");
        var logsDir         = Path.Combine(sptRoot, "SPT", "user", "logs");

        var path = File.Exists(serverModConfig) ? serverModConfig
                 : File.Exists(sidecarConfig)   ? sidecarConfig
                 : null;

        if (path is null)
        {
            Plugin.Log?.LogWarning("CompoundingPerf.Client could not locate config.json — falling back to defaults");
            return (new CompoundingPerfConfig(), Directory.Exists(logsDir) ? logsDir : null);
        }

        Plugin.Log?.LogInfo($"CompoundingPerf.Client reading config from {path}");
        var raw = File.ReadAllText(path);
        var parsed = JsonConvert.DeserializeObject<CompoundingPerfConfig>(raw) ?? new CompoundingPerfConfig();
        return (parsed, Directory.Exists(logsDir) ? logsDir : null);
    }
}
