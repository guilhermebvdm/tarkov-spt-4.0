using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChangeHelmetVisor.Patches;

namespace ChangeHelmetVisor
{
    [BepInPlugin("com.trinagan.ChangeHelmetVisor", "ChangeHelmetVisor", "2.0.0")]
    public class ChangeHelmetVisor : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        private const string MainSectionName = "Main";
        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<float> zoomScaleWide;
        internal static ConfigEntry<float> zoomScaleNarrow;

        private static readonly string configPath = Path.Combine(BepInEx.Paths.PluginPath, "ChangeHelmetVisorClient", "config.jsonc");
        private void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("plugin loaded!");
            InitConfiguration();

            new VisorTexturePatch().Enable();
        }

        private void InitConfiguration()
        {
            ModEnabled = Config.Bind(
                MainSectionName,
                "Enabled",
                true,
                "Enables the mod.");

            zoomScaleNarrow = Config.Bind(
                MainSectionName,
                "Narrow Visor Zoom Slider",
                1.0f,
                new ConfigDescription(
                    "The narrow masks zoom slider",
                    new AcceptableValueRange<float>(1.0f, 4.0f)));

            zoomScaleWide = Config.Bind(
                MainSectionName,
                "Wide Visor Zoom Slider",
                1.0f,
                new ConfigDescription(
                    "The wide masks zoom slider",
                    new AcceptableValueRange<float>(1.0f, 1.5f)));
        }
    }
}
