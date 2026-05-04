// DevTools.cs — RemzDNB 2026

using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using Path = System.IO.Path;

namespace RZDataDump;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class DevTools(ILogger<DevTools> logger, DatabaseService databaseService, ConfigLoader configLoader) : IOnLoad
{
    private static readonly string _devDir = Path.Combine(
        AppContext.BaseDirectory, "user", "mods", "RZDataDump", "dev"
    );

    public Task OnLoad()
    {
        var config = configLoader.Load<MasterConfig>(MasterConfig.FileName, Assembly.GetExecutingAssembly());

        if (config.DumpItemsEnabled)
            DumpItems(config);

        if (config.DumpCategoriesEnabled)
            DumpCategories();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DumpItems
    // Dumps the handbook as a nested JSON structure mirroring the category tree.
    // Items are at the leaves, formatted as "tpl": price, // Name
    //
    // DumpMode:
    //   0 — all items
    //   1 — vanilla only  (requires vanilla_handbook.json at mod root)
    //   2 — modded only   (requires vanilla_handbook.json at mod root)
    // ─────────────────────────────────────────────────────────────────────────

    private void DumpItems(MasterConfig config)
    {
        var handbook = databaseService.GetTables().Templates?.Handbook;
        if (handbook is null)
        {
            logger.LogWarning("[RZDataDump] Handbook is null — cannot dump items.");
            return;
        }

        Dictionary<string, string>? enLocale = null;
        if (databaseService.GetTables().Locales?.Global.TryGetValue("en", out var enLocaleRaw) == true)
            enLocale = enLocaleRaw?.Value;

        // Build vanilla TPL set if needed.
        HashSet<string>? vanillaTpls = null;
        if (config.DumpItemsMode is 1 or 2)
        {
            vanillaTpls = LoadVanillaTpls();
            if (vanillaTpls is null) return;
        }

        // Filter items by mode.
        var filteredItems = handbook.Items
            .Where(h =>
            {
                var tpl = h.Id.ToString();
                return config.DumpItemsMode switch
                {
                    1 => vanillaTpls!.Contains(tpl),
                    2 => !vanillaTpls!.Contains(tpl),
                    _ => true
                };
            })
            .ToList();

        if (filteredItems.Count == 0)
        {
            logger.LogInformation("[RZDataDump] No items matched the dump criteria.");
            return;
        }

        // Build category id → name map.
        var templateItems = databaseService.GetTables().Templates?.Items;
        var categoryName = handbook.Categories.ToDictionary(
            c => c.Id.ToString(),
            c =>
            {
                if (templateItems is not null && templateItems.TryGetValue(c.Id, out var t) && t.Name is not null)
                    return t.Name;
                return ResolveName(c.Id.ToString(), enLocale); // fallback locale
            }
        );

        // Build category tree : parent → ordered children.
        var categoryChildren = handbook.Categories
            .Where(c => c.ParentId is not null)
            .GroupBy(c => c.ParentId!.ToString())
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id.ToString()).ToList());

        // Build item lookup : parentCategoryId → items.
        var itemsByCategory = filteredItems
            .GroupBy(h => h.ParentId.ToString())
            .ToDictionary(g => g.Key, g => g.OrderBy(h => ResolveName(h.Id.ToString(), enLocale)).ToList());

        // Root categories : those whose parent is not itself a category.
        var allCategoryIds = handbook.Categories.Select(c => c.Id.ToString()).ToHashSet();
        var rootCategories = handbook.Categories
            .Where(c => c.ParentId is null || !allCategoryIds.Contains(c.ParentId.ToString()))
            .Select(c => c.Id.ToString())
            .OrderBy(id => categoryName.GetValueOrDefault(id, id))
            .ToList();

        // Write nested JSON.
        var sb = new StringBuilder();
        sb.AppendLine("{");

        WriteCategories(sb, rootCategories, categoryChildren, categoryName, itemsByCategory, enLocale, depth: 1);

        sb.Append("}");

        var outputPath = Path.Combine(_devDir, "item_dump.json");
        try
        {
            Directory.CreateDirectory(_devDir);
            File.WriteAllText(outputPath, sb.ToString());
            logger.LogInformation("\e[1;32m[RZDataDump] {Count} item(s) dumped to dev/item_dump.json.\e[0m", filteredItems.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning("[RZDataDump] Failed to write item_dump.json: {Err}", ex.Message);
        }
    }

