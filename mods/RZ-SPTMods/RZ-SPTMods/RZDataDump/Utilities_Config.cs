// RemzDNB - 2026

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;

namespace RZDataDump;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// CONFIG LOADER
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

[Injectable(InjectionType.Singleton)]
public class ConfigLoader(ILogger<ConfigLoader> logger, ModHelper modHelper)
{
    private readonly Dictionary<(Type, string), object> _cachedConfigs = new();

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public T Load<T>(string filename, Assembly callerAssembly) where T : new()
    {
        var modDir = modHelper.GetAbsolutePathToModFolder(callerAssembly);
        var key = (typeof(T), modDir);

        if (_cachedConfigs.TryGetValue(key, out var cached)) {
            return (T)cached;
        }

        var path = Path.Combine(modDir, "config", filename);
        if (!File.Exists(path))
        {
            logger.LogError("[RZ] {File} not found in '{Dir}' : using defaults.", filename, modDir);
            var def = new T();
            _cachedConfigs[key] = def;
            return def;
        }

        var result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), _serializerOptions) ?? new T();
        _cachedConfigs[key] = result;
        return result;
    }

    public IEnumerable<T> LoadAll<T>(string subfolder, Assembly callerAssembly) where T : new()
    {
        var modDir = modHelper.GetAbsolutePathToModFolder(callerAssembly);
        var dir = Path.Combine(modDir, subfolder);

        if (!Directory.Exists(dir))
        {
            logger.LogWarning("[RZ] Folder '{Dir}' not found.", dir);
            return [];
        }

        return Directory.GetFiles(dir, "*.json")
            .Select(path =>
            {
                var result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), _serializerOptions);
                if (result is null) {
                    logger.LogWarning("[RZ] Could not deserialize '{File}' - skipping.", path);
                }

                return result;
            })
            .Where(r => r is not null)
            .Cast<T>();
    }
}
