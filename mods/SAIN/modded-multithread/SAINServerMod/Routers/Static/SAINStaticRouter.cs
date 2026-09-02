using SAINServerMod.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Routers.Static;

[Injectable]
public sealed class SAINStaticRouter(JsonUtil jsonUtil, ConfigService configService)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/sain/namepersonalities",
                async (url, info, sessionID, output) =>
                    jsonUtil.Serialize(configService.NicknamesModel)
                    ?? throw new InvalidOperationException("Could not serialize personalities!")
            ),
        ]
    ) { }
