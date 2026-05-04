// RemzDNB - 2026
// ReSharper disable EnforceIfStatementBraces

using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Loot;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class ConfigValidator(ILogger<ConfigValidator> logger, ConfigLoader configLoader, DatabaseService databaseService ) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<LootConfig>(LootConfig.FileName);
        if (!config.Enabled)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            logger.LogWarning("[RZCustomLoot] Templates.Items is null — cannot validate config.");
            return Task.CompletedTask;
        }

        var lootPools = new List<(string label, List<LootEntry> entries)>();

        if (config.Bosses?.Global?.Pockets is not null)
            lootPools.Add(("Bosses.Global.Pockets", config.Bosses.Global.Pockets));
        if (config.Bosses?.PerBoss is not null) {
            foreach ((string role, BotLootSection section) in config.Bosses.PerBoss) {
                if (section?.Pockets is not null)
                    lootPools.Add(($"Bosses.PerBoss.{role}.Pockets", section.Pockets));
            }
        }
        if (config.Followers?.Pockets is not null)
            lootPools.Add(("Followers.Pockets", config.Followers.Pockets));
        if (config.Pmc?.Pockets is not null)
            lootPools.Add(("Pmc.Pockets", config.Pmc.Pockets));
        if (config.Scav?.Pockets is not null)
            lootPools.Add(("Scav.Pockets", config.Scav.Pockets));

        int warnings = 0;
        foreach ((string label, List<LootEntry> entries) in lootPools) {
            warnings += entries.Sum(entry => ValidateEntry(entry, label, items));
        }

        if (warnings != 0)
            logger.LogWarning("[RZCustomLoot] Config validation finished — {Count} warning(s).", warnings);
        //logger.LogInformation("[RZCustomLoot] Config validation passed — no issues found.");

        return Task.CompletedTask;
    }

    private int ValidateEntry(LootEntry entry, string label, Dictionary<MongoId, TemplateItem> items)
    {

        int warnings = 0;

        // TPL exists
        if (!items.TryGetValue(new MongoId(entry.Tpl), out TemplateItem? template))
        {
            logger.LogWarning("[RZCustomLoot] [{Label}] TPL '{Tpl}' not found in database — entry will be skipped.",
                label, entry.Tpl);
            return 1;
        }

        string name = entry.Tpl; // fallback si pas de locale dispo

        // 1x1 check
        int w = template.Properties?.Width ?? 1;
        int h = template.Properties?.Height ?? 1;
        if (w != 1 || h != 1)
        {
            logger.LogWarning("[RZCustomLoot] [{Label}] '{Tpl}' is {W}x{H} — pocket items should be 1x1, may cause visual issues.",
                label, name, w, h);
            warnings++;
        }

        // Chance range
        if (entry.Chance < 0 || entry.Chance > 100)
        {
            logger.LogWarning("[RZCustomLoot] [{Label}] '{Tpl}' has Chance={Chance} — must be between 0 and 100.",
                label, name, entry.Chance);
            warnings++;
        }

        // Stack sanity
        int stackMax = template.Properties?.StackMaxSize ?? 1;
        int minStack = entry.MinStack;
        int maxStack = entry.MaxStack;

        if (minStack < 1)
        {
            logger.LogWarning("[RZCustomLoot] [{Label}] '{Tpl}' MinStack={Min} is less than 1 — will be clamped to 1.",
                label, name, minStack);
            warnings++;
        }

        if (maxStack < minStack)
        {
            logger.LogWarning("[RZCustomLoot] [{Label}] '{Tpl}' MaxStack={Max} is less than MinStack={Min} — entry may behave unexpectedly.",
                label, name, maxStack, minStack);
            warnings++;
        }

        if (maxStack > stackMax)
        {
            logger.LogWarning("[RZCustomLoot] [{Label}] '{Tpl}' MaxStack={Max} exceeds template max {TemplateMax} — will be clamped automatically.",
                label, name, maxStack, stackMax);
            warnings++;
        }

        return warnings;
    }
}
