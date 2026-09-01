using Comfort.Common;
using Fika.Core.Main.Players;
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
        private readonly Action<MainPlayerLadderControllerTracker> _onDisposed;

        private float timeSinceLastSentRollPacket;
        private const float PacketSendCooldown = 0.05f;

        internal MainPlayerLadderControllerTracker(PlayerLadderController mainPlayerController, Action<MainPlayerLadderControllerTracker> onDisposed = null)
        {
            controller = mainPlayerController;
            _onDisposed = onDisposed;

            controller.OnProceduralBodyCreate += Controller_OnProceduralBodyCreate;
            controller.OnProceduralBodyDestroy += Controller_OnProceduralBodyDestroy;
            controller.OnBarAngleChanged += Controller_OnBarAngleChanged;
        }

        private int GetPlayerNetId()
        {
            if (controller?.Player is FikaPlayer fp)
            {
                return fp.NetId;
            }
            return controller?.Player?.PlayerId ?? 0;
        }

        private void Controller_OnProceduralBodyCreate()
        {
            LadderStatePacket packet = new LadderStatePacket()
            {
                LadderId = controller.Ladder?.NetId,
                NetId = GetPlayerNetId(),
                Type = LadderStatePacket.EStateType.Enter
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);
        }

        private void Controller_OnProceduralBodyDestroy()
        {
            LadderStatePacket packet = new LadderStatePacket()
            {
                LadderId = controller.Ladder?.NetId,
                NetId = GetPlayerNetId(),
                Type = LadderStatePacket.EStateType.Exit
            };
            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, true);

            // ref: AUD-01-01 Auto-descarte ao sair da escada
            Dispose();
        }

        private void Controller_OnBarAngleChanged(float rollAngle)
        {
            timeSinceLastSentRollPacket += Time.unscaledDeltaTime;
            if (timeSinceLastSentRollPacket > PacketSendCooldown)
            {
                BarAnglePacket packet = new BarAnglePacket()
                {
                    NetId = GetPlayerNetId(),
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
            _onDisposed?.Invoke(this);
        }
    }
}
