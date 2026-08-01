using BepInEx.Configuration;
using UnityEngine;

namespace PlayerLives.Helpers
{
    internal class Settings
    {
        public static ConfigEntry<float> REVIVAL_DURATION;
        public static ConfigEntry<KeyCode> REVIVAL_KEY;
        public static ConfigEntry<KeyCode> GIVE_UP_KEY;
        public static ConfigEntry<bool> RESTORE_DESTROYED_BODY_PARTS;
        public static ConfigEntry<bool> TESTING;
        public static ConfigEntry<bool> REQUIRE_HEAD_HEALTH;
        public static ConfigEntry<int> PLAYER_LIVES;
        public static ConfigEntry<int> RESTORE_DESTROYED_BODY_PARTS_HEALING;
        public static ConfigEntry<string> REQUIRE_BUFF_TYPE;
        public static ConfigEntry<string> REQUIRE_STIM;

        public static void Init(ConfigFile config)
        {
            PLAYER_LIVES = config.Bind(
                "General",
                "Player Lives",
                1,
               "How many revives per raid."
            );
            REVIVAL_DURATION = config.Bind(
                "General",
                "Invulnerability Duration (s)",
                10f,
               "How long you are invulnerable for after revive."
            );
            REVIVAL_KEY = config.Bind(
                "General",
                "Revival Key",
                KeyCode.F5
            );
            GIVE_UP_KEY = config.Bind(
                 "General",
                 "Give Up Key",
                 KeyCode.F9
             );


            REQUIRE_BUFF_TYPE = config.Bind(
                "Revive Conditions",
                "Require Active Buff",
                "None",
                new ConfigDescription(
                    "Select required buff to be active for revive.",
                    new AcceptableValueList<string>("None", "BuffsAdrenaline", "BuffsPropital",
                    "BuffsSJ1TGLabs", "BuffsSJ6TGLabs", "BuffsZagustin", "BuffseTGchange",
                    "Buffs_2A2bTG", "Buffs_3bTG", "Buffs_AHF1M", "Buffs_Antidote",
                    "Buffs_L1", "Buffs_MULE", "Buffs_Meldonin", "Buffs_Obdolbos", "Buffs_Obdolbos2",
                    "Buffs_P22", "Buffs_PNB", "Buffs_Perfotoran", "Buffs_SJ12_TGLabs", "Buffs_Trimadol")
                )
            );

            REQUIRE_STIM = config.Bind(
                "Revive Conditions",
                "Require Stim",
                "None",
                new ConfigDescription(
                    "Select stim that will be used for revive. 'Any' uses the cheapest stim you carry first.",
                    new AcceptableValueList<string>("None", "Any", "Adrenaline", "Propital",
                    "SJ1TGLabs", "SJ6TGLabs", "Zagustin", "eTGchange", "2A2bTG", "3bTG",
                    "AHF1M", "Antidote", "L1", "MULE", "Meldonin", "Obdolbos", "Obdolbos2",
                    "P22", "PNB", "Perfotoran", "SJ12_TGLabs", "Trimadol")
                )
            );

            REQUIRE_HEAD_HEALTH = config.Bind(
                "Revive Conditions",
                "Require Head Health > 0",
                false,
                "if your head health is 0, revives will no longer work."
            );


            RESTORE_DESTROYED_BODY_PARTS = config.Bind(
                "On Revive",
                "Restore destroyed body parts",
                true,
               "Blackened body parts are restored%"
            );

            RESTORE_DESTROYED_BODY_PARTS_HEALING = config.Bind(
                "On Revive",
                "Restore destroyed body parts healing",
                25,
                 new ConfigDescription(
                    "Healing amount %",
                    new AcceptableValueRange<int>(1, 100)
                )
            );

            TESTING = config.Bind(
                "Development",
                "Test Mode",
                false,
                new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true })
            );
        }
    }
}
