using BepInEx.Configuration;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Opções do F12 (arquivo com.trl.pvpmode.cfg).
    /// ref: backlog/001-morte-desligada-timer/001-morte-desligada-timer-02-spec-tech.md §3
    /// </summary>
    internal static class Settings
    {
        private const string SECTION_LIVES = "Lives";

        public static ConfigEntry<bool> ENABLED;
        public static ConfigEntry<int> LIVES_PER_RAID;
        public static ConfigEntry<float> DOWNED_TIMEOUT;
        public static ConfigEntry<bool> HEADSHOT_KILLS;

        public static void Init(ConfigFile config)
        {
            ENABLED = config.Bind(
                SECTION_LIVES,
                "Enable Lives Mode",
                true,
                new ConfigDescription(
                    "Liga o modo de vidas por raid. Desligado, volta a valer o resgate padrao do Fika " +
                    "(companheiro pode te levantar e o tempo vem do servidor)."));

            LIVES_PER_RAID = config.Bind(
                SECTION_LIVES,
                "Lives Per Raid",
                1,
                new ConfigDescription(
                    "Quantas vezes voce pode renascer por partida. -1 = ilimitado. 0 = nenhuma (morre de primeira).",
                    new AcceptableValueRange<int>(-1, 10)));

            DOWNED_TIMEOUT = config.Bind(
                SECTION_LIVES,
                "Downed Timeout (s)",
                60f,
                new ConfigDescription(
                    "Tempo para decidir renascer, em segundos. Ao zerar, a morte e definitiva. " +
                    "0 = sem limite: voce fica caido ate decidir. O valor e lido no instante da queda — " +
                    "mudar durante a partida so vale na proxima.",
                    new AcceptableValueRange<float>(0f, 600f)));

            HEADSHOT_KILLS = config.Bind(
                SECTION_LIVES,
                "Headshot Kills Instantly",
                false,
                new ConfigDescription(
                    "Tiro na cabeca encerra a partida na hora, ignorando as vidas restantes."));
        }
    }
}
