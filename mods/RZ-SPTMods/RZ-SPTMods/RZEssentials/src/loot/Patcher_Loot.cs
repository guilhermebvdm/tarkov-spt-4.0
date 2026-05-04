// RemzDNB - 2026
// ReSharper disable EnforceIfStatementBraces
// ReSharper disable DuplicatedSequentialIfBodies

using System.Collections.Concurrent;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Loot;

// ToDos : _processedBots never cleared — static HashSet grows indefinitely.
// Check if bot MongoIds are reused between raids, and if not, add a flush mechanism (on raid end, or capped size).

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class CustomLootHook(ILogger<CustomLootHook> logger, ConfigLoader configLoader, DatabaseService databaseService) : IOnLoad
{
    public Task OnLoad()
    {
        CustomLootPatch._logger = logger;
        CustomLootPatch._lootConfig = configLoader.Load<LootConfig>(LootConfig.FileName);
        CustomLootPatch._databaseService = databaseService;
        CustomLootPatch._bossRoles = new HashSet<string>(CustomLootPatch._lootConfig.BossRoles, StringComparer.OrdinalIgnoreCase);
        CustomLootPatch._followerRoles = new HashSet<string>(CustomLootPatch._lootConfig.FollowerRoles, StringComparer.OrdinalIgnoreCase);

        var harmony = new Harmony("com.rz.essentials.loot");
        harmony.Patch(
            AccessTools.Method(typeof(BotGenerator), nameof(BotGenerator.PrepareAndGenerateBot)),
            postfix: new HarmonyMethod(typeof(CustomLootPatch), nameof(CustomLootPatch.Postfix))
        );

        return Task.CompletedTask;
    }
}

public static class CustomLootPatch
{
    internal static ILogger? _logger;
    internal static DatabaseService? _databaseService;
    internal static LootConfig? _lootConfig;
    internal static HashSet<string> _bossRoles = new(StringComparer.OrdinalIgnoreCase);
    internal static HashSet<string> _followerRoles = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ConcurrentDictionary<MongoId, byte> _processedBots = new();
    private static int _tagillaLogged = 0;

    // POSTFIX
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    public static void Postfix(BotGenerationDetails botGenerationDetails, ref BotBase __result)
    {
        if (_lootConfig is null || !_lootConfig.Enabled) return;
        if (__result?.Inventory?.Items is null) return;
        if (__result.Id is null) return;
        if (!_processedBots.TryAdd(__result.Id.Value, 0)) return;

        string role = botGenerationDetails.RoleLowercase ?? "";
        BotLootSection? section = ResolveSection(role, botGenerationDetails.IsPmc);
        if (section is null || !section.Enabled) return;

        InjectPocketLoot(__result, section, role, _lootConfig.EnableVerboseLogging);
    }

