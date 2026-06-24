using System;
using System.Linq;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using UnityEngine;
using CameraRotationMod.Networking;

namespace CameraRotationMod.Networking
{
    public static class FikaSyncManager
    {
        private static ManualLogSource _logger;
        private static bool _initialized = false;
        private static IFikaNetworkManager _fikaNetworkManager;

        public static void Initialize(ManualLogSource logger)
        {
            if (_initialized) return;
            _logger = logger;

            try
            {
                // Subscribe to Fika's network manager creation event
                FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkManagerCreated);
                _logger.LogInfo("[CameraRotationMod] Fika integration initialized.");
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CameraRotationMod] Failed to initialize Fika integration: {ex.Message}");
            }
        }

        private static void OnNetworkManagerCreated(FikaNetworkManagerCreatedEvent ev)
        {
            _logger.LogInfo("[CameraRotationMod] Fika Network Manager created. Registering packets.");
            _fikaNetworkManager = ev.Manager;
            
            // Register our custom packet
            _fikaNetworkManager.RegisterPacket<StanceSyncPacket>(OnStanceSyncPacketReceived);
        }

        public static void SendStance(int stance, bool isAiming)
        {
            if (!_initialized || _fikaNetworkManager == null) return;

            Player player = Singleton<EFT.GameWorld>.Instantiated && Singleton<EFT.GameWorld>.Instance.MainPlayer != null
                ? Singleton<EFT.GameWorld>.Instance.MainPlayer
                : null;

            if (player == null) return;

            var packet = new StanceSyncPacket
            {
                ProfileId = player.ProfileId,
                Stance = stance,
                IsAiming = isAiming
            };

            // Send to all other clients via Fika
            _fikaNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, true);
        }

        private static void OnStanceSyncPacketReceived(StanceSyncPacket packet)
        {
            if (_fikaNetworkManager == null) return;

            // Find the ObservedPlayer associated with the ProfileId
            var observedPlayer = _fikaNetworkManager.ObservedPlayers.FirstOrDefault(p => p.ProfileId == packet.ProfileId);
            if (observedPlayer != null)
            {
                // Assign an animator component if it doesn't exist
                var animator = observedPlayer.gameObject.GetComponent<ObservedStanceAnimator>();
                if (animator == null)
                {
                    animator = observedPlayer.gameObject.AddComponent<ObservedStanceAnimator>();
                    animator.Init(observedPlayer);
                }

                // Apply the new stance
                animator.SetStance(packet.Stance, packet.IsAiming);
            }
        }
    }
}
