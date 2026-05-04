// RemzDNB - 2026

using RZEssentials._Shared;

namespace RZEssentials.Quests;

public record QuestsMainConfig : IConfig
{
    public static string ConfigFolderName => "quests/";
    public static string FileName => ConfigFolderName + "questsConfig.json";

    public bool DisableAllQuests { get; set; } = false;
}
