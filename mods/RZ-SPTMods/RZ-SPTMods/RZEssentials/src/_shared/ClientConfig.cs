// RemzDNB - 2026

using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace RZEssentials._Shared;

[Injectable]
public class TraderGridSortingRouter(JsonUtil jsonUtil, TraderGridSortingCallbacks callbacks)
    : StaticRouter(jsonUtil, [
        new RouteAction("/rz/clientConfig",
            async (url, info, sessionId, output) => {
                var data = await callbacks.GetConfig();
                return jsonUtil.Serialize(data) ?? throw new InvalidOperationException();
            })
    ])
{ }

[Injectable]
public class TraderGridSortingCallbacks(ConfigLoader configLoader)
{
    public ValueTask<ClientConfig> GetConfig()
    {
        return new ValueTask<ClientConfig>(configLoader.Load<ClientConfig>());
    }
}
