using MOARServer.Controllers;
using MOARServer.Globals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Utils;

namespace MOARServer.Routers;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader)]
public class StaticRouters : StaticRouter
{
    private static JsonUtil _jsonUtil;
    private static MapSpawns _mapSpawns;

    public StaticRouters(
        JsonUtil jsonUtil,
        MapSpawns mapSpawns
    ) : base(
        jsonUtil,
        GetCustomRoutes()
    )
    {
        _jsonUtil = jsonUtil;
        _mapSpawns = mapSpawns;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction("/client/match/local/end",
                async (url, info, sessionID, output) =>
                {
                    return output;
                },
                typeof(EndLocalRaidRequestData)
            ),
            new RouteAction("/moar/getDefaultConfig",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(ModConfig.Config));
                }
            ),
            new RouteAction("/moar/getServerConfigWithOverrides",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(ModConfig.Config));
                }
            ),
            new RouteAction("/moar/getPresets",
                async (url, info, sessionID, output) =>
                {
                    var presets = new List<object>();
                    foreach (var key in ModConfig.PresetWeights.Keys)
                    {
                        var nameParts = key.Split('-');
                        for (int i = 0; i < nameParts.Length; i++)
                        {
                            if (nameParts[i].Length > 0)
                                nameParts[i] = char.ToUpper(nameParts[i][0]) + nameParts[i].Substring(1);
                        }
                        presets.Add(new { Name = string.Join(" ", nameParts), Label = key });
                    }
                    presets.Add(new { Name = "Random", Label = "Random" });
                    presets.Add(new { Name = "Custom", Label = "Custom" });

                    var result = new { data = presets };
                    return await new ValueTask<string>(_jsonUtil.Serialize(result));
                }
            ),
            new RouteAction("/moar/announcePreset",
                async (url, info, sessionID, output) =>
                {
                    string p = ModConfig.Config.CurrentPreset;
                    if (string.IsNullOrEmpty(p) || p.Equals("Random", StringComparison.OrdinalIgnoreCase))
                    {
                        return await new ValueTask<string>(_jsonUtil.Serialize(p));
                    }
                    return await new ValueTask<string>(_jsonUtil.Serialize(p));
                }
            ),
            new RouteAction("/moar/currentPreset",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(ModConfig.Config.CurrentPreset ?? "Random"));
                }
            ),
            new RouteAction("/moar/addBotSpawn",
                async (url, info, sessionID, output) =>
                {
                    try
                    {
                        var req = info as MOARServer.Models.AddBotSpawnRequest;
                        if (req != null)
                        {
                            MOARServer.Helpers.SpawnSaver.SaveSpawn(req.Type, req.X, req.Y, req.Z, req.Map);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MOAR] Error saving spawn: {ex.Message}");
                    }
                    return await new ValueTask<string>("OK");
                },
                typeof(MOARServer.Models.AddBotSpawnRequest)
            ),
            new RouteAction("/moar/deleteBotSpawn",
                async (url, info, sessionID, output) =>
                {
                    try
                    {
                        var req = info as MOARServer.Models.AddBotSpawnRequest;
                        if (req != null)
                        {
                            MOARServer.Helpers.SpawnSaver.DeleteSpawn(req.Type, req.X, req.Y, req.Z, req.Map);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MOAR] Error deleting spawn: {ex.Message}");
                    }
                    return await new ValueTask<string>("OK");
                },
                typeof(MOARServer.Models.AddBotSpawnRequest)
            )
        ];
    }
}
