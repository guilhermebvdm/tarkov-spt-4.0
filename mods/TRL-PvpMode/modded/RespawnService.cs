using System;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Game.Spawning;
using Fika.Core.Main.Players;
using UnityEngine;

namespace TarkovRedLine.PvpMode
{
    /// <summary>
    /// Traz o jogador caído de volta ao jogo em outro ponto do mapa.
    ///
    /// A ordem dos passos importa e está justificada na spec técnica do item 002 §1:
    ///   avisar a rede → teleportar → religar → debitar vida → levantar → curar → proteger
    /// </summary>
    internal static class RespawnService
    {
        private const float SAME_SPOT_TOLERANCE_SQR = 25f;  // 5 m

        private static readonly System.Collections.Generic.List<Vector3> _candidateBuffer = new();

        private static float _protectionLeft;
        private static bool _protectionActive;
        private static Player _protectedPlayer;

        public static void Reset()
        {
            // Devolver o coeficiente antes de largar o estado: senão um fim de raid no meio da
            // proteção deixaria o jogador invulnerável se o objeto sobrevivesse (D-09).
            if (_protectionActive && _protectedPlayer?.ActiveHealthController != null)
            {
                try { _protectedPlayer.ActiveHealthController.SetDamageCoeff(1f); }
                catch (Exception ex) { Plugin.Log.LogError($"[TRL-PvpMode] Reset protection: {ex.Message}"); }
            }

            _protectionLeft = 0f;
            _protectionActive = false;
            _protectedPlayer = null;
            _candidateBuffer.Clear();
        }

        /// <summary>
        /// Executa o renascimento. Devolve falso (sem efeito colateral) quando o jogador não está
        /// caído, não há vida, ou o sistema de spawn não está disponível.
        /// </summary>
        public static bool TryRespawn(Player player)
        {
            try
            {
                if (player is not FikaPlayer fikaPlayer) return Fail("jogador nao e do tipo esperado");
                if (!RaidState.IsActive) return Fail("modo de vidas inativo nesta raid");

                if (player.ActiveHealthController is not Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true } chc)
                    return Fail("voce nao esta mais no estado caido");

                // O Fika NAO limpa Downed quando o prazo acaba: BleedOut() marca _bledOut, revive
                // o suficiente para chamar Kill() e pronto. Por 1-2 quadros o jogador segue com
                // Downed == true, ja com cadaver criado - completar a tecla nessa janela renasceria
                // em cima do proprio cadaver (code review 002, D-03).
                //
                // NAO usar IsAlive como discriminante: ele e FALSE durante todo o estado de caido
                // (o prefixo de Kill do Fika o zera ao entrar), entao testa-lo bloquearia todo
                // renascimento legitimo e deixaria passar so a janela proibida - polaridade
                // exatamente invertida (review completo, F-01).
                if (FikaBridge.HasBledOut(chc)) return Fail("o tempo ja acabou");

                if (!TryGetSpawnPosition(player, out var position))
                    return Fail("nenhum ponto de renascimento disponivel neste mapa");

                // 1. Avisar os outros clientes ANTES de teleportar. O estado de posição do FIKA
                //    trafega em canal não-confiável e o aviso em canal confiável — não há ordem
                //    garantida entre canais diferentes. Avisando primeiro, o receptor já está
                //    defendendo a posição nova quando o primeiro estado pós-teleporte chegar
                //    (code review 003, E-01).
                Networking.RespawnNetwork.BroadcastRespawn(fikaPlayer, position);

                // 2. Teleportar. Ainda não gastamos vida: se algo estourar daqui para trás, o
                //    jogador não perde nada.
                player.Teleport(position);

                // 3. Religar. Envia o DownedSyncPacket avisando todo mundo que levantamos.
                //    Este é o ponto de não-retorno.
                fikaPlayer.ToggleDowned(false);

                // 4. Debitar a vida SÓ depois do passo que não tem volta. Cobrar antes deixaria o
                //    jogador sem vida e ainda caído se qualquer passo acima estourasse (D-06).
                RaidState.TryConsumeLife();

                // 5. Levantar de fato: ToggleDowned(false) restaura dano, arma, eixos e metabolismo,
                //    mas NÃO desfaz a pose deitada que ele mesmo aplicou ao cair (D-04).
                if (player.MovementContext != null)
                {
                    player.MovementContext.IsInPronePose = false;
                    player.MovementContext.SetPoseLevel(1f);
                }

                // 6. Curar. Só agora, porque ToggleDowned(false) é quem devolve IsAlive = true.
                //    RestoreFullHealth restaura membros e vida, mas remove apenas sangramento —
                //    fratura, dor e intoxicação sobreviveriam ao renascimento (D-05).
                player.ActiveHealthController.RestoreFullHealth();
                player.ActiveHealthController.RemoveNegativeEffects(EBodyPart.Common);

                // 7. Proteger. Por ultimo: ToggleDowned(false) restaura o coeficiente de dano
                //    para 1 e sobrescreveria uma proteção aplicada antes.
                StartProtection(player);

                var left = RaidState.IsUnlimited ? "ilimitadas" : RaidState.LivesLeft.ToString();
                Notify($"Voce renasceu. Vidas restantes: {left}", Color.green);
                Plugin.Log.LogInfo($"[TRL-PvpMode] Respawn em {position} — vidas restantes: {left}");
                return true;
            }
            catch (Exception ex)
            {
                Fail($"erro interno: {ex.Message}");
                Plugin.Log.LogError($"[TRL-PvpMode] TryRespawn: {ex}");

                // O anuncio ja saiu (e o passo 1). Sem cancelar, os outros clientes ficariam com o
                // corpo cravado num ponto onde o jogador nunca chegou (G-06).
                try { Networking.RespawnNetwork.BroadcastCorrection(player as FikaPlayer); }
                catch { /* melhor esforco */ }

                return false;
            }
        }

