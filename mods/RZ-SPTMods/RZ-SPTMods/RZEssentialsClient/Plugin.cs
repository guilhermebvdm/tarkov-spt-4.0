// RemzDNB - 2026

using BepInEx;
using BepInEx.Logging;

namespace RZEssentialsClient;

[BepInPlugin("com.rz.essentials.client", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;

    private void Awake()
    {
        Settings.Init(Config);
        Log = Logger;
        
        ClientConfig.Fetch();
        SkipScreensPatches.Enable();

        new TraderGridSortingPatch().Enable();
        new VersionLabelPatch().Enable();
    }
}