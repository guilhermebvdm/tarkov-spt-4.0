using BepInEx.Configuration;
using System.Collections.Generic;

namespace MOAR.Helpers
{
    public class Settings
    {
        public static ConfigEntry<int> maxBotCapFactory;
        public static ConfigEntry<int> maxBotCapCustoms;
        public static ConfigEntry<int> maxBotCapWoods;
        public static ConfigEntry<int> maxBotCapShoreline;
        public static ConfigEntry<int> maxBotCapInterchange;
        public static ConfigEntry<int> maxBotCapReserve;
        public static ConfigEntry<int> maxBotCapLighthouse;
        public static ConfigEntry<int> maxBotCapStreets;
        public static ConfigEntry<int> maxBotCapGroundZero;
        public static ConfigEntry<int> maxBotCapLaboratory;

        public static void Init(ConfigFile config)
        {
            string section = "Host Performance Caps";

            maxBotCapFactory = config.Bind(section, "Factory Max Bots", 15, 
                new ConfigDescription("Maximum number of bots allowed on Factory.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapCustoms = config.Bind(section, "Customs Max Bots", 25, 
                new ConfigDescription("Maximum number of bots allowed on Customs.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapWoods = config.Bind(section, "Woods Max Bots", 25, 
                new ConfigDescription("Maximum number of bots allowed on Woods.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapShoreline = config.Bind(section, "Shoreline Max Bots", 25, 
                new ConfigDescription("Maximum number of bots allowed on Shoreline.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapInterchange = config.Bind(section, "Interchange Max Bots", 25, 
                new ConfigDescription("Maximum number of bots allowed on Interchange.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapReserve = config.Bind(section, "Reserve Max Bots", 25, 
                new ConfigDescription("Maximum number of bots allowed on Reserve.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapLighthouse = config.Bind(section, "Lighthouse Max Bots", 25, 
                new ConfigDescription("Maximum number of bots allowed on Lighthouse.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapStreets = config.Bind(section, "Streets Max Bots", 30, 
                new ConfigDescription("Maximum number of bots allowed on Streets.", new AcceptableValueRange<int>(5, 60)));

            maxBotCapGroundZero = config.Bind(section, "Ground Zero Max Bots", 20, 
                new ConfigDescription("Maximum number of bots allowed on Ground Zero.", new AcceptableValueRange<int>(5, 50)));

            maxBotCapLaboratory = config.Bind(section, "Laboratory Max Bots", 20, 
                new ConfigDescription("Maximum number of bots allowed on Laboratory.", new AcceptableValueRange<int>(5, 50)));
        }

        public static int GetMapCap(string mapId)
        {
            string lowerMapId = mapId.ToLower();
            if (lowerMapId.Contains("factory")) return maxBotCapFactory.Value;
            if (lowerMapId.Contains("bigmap")) return maxBotCapCustoms.Value;
            if (lowerMapId.Contains("woods")) return maxBotCapWoods.Value;
            if (lowerMapId.Contains("shoreline")) return maxBotCapShoreline.Value;
            if (lowerMapId.Contains("interchange")) return maxBotCapInterchange.Value;
            if (lowerMapId.Contains("rezerv")) return maxBotCapReserve.Value;
            if (lowerMapId.Contains("lighthouse")) return maxBotCapLighthouse.Value;
            if (lowerMapId.Contains("tarkovstreets")) return maxBotCapStreets.Value;
            if (lowerMapId.Contains("sandbox")) return maxBotCapGroundZero.Value;
            if (lowerMapId.Contains("laboratory")) return maxBotCapLaboratory.Value;
            
            return 30; // Default fallback
        }
    }
}
