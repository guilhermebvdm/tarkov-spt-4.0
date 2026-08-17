using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;
using UnityEngine;

namespace tarkin.ladders.fika
{
    [BepInPlugin("com.tarkin.ladders.fika", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.tarkin.ladders", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.tarkin.ladders", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; }

        FikaHandler fikaHandler;

        void Start()
        {
            Logger = base.Logger;

            fikaHandler = new FikaHandler();
        }

        void OnDestroy()
        {
            FindObjectsByTypeAndDestroy<ObservedPlayerLadderController>();

            fikaHandler.Dispose();

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
