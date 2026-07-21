using System;
using System.IO;
using DiscordRaidMap.RaidMap;
using EFT;

namespace DiscordRaidMap.Discord
{
    internal sealed class RaidMapLifecycle : IDisposable
    {
        private RaidBroadcaster _broadcaster;

        public void OnGameStarted(GameWorld gameWorld)
        {
            StopBroadcaster();

            if (!DiscordRaidMap.RaidMap.HostCheck.CanBroadcast())
            {
                Plugin.Log.LogInfo("Discord raid map is disabled for Fika clients.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Settings.WebhookUrl.Value))
            {
                Plugin.Log.LogWarning("Webhook URL is empty; Discord raid map will not start.");
                return;
            }

            var collector = new RaidStateCollector(gameWorld);
            var renderer = CreateRenderer();
            var discord = new DiscordWebhookClient(Settings.WebhookUrl.Value);

            _broadcaster = new RaidBroadcaster(collector, renderer, discord, Settings.UpdateIntervalSeconds.Value);
            _broadcaster.Start();
        }

        public void OnRaidEnd()
        {
            StopBroadcaster();
        }

        public void Update()
        {
            _broadcaster?.Update();
        }

        public void OnUpdateIntervalChanged(object sender, EventArgs args)
        {
            _broadcaster?.SetUpdateInterval(Settings.UpdateIntervalSeconds.Value);
        }

        public void OnRendererSettingsChanged(object sender, EventArgs args)
        {
            _broadcaster?.ReplaceRenderer(CreateRenderer());
        }

        public void Dispose()
        {
            StopBroadcaster();
        }

        private static Renderer CreateRenderer()
        {
            return new Renderer(
                Path.Combine(Settings.PluginPath, "Assets", "Maps"),
                Path.Combine(Settings.PluginPath, "Assets", "Markers"),
                Path.Combine(Settings.PluginPath, "Assets", "Fonts"),
                Settings.MapTextFont.Value,
                Settings.MapTextFontSize.Value,
                Settings.MarkerDisplaySize.Value);
        }

        private void StopBroadcaster()
        {
            if (_broadcaster == null)
            {
                return;
            }

            _broadcaster.Stop();
            _broadcaster = null;
        }
    }
}
