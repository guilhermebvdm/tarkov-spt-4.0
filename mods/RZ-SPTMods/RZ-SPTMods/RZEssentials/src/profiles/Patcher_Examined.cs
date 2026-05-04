// RemzDNB - 2026

using System.Reflection;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using RZEssentials._Shared;

namespace RZEssentials.Profiles;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 2)]
public class ExaminedPatcher(ILogger<ExaminedPatcher> logger, DatabaseService databaseService, ConfigLoader configLoader) : IOnLoad
{
    public Task OnLoad()
    {
        var masterConfig = configLoader.Load<ProfilesMainConfig>();
        var profileConfigs = configLoader.LoadAll<ProfileConfig>("profiles/templates").ToList();

        var profilesToExamine = profileConfigs.Where(p => p.Enabled && p.AllItemsExamined).ToList();
        if (profilesToExamine.Count == 0) {
            return Task.CompletedTask;
        }

        var templateItems = databaseService.GetTables().Templates?.Items;
        if (templateItems is null) {
            logger.LogWarning("[RZCustomProfiles] Templates.Items is null : skipping AllItemsExamined.");
            return Task.CompletedTask;
        }

        var handbook = databaseService.GetTables().Templates?.Handbook;
        if (handbook is null) {
            logger.LogWarning("[RZCustomProfiles] Handbook is null : skipping AllItemsExamined.");
            return Task.CompletedTask;
        }

        var allHandbookTpls = handbook.Items.Select(i => i.Id.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var blacklistedTpls = new HashSet<string>(masterConfig.ExaminedBlacklist, StringComparer.OrdinalIgnoreCase);

        // ExaminedCategoryBlacklist
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        var enabledCategories = masterConfig.ExaminedCategoryBlacklist
            .Where(e => e.Enabled && !string.IsNullOrWhiteSpace(e.CategoryId))
            .Select(e => e.CategoryId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (enabledCategories.Count > 0)
        {
            foreach (var handbookItem in handbook.Items)
            {
                var tpl = handbookItem.Id.ToString();
                if (IsDescendantOfAny(tpl, enabledCategories, templateItems)) {
                    blacklistedTpls.Add(tpl);
                }
            }
        }

        var tplsToExamine = allHandbookTpls.Where(t => !blacklistedTpls.Contains(t)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sptProfiles = databaseService.GetProfileTemplates();

        foreach (var config in profilesToExamine)
        {
            if (!sptProfiles.TryGetValue(config.Name, out var profile))
            {
                logger.LogWarning("[RZCustomProfiles] Profile '{Name}' not found : skipping AllItemsExamined.", config.Name);
                continue;
            }

            foreach (var side in new[] { profile.Usec, profile.Bear })
            {
                var character = side?.Character;
                if (character is null) {
                    continue;
                }

                character.Encyclopedia ??= new Dictionary<MongoId, bool>();

                // Virer les blacklistés qui seraient déjà dans l'encyclopedia du profil de base.
                var removedCount = 0;
                foreach (var tpl in blacklistedTpls)
                {
                    if (character.Encyclopedia.Remove(new MongoId(tpl))) {
                        removedCount++;
                    }
                }

                //if (removedCount > 0)
                   //logger.LogInformation("[RZCustomProfiles] Removed {Count} blacklisted entries from encyclopedia.", removedCount);

                foreach (var tpl in tplsToExamine) {
                    character.Encyclopedia.TryAdd(new MongoId(tpl), true);
                }
            }
        }

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // IsDescendantOfAny
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private static bool IsDescendantOfAny(string tpl, HashSet<string> targetParents, Dictionary<MongoId, TemplateItem> templates)
    {
        var current = tpl;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!string.IsNullOrEmpty(current) && visited.Add(current))
        {
            if (targetParents.Contains(current)) {
                return true;
            }

            if (!templates.TryGetValue(current, out var item)) {
                break;
            }

            current = item.Parent.ToString();
        }

        return false;
    }
}
