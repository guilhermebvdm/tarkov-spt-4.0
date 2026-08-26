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
        BepInPlugin("TRLDynamicSpawn.settings", "TRLDynamicSpawn", "3.5.1"),
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
            new ZryachiyAggressivenessPatch().Enable();
            new ZryachiyActivatePatch().Enable();

            // Raid lifecycle hooks (backlog 009/010 — ref: AUD-01-01, AUD-01-02, AUD-01-06)
            new RaidStartPatch().Enable();
            new GameWorldOnDestroyPatch().Enable();
            new LocalGameStopPatch().Enable();
            if (CoopGameStopPatch.TargetType != null)   // Fika soft dependency — null target would throw PatchException (PA-02-03)
            {
                new CoopGameStopPatch().Enable();
            }

            // Vanilla continuous scav spawner refused before profile creation (backlog 010 — ref: AUD-01-08)
            new ActivateBotsWithoutWavePatch().Enable();

            // Enable Despawn Manager & Map Overlay Components
            TRLDynamicSpawn.Components.BotDespawnManager.Enable();
            TRLDynamicSpawn.Components.TRLMapBubbleOverlay.Enable();
        }

        private void Update()
        {
        }
    };
}
