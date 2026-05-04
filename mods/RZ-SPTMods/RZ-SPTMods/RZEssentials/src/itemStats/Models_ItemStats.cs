using RZEssentials._Shared;

namespace RZEssentials.ItemStats;

public record ItemStatsConfig
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, ItemOverride> CategoryOverrides { get; set; } = new();
    public Dictionary<string, ItemOverride> Overrides { get; set; } = new();
}

public class ItemOverride
{
    public Dictionary<string, StatOperation>? Ops { get; set; }
    public Dictionary<string, GridOverride>? Grids { get; set; }
}

public class GridOverride
{
    public int? CellsH { get; set; }
    public int? CellsV { get; set; }
    public int? MaxWeight { get; set; }
}
