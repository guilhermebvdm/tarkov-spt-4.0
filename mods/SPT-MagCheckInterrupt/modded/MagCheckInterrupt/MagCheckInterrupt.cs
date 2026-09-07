using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using MagCheckInterrupt.Utils;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt;

[BepInPlugin("com.ozen.magcheckinterrupt", "MagCheckInterrupt", "1.0.4")]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.tyfon.uifixes", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Tyfon.UIFixes", BepInDependency.DependencyFlags.SoftDependency)]
public class MagCheckInterrupt : BaseUnityPlugin
{
    public static ManualLogSource LogSource { get; private set; }

    protected void Awake()
    {
        LogSource = Logger;

        ConfigUtil.Init(Config);

        var patchManager = new PatchManager(this, true);
        patchManager.EnablePatches();

        if (Chainloader.PluginInfos.ContainsKey("com.fika.core"))
        {
            External.Fika.Init();
        }
        if (Chainloader.PluginInfos.ContainsKey("com.tyfon.uifixes") || Chainloader.PluginInfos.ContainsKey("Tyfon.UIFixes"))
        {
            External.UIFixes.Init();
        }
    }
}