        /// <summary>
        /// Toda saida sem renascimento passa por aqui. Falhar em silencio e indistinguivel, para
        /// quem esta jogando, de "a barra travou e voltou ao zero" - foi exatamente essa a leitura
        /// no segundo teste in-game.
        /// </summary>
        private static bool Fail(string reason)
        {
            NotifyThrottled($"Nao deu para renascer: {reason}.", Color.red);
            Plugin.Log.LogWarning($"[TRL-PvpMode] Respawn recusado — {reason}.");
            return false;
        }

        /// <summary>
        /// Sorteia de verdade um ponto de nascimento de jogador.
        ///
        /// ⚠️ NÃO usamos <c>ISpawnSystem.SelectSpawnPoint</c>: com os argumentos disponíveis aqui
        /// (sem grupo, sem time) ele cai em "o mais distante de todos os jogadores", que é
        /// **determinístico** — chamá-lo cinco vezes devolve cinco vezes o mesmo ponto, e o jogador
        /// renasceria sempre no mesmo canto do mapa a cada morte. O identificador aleatório que a
        /// versão anterior passava só influencia o desvio de transição, nada de afastamento
        /// (code review 002, D-01).
        ///
        /// Aqui montamos a lista de candidatos, filtramos por categoria, lado e distância de quem
        /// está vivo, e sorteamos.
        /// </summary>
        private static bool TryGetSpawnPosition(Player player, out Vector3 position)
        {
            position = Vector3.zero;

            var points = FikaBridge.GetSpawnPoints();
            if (points == null)
            {
                Plugin.Log.LogWarning("[TRL-PvpMode] Lista de pontos de nascimento indisponivel.");
                return false;
            }

            var sideMask = ToSideMask(player.Side);
            var deathPosition = player.Position;
            var minDistanceSqr = Settings.MIN_SPAWN_DISTANCE.Value * Settings.MIN_SPAWN_DISTANCE.Value;

            var candidates = _candidateBuffer;
            candidates.Clear();

            // Duas passadas: primeiro exigindo distância dos vivos; se ninguém sobrar, relaxa o
            // filtro (mapa pequeno ou lotado não pode impedir o renascimento).
            for (var strict = 1; strict >= 0; strict--)
            {
                foreach (var point in points)
                {
                    if (point == null) continue;
                    if ((point.Categories & ESpawnCategoryMask.Player) == 0) continue;
                    if ((point.Sides & sideMask) == 0) continue;

                    var candidate = point.Position;
                    if (candidate == Vector3.zero) continue;   // sentinela de ponto inválido

                    if ((candidate - deathPosition).sqrMagnitude <= SAME_SPOT_TOLERANCE_SQR) continue;

                    if (strict == 1 && !IsFarFromLivingPlayers(candidate, player, minDistanceSqr)) continue;

                    candidates.Add(candidate);
                }

                if (candidates.Count > 0) break;
            }

            if (candidates.Count == 0)
            {
                Plugin.Log.LogWarning(
                    $"[TRL-PvpMode] Nenhum candidato de spawn (lado {player.Side}, distancia minima " +
                    $"{Settings.MIN_SPAWN_DISTANCE.Value}m) — o mapa pode nao expor pontos do lado certo.");
                return false;
            }

            position = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            Plugin.Log.LogInfo($"[TRL-PvpMode] {candidates.Count} pontos candidatos; escolhido {position}.");
            return true;
        }

        private static bool IsFarFromLivingPlayers(Vector3 candidate, Player self, float minDistanceSqr)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var players = gameWorld?.AllAlivePlayersList;
            if (players == null) return true;

            for (var i = 0; i < players.Count; i++)
            {
                var other = players[i];
                if (other == null || other == self) continue;
                if ((other.Position - candidate).sqrMagnitude < minDistanceSqr) return false;
            }

            return true;
        }

        private static EPlayerSideMask ToSideMask(EPlayerSide side) => side switch
        {
            EPlayerSide.Usec => EPlayerSideMask.Usec,
            EPlayerSide.Bear => EPlayerSideMask.Bear,
            EPlayerSide.Savage => EPlayerSideMask.Savage,
            _ => EPlayerSideMask.All,
        };

        private static void StartProtection(Player player)
        {
            var seconds = Mathf.Max(0f, Settings.SPAWN_PROTECTION.Value);
            if (seconds <= 0f) return;

            player.ActiveHealthController.SetDamageCoeff(0f);
            _protectionLeft = seconds;
            _protectionActive = true;
            _protectedPlayer = player;
        }

        /// <summary>Chamado a cada quadro pelo patch de input, já filtrado para o jogador local.</summary>
        public static void Tick(Player player, float deltaTime)
        {
            if (!_protectionActive) return;

            _protectionLeft -= deltaTime;
            if (_protectionLeft > 0f) return;

            _protectionActive = false;
            _protectedPlayer = null;

            // Nao devolver o coeficiente se o jogador caiu de novo no meio da protecao: ali quem
            // manda e a invulnerabilidade do estado de caido, aplicada pelo Fika (F-09).
            if (player.ActiveHealthController is Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true }) return;

            player.ActiveHealthController.SetDamageCoeff(1f);
            Notify("Protecao de renascimento encerrada.", Color.yellow);
        }

        private static float _nextFailureNotice;

        /// <summary>
        /// Falha de sorteio com a tecla segurada se repetiria a cada ciclo; o antirrepique evita
        /// entupir a tela (D-11).
        /// </summary>
        private static void NotifyThrottled(string message, Color color)
        {
            if (Time.time < _nextFailureNotice) return;
            _nextFailureNotice = Time.time + 3f;
            Notify(message, color);
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
