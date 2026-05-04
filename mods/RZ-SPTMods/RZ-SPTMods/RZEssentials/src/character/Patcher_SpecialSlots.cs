// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Character;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class SpecialSlotsPatcher(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    private static readonly string[] PocketTpls =
    [
        "627a4e6b255f7527fb05a0f6", // Standard pockets
        "65e080be269cbd5c5005e529", // Post-quest pockets
    ];

    public Task OnLoad()
    {
        var config = configLoader.Load<SpecialSlotsConfig>();
        if (!config.Enabled || config.Items.Count == 0)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.Character, "SpecialSlots: Templates.Items is null, aborting.");
            return Task.CompletedTask;
        }

        var toAdd = config.Items.Where(e => e.Enabled  && !string.IsNullOrWhiteSpace(e.Tpl)).ToList();
        var toRemove = config.Items.Where(e => !e.Enabled && !string.IsNullOrWhiteSpace(e.Tpl)).ToList();

        foreach (var tpl in PocketTpls)
        {
            if (!items.TryGetValue(new MongoId(tpl), out var pocketTemplate))
            {
                log.Warning(LogChannel.Character, $"SpecialSlots: pocket TPL '{tpl}' not found in DB, skipping.");
                continue;
            }

            var slots = pocketTemplate.Properties?.Slots;
            if (slots is null)
                continue;

            foreach (var slot in slots)
            {
                if (slot.Name is null || !slot.Name.StartsWith("SpecialSlot", StringComparison.OrdinalIgnoreCase))
                    continue;

                var filtersList = slot.Properties?.Filters?.ToList();
                if (filtersList is null || filtersList.Count == 0)
                    continue;

                var filterEntry = filtersList[0];
                if (filterEntry.Filter is null)
                    continue;

                foreach (var entry in toAdd)
                {
                    var id = new MongoId(entry.Tpl);
                    filterEntry.Filter.Add(id);

                    if (items.TryGetValue(id, out var allowedItem) && allowedItem.Properties is not null)
                    {
                        allowedItem.Properties.DiscardLimit = -1;
                        allowedItem.Properties.InsuranceDisabled = true;
                    }
                }

                foreach (var entry in toRemove)
                    filterEntry.Filter.Remove(new MongoId(entry.Tpl));
            }
        }

        log.Info(LogChannel.Character, $"SpecialSlots: {toAdd.Count} added, {toRemove.Count} removed.");
        return Task.CompletedTask;
    }
}
