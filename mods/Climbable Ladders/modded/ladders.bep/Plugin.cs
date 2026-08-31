using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using UnityEngine;

namespace tarkin.ladders.bep
{
    [BepInPlugin("com.tarkin.ladders", "tarkin.ladders", "1.0.5")]
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private PatchManager patchManager;

        private LaddersLoader laddersLoader;

        void Start()
        {
            Logger = base.Logger;

            patchManager = new PatchManager(this, autoPatch: true);
            patchManager.EnablePatches();

            laddersLoader = new LaddersLoader(System.IO.Path.Combine(BepInEx.Paths.PluginPath, "tarkin-ladders"));

            Patch_GameWorld_OnGameStarted.OnPostfix += laddersLoader.Load;
            Patch_GameWorld_Dispose.OnPostfix += laddersLoader.Unload;

            if (Singleton<GameWorld>.Instantiated)
            {
                laddersLoader.Load(Singleton<GameWorld>.Instance);

#if DEBUG
                Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                if (mainPlayer != null && mainPlayer.ActiveHealthController != null)
                {
                    mainPlayer.ActiveHealthController.ChangeHydration(100f);
                    mainPlayer.ActiveHealthController.ChangeEnergy(100f);
                }
#endif
            }
        }

        public static void DisplayNotification(object obj)
        {
            NotificationManager.DisplayMessageNotification(obj.ToString());
        }

        void OnDestroy()
        {
            Patch_GameWorld_OnGameStarted.OnPostfix -= laddersLoader.Load;
            Patch_GameWorld_Dispose.OnPostfix -= laddersLoader.Unload;

            Patch_MoveInputTranslator_TranslateAxes.ClearAllSubscribers();

            laddersLoader.Dispose();

            patchManager.DisablePatches();
            patchManager = null;

            FindObjectsByTypeAndDestroy<PlayerLadderController>();

            Logger = null;
        }

        static void FindObjectsByTypeAndDestroy<T>() where T : Object
        {
            foreach (var item in FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Component.Destroy(item);
            }
        }
    }
}
