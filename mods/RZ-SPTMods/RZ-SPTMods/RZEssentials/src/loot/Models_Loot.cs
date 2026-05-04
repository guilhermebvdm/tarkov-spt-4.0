using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace RZEssentials.Loot;

public record LootConfig
{
    public const string FileName = "loot/lootConfig.json";

    public bool Enabled { get; set; } = true;
    public List<string> BossRoles { get; set; } = [];
    public List<string> FollowerRoles { get; set; } = [];

    public BossLootSection? Bosses { get; set; }
    public BotLootSection? Followers { get; set; }
    public BotLootSection? Pmc { get; set; }
    public BotLootSection? Scav { get; set; }

    public OverwriteBlacklist? OverwriteBlacklist { get; set; }

    public bool EnableVerboseLogging { get; set; } = false;
}

public record BossLootSection
{
    public bool Enabled { get; set; } = true;
    public bool UseGlobalConfig { get; set; } = true;
    public BotLootSection? Global { get; set; }
    public Dictionary<string, BotLootSection>? PerBoss { get; set; }
}

public record BotLootSection
{
    public bool Enabled { get; set; } = true;
    public List<LootEntry> Pockets { get; set; } = [];
}

public record LootEntry
{
    public string Tpl { get; set; } = "";
    public double Chance { get; set; } = 100;
    public int MinStack { get; set; } = 1;
    public int MaxStack { get; set; } = 1;
}

public class OverwriteBlacklist
{
    public HashSet<string> Tpls { get; set; } = new();
    public HashSet<string> Categories { get; set; } = new();
}
