// RemzDNB - 2026

using System.Reflection;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils;
using RZEssentials._Shared;

namespace RZEssentials.Profiles;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class ProfilesPatcher(
    ILogger<ProfilesPatcher> logger,
    DatabaseService databaseService,
    LocaleService localeService,
    ConfigLoader configLoader,
    ProfilesUtilities profilesUtilities
) : IOnLoad
{
    public Task OnLoad()
    {
        var masterConfig = configLoader.Load<ProfilesMainConfig>();
        var profileConfigs = configLoader.LoadAll<ProfileConfig>(ProfilesMainConfig.TemplatesFolder).ToList();
        var sptProfiles = databaseService.GetProfileTemplates();
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Sanity checks
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        var duplicates = profileConfigs
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in duplicates)
        {
            logger.LogError(
                "[RZCustomProfiles] Profile name '{Name}' is defined {Count} times : profiles will overwrite each other. Check your profiles/ folder.", group.Key, group.Count()
            );
        }

        // Register custom profiles.
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        foreach (var config in profileConfigs)
        {
            if (!config.Enabled) {
                continue;
            }

            if (!ProfilesMainConfig.BaseProfiles.TryGetValue(config.BaseProfile, out var baseProfileName))
            {
                logger.LogWarning("[RZCustomProfiles] Invalid BaseProfile '{Id}' for '{Name}' : skipping.", config.BaseProfile, config.Name);
                continue;
            }

            if (!sptProfiles.TryGetValue(baseProfileName, out var baseProfile))
            {
                logger.LogWarning("[RZCustomProfiles] Base profile '{Base}' not found for '{Name}' : skipping.", baseProfileName, config.Name);
                continue;
            }

            var cloned = FastCloner.FastCloner.DeepClone(baseProfile);
            if (cloned is null)
            {
                logger.LogWarning("[RZCustomProfiles] Failed to clone '{Base}' for profile '{Name}' : skipping.", baseProfileName, config.Name);
                continue;
            }

            foreach (var locale in databaseService.GetTables().Locales.Languages.Keys) {
                localeService.GetLocaleDb(locale).TryAdd($"launcher-profile_{config.Name}", config.Description ?? config.Name);
            }

            cloned.DescriptionLocaleKey = config.Description ?? config.Name;

            profilesUtilities.PatchSide(cloned.Usec, "USEC", config);
            profilesUtilities.PatchSide(cloned.Bear, "BEAR", config);

            sptProfiles[config.Name] = cloned;
            allowed.Add(config.Name);
        }

        // Add enabled default profiles.
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        foreach (var index in masterConfig.EnabledBaseProfiles)
        {
            if (ProfilesMainConfig.BaseProfiles.TryGetValue(index, out var profileName)) {
                allowed.Add(profileName);
            }
            else {
                logger.LogWarning("[RZCustomProfiles] Unknown SPT profile index '{Index}' : skipped.", index);
            }
        }

        // Delete non allowed profiles.
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        foreach (var key in sptProfiles.Keys.Where(k => !allowed.Contains(k)).ToList()) {
            sptProfiles.Remove(key);
        }

        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class HarmonyHook(
    ILogger<HarmonyHook> logger,
    SaveServer saveServer,
    PrestigeHelper prestigeHelper,
    DatabaseService databaseService,
    TimeUtil timeUtil,
    ConfigLoader configLoader
) : IOnLoad
{
    public Task OnLoad()
    {
        HarmonyPatch.Logger          = logger;
        HarmonyPatch.SaveServer      = saveServer;
        HarmonyPatch.PrestigeHelper  = prestigeHelper;
        HarmonyPatch.DatabaseService = databaseService;
        HarmonyPatch.TimeUtil        = timeUtil;
        HarmonyPatch.Configs         = configLoader
            .LoadAll<ProfileConfig>(ProfilesMainConfig.TemplatesFolder)
            .Where(p => p.Enabled)
            .ToList();

        var harmony = new Harmony("com.rz.customprofiles");

        harmony.Patch(
            AccessTools.Method(typeof(CreateProfileService), "ResetAllTradersInProfile"),
            prefix:  new HarmonyMethod(typeof(HarmonyPatch), nameof(HarmonyPatch.ApplyPrestige))
        );

        harmony.Patch(
            AccessTools.Method(typeof(CreateProfileService), "ResetAllTradersInProfile"),
            postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(HarmonyPatch.ApplyTradersLoyalty))
        );

        harmony.Patch(
            AccessTools.Method(typeof(CreateProfileService), "ResetAllTradersInProfile"),
            postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(HarmonyPatch.ApplyAchievements))
        );

        harmony.Patch(
            AccessTools.Method(typeof(PrestigeHelper), "AddPrestigeRewardsToProfile"),
            prefix: new HarmonyMethod(typeof(HarmonyPatch), nameof(HarmonyPatch.SkipPrestigeRewards))
        );

        harmony.Patch(
            AccessTools.Method(typeof(CreateProfileService), "ResetAllTradersInProfile"),
            postfix: new HarmonyMethod(typeof(HarmonyPatch), nameof(HarmonyPatch.ApplySkills))
        );

        return Task.CompletedTask;
    }
}

