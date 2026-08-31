using Comfort.Common;
using EFT;
using Fika.Core.Main.Components;
using Fika.Core.Main.Players;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using tarkin.ladders.bep;
using tarkin.ladders.shared;
using UnityEngine;

namespace tarkin.ladders.fika
{
    internal class FikaHandler : IDisposable
    {
        private readonly List<MainPlayerLadderControllerTracker> _trackers = new List<MainPlayerLadderControllerTracker>();

        internal FikaHandler() 
        {
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);
            if (Singleton<IFikaNetworkManager>.Instantiated)
                RegisterPackets(Singleton<IFikaNetworkManager>.Instance);

            PlayerLadderController.OnPlayerLadderControllerInit += PlayerLadderController_OnPlayerLadderControllerSpawned;
        }

        void OnFikaNetworkCreated(FikaNetworkManagerCreatedEvent fikaEvent)
        {
            RegisterPackets(fikaEvent.Manager);
        }

        void RegisterPackets(IFikaNetworkManager manager)
        {
            manager.RegisterPacket<LadderStatePacket>(OnLadderStatePacketReceived);
            manager.RegisterPacket<BarAnglePacket>(OnBarAnglePacketReceived);
            Plugin.Logger.LogInfo("[FikaHandler] Pacotes LadderStatePacket e BarAnglePacket registrados.");
        }

        private void PlayerLadderController_OnPlayerLadderControllerSpawned(PlayerLadderController controller)
        {
            if (controller == null) return;

            lock (_trackers)
            {
                _trackers.Add(new MainPlayerLadderControllerTracker(controller, OnTrackerDisposed));
            }
        }

        private void OnTrackerDisposed(MainPlayerLadderControllerTracker tracker)
        {
            // ref: AUD-01-01 Remoção síncrona do tracker descartado
            lock (_trackers)
            {
                _trackers.Remove(tracker);
            }
        }

        private Player ResolvePlayerByNetId(int netId)
        {
            // 1. Tenta via CoopHandler.Players
            if (CoopHandler.TryGetCoopHandler(out var coopHandler) && coopHandler.Players != null)
            {
                if (coopHandler.Players.TryGetValue(netId, out var fikaPlayer) && fikaPlayer != null)
                {
                    if (fikaPlayer.IsYourPlayer) return null;
                    return fikaPlayer;
                }
            }

            // 2. Fallback: Varredura em AllAlivePlayersList
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null) return null;

            var alivePlayers = gameWorld.AllAlivePlayersList;
            if (alivePlayers != null)
            {
                for (int i = 0; i < alivePlayers.Count; i++)
                {
                    var p = alivePlayers[i];
                    if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                    {
                        return fp;
                    }
                }
            }

            // 3. Fallback: Varredura em AllPlayersEverExisted
            var allPlayers = gameWorld.AllPlayersEverExisted;
            if (allPlayers != null)
            {
                foreach (var p in allPlayers)
                {
                    if (p is FikaPlayer fp && fp.NetId == netId && !fp.IsYourPlayer)
                    {
                        return fp;
                    }
                }
            }

            return null;
        }

        void OnLadderStatePacketReceived(LadderStatePacket packet)
        {
            try
            {
                Player player = ResolvePlayerByNetId(packet.NetId);
                if (player == null)
                {
                    Plugin.Logger.LogDebug($"[FikaHandler] Jogador com NetId {packet.NetId} não encontrado para LadderState.");
                    return;
                }

                switch (packet.Type)
                {
                    case LadderStatePacket.EStateType.Enter:
                        if (Ladder.TryGetLadderInstanceByNetId(packet.LadderId, out Ladder ladder))
                        {
                            player.GetOrAddComponent<ObservedPlayerLadderController>().Init(ladder);
                        }
                        else
                        {
                            Plugin.Logger.LogError($"[FikaHandler] Falha ao encontrar escada com NetId: {packet.LadderId}");
                        }
                        break;

                    case LadderStatePacket.EStateType.Exit:
                        if (player.TryGetComponent<ObservedPlayerLadderController>(out var ladderController))
                        {
                            Component.Destroy(ladderController);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[FikaHandler] Erro ao processar LadderStatePacket: {ex}");
            }
        }

        void OnBarAnglePacketReceived(BarAnglePacket packet)
        {
            try
            {
                Player player = ResolvePlayerByNetId(packet.NetId);
                if (player == null) return;

                if (player.TryGetComponent<ObservedPlayerLadderController>(out var ladderController))
                {
                    ladderController.ReceiveBarAngle(packet.Angle);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[FikaHandler] Erro ao processar BarAnglePacket: {ex}");
            }
        }

        void UnregisterPackets(IFikaNetworkManager manager)
        {
            NetPacketProcessor packetProcessor = GetPacketProcessor(manager);
            if (packetProcessor == null)
                return;

            packetProcessor.RemoveSubscription<LadderStatePacket>();
            packetProcessor.RemoveSubscription<BarAnglePacket>();
        }

        private static FieldInfo _cachedPacketProcessorField;

        public static NetPacketProcessor GetPacketProcessor(IFikaNetworkManager manager = null)
        {
            manager ??= Singleton<IFikaNetworkManager>.Instance;
            if (manager == null) return null;

            // ref: AUD-01-05 Reflection com cache estático
            if (_cachedPacketProcessorField == null)
            {
                _cachedPacketProcessorField = AccessTools.Field(manager.GetType(), "_packetProcessor");
            }

            return _cachedPacketProcessorField?.GetValue(manager) as NetPacketProcessor;
        }

        public void Dispose()
        {
            PlayerLadderController.OnPlayerLadderControllerInit -= PlayerLadderController_OnPlayerLadderControllerSpawned;

            lock (_trackers)
            {
                foreach (var tracker in _trackers)
                {
                    tracker.Dispose();
                }
                _trackers.Clear();
            }

            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);

            if (Singleton<IFikaNetworkManager>.Instantiated)
                UnregisterPackets(Singleton<IFikaNetworkManager>.Instance);
        }
    }
}
