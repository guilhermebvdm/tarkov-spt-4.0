// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using RZEssentials._Shared;

namespace RZEssentials.Quests;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 1)]
public class Patcher_Quests(
    DatabaseService databaseService,
    ConfigLoader configLoader,
    ConfigServer configServer
) : IOnLoad
{
    private readonly QuestsMainConfig _questsMainConfig = configLoader.Load<QuestsMainConfig>();

    public Task OnLoad()
    {
        if (_questsMainConfig.DisableAllQuests)
        {
            databaseService.GetQuests().Clear();
            configServer.GetConfig<QuestConfig>().RepeatableQuests.Clear();
        }

        return Task.CompletedTask;
    }
}
