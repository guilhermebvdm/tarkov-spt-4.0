using Comfort.Common;
using Fika.Core.Main.Components;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using tarkin.ladders.bep;
using tarkin.ladders.shared;
using UnityEngine;

namespace tarkin.ladders.fika
{
    internal class FikaHandler : IDisposable
    {
        readonly List<MainPlayerLadderControllerTracker> mainPlayerLadderControllerTrackers = new List<MainPlayerLadderControllerTracker>();

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
        }

        private void PlayerLadderController_OnPlayerLadderControllerSpawned(PlayerLadderController controller)
        {
            mainPlayerLadderControllerTrackers.Add(new MainPlayerLadderControllerTracker(controller));
        }

        void OnLadderStatePacketReceived(LadderStatePacket packet)
        {
            if (CoopHandler.TryGetCoopHandler(out var coopHandler))
            {
                if (coopHandler.Players.TryGetValue(packet.PlayerId, out var player))
                {
                    if (player.IsYourPlayer)
                        return;

                    switch (packet.Type)
                    {
                        case LadderStatePacket.EStateType.Enter:
                            {
                                if (Ladder.TryGetLadderInstanceByNetId(packet.LadderId, out Ladder ladder))
                                {
                                    player.GetOrAddComponent<ObservedPlayerLadderController>().Init(ladder);
                                }
                                else
                                {
                                    Plugin.Logger.LogError($"Failed to find ladder with NetId: {packet.LadderId}");
                                }
                            }
                            break;
                        case LadderStatePacket.EStateType.Exit:
                            {
                                if (player.TryGetComponent<ObservedPlayerLadderController>(out var ladderController))
                                {
                                    Component.Destroy(ladderController);
                                }
                            }
                            break;
                    }
                }
            }
        }

        void OnBarAnglePacketReceived(BarAnglePacket packet)
        {
            if (CoopHandler.TryGetCoopHandler(out var coopHandler))
            {
                if (coopHandler.Players.TryGetValue(packet.PlayerId, out var player))
                {
                    if (player.IsYourPlayer)
                        return;

                    if (player.TryGetComponent<ObservedPlayerLadderController>(out var ladderController))
                    {
                        ladderController.ReceiveBarAngle(packet.Angle);
                    }
                }
            }
        }

        void UnregisterPackets(IFikaNetworkManager manager)
        {
            NetPacketProcessor packetProcessor = GetPacketProcessor();
            if (packetProcessor == null)
                return;

            packetProcessor.RemoveSubscription<LadderStatePacket>();
            packetProcessor.RemoveSubscription<BarAnglePacket>();
        }

        public static NetPacketProcessor GetPacketProcessor()
        {
            IFikaNetworkManager manager = Singleton<IFikaNetworkManager>.Instance;
            if (manager == null) return null;

            var field = AccessTools.Field(manager.GetType(), "_packetProcessor");

            return field?.GetValue(manager) as NetPacketProcessor;
        }

        public void Dispose()
        {
            PlayerLadderController.OnPlayerLadderControllerInit -= PlayerLadderController_OnPlayerLadderControllerSpawned;

            foreach (var tracker in mainPlayerLadderControllerTrackers)
            {
                tracker.Dispose();
            }

            FikaEventDispatcher.UnsubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetworkCreated);

            if (Singleton<IFikaNetworkManager>.Instantiated)
                UnregisterPackets(Singleton<IFikaNetworkManager>.Instance);
        }
    }
}
