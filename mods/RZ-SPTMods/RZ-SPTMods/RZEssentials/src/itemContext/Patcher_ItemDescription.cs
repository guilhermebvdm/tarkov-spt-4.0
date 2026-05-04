// Patcher_ItemDescription.cs — RemzDNB 2026

using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.ItemContext;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1001)]
public class Patcher_ItemDescription(
    DatabaseService databaseService,
    LocaleService localeService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    private const string GreyHeader = "<color=grey>";
    private const string RedTag = "<color=#ff4444>";
    private const string GreenTag = "<color=#44ff44>";
    private const string WhiteTag = "<color=white>";
    private const string HighlightTag = "<color=#ffff00>";
    private const string CloseTag = "</color>";

    public Task OnLoad()
    {
        var config = configLoader.Load<ItemContextConfig>();
        if (!config.EnableItemContext)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.ItemContext, "ItemDescriptionPatcher : Templates.Items is null — aborting.");
            return Task.CompletedTask;
        }

        var enLocale = localeService.GetLocaleDb("en");
        var patches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        PatchDescriptionCleanup(config, items, patches);
        PatchCraftInfo(config, items, enLocale, patches);
        PatchHideoutInfo(config, items, enLocale, patches);
        PatchBarterInfo(config, items, enLocale, patches);
        PatchQuestInfo(config, items, enLocale, patches);
        PatchContainerInfo(config, items, patches);
        PatchHeadsetInfo(config, items, patches);
        PatchArmorPlateInfo(config, items, patches);
        PatchKeyInfo(config, patches);

        if (patches.Count == 0)
            return Task.CompletedTask;

        foreach (var (_, lazyLoad) in databaseService.GetLocales().Global)
        {
            lazyLoad.AddTransformer(dict =>
            {
                if (dict is null) return dict;

                foreach (var (tpl, description) in patches)
                    dict[$"{tpl} Description"] = description;

                return dict;
            });
        }

        log.Info(LogChannel.ItemContext, $"ItemDescriptionPatcher : {patches.Count} item description(s) patched.");
        return Task.CompletedTask;
    }

    private void PatchDescriptionCleanup(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> patches)
    {
        if (!config.DescriptionCleanup.Enabled)
            return;

        foreach (var categoryId in config.DescriptionCleanup.Categories)
            foreach (var tpl in BuildCategoryTplSet(categoryId, items))
                patches[tpl] = "";
    }

    private void PatchCraftInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> enLocale,
        Dictionary<string, string> patches)
    {
        if (!config.EnableCraftingInfo && !config.EnableCraftingToolInfo)
            return;

        var recipes = databaseService.GetTables().Hideout?.Production?.Recipes;
        if (recipes is null) return;

        var craftMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var craftToolMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var recipe in recipes)
        {
            if (recipe.EndProduct == default) continue;

            var endName = ResolveShortName(recipe.EndProduct.ToString(), enLocale, items);
            var endCount = recipe.Count ?? 1;
            var endStr = endCount > 1 ? $"{endName} x{endCount}" : endName;

            if (config.EnableCraftingInfo)
            {
                var ingredients = recipe.Requirements?
                    .Where(r => r.Type == "Item" && r.TemplateId is not null)
                    .ToList();

                if (ingredients is not null && ingredients.Count > 0)
                {
                    var parts = ingredients.Select(req => (
                        Tpl: req.TemplateId!.ToString(),
                        Line: $"{ResolveShortName(req.TemplateId!.ToString(), enLocale, items)} x{req.Count ?? 1}"
                    )).ToList();

                    foreach (var (tpl, _) in parts)
                    {
                        if (!craftMap.TryGetValue(tpl, out var lines))
                            craftMap[tpl] = lines = [];
                        lines.Add(BuildRecipeLine(endStr, parts, tpl));
                    }
                }
            }

            if (config.EnableCraftingToolInfo)
            {
                var tools = recipe.Requirements?
                    .Where(r => r.Type == "Tool" && r.TemplateId is not null)
                    .ToList();

                if (tools is not null && tools.Count > 0)
                {
                    foreach (var tool in tools)
                    {
                        var tpl = tool.TemplateId!.ToString();
                        if (!craftToolMap.TryGetValue(tpl, out var lines))
                            craftToolMap[tpl] = lines = [];
                        if (!lines.Contains(endStr))
                            lines.Add(endStr);
                    }
                }
            }
        }

        AppendSection(patches, craftMap, "[ Crafting Components ]");
        AppendInlineSection(patches, craftToolMap, "[ Crafting Tools ]");
    }

    private void PatchHideoutInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> enLocale,
        Dictionary<string, string> patches)
    {
        if (!config.EnableHideoutInfo) return;

        var areas = databaseService.GetTables().Hideout?.Areas;
        if (areas is null) return;

        var hideoutMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var area in areas)
        {
            if (area.Stages is null || !area.Type.HasValue) continue;

            var areaName = area.Type.Value.ToString();
            if (config.HideoutExcludedAreas.Any(e => string.Equals(e, areaName, StringComparison.OrdinalIgnoreCase)))
                continue;

            foreach (var (levelStr, stage) in area.Stages)
            {
                if (levelStr == "0") continue;

                var reqs = stage.Requirements?
                    .Where(r => r.Type == "Item")
                    .ToList();

                if (reqs is null || reqs.Count == 0) continue;

                var label = $"{areaName} Lvl {levelStr}";

                var parts = reqs.Select(req => (
                    Tpl: req.TemplateId!.ToString(),
                    Line: $"{ResolveShortName(req.TemplateId!.ToString(), enLocale, items)} x{req.Count ?? 1}"
                )).ToList();

                foreach (var (tpl, _) in parts)
                {
                    if (!hideoutMap.TryGetValue(tpl, out var lines))
                        hideoutMap[tpl] = lines = [];
                    lines.Add(BuildRecipeLine(label, parts, tpl));
                }
            }
        }

        AppendSection(patches, hideoutMap, "[ Hideout ]");
    }

    private void PatchBarterInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> enLocale,
        Dictionary<string, string> patches)
    {
        if (!config.EnableBarterInfo) return;

        var currencyTpls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "5449016a4bdc2d6f028b456f",
            "5696686a4bdc2da3298b456a",
            "569668774bdc2da2298b4568",
        };

        var barterMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, trader) in databaseService.GetTraders())
        {
            if (trader.Assort?.BarterScheme is null) continue;

            var nickname = trader.Base?.Nickname ?? "?";

            foreach (var (assortId, schemeList) in trader.Assort.BarterScheme)
            {
                foreach (var scheme in schemeList)
                {
                    if (scheme.All(s => currencyTpls.Contains(s.Template.ToString()))) continue;

                    var loyaltyLevel = trader.Assort.LoyalLevelItems.TryGetValue(assortId, out var ll) ? ll : 1;
                    var rootItem = trader.Assort.Items.FirstOrDefault(i => i.Id == assortId);
                    if (rootItem is null) continue;

                    var soldName = ResolveShortName(rootItem.Template.ToString(), enLocale, items);
                    var label = config.ShowBarterLoyaltyLevel
                        ? $"{soldName} ({nickname} LL{loyaltyLevel})"
                        : $"{soldName} ({nickname})";

                    var parts = scheme.Select(s => (
                        Tpl: s.Template.ToString(),
                        Line: $"{ResolveShortName(s.Template.ToString(), enLocale, items)} x{(int)s.Count}"
                    )).ToList();

                    foreach (var (tpl, _) in parts)
                    {
                        if (!barterMap.TryGetValue(tpl, out var lines))
                            barterMap[tpl] = lines = [];
                        lines.Add(BuildRecipeLine(label, parts, tpl));
                    }
                }
            }
        }

        AppendSection(patches, barterMap, "[ Barters ]");
    }

    private void PatchQuestInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> enLocale,
        Dictionary<string, string> patches)
    {
        if (!config.EnableQuestInfo)
            return;

        var quests = databaseService.GetTables().Templates?.Quests;
        if (quests is null) return;

        var traders = databaseService.GetTraders();
        var questMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, quest) in quests)
        {
            var conditions = quest.Conditions?.AvailableForFinish;
            if (conditions is null) continue;

            var traderName = traders.TryGetValue(quest.TraderId, out var trader)
                ? trader.Base?.Nickname ?? "?"
                : "?";

            var questName = enLocale.TryGetValue($"{quest.Id} name", out var qn) && !string.IsNullOrWhiteSpace(qn)
                ? qn
                : quest.QuestName ?? quest.Id.ToString();

            var label = $"{questName} ({traderName})";

            foreach (var condition in conditions)
            {
                if (condition.ConditionType != "HandoverItem" &&
                    condition.ConditionType != "FindItem" &&
                    condition.ConditionType != "LeaveItemAtLocation")
                    continue;

                var targets = condition.Target?.IsList == true
                    ? condition.Target.List
                    : condition.Target?.IsItem == true && condition.Target.Item is not null
                        ? [condition.Target.Item]
                        : null;

                if (targets is null || targets.Count == 0) continue;
                foreach (var tpl in targets)
                {
                    if (!questMap.TryGetValue(tpl, out var lines))
                        questMap[tpl] = lines = [];

                    var line = $"{label}";
                    if (!lines.Contains(line))
                        lines.Add(line);
                }
            }
        }

        AppendSection(patches, questMap, "[ Quests ]");
    }

    private void PatchContainerInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> patches)
    {
        if (!config.EnableContainerInfo) return;

        var containerCategoryIds = new[]
        {
            "566965d44bdc2d814c8b4571",
            "5671435f4bdc2d96058b4569",
            "5448bf274bdc2dfc2f8b456a",
            "566168634bdc2d144c8b456c",
            "5795f317245977243854e041",
        };

        var containerMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var containerTpls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var categoryId in containerCategoryIds)
            foreach (var tpl in BuildCategoryTplSet(categoryId, items))
                containerTpls.Add(tpl);

        foreach (var tpl in containerTpls)
        {
            if (!items.TryGetValue(tpl, out var item) || item.Properties is null) continue;

            var grids = item.Properties.Grids?.ToList();
            if (grids is null || grids.Count == 0) continue;

            var itemW = item.Properties.Width ?? 1;
            var itemH = item.Properties.Height ?? 1;
            var itemSlots = itemW * itemH;

            var totalSlots = grids.Sum(g => (g.Properties?.CellsH ?? 0) * (g.Properties?.CellsV ?? 0));
            if (totalSlots <= 0) continue;

            var efficiency = itemSlots > 0 ? (double)totalSlots / itemSlots : 0;
            var efficiencyColor = efficiency > 1.0 ? GreenTag : efficiency < 1.0 ? RedTag : WhiteTag;
            var efficiencyStr = $"{efficiencyColor}x{efficiency:F1}{CloseTag}";

            containerMap[tpl] =
            [
                $"Size: {itemW}x{itemH} ({itemSlots} slots)",
                $"Capacity: {totalSlots} slots",
                $"Efficiency: {efficiencyStr}",
            ];
        }

        AppendSection(patches, containerMap, "[ Size ]");
    }

    private void PatchHeadsetInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> patches)
    {
        if (!config.EnableHeadsetInfo) return;

        var headsetMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var tpl in BuildCategoryTplSet("5645bcb74bdc2ded0b8b4578", items))
        {
            if (!items.TryGetValue(tpl, out var item) || item.Properties is null) continue;

            var p = item.Properties;

            var suppression = (int)Math.Round(
                ((p.AmbientCompressorSendLevel ?? -10) + 10 +
                 (p.EffectsReturnsGrEnvCommonCompressorSendLeveloupVolume ?? -7) + 7 +
                 (p.EnvNatureCompressorSendLevel ?? -5) + 5 +
                 (p.EnvTechnicalCompressorSendLevel ?? -7) + 7)
                * 10.0
            );

            var boost = (p.CompressorGain ?? 0) + Math.Abs((p.CompressorThreshold ?? -20) + 20);
            var suppressionStr = suppression >= 0 ? $"+{suppression}%" : $"{suppression}%";

            headsetMap[tpl] = [$"Boost: +{boost}db | Suppression: {suppressionStr}"];
        }

        AppendSection(patches, headsetMap, "[ Audio ]");
    }

    private void PatchArmorPlateInfo(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> patches)
    {
        if (!config.EnableArmorPlateInfo) return;

        var armorMaterials = databaseService.GetGlobals().Configuration.ArmorMaterials;

        var plateCategoryIds = new[]
        {
            "644120aa86ffbe10ee032b6f", // ArmorPlate
            "65649eb40bf0ed77b8044453", // BuiltInInserts
        };

        var plateTpls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var catId in plateCategoryIds)
        foreach (var tpl in BuildCategoryTplSet(catId, items))
            plateTpls.Add(tpl);

        foreach (var tpl in plateTpls)
        {
            if (!items.TryGetValue(tpl, out var item) || item.Properties is null) continue;

            var props = item.Properties;
            var maxDur = props.MaxDurability ?? 0;
            var armorClass = props.ArmorClass ?? 0;
            var material = props.ArmorMaterial;

            if (maxDur <= 0 || armorClass <= 0 || material is null) continue;
            if (!armorMaterials.TryGetValue(material.Value, out var armorType)) continue;

            var destructibility = armorType.Destructibility;
            if (destructibility <= 0) continue;

            var effectiveDur = (int)Math.Round(maxDur / destructibility);

            var block = $"{GreyHeader}[ Armor ]{CloseTag}\n" +
                        $"Class {armorClass} | Eff. durability: {HighlightTag}{effectiveDur}{CloseTag} " +
                        $"{GreyHeader}(max {maxDur:0} × {Math.Round(1.0 / destructibility, 2)}){CloseTag}";

            patches[tpl] = patches.TryGetValue(tpl, out var existing) && !string.IsNullOrEmpty(existing)
                ? $"{existing}\n\n{block}"
                : block;
        }
    }

    private static void PatchKeyInfo(
        ItemContextConfig config,
        Dictionary<string, string> patches)
    {
        if (!config.EnableKeyInfo) return;

        foreach (var (tpl, description) in config.KeyStats)
        {
            patches[tpl] = patches.TryGetValue(tpl, out var existing) && !string.IsNullOrEmpty(existing)
                ? $"{existing}\n\n{description}"
                : description;
        }
    }

    // Multiline section — one entry per line
    private static void AppendSection(
        Dictionary<string, string> patches,
        Dictionary<string, List<string>> map,
        string header)
    {
        if (map.Count == 0) return;

        foreach (var (tpl, lines) in map)
        {
            var block = $"{GreyHeader}{header}{CloseTag}\n{string.Join("\n", lines.Select(l => $"▸ {l}"))}";

            patches[tpl] = patches.TryGetValue(tpl, out var existing) && !string.IsNullOrEmpty(existing)
                ? $"{existing}\n\n{block}"
                : block;
        }
    }

    // Inline section — all entries on a single line separated by " - "
    private static void AppendInlineSection(
        Dictionary<string, string> patches,
        Dictionary<string, List<string>> map,
        string header)
    {
        if (map.Count == 0) return;

        foreach (var (tpl, lines) in map)
        {
            var block = $"{GreyHeader}{header}{CloseTag}\n{string.Join(" - ", lines)}";

            patches[tpl] = patches.TryGetValue(tpl, out var existing) && !string.IsNullOrEmpty(existing)
                ? $"{existing}\n\n{block}"
                : block;
        }
    }

    private static string BuildRecipeLine(string label, List<(string Tpl, string Line)> parts, string currentTpl)
    {
        var ingredients = string.Join(" + ", parts.Select(p =>
            p.Tpl == currentTpl
                ? $"{HighlightTag}{p.Line}{CloseTag}"
                : p.Line
        ));
        return $"{label} = {ingredients}";
    }

    private static string ResolveShortName(
        string tpl,
        Dictionary<string, string> enLocale,
        Dictionary<MongoId, TemplateItem> items)
    {
        if (enLocale.TryGetValue($"{tpl} ShortName", out var sn) && !string.IsNullOrWhiteSpace(sn)) return sn;
        if (enLocale.TryGetValue($"{tpl} Name", out var n) && !string.IsNullOrWhiteSpace(n)) return n;
        if (items.TryGetValue(tpl, out var item) && !string.IsNullOrWhiteSpace(item.Name)) return item.Name;
        return tpl;
    }

    private static HashSet<string> BuildCategoryTplSet(string categoryId, Dictionary<MongoId, TemplateItem> items)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (mongoId, item) in items)
        {
            if (item.Properties is null) continue;

            var current = item.Parent.ToString();
            while (!string.IsNullOrEmpty(current))
            {
                if (current == categoryId)
                {
                    result.Add(mongoId.ToString());
                    break;
                }
                if (!items.TryGetValue(current, out var parent)) break;
                current = parent.Parent.ToString();
            }
        }

        return result;
    }
}
