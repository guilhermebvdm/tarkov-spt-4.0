// RemzDNB - 2026

using System.Reflection;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Eft.Weather;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Servers;
using RZEssentials._Shared;
using RZEssentials.Raids;
using SptWeatherConfig = SPTarkov.Server.Core.Models.Spt.Config.WeatherConfig;

namespace RZEssentials.Raids;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class WeatherHook(ConfigLoader configLoader, ConfigServer configServer, RaidWeatherService raidWeatherService, RzeLogger log) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<WeatherConfig>();
        if (!config.Enabled)
            return Task.CompletedTask;

        WeatherPatch._config             = config;
        WeatherPatch._log                = log;
        WeatherPatch._weatherCfg         = configServer.GetConfig<SptWeatherConfig>();
        WeatherPatch._raidWeatherService = raidWeatherService;

        var harmony = new Harmony("com.rz.essentials.weather");

        // Season + cache clear : runs at the start of every raid
        harmony.Patch(
            AccessTools.Method(typeof(WeatherCallbacks), nameof(WeatherCallbacks.GetLocalWeather)),
            prefix: new HarmonyMethod(typeof(WeatherPatch), nameof(WeatherPatch.OnRaidStart))
        );

        if (config.Presets.Enabled && config.Presets.Pool.Count > 0)
        {
            if (config.Presets.FixedForRaid)
            {
                // One preset drawn per raid, applied to the entire cached forecast
                harmony.Patch(
                    AccessTools.Method(typeof(RaidWeatherService), nameof(RaidWeatherService.GetUpcomingWeather)),
                    postfix: new HarmonyMethod(typeof(WeatherPatch), nameof(WeatherPatch.FixedPresetPostfix))
                );
            }
            else
            {
                // One preset drawn per weather period, matches vanilla generation rhythm
                harmony.Patch(
                    AccessTools.Method(typeof(WeatherGenerator), nameof(WeatherGenerator.GenerateWeather)),
                    postfix: new HarmonyMethod(typeof(WeatherPatch), nameof(WeatherPatch.PerPeriodPresetPostfix))
                );
            }
        }

        log.Info(LogChannel.MiscSettings, $"WeatherPatcher loaded. FixedForRaid={config.Presets.FixedForRaid}");
        return Task.CompletedTask;
    }
}

public static class WeatherPatch
{
    internal static WeatherConfig?       _config;
    internal static RzeLogger?           _log;
    internal static SptWeatherConfig?    _weatherCfg;
    internal static RaidWeatherService?  _raidWeatherService;

    // Preset stored for the duration of the raid in FixedForRaid mode
    private static WeatherPresetEntry? _currentRaidPreset;

    private static readonly FieldInfo _forecastField =
        typeof(RaidWeatherService).GetField("WeatherForecast", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly Dictionary<string, Season> _seasonMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Winter"]      = Season.WINTER,
        ["EarlySpring"] = Season.SPRING_EARLY,
        ["Spring"]      = Season.SPRING,
        ["Storm"]       = Season.STORM,
        ["Summer"]      = Season.SUMMER,
        ["Autumn"]      = Season.AUTUMN,
        ["LateAutumn"]  = Season.AUTUMN_LATE,
    };

    // Runs at the start of every raid via GetLocalWeather prefix
    public static void OnRaidStart()
    {
        // Draw a new season and clear the cache so it rebuilds with the new one
        if (_config?.Season is { Enabled: true } season && _weatherCfg is not null)
            _weatherCfg.OverrideSeason = DrawSeason(season);

        var forecast = _forecastField?.GetValue(_raidWeatherService) as List<Weather>;
        forecast?.Clear();

        // Draw and store the raid preset now so FixedPresetPostfix always returns the same one
        if (_config?.Presets is { Enabled: true, FixedForRaid: true })
            _currentRaidPreset = DrawPreset(_config.Presets);
    }

    // Vanilla mode : one preset per weather period
    public static void PerPeriodPresetPostfix(ref Weather __result)
    {
        var preset = DrawPreset(_config?.Presets);
        if (preset is null) return;
        ApplyPreset(__result, preset);
    }

    // Fixed mode : same preset for the entire raid, reused across multiple GetUpcomingWeather calls
    public static void FixedPresetPostfix(ref IEnumerable<Weather> __result)
    {
        if (_currentRaidPreset is null) return;
        foreach (var weather in __result)
            ApplyPreset(weather, _currentRaidPreset);
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static Season DrawSeason(SeasonConfig config)
    {
        var pool = config.Weights
            .Where(kvp => _seasonMap.ContainsKey(kvp.Key) && kvp.Value > 0)
            .Select(kvp => (Season: _seasonMap[kvp.Key], Weight: kvp.Value))
            .ToList();

        var total = pool.Sum(p => p.Weight);
        if (total <= 0) return Season.SUMMER;

        var roll = Random.Shared.Next(total);
        foreach (var (s, weight) in pool)
        {
            if (roll < weight) return s;
            roll -= weight;
        }

        return Season.SUMMER;
    }

    private static WeatherPresetEntry? DrawPreset(PresetsConfig? config)
    {
        if (config is null || !config.Enabled || config.Pool.Count == 0)
            return null;

        var pool = config.Pool.Where(p => p.Weight > 0).ToList();
        var total = pool.Sum(p => p.Weight);
        if (total <= 0) return null;

        var roll = Random.Shared.Next(total);
        foreach (var preset in pool)
        {
            if (roll < preset.Weight) return preset;
            roll -= preset.Weight;
        }

        return null;
    }

    private static void ApplyPreset(Weather weather, WeatherPresetEntry preset)
    {
        if (preset.Rain.HasValue)          weather.Rain          = preset.Rain;
        if (preset.RainIntensity.HasValue) weather.RainIntensity = preset.RainIntensity;
        if (preset.Fog.HasValue)           weather.Fog           = preset.Fog;
        if (preset.Cloud.HasValue)         weather.Cloud         = preset.Cloud;
        if (preset.WindSpeed.HasValue)     weather.WindSpeed     = preset.WindSpeed;
        if (preset.WindGustiness.HasValue) weather.WindGustiness = preset.WindGustiness;
    }
}
