// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;

using RZEssentials._Shared;

namespace RZEssentials.Hideout;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Patcher_HideoutAreas(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    private const string TypeItem = "Item";
    private const string TypeArea = "Area";
    private const string TypeSkill = "Skill";
    private const string TypeTraderLoyalty = "TraderLoyalty";

    private readonly HideoutAreasConfig _hideoutAreasConfig = configLoader.Load<HideoutAreasConfig>(HideoutAreasConfig.FileName);

    public Task OnLoad()
    {
        if (!_hideoutAreasConfig.Enabled) {
            return Task.CompletedTask;
        }

        ApplyAreaOverrides();
        PatchBitcoinFarm(_hideoutAreasConfig.BitcoinFarm);

        return Task.CompletedTask;
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Area overrides
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void ApplyAreaOverrides()
    {
        var areas = databaseService.GetTables().Hideout?.Areas;
        if (areas is null || areas.Count == 0) {
            log.Warning(LogChannel.Hideout, "No hideout areas found in database — nothing to patch.");
            return;
        }

        foreach (var (areaName, areaConfig) in _hideoutAreasConfig.Areas)
        {
            if (!Enum.TryParse<HideoutAreas>(areaName, ignoreCase: true, out var areaType)) {
                log.Warning(LogChannel.Hideout, $"Unknown hideout area '{areaName}' in config : skipping.");
                continue;
            }

            var area = areas.FirstOrDefault(a => a.Type == areaType);
            if (area is null) {
                log.Warning(LogChannel.Hideout, $"Area '{areaName}' ({areaType}) not found in database : skipping.");
                continue;
            }

            if (areaConfig.RemoveFromDb == true) {
                areas.RemoveAll(a => a.Type == areaType);
                continue;
            }

            if (areaConfig.Enabled.HasValue)
                area.IsEnabled = areaConfig.Enabled.Value;

            if (areaConfig.DisplayLevel.HasValue)
                area.DisplayLevel = areaConfig.DisplayLevel.Value;

            if (area.Stages is null)
                continue;

            ApplyRequirementPatches(area, areaConfig, areaName);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Requirement patching
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void ApplyRequirementPatches(HideoutArea area, HideoutAreaConfig config, string areaName)
    {
        if (config.UseCustomItemRequirements) {
            ClearRequirementsByType(area, TypeItem);
            InjectRequirements(area, config.ItemRequirements, areaName, InjectItemRequirement);
        }

        if (config.UseCustomAreaRequirements) {
            ClearRequirementsByType(area, TypeArea);
            InjectRequirements(area, config.AreaRequirements, areaName, InjectAreaRequirement);
        }

        if (config.UseCustomSkillRequirements) {
            ClearRequirementsByType(area, TypeSkill);
            InjectRequirements(area, config.SkillRequirements, areaName, InjectSkillRequirement);
        }

        if (config.UseCustomTraderRequirements) {
            ClearRequirementsByType(area, TypeTraderLoyalty);
            InjectRequirements(area, config.TraderRequirements, areaName, InjectTraderRequirement);
        }

        if (config.UseCustomConstructionTime) {
            foreach (var (levelStr, stage) in area.Stages!) {
                stage.ConstructionTime = config.ConstructionTime.TryGetValue(levelStr, out var time) ? time : 0;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Clear
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private static void ClearRequirementsByType(HideoutArea area, string type)
    {
        if (area.Stages is null)
            return;

        foreach (var (_, stage) in area.Stages)
            stage.Requirements?.RemoveAll(r => string.Equals(r.Type, type, StringComparison.OrdinalIgnoreCase));
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Generic inject dispatcher
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void InjectRequirements<T>(
        HideoutArea area,
        Dictionary<string, List<T>> byStage,
        string areaName,
        Func<T, string, string, StageRequirement?> converter)
    {
        if (byStage.Count == 0 || area.Stages is null)
            return;

        foreach (var (levelStr, requirements) in byStage)
        {
            if (!area.Stages.TryGetValue(levelStr, out var stage)) {
                log.Warning(LogChannel.Hideout, $"Area '{areaName}': stage '{levelStr}' not found in database : skipping.");
                continue;
            }

            var converted = requirements
                .Select(req => converter(req, areaName, levelStr))
                .Where(r => r is not null)
                .ToList()!;

            if (converted.Count == 0)
                continue;

            stage.Requirements ??= new List<SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement>();
            stage.Requirements.AddRange(converted);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Converters
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private StageRequirement? InjectItemRequirement(ItemRequirement req, string areaName, string levelStr)
    {
        if (string.IsNullOrEmpty(req.ItemTpl)) {
            log.Warning(LogChannel.Hideout, $"Area '{areaName}' stage '{levelStr}': Item requirement missing ItemTpl : skipping.");
            return null;
        }

        return new StageRequirement {
            Type = TypeItem,
            TemplateId = req.ItemTpl,
            Count = req.ItemCount,
            IsFunctional = req.ItemFunctional,
            IsEncoded = false
        };
    }

    private StageRequirement? InjectAreaRequirement(AreaRequirement req, string areaName, string levelStr)
    {
        if (string.IsNullOrEmpty(req.AreaName)) {
            log.Warning(LogChannel.Hideout, $"Area '{areaName}' stage '{levelStr}': Area requirement missing AreaName : skipping.");
            return null;
        }

        if (!Enum.TryParse<HideoutAreas>(req.AreaName, ignoreCase: true, out var depAreaType)) {
            log.Warning(LogChannel.Hideout, $"Area '{areaName}' stage '{levelStr}': Unknown dependency area '{req.AreaName}' : skipping.");
            return null;
        }

        return new StageRequirement {
            Type = TypeArea,
            AreaType = (int)depAreaType,
            RequiredLevel = req.AreaLevel,
            IsEncoded = false
        };
    }

    private StageRequirement? InjectSkillRequirement(SkillRequirement req, string areaName, string levelStr)
    {
        if (string.IsNullOrEmpty(req.SkillName)) {
            log.Warning(LogChannel.Hideout, $"Area '{areaName}' stage '{levelStr}': Skill requirement missing SkillName : skipping.");
            return null;
        }

        return new StageRequirement {
            Type = TypeSkill,
            SkillName = req.SkillName,
            SkillLevel = req.SkillLevel,
            IsEncoded = false
        };
    }

    private StageRequirement? InjectTraderRequirement(TraderRequirement req, string areaName, string levelStr)
    {
        if (string.IsNullOrEmpty(req.TraderName)) {
            log.Warning(LogChannel.Hideout, $"Area '{areaName}' stage '{levelStr}': TraderLoyalty requirement missing TraderName : skipping.");
            return null;
        }

        var traders = databaseService.GetTraders();
        var traderEntry = traders.FirstOrDefault(t =>
            t.Value.Base?.Nickname != null &&
            t.Value.Base.Nickname.Equals(req.TraderName, StringComparison.OrdinalIgnoreCase));

        if (traderEntry.Value is null) {
            log.Warning(LogChannel.Hideout, $"Area '{areaName}' stage '{levelStr}': Trader '{req.TraderName}' not found : skipping.");
            return null;
        }

        return new StageRequirement {
            Type = TypeTraderLoyalty,
            TraderId = traderEntry.Key,
            LoyaltyLevel = req.TraderLoyalty,
            IsEncoded = false
        };
    }


    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // Bitcoin farm
    // ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void PatchBitcoinFarm(BitcoinFarmConfig? config)
    {
        if (config is null)
            return;

        var hideout = databaseService.GetHideout();
        var recipes = hideout.Production?.Recipes?.FindAll(r => r.EndProduct == ItemTpl.BARTER_PHYSICAL_BITCOIN);
        if (recipes is null || recipes.Count == 0) {
            log.Warning(LogChannel.Hideout, "Bitcoin farm : no recipes found, skipping.");
            return;
        }

        foreach (var recipe in recipes)
        {
            if (config.ProductionSpeedMultiplier.HasValue)
                recipe.ProductionTime = (int)Math.Round((double)recipe.ProductionTime! / config.ProductionSpeedMultiplier.Value);

            if (config.MaxCapacity.HasValue)
                recipe.ProductionLimitCount = config.MaxCapacity.Value;
        }

        if (config.GpuBoostRate.HasValue)
            hideout.Settings.GpuBoostRate = config.GpuBoostRate.Value;
    }
}
