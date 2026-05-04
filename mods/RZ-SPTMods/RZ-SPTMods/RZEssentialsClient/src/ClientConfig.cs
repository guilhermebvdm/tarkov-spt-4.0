// RemzDNB - 2026

using Newtonsoft.Json;
using SPT.Common.Http;
using System.Collections.Generic;

namespace RZEssentialsClient;

public class ClientConfig
{
    // Skip screens
    public bool SkipSideSelectionScreen { get; set; } = true;
    public bool SkipInsuranceScreen { get; set; } = true;
    public bool SkipRaidSettingsScreen { get; set; } = true;
    public bool SkipExperienceScreen { get; set; } = true;

    // Trader grid sorting
    public bool EnableCategorySort { get; set; } = true;
    public bool EnableItemSort { get; set; } = true;
    public bool EnableAlphabeticalSort { get; set; } = true;
    public List<string> CategoryOrder { get; set; } = [];
    public List<string> ItemOrder { get; set; } = [];
    
    // Version label
    public bool EnableVersionLabelOverride { get; set; }
    public string VersionLabelText { get; set; } = "";

    // Singleton
    public static ClientConfig Instance { get; private set; } = new();

    public static void Fetch()
    {
        try
        {
            var json = RequestHandler.GetJson("/rz/clientConfig");
            Instance = JsonConvert.DeserializeObject<ClientConfig>(json) ?? new ClientConfig();
        }
        catch
        {
            Instance = new ClientConfig();
        }
    }
}