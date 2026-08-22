using System;
using Comfort.Common;
using Fika.Core.Main.Players;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using tarkin.ladders.bep;
using TRL.FikaSync.ClimbableLadders.Networking.Packets;
using UnityEngine;

namespace TRL.FikaSync.ClimbableLadders.Networking
{
    internal class MainPlayerLadderTracker : IDisposable
    {
        private readonly PlayerLadderController _controller;
        private readonly Action<MainPlayerLadderTracker> _onDisposed;
        private float _timeSinceLastSentRollPacket;
        private const float PacketSendCooldown = 0.05f; // 20 Hz
        private bool _isDisposed;

        internal MainPlayerLadderTracker(PlayerLadderController mainPlayerController, Action<MainPlayerLadderTracker> onDisposed = null)
        {
            _controller = mainPlayerController;
            _onDisposed = onDisposed;

            _controller.OnProceduralBodyCreate += Controller_OnProceduralBodyCreate;
            _controller.OnProceduralBodyDestroy += Controller_OnProceduralBodyDestroy;
            _controller.OnBarAngleChanged += Controller_OnBarAngleChanged;
        }

        private int GetPlayerNetId()
        {
            if (_controller.Player is FikaPlayer fp)
            {
                return fp.NetId;
            }

            return _controller.Player.PlayerId;
        }

        private void Controller_OnProceduralBodyCreate()
        {
            if (_controller?.Ladder == null || _controller.Player == null)
                return;

            var packet = new LadderStatePacket
            {
                NetId = GetPlayerNetId(),
                LadderId = _controller.Ladder.NetId,
                Type = LadderStatePacket.EStateType.Enter
            };

            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, broadcast: true);
            Plugin.Logger?.LogDebug($"[TRL-FikaSync] Enviado LadderStatePacket.Enter (Player NetId: {packet.NetId}, Ladder: {packet.LadderId})");
        }

        private void Controller_OnProceduralBodyDestroy()
        {
            if (_controller?.Player == null)
                return;

            var packet = new LadderStatePacket
            {
                NetId = GetPlayerNetId(),
                LadderId = _controller.Ladder != null ? _controller.Ladder.NetId : string.Empty,
                Type = LadderStatePacket.EStateType.Exit
            };

            Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.ReliableOrdered, broadcast: true);
            Plugin.Logger?.LogDebug($"[TRL-FikaSync] Enviado LadderStatePacket.Exit (Player NetId: {packet.NetId})");

            Dispose();
        }

        private void Controller_OnBarAngleChanged(float rollAngle)
        {
            if (_controller?.Player == null || _isDisposed)
                return;

            _timeSinceLastSentRollPacket += Time.unscaledDeltaTime;
            if (_timeSinceLastSentRollPacket >= PacketSendCooldown)
            {
                var packet = new BarAnglePacket
                {
                    NetId = GetPlayerNetId(),
                    Angle = rollAngle
                };

                Singleton<IFikaNetworkManager>.Instance?.SendData(ref packet, DeliveryMethod.Sequenced, broadcast: true);
                _timeSinceLastSentRollPacket = 0f;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_controller != null)
            {
                _controller.OnProceduralBodyCreate -= Controller_OnProceduralBodyCreate;
                _controller.OnProceduralBodyDestroy -= Controller_OnProceduralBodyDestroy;
                _controller.OnBarAngleChanged -= Controller_OnBarAngleChanged;
            }

            _onDisposed?.Invoke(this);
        }
    }
}
