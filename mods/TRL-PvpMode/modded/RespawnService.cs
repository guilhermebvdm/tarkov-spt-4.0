using System;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Game.Spawning;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Players;
using UnityEngine;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Traz o jogador caído de volta ao jogo em outro ponto do mapa.
    ///
    /// A ordem dos passos importa e está justificada na spec técnica do item 002 §1:
    ///   consumir vida → teleportar → religar o boneco → curar → proteger
    /// </summary>
    internal static class RespawnService
    {
        private const int SPAWN_PICK_ATTEMPTS = 5;
        private const float SAME_SPOT_TOLERANCE_SQR = 25f;  // 5 m

        private static float _protectionLeft;
        private static bool _protectionActive;

        public static void Reset()
        {
            _protectionLeft = 0f;
            _protectionActive = false;
        }

        /// <summary>
        /// Executa o renascimento. Devolve falso (sem efeito colateral) quando o jogador não está
        /// caído, não há vida, ou o sistema de spawn não está disponível.
        /// </summary>
        public static bool TryRespawn(Player player)
        {
            try
            {
                if (player is not FikaPlayer fikaPlayer) return false;
                if (!RaidState.IsActive) return false;

                // Guarda contra disparo duplo e contra a corrida com o tempo esgotando: se o
                // jogador já não está caído, não há o que renascer.
                if (player.ActiveHealthController is not Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true })
                    return false;

                if (!TryGetSpawnPosition(player, out var position))
                {
                    Notify("TRL-PvpMode: nao foi possivel achar um ponto de renascimento.", Color.red);
                    return false;
                }

                if (!RaidState.TryConsumeLife())
                {
                    Notify("Sem vidas restantes.", Color.red);
                    return false;
                }

                // 1. Teleportar ANTES de religar: durante o caído o boneco está desligado nos
                //    outros clientes, e religar primeiro o faria reaparecer na posição antiga.
                player.Teleport(position);

                // 2. Religar. Envia o DownedSyncPacket avisando todo mundo que levantamos.
                fikaPlayer.ToggleDowned(false);

                // 3. Curar. Só agora, porque ToggleDowned(false) é quem devolve IsAlive = true.
                player.ActiveHealthController.RestoreFullHealth();

                // 4. Proteger. Por último: ToggleDowned(false) restaura o coeficiente de dano
                //    para 1 e sobrescreveria uma proteção aplicada antes.
                StartProtection(player);

                // 5. Avisar os outros clientes para cravarem a posição nova em vez de interpolar
                //    o trajeto inteiro (item 003).
                Networking.RespawnNetwork.BroadcastRespawn(fikaPlayer, position);

                var left = RaidState.IsUnlimited ? "ilimitadas" : RaidState.LivesLeft.ToString();
                Notify($"Voce renasceu. Vidas restantes: {left}", Color.green);
                Plugin.Log.LogInfo($"[TRL-PvpMode] Respawn em {position} — vidas restantes: {left}");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] TryRespawn: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Sorteia um ponto de nascimento de jogador. Passar um identificador aleatório a cada
        /// tentativa faz o sistema de spawn tratar o pedido como de outro personagem e aplicar
        /// sozinho o afastamento de quem já está no mapa.
        /// </summary>
        private static bool TryGetSpawnPosition(Player player, out Vector3 position)
        {
            position = Vector3.zero;

            var game = Singleton<IFikaGame>.Instance;
            var spawnSystem = game?.GameController?.SpawnSystem;
            if (spawnSystem == null)
            {
                Plugin.Log.LogWarning("[TRL-PvpMode] SpawnSystem indisponivel — respawn abortado.");
                return false;
            }

            var deathPosition = player.Position;

            for (var attempt = 0; attempt < SPAWN_PICK_ATTEMPTS; attempt++)
            {
                var point = spawnSystem.SelectSpawnPoint(
                    ESpawnCategory.Player, player.Side, null, null, null, null, Guid.NewGuid().ToString());

                if (point == null) continue;

                position = point.Position;

                // Mapa pequeno pode não ter alternativa; aceitamos o repetido na última tentativa.
                if ((position - deathPosition).sqrMagnitude > SAME_SPOT_TOLERANCE_SQR)
                    return true;
            }

            // Se chegou aqui com um ponto qualquer, ele serve — melhor renascer perto do que não renascer.
            return position != Vector3.zero;
        }

        private static void StartProtection(Player player)
        {
            var seconds = Mathf.Max(0f, Settings.SPAWN_PROTECTION.Value);
            if (seconds <= 0f) return;

            player.ActiveHealthController.SetDamageCoeff(0f);
            _protectionLeft = seconds;
            _protectionActive = true;
        }

        /// <summary>Chamado a cada quadro pelo patch de input, já filtrado para o jogador local.</summary>
        public static void Tick(Player player, float deltaTime)
        {
            if (!_protectionActive) return;

            _protectionLeft -= deltaTime;
            if (_protectionLeft > 0f) return;

            _protectionActive = false;
            player.ActiveHealthController.SetDamageCoeff(1f);
            Notify("Protecao de renascimento encerrada.", Color.yellow);
        }

        private static void Notify(string message, Color color)
        {
            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    message, ENotificationDurationType.Default, ENotificationIconType.Default, color);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] Notify: {ex.Message}");
            }
        }
    }
}
