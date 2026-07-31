using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Utils;
using TRLDynamicSpawnServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TRLDynamicSpawnServer.Routers;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader)]
public class TRLRouters : StaticRouter
{
    private static JsonUtil _jsonUtil;

    // This is a placeholder for the live config which would normally be managed by a ConfigService.
    public static TRLConfig CurrentConfig = new TRLConfig();

    public TRLRouters(JsonUtil jsonUtil) : base(jsonUtil, GetCustomRoutes())
    {
        _jsonUtil = jsonUtil;
        _ = TRLDynamicSpawnServer.Globals.TRLConfigManager.LoadConfig();
    }

    private static string GetStaticFile(string relativePath)
    {
        try
        {
            var asmPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var modDir = System.IO.Path.GetDirectoryName(asmPath) ?? "";
            
            string[] candidateDirs = new string[]
            {
                System.IO.Path.Combine(modDir, "wwwroot"),
                System.IO.Path.Combine(modDir, "..", "wwwroot"),
                System.IO.Path.Combine(modDir, "..", "..", "wwwroot"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "mods", "TRL-DynamicSpawn", "Server", "wwwroot"),
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "mods", "TRL-DynamicSpawn", "wwwroot")
            };

            foreach (var dir in candidateDirs)
            {
                string fullPath = System.IO.Path.Combine(dir, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    return System.IO.File.ReadAllText(fullPath);
                }
            }
        }
        catch { }
        return string.Empty;
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return new List<RouteAction>
        {
            new RouteAction("/trldynamicspawn",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("index.html"))
            ),
            new RouteAction("/trldynamicspawn/",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("index.html"))
            ),
            new RouteAction("/trldynamicspawn/index.html",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("index.html"))
            ),
            new RouteAction("/trldynamicspawn/css/trl-tokens.css",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("css/trl-tokens.css"))
            ),
            new RouteAction("/trldynamicspawn/css/trl-components.css",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("css/trl-components.css"))
            ),
            new RouteAction("/trldynamicspawn/css/trl-utilities.css",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("css/trl-utilities.css"))
            ),
            new RouteAction("/trldynamicspawn/fonts/bender.css",
                async (url, info, sessionID, output) => await new ValueTask<string>(GetStaticFile("fonts/bender.css"))
            ),
            new RouteAction("/trldynamicspawn/getConfig",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(CurrentConfig));
                }
            ),
            new RouteAction("/trldynamicspawn/getPmcSpawns",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.PmcSpawns));
                }
            ),
            new RouteAction("/trldynamicspawn/getScavSpawns",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.ScavSpawns));
                }
            ),
            new RouteAction("/trldynamicspawn/getSniperSpawns",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.SniperSpawns));
                }
            ),
            new RouteAction("/trldynamicspawn/getPlayerSpawns",
                async (url, info, sessionID, output) =>
                {
                    return await new ValueTask<string>(_jsonUtil.Serialize(TRLDynamicSpawnServer.Helpers.SpawnPointsManager.PlayerSpawns));
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
                            return await new ValueTask<string>(_jsonUtil.Serialize(new { status = "success", message = "Config saved successfully" }));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        return await new ValueTask<string>(_jsonUtil.Serialize(new { status = "error", message = ex.Message }));
                    }
                    return await new ValueTask<string>(_jsonUtil.Serialize(new { status = "error", message = "Invalid payload" }));
                }
            ),
            new RouteAction<SPTarkov.Server.Core.Models.Eft.Common.EmptyRequestData>("/trldynamicspawn/resetConfig",
                async (url, info, sessionID, output) =>
                {
                    try
                    {
                        CurrentConfig = new TRLConfig();
                        await TRLDynamicSpawnServer.Globals.TRLConfigManager.SaveConfig();
                        return await new ValueTask<string>(_jsonUtil.Serialize(new { status = "success", message = "Config reset to defaults" }));
                    }
                    catch (System.Exception ex)
                    {
                        return await new ValueTask<string>(_jsonUtil.Serialize(new { status = "error", message = ex.Message }));
                    }
                }
            )
        };
    }
}
