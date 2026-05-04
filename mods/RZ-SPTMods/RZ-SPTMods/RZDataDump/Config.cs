// RemzDNB - 2026

namespace RZDataDump;

public class MasterConfig
{
    public const string FileName = "masterConfig.json";

    public bool DumpItemsEnabled { get; set; } = false;
    public int DumpItemsMode { get; set; } = 0;

    public bool DumpCategoriesEnabled { get; set; } = false;

    public bool DumpHideoutEnabled { get; set; } = false;
}
