// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.UI;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1000)]
public class LocalePatcher(
    DatabaseService databaseService,
    LocaleService   localeService,
    ConfigLoader    configLoader,
    RzeLogger       log
) : IOnLoad
{
    private static readonly string[] ItemSuffixes = [" Name", " ShortName", " Description"];

    public Task OnLoad()
    {
        var config = configLoader.Load<LocalesConfig>();

        var hasItemMaps         = config.ForceEnglishItems || config.ForceEnglishMaps;
        var hasHideout          = config.ForceEnglishHideout;
        var hasTraders          = config.ForceEnglishTraders;
        var hasLocaleOverrides  = config.EnableLocaleOverrides && config.LocaleOverrides.Count > 0;
        var hasTraderLocales    = config.EnableTraderLocaleOverrides  && config.TraderLocaleOverrides.Count  > 0;

        if (!hasItemMaps && !hasHideout && !hasTraders && !hasLocaleOverrides && !hasTraderLocales)
            return Task.CompletedTask;

        var locationTpls = databaseService.GetTables().Locations?
            .GetDictionary()
            .Values
            .Where(loc => loc?.Base?.IdField != null)
            .Select(loc => loc!.Base!.IdField.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        var english = localeService.GetLocaleDb("en");

        var traderIdSet = hasTraders
            ? databaseService.GetTraders().Keys
                .Select(k => k.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        var traderKeys = hasTraders
            ? english.Keys
                .Where(k => traderIdSet.Any(id => k.StartsWith(id, StringComparison.OrdinalIgnoreCase)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        var hideoutKeys = hasHideout
            ? english.Keys
                .Where(k => k.StartsWith("hideout_area_", StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        var traderLocaleOverrides = new Dictionary<string, string>();
        if (hasTraderLocales)
        {
            foreach (var (traderId, entry) in config.TraderLocaleOverrides)
            {
                if (!string.IsNullOrWhiteSpace(entry.FullName))    traderLocaleOverrides[$"{traderId} FullName"]    = entry.FullName;
                if (!string.IsNullOrWhiteSpace(entry.FirstName))   traderLocaleOverrides[$"{traderId} FirstName"]   = entry.FirstName;
                if (!string.IsNullOrWhiteSpace(entry.Nickname))    traderLocaleOverrides[$"{traderId} Nickname"]    = entry.Nickname;
                if (!string.IsNullOrWhiteSpace(entry.Location))    traderLocaleOverrides[$"{traderId} Location"]    = entry.Location;
                if (!string.IsNullOrWhiteSpace(entry.Description)) traderLocaleOverrides[$"{traderId} Description"] = entry.Description;
            }
        }

        foreach (var (langCode, lazyLoad) in databaseService.GetLocales().Global)
        {
            var isEnglish = string.Equals(langCode, "en", StringComparison.OrdinalIgnoreCase);

            if (isEnglish && !hasLocaleOverrides && !hasTraderLocales)
                continue;

            lazyLoad.AddTransformer(dict =>
            {
                if (dict is null) return dict;

                if (!isEnglish)
                {
                    if (hasItemMaps)
                    {
                        foreach (var kvp in english)
                        {
                            if (!ShouldOverride(kvp.Key, config, locationTpls)) continue;
                            if (!dict.ContainsKey(kvp.Key)) continue;
                            dict[kvp.Key] = kvp.Value;
                        }
                    }

                    if (hasHideout)
                    {
                        foreach (var key in hideoutKeys)
                            if (dict.ContainsKey(key) && english.TryGetValue(key, out var val))
                                dict[key] = val;
                    }

                    if (hasTraders)
                    {
                        foreach (var key in traderKeys)
                            if (dict.ContainsKey(key) && english.TryGetValue(key, out var val))
                                dict[key] = val;
                    }
                }

                if (hasLocaleOverrides)
                    foreach (var (key, value) in config.LocaleOverrides)
                        if (!string.IsNullOrWhiteSpace(value))
                            dict[key] = value;

                if (hasTraderLocales)
                    foreach (var (key, value) in traderLocaleOverrides)
                        if (dict.ContainsKey(key))
                            dict[key] = value;

                return dict;
            });
        }

        log.Info(LogChannel.UI, $"Transformers attached to {databaseService.GetLocales().Global.Count} language(s).");

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static bool ShouldOverride(string key, LocalesConfig config, HashSet<string> locationTpls)
    {
        if (IsMapKey(key, locationTpls))
            return config.ForceEnglishMaps;

        if (IsItemKey(key))
            return config.ForceEnglishItems;

        return false;
    }

    private static bool IsItemKey(string key)
    {
        foreach (var suffix in ItemSuffixes)
            if (key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    private static bool IsMapKey(string key, HashSet<string> locationTpls)
    {
        foreach (var suffix in ItemSuffixes)
        {
            if (!key.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                continue;

            var prefix = key[..^suffix.Length];
            if (locationTpls.Contains(prefix))
                return true;
        }
        return false;
    }
}
