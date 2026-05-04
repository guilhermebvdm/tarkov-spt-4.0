// RemzDNB - 2026

using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Helpers;

namespace RZEssentials.Traders;

[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 2)]
public class Patcher_Trades_Routed(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    AssortUtilities assortUtilities,
    ModHelper modHelper,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var routedTradesConfig = configLoader.Load<RoutedTradesConfig>();
        var traders = databaseService.GetTraders();
        var handbook = databaseService.GetTables().Templates?.Handbook;
        var dbItems = databaseService.GetTables().Templates?.Items;

        if (handbook is null)
        {
            log.Warning(LogChannel.Traders, "Handbook is null, skipping auto-routing.");
            return Task.CompletedTask;
        }

        if (dbItems is null)
        {
            log.Warning(LogChannel.Traders, "Templates.Items is null, skipping auto-routing.");
            return Task.CompletedTask;
        }

        // 1. Build blacklist.

        var blacklist = new HashSet<string>(routedTradesConfig.Blacklist, StringComparer.OrdinalIgnoreCase);

        // 2. Build modded item set.

        if (routedTradesConfig.RouteModdedItemsOnly && routedTradesConfig.RouteVanillaItemsOnly)
        {
            log.Warning(LogChannel.Traders, "RouteModdedItemsOnly and RouteVanillaItemsOnly are both true & mutually exclusive, both will be ignored.");
            routedTradesConfig.RouteModdedItemsOnly = false;
            routedTradesConfig.RouteVanillaItemsOnly = false;
        }

        HashSet<string>? moddedTpls = null;
        if (routedTradesConfig.RouteModdedItemsOnly || routedTradesConfig.RouteVanillaItemsOnly)
        {
            moddedTpls = BuildModdedItemSet(dbItems);
            log.Info(LogChannel.Traders, $"Detected {moddedTpls.Count} modded item(s) via items.json diff.");
        }

        // 3. Resolve exchange rates for currency conversion.

        var handbookPrices = handbook.Items.ToDictionary(e => e.Id.ToString(), e => (double)(e.Price ?? 0));
        var dollarPriceInRub = handbookPrices.GetValueOrDefault(ItemTpl.MONEY_DOLLARS.ToString(), 0.0);
        var euroPriceInRub = handbookPrices.GetValueOrDefault(ItemTpl.MONEY_EUROS.ToString(), 0.0);

        if (dollarPriceInRub <= 0)
            log.Warning(LogChannel.Traders, "RoutedTrades: handbook price for USD is 0 or missing, USD traders will fallback to roubles.");
        if (euroPriceInRub <= 0)
            log.Warning(LogChannel.Traders, "RoutedTrades: handbook price for EUR is 0 or missing, EUR traders will fallback to roubles.");

        // 4. Build category route map : itemTpl -> List<(traderId, route)>.

        var categoryToTraderRoutes = BuildCategoryRouteMap(
            dbItems,
            routedTradesConfig.CategoryRoutes,
            routedTradesConfig.ForceRouteAll
        );

        int routed = 0, skipped = 0;

        // 5. Route.

        foreach (var hbItem in handbook.Items)
        {
            var itemTpl = hbItem.Id.ToString();

            if (blacklist.Contains(itemTpl) && !routedTradesConfig.ForceRouteAll)
            {
                skipped++;
                continue;
            }

            if (moddedTpls is not null)
            {
                var isModded = moddedTpls.Contains(itemTpl);
                if (routedTradesConfig.RouteModdedItemsOnly && !isModded || routedTradesConfig.RouteVanillaItemsOnly && isModded)
                {
                    skipped++;
                    continue;
                }
            }

            if (!categoryToTraderRoutes.TryGetValue(hbItem.Id, out var traderRoutes))
            {
                if (routedTradesConfig.ForceRouteAll && routedTradesConfig.FallbackTrader is not null)
                {
                    if (traders.TryGetValue(routedTradesConfig.FallbackTrader, out var fallbackTrader))
                    {
                        var fallbackPrice = Math.Max(1, (int)Math.Round(
                            assortUtilities.GetTotalHandbookPrice(hbItem.Id, handbookPrices)
                        ));
                        InjectAutoOffer(fallbackTrader.Assort, hbItem.Id, fallbackPrice, ItemTpl.MONEY_ROUBLES, -1, 1, 100);
                        routed++;
                    }
                }
                else
                {
                    skipped++;
                }
                continue;
            }

            foreach (var (traderId, route) in traderRoutes)
            {
                if (!traders.TryGetValue(traderId, out var trader))
                {
                    if (traderId != CustomTraderConfig.TraderId)
                        log.Warning(LogChannel.Traders, $"Route trader '{traderId}' not found, skipping.");
                    continue;
                }

                var currency = routedTradesConfig.TraderCurrencies.GetValueOrDefault(traderId, TradeCurrency.Rub);
                var (currencyTpl, price) = ResolvePrice(
                    assortUtilities.GetTotalHandbookPrice(hbItem.Id, handbookPrices),
                    route.PriceMultiplier,
                    currency,
                    dollarPriceInRub, euroPriceInRub
                );
                InjectAutoOffer(trader.Assort, hbItem.Id, price, currencyTpl, -1, route.LoyaltyLevel, 100);
                routed++;
            }
        }

        if (routed > 0)
            log.Info(LogChannel.Traders, $"{routed} auto-routed, {skipped} skipped.{(routedTradesConfig.ForceRouteAll ? " [ForceRouteAll ON]" : "")}");

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ResolvePrice
    // Converts a rouble handbook price to the target currency.
    // ─────────────────────────────────────────────────────────────────────────

    private static (MongoId CurrencyTpl, int Price) ResolvePrice(
        double priceRub,
        double multiplier,
        TradeCurrency currency,
        double dollarPriceInRub,
        double euroPriceInRub
    )
    {
        var adjusted = priceRub * multiplier;
        return currency switch
        {
            TradeCurrency.Usd when dollarPriceInRub > 0 =>
                (ItemTpl.MONEY_DOLLARS, Math.Max(1, (int)Math.Round(adjusted / dollarPriceInRub))),
            TradeCurrency.Eur when euroPriceInRub > 0 =>
                (ItemTpl.MONEY_EUROS, Math.Max(1, (int)Math.Round(adjusted / euroPriceInRub))),
            _ =>
                (ItemTpl.MONEY_ROUBLES, Math.Max(1, (int)Math.Round(adjusted))),
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // InjectAutoOffer
    // ─────────────────────────────────────────────────────────────────────────

    private void InjectAutoOffer(
        TraderAssort assort,
        MongoId tpl,
        int price,
        MongoId currencyTpl,
        int stackCount,
        int loyaltyLevel,
        int durability
    )
    {
        var itemId = new MongoId();

        assort.Items.Add(new Item
        {
            Id = itemId,
            Template = tpl,
            ParentId = "hideout",
            SlotId = "hideout",
            Upd = new Upd
            {
                UnlimitedCount = stackCount <= 0,
                StackObjectsCount = stackCount <= 0 ? 999999 : stackCount,
                BuyRestrictionMax = null,
                BuyRestrictionCurrent = null,
            },
        });

        assortUtilities.ResolveRequiredChildren(assort.Items, itemId, tpl, durability, new HashSet<string>());
        assort.BarterScheme[itemId] = [[new BarterScheme { Template = currencyTpl, Count = price }]];
        assort.LoyalLevelItems[itemId] = loyaltyLevel;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildCategoryRouteMap
    // Returns a map of itemTpl -> List<(traderId, route)>.
    // Supports multiple traders per category.
    // ─────────────────────────────────────────────────────────────────────────

    private static Dictionary<MongoId, List<(string TraderId, CategoryRoute Route)>> BuildCategoryRouteMap(
        Dictionary<MongoId, TemplateItem> dbItems,
        Dictionary<string, List<CategoryRoute>> categoryRoutes,
        bool forceRouteAll
    )
    {
        var directRoutes = new Dictionary<string, List<(string TraderId, CategoryRoute Route)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (traderId, routes) in categoryRoutes)
        {
            foreach (var route in routes)
            {
                if (!forceRouteAll && !route.Enabled)
                    continue;

                if (!directRoutes.TryGetValue(route.CategoryId, out var list))
                {
                    list = new List<(string, CategoryRoute)>();
                    directRoutes[route.CategoryId] = list;
                }

                list.Add((traderId, route));
            }
        }

        var result = new Dictionary<MongoId, List<(string, CategoryRoute)>>();

        foreach (var (tpl, _) in dbItems)
        {
            var resolved = FindRoutesForItem(tpl.ToString(), dbItems, directRoutes);
            if (resolved is { Count: > 0 })
                result[tpl] = resolved;
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FindRoutesForItem
    // Walks the DB parent chain, returns all routes matching the first
    // category hit. Supports multiple traders on the same category.
    // ─────────────────────────────────────────────────────────────────────────

    private static List<(string TraderId, CategoryRoute Route)>? FindRoutesForItem(
        string tpl,
        Dictionary<MongoId, TemplateItem> dbItems,
        Dictionary<string, List<(string TraderId, CategoryRoute Route)>> directRoutes
    )
    {
        if (!dbItems.TryGetValue(tpl, out var item))
            return null;

        var current = item.Parent.ToString();

        while (!string.IsNullOrEmpty(current))
        {
            if (directRoutes.TryGetValue(current, out var routes))
                return routes;

            if (!dbItems.TryGetValue(current, out var parent))
                break;

            current = parent.Parent.ToString();
        }

        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildModdedItemSet
    // ─────────────────────────────────────────────────────────────────────────

    private HashSet<string> BuildModdedItemSet(Dictionary<MongoId, TemplateItem> dbItems)
    {
        var sptRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
            "..", "..", ".."
        ));

        var vanillaPath = System.IO.Path.Combine(sptRoot, "SPT_Data", "database", "templates", "items.json");

        if (!File.Exists(vanillaPath))
        {
            log.Error(LogChannel.Traders, $"vanilla items.json not found at '{vanillaPath}' — RouteModdedItemsOnly / RouteVanillaItemsOnly will have no effect.");
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        HashSet<string> vanillaTpls;
        try
        {
            var raw = File.ReadAllText(vanillaPath);
            using var doc = System.Text.Json.JsonDocument.Parse(raw);

            vanillaTpls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
                vanillaTpls.Add(prop.Name);
        }
        catch (Exception ex)
        {
            log.Error(LogChannel.Traders, $"Failed to parse SPT items.json: {ex.Message}");
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return dbItems.Keys
            .Select(k => k.ToString())
            .Where(tpl => !vanillaTpls.Contains(tpl))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
