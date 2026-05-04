// RemzDNB - 2026

using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;

namespace RZEssentials._Shared;

public enum LogChannel
{
    Character,
    Economy,
    Hideout,
    ItemContext,
    ItemStats,
    ItemTiers,
    Loot,
    MiscSettings,
    Profiles,
    Raids,
    Traders,
    UI,
}

[Injectable(InjectionType.Singleton)]
public class RzeLogger(ILogger<RzeLogger> logger, ConfigLoader configLoader)
{
    private readonly LogConfig _logConfig = configLoader.Load<LogConfig>();

    private static readonly Dictionary<LogChannel, string> Tags = new()
    {
        [LogChannel.Character] = "[\e[1;94mRZE_Character\e[0m]",       // Bold Light Blue
        [LogChannel.Economy]      = "[\e[0;33mRZE_Economy\e[0m]",      // Dark Yellow
        [LogChannel.Hideout]      = "[\e[1;33mRZE_Hideout\e[0m]",      // Bold Yellow
        [LogChannel.ItemContext]  = "[\e[1;36mRZE_ItemContext\e[0m]",  // Bold Cyan
        [LogChannel.ItemStats]    = "[\e[0;32mRZE_ItemStats\e[0m]",    // Green
        [LogChannel.ItemTiers]    = "[\e[1;36mRZE_ItemTiers\e[0m]",    // Bold Cyan
        [LogChannel.Loot]         = "[\e[1;31mRZE_Loot\e[0m]",         // Bold Red
        [LogChannel.MiscSettings] = "[\e[1;37mRZE_MiscSettings\e[0m]", // Bold White
        [LogChannel.Profiles]     = "[\e[1;34mRZE_Profiles\e[0m]",     // Bold Blue
        [LogChannel.Raids]        = "[\e[1;32mRZE_Raids\e[0m]",        // Bold Green
        [LogChannel.Traders]      = "[\e[1;35mRZE_Traders\e[0m]",      // Bold Magenta
        [LogChannel.UI]           = "[\e[1;95mRZE_UI\e[0m]",           // Bold Pink
    };

    public void Info(LogChannel channel, string message)
    {
        if (!IsEnabled(channel)) return;
        logger.LogInformation($"{Tags[channel]} {message}");
    }

    public void Warning(LogChannel channel, string message)
    {
       // if (!IsEnabled(channel)) return;
        logger.LogWarning($"{Tags[channel]} {message}");
    }

    public void Error(LogChannel channel, string message)
    {
        logger.LogError($"{Tags[channel]} {message}");
    }

    private bool IsEnabled(LogChannel channel) => _logConfig.Channels.TryGetValue(channel.ToString(), out var enabled) && enabled;
}