    private void WriteCategories(
        StringBuilder sb,
        List<string> categoryIds,
        Dictionary<string, List<string>> categoryChildren,
        Dictionary<string, string> categoryName,
        Dictionary<string, List<SPTarkov.Server.Core.Models.Eft.Common.Tables.HandbookItem>> itemsByCategory,
        Dictionary<string, string>? enLocale,
        int depth)
    {
        var indent = new string(' ', depth * 2);
        var innerIndent = new string(' ', (depth + 1) * 2);

        for (var i = 0; i < categoryIds.Count; i++)
        {
            var catId = categoryIds[i];
            var name = categoryName.GetValueOrDefault(catId, catId);
            var hasChildren = categoryChildren.ContainsKey(catId);
            var hasItems = itemsByCategory.ContainsKey(catId);
            var isLastCategory = i == categoryIds.Count - 1;

            if (!hasChildren && !hasItems)
                continue;

            sb.AppendLine($"{indent}\"{name}\": {{");

            // Recurse into sub-categories first.
            if (hasChildren)
            {
                var children = categoryChildren[catId]
                    .OrderBy(id => categoryName.GetValueOrDefault(id, id))
                    .ToList();
                WriteCategories(sb, children, categoryChildren, categoryName, itemsByCategory, enLocale, depth + 1);
            }

            // Then write items at this level.
            if (hasItems)
            {
                var items = itemsByCategory[catId];
                for (var j = 0; j < items.Count; j++)
                {
                    var h = items[j];
                    var tpl = h.Id.ToString();
                    var price = h.Price ?? 0;
                    var itemName = ResolveName(tpl, enLocale);
                    var comma = (j < items.Count - 1 || hasChildren) ? "," : "";
                    sb.AppendLine($"{innerIndent}\"{tpl}\": {price}{comma} // {itemName}");
                }
            }

            var trailingComma = !isLastCategory ? "," : "";
            sb.AppendLine($"{indent}}}{trailingComma}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DumpCategories
    // Dumps all Node entries from Templates.Items to dev/category_dump.txt.
    // Sorted as an indented tree : root categories at the top, children below.
    // ─────────────────────────────────────────────────────────────────────────

    private void DumpCategories()
    {
        var templateItems = databaseService.GetTables().Templates?.Items;
        if (templateItems is null)
        {
            logger.LogWarning("[RZDataDump] Templates.Items is null — cannot dump categories.");
            return;
        }

        Dictionary<string, string>? enLocale = null;
        if (databaseService.GetTables().Locales?.Global.TryGetValue("en", out var enLocaleRaw) == true)
            enLocale = enLocaleRaw?.Value;

        var nodes = templateItems
            .Where(kvp => string.Equals(kvp.Value.Type, "Node", StringComparison.OrdinalIgnoreCase))
            .Select(kvp =>
            {
                var id = kvp.Key.ToString();
                var parentId = kvp.Value.Parent.ToString() ?? "";
                return (Id: id, ParentId: parentId, Name: ResolveName(id, enLocale));
            })
            .ToList();

        var childrenOf = nodes
            .GroupBy(n => n.ParentId)
            .ToDictionary(g => g.Key, g => g.Select(n => n.Id).ToList());

        var idToNode = nodes.ToDictionary(n => n.Id);

        var rootIds = nodes
            .Where(n => !idToNode.ContainsKey(n.ParentId))
            .Select(n => n.Id)
            .OrderBy(id => idToNode.TryGetValue(id, out var nd) ? nd.Name : id)
            .ToList();

        var ordered = new List<(string Id, string ParentId, string Name, int Depth)>();
        var queue = new Queue<(string Id, int Depth)>(rootIds.Select(id => (id, 0)));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();
            if (!idToNode.TryGetValue(current, out var node)) continue;

            ordered.Add((node.Id, node.ParentId, node.Name, depth));

            if (!childrenOf.TryGetValue(current, out var children)) continue;

            foreach (var child in children.OrderBy(c => idToNode.TryGetValue(c, out var cn) ? cn.Name : c))
                queue.Enqueue((child, depth + 1));
        }

        var lines = ordered.Select(n =>
        {
            var indent = new string(' ', n.Depth * 2);
            var parentInfo = n.Depth == 0 ? "ROOT" : $"parent:{n.ParentId}";
            return $"{indent}{n.Id}  {parentInfo}  {n.Name}";
        });

        var outputPath = Path.Combine(_devDir, "category_dump.txt");
        try
        {
            Directory.CreateDirectory(_devDir);
            File.WriteAllLines(outputPath, lines);
            logger.LogInformation("\e[1;32m[RZDataDump] {Count} category/ies dumped to dev/category_dump.txt.\e[0m", ordered.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning("[RZDataDump] Failed to write category_dump.txt: {Err}", ex.Message);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string ResolveName(string tpl, Dictionary<string, string>? enLocale)
    {
        if (enLocale is null) return "?";
        if (enLocale.TryGetValue(tpl, out var name) && name is not null) return name;
        if (enLocale.TryGetValue($"{tpl} Name", out name) && name is not null) return name;
        return "?";
    }

    private HashSet<string>? LoadVanillaTpls()
    {
        var vanillaPath = Path.Combine(AppContext.BaseDirectory, "user", "mods", "RZDataDump", "vanilla_handbook.json");
        if (!File.Exists(vanillaPath))
        {
            logger.LogWarning("[RZDataDump] vanilla_handbook.json not found — cannot filter by vanilla/modded.");
            return null;
        }

        try
        {
            var raw = File.ReadAllText(vanillaPath);
            using var doc = System.Text.Json.JsonDocument.Parse(raw);

            var tpls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (doc.RootElement.TryGetProperty("Items", out var itemsEl))
                foreach (var item in itemsEl.EnumerateArray())
                    if (item.TryGetProperty("Id", out var idEl))
                        tpls.Add(idEl.GetString() ?? "");

            return tpls;
        }
        catch (Exception ex)
        {
            logger.LogWarning("[RZDataDump] Failed to parse vanilla_handbook.json: {Err}", ex.Message);
            return null;
        }
    }
}
