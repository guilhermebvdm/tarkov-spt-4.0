using BepInEx;
using Comfort.Common;
using DynamicExternalResolution.Configs;
using EFT;
using EFT.CameraControl;

namespace DynamicExternalResolution
{
    [BepInPlugin("com.Shibatsu.DynamicExternalResolution", "Shibatsu-DynamicExternalResolution", "1.1.1")]
    public class DynamicExternalResolution : BaseUnityPlugin
    {
        private static Player _localPlayer = null;

        public static Player getPlayerInstance()
        {
            if (_localPlayer != null)
            {
                return _localPlayer;
            }

            _localPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            return _localPlayer;
        }

        public static CameraManager getCameraInstance()
        {
            return CameraManager.Instance;
        }

        private void Awake()
        {
            DynamicExternalResolutionConfig.Init(Config);
            Patcher.PatchAll();
            Logger.LogInfo($"Plugin Dynamic External Resolution is loaded!");
        }

        private void OnDestroy()
        {
            Patcher.UnpatchAll();
            Logger.LogInfo($"Plugin DynamicExternalResolution is unloaded!");
        }
    }
}
