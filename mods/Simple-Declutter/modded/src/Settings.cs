using BepInEx.Configuration;
using System;


namespace SimpleDeclutter
{
    internal class Settings
    {
        public static ConfigEntry<bool> declutterEnabledConfig;
        public static ConfigEntry<bool> declutterGarbageEnabledConfig;
        public static ConfigEntry<bool> declutterHeapsEnabledConfig;
        public static ConfigEntry<bool> declutterSpentCartridgesEnabledConfig;
        public static ConfigEntry<bool> declutterFakeFoodEnabledConfig;
        public static ConfigEntry<bool> declutterDecalsEnabledConfig;
        public static ConfigEntry<bool> declutterPuddlesEnabledConfig;
        public static ConfigEntry<bool> declutterShardsEnabledConfig;
        // public static ConfigEntry<bool> declutterUnscrutinizedEnabledConfig;
        public static ConfigEntry<float> declutterScaleOffsetConfig;
        public static ConfigEntry<bool> EnableFactory;
        public static ConfigEntry<bool> EnableLighthouse;
        public static ConfigEntry<bool> EnableShoreline;
        public static ConfigEntry<bool> EnableReserve;
        public static ConfigEntry<bool> EnableWoods;
        public static ConfigEntry<bool> EnableInterchange;
        public static ConfigEntry<bool> EnableCustoms;
        public static ConfigEntry<bool> EnableStreets;
        public static ConfigEntry<bool> EnableGroundZero;
        public static ConfigEntry<bool> EnableLab;
        public static ConfigEntry<bool> framesaverPotatoShadow;

        public static void Init(ConfigFile Config)
        {
            declutterEnabledConfig = Config.Bind("A - De-Clutter Enabler", "A - De-Clutterer Enabled", true, "Enables the De-Clutterer");
            declutterScaleOffsetConfig = Config.Bind<float>("A - De-Clutter Enabler", "B - De-Clutterer Scaler", 1f, new BepInEx.Configuration.ConfigDescription("Larger Scale = Larger the Clutter Removed.", new BepInEx.Configuration.AcceptableValueRange<float>(0.5f, 2f)));

            declutterGarbageEnabledConfig = Config.Bind("B - De-Clutter Settings", "Garbage & Litter De-Clutter", true, "De-Clutters things labeled 'garbage' or similar. Smaller garbage piles.");
            declutterHeapsEnabledConfig = Config.Bind("B - De-Clutter Settings", "Heaps & Piles De-Clutter", true, "De-Clutters things labeled 'heaps' or similar. Larger garbage piles.");
            declutterSpentCartridgesEnabledConfig = Config.Bind("B - De-Clutter Settings", "Spent Cartridges De-Clutter", true, "De-Clutters pre-generated spent ammunition on floor.");
            declutterFakeFoodEnabledConfig = Config.Bind("B - De-Clutter Settings", "Fake Food De-Clutter", true, "De-Clutters fake 'food' items.");
            declutterDecalsEnabledConfig = Config.Bind("B - De-Clutter Settings", "Decal De-Clutter", true, "De-Clutters decals (Blood, grafiti, etc.)");
            declutterPuddlesEnabledConfig = Config.Bind("B - De-Clutter Settings", "Puddle De-Clutter", true, "De-Clutters fake reflective puddles.");
            declutterShardsEnabledConfig = Config.Bind("B - De-Clutter Settings", "Glass & Tile Shards", true, "De-Clutters things labeled 'shards' or similar. The things you can step on that make noise.");
            // declutterUnscrutinizedEnabledConfig = Config.Bind("B - De-Clutter Settings", "H - Experimental Unscrutinized Disabler", false, "De-Clutters literally everything that doesn't have a collider, doesn't chare what the name is or the group is so above enablers will have no effect. It'll disable it all. Experimental, testing however has had positive results. Massively improves FPS.");

            framesaverPotatoShadow = Config.Bind("C - Framesavers", "A - Disable shadows", false, "Remove all shadows");

            EnableFactory = Config.Bind("D - Map", "Factory", true, "Enable plugin on the map.");
            EnableLighthouse = Config.Bind("D - Map", "Lighthouse", true, "Enable plugin on the map.");
            EnableShoreline = Config.Bind("D - Map", "Shoreline", true, "Enable plugin on the map.");
            EnableReserve = Config.Bind("D - Map", "Reserve", true, "Enable plugin on the map.");
            EnableWoods = Config.Bind("D - Map", "Woods", true, "Enable plugin on the map.");
            EnableInterchange = Config.Bind("D - Map", "Interchange", true, "Enable plugin on the map.");
            EnableCustoms = Config.Bind("D - Map", "Customs", true, "Enable plugin on the map.");
            EnableStreets = Config.Bind("D - Map", "Streets", true, "Enable plugin on the map.");
            EnableGroundZero = Config.Bind("D - Map", "Ground Zero", true, "Enable plugin on the map.");
            EnableLab = Config.Bind("D - Map", "Lab", false, "Enable plugin on the map.");

        }

    }
}