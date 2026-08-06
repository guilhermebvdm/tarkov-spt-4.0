using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using TRLDynamicSpawn.Helpers;
using TRLDynamicSpawn.Patches;

namespace TRLDynamicSpawn
{
    [
        BepInPlugin("TRLDynamicSpawn.settings", "TRLDynamicSpawn", "3.2.3"),
        BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)
    ]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        // private void Awake()
        // {
        //     var harmony = new Harmony("com.example.botzonepatch");
        //     harmony.PatchAll();
        // }

        private void Start()
        {
            LogSource = Logger;

            Settings.Init(Config);

            new NoTeleportPatch().Enable();
            new SniperPatch().Enable();
            new RefreshLocation().Enable();
            new SetMaxBotCountPatch().Enable();
            new BotSpawnLoggerPatch().Enable();
            new DisableVanillaWavesPatch().Enable();
            new DisableVanillaBossWavesPatch().Enable();
            new DynamicSpawnManagerPatch().Enable();
            new TryToSpawnInZoneAndDelayPatch().Enable();
            new ChooseProfilePatch().Enable();

            // Enable Despawn Manager & Map Overlay Components
            TRLDynamicSpawn.Components.BotDespawnManager.Enable();
            TRLDynamicSpawn.Components.TRLMapBubbleOverlay.Enable();
        }

        private void Update()
        {
        }
    };
}
