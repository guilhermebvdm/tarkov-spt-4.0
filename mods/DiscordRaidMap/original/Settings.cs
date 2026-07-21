using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DiscordRaidMap
{
    internal static class Settings
    {
        internal static ConfigFile Config;
        internal static readonly List<ConfigEntryBase> ConfigEntries = new List<ConfigEntryBase>();

        internal static ConfigEntry<string> WebhookUrl;
        internal static ConfigEntry<int> UpdateIntervalSeconds;
        internal static ConfigEntry<string> MapTextFont;
        internal static ConfigEntry<int> MapTextFontSize;
        internal static ConfigEntry<int> MarkerDisplaySize;
        internal static ConfigEntry<string> MessageName;
        internal static string PluginPath;
        internal static void Init(ConfigFile config)
        {

            PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fontAssetPath = Path.Combine(PluginPath, "Assets", "Fonts");
            Directory.CreateDirectory(fontAssetPath);

            Config = config;
            ConfigEntries.Clear();

            ConfigEntries.Add(WebhookUrl = config.Bind("Discord", "Webhook Url", "",
                new ConfigDescription(
                    "Discord webhook URL used for the raid map message.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false })));

            ConfigEntries.Add(MessageName = config.Bind("Discord", "Message Name", "Raid Map",
                new ConfigDescription(
                    "Name used for the Discord map message.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false })));

            ConfigEntries.Add(UpdateIntervalSeconds = config.Bind("Discord", "Update Interval Seconds", 5,
                new ConfigDescription(
                    "How often to edit the Discord map message.",
                    new AcceptableValueRange<int>(2, 120),
                    new ConfigurationManagerAttributes { IsAdvanced = false })));

            ConfigEntries.Add(MapTextFont = config.Bind("Map Text", "Map Text Font", "DelaGothicOne-Regular.ttf", 
                new ConfigDescription(
                    "Font file used for map text. Add .ttf or .otf files to Assets\\Fonts, then restart the game to update this list.",
                    new AcceptableValueList<string>(GetFontChoices(fontAssetPath)),
                    new ConfigurationManagerAttributes { IsAdvanced = false })));

            ConfigEntries.Add(MapTextFontSize = config.Bind("Map Text", "Map Text Font Size", 36,
                new ConfigDescription(
                    "Font size used for map text.",
                    new AcceptableValueRange<int>(1, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = false })));

            ConfigEntries.Add(MarkerDisplaySize = config.Bind("Markers", "Marker Display Size", 60, 
                new ConfigDescription(
                    "Pixel size used to draw marker icons on the map. Source PNGs can be larger for sharper downsampling.",
                    new AcceptableValueRange<int>(16, 256),
                    new ConfigurationManagerAttributes { IsAdvanced = false })));

            RecalcOrder();
        }


        private static string[] GetFontChoices(string fontAssetPath)
        {
            var fonts = Directory.Exists(fontAssetPath)
                ? Directory.GetFiles(fontAssetPath)
                    .Where(path => string.Equals(Path.GetExtension(path), ".ttf", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(Path.GetExtension(path), ".otf", StringComparison.OrdinalIgnoreCase))
                    .Select(Path.GetFileName)
                    .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
                : Enumerable.Empty<string>();

            return new[] { "Default" }.Concat(fonts).ToArray();
        }
        private static void RecalcOrder()
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            int settingOrder = ConfigEntries.Count;
            foreach (var entry in ConfigEntries)
            {
                ConfigurationManagerAttributes attributes = entry.Description.Tags[0] as ConfigurationManagerAttributes;
                if (attributes != null)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
    }
}
