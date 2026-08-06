using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Utils;
using TRLDynamicSpawnServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace TRLDynamicSpawnServer.Routers;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader)]
public class TRLRouters : StaticRouter
{
    private static JsonUtil _jsonUtil;
    public static TRLConfig CurrentConfig = new TRLConfig();

    private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public TRLRouters(JsonUtil jsonUtil) : base(jsonUtil, GetCustomRoutes())
    {
        _jsonUtil = jsonUtil;
        _ = TRLDynamicSpawnServer.Globals.TRLConfigManager.LoadConfig();
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return new List<RouteAction>
        {
            new RouteAction("/trldynamicspawn/getConfig",
                async (url, info, sessionID, output) =>
                {
                    var json = JsonSerializer.Serialize(CurrentConfig, JsonOpts);
                    return await new ValueTask<string>(json);
                }
            ),
            new RouteAction("/trldynamicspawn/getPmcSpawns",
                async (url, info, sessionID, output) =>
                {
                    var json = JsonSerializer.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.PmcSpawns, JsonOpts);
                    return await new ValueTask<string>(json);
                }
            ),
            new RouteAction("/trldynamicspawn/getScavSpawns",
                async (url, info, sessionID, output) =>
                {
                    var json = JsonSerializer.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.ScavSpawns, JsonOpts);
                    return await new ValueTask<string>(json);
                }
            ),
            new RouteAction("/trldynamicspawn/getSniperSpawns",
                async (url, info, sessionID, output) =>
                {
                    var json = JsonSerializer.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.SniperSpawns, JsonOpts);
                    return await new ValueTask<string>(json);
                }
            ),
            new RouteAction("/trldynamicspawn/getPlayerSpawns",
                async (url, info, sessionID, output) =>
                {
                    var json = JsonSerializer.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.PlayerSpawns, JsonOpts);
                    return await new ValueTask<string>(json);
                }
            ),
            new RouteAction<TRLConfig>("/trldynamicspawn/saveConfig",
                async (url, info, sessionID, output) =>
                {
                    try
                    {
                        if (info != null)
                        {
                            CurrentConfig = info;
                            await TRLDynamicSpawnServer.Globals.TRLConfigManager.SaveConfig();
                            return await new ValueTask<string>("{\"status\":\"success\",\"message\":\"Config saved successfully\"}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        return await new ValueTask<string>($"{{\"status\":\"error\",\"message\":\"{ex.Message}\"}}");
                    }
                    return await new ValueTask<string>("{\"status\":\"error\",\"message\":\"Invalid payload\"}");
                }
            ),
            new RouteAction<SPTarkov.Server.Core.Models.Eft.Common.EmptyRequestData>("/trldynamicspawn/resetConfig",
                async (url, info, sessionID, output) =>
                {
                    try
                    {
                        var result = await TRLDynamicSpawnServer.Globals.TRLConfigManager.ResetConfig();
                        if (result == TRLDynamicSpawnServer.Globals.ConfigOperationResult.Success)
                        {
                            return await new ValueTask<string>("{\"status\":\"success\",\"message\":\"Config reset to defaults\"}");
                        }
                        return await new ValueTask<string>("{\"status\":\"error\",\"message\":\"Failed to reset config\"}");
                    }
                    catch (System.Exception ex)
                    {
                        return await new ValueTask<string>($"{{\"status\":\"error\",\"message\":\"{ex.Message}\"}}");
                    }
                }
            )
        };
    }
}
