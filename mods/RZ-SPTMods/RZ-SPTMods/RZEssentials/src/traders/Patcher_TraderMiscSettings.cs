// RemzDNB - 2026

using HarmonyLib;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Traders;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Patcher_TraderMiscSettings(
    DatabaseService db,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    private readonly TraderMiscSettingsConfig _config = configLoader.Load<TraderMiscSettingsConfig>();

    public Task OnLoad()
    {
        // Unlock Jaeger and Ref
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        if (_config.UnlockJaeger)
        {
            var traders = db.GetTraders();
            if (traders.TryGetValue(SPTarkov.Server.Core.Models.Enums.Traders.JAEGER, out var jaegerTrader)) {
                jaegerTrader.Base.UnlockedByDefault = true;
            }
        }

        if (_config.UnlockRef)
        {
            var traders = db.GetTraders();
            if (traders.TryGetValue(SPTarkov.Server.Core.Models.Enums.Traders.REF, out var refTrader)) {
                refTrader.Base.UnlockedByDefault = true;
            }
        }

        PatchLoyaltyLevels();
        PatchRepair();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // LOYALTY LEVELS
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void PatchLoyaltyLevels()
    {
        if (!_config.EnableLoyaltyLevels)
            return;

        var traders = db.GetTables().Traders;

        foreach (var (traderId, levels) in _config.LoyaltyLevelsOverrides)
        {
            if (!traders.TryGetValue(traderId, out var trader))
                continue;

            foreach (var level in trader.Base.LoyaltyLevels!)
                level.BuyPriceCoefficient = 50;

            log.Info(LogChannel.Traders, $"LoyaltyLevels: '{traderId}' — {levels.Count} level(s) applied.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // REPAIR
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void PatchRepair()
    {
        if (!_config.EnableRepairOverrides)
            return;

        // Per-trader settings (availability, currency, quality, price rate, exclusions...)
        var traders = db.GetTables().Traders;

        foreach (var (traderId, patch) in _config.RepairOverrides)
        {
            if (!traders.TryGetValue(traderId, out var trader))
            {
                log.Warning(LogChannel.Traders, $"Repair: trader '{traderId}' not found, skipping.");
                continue;
            }

            var repair = trader.Base.Repair;
            if (repair is null)
            {
                log.Warning(LogChannel.Traders, $"Repair: trader '{traderId}' has no repair block, skipping.");
                continue;
            }

            if (patch.Availability is not null)      repair.Availability      = patch.Availability;
            if (patch.Currency is not null)          repair.Currency          = patch.Currency;
            if (patch.CurrencyCoefficient is not null) repair.CurrencyCoefficient = patch.CurrencyCoefficient;
            if (patch.Quality is not null)           repair.Quality           = patch.Quality;
            if (patch.PriceRate is not null)         repair.PriceRate         = patch.PriceRate;
            if (patch.ExcludedIdList is not null)    repair.ExcludedIdList    = patch.ExcludedIdList;
            if (patch.ExcludedCategory is not null)  repair.ExcludedCategory  = patch.ExcludedCategory;
        }

        log.Info(LogChannel.Traders, $"Repair: {_config.RepairOverrides.Count} trader(s) patched.");
    }
}
