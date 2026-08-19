using System;
using Comfort.Common;
using EFT;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;
using VisceralCombat.Dismemberment.Classes.Packets;
using VisceralCombat.Ragdolls.Classes.Packets;

namespace VisceralCombat.Combined.Classes;

public static class VisceralNetworkUtils
{
	public static void SendDismemberment(Player player, Vector3 direction, EBodyPart bodyPartType, string bone, string capAssetName, string[] assetNames)
	{
		if (player == null) return;

		DismembermentPacket packet = new()
		{
			playerID = player.Id,
			Direction = direction,
			bodyPartType = bodyPartType,
			bone = bone,
			capAssetName = capAssetName,
			assetNames = assetNames
		};

		SendPacket(ref packet);
	}

	public static void SendLivingDismemberment(Player player, EBodyPart leg, Vector3 direction, string bone, string capAssetName, string[] assetNames)
	{
		if (player == null) return;

		LivingDismembermentPacket packet = new()
		{
			PlayerID = player.Id,
			Leg = leg,
			Direction = direction,
			Bone = bone,
			CapAssetName = capAssetName,
			AssetNames = assetNames
		};

		SendPacket(ref packet);
	}

	public static void SendRagdollSync(Player player, EBodyPart bodyPart, int randomChance)
	{
		if (player == null) return;

		RagdollSyncPacket packet = new()
		{
			PlayerID = player.Id,
			BodyPart = bodyPart,
			RandomChance = randomChance
		};

		SendPacket(ref packet);
	}

	private static void SendPacket<T>(ref T packet) where T : struct, INetSerializable
	{
		try
		{
			if (Singleton<FikaServer>.Instantiated && Singleton<FikaServer>.Instance != null)
			{
				// Host sends directly to all clients via ReliableOrdered
				Singleton<FikaServer>.Instance.SendData<T>(ref packet, DeliveryMethod.ReliableOrdered, false);
			}
			else if (Singleton<FikaClient>.Instantiated && Singleton<FikaClient>.Instance != null)
			{
				// Client sends to Host via ReliableOrdered (Host relays to other clients)
				Singleton<FikaClient>.Instance.SendData<T>(ref packet, DeliveryMethod.ReliableOrdered, false);
			}
		}
		catch (Exception ex)
		{
			QuickLogger.Log(ELogType.Error, $"[VisceralNetworkUtils] Failed to send {typeof(T).Name}: {ex.Message}");
		}
	}
}
