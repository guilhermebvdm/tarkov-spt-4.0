using BepInEx.Configuration;
using UnityEngine;

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
        public static ConfigEntry<KeyboardShortcut> RESPAWN_KEY;
        public static ConfigEntry<float> RESPAWN_HOLD_TIME;
        public static ConfigEntry<float> SPAWN_PROTECTION;
        public static ConfigEntry<bool> SHOW_LIVES_HUD;
        public static ConfigEntry<float> MIN_SPAWN_DISTANCE;

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

            // KeyboardShortcut e nao KeyCode: o seletor do ConfigurationManager para KeyCode
            // captura teclas fantasma (F13) em teclado de notebook. O de KeyboardShortcut usa o
            // caminho de captura do proprio BepInEx e aceita combinacao.
            //
            // Default Home, nao F5: F5 ja tem funcao no jogo (o teste in-game caiu na tela de sair
            // da raid). Home fica livre no EFT e existe como tecla fisica direta na maioria dos
            // teclados - Pause costuma exigir Fn e nem sempre chega ao Unity.
            RESPAWN_KEY = config.Bind(
                SECTION_LIVES,
                "Respawn Key",
                new KeyboardShortcut(KeyCode.Home),
                new ConfigDescription(
                    "Tecla para renascer. Segure-a enquanto estiver caido."));

            RESPAWN_HOLD_TIME = config.Bind(
                SECTION_LIVES,
                "Respawn Hold Time (s)",
                2f,
                new ConfigDescription(
                    "Por quanto tempo a tecla precisa ficar pressionada. Soltar antes cancela sem gastar vida.",
                    new AcceptableValueRange<float>(0.1f, 10f)));

            SPAWN_PROTECTION = config.Bind(
                SECTION_LIVES,
                "Spawn Protection (s)",
                5f,
                new ConfigDescription(
                    "Tempo sem receber dano depois de renascer. 0 = sem protecao.",
                    new AcceptableValueRange<float>(0f, 30f)));

            MIN_SPAWN_DISTANCE = config.Bind(
                SECTION_LIVES,
                "Min Spawn Distance (m)",
                80f,
                new ConfigDescription(
                    "Distancia minima de qualquer jogador ou bot vivo ao sortear onde renascer. " +
                    "Se nenhum ponto atender, o filtro e relaxado em vez de impedir o renascimento.",
                    new AcceptableValueRange<float>(0f, 500f)));

            SHOW_LIVES_HUD = config.Bind(
                SECTION_LIVES,
                "Show Lives Counter",
                true,
                new ConfigDescription(
                    "Mostra as vidas restantes na tela. Fica destacado enquanto voce estiver caido."));

            HEADSHOT_KILLS = config.Bind(
                SECTION_LIVES,
                "Headshot Kills Instantly",
                false,
                new ConfigDescription(
                    "Tiro na cabeca encerra a partida na hora, ignorando as vidas restantes."));
        }
    }
}
