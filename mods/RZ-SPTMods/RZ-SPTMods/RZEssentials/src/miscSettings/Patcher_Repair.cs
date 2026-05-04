// RemzDNB - 2026

using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Servers;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Services;
using SptRepairConfig = SPTarkov.Server.Core.Models.Spt.Config.RepairConfig;

namespace RZEssentials.MiscSettings;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Patcher_Repair(
    ConfigServer configServer,
    ConfigLoader configLoader,
    RzeLogger log
) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<RepairConfig>();

        PatchNoRepairDegradation(config);
        PatchRepairKit(config);

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // NoRepairDegradation
    // ─────────────────────────────────────────────────────────────────────────

    private void PatchNoRepairDegradation(RepairConfig config)
    {
        if (config.NoRepairDegradation != true)
            return;

        var harmony = new Harmony("com.rz.essentials.repairdegradation");
        harmony.Patch(
            AccessTools.Method(typeof(RepairHelper), nameof(RepairHelper.UpdateItemDurability)),
            prefix: new HarmonyMethod(typeof(RepairDegradationPatch), nameof(RepairDegradationPatch.Prefix))
        );

        log.Info(LogChannel.MiscSettings, "Repair: degradation disabled.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RepairKit buffs
    // ─────────────────────────────────────────────────────────────────────────

    private void PatchRepairKit(RepairConfig config)
    {
        if (config.RepairKit is null || !config.RepairKit.Enabled)
            return;

        var kit = configServer.GetConfig<SptRepairConfig>().RepairKit;

        PatchBonusSettings(kit.Armor, config.RepairKit.Armor);
        PatchBonusSettings(kit.Weapon, config.RepairKit.Weapon);
        PatchBonusSettings(kit.Vest, config.RepairKit.Vest);
        PatchBonusSettings(kit.Headwear, config.RepairKit.Headwear);

        log.Info(LogChannel.MiscSettings, "Repair: kit buffs patched.");
    }

    private static void PatchBonusSettings(
        SPTarkov.Server.Core.Models.Spt.Config.BonusSettings target,
        RepairKitBonusSettings? patch)
    {
        if (patch is null)
            return;

        if (patch.RarityWeight is not null)
            foreach (var (key, value) in patch.RarityWeight)
                target.RarityWeight[key] = value;

        if (patch.BonusTypeWeight is not null)
            foreach (var (key, value) in patch.BonusTypeWeight)
                target.BonusTypeWeight[key] = value;

        if (patch.Common is not null)
            PatchBonusTier(target.Common, patch.Common);

        if (patch.Rare is not null)
            PatchBonusTier(target.Rare, patch.Rare);
    }

    private static void PatchBonusTier(
        Dictionary<string, SPTarkov.Server.Core.Models.Spt.Config.BonusValues> target,
        Dictionary<string, RepairBonusValues> patch)
    {
        foreach (var (bonusType, values) in patch)
        {
            if (!target.TryGetValue(bonusType, out var existing))
                continue;

            if (values.ValuesMinMax is not null)
            {
                existing.ValuesMinMax.Min = values.ValuesMinMax.Min;
                existing.ValuesMinMax.Max = values.ValuesMinMax.Max;
            }

            if (values.ActiveDurabilityPercentMinMax is not null)
            {
                existing.ActiveDurabilityPercentMinMax.Min = values.ActiveDurabilityPercentMinMax.Min;
                existing.ActiveDurabilityPercentMinMax.Max = values.ActiveDurabilityPercentMinMax.Max;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Harmony patch
    // ─────────────────────────────────────────────────────────────────────────

    public static class RepairDegradationPatch
    {
        public static void Prefix(
            Item itemToRepair,
            TemplateItem itemToRepairDetails,
            ref double amountToRepair,
            ref bool applyMaxDurabilityDegradation)
        {
            applyMaxDurabilityDegradation = false;

            var repairable = itemToRepair.Upd?.Repairable;
            if (repairable is null)
                return;

            var templateMax = itemToRepairDetails.Properties?.MaxDurability;
            if (templateMax is > 0)
                repairable.MaxDurability = templateMax;

            amountToRepair = (repairable.MaxDurability ?? 0) - (repairable.Durability ?? 0);
        }
    }
}
