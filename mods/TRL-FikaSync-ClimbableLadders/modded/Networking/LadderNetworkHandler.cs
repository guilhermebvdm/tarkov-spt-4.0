using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using Fika.Core.Main.Components;
using Fika.Core.Main.Players;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;
using HarmonyLib;
using tarkin.ladders.bep;
using tarkin.ladders.shared;
using TRL.FikaSync.ClimbableLadders.Controllers;
using TRL.FikaSync.ClimbableLadders.Networking.Packets;
using UnityEngine;

namespace TRL.FikaSync.ClimbableLadders.Networking
{
    internal class LadderNetworkHandler : IDisposable
    {
        private readonly List<MainPlayerLadderTracker> _trackers = new List<MainPlayerLadderTracker>();

        internal LadderNetworkHandler()
        {
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);

            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                RegisterPackets(Singleton<IFikaNetworkManager>.Instance);
            }

            PlayerLadderController.OnPlayerLadderControllerInit += OnPlayerLadderControllerSpawned;
        }

        private void OnFikaNetworkCreated(FikaNetworkManagerCreatedEvent fikaEvent)
        {
            if (fikaEvent?.Manager != null)
            {
                RegisterPackets(fikaEvent.Manager);
            }
        }

        private void RegisterPackets(IFikaNetworkManager manager)
        {
            manager.RegisterPacket<LadderStatePacket>(OnLadderStatePacketReceived);
            manager.RegisterPacket<BarAnglePacket>(OnBarAnglePacketReceived);
            Plugin.Logger?.LogInfo("[TRL-FikaSync] Pacotes de rede LadderStatePacket e BarAnglePacket registrados.");
        }

        private void OnPlayerLadderControllerSpawned(PlayerLadderController controller)
        {
            if (controller == null)
                return;

            var tracker = new MainPlayerLadderTracker(controller, OnTrackerDisposed);
            lock (_trackers)
            {
                _trackers.Add(tracker);
            }
        }

        private void OnTrackerDisposed(MainPlayerLadderTracker tracker)
        {
            lock (_trackers)
            {
                _trackers.Remove(tracker);
            }
        }

        private static Player ResolvePlayerByNetId(int netId)
        {
            if (CoopHandler.TryGetCoopHandler(out var coopHandler))
            {
                if (coopHandler.Players.TryGetValue(netId, out var fikaPlayer) && !fikaPlayer.IsYourPlayer)
                {
                    return fikaPlayer;
                }
            }

            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
                return null;

            var alivePlayers = gameWorld.AllAlivePlayersList;
            if (alivePlayers != null)
            {
                for (int i = 0; i < alivePlayers.Count; i++)
                {
                    var p = alivePlayers[i];
                    if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                    {
                        return p;
                    }
                }
            }

            var allPlayers = gameWorld.AllPlayersEverExisted;
            if (allPlayers != null)
            {
                foreach (var p in allPlayers)
                {
                    if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                    {
                        return p;
                    }
                }
            }

            return null;
        }

        private void OnLadderStatePacketReceived(LadderStatePacket packet)
        {
            try
            {
                var player = ResolvePlayerByNetId(packet.NetId);
                if (player == null)
                {
                    Plugin.Logger?.LogDebug($"[TRL-FikaSync] LadderStatePacket recebido para NetId {packet.NetId}, mas jogador ainda não foi resolvido.");
                    return;
                }

                switch (packet.Type)
                {
                    case LadderStatePacket.EStateType.Enter:
                    {
                        if (Ladder.TryGetLadderInstanceByNetId(packet.LadderId, out var ladder))
                        {
                            var controller = player.GetOrAddComponent<ObservedPlayerLadderController>();
                            controller.Init(ladder);
                            Plugin.Logger?.LogDebug($"[TRL-FikaSync] ObservedPlayerLadderController anexado e iniciado para jogador {player.Profile?.Nickname} (Ladder: {packet.LadderId})");
                        }
                        else
                        {
                            Plugin.Logger?.LogError($"[TRL-FikaSync] Falha ao encontrar escada com NetId '{packet.LadderId}' para o jogador {player.Profile?.Nickname}");
                        }
                        break;
                    }
                    case LadderStatePacket.EStateType.Exit:
                    {
                        if (player.TryGetComponent<ObservedPlayerLadderController>(out var controller))
                        {
                            Component.Destroy(controller);
                            Plugin.Logger?.LogDebug($"[TRL-FikaSync] ObservedPlayerLadderController removido do jogador {player.Profile?.Nickname}");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[TRL-FikaSync] Erro ao processar LadderStatePacket: {ex}");
            }
        }

        private void OnBarAnglePacketReceived(BarAnglePacket packet)
        {
            try
            {
                var player = ResolvePlayerByNetId(packet.NetId);
                if (player == null)
                    return;

                if (player.TryGetComponent<ObservedPlayerLadderController>(out var controller))
                {
                    controller.ReceiveBarAngle(packet.Angle);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[TRL-FikaSync] Erro ao processar BarAnglePacket: {ex}");
            }
        }

        private void UnregisterPackets(IFikaNetworkManager manager)
        {
            var packetProcessor = GetPacketProcessor(manager);
            if (packetProcessor == null)
                return;

            packetProcessor.RemoveSubscription<LadderStatePacket>();
            packetProcessor.RemoveSubscription<BarAnglePacket>();
        }

        private static NetPacketProcessor GetPacketProcessor(IFikaNetworkManager manager)
        {
            if (manager == null)
                return null;

            var field = AccessTools.Field(manager.GetType(), "_packetProcessor");
            return field?.GetValue(manager) as NetPacketProcessor;
        }

        public void Dispose()
        {
            PlayerLadderController.OnPlayerLadderControllerInit -= OnPlayerLadderControllerSpawned;

            lock (_trackers)
            {
                for (int i = _trackers.Count - 1; i >= 0; i--)
                {
                    _trackers[i].Dispose();
                }
                _trackers.Clear();
            }

            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);

            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                UnregisterPackets(Singleton<IFikaNetworkManager>.Instance);
            }
        }
    }
}
