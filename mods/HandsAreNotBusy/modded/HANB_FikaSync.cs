using System;
using System.Linq;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using Fika.Core.Networking.LiteNetLib.Utils;
using Fika.Core.Main.Utils;

namespace HandsAreNotBusy
{
    public struct HanbClearInventoryPacket : INetSerializable
    {
        public string ProfileId;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId ?? string.Empty);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = string.Empty;
            if (!reader.TryGetString(out ProfileId)) return;
        }
    }

    internal static class HANB_FikaSync
    {
        private static ManualLogSource _logger;
        private static bool _initialized = false;
        private static IFikaNetworkManager _lastRegisteredNetworkManager;

        public static bool IsFikaClient
        {
            get
            {
                try
                {
                    return _initialized && FikaBackendUtils.ClientType == EClientType.Client;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Initialize(ManualLogSource logger)
        {
            if (_initialized) return;
            _logger = logger;

            try
            {
                FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
                FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerDestroyedEvent>(OnNetworkManagerDestroyed);
                _logger?.LogInfo("[HANB] Integração com FIKA Network inicializada.");
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[HANB] FIKA não detectado ou integração indisponível: {ex.Message}");
            }
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent ev)
        {
            EnsurePacketsRegistered();
        }

        private static void OnNetworkManagerDestroyed(FikaNetworkManagerDestroyedEvent ev)
        {
            // Padrão canônico FIKA: NUNCA desregistrar pacotes (UnregisterPacket) no LiteNetLib,
            // pois datagramas tardios lançariam ParseException. Apenas zeramos a referência local.
            _lastRegisteredNetworkManager = null;
            _logger?.LogInfo("[HANB] Sessão FIKA encerrada; referências de rede limpas.");
        }

        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated)
            {
                _lastRegisteredNetworkManager = null;
                return;
            }

            var currentManager = Singleton<IFikaNetworkManager>.Instance;
            if (currentManager == null) return;
            if (_lastRegisteredNetworkManager == currentManager) return;

            try
            {
                currentManager.RegisterPacket<HanbClearInventoryPacket>(OnClearInventoryPacketReceived);
                _lastRegisteredNetworkManager = currentManager;
                _logger?.LogInfo($"[HANB] Pacote HanbClearInventoryPacket registrado no {currentManager.GetType().Name}.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[HANB] Erro ao registrar HanbClearInventoryPacket: {ex}");
            }
        }

        public static void SendResetRequestToServer(Player player)
        {
            if (player == null) return;

            try
            {
                EnsurePacketsRegistered();
                if (_lastRegisteredNetworkManager == null) return;

                if (FikaBackendUtils.ClientType == EClientType.Client)
                {
                    var packet = new HanbClearInventoryPacket
                    {
                        ProfileId = player.ProfileId
                    };

                    _lastRegisteredNetworkManager.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
                    _logger?.LogInfo($"[HANB-Fika] Convidado enviou HanbClearInventoryPacket para o Host (Profile: {player.ProfileId}).");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[HANB-Fika] Erro ao enviar reset para o servidor: {ex}");
            }
        }

        private static void OnClearInventoryPacketReceived(HanbClearInventoryPacket packet)
        {
            try
            {
                if (!FikaBackendUtils.IsServer) return;

                if (string.IsNullOrEmpty(packet.ProfileId)) return;

                var gameWorld = Singleton<GameWorld>.Instance;
                Player targetPlayer = null;

                // 1. Tenta localizar em ObservedPlayers do FIKA
                if (_lastRegisteredNetworkManager?.ObservedPlayers != null)
                {
                    targetPlayer = _lastRegisteredNetworkManager.ObservedPlayers.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);
                }

                // 2. Fallback: busca em CoopHandler.Players do FIKA
                if (targetPlayer == null && _lastRegisteredNetworkManager?.CoopHandler?.Players != null)
                {
                    targetPlayer = _lastRegisteredNetworkManager.CoopHandler.Players.Values.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);
                }

                // 3. Fallback: busca em AllAlivePlayersList do GameWorld
                if (targetPlayer == null && gameWorld != null && gameWorld.AllAlivePlayersList != null)
                {
                    targetPlayer = gameWorld.AllAlivePlayersList.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);
                }

                // 4. Fallback: busca em RegisteredPlayers (abrange todos os estados)
                if (targetPlayer == null && gameWorld != null && gameWorld.RegisteredPlayers != null)
                {
                    targetPlayer = gameWorld.RegisteredPlayers.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId) as Player;
                }

                if (targetPlayer == null)
                {
                    _logger?.LogWarning($"[HANB-Fika] Host recebeu pedido de reset mas não encontrou jogador com ProfileId: {packet.ProfileId}");
                    return;
                }

                var inv = targetPlayer.InventoryController;
                if (inv != null && inv.List_0 != null)
                {
                    int length = inv.List_0.Count;
                    if (length > 0)
                    {
                        var args = new GEventArgs1[length];
                        inv.List_0.CopyTo(args);
                        foreach (var queuedEvent in args)
                        {
                            inv.RemoveActiveEvent(queuedEvent);
                        }
                        string nickname = targetPlayer.Profile?.Nickname ?? packet.ProfileId;
                        _logger?.LogInfo($"[HANB-Fika] Host limpou com sucesso {length} operações travadas no inventário de {nickname} ({packet.ProfileId}).");
                    }
                    else
                    {
                        string nickname = targetPlayer.Profile?.Nickname ?? packet.ProfileId;
                        _logger?.LogInfo($"[HANB-Fika] Host verificou inventário de {nickname}: nenhuma operação travada em List_0.");
                    }
                }

                // CR-01-05: seguro — FikaServer/FikaClient.Update() chama PollEvents() na thread Unity.
                // Este callback é sempre executado na thread principal.
                // Ref: FikaServer.cs:L548, FikaClient.cs:L314 (mods/FIKA/modded)
                targetPlayer.ProcessStatus = Player.EProcessStatus.None;
                targetPlayer.SetInventoryOpened(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[HANB-Fika] Erro ao processar HanbClearInventoryPacket no Host: {ex}");
            }
        }
    }
}
