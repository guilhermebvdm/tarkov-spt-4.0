using BepInEx.Configuration;
using System.Linq;

namespace ZGFueDkx.ZGCLib.Config
{
    internal static class ConfigHelper
    {
        public static ConfigCategory MakeCategory(this ConfigFile config, int order, string name)
        {
            return new ConfigCategory(config, order, name);
        }

        public static void SetReadOnly(this ConfigEntryBase entry, bool readOnly)
        {
            if (TryGetAttributes(entry, out var attributes))
            {
                attributes.ReadOnly = readOnly;
            }
        }

        public static void SetAdvanced(this ConfigEntryBase entry, bool advanced)
        {
            if (TryGetAttributes(entry, out var attributes))
            {
                attributes.IsAdvanced = advanced;
            }
        }

        private static bool TryGetAttributes(ConfigEntryBase entry, out ConfigurationManagerAttributes attributes)
        {
            attributes = (ConfigurationManagerAttributes)
                entry.Description.Tags.FirstOrDefault(t => t.GetType() == typeof(ConfigurationManagerAttributes));

            return attributes is not null;
        }

        public static T GetValue<T>(this ConfigEntry<T> entry, bool useDefault = false)
        {
            T def = (T)entry.DefaultValue;
            return useDefault ? def : entry.Value ?? def;
        }
    }
}
