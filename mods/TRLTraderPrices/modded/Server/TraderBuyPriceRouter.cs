using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace TRLTraderPrices;

/// <summary>
///     B-3 Rota B: serves the buy-price overrides to the CLIENT mod
///     (<c>modded/Client/BuyPriceOverrides.cs</c>, via <c>SPT.Common.Http.RequestHandler.GetJson</c>) so
///     the client-side display patch (<c>TraderClass.GetUserItemPrice</c>) reads the SAME data the
///     server backstop (<see cref="TraderBuyPricePatch"/>) applies — exhibited price and credited money
///     always agree. No sessionId needed: the config is global, not per-profile.
///     <para>
///     CR-01: serves <see cref="TRLTraderPricesMod.BuyOverrides"/> — the parsed, VALIDATED, Fence-excluded
///     in-memory dict <see cref="TraderBuyPricePatch"/> itself reads — instead of the raw config file.
///     Reading the raw file would let a hand-edited Fence entry (or any malformed entry the loader would
///     have dropped) reach the client display while the server backstop keeps ignoring it, reintroducing
///     the exact display/credited desync Rota B exists to prevent. Single in-memory source of truth for
///     both mod halves; a config edit only takes effect after a restart either way (same as the sell
///     overrides), so there is no live-reload requirement lost by not re-reading the file per request.
///     </para>
///     Pattern: <see cref="StaticRouter"/>/<see cref="RouteAction{T}"/>, confirmed in
///     <c>mods/CustomClasses/modded/Server/SkillMultipliersRouter.cs</c>.
/// </summary>
[Injectable]
public class TraderBuyPriceRouter : StaticRouter
{
    public TraderBuyPriceRouter(JsonUtil jsonUtil)
        : base(jsonUtil, GetRoutes(jsonUtil))
    {
    }

    private static List<RouteAction> GetRoutes(JsonUtil jsonUtil)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/trltraderprices/buy-overrides",
                (url, info, sessionId, output) =>
                {
                    // String-keyed rebuild (not a direct Dictionary<MongoId, ...> serialize) — MongoId as
                    // a JSON dictionary KEY needs a converter capable of WriteAsPropertyName, which is not
                    // guaranteed here; converting keys to string ourselves sidesteps that risk entirely.
                    var parsed = TRLTraderPricesMod.BuyOverrides;
                    var outMap = new Dictionary<string, Dictionary<string, TRLTraderPricesMod.TraderOverride>>();
                    if (parsed is not null)
                    {
                        foreach (var (traderId, tplMap) in parsed)
                        {
                            var inner = new Dictionary<string, TRLTraderPricesMod.TraderOverride>();
                            foreach (var (tpl, ovr) in tplMap)
                            {
                                inner[tpl.ToString()] = ovr;
                            }

                            outMap[traderId.ToString()] = inner;
                        }
                    }

                    var json = jsonUtil.Serialize(outMap) ?? "{}";
                    return new ValueTask<string>(json);
                }),
        ];
    }
}
