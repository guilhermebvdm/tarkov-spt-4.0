// RemzDNB - 2026

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;

namespace RZEssentials._Shared;

public interface IConfig
{
    static abstract string FileName { get; }
}

[Injectable(InjectionType.Singleton)]
public class ConfigLoader(ILogger<ConfigLoader> logger, ModHelper modHelper)
{
    private readonly string _modDir = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
    private readonly Dictionary<(Type, string), object> _cachedConfigs = new();

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    //

    public T Load<T>() where T : class, IConfig, new()
    {
        return Load<T>(T.FileName);
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Load a single config file.
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    public T Load<T>(string filename) where T : new()
    {
        var key = (typeof(T), filename);

        if (_cachedConfigs.TryGetValue(key, out var cached))
            return (T)cached;

        var path = Path.Combine(_modDir, "config", filename);
        if (!File.Exists(path))
        {
            logger.LogError("[RZE] {File} not found : using defaults.", filename);
            var def = new T();
            _cachedConfigs[key] = def;
            return def;
        }

        var result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), _serializerOptions) ?? new T();
        _cachedConfigs[key] = result;
        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Load all *.json files from a subfolder.
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    public IEnumerable<T> LoadAll<T>(string subfolder, IEnumerable<string>? exclude = null) where T : new()
    {
        var dir = Path.Combine(_modDir, "config", subfolder);

        if (!Directory.Exists(dir))
        {
            logger.LogError("[RZE] Folder '{Dir}' not found.", dir);
            return [];
        }

        var excludeSet = new HashSet<string>(exclude ?? [], StringComparer.OrdinalIgnoreCase);

        return Directory.GetFiles(dir, "*.json")
            .Where(path => !excludeSet.Contains(Path.GetFileName(path)))
            .OrderBy(path => path)
            .Select<string, T?>(path =>
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(File.ReadAllText(path), _serializerOptions);
                    if (result is null)
                        logger.LogError("[RZE] Could not deserialize '{File}' : skipping.", path);
                    return result;
                }
                catch (Exception ex)
                {
                    logger.LogError("[RZE] Failed to load '{File}' : {Msg} : skipping.", path, ex.Message);
                    return default;
                }
            })
            .Where(r => r is not null)
            .Cast<T>();
    }
}
