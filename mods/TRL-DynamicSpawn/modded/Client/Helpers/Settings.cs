using BepInEx.Configuration;
using System.Collections.Generic;

namespace TRLDynamicSpawn.Helpers
{
    public class Settings
    {
        public static ConfigEntry<string> activePerformancePreset;

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

        public static ConfigEntry<bool> enableLoSCulling;
        public static ConfigEntry<float> losCullingDistance;

        public static ConfigEntry<bool> enableSmoothSpawning;
        public static ConfigEntry<float> smoothSpawningDelay;

        public static ConfigEntry<bool> enableDebugLogs;

        public static ConfigEntry<bool> masterDespawnToggle;
        public static ConfigEntry<bool> enableSpawnBubble;

        public static ConfigEntry<bool> enableMapOverlay;
        public static ConfigEntry<bool> showSafeZoneCircle;
        public static ConfigEntry<bool> showSpawnBubbleCircle;
        public static ConfigEntry<bool> showLoSCone;

        // Corpse Optimization & Cleanup
        public static ConfigEntry<bool> enableCorpseCleanup;
        public static ConfigEntry<string> corpseCleanupMode;
        public static ConfigEntry<float> corpseLifetimeMinutes;
        public static ConfigEntry<float> corpseMinSafeDistance;
        public static ConfigEntry<bool> protectBossCorpses;
        public static ConfigEntry<bool> corpseCheckLoS;

        public static ConfigEntry<bool> reloadServerConfig;
        public static ConfigEntry<int> initialProfilePreload;

