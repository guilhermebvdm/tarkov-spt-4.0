using BepInEx;
using BepInEx.Logging;
using DiscordRaidMap.Discord;
using DiscordRaidMap.Patches;

namespace DiscordRaidMap
{
    [BepInPlugin("com.fiodor.discordraidmap", "Discord Raid Map", "1.1.3")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }
        private RaidMapLifecycle _lifecycle;

        private void Awake()
        {
            Log = Logger;
            Settings.Init(Config);

            _lifecycle = new RaidMapLifecycle();

            Settings.UpdateIntervalSeconds.SettingChanged += _lifecycle.OnUpdateIntervalChanged;
            Settings.MapTextFont.SettingChanged += _lifecycle.OnRendererSettingsChanged;
            Settings.MapTextFontSize.SettingChanged += _lifecycle.OnRendererSettingsChanged;
            Settings.MarkerDisplaySize.SettingChanged += _lifecycle.OnRendererSettingsChanged;
            Settings.MaxImageDimension.SettingChanged += _lifecycle.OnRendererSettingsChanged;
            Settings.ImageFormat.SettingChanged += _lifecycle.OnRendererSettingsChanged;
            Settings.JpegQuality.SettingChanged += _lifecycle.OnRendererSettingsChanged;

            new GameStartedPatch().Enable();
            new GameWorldOnDestroyPatch().Enable();

            GameStartedPatch.OnGameStarted += _lifecycle.OnGameStarted;
            GameWorldOnDestroyPatch.OnRaidEnd += _lifecycle.OnRaidEnd;

            Log.LogInfo("Discord Raid Map loaded.");
        }

        private void Update()
        {
            _lifecycle?.Update();
        }

        private void OnDestroy()
        {
            if (_lifecycle != null)
            {
                Settings.UpdateIntervalSeconds.SettingChanged -= _lifecycle.OnUpdateIntervalChanged;
                Settings.MapTextFont.SettingChanged -= _lifecycle.OnRendererSettingsChanged;
                Settings.MapTextFontSize.SettingChanged -= _lifecycle.OnRendererSettingsChanged;
                Settings.MarkerDisplaySize.SettingChanged -= _lifecycle.OnRendererSettingsChanged;
                Settings.MaxImageDimension.SettingChanged -= _lifecycle.OnRendererSettingsChanged;
                Settings.ImageFormat.SettingChanged -= _lifecycle.OnRendererSettingsChanged;
                Settings.JpegQuality.SettingChanged -= _lifecycle.OnRendererSettingsChanged;
                GameStartedPatch.OnGameStarted -= _lifecycle.OnGameStarted;
                GameWorldOnDestroyPatch.OnRaidEnd -= _lifecycle.OnRaidEnd;
                _lifecycle.Dispose();
                _lifecycle = null;
            }
        }
    }
}
