using System;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Players;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using UnityEngine;

namespace TarkovRedLine.PvpMode.Networking
{
    /// <summary>
    /// Registro e tráfego do aviso de renascimento.
    ///
    /// Padrão obrigatório do repo (AP-11, guia de rede §4):
    ///  - registro rastreado por **instância** do gerenciador, nunca por um `bool` — o FIKA destrói e
    ///    recria o gerenciador a cada troca de sessão, e o novo já nasce sem os nossos registros;
    ///  - `EnsurePacketsRegistered()` chamado no `Update` do plugin **e** antes de todo envio;
    ///  - **nunca** chamar `UnregisterPacket`; fora de raid se resolve com guarda no callback;
    ///  - airbag (`try/catch`) na raiz do callback: exceção que escapa daqui derruba a fila de eventos
    ///    do quadro para **todos os pares e todos os mods**, não só para este mod.
    /// </summary>
    internal static class RespawnNetwork
    {
        private static IFikaNetworkManager _lastRegisteredManager;

        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated) return;

            var manager = Singleton<IFikaNetworkManager>.Instance;
            if (ReferenceEquals(_lastRegisteredManager, manager)) return;

            try
            {
                manager.RegisterPacket<TrlRespawnSyncPacket>(OnRespawnSyncReceived);
                _lastRegisteredManager = manager;
                Plugin.Log.LogInfo("[TRL-PvpMode] Pacote de respawn registrado na instancia atual do FIKA.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] Falha ao registrar o pacote de respawn: {ex.Message}");
            }
        }

        /// <summary>Anuncia que o jogador local renasceu na posição dada.</summary>
        public static void BroadcastRespawn(FikaPlayer player, Vector3 position)
        {
            EnsurePacketsRegistered();
            if (!Singleton<IFikaNetworkManager>.Instantiated) return;

            try
            {
                var packet = new TrlRespawnSyncPacket
                {
                    NetId = player.NetId,
                    Position = position,
                    Yaw = player.Rotation.x,
                };

                // Envio só a partir da main thread: o FIKA compartilha um único NetDataWriter sem
                // trava entre todos os envios. Este caminho vem de UpdateTick, então está na main.
                Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered, broadcast: true);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"[TRL-PvpMode] Erro ao anunciar respawn: {ex.Message}");
            }
        }

        private static void OnRespawnSyncReceived(TrlRespawnSyncPacket packet)
        {
            try
            {
                // Guarda de contexto no lugar de UnregisterPacket.
                if (!Singleton<GameWorld>.Instantiated) return;

                // Corpo truncado: não processar NEM retransmitir.
                if (!packet.Valid) return;

                if (!Singleton<IFikaNetworkManager>.Instantiated) return;

                var observed = FindObserved(packet.NetId);

                // Jogador desconhecido (entrou depois, já saiu, ou é o nosso próprio eco):
                // ignorar em silêncio é o comportamento correto.
                if (observed == null) return;

                ApplySnap(observed, packet.Position, packet.Yaw);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] Excecao no callback de respawn: {ex}");
            }
        }

        private static ObservedPlayer FindObserved(int netId)
        {
            var players = Singleton<IFikaNetworkManager>.Instance.ObservedPlayers;
            if (players == null) return null;

            // Laço simples em vez de LINQ: o callback roda no caminho de rede.
            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                if (candidate != null && candidate.NetId == netId) return candidate;
            }

            return null;
        }

        /// <summary>
        /// Corte seco na posição. Sem isto, o corpo interpola entre a posição antiga e a nova
        /// (ObservedPlayer.cs:906, Vector3.LerpUnclamped) — sem nenhuma detecção de teleporte — e
        /// atravessa o mapa em linha reta.
        ///
        /// Limpar o histórico de estados é o que impede a interpolação de continuar mirando o
        /// trajeto antigo depois que cravamos a posição.
        /// </summary>
        private static void ApplySnap(ObservedPlayer observed, Vector3 position, float yaw)
        {
            observed.Snapshotter?.Clear();

            observed.Teleport(position);
            observed.Transform.position = position;

            var rotation = observed.Rotation;
            rotation.x = yaw;
            observed.Rotation = rotation;
        }
    }
}
