// RemzDNB - 2026

using RZEssentials._Shared;

namespace RZEssentials.UI;

public record LocalesConfig : IConfig
{
    public static string FileName => "ui/localesConfig.json";

    public bool ForceEnglishItems { get; set; } = false;
    public bool ForceEnglishMaps { get; set; } = false;
    public bool ForceEnglishHideout { get; set; } = false;
    public bool ForceEnglishTraders { get; set; } = false;

    public bool EnableTraderLocaleOverrides { get; set; } = false;
    public Dictionary<string, TraderLocaleEntry> TraderLocaleOverrides { get; set; } = new();

    public bool EnableLocaleOverrides { get; set; } = false;
    public Dictionary<string, string> LocaleOverrides { get; set; } = new();
}

public record TraderLocaleEntry
{
    public string? FullName    { get; set; }
    public string? FirstName   { get; set; }
    public string? Nickname    { get; set; }
    public string? Location    { get; set; }
    public string? Description { get; set; }
}

public record AvatarsConfig : IConfig
{
    public static string FileName => "ui/avatarsConfig.json";

    public bool EnableAvatarOverrides { get; set; } = false;
    public Dictionary<string, string> AvatarOverrides { get; set; } = new();
}
