using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Utils;               // JsonUtil, FileUtil

namespace TRLTraderPrices;

/// <summary>
///     B-3 Rota B: serves the raw <c>config/buy-overrides.json</c> to the CLIENT mod
///     (<c>modded/Client/BuyPriceOverrides.cs</c>, via <c>SPT.Common.Http.RequestHandler.GetJson</c>) so
///     the client-side display patch (<c>TraderClass.GetUserItemPrice</c>) reads the SAME config the
///     server backstop (<see cref="TraderBuyPricePatch"/>) applies — exhibited price and credited money
///     always agree. No sessionId needed: the config is global, not per-profile.
///     Pattern: <see cref="StaticRouter"/>/<see cref="RouteAction{T}"/>, confirmed in
///     <c>mods/CustomClasses/modded/Server/SkillMultipliersRouter.cs</c>. Missing file → served as
///     <c>"{}"</c> (the viewer creates the real file on the first edit; the client tolerates an empty map).
/// </summary>
[Injectable]
public class TraderBuyPriceRouter : StaticRouter
{
    public TraderBuyPriceRouter(JsonUtil jsonUtil, FileUtil fileUtil)
        : base(jsonUtil, GetRoutes(fileUtil))
    {
    }

    private static List<RouteAction> GetRoutes(FileUtil fileUtil)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/trltraderprices/buy-overrides",
                (url, info, sessionId, output) =>
                {
                    var configPath = Path.Combine(fileUtil.GetModPath("TRLTraderPrices"), "config", "buy-overrides.json");
                    var json = fileUtil.FileExists(configPath) ? fileUtil.ReadFile(configPath) : "{}";
                    return new ValueTask<string>(json);
                }),
        ];
    }
}
