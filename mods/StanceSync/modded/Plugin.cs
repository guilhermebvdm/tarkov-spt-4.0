using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using hazelify.StanceSync.Patches;
using UnityEngine;

namespace hazelify.StanceSync
{
    [BepInPlugin( /* internal name: */"hazelify.StanceSync", /* F12 name: */ "hazelify.StanceSync", "1.0.1")]
    [BepInIncompatibility("com.janky.hollywoodcam")]
    // good to know, if you have StanceSync installed, HollywoodCam will not work

    public class Plugin : BaseUnityPlugin
    {
        // big ups to the legend Janky for this one, couldn't have done it without him <3
        public static ManualLogSource LogSource;
        public static ConfigEntry<bool> enableSync;
        public static ConfigEntry<bool> enableResetSync;

        public static ConfigEntry<bool> disableSyncWhileADS;
        public static ConfigEntry<bool> disableSyncWithOptic;

        private void Awake()
        {
            LogSource = Logger;
            initConfig();

            // enable patches, without whom this mod is useless
            new PlayerCameraControllerPatchPrefix().Enable();
            new OnLeanPatchPostfix().Enable();

            // log that the mod has loaded
            LogSource.LogInfo($"hazelify.StanceSync loaded!");
        }

        private void initConfig()
        {
            // bool config binds for the F12 BepInEx menu

            // core mod feature, enabling lean swap sync [toggle]
            enableSync = Config.Bind(
                "Core",
                "Sync leaning with shoulder swapping?",
                true,
                new ConfigDescription("If leaning left, should the character automatically shoulder swap?",
                null,
                new ConfigurationManagerAttributes { Order = 4 }));

            // reset lean sync for when you stop leaning [toggle]
            enableResetSync = Config.Bind(
                "Core",
                "Sync leaning reset?",
                true,
                new ConfigDescription("If no longer leaning left, should the character automatically reset the shoulder swap?",
                null,
                new ConfigurationManagerAttributes { Order = 3 }));

            // disable synced lean while aiming down sights (ADS) [toggle]
            disableSyncWhileADS = Config.Bind(
                "Core",
                "Disable synced lean while aiming?",
                false,
                new ConfigDescription("If aiming down sights, disable synchronized leaning completely.",
                null,
                new ConfigurationManagerAttributes { Order = 2 }));

            // disable synced lean only while aiming down sights (ADS) with a magnified optic [toggle]
            disableSyncWithOptic = Config.Bind(
                "Core",
                "Disable synced lean during optic ADS?",
                false,
                new ConfigDescription("If aiming down sights, only disable synchronized leaning while aiming with a magnified optic." +
                                      "All other optics, such as red dots, collimators, or iron sights will allow synchronized leaning." +
                                      "\n" +
                                      "\n" +
                                      "This will override `Disable synced lean while aiming?`. If you want to disable synchronized leaning while aiming with ANY optic, enable the other option and disable this one." +
                                      "\nIf this option is enabled, but `Disable synced lean while aiming?` is not enabled, sync behavior will default to always working.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }));
        }
    }
}
