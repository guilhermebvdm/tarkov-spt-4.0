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
        private static IFikaNetworkManager _lastRegisteredNetworkManager;

        public static void Initialize(ManualLogSource logger)
        {
            if (_initialized) return;
            _logger = logger;

            try
            {
                // Subscribe to Fika's network manager creation event as secondary trigger
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
            EnsurePacketsRegistered();
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

            if (_lastRegisteredNetworkManager != currentManager)
            {
                try
                {
                    currentManager.RegisterPacket<StanceSyncPacket>(OnStanceSyncPacketReceived);
                    _lastRegisteredNetworkManager = currentManager;
                    _logger?.LogInfo($"[CameraRotationMod] Registered StanceSyncPacket on new IFikaNetworkManager instance ({currentManager.GetType().Name}).");
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"[CameraRotationMod] Error registering StanceSyncPacket: {ex.Message}");
                }
            }
        }

        public static void SendStance(int stance, bool isAiming)
        {
            EnsurePacketsRegistered();
            if (_lastRegisteredNetworkManager == null) return;

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
            try
            {
                _lastRegisteredNetworkManager.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, true);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[CameraRotationMod] Error sending StanceSyncPacket: {ex.Message}");
            }
        }

        private static void OnStanceSyncPacketReceived(StanceSyncPacket packet)
        {
            try
            {
                if (_lastRegisteredNetworkManager == null) return;

                // Find the ObservedPlayer associated with the ProfileId
                var observedPlayer = _lastRegisteredNetworkManager.ObservedPlayers?.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);
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
            catch (Exception ex)
            {
                _logger?.LogError($"[CameraRotationMod] Error processing StanceSyncPacket: {ex.Message}");
            }
        }
    }
}
