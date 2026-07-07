using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Utils;               // JsonUtil
using TRLItemsManagement.Pricing;

namespace TRLItemsManagement.Web;

/// <summary>
///     Serves the buy-price overrides to the CLIENT mod (<c>modded/Client/BuyPriceOverrides.cs</c>, via
///     <c>SPT.Common.Http.RequestHandler.GetJson</c>) so the client-side display patch
///     (<c>TraderClass.GetUserItemPrice</c>) reads the SAME data the server backstop
///     (<see cref="Pricing.SellItemPatch"/>) applies — exhibited price and credited money always agree.
///     No sessionId needed: the config is global, not per-profile.
///     <para>
///     Serves <see cref="TraderPriceOnLoad.BuyOverrides"/> — the parsed, VALIDATED, Fence-excluded
///     in-memory dict <see cref="Pricing.SellItemPatch"/> itself reads — instead of the raw config file.
///     Reading the raw file would let a hand-edited Fence entry (or any malformed entry the loader would
///     have dropped) reach the client display while the server backstop keeps ignoring it, reintroducing
///     the exact display/credited desync this router exists to prevent. Single in-memory source of truth
///     for both mod halves; a config edit only takes effect after a restart either way (same as the sell
///     overrides), so there is no live-reload requirement lost by not re-reading the file per request.
///     </para>
///     This is a DIFFERENT route/source than the browser-facing <c>GET /api/trader-buy-overrides</c>
///     (<c>Api/TraderPriceController.cs</c>), which reads the raw file from disk on purpose — the viewer
///     needs to show exactly what is saved, including an entry the boot would still reject. See the
///     plan's "A UI compartilhada" section for the full rationale of the two-routes-two-sources split.
/// </summary>
[Injectable]
public class TraderBuyOverridesRouter : StaticRouter
{
    public TraderBuyOverridesRouter(JsonUtil jsonUtil)
        : base(jsonUtil, GetRoutes(jsonUtil))
    {
    }

    private static List<RouteAction> GetRoutes(JsonUtil jsonUtil)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/trlitemsmanagement/buy-overrides",
                (url, info, sessionId, output) =>
                {
                    // String-keyed rebuild (not a direct Dictionary<MongoId, ...> serialize) — MongoId as
                    // a JSON dictionary KEY needs a converter capable of WriteAsPropertyName, which is not
                    // guaranteed here; converting keys to string ourselves sidesteps that risk entirely.
                    var parsed = TraderPriceOnLoad.BuyOverrides;
                    var outMap = new Dictionary<string, Dictionary<string, TraderOverride>>();
                    if (parsed is not null)
                    {
                        foreach (var (traderId, tplMap) in parsed)
                        {
                            var inner = new Dictionary<string, TraderOverride>();
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
