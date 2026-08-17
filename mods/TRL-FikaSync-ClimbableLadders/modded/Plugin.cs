using BepInEx;
using BepInEx.Logging;
using TRL.FikaSync.ClimbableLadders.Controllers;
using TRL.FikaSync.ClimbableLadders.Networking;
using UnityEngine;

namespace TRL.FikaSync.ClimbableLadders
{
    [BepInPlugin("com.trl.fikasync.climbableladders", "TRL-FikaSync-ClimbableLadders", "1.0.0")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.tarkin.ladders", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        private LadderNetworkHandler _networkHandler;

        private void Awake()
        {
            Logger = base.Logger;

            _networkHandler = new LadderNetworkHandler();

            Logger.LogInfo("TRL-FikaSync-ClimbableLadders carregado com sucesso!");
        }

        private void OnDestroy()
        {
            FindObjectsByTypeAndDestroy<ObservedPlayerLadderController>();

            _networkHandler?.Dispose();
            _networkHandler = null;

            Logger = null;
        }

        private static void FindObjectsByTypeAndDestroy<T>() where T : Object
        {
            foreach (var item in FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Destroy(item);
            }
        }
    }
}
