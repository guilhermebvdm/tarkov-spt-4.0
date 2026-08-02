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
        private static bool _isUnlimited;

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

        /// <summary>
        /// Fotografado no inicio da raid, junto com o total de vidas. Ler o F12 ao vivo faria
        /// metade da mesma opcao mudar no meio da partida - trocar para ilimitado concederia vidas
        /// na hora, e o caminho inverso zeraria o contador (F-03).
        /// </summary>
        public static bool IsUnlimited => _isUnlimited;

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
            _isUnlimited = false;
            LastKillDamageType = EDamageType.Undefined;
            RespawnService.Reset();
            Patches.RespawnInputPatch.Reset();
            Networking.RespawnNetwork.Reset();
            LivesHud.Reset();

            // Menu e esconderijo não têm modo de vidas (R-08).
            if (gameWorld == null || gameWorld.MainPlayer == null)
            {
                // O anfitriao sem tela nao tem jogador local, entao o modo fica inerte nele - o
                // que esta certo. Registrar para o operador distinguir "carregado e inerte de
                // proposito" de "mod ausente", que e a causa do problema de rede acima (G-09).
                Plugin.Log.LogInfo("[TRL-PvpMode] Sem jogador local nesta instancia — modo de vidas inerte.");
                return;
            }
            if (gameWorld.MainPlayer is HideoutPlayer) return;

            if (!Settings.ENABLED.Value)
            {
                Plugin.Log.LogInfo("[TRL-PvpMode] Modo de vidas desligado no F12 — resgate padrao do Fika em vigor.");
                return;
            }

            if (!FikaBridge.IsUsable) return;

            WarnIfHitboxFixMissing();

            if (WarnIfConflictingModLoaded()) return;
            if (!WarnIfServerReviveDisabled(gameWorld.MainPlayer)) return;

            _isUnlimited = Settings.LIVES_PER_RAID.Value < 0;
            _livesLeft = _isUnlimited ? 0 : Settings.LIVES_PER_RAID.Value;
            _active = true;

            ApplyDownedTimeout(gameWorld.MainPlayer);

            Plugin.Log.LogInfo(
                $"[TRL-PvpMode] Raid iniciada — vidas: {(IsUnlimited ? "ilimitadas" : _livesLeft.ToString())}, " +
                $"tempo para decidir: {(Settings.DOWNED_TIMEOUT.Value <= 0f ? "sem limite" : Settings.DOWNED_TIMEOUT.Value + "s")}");
        }

        /// <summary>Idempotente — pode ser chamado mais de uma vez por fim de raid.</summary>
        public static void End()
        {
            // Sem atalho de saida: o estado de rede e populado mesmo com o modo inativo (o
            // callback do pacote so checa se ha partida), entao um return antecipado deixaria os
            // guards de posicao sujos entre raids (F-05). As limpezas sao idempotentes.
            _active = false;
            _livesLeft = 0;
            _isUnlimited = false;
            LastKillDamageType = EDamageType.Undefined;
            RespawnService.Reset();
            Patches.RespawnInputPatch.Reset();
            Networking.RespawnNetwork.Reset();
            LivesHud.Reset();
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

            // O teto de revives do Fika é o segundo termo de CanBeDowned, que não patchamos.
            // Zerá-lo (0 = ilimitado para o Fika) deixa a contagem inteiramente com o nosso
            // contador de vidas — senão ele corta o modo pela metade em silêncio (D-02).
            if (!FikaBridge.TryUncapRevives(chc))
            {
                Notify("TRL-PvpMode: nao foi possivel liberar o limite de revives do Fika. " +
                       "Se \"maxRevives\" estiver definido no servidor, ele vai limitar suas vidas.", Color.yellow);
            }

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
        /// Sem o TRL-Fixes, o caminho de saida do estado caido devolve os colisores do corpo na
        /// camada errada e o jogador que renasceu fica impossivel de acertar para os outros pelo
        /// resto da partida. O mod converte um bug ocasional do Fika no caminho unico e garantido,
        /// entao a ausencia precisa gritar (G-05).
        /// </summary>
        private static void WarnIfHitboxFixMissing()
        {
            if (Chainloader.PluginInfos.ContainsKey(Plugin.TrlFixesGuid)) return;

            Notify("TRL-PvpMode: instale o TRL-Fixes. Sem ele, quem renascer fica impossivel de " +
                   "acertar para os outros jogadores.", Color.yellow);
            Plugin.Log.LogWarning("[TRL-PvpMode] com.trl.fixes ausente — hitbox pos-respawn ficara quebrada.");
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