        public static void Init(ConfigFile config)
        {
            string presetSection = "Performance Presets & Style";
            activePerformancePreset = config.Bind(presetSection, "Active Performance Preset", "Balanced", 
                new ConfigDescription("Select the active performance preset for spawns. Options: 'Balanced' (Dynamic Bubble), 'High-End' (Full Map / Vanilla), 'Performance' (Compact Bubble / FPS Boost).", 
                new AcceptableValueList<string>("Balanced", "High-End", "Performance")));

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

            string losSection = "Spawn Culling (Line of Sight)";
            enableLoSCulling = config.Bind(losSection, "Enable LoS Culling", true, 
                new ConfigDescription("Prevents bots from spawning in the field of view of players."));

            losCullingDistance = config.Bind(losSection, "LoS Culling Max Distance", 150f, 
                new ConfigDescription("Max distance to check for player's line of sight before spawning.", new AcceptableValueRange<float>(10f, 500f)));

            string spawnSection = "Smooth Spawning";
            enableSmoothSpawning = config.Bind(spawnSection, "Enable Smooth Spawning", true, 
                new ConfigDescription("Enable staggered bot spawns to reduce stuttering during waves."));
            
            smoothSpawningDelay = config.Bind(spawnSection, "Smooth Spawning Delay", 1.5f, 
                new ConfigDescription("Delay in seconds between bot/group spawns if Smooth Spawning is enabled.", new AcceptableValueRange<float>(0f, 10f)));

            string debugSection = "Debug Logs";
            enableDebugLogs = config.Bind(debugSection, "Enable Debug Logs", false, 
                new ConfigDescription("Enable debug logs for map culling and dynamic spawns in the console."));

            string teleportSection = "Bot Teleport & Density Settings";
            masterDespawnToggle = config.Bind(teleportSection, "Enable Bot Teleport System", true, 
                new ConfigDescription("Master toggle to enable or disable the bot teleport system (relocates distant bots into active combat zones)."));

            string bubbleSection = "Spawn Bubble Settings";
            enableSpawnBubble = config.Bind(bubbleSection, "Enable Spawn Bubble", true, 
                new ConfigDescription("Forces Scavs and PMCs to spawn only within the radius of Despawn Distance from players, improving performance."));

            string overlaySection = "Map Overlay (SPT-DynamicMaps)";
            enableMapOverlay = config.Bind(overlaySection, "Enable Map Overlay", true, 
                new ConfigDescription("Master toggle for visual circles and LoS cone overlay on SPT-DynamicMaps."));

            showSafeZoneCircle = config.Bind(overlaySection, "Show Safe Zone Circle", true, 
                new ConfigDescription("Displays the red inner Safe Zone circle around the player on the map."));

            showSpawnBubbleCircle = config.Bind(overlaySection, "Show Spawn Bubble Circle", true, 
                new ConfigDescription("Displays the cyan outer Spawn Bubble circle around the player on the map."));

            showLoSCone = config.Bind(overlaySection, "Show LoS / FOV Cone", true,
                new ConfigDescription("Displays the yellow player Field of View (LoS) cone on the map."));

            string corpseSection = "Corpse Optimization & Cleanup";
            enableCorpseCleanup = config.Bind(corpseSection, "Enable Corpse Cleanup", true,
                new ConfigDescription("Master toggle to optimize dead bodies after cooldown (freezes physics and reduces draw calls)."));

            corpseCleanupMode = config.Bind(corpseSection, "Cleanup Mode", "Backpack Convert",
                new ConfigDescription("Mode: 'Backpack Convert' hides body meshes and keeps loot interactive; 'Full Despawn' destroys body and loot completely.",
                new AcceptableValueList<string>("Backpack Convert", "Full Despawn")));

            corpseLifetimeMinutes = config.Bind(corpseSection, "Corpse Lifetime (Minutes)", 5.0f,
                new ConfigDescription("Time in minutes a corpse remains untouched before conversion or despawn.",
                new AcceptableValueRange<float>(1.0f, 30.0f)));

            corpseMinSafeDistance = config.Bind(corpseSection, "Min Safe Distance to Players (M)", 25.0f,
                new ConfigDescription("Minimum distance from any living human player before a corpse can be converted or despawned.",
                new AcceptableValueRange<float>(10.0f, 100.0f)));

            protectBossCorpses = config.Bind(corpseSection, "Protect Boss Corpses", true,
                new ConfigDescription("If enabled, Bosses, Rogues, Raiders and Cultists corpses will never be converted or despawned."));

            corpseCheckLoS = config.Bind(corpseSection, "Check Line of Sight (LoS)", true,
                new ConfigDescription("If enabled, will not convert or despawn corpses while any living human player is directly looking at it."));

            // ref: AUD-01-01 — manual path that replaces the 5 s live re-fetch (AC-X1). Acts as a button:
            // only reacts to true, clears the raid-scoped cache and resets itself to false. Zero per-frame cost.
            string serverSection = "Server Config";
            reloadServerConfig = config.Bind(serverSection, "Reload Server Config", false,
                new ConfigDescription("Tick to reload the web panel configuration now (applies edits made during the raid). Unticks itself after reloading."));
            reloadServerConfig.SettingChanged += (_, __) =>
            {
                if (!reloadServerConfig.Value) return;          // the reset below re-enters with false → ignored
                ServerConfigProvider.ForceRefresh();
                Plugin.LogSource?.LogInfo("[TRL-DynamicSpawn] Server config cache cleared by user (F12). Next read will fetch.");
                reloadServerConfig.Value = false;
            };

            // ref: AUD-01-04 / AC-X2 / CR-01-01 — STANDING PMC profile cache level (USEC and BEAR, normal difficulty) that SPT keeps
            // replenished during the whole raid (was fixed 30/30). Scav levels are vanilla's (8 per difficulty). Read once per raid.
            // Minimum 5: with no PMC cache every PMC slot becomes a synchronous LoadBots(3) at spawn time.
            string poolSection = "Profile Pool (Advanced)";
            initialProfilePreload = config.Bind(poolSection, "Initial Profile Preload", 15,
                new ConfigDescription("Standing cache level of PMC bot profiles (USEC and BEAR, normal difficulty) that the game keeps replenished during the whole raid. Higher = first wave ready sooner, more memory. Scav profiles are managed by the game (8 per difficulty).",
                    new AcceptableValueRange<int>(5, 30), new ConfigurationManagerAttributes { IsAdvanced = true }));
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
