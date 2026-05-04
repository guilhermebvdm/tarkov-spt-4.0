// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;
using RZEssentials._Shared;
using SPTarkov.Server.Core.Helpers;

namespace RZEssentials.Traders;

// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
// CUSTOM TRADER PATCHER
//
// Registers Sable as a fully autonomous trader entry in the database.
// ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Patcher_CustomTrader(
    DatabaseService databaseService,
    ConfigLoader    configLoader,
    ModHelper       modHelper,
    ImageRouter     imageRouter,
    ConfigServer    configServer,
    ICloner         cloner,
    RzeLogger       log
) : IOnLoad
{
    public Task OnLoad()
    {
        var config = configLoader.Load<CustomTraderConfig>();
        var modRoot = modHelper.GetAbsolutePathToModFolder(System.Reflection.Assembly.GetExecutingAssembly());

        if (!config.EnableCustomTrader)
            return Task.CompletedTask;

        // Image

        var imagePath = System.IO.Path.Combine(modRoot, "db", config.AvatarFile);
        if (!File.Exists(imagePath))
        {
            log.Error(LogChannel.Traders, $"CustomTrader: avatar file not found at '{imagePath}', aborting.");
            return Task.CompletedTask;
        }
        imageRouter.AddRoute($"/files/trader/avatar/{CustomTraderConfig.TraderId}", imagePath);

        // TraderConfig (UpdateTime)

        var traderConfig = configServer.GetConfig<TraderConfig>();
        if (traderConfig.UpdateTime.All(x => x.TraderId != CustomTraderConfig.TraderId))
        {
            traderConfig.UpdateTime.Add(new UpdateTime
            {
                TraderId = CustomTraderConfig.TraderId,
                Seconds = new MinMax<int>(3600, 3600)
            });
        }

        var ragfairConfig = configServer.GetConfig<RagfairConfig>();
        ragfairConfig.Traders.TryAdd(new MongoId(CustomTraderConfig.TraderId), true);

        // Build TraderBase from Prapor

        var traderBase = cloner.Clone(databaseService.GetTables().Traders[new MongoId("54cb50c76803fa8b248b4571")].Base);
        if (traderBase is null)
        {
            log.Error(LogChannel.Traders, "CustomTrader: couldn't clone Prapor base, aborting.");
            return Task.CompletedTask;
        }
        traderBase.Id = new MongoId(CustomTraderConfig.TraderId);
        traderBase.Avatar = $"/files/trader/avatar/{CustomTraderConfig.TraderId}.png";
        traderBase.Nickname = config.Nickname;

        // Register in DB

        var traderEntry = new Trader
        {
            Base   = traderBase,
            Assort = new TraderAssort
            {
                Items = [],
                BarterScheme = new Dictionary<MongoId, List<List<BarterScheme>>>(),
                LoyalLevelItems = new Dictionary<MongoId, int>()
            },
            QuestAssort = new Dictionary<string, Dictionary<MongoId, MongoId>>
            {
                { "Started", new Dictionary<MongoId, MongoId>() },
                { "Success", new Dictionary<MongoId, MongoId>() },
                { "Fail",    new Dictionary<MongoId, MongoId>() },
            },
            Dialogue = new Dictionary<string, List<string>?>
            {
                { "start", [] },
                { "end", [] },
                { "goodbuy", [] },
                { "badass", [] },
                { "cantafford", [] },
                { "loyalty_gain", [] },
                { "barterReady", [] },
            }
        };

        if (!databaseService.GetTables().Traders.TryAdd(traderBase.Id, traderEntry))
        {
            log.Warning(LogChannel.Traders, $"CustomTrader: trader '{CustomTraderConfig.TraderId}' already exists in DB, skipping.");
            return Task.CompletedTask;
        }

        // Seed locale keys for all languages as fallback

        foreach (var (_, locale) in databaseService.GetTables().Locales.Global)
        {
            locale.AddTransformer(data =>
            {
                data.TryAdd($"{CustomTraderConfig.TraderId} FullName", config.FullName);
                data.TryAdd($"{CustomTraderConfig.TraderId} FirstName", config.FirstName);
                data.TryAdd($"{CustomTraderConfig.TraderId} Nickname", config.Nickname);
                data.TryAdd($"{CustomTraderConfig.TraderId} Location", config.Location);
                data.TryAdd($"{CustomTraderConfig.TraderId} Description", config.Description);
                return data;
            });
        }

        log.Info(LogChannel.Traders, "CustomTrader: Sable registered.");
        return Task.CompletedTask;
    }
}
