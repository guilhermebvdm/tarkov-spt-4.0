using System;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace MOAR.Helpers
{
    internal class Settings
    {
        private static ConfigFile _config;

        public static ConfigEntry<bool> ShowPresetOnRaidStart;
        public static bool IsFika = false;
        public static ConfigSettings ServerStoredValues;
        public static ConfigSettings ServerStoredDefaults;
        public static ConfigEntry<bool> debug;
        public static ConfigEntry<bool> factionAggression;
        public static ConfigEntry<bool> enablePointOverlay;
        public static ConfigEntry<double> pmcDifficulty;
        public static ConfigEntry<double> scavDifficulty;

        public static ConfigEntry<bool> zombiesEnabled;
        public static ConfigEntry<double> zombieWaveDistribution;
        public static ConfigEntry<double> zombieWaveQuantity;
        public static ConfigEntry<double> zombieHealth;
        public static ConfigEntry<double> scavWaveDistribution;

        public static ConfigEntry<double> scavWaveQuantity;

        public static ConfigEntry<bool> startingPmcs;
        public static ConfigEntry<bool> spawnSmoothing;
        public static ConfigEntry<bool> randomSpawns;
        public static ConfigEntry<double> pmcWaveDistribution;
        public static ConfigEntry<double> pmcWaveQuantity;

        public static ConfigEntry<int> maxBotCap;
        public static ConfigEntry<int> maxBotPerZone;
        public static ConfigEntry<double> scavGroupChance;
        public static ConfigEntry<double> pmcGroupChance;
        public static ConfigEntry<double> sniperGroupChance;
        public static ConfigEntry<int> pmcMaxGroupSize;
        public static ConfigEntry<int> scavMaxGroupSize;
        public static ConfigEntry<double> sniperMaxGroupSize;
        public static ConfigEntry<bool> bossOpenZones;
        public static ConfigEntry<bool> randomRaiderGroup;
        public static ConfigEntry<int> randomRaiderGroupChance;
        public static ConfigEntry<bool> randomRogueGroup;
        public static ConfigEntry<int> randomRogueGroupChance;
        public static ConfigEntry<bool> disableBosses;
        public static ConfigEntry<KeyboardShortcut> DeleteScavSpawn;
        public static ConfigEntry<KeyboardShortcut> DeleteSniperSpawn;
        public static ConfigEntry<KeyboardShortcut> DeletePlayerSpawn;
        public static ConfigEntry<KeyboardShortcut> DeletePmcSpawn;
        public static ConfigEntry<KeyboardShortcut> AddScavSpawn;
        public static ConfigEntry<KeyboardShortcut> AddSniperSpawn;
        public static ConfigEntry<KeyboardShortcut> AddPlayerSpawn;
        public static ConfigEntry<KeyboardShortcut> AddPmcSpawn;

        public static ConfigEntry<KeyboardShortcut> AnnounceKey;
        public static ConfigEntry<int> mainBossChanceBuff;
        public static ConfigEntry<bool> bossInvasion;
        public static ConfigEntry<int> bossInvasionSpawnChance;
        public static ConfigEntry<bool> gradualBossInvasion;

        public static Preset[] PresetList;
        public static double LastUpdatedServer = 0;
        public static ConfigEntry<string> currentPreset;

        public static ManualLogSource Log;

        public static void Init(ConfigFile Config)
        {
            _config = Config;
            Log = Plugin.LogSource;
            IsFika = Chainloader.PluginInfos.ContainsKey("com.fika.core");
            ServerStoredDefaults = Routers.GetDefaultConfig();
            PresetList = Routers.GetPresetsList();
            UpdateServerStoredValues();

            // Main SETTINGS =====================================


            factionAggression = Config.Bind(
                "1. Main Settings",
                "Faction Based Aggression On/Off",
                false,
                new ConfigDescription(
                    "Enabling this feature makes it so that AI PMCs from the same faction will not engage in combat with one another. You know, like actual military contractors. (This is client side only)",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        Order = 93,
                        ReadOnly = !!startingPmcs?.Value,
                    }
                )
            );

            randomSpawns = Config.Bind(
                "1. Main Settings",
                "randomSpawns On/Off",
                ServerStoredDefaults.randomSpawns,
                new ConfigDescription(
                    "This turns off the new cascading spawn system, and makes scavs/pmcs spawn at random across the map.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        Order = 94,
                        ReadOnly = !!startingPmcs?.Value,
                    }
                )
            );

            spawnSmoothing = Config.Bind(
                "1. Main Settings",
                "spawnSmoothing On/Off",
                ServerStoredDefaults.spawnSmoothing,
                new ConfigDescription(
                    "Improves performance: Ensures spawn spacing between waves. (doesn't change quantity, or change overall timing, just prevents bursts of spawns close together)",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        Order = 95,
                        ReadOnly = !!startingPmcs?.Value,
                    }
                )
            );

            startingPmcs = Config.Bind(
                "1. Main Settings",
                "Starting PMCS On/Off",
                ServerStoredDefaults.startingPmcs,
                new ConfigDescription(
                    "Performance Hit: Causes all PMCs to spawn in the first few minutes of the game (performance intensive)",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false, Order = 95 }
                )
            );

            pmcDifficulty = Config.Bind(
                "1. Main Settings",
                "Pmc difficulty",
                ServerStoredDefaults.pmcDifficulty,
                new ConfigDescription(
                    "Works with SAIN or SPT to decide the bot's 'difficulty' preset (EASY: 0, easy-MEDIUM: 0.4,  easy-MEDIUM-hard: 0.6, medium-hard: 0.85, HARD-impossible: 1, etc..)",
                    new AcceptableValueRange<double>(0, 1.5),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.pmcDifficulty,
                        Order = 96,
                    }
                )
            );

            scavDifficulty = Config.Bind(
                "1. Main Settings",
                "Scav difficulty",
                ServerStoredDefaults.scavDifficulty,
                new ConfigDescription(
                    "Works with SAIN or SPT to decide the bot's 'difficulty' preset (EASY: 0, easy-MEDIUM: 0.4,  easy-MEDIUM-hard: 0.6, medium-hard: 0.85, HARD-impossible: 1, etc..)",
                    new AcceptableValueRange<double>(0, 1.5),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.scavDifficulty,
                        Order = 97,
                    }
                )
            );

            currentPreset = Config.Bind(
                "1. Main Settings",
                "Moar Preset",
                "Random",
                new ConfigDescription(
                    "Preset to be used, random pulls a random weighted preset from the PresetWeights.json every time a raid ends",
                    new AcceptableValueList<string>(PresetList.Select(item => item.Name).ToArray()),
                    new ConfigurationManagerAttributes { ShowRangeAsPercent = false, Order = 98 }
                )
            );

            ShowPresetOnRaidStart = Config.Bind(
                "1. Main Settings",
                "Preset Announce On/Off",
                true,
                new ConfigDescription(
                    "Enable/Disable preset announce preset on raid start",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false, Order = 99 }
                )
            );

            AnnounceKey = Config.Bind(
                "1. Main Settings",
                "Announce Key",
                new KeyboardShortcut(KeyCode.End),
                new ConfigDescription(
                    "Announces preset",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false, Order = 100 }
                )
            );

            if (IsFika)
            {
                CreateSimpleButton(
                    "1. Main Settings",
                    "FIKA DETECTED: ALWAYS PRESS THIS FIRST BEFORE MAKING CHANGES!!",
                    "Pull settings from server",
                    "Pulls all server settings from server",
                    () =>
                    {
                        UpdateServerStoredValues();
                        UpdateValuesFromServerStoredValues();
                        Methods.DisplayMessage("Pulled latest settings from servers");
                        return "";
                    },
                    100
                );
            }

            // Main SETTINGS =====================================
            CreateSimpleButton(
                "2. Custom game Settings",
                "Save the above changes",
                "Save",
                "Pushes settings to server",
                () =>
                {
                    if (currentPreset.Value != "Custom" && !CustomUnchanged())
                    {
                        currentPreset.Value = "Custom";
                    }
                    OverwriteServerStoredValuesAndSubmit();
                    UpdateValuesFromServerStoredValues();
                    Methods.DisplayMessage("Pushed latest settings to servers");
                    return "";
                },
                75
            );

            CreateSimpleButton(
                "2. Custom game Settings",
                "Reset",
                "Reset all settings to defaults",
                "Resets all settings to defaults",
                () =>
                {
                    currentPreset.Value = "Random";
                    UpdateValuesFromDefaults(true);
                    Routers.SetOverrideConfig(ServerStoredDefaults);
                    Methods.DisplayMessage("Reset all settings");
                    return "";
                },
                76
            );

            gradualBossInvasion = Config.Bind(
                "2. Custom game Settings",
                "gradualBossInvasion On/Off",
                ServerStoredDefaults.gradualBossInvasion,
                new ConfigDescription(
                    "Makes it so invading bosses do not spawn all at the beginning (recommend for performance)",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.gradualBossInvasion,
                        Order = 77,
                    }
                )
            );

            bossInvasionSpawnChance = Config.Bind(
                "2. Custom game Settings",
                "bossInvasionSpawnChance",
                ServerStoredDefaults.bossInvasionSpawnChance,
                new ConfigDescription(
                    "Percentage chance of each invading boss spawning.",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = true,
                        DefaultValue = ServerStoredDefaults.bossInvasionSpawnChance,
                        Order = 78,
                    }
                )
            );

            bossInvasion = Config.Bind(
                "2. Custom game Settings",
                "bossInvasion On/Off",
                ServerStoredDefaults.bossInvasion,
                new ConfigDescription(
                    "Allows the main bosses (not knight,rogues,raiders) to invade other maps with a reduced retinue, by default they will spawn in native boss locations",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.bossInvasion,
                        Order = 79,
                    }
                )
            );

            mainBossChanceBuff = Config.Bind(
                "2. Custom game Settings",
                "mainBossChanceBuff",
                ServerStoredDefaults.mainBossChanceBuff,
                new ConfigDescription(
                    "Increases the spawn chance of the single 'main' boss on each map by this percentage",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = true,
                        DefaultValue = ServerStoredDefaults.mainBossChanceBuff,
                        Order = 80,
                    }
                )
            );

            disableBosses = Config.Bind(
                "2. Custom game Settings",
                "disableBosses On/Off",
                ServerStoredDefaults.disableBosses,
                new ConfigDescription(
                    "Disables all bosses, good for debugging.",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.disableBosses,
                        Order = 81,
                    }
                )
            );

            randomRogueGroupChance = Config.Bind(
                "2. Custom game Settings",
                "randomRogueGroupChance",
                ServerStoredDefaults.randomRogueGroupChance,
                new ConfigDescription(
                    "Chance of a rogue group spawning",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = true,
                        DefaultValue = ServerStoredDefaults.randomRogueGroupChance,
                        Order = 82,
                    }
                )
            );

            randomRogueGroup = Config.Bind(
                "2. Custom game Settings",
                "randomRogueGroup On/Off",
                ServerStoredDefaults.randomRogueGroup,
                new ConfigDescription(
                    "Experimental: Makes it so a randomRogueGroup can spawn anywhere",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.randomRogueGroup,
                        Order = 83,
                    }
                )
            );

            randomRaiderGroupChance = Config.Bind(
                "2. Custom game Settings",
                "randomRaiderGroupChance",
                ServerStoredDefaults.randomRaiderGroupChance,
                new ConfigDescription(
                    "Chance of a raider group spawning",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = true,
                        DefaultValue = ServerStoredDefaults.randomRaiderGroupChance,
                        Order = 84,
                    }
                )
            );

            randomRaiderGroup = Config.Bind(
                "2. Custom game Settings",
                "randomRaiderGroup On/Off",
                ServerStoredDefaults.randomRaiderGroup,
                new ConfigDescription(
                    "Experimental: Makes it so a randomRaiderGroup can spawn anywhere",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.randomRaiderGroup,
                        Order = 85,
                    }
                )
            );

            bossOpenZones = Config.Bind(
                "2. Custom game Settings",
                "bossOpenZones On/Off",
                ServerStoredDefaults.bossOpenZones,
                new ConfigDescription(
                    "Experimental: Makes it so the main bosses can spawn anywhere",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.bossOpenZones,
                        Order = 86,
                    }
                )
            );

            sniperMaxGroupSize = Config.Bind(
                "2. Custom game Settings",
                "sniperMaxGroupSize",
                ServerStoredDefaults.sniperMaxGroupSize,
                new ConfigDescription(
                    "Sniper Max group size",
                    new AcceptableValueRange<double>(0, 5),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.sniperMaxGroupSize,
                        Order = 87,
                    }
                )
            );

            scavMaxGroupSize = Config.Bind(
                "2. Custom game Settings",
                "scavMaxGroupSize",
                ServerStoredDefaults.scavMaxGroupSize,
                new ConfigDescription(
                    "Max scav group size",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.scavMaxGroupSize,
                        Order = 88,
                    }
                )
            );

            pmcMaxGroupSize = Config.Bind(
                "2. Custom game Settings",
                "pmcMaxGroupSize",
                ServerStoredDefaults.pmcMaxGroupSize,
                new ConfigDescription(
                    "Max pmc group size",
                    new AcceptableValueRange<int>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.pmcMaxGroupSize,
                        Order = 89,
                    }
                )
            );

            pmcGroupChance = Config.Bind(
                "2. Custom game Settings",
                "pmcGroupChance Percentage",
                ServerStoredDefaults.pmcGroupChance,
                new ConfigDescription(
                    "Controls the chance of group vs solo  spawning, max amount configured by maxGroup",
                    new AcceptableValueRange<double>(0, 1),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.pmcGroupChance,
                        Order = 90,
                    }
                )
            );

            scavGroupChance = Config.Bind(
                "2. Custom game Settings",
                "scavGroupChance Percentage",
                ServerStoredDefaults.scavGroupChance,
                new ConfigDescription(
                    "Controls the chance of group vs solo spawning, max amount configured by maxGroup",
                    new AcceptableValueRange<double>(0, 1),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.scavGroupChance,
                        Order = 91,
                    }
                )
            );

            sniperGroupChance = Config.Bind(
                "2. Custom game Settings",
                "sniperGroupChance Percentage",
                ServerStoredDefaults.sniperGroupChance,
                new ConfigDescription(
                    "Controls the chance of group vs solo spawning, max amount configured by maxGroup",
                    new AcceptableValueRange<double>(0, 1),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.sniperGroupChance,
                        Order = 91,
                    }
                )
            );

            maxBotPerZone = Config.Bind(
                "2. Custom game Settings",
                "MaxBotPerZone",
                ServerStoredDefaults.maxBotPerZone,
                new ConfigDescription(
                    "Max bots permitted in any particular spawn zone, recommend not to touch this.",
                    new AcceptableValueRange<int>(0, 15),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.maxBotPerZone,
                        Order = 92,
                    }
                )
            );

            maxBotCap = Config.Bind(
                "2. Custom game Settings",
                "MaxBotCap",
                ServerStoredDefaults.maxBotCap,
                new ConfigDescription(
                    "Max bots alive at one time",
                    new AcceptableValueRange<int>(0, 50),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.maxBotCap,
                        Order = 93,
                    }
                )
            );

            zombieHealth = Config.Bind(
                "2. Custom game Settings",
                "ZombieHealth",
                ServerStoredDefaults.zombieHealth,
                new ConfigDescription(
                    "This controls the health of zombies",
                    new AcceptableValueRange<double>(0, 3),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.zombieHealth,
                        Order = 94,
                    }
                )
            );

            zombieWaveQuantity = Config.Bind(
                "2. Custom game Settings",
                "ZombieWaveQuantity",
                ServerStoredDefaults.zombieWaveQuantity,
                new ConfigDescription(
                    "Multiplies wave counts seen in the server's mapConfig.json by this number",
                    new AcceptableValueRange<double>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.zombieWaveQuantity,
                        Order = 95,
                    }
                )
            );

            zombieWaveDistribution = Config.Bind(
                "2. Custom game Settings",
                "ZombieWaveDistribution",
                ServerStoredDefaults.zombieWaveDistribution,
                new ConfigDescription(
                    "Determines the weighting of spawns, (0.1) waves will spawn mostly at the beginning, (1) Default: waves are spread evenly, or (1.9) spawns will appear near the end of the raid of the raid",
                    new AcceptableValueRange<double>(0.1, 1.9),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.zombieWaveDistribution,
                        Order = 96,
                    }
                )
            );

            zombiesEnabled = Config.Bind(
                "2. Custom game Settings",
                "zombiesEnabled On/Off",
                ServerStoredDefaults.zombiesEnabled,
                new ConfigDescription(
                    "Enables zombies to spawn",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.zombiesEnabled,
                        Order = 97,
                    }
                )
            );

            scavWaveDistribution = Config.Bind(
                "2. Custom game Settings",
                "ScavWaveDistribution",
                ServerStoredDefaults.scavWaveDistribution,
                new ConfigDescription(
                    "Determines the weighting of spawns, (0.1) waves will spawn mostly at the beginning, (1) Default: waves are spread evenly, or (1.9) spawns will appear near the end of the raid of the raid",
                    new AcceptableValueRange<double>(0.1, 1.9),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.scavWaveDistribution,
                        Order = 98,
                    }
                )
            );

            pmcWaveDistribution = Config.Bind(
                "2. Custom game Settings",
                "PmcWaveDistribution",
                ServerStoredDefaults.pmcWaveDistribution,
                new ConfigDescription(
                    "Determines the weighting of spawns, (0.1) waves will spawn mostly at the beginning, (1) Default: waves are spread evenly, or (1.9) spawns will appear near the end of the raid of the raid",
                    new AcceptableValueRange<double>(0.1, 1.9),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.pmcWaveDistribution,
                        Order = 99,
                    }
                )
            );

            scavWaveQuantity = Config.Bind(
                "2. Custom game Settings",
                "ScavWaveQuantity",
                ServerStoredDefaults.scavWaveQuantity,
                new ConfigDescription(
                    "Multiplies wave counts seen in the server's mapConfig.json by this number",
                    new AcceptableValueRange<double>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.scavWaveQuantity,
                        Order = 100,
                    }
                )
            );

            pmcWaveQuantity = Config.Bind(
                "2. Custom game Settings",
                "PmcWaveQuantity",
                ServerStoredDefaults.pmcWaveQuantity,
                new ConfigDescription(
                    "Multiplies wave counts seen in the server's mapConfig.json by this number",
                    new AcceptableValueRange<double>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.pmcWaveQuantity,
                        Order = 101,
                    }
                )
            );

            debug = Config.Bind(
                "3.Debug",
                "debug On/Off",
                ServerStoredDefaults.debug,
                new ConfigDescription(
                    "This is for debugging server output, leave off if you don't know what you're doing",
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        DefaultValue = ServerStoredDefaults.debug,
                        Order = 100,
                    }
                )
            );

            DeleteSniperSpawn = Config.Bind(
                "4. Advanced",
                "Delete a sniper spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to delete the nearest sniper spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            DeletePlayerSpawn = Config.Bind(
                "4. Advanced",
                "Delete a player spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to delete the nearest player starting spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            DeletePmcSpawn = Config.Bind(
                "4. Advanced",
                "Delete a pmc spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to delete the nearest pmc spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            DeleteScavSpawn = Config.Bind(
                "4. Advanced",
                "Delete a scav spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to delete the nearest scav spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            AddSniperSpawn = Config.Bind(
                "4. Advanced",
                "Add a sniper spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to add a sniper spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            AddPlayerSpawn = Config.Bind(
                "4. Advanced",
                "Add a player spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to add a player starting spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            AddPmcSpawn = Config.Bind(
                "4. Advanced",
                "Add a pmc spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to add a pmc spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            AddScavSpawn = Config.Bind(
                "4. Advanced",
                "Add a scav spawn",
                new KeyboardShortcut(),
                new ConfigDescription(
                    "Hotkey to add a scav spawn",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            enablePointOverlay = Config.Bind(
                "4. Advanced",
                "Spawnpoint overlay On/Off",
                false,
                new ConfigDescription(
                    "Dev value - Turn on and off pointOverlay dev tool (Requires restart)",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true }
                )
            );

            currentPreset.SettingChanged += OnPresetChange;

            spawnSmoothing.SettingChanged += OnStartingPmcsChanged;
            randomSpawns.SettingChanged += OnStartingPmcsChanged;
            startingPmcs.SettingChanged += OnStartingPmcsChanged;

            if (IsFika)
            {
                UpdateServerStoredValues();
                UpdateValuesFromServerStoredValues();
            }
            else
            {
                if (currentPreset.Value != null)
                {
                    var current = Array
                        .Find(PresetList, (item) => item.Name == currentPreset.Value)
                        .Label;
                    var result = Routers.SetPreset(current);
                }
                if (!CustomUnchanged())
                {
                    currentPreset.Value = "Custom";
                }
                OverwriteServerStoredValuesAndSubmit();
            }
        }

        private static void OverwriteServerStoredValuesAndSubmit()
        {
            if (currentPreset.Value == "Custom")
            {
                ServerStoredValues.scavDifficulty = Math.Round(scavDifficulty.Value, 2);
                ServerStoredValues.pmcDifficulty = Math.Round(pmcDifficulty.Value, 2);
                ServerStoredValues.startingPmcs = startingPmcs.Value;
                ServerStoredValues.spawnSmoothing = spawnSmoothing.Value;

                ServerStoredValues.randomSpawns = randomSpawns.Value;
                ServerStoredValues.debug = debug.Value;

                ServerStoredValues.zombiesEnabled = zombiesEnabled.Value;
                ServerStoredValues.zombieHealth = Math.Round(zombieHealth.Value, 1);
                ServerStoredValues.zombieWaveQuantity = Math.Round(zombieWaveQuantity.Value, 1);
                ServerStoredValues.zombieWaveDistribution = Math.Round(
                    zombieWaveDistribution.Value,
                    1
                );
                ServerStoredValues.scavWaveDistribution = Math.Round(scavWaveDistribution.Value, 1);

                ServerStoredValues.pmcWaveDistribution = Math.Round(pmcWaveDistribution.Value, 1);
                ServerStoredValues.pmcWaveQuantity = Math.Round(pmcWaveQuantity.Value, 1);
                ServerStoredValues.scavWaveQuantity = Math.Round(scavWaveQuantity.Value, 1);

                ServerStoredValues.maxBotCap = maxBotCap.Value;
                ServerStoredValues.maxBotPerZone = maxBotPerZone.Value;
                ServerStoredValues.scavGroupChance = scavGroupChance.Value;
                ServerStoredValues.pmcGroupChance = pmcGroupChance.Value;
                ServerStoredValues.sniperGroupChance = sniperGroupChance.Value;
                ServerStoredValues.pmcMaxGroupSize = pmcMaxGroupSize.Value;
                ServerStoredValues.scavMaxGroupSize = scavMaxGroupSize.Value;
                ServerStoredValues.sniperMaxGroupSize = sniperMaxGroupSize.Value;
                ServerStoredValues.bossOpenZones = bossOpenZones.Value;
                ServerStoredValues.randomRaiderGroup = randomRaiderGroup.Value;
                ServerStoredValues.randomRaiderGroupChance = randomRaiderGroupChance.Value;
                ServerStoredValues.randomRogueGroup = randomRogueGroup.Value;
                ServerStoredValues.randomRogueGroupChance = randomRogueGroupChance.Value;
                ServerStoredValues.disableBosses = disableBosses.Value;
                ServerStoredValues.mainBossChanceBuff = mainBossChanceBuff.Value;
                ServerStoredValues.bossInvasion = bossInvasion.Value;
                ServerStoredValues.bossInvasionSpawnChance = bossInvasionSpawnChance.Value;
                ServerStoredValues.gradualBossInvasion = gradualBossInvasion.Value;
            }
            else
            {
                UpdateValuesFromDefaults(false);
                ServerStoredValues.debug = debug.Value;
                ServerStoredValues.scavDifficulty = Math.Round(scavDifficulty.Value, 2);
                ServerStoredValues.pmcDifficulty = Math.Round(pmcDifficulty.Value, 2);
                ServerStoredValues.startingPmcs = startingPmcs.Value;
                ServerStoredValues.spawnSmoothing = spawnSmoothing.Value;
                ServerStoredValues.randomSpawns = randomSpawns.Value;
            }

            Routers.SetOverrideConfig(ServerStoredValues);

            Methods.RefreshLocationInfo();
        }

        private static bool CustomUnchanged()
        {
            return zombieHealth.Value == ServerStoredValues.zombieHealth
                && zombiesEnabled.Value == ServerStoredValues.zombiesEnabled
                && zombieWaveDistribution.Value == ServerStoredValues.zombieWaveDistribution
                && zombieWaveQuantity.Value == ServerStoredValues.zombieWaveQuantity
                && scavWaveDistribution.Value == ServerStoredValues.scavWaveDistribution
                && pmcWaveDistribution.Value == ServerStoredValues.pmcWaveDistribution
                && scavWaveQuantity.Value == ServerStoredValues.scavWaveQuantity
                && pmcWaveQuantity.Value == ServerStoredValues.pmcWaveQuantity
                && maxBotCap.Value == ServerStoredValues.maxBotCap
                && maxBotPerZone.Value == ServerStoredValues.maxBotPerZone
                && scavGroupChance.Value == ServerStoredValues.scavGroupChance
                && pmcGroupChance.Value == ServerStoredValues.pmcGroupChance
                && sniperGroupChance.Value == ServerStoredValues.sniperGroupChance
                && pmcMaxGroupSize.Value == ServerStoredValues.pmcMaxGroupSize
                && scavMaxGroupSize.Value == ServerStoredValues.scavMaxGroupSize
                && sniperMaxGroupSize.Value == ServerStoredValues.sniperMaxGroupSize
                && bossOpenZones.Value == ServerStoredValues.bossOpenZones
                && randomRaiderGroup.Value == ServerStoredValues.randomRaiderGroup
                && randomRaiderGroupChance.Value == ServerStoredValues.randomRaiderGroupChance
                && randomRogueGroup.Value == ServerStoredValues.randomRogueGroup
                && randomRogueGroupChance.Value == ServerStoredValues.randomRogueGroupChance
                && disableBosses.Value == ServerStoredValues.disableBosses
                && mainBossChanceBuff.Value == ServerStoredValues.mainBossChanceBuff
                && bossInvasion.Value == ServerStoredValues.bossInvasion
                && bossInvasionSpawnChance.Value == ServerStoredValues.bossInvasionSpawnChance
                && gradualBossInvasion.Value == ServerStoredValues.gradualBossInvasion;
        }

        private static void UpdateValuesFromDefaults(bool updateDifficulty = false)
        {
            if (updateDifficulty)
            {
                scavDifficulty.Value = ServerStoredDefaults.scavDifficulty;
                pmcDifficulty.Value = ServerStoredDefaults.pmcDifficulty;
                startingPmcs.Value = ServerStoredDefaults.startingPmcs;
                spawnSmoothing.Value = ServerStoredDefaults.spawnSmoothing;
                randomSpawns.Value = ServerStoredDefaults.randomSpawns;
                debug.Value = ServerStoredDefaults.debug;
            }

            zombieHealth.Value = ServerStoredDefaults.zombieHealth;
            zombiesEnabled.Value = ServerStoredDefaults.zombiesEnabled;
            zombieWaveDistribution.Value = ServerStoredDefaults.zombieWaveDistribution;
            zombieWaveQuantity.Value = ServerStoredDefaults.zombieWaveQuantity;

            scavWaveDistribution.Value = ServerStoredDefaults.scavWaveDistribution;
            pmcWaveDistribution.Value = ServerStoredDefaults.pmcWaveDistribution;
            scavWaveQuantity.Value = ServerStoredDefaults.scavWaveQuantity;
            pmcWaveQuantity.Value = ServerStoredDefaults.pmcWaveQuantity;
            maxBotCap.Value = ServerStoredDefaults.maxBotCap;
            maxBotPerZone.Value = ServerStoredDefaults.maxBotPerZone;
            scavGroupChance.Value = ServerStoredDefaults.scavGroupChance;
            pmcGroupChance.Value = ServerStoredDefaults.pmcGroupChance;
            sniperGroupChance.Value = ServerStoredDefaults.sniperGroupChance;
            pmcMaxGroupSize.Value = ServerStoredDefaults.pmcMaxGroupSize;
            scavMaxGroupSize.Value = ServerStoredDefaults.scavMaxGroupSize;
            sniperMaxGroupSize.Value = ServerStoredDefaults.sniperMaxGroupSize;
            bossOpenZones.Value = ServerStoredDefaults.bossOpenZones;
            randomRaiderGroup.Value = ServerStoredDefaults.randomRaiderGroup;
            randomRaiderGroupChance.Value = ServerStoredDefaults.randomRaiderGroupChance;
            randomRogueGroup.Value = ServerStoredDefaults.randomRogueGroup;
            randomRogueGroupChance.Value = ServerStoredDefaults.randomRogueGroupChance;
            disableBosses.Value = ServerStoredDefaults.disableBosses;
            mainBossChanceBuff.Value = ServerStoredDefaults.mainBossChanceBuff;
            bossInvasion.Value = ServerStoredDefaults.bossInvasion;
            bossInvasionSpawnChance.Value = ServerStoredDefaults.bossInvasionSpawnChance;
            gradualBossInvasion.Value = ServerStoredDefaults.gradualBossInvasion;
            ServerStoredValues = Routers.GetDefaultConfig();
        }

        private static void UpdateValuesFromServerStoredValues()
        {
            currentPreset.Value = Routers.GetCurrentPresetName();
            scavDifficulty.Value = ServerStoredValues.scavDifficulty;
            pmcDifficulty.Value = ServerStoredValues.pmcDifficulty;
            startingPmcs.Value = ServerStoredValues.startingPmcs;
            spawnSmoothing.Value = ServerStoredValues.spawnSmoothing;

            randomSpawns.Value = ServerStoredValues.randomSpawns;
            debug.Value = ServerStoredValues.debug;

            zombieHealth.Value = ServerStoredValues.zombieHealth;
            zombiesEnabled.Value = ServerStoredValues.zombiesEnabled;
            zombieWaveDistribution.Value = ServerStoredValues.zombieWaveDistribution;
            zombieWaveQuantity.Value = ServerStoredValues.zombieWaveQuantity;

            scavWaveDistribution.Value = ServerStoredValues.scavWaveDistribution;
            pmcWaveDistribution.Value = ServerStoredValues.pmcWaveDistribution;
            scavWaveQuantity.Value = ServerStoredValues.scavWaveQuantity;
            pmcWaveQuantity.Value = ServerStoredValues.pmcWaveQuantity;
            maxBotCap.Value = ServerStoredValues.maxBotCap;
            maxBotPerZone.Value = ServerStoredValues.maxBotPerZone;
            scavGroupChance.Value = ServerStoredValues.scavGroupChance;
            pmcGroupChance.Value = ServerStoredValues.pmcGroupChance;
            sniperGroupChance.Value = ServerStoredValues.sniperGroupChance;
            pmcMaxGroupSize.Value = ServerStoredValues.pmcMaxGroupSize;
            scavMaxGroupSize.Value = ServerStoredValues.scavMaxGroupSize;
            sniperMaxGroupSize.Value = ServerStoredValues.sniperMaxGroupSize;
            bossOpenZones.Value = ServerStoredValues.bossOpenZones;
            randomRaiderGroup.Value = ServerStoredValues.randomRaiderGroup;
            randomRaiderGroupChance.Value = ServerStoredValues.randomRaiderGroupChance;
            randomRogueGroup.Value = ServerStoredValues.randomRogueGroup;
            randomRogueGroupChance.Value = ServerStoredValues.randomRogueGroupChance;
            disableBosses.Value = ServerStoredValues.disableBosses;
            mainBossChanceBuff.Value = ServerStoredValues.mainBossChanceBuff;
            bossInvasion.Value = ServerStoredValues.bossInvasion;
            bossInvasionSpawnChance.Value = ServerStoredValues.bossInvasionSpawnChance;
            gradualBossInvasion.Value = ServerStoredValues.gradualBossInvasion;
        }

        private static void OnStartingPmcsChanged(object sender, EventArgs e)
        {
            if (startingPmcs?.Value == true)
            {
                randomSpawns.Value = true;
                spawnSmoothing.Value = false;
            }
        }

        private static void OnPresetChange(object sender, EventArgs e)
        {
            var current = Array.Find(PresetList, (item) => item.Name == currentPreset.Value).Label;
            var result = Routers.SetPreset(current);
            Methods.DisplayMessage(result);
            if (current != "custom")
            {
                OverwriteServerStoredValuesAndSubmit();
                UpdateServerStoredValues();
            }
        }

        private static void UpdateServerStoredValues()
        {
            ServerStoredValues = Routers.GetServerConfigWithOverrides();
        }

        private static void CreateSimpleButton(
            string configSection,
            string configEntryName,
            string buttonName,
            string description,
            Func<string> doThings,
            int? order
        )
        {
            Action<ConfigEntryBase> drawer = (ConfigEntryBase entry) =>
            {
                if (GUILayout.Button(buttonName, GUILayout.ExpandWidth(true)))
                {
                    doThings();
                }
            };

            ConfigDescription configDescription =
                new(
                    description,
                    null,
                    new ConfigurationManagerAttributes { Order = order, CustomDrawer = drawer }
                );

            _config.Bind(configSection, configEntryName, "", configDescription);
        }
    }
}