    // INJECTION
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private static void InjectPocketLoot(BotBase bot, BotLootSection section, string role, bool bDebugLog)
    {
        // No idea wtf is going on raid start, and I don't want to know.

        bool isTagilla = string.Equals(role, "bosstagilla", StringComparison.OrdinalIgnoreCase)
            && Interlocked.CompareExchange(ref _tagillaLogged, 1, 0) == 0;

        //

        List<Item>? items = bot.Inventory?.Items;
        Item? pocketContainer = items?.FirstOrDefault(i => string.Equals(i.SlotId, "Pockets", StringComparison.OrdinalIgnoreCase));
        string? pocketContainerId = pocketContainer?.Id.ToString();
        if (pocketContainerId is null)
            return;

        TemplateItem? pocketTemplate =
            _databaseService!.GetTables().Templates?.Items?.GetValueOrDefault(pocketContainer.Template.ToString());

        List<Grid>? grids = pocketTemplate?.Properties?.Grids?.ToList();
        if (grids is null || grids.Count == 0)
            return;

        List<LootEntry> pool = section.Pockets.ToList();
        if (pool.Count == 0)
            return;

        // Total cell capacity per grid : CellsH * CellsV for each pocket grid.
        int[] gridCapacity = grids.Select(g => (g.Properties?.CellsH ?? 1) * (g.Properties?.CellsV ?? 1)).ToArray();

        // Cells already occupied by SPT-generated items, tracked per grid.
        int[] gridUsed = new int[grids.Count];

        // Collect all SPT-generated items already present in the pocket container, and calculate how many cells they occupy per grid using
        // their real template dimensions (Width * Height).
        // This accounts for items larger than 1x1 to avoid overcounting available space.
        List<Item> sptPocketItems = items!.Where(i => i.ParentId == pocketContainerId).ToList();
        foreach (Item sptItem in sptPocketItems)
        {
            for (int i = 0; i < grids.Count; i++)
            {
                if (string.Equals(sptItem.SlotId, $"pocket{i + 1}", StringComparison.OrdinalIgnoreCase))
                {
                    TemplateItem? t = _databaseService!.GetTables().Templates?.Items
                        ?.GetValueOrDefault(sptItem.Template.ToString());
                    gridUsed[i] += (t?.Properties?.Width ?? 1) * (t?.Properties?.Height ?? 1);
                    break;
                }
            }
        }

        // Free slot queue.
        Queue<string> freeSlots =
            new(Enumerable.Range(0, grids.Count).SelectMany(i => Enumerable.Repeat($"pocket{i + 1}", gridCapacity[i] - gridUsed[i])));

        // Overwritable SPT items, filtered by blacklist and sorted by handbook price ascending.
        // Cheapest items are overwritten first to preserve the most valuable SPT loot.
        var handbook = _databaseService!.GetTables().Templates?.Handbook?.Items;
        List<(Item item, int gridIndex)> sptOverwritable = sptPocketItems
            .Select(i => (item: i, gridIndex: GetGridIndex(i.SlotId)))
            .Where(t => t.gridIndex >= 0)
            .Where(t => !IsBlacklisted(t.item.Template.ToString()))
            .OrderBy(t =>
            {
                var entry = handbook?.FirstOrDefault(h => h.Id == t.item.Template);
                return entry?.Price ?? int.MaxValue;
            })
            .ToList();

        int injectedFree = 0;
        int injectedOverwrite = 0;
        foreach (LootEntry entry in pool)
        {
            // If we have no more slots left, exit.
            if (freeSlots.Count == 0 && sptOverwritable.Count == 0)
                break;

            // Drop roll.
            double roll = Random.Shared.NextDouble() * 100.0;
            if (roll > entry.Chance)
                continue;

            // Stack size roll.
            int templateStackMax =
                _databaseService!.GetTables().Templates?.Items?.GetValueOrDefault(entry.Tpl)?.Properties?.StackMaxSize ?? 1;
            int minStack = Math.Max(1, entry.MinStack);
            int maxStack = Math.Min(Math.Max(entry.MaxStack, minStack), templateStackMax);
            int stackCount = Random.Shared.Next(minStack, maxStack + 1);

            //
            if (freeSlots.Count > 0)
            {
                // Add to free slot.

                string slot = freeSlots.Dequeue();
                items.Add(new Item
                {
                    Id = new MongoId(),
                    Template = new MongoId(entry.Tpl),
                    ParentId = pocketContainerId,
                    SlotId = slot,
                    Upd = new Upd { StackObjectsCount = stackCount },
                });
                injectedFree++;

                if (isTagilla && bDebugLog)
                    _logger?.LogWarning("[ForcedLoot] Tagilla — '{Tpl}' → {Slot} free ✓", entry.Tpl, slot);
            }
            else
            {
                // Overwrite.

                (Item sptItem, int gridIndex) = sptOverwritable[0];
                sptOverwritable.RemoveAt(0);
                items!.Remove(sptItem);

                // Récupérer les cellules libérées et les remettre dans la queue
                TemplateItem? sptTemplate = _databaseService!.GetTables().Templates?.Items
                    ?.GetValueOrDefault(sptItem.Template.ToString());
                int freedCells = (sptTemplate?.Properties?.Width ?? 1) * (sptTemplate?.Properties?.Height ?? 1);

                for (int i = 0; i < freedCells; i++)
                    freeSlots.Enqueue(sptItem.SlotId ?? $"pocket{gridIndex + 1}");

                // Injecter notre item sur le premier slot libéré
                string slot = freeSlots.Dequeue();
                items.Add(new Item
                {
                    Id = new MongoId(),
                    Template = new MongoId(entry.Tpl),
                    ParentId = pocketContainerId,
                    SlotId = slot,
                    Upd = new Upd { StackObjectsCount = stackCount },
                });
                injectedOverwrite++;

                if (isTagilla && bDebugLog)
                    _logger?.LogWarning("[ForcedLoot] Tagilla — '{Tpl}' overwrote SPT '{Spt}' on {Slot} ({Freed} cells freed) ✓",
                        entry.Tpl, sptItem.Template, slot, freedCells);
            }
        }

        //

        if (isTagilla && bDebugLog)
            _logger?.LogWarning("[ForcedLoot] Tagilla — done. {Free} free + {Overwrite} overwrite = {Total} injected.",
                injectedFree, injectedOverwrite, injectedFree + injectedOverwrite);
    }

    // HELPERS
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private static int GetGridIndex(string? slotId)
    {
        if (slotId is null || !slotId.StartsWith("pocket", StringComparison.OrdinalIgnoreCase))
            return -1;
        return int.TryParse(slotId.AsSpan(6), out int idx) ? idx - 1 : -1;
    }

    private static bool IsBlacklisted(string tpl)
    {
        var blacklist = _lootConfig!.OverwriteBlacklist;
        if (blacklist is null) return false;

        if (blacklist.Tpls?.Contains(tpl) == true)
            return true;

        if (blacklist.Categories?.Count > 0 && IsInCategory(tpl, blacklist.Categories))
            return true;

        return false;
    }

    private static bool IsInCategory(string tpl, HashSet<string> categoryIds)
    {
        var items = _databaseService!.GetTables().Templates?.Items;
        if (items is null) return false;

        string? current = tpl;
        while (current is not null)
        {
            if (categoryIds.Contains(current)) return true;
            if (!items.TryGetValue(current, out TemplateItem? tmpl)) break;
            current = tmpl.Parent.ToString();
        }
        return false;
    }

    private static BotLootSection? ResolveSection(string role, bool isPmc)
    {
        if (isPmc)
            return _lootConfig!.Pmc;

        if (_bossRoles.Contains(role))
            return ResolveBossSection(role);

        if (_followerRoles.Contains(role))
            return _lootConfig!.Followers;

        if (string.Equals(role, "assault", StringComparison.OrdinalIgnoreCase))
            return _lootConfig!.Scav;

        return null;
    }

    private static BotLootSection? ResolveBossSection(string role)
    {
        BossLootSection? bosses = _lootConfig!.Bosses;
        if (bosses is null || !bosses.Enabled) return null;

        if (bosses.UseGlobalConfig)
            return bosses.Global;

        if (bosses.PerBoss is not null && bosses.PerBoss.TryGetValue(role, out BotLootSection? perBoss))
            return perBoss;

        return bosses.Global;
    }
}