public static class HarmonyPatch
{
    public static ILogger<HarmonyHook>? Logger;
    public static SaveServer?           SaveServer;
    public static PrestigeHelper?       PrestigeHelper;
    public static DatabaseService?      DatabaseService;
    public static TimeUtil?             TimeUtil;
    public static List<ProfileConfig>?  Configs;

    // Internal flag : true while ApplyPrestige loop is running
    private static bool _skipPrestigeRewards;

    // ─────────────────────────────────────────────────────────────────────────
    // Prestige
    // ─────────────────────────────────────────────────────────────────────────

    public static void ApplyPrestige(MongoId sessionId)
    {
        var profile = SaveServer?.GetProfile(sessionId);
        var edition = profile?.ProfileInfo?.Edition;

        if (edition is null)
            return;

        var config = Configs?.FirstOrDefault(p =>
            string.Equals(p.Name, edition, StringComparison.OrdinalIgnoreCase));

        if (config?.StartingPrestigeLevel is not { } prestige || prestige <= 0)
            return;

        _skipPrestigeRewards = config.SkipPrestigeRewards;

        for (var i = 1; i <= prestige; i++)
        {
            var pendingPrestige = new PendingPrestige { PrestigeLevel = i };
            PrestigeHelper!.ProcessPendingPrestige(profile!, profile!, pendingPrestige);
        }

        _skipPrestigeRewards = false;
    }

    // Prefix on PrestigeHelper.AddPrestigeRewardsToProfile — returns false to skip when flag is set.
    public static bool SkipPrestigeRewards()
    {
        return !_skipPrestigeRewards;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Traders loyalty
    // ─────────────────────────────────────────────────────────────────────────

    public static void ApplyTradersLoyalty(MongoId sessionId)
    {
        var profile = SaveServer?.GetProfile(sessionId);
        var edition = profile?.ProfileInfo?.Edition;

        if (edition is null)
            return;

        var config = Configs?.FirstOrDefault(p =>
            string.Equals(p.Name, edition, StringComparison.OrdinalIgnoreCase));

        if (config?.TradersLoyalty is null || config.TradersLoyalty.Count == 0)
            return;

        var tradersInfo = profile?.CharacterData?.PmcData?.TradersInfo;
        if (tradersInfo is null)
            return;

        foreach (var (traderId, traderConfig) in config.TradersLoyalty)
        {
            var key = new MongoId(traderId);

            if (!tradersInfo.TryGetValue(key, out var trader))
            {
                trader = new TraderInfo { Unlocked = true, Disabled = false, LoyaltyLevel = 1 };
                tradersInfo[key] = trader;
            }

            trader.Standing = traderConfig.Standing;
            trader.SalesSum = traderConfig.SalesSum;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Achievements
    // ─────────────────────────────────────────────────────────────────────────

    public static void ApplyAchievements(MongoId sessionId)
    {
        var profile = SaveServer?.GetProfile(sessionId);
        var edition = profile?.ProfileInfo?.Edition;

        if (edition is null)
            return;

        var config = Configs?.FirstOrDefault(p =>
            string.Equals(p.Name, edition, StringComparison.OrdinalIgnoreCase));

        if (config?.UnlockAllAchievements is not true)
            return;

        var pmcData = profile?.CharacterData?.PmcData;
        if (pmcData?.Achievements is null)
            return;

        var allAchievements = DatabaseService?.GetTemplates().Achievements ?? [];
        var timestamp = TimeUtil!.GetTimeStamp();
        var added = 0;

        foreach (var achievement in allAchievements)
        {
            if (pmcData.Achievements.TryAdd(achievement.Id, (long)timestamp))
                added++;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Apply Skills
    // ─────────────────────────────────────────────────────────────────────────

    public static void ApplySkills(MongoId sessionId)
    {
        var profile = SaveServer?.GetProfile(sessionId);
        var edition = profile?.ProfileInfo?.Edition;

        if (edition is null)
            return;

        var config = Configs?.FirstOrDefault(p =>
            string.Equals(p.Name, edition, StringComparison.OrdinalIgnoreCase));

        if (config is null || (!config.MaxSkills && config.SkillOverrides is not { Count: > 0 }))
            return;

        var botSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BotReload", "BotSound" };

        foreach (var pmcData in new[] { profile?.CharacterData?.PmcData, profile?.CharacterData?.ScavData })
        {
            var skills = pmcData?.Skills?.Common;
            if (skills is null) continue;

            if (config.MaxSkills)
            {
                foreach (var skill in skills)
                {
                    if (botSkills.Contains(skill.Id.ToString())) continue;
                    skill.Progress = 5100f;
                }
            }
            else
            {
                foreach (var (skillId, level) in config.SkillOverrides!)
                {
                    if (botSkills.Contains(skillId)) continue;
                    var skill = skills.FirstOrDefault(s =>
                        string.Equals(s.Id.ToString(), skillId, StringComparison.OrdinalIgnoreCase));
                    if (skill is null) continue;
                    skill.Progress = Math.Clamp(level, 0, 51) * 100f;
                }
            }
        }
    }
}
