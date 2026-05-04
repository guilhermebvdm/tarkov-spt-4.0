// RemzDNB - 2026

using System.Reflection;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Routers;
using RZEssentials._Shared;

namespace RZEssentials.UI;

[Injectable(TypePriority = OnLoadOrder.PostSptModLoader + 2)]
public class Patcher_Images(
    ILogger<Patcher_Images> logger,
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigLoader configLoader
) : IOnLoad
{

    private readonly AvatarsConfig _UIConfig = configLoader.Load<AvatarsConfig>();

    public Task OnLoad()
    {
        PatchTraderAvatars();

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    // TRADER AVATARS
    // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

    private void PatchTraderAvatars()
    {
        var modRoot = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        if (!_UIConfig.EnableAvatarOverrides)
            return;

        foreach (var (traderId, fileName) in _UIConfig.AvatarOverrides)
        {
            var filePath = Path.Combine(modRoot, "db", fileName);
            if (!File.Exists(filePath))
            {
                logger.LogWarning("[RZCustomTraders] TraderOverrides/Avatars: file not found, skipping: {File}", fileName);
                continue;
            }

            imageRouter.AddRoute($"/files/trader/avatar/{traderId}", filePath);
        }
    }
}
