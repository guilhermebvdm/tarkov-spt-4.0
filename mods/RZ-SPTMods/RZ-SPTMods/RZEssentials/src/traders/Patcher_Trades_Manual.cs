// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Traders;

[Injectable(TypePriority = OnLoadOrder.RagfairCallbacks - 2)]
public class Patcher_Trades_Manual(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    AssortUtilities assortUtilities,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var manualTradesConfig = configLoader.Load<ManualTradesConfig>();
        if (manualTradesConfig.ManualOffers.Count == 0)
            return Task.CompletedTask;

        var traders = databaseService.GetTraders();
        var manualById = manualTradesConfig.ManualOffers.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);

        var injected = 0;
        foreach (var (id, trader) in traders)
        {
            if (!manualById.TryGetValue(id.ToString(), out var manualOffers))
                continue;

            var validOffers = manualOffers.Offers.Where(o => ValidateOffer(o, id.ToString())).ToList();
            InjectManualOffers(trader.Assort, validOffers);
            injected += validOffers.Count;
        }

        if (injected > 0)
            log.Info(LogChannel.Traders, $"{injected} manual offer(s) injected.");

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // InjectManualOffers
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void InjectManualOffers(TraderAssort assort, List<TradeOffer> offers)
    {
        foreach (var offer in offers)
        {
            var itemId = new MongoId();

            assort.Items.Add(
                new Item
                {
                    Id = itemId,
                    Template = offer.Tpl,
                    ParentId = "hideout",
                    SlotId = "hideout",
                    Upd = new Upd
                    {
                        UnlimitedCount = offer.StackCount <= 0,
                        StackObjectsCount = offer.StackCount <= 0 ? 999999 : offer.StackCount,
                        BuyRestrictionMax = null,
                        BuyRestrictionCurrent = null,
                        Repairable = offer.Durability is > 0 and < 100
                            ? new UpdRepairable { MaxDurability = 100, Durability = offer.Durability }
                            : null,
                    },
                }
            );

            foreach (var child in offer.Children)
            {
                assort.Items.Add(
                    new Item
                    {
                        Id = new MongoId(),
                        Template = child.Tpl,
                        ParentId = itemId,
                        SlotId = child.SlotId,
                        Upd = new Upd { StackObjectsCount = child.Count },
                    }
                );
            }

            var manualSlots = offer.Children.Select(c => c.SlotId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            assortUtilities.ResolveRequiredChildren(assort.Items, itemId, offer.Tpl, offer.Durability, manualSlots);

            var barterItems = offer.BarterItems ?? [];
            var price = offer.PriceRoubles;
            if (price <= 0 && barterItems.Count == 0)
            {
                var handbook = databaseService.GetTables().Templates?.Handbook;
                var handbookPrices = handbook?.Items
                    .ToDictionary(e => e.Id.ToString(), e => (double)(e.Price ?? 0));

                if (handbookPrices is not null)
                    price = (int)Math.Round(assortUtilities.GetTotalHandbookPrice(offer.Tpl, handbookPrices));

                if (price <= 0)
                    log.Warning(LogChannel.Traders, $"Manual offer '{offer.Tpl}': no price and no handbook price found, defaulting to 1.");
            }

            assort.BarterScheme[itemId] = new List<List<BarterScheme>>
            {
                assortUtilities.BuildPayment(price, barterItems)
            };
            assort.LoyalLevelItems[itemId] = offer.LoyaltyLevel;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // ValidateOffer
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private bool ValidateOffer(TradeOffer offer, string traderId)
    {
        if (string.IsNullOrWhiteSpace(offer.Tpl))
        {
            log.Error(LogChannel.Traders, $"Manual offer with empty ItemTpl for trader '{traderId}', skipping.");
            return false;
        }

        var emptyBarters = offer.BarterItems.Where(b => string.IsNullOrWhiteSpace(b.Tpl)).ToList();
        if (emptyBarters.Count > 0)
        {
            log.Error(LogChannel.Traders, $"Manual offer '{offer.Tpl}' for trader '{traderId}' has {emptyBarters.Count} barter item(s) with empty Tpl, skipping.");
            return false;
        }

        var emptyChildren = offer.Children.Where(c => string.IsNullOrWhiteSpace(c.Tpl)).ToList();
        if (emptyChildren.Count > 0)
        {
            log.Error(LogChannel.Traders, $"Manual offer '{offer.Tpl}' for trader '{traderId}' has {emptyChildren.Count} child(ren) with empty Tpl, skipping.");
            return false;
        }

        return true;
    }
}
