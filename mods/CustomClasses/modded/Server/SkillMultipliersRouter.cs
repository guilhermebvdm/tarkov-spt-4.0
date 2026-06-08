using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Servers;             // SaveServer
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace CustomClasses;

/// <summary>
///     Item 005/010: serve ao client os multiplicadores de XP de skill da CLASSE do session atual.
///     Resolve sessionId → profile.ProfileInfo.Edition → registry. Edition vanilla → multipliers vazio.
///     Item 010: o payload agora é { className, multipliers } (className = edition = nome da classe).
///     Padrão StaticRouter/RouteAction confirmado em mods/SkillDistribution + spt-source DI/Router.cs.
/// </summary>
[Injectable]
public class SkillMultipliersRouter : StaticRouter
{
    public SkillMultipliersRouter(JsonUtil jsonUtil, SkillMultiplierRegistry registry, SaveServer saveServer)
        : base(jsonUtil, GetRoutes(jsonUtil, registry, saveServer))
    {
    }

    private static List<RouteAction> GetRoutes(JsonUtil jsonUtil, SkillMultiplierRegistry registry, SaveServer saveServer)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/customclasses/skill-multipliers",
                (url, info, sessionId, output) =>
                {
                    // ref: SaveServer.cs:118 (GetProfile(MongoId)); Edition usada em CreateProfileService.cs:44
                    var edition = saveServer.GetProfile(sessionId)?.ProfileInfo?.Edition ?? string.Empty;
                    var mults = registry.Get(edition);
                    var dto = new SkillMultipliersResponse
                    {
                        // className só faz sentido p/ classe do mod (que tem multiplicadores). edition == nome da classe.
                        ClassName = mults.Count > 0 ? edition : null,
                        Multipliers = mults,
                    };
                    var json = jsonUtil.Serialize(dto) ?? "{}";
                    return new ValueTask<string>(json);
                }),
        ];
    }
}
