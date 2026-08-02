using System;
using BepInEx.Bootstrap;
using EFT;
using EFT.Communications;
using UnityEngine;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Estado do modo de vidas durante UMA raid. Tudo que vive aqui é zerado na primeira linha de
    /// <see cref="Begin"/>, então nada atravessa de uma partida para a seguinte (AP-01 / critério
    /// "estado entre raids").
    /// </summary>
    internal static class RaidState
    {
        private const string PLAYER_LIVES_GUID = "com.somtam.playerLives";

        private static bool _active;
        private static int _livesLeft;

        /// <summary>
        /// O modo está de fato governando esta raid. Falso quando desligado no F12, quando um
        /// pré-requisito falta, ou fora de partida.
        ///
        /// Todo patch que altera comportamento do Fika PRECISA consultar isto antes de tocar em
        /// qualquer resultado — senão o "modo desativado" não desativa, piora (code review 01, C-02).
        /// </summary>
        public static bool IsActive => _active;

        /// <summary>Vidas restantes. Ilimitado quando <see cref="IsUnlimited"/>.</summary>
        public static int LivesLeft => _livesLeft;

        public static bool IsUnlimited => Settings.LIVES_PER_RAID != null && Settings.LIVES_PER_RAID.Value < 0;

        /// <summary>
        /// Tipo do dano que está matando o jogador neste quadro. Gravado pelo prefixo de
        /// <c>Kill</c> e lido no mesmo quadro pelos portões — é a única fonte confiável para dano
        /// de desgaste (code review 01, C-01).
        /// </summary>
        public static EDamageType LastKillDamageType { get; set; } = EDamageType.Undefined;

        /// <summary>Há vida para gastar? Consultado no portão de morte: barato e nunca lança.</summary>
        public static bool HasLifeAvailable => _active && (IsUnlimited || _livesLeft > 0);

        /// <summary>Debita uma vida. Chamado quando o jogador escolhe renascer (item 002).</summary>
        public static bool TryConsumeLife()
        {
            if (!HasLifeAvailable) return false;
            if (!IsUnlimited) _livesLeft--;
            return true;
        }

        public static void Begin(GameWorld gameWorld)
        {
            // Reset INCONDICIONAL, antes de qualquer guarda: se um caminho de saída deixar de
            // chamar End(), o estado não pode atravessar para a raid seguinte (C-05).
            _active = false;
            _livesLeft = 0;
            LastKillDamageType = EDamageType.Undefined;

            // Menu e esconderijo não têm modo de vidas (R-08).
            if (gameWorld == null || gameWorld.MainPlayer == null) return;
            if (gameWorld.MainPlayer is HideoutPlayer) return;

            if (!Settings.ENABLED.Value)
            {
                Plugin.Log.LogInfo("[TRL-PvpMode] Modo de vidas desligado no F12 — resgate padrao do Fika em vigor.");
                return;
            }

            FikaBridge.Resolve();
            if (!FikaBridge.IsUsable) return;

            if (WarnIfConflictingModLoaded()) return;
            if (!WarnIfServerReviveDisabled(gameWorld.MainPlayer)) return;

            _livesLeft = Mathf.Max(0, Settings.LIVES_PER_RAID.Value);
            _active = true;

            ApplyDownedTimeout(gameWorld.MainPlayer);

            Plugin.Log.LogInfo(
                $"[TRL-PvpMode] Raid iniciada — vidas: {(IsUnlimited ? "ilimitadas" : _livesLeft.ToString())}, " +
                $"tempo para decidir: {(Settings.DOWNED_TIMEOUT.Value <= 0f ? "sem limite" : Settings.DOWNED_TIMEOUT.Value + "s")}");
        }

        /// <summary>Idempotente — pode ser chamado mais de uma vez por fim de raid.</summary>
        public static void End()
        {
            if (!_active && _livesLeft == 0) return;

            _active = false;
            _livesLeft = 0;
            LastKillDamageType = EDamageType.Undefined;
            Plugin.Log.LogInfo("[TRL-PvpMode] Raid encerrada — estado do modo de vidas limpo.");
        }

        /// <summary>
        /// Sobrescreve o prazo do Fika pelo valor do F12. Feito no início da raid porque
        /// Bleedout.Init fotografa o valor no instante da queda.
        /// </summary>
        private static void ApplyDownedTimeout(Player mainPlayer)
        {
            if (mainPlayer.ActiveHealthController is not Fika.Core.Main.ClientClasses.ClientHealthController chc)
                return;

            var seconds = Mathf.Max(0f, Settings.DOWNED_TIMEOUT.Value);
            if (FikaBridge.TrySetBleedoutTime(chc, seconds)) return;

            // Falha silenciosa aqui significaria jogar a raid inteira com o tempo do servidor
            // achando que o do F12 está valendo — precisa aparecer na tela (C-06).
            Notify($"TRL-PvpMode: nao foi possivel aplicar o tempo de decisao do F12. " +
                   $"Valendo o valor do servidor ({chc.BleedoutTime}s).", Color.yellow);
            Plugin.Log.LogWarning(
                $"[TRL-PvpMode] Tempo de decisao nao aplicado — servidor: {chc.BleedoutTime}s.");
        }

        /// <summary>
        /// O PlayerLives prefixa o mesmo ponto de morte e retorna false, o que impede todo o
        /// resto de rodar. Os dois não coexistem.
        /// </summary>
        private static bool WarnIfConflictingModLoaded()
        {
            if (!Chainloader.PluginInfos.ContainsKey(PLAYER_LIVES_GUID)) return false;

            Notify("TRL-PvpMode DESATIVADO: o mod PlayerLives esta instalado e disputa o mesmo ponto de morte. " +
                   "Remova um dos dois.", Color.red);
            Plugin.Log.LogError($"[TRL-PvpMode] Conflito com {PLAYER_LIVES_GUID} — modo de vidas desativado.");
            return true;
        }

        /// <summary>
        /// Sem reviveConfig.enabled no servidor, o Fika nem instala os patches em que este modo
        /// se apoia (ref: fika-plugin/Fika.Core/FikaConfig.cs:908).
        /// </summary>
        private static bool WarnIfServerReviveDisabled(Player mainPlayer)
        {
            var enabled = mainPlayer.ActiveHealthController is
                Fika.Core.Main.ClientClasses.ClientHealthController { ReviveEnabled: true };

            if (enabled) return true;

            Notify("TRL-PvpMode DESATIVADO: ligue \"reviveConfig.enabled\" no fika.jsonc do servidor.", Color.yellow);
            Plugin.Log.LogWarning("[TRL-PvpMode] reviveConfig.enabled=false no servidor — modo de vidas desativado.");
            return false;
        }

        private static void Notify(string message, Color color)
        {
            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    message, ENotificationDurationType.Long, ENotificationIconType.Alert, color);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] Notify: {ex.Message}");
            }
        }
    }
}
