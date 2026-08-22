using Comfort.Common;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using System;
using tarkin.ladders.bep;
using UnityEngine;

namespace tarkin.ladders.fika
{
    internal class MainPlayerLadderControllerTracker : IDisposable
    {
        readonly PlayerLadderController controller;

        private float timeSinceLastSentRollPacket;
        private const float PacketSendCooldown = 0.05f;

        internal MainPlayerLadderControllerTracker(PlayerLadderController mainPlayerController)
        {
            controller = mainPlayerController;

            controller.OnProceduralBodyCreate += Controller_OnProceduralBodyCreate;
            controller.OnProceduralBodyDestroy += Controller_OnProceduralBodyDestroy;
            controller.OnBarAngleChanged += Controller_OnBarAngleChanged;
        }

        private void Controller_OnProceduralBodyCreate()
        {
            LadderStatePacket packet = new LadderStatePacket()
            {
                LadderId = controller.Ladder.NetId,
                PlayerId = controller.Player.PlayerId,
                Type = LadderStatePacket.EStateType.Enter
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        private void Controller_OnProceduralBodyDestroy()
        {
            LadderStatePacket packet = new LadderStatePacket()
            {
                LadderId = controller.Ladder.NetId,
                PlayerId = controller.Player.PlayerId,
                Type = LadderStatePacket.EStateType.Exit
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        private void Controller_OnBarAngleChanged(float rollAngle)
        {
            timeSinceLastSentRollPacket += Time.unscaledDeltaTime;
            if (timeSinceLastSentRollPacket > PacketSendCooldown)
            {
                BarAnglePacket packet = new BarAnglePacket()
                {
                    PlayerId = controller.Player.PlayerId,
                    Angle = rollAngle
                };
                Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.Sequenced, true);
                timeSinceLastSentRollPacket = 0f;
            }
        }

        public void Dispose()
        {
            if (controller != null)
            {
                controller.OnProceduralBodyCreate -= Controller_OnProceduralBodyCreate;
                controller.OnProceduralBodyDestroy -= Controller_OnProceduralBodyDestroy;
                controller.OnBarAngleChanged -= Controller_OnBarAngleChanged;
            }
        }
    }
}
