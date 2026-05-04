// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Character;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Patcher_HolsterSlot(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    private const string PmcTemplateTpl = "55d7217a4bdc2d86028b456d";

    // Maps each toggle to its category ID.
    private static readonly Dictionary<string, string> CategoryIds = new()
    {
        ["AssaultCarbine"]  = "5447b5fc4bdc2d87278b4567",
        ["AssaultRifle"]    = "5447b5f14bdc2d61278b4567",
        ["GrenadeLauncher"] = "5447bedf4bdc2d87278b4568",
        ["MachineGun"]      = "5447bed64bdc2d97278b4568",
        ["MarksmanRifle"]   = "5447b6194bdc2d67278b4567",
        ["Pistol"]          = "5447b5cf4bdc2d65278b4567",
        ["Revolver"]        = "617f1ef5e8b54b0998387733",
        ["RocketLauncher"]  = "67446d4f04141c10630604e7",
        ["Shotgun"]         = "5447b6094bdc2dc3278b4567",
        ["Smg"]             = "5447b5e04bdc2d62278b4567",
        ["SniperRifle"]     = "5447b6254bdc2dc3278b4568",
        ["SpecialWeapon"]   = "5447bee84bdc2dc3278b4569",
        ["Knife"]           = "5447e1d04bdc2dff2f8b4567",
    };

    public Task OnLoad()
    {
        var config = configLoader.Load<HolsterSlotConfig>();
        if (!config.Enabled)
            return Task.CompletedTask;

        var items = databaseService.GetTables().Templates?.Items;
        if (items is null)
        {
            log.Error(LogChannel.Character, "HolsterSlot: Templates.Items is null — aborting.");
            return Task.CompletedTask;
        }

        if (!items.TryGetValue(PmcTemplateTpl, out var pmcTemplate) || pmcTemplate.Properties?.Slots is null)
        {
            log.Error(LogChannel.Character, $"HolsterSlot: PMC template '{PmcTemplateTpl}' not found or has no slots — aborting.");
            return Task.CompletedTask;
        }

        var holsterSlot = pmcTemplate.Properties.Slots
            .FirstOrDefault(s => string.Equals(s.Name, "Holster", StringComparison.OrdinalIgnoreCase));

        if (holsterSlot?.Properties?.Filters is null)
        {
            log.Error(LogChannel.Character, "HolsterSlot: holster slot or its filters not found — aborting.");
            return Task.CompletedTask;
        }

        var firstFilter = holsterSlot.Properties.Filters.FirstOrDefault();
        if (firstFilter is null)
        {
            log.Error(LogChannel.Character, "HolsterSlot: holster slot has no filter entries — aborting.");
            return Task.CompletedTask;
        }

        firstFilter.Filter ??= new HashSet<MongoId>();

        // Resolve enabled category IDs from toggles.

        var toggles = config.Categories;
        var enabledCategoryIds = CategoryIds
            .Where(kvp => IsToggled(toggles, kvp.Key))
            .Select(kvp => kvp.Value)
            .ToList();

        var toAdd = new HashSet<string>(config.AllowedTpls, StringComparer.OrdinalIgnoreCase);

        if (enabledCategoryIds.Count > 0)
            toAdd.UnionWith(ItemUtils.BuildCategoryTplSet(enabledCategoryIds, items));

        if (toAdd.Count == 0)
        {
            log.Warning(LogChannel.Character, "HolsterSlot: no categories or TPLs enabled — nothing to do.");
            return Task.CompletedTask;
        }

        var injected = 0;
        foreach (var tpl in toAdd)
        {
            if (firstFilter.Filter.Add(new MongoId(tpl)))
                injected++;
        }

        log.Info(LogChannel.Character, $"HolsterSlot: {injected} item(s) added to holster filter.");
        return Task.CompletedTask;
    }

    private static bool IsToggled(HolsterCategoryToggles t, string name) => name switch
    {
        "AssaultCarbine"  => t.AssaultCarbine,
        "AssaultRifle"    => t.AssaultRifle,
        "GrenadeLauncher" => t.GrenadeLauncher,
        "MachineGun"      => t.MachineGun,
        "MarksmanRifle"   => t.MarksmanRifle,
        "Pistol"          => t.Pistol,
        "Revolver"        => t.Revolver,
        "RocketLauncher"  => t.RocketLauncher,
        "Shotgun"         => t.Shotgun,
        "Smg"             => t.Smg,
        "SniperRifle"     => t.SniperRifle,
        "SpecialWeapon"   => t.SpecialWeapon,
        "Knife"           => t.Knife,
        _                 => false,
    };
}
