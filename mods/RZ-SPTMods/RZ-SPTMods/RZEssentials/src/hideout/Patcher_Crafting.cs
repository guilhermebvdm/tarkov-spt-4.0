// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Hideout;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class CraftingPatcher(DatabaseService databaseService, ConfigLoader configLoader, RzeLogger log) : IOnLoad
{
    private readonly CraftingConfig _craftingConfig = configLoader.Load<CraftingConfig>();

    public Task OnLoad()
    {
        if (!_craftingConfig.EnableCraftingConfig)
            return Task.CompletedTask;

        var config = configLoader.Load<CraftingConfig>(CraftingConfig.FileName);
        var recipes = databaseService.GetTables().Hideout?.Production?.Recipes;

        if (recipes is null)
        {
            log.Warning(LogChannel.Hideout, "Hideout recipes is null -- skipping.");
            return Task.CompletedTask;
        }

        // Clear specified areas.

        var toRemove = config.ClearAreas.ToHashSet();
        recipes.RemoveAll(r => r.AreaType.HasValue && toRemove.Contains(r.AreaType.Value));

        // Inject custom recipes.

        if (config.Recipes.Count == 0)
            return Task.CompletedTask;

        var injected = 0;
        foreach (var (areaType, areaRecipes) in config.Recipes)
        {
            foreach (var recipe in areaRecipes)
            {
                recipes.Add(BuildProduction(recipe, areaType));
                injected++;
            }
        }

        log.Info(LogChannel.Hideout, $"{injected} custom recipe(s) injected.");
        return Task.CompletedTask;
    }

    private static HideoutProduction BuildProduction(CraftRecipe r, HideoutAreas areaType)
    {
        return new HideoutProduction
        {
            Id = new MongoId(),
            AreaType = areaType,
            ProductionTime = r.ProductionTime,
            NeedFuelForAllProductionTime = r.NeedFuelForAllProductionTime,
            Locked = r.Locked,
            EndProduct = r.EndProduct,
            Continuous = r.Continuous,
            Count = r.Count,
            ProductionLimitCount = 0,
            IsEncoded = false,
            Requirements = r
                .Requirements.Select(req => new Requirement
                {
                    Type = req.Type,
                    TemplateId = req.TemplateId!,
                    AreaType = (int?)req.AreaType,
                    RequiredLevel = req.RequiredLevel,
                    Count = req.Count,
                    IsFunctional = req.IsFunctional,
                    IsEncoded = req.IsEncoded,
                })
                .ToList(),
        };
    }
}
