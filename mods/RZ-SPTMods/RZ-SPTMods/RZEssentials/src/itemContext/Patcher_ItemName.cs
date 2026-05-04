// RemzDNB - 2026

using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.ItemContext;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1001)]
public class Patcher_ItemName(
    DatabaseService databaseService,
    LocaleService localeService,
    ConfigLoader configLoader,
    AssortUtilities assortUtilities,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<ItemContextConfig>();
        if (!config.EnableItemContext)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.ItemContext, "ItemNamePatcher : Templates.Items is null — aborting.");
            return Task.CompletedTask;
        }

        var handbookItems = databaseService.GetTables().Templates?.Handbook?.Items;
        var enLocale = localeService.GetLocaleDb("en");

        var patches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        PatchAmmoNames(config, items, patches);
        PatchArmorPlateNames(config, items, patches);
        PatchHandbookPrices(config, items, handbookItems, patches);

        if (patches.Count == 0)
            return Task.CompletedTask;

        foreach (var (_, lazyLoad) in databaseService.GetLocales().Global)
        {
            lazyLoad.AddTransformer(dict =>
            {
                if (dict is null) return dict;

                foreach (var (tpl, suffix) in patches)
                {
                    var key = $"{tpl} Name";
                    if (dict.TryGetValue(key, out var name) && !string.IsNullOrEmpty(name))
                        dict[key] = $"<color=white>{name}</color> {suffix}";
                }

                return dict;
            });
        }

        log.Info(LogChannel.ItemContext, $"ItemNamePatcher : {patches.Count} item name(s) patched.");
        return Task.CompletedTask;
    }

    private void PatchAmmoNames(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> patches)
    {
        if (!config.AmmoNameEnrichment.Enabled)
            return;

        var ammoTpls = BuildCategoryTplSet("5485a8684bdc2da71d8b4567", items);

        foreach (var tpl in ammoTpls)
        {
            if (!items.TryGetValue(tpl, out var item) || item.Properties is null)
                continue;

            var dmg = (int)Math.Ceiling((double)(item.Properties.Damage ?? 0));
            var pen = (int)Math.Ceiling((double)(item.Properties.PenetrationPower ?? 0));

            var dmgStr = ColoredStat(dmg, config.AmmoNameEnrichment.DamageThresholds);
            var penStr = ColoredStat(pen, config.AmmoNameEnrichment.PenetrationThresholds);

            var suffix = config.AmmoNameEnrichment.ShowPrefixes
                ? $"[dmg:{dmgStr}/pen:{penStr}]"
                : $"[{dmgStr}/{penStr}]";

            patches[tpl] = patches.TryGetValue(tpl, out var existing)
                ? $"{existing} {suffix}"
                : suffix;
        }
    }

    private void PatchArmorPlateNames(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        Dictionary<string, string> patches)
    {
        if (!config.EnableArmorPlateInfo) return;

        var armorMaterials = databaseService.GetGlobals().Configuration.ArmorMaterials;
        if (armorMaterials is null) return;

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

            var suffix = $"[{armorClass}/{effectiveDur}]";

            patches[tpl] = patches.TryGetValue(tpl, out var existing)
                ? $"{existing} {suffix}"
                : suffix;
        }
    }

    private void PatchHandbookPrices(
        ItemContextConfig config,
        Dictionary<MongoId, TemplateItem> items,
        IReadOnlyList<HandbookItem>? handbookItems,
        Dictionary<string, string> patches)
    {
        if (!config.BasePriceDisplay.Enabled || handbookItems is null)
            return;

        var handbookPrices = handbookItems.ToDictionary(
            e => e.Id.ToString(),
            e => (double)(e.Price ?? 0),
            StringComparer.OrdinalIgnoreCase);

        foreach (var (categoryId, enabled) in config.BasePriceDisplay.Categories)
        {
            if (!enabled) continue;

            foreach (var tpl in BuildCategoryTplSet(categoryId, items))
            {
                var price = (int)Math.Round(assortUtilities.GetTotalHandbookPrice(tpl, handbookPrices));
                if (price <= 0) continue;

                var priceStr = config.BasePriceDisplay.PriceThresholds.Count > 0
                    ? ColoredStat(price, config.BasePriceDisplay.PriceThresholds)
                    : price.ToString();

                var suffix = $"[{priceStr}₽]";

                patches[tpl] = patches.TryGetValue(tpl, out var existing)
                    ? $"{existing} {suffix}"
                    : suffix;
            }
        }
    }

    private static string ColoredStat(int value, List<StatThreshold> thresholds)
    {
        var color = thresholds
            .OrderByDescending(t => t.Min)
            .FirstOrDefault(t => value >= t.Min)?
            .Color;

        return color is not null
            ? $"<color={color}>{value}</color>"
            : value.ToString();
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
