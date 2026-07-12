using System;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib.Utils;
using EFT;

namespace TrueTrauma
{
    // Crie o pacote de rede implementando INetSerializable
    public struct TraumaFaintPacket : INetSerializable
    {
        public string ProfileId;
        public bool IsFainted;

        public TraumaFaintPacket(string profileId, bool isFainted)
        {
            ProfileId = profileId;
            IsFainted = isFainted;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(IsFainted);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            IsFainted = reader.GetBool();
        }
    }

    // Gerencia o registro do pacote via Eventos do Fika
    public static class FikaPacketManager
    {
        private static bool _initialized = false;
        private static IFikaNetworkManager _networkManager;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // Inscreve no evento disparado quando o gerenciador de rede do Fika for instanciado
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent ev)
        {
            _networkManager = ev.Manager;
            TraumaState.Logger.LogInfo("TrueTrauma: Gerenciador de rede do Fika detectado. Registrando pacotes...");

            // Servidor (Host): usa handler com NetPeer para poder re-enviar excluindo o remetente
            if (_networkManager is FikaServer)
            {
                _networkManager.RegisterPacket<TraumaFaintPacket, LiteNetLib.NetPeer>(OnTraumaFaintPacketReceivedOnServer);
                TraumaState.Logger.LogInfo("TrueTrauma: Registrado handler de SERVIDOR para pacotes de desmaio.");
            }
            // Cliente (Convidado): handler simples sem NetPeer
            else
            {
                _networkManager.RegisterPacket<TraumaFaintPacket>(OnTraumaFaintPacketReceivedOnClient);
                TraumaState.Logger.LogInfo("TrueTrauma: Registrado handler de CLIENTE para pacotes de desmaio.");
            }
        }

        // --- HANDLER DO SERVIDOR (HOST) ---
        // Recebe o pacote do convidado, atualiza localmente, controla bots, e re-envia para todos os outros clientes
        private static void OnTraumaFaintPacketReceivedOnServer(TraumaFaintPacket packet, LiteNetLib.NetPeer senderPeer)
        {
            TraumaState.Logger.LogInfo(string.Format("TrueTrauma: [Servidor] Recebido pacote de desmaio. Perfil: {0}, Desmaiado: {1}", packet.ProfileId, packet.IsFainted));
            
            // 1. Atualiza a lista de desmaiados no Host
            FikaBridge.UpdateFaintedList(packet.ProfileId, packet.IsFainted);

            // 2. Sincroniza os BlackoutTimers no Host para que o MainLoopPatch
            //    execute a neutralização contínua de aggro a cada frame
            if (packet.IsFainted)
            {
                float duration = TrueTraumaPlugin.ConfigBlackoutDuration.Value;
                float now = UnityEngine.Time.time;
                TraumaState.BlackoutTimers[packet.ProfileId] = now + duration;
                TraumaState.BlackoutStartTimes[packet.ProfileId] = now;
                TraumaState.GraceTimers[packet.ProfileId] = now + duration + 5f;
            }
            else
            {
                TraumaState.BlackoutTimers.Remove(packet.ProfileId);
                TraumaState.BlackoutStartTimes.Remove(packet.ProfileId);
            }

            // 3. Controla o aggro dos bots (os bots rodam no Host)
            var gameWorld = Comfort.Common.Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                var victim = gameWorld.GetAlivePlayerByProfileID(packet.ProfileId);
                if (victim != null)
                {
                    if (packet.IsFainted)
                    {
                        AggroHelper.NeutralizeAggro(victim);
                    }
                    else
                    {
                        AggroHelper.RestoreAggro(victim);
                    }
                }
            }

            // 3. Re-envia o pacote para TODOS os outros clientes (excluindo o remetente)
            if (_networkManager is FikaServer server)
            {
                server.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered, senderPeer);
                TraumaState.Logger.LogInfo(string.Format("TrueTrauma: [Servidor] Reenviando pacote de desmaio para outros clientes. Perfil: {0}", packet.ProfileId));
            }
        }

        // --- HANDLER DO CLIENTE (CONVIDADO) ---
        // Recebe o pacote re-enviado pelo servidor e atualiza a lista local
        private static void OnTraumaFaintPacketReceivedOnClient(TraumaFaintPacket packet)
        {
            TraumaState.Logger.LogInfo(string.Format("TrueTrauma: [Cliente] Recebido pacote de desmaio. Perfil: {0}, Desmaiado: {1}", packet.ProfileId, packet.IsFainted));
            
            // Atualiza a lista local de desmaiados (para o BotVisionPatch funcionar)
            FikaBridge.UpdateFaintedList(packet.ProfileId, packet.IsFainted);
        }

        public static void SendTraumaFaintPacket(string profileId, bool isFainted)
        {
            if (_networkManager == null) return; // Só envia se em modo multiplayer/fika ativo

            var packet = new TraumaFaintPacket(profileId, isFainted);
            
            // Se é o Servidor (Host), envia para todos os clientes
            if (_networkManager is FikaServer server)
            {
                server.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
            // Se é o Cliente (Convidado), envia para o servidor (que vai re-enviar pra todos)
            else if (_networkManager is FikaClient client)
            {
                client.SendData(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
