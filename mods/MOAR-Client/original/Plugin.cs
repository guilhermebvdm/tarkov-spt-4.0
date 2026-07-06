using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using HarmonyLib;
using MOAR.Helpers;
using MOAR.Patches;

namespace MOAR
{
    [
        BepInPlugin("MOAR.settings", "MOAR", "3.1.2"),
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
            Routers.Init(Config);

            new NoTeleportPatch().Enable();
            new SniperPatch().Enable();
            new AddEnemyPatch().Enable();
            new RefreshLocation().Enable();
            if (Settings.enablePointOverlay.Value)
            {
                new OnGameStartedPatch().Enable();
            }
            new NotificationPatch().Enable();
        }

        private void Update()
        {
            if (Settings.DeleteSniperSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.DeleteBotSpawn("sniper");
                    Methods.DisplayMessage(
                        "Deleted 1 sniper spawn point from "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }
            if (Settings.DeletePmcSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.DeleteBotSpawn("pmc");
                    Methods.DisplayMessage(
                        "Deleted 1 pmc spawn point from "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }
            if (Settings.DeleteScavSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.DeleteBotSpawn("scav");
                    Methods.DisplayMessage(
                        "Deleted 1 scav spawn point from "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }
            if (Settings.DeletePlayerSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.DeleteBotSpawn("player");
                    Methods.DisplayMessage(
                        "Deleted 1 player spawn point from "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }

            if (Settings.AddPmcSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.AddBotSpawn("pmc");
                    Methods.DisplayMessage(
                        "Added 1 pmc spawn point to "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }

            if (Settings.AddScavSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.AddBotSpawn("scav");
                    Methods.DisplayMessage(
                        "Added 1 scav spawn point to "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }

            if (Settings.AddSniperSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.AddBotSpawn("sniper");
                    Methods.DisplayMessage(
                        "Added 1 sniper spawn point to "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }

            if (Settings.AddPlayerSpawn.Value.BetterIsDown())
            {
                if (Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    Routers.AddBotSpawn("player");
                    Methods.DisplayMessage(
                        "Added 1 player spawn point to "
                            + Singleton<GameWorld>.Instance.MainPlayer.Location,
                        EFT.Communications.ENotificationIconType.Default
                    );
                }
            }

            if (Settings.AnnounceKey.Value.BetterIsDown())
            {
                Methods.DisplayMessage(
                    "Current preset is " + Routers.GetAnnouncePresetName(),
                    EFT.Communications.ENotificationIconType.EntryPoint
                );
            }
        }
    };
}
