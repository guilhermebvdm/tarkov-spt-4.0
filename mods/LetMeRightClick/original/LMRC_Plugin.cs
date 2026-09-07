using BepInEx;
using BepInEx.Logging;
using LetMeRightClick.Patches;

namespace LetMeRightClick;

[BepInPlugin("com.lacyway.lmrc", "LetMeRightClick", PluginVersion)]
internal class LMRC_Plugin : BaseUnityPlugin
{
    public const string PluginVersion = "1.0.0";

    internal static ManualLogSource LMRC_Logger;

    protected void Awake()
    {
        LMRC_Logger = Logger;
        LMRC_Logger.LogInfo($"{nameof(LMRC_Plugin)} has been loaded.");

        new ItemUiContext_GetItemContextInteractions_Patch().Enable();
    }
}
