using BepInEx.Configuration;
using System.Linq;

namespace CameraRotationMod
{
    public static class TranslationUpdater
    {
        public static void Update(ConfigFile config)
        {
            foreach (ConfigDefinition def in config.Keys)
            {
                var entry = config[def];
                
                if (entry.Description.Tags != null)
                {
                    var attrs = entry.Description.Tags.OfType<ConfigurationManagerAttributes>().FirstOrDefault();
                    if (attrs != null)
                    {
                        attrs.DispName = LocaleUtils.Localize(def.Key);
                        attrs.Category = LocaleUtils.Localize(def.Section);
                    }
                }
            }
        }
    }
}
