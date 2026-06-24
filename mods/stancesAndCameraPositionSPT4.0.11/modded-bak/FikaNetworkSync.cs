using BepInEx.Bootstrap;
using CameraRotationMod;
using Fika.Core.Networking.LiteNetLib.Utils;
using Fika.Core.Networking;
using UnityEngine;
using Comfort.Common;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace CameraRotationMod.FikaSync
{
    public struct StanceSyncPacket : INetSerializable
    {
        public string ProfileId;
        public int StanceInt;
        public bool IsMounting;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ProfileId);
            writer.Put(StanceInt);
            writer.Put(IsMounting);
        }

        public void Deserialize(NetDataReader reader)
        {
            ProfileId = reader.GetString();
            StanceInt = reader.GetInt();
            IsMounting = reader.GetBool();
        }
    }

    public static class FikaNetworkSync
    {
        public static bool IsFikaLoaded { get; private set; }

        public static void Init()
        {
            IsFikaLoaded = Chainloader.PluginInfos.ContainsKey("com.fika.core");
            if (IsFikaLoaded)
            {
                RegisterFikaPackets();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterFikaPackets()
        {
            if (Singleton<FikaClient>.Instance != null)
            {
                Singleton<FikaClient>.Instance.RegisterPacket<StanceSyncPacket>(OnStancePacketReceivedClient);
            }
            if (Singleton<FikaServer>.Instance != null)
            {
                Singleton<FikaServer>.Instance.RegisterPacket<StanceSyncPacket>(OnStancePacketReceivedServer);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SendStanceUpdate(string profileId, Stance stance, bool isMounting)
        {
            if (!IsFikaLoaded) return;

            var packet = new StanceSyncPacket
            {
                ProfileId = profileId,
                StanceInt = (int)stance,
                IsMounting = isMounting
            };

            if (Singleton<FikaClient>.Instance != null)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
            else if (Singleton<FikaServer>.Instance != null)
            {
                Singleton<FikaServer>.Instance.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, broadcast: true);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OnStancePacketReceivedClient(StanceSyncPacket packet)
        {
            ApplyStanceToPlayer(packet.ProfileId, (Stance)packet.StanceInt, packet.IsMounting);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void OnStancePacketReceivedServer(StanceSyncPacket packet)
        {
            // Server receives and broadcasts to others
            Singleton<FikaServer>.Instance.SendData(ref packet, Fika.Core.Networking.LiteNetLib.DeliveryMethod.ReliableOrdered, broadcast: true);
            ApplyStanceToPlayer(packet.ProfileId, (Stance)packet.StanceInt, packet.IsMounting);
        }

        private static void ApplyStanceToPlayer(string profileId, Stance stance, bool isMounting)
        {
            var gameWorld = StanceManager.GetCachedGameWorld();
            if (gameWorld == null) return;

            // Encontrar jogador
            EFT.Player targetPlayer = null;
            if (gameWorld.RegisteredPlayers != null)
            {
                foreach (var p in gameWorld.RegisteredPlayers)
                {
                    if (p.ProfileId == profileId)
                    {
                        targetPlayer = p as EFT.Player;
                        break;
                    }
                }
            }

            if (targetPlayer == null || targetPlayer.IsYourPlayer) return;

            var controller = targetPlayer.gameObject.GetComponent<PlayerStanceController>();
            if (controller == null)
            {
                controller = targetPlayer.gameObject.AddComponent<PlayerStanceController>();
                controller.Init(targetPlayer);
            }

            controller.UpdateStanceFromNetwork(stance, isMounting);
        }
    }
}
