using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.NetworkPackets;
using UnityEngine;

public struct GStruct210
{
	public const int INVENTORY_COMMANDS_BITS_COUNT = 8;

	public const int MAX_NON_CRITICAL_PACKETS_IN_ROW = 5;

	public const int MAX_FRAMES_BETWEEN_CRITICAL_PACKETS = 6;

	public const int MAX_PACKETS_QUEUE = 127;

	[NonSerialized]
	public const int Int_0 = 3;

	public static GStruct210 DEFAULT_CLIENT2_SERVER_PACKET = new GStruct210
	{
		FrameId = 0uL
	};

	[NonSerialized]
	public const int Int_1 = 120;

	public const float LOWEST_USUAL_DELTA_TIME = 1f / 120f;

	[NonSerialized]
	public const float Float_0 = 0.00048828125f;

	[NonSerialized]
	public const float Float_1 = 0.1f;

	[NonSerialized]
	public static FloatQuantizerStruct FloatQuantizerStruct = new FloatQuantizerStruct(0f, 6f, 0.00048828125f, checkBounds: true);

	[NonSerialized]
	public static FloatQuantizerStruct FloatQuantizerStruct_1 = new FloatQuantizerStruct(0f, 6f, 4.8828126E-05f, checkBounds: true);

	public bool HasCriticalData;

	public ushort? RTT;

	public bool IsExtraPrecisionMovement;

	public float TempDeltaTime;

	public float ClientTime;

	public float DeltaTimeFromLastCriticalPacket;

	public bool SyncSkills;

	public MovementInfoPacketStruct MovementInfoPacket;

	public GStruct234 HandsChangePacket;

	public List<GStruct252> InventoryCommandPackets;

	public GStruct185? LootRayInfo;

	public EHandsTypePacket HandsTypePacket;

	public GStruct243 FirearmPacket;

	public GrenadePacketStruct GrenadePacket;

	public GStruct380 KnifePacket;

	public GStruct383 PhraseCommandPacket;

	public GStruct371 EmptyHandPacket;

	public GStruct391 UsableItemPacket;

	public GStruct229 TripwireSoundInteractionPacket;

	public Player.EVoipState VoipState;

	public bool IsTalked;

	public bool? IsVoiceMuffled;

	public bool? IsUnderRoof;

	public GStruct268 ViewPacket;

	public GStruct184? CancelApplyingItemPacket;

	public GStruct183? StopSearchContentPacket;

	public GStruct186? CutsceneEndedPacket;

	public GStruct237? HelmetLightPacket;

	public GStruct187? DevelopHealPacket;

	public GStruct188? DevelopTeleportPacket;

	public GStruct189? DevelopSetDamageCoeffPacket;

	public GStruct192? DevelopContainerHidePacket;

	public GStruct193? DevelopEnableServerHitDebuggingPacket;

	public GStruct200? DevelopKillMePacket;

	public GStruct194? DevelopKillAllAIs;

	public GStruct195? DevelopSpawnAI;

	public GStruct196? DevelopStartEvent;

	public GStruct198? DevelopSnapshotAllPlayers;

	public GStruct199? DevelopAirdrop;

	public GStruct197? DevelopUnlockAllDoors;

	public GStruct201? DevelopResetDiscardLimits;

	public GStruct203? DevelopResetBufferZoneUsageTime;

	public GStruct202? DevelopSetBufferZoneAccess;

	public GStruct204? DevelopSetEncodedRadioTransmitter;

	public GStruct205? DevelopSetActiveLighthouseTraderZoneDebug;

	public GStruct190? DevelopLighthouseKeeperServicesDebug;

	public GStruct191? DevelopBtrSupportServiceDebug;

	public GStruct206 PacketAuxiliaryData;

	public ulong FrameId;

	[NonSerialized]
	public const int Int_2 = 15;

	public bool HasImportantData
	{
		get
		{
			if (!MovementInfoPacket.HasImportantData && HandsChangePacket.OperationType == GStruct234.EOperationType.None && (InventoryCommandPackets == null || InventoryCommandPackets.Count <= 0) && !HasCommandsForHands && !PhraseCommandPacket.HasPhrase && !CancelApplyingItemPacket.HasValue && !StopSearchContentPacket.HasValue && !CutsceneEndedPacket.HasValue && !DevelopHealPacket.HasValue && !DevelopTeleportPacket.HasValue && !DevelopSetDamageCoeffPacket.HasValue && !DevelopContainerHidePacket.HasValue && !DevelopSetBufferZoneAccess.HasValue && !DevelopResetBufferZoneUsageTime.HasValue && !DevelopEnableServerHitDebuggingPacket.HasValue && !DevelopKillMePacket.HasValue && !DevelopSpawnAI.HasValue && !DevelopStartEvent.HasValue && !DevelopKillAllAIs.HasValue && !DevelopUnlockAllDoors.HasValue && !DevelopSnapshotAllPlayers.HasValue && !DevelopAirdrop.HasValue && !DevelopResetDiscardLimits.HasValue && !DevelopSetEncodedRadioTransmitter.HasValue && !DevelopSetActiveLighthouseTraderZoneDebug.HasValue && !SyncSkills && !HelmetLightPacket.HasValue && !IsVoiceMuffled.HasValue)
			{
				return IsUnderRoof.HasValue;
			}
			return true;
		}
	}

	public bool HasCommandsForHands => HandsTypePacket switch
	{
		EHandsTypePacket.None => false, 
		EHandsTypePacket.Firearm => FirearmPacket.Boolean_0, 
		EHandsTypePacket.Grenade => GrenadePacket.HasCommandsForHands, 
		EHandsTypePacket.Knife => KnifePacket.HasCommandsForHands, 
		EHandsTypePacket.EmptyHand => EmptyHandPacket.HasCommandsForHands, 
		EHandsTypePacket.UsableItem => UsableItemPacket.HasCommandsForHands, 
		_ => false, 
	};

	public static void SerializeDiffUsing(GInterface131 writer, ref GStruct210 current, GStruct210 prevFrame)
	{
		writer.Write(current.ClientTime);
		writer.Write(current.HasCriticalData);
		writer.Write(current.RTT.HasValue);
		if (current.RTT.HasValue)
		{
			writer.Write(current.RTT.Value);
		}
		writer.Write(current.SyncSkills);
		writer.Write(current.IsVoiceMuffled.HasValue);
		if (current.IsVoiceMuffled.HasValue)
		{
			writer.Write(current.IsVoiceMuffled.Value);
		}
		writer.Write(current.IsUnderRoof.HasValue);
		if (current.IsUnderRoof.HasValue)
		{
			writer.Write(current.IsUnderRoof.Value);
		}
		writer.Write(current.IsExtraPrecisionMovement);
		if (current.IsExtraPrecisionMovement)
		{
			current.DeltaTimeFromLastCriticalPacket = GClass1370.Write(FloatQuantizerStruct_1, writer, current.DeltaTimeFromLastCriticalPacket);
		}
		else
		{
			current.DeltaTimeFromLastCriticalPacket = GClass1370.Write(FloatQuantizerStruct, writer, current.DeltaTimeFromLastCriticalPacket);
		}
		MovementInfoPacketStruct.SerializeDiffUsing(writer, ref current.MovementInfoPacket, prevFrame.MovementInfoPacket, current.IsExtraPrecisionMovement);
		GStruct234.Serialize(writer, ref current.HandsChangePacket, prevFrame.HandsChangePacket);
		int num = ((current.InventoryCommandPackets != null) ? current.InventoryCommandPackets.Count : 0);
		writer.WriteBits((uint)num, 8);
		for (int i = 0; i < num; i++)
		{
			current.InventoryCommandPackets[i].Serialize(writer);
		}
		bool hasCommandsForHands = current.HasCommandsForHands;
		writer.Write(hasCommandsForHands);
		if (hasCommandsForHands)
		{
			writer.Serialize(ref current.HandsTypePacket);
			if (current.HandsTypePacket == EHandsTypePacket.Firearm)
			{
				GStruct243.SerializeDiffUsing(writer, current.FirearmPacket, prevFrame.FirearmPacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.Grenade)
			{
				GClass3000.Serialize(writer, ref current.GrenadePacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.Knife)
			{
				GClass2990.Serialize(writer, ref current.KnifePacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.EmptyHand)
			{
				GClass2992.Serialize(writer, ref current.EmptyHandPacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.UsableItem)
			{
				GClass2991.Serialize(writer, ref current.UsableItemPacket);
			}
		}
		GClass2997.Serialize(writer, ref current.PhraseCommandPacket);
		writer.Serialize(ref current.VoipState);
		writer.Serialize(ref current.IsTalked);
		GStruct268.SerializeDiffUsing(writer, ref current.ViewPacket);
		bool flag = current.CancelApplyingItemPacket.HasValue || current.StopSearchContentPacket.HasValue || current.HelmetLightPacket.HasValue;
		writer.Write(flag);
		if (flag)
		{
			GClass2084.SerializePacket(writer, ref current.CancelApplyingItemPacket);
			GClass2084.SerializePacket(writer, ref current.StopSearchContentPacket);
			GClass2084.SerializePacket(writer, ref current.HelmetLightPacket);
		}
		flag = current.CutsceneEndedPacket.HasValue;
		writer.Write(flag);
		if (flag)
		{
			GClass2084.SerializePacket(writer, ref current.CutsceneEndedPacket);
		}
		flag = current.LootRayInfo.HasValue;
		writer.Write(flag);
		if (flag)
		{
			GClass2084.SerializePacket(writer, ref current.LootRayInfo);
		}
		current.TripwireSoundInteractionPacket.Serialize(writer);
		flag = current.DevelopHealPacket.HasValue || current.DevelopTeleportPacket.HasValue || current.DevelopSetDamageCoeffPacket.HasValue || current.DevelopContainerHidePacket.HasValue || current.DevelopEnableServerHitDebuggingPacket.HasValue || current.DevelopKillAllAIs.HasValue || current.DevelopSpawnAI.HasValue || current.DevelopStartEvent.HasValue || current.DevelopKillMePacket.HasValue || current.DevelopUnlockAllDoors.HasValue || current.DevelopSnapshotAllPlayers.HasValue || current.DevelopAirdrop.HasValue || current.DevelopResetDiscardLimits.HasValue || current.DevelopSetBufferZoneAccess.HasValue || current.DevelopResetBufferZoneUsageTime.HasValue || current.DevelopSetEncodedRadioTransmitter.HasValue || current.DevelopSetActiveLighthouseTraderZoneDebug.HasValue || current.DevelopLighthouseKeeperServicesDebug.HasValue || current.DevelopBtrSupportServiceDebug.HasValue;
		writer.Write(flag);
		if (flag)
		{
			GClass2084.SerializePacket(writer, ref current.DevelopHealPacket);
			GClass2084.SerializePacket(writer, ref current.DevelopTeleportPacket);
			GClass2084.SerializePacket(writer, ref current.DevelopSetDamageCoeffPacket);
			GClass2084.SerializePacket(writer, ref current.DevelopContainerHidePacket);
			GClass2084.SerializePacket(writer, ref current.DevelopEnableServerHitDebuggingPacket);
			GClass2084.SerializePacket(writer, ref current.DevelopKillMePacket);
			GClass2084.SerializePacket(writer, ref current.DevelopKillAllAIs);
			GClass2084.SerializePacket(writer, ref current.DevelopSpawnAI);
			GClass2084.SerializePacket(writer, ref current.DevelopStartEvent);
			GClass2084.SerializePacket(writer, ref current.DevelopUnlockAllDoors);
			GClass2084.SerializePacket(writer, ref current.DevelopSnapshotAllPlayers);
			GClass2084.SerializePacket(writer, ref current.DevelopResetDiscardLimits);
			GClass2084.SerializePacket(writer, ref current.DevelopSetEncodedRadioTransmitter);
			GClass2084.SerializePacket(writer, ref current.DevelopSetActiveLighthouseTraderZoneDebug);
			GClass2084.SerializePacket(writer, ref current.DevelopAirdrop);
			GClass2084.SerializePacket(writer, ref current.DevelopSetBufferZoneAccess);
			GClass2084.SerializePacket(writer, ref current.DevelopResetBufferZoneUsageTime);
			GClass2084.SerializePacket(writer, ref current.DevelopLighthouseKeeperServicesDebug);
			GClass2084.SerializePacket(writer, ref current.DevelopBtrSupportServiceDebug);
		}
	}

	public static bool DeserializeDiffUsing(IDataReader reader, ref GStruct210 current, GStruct210 prevFrame)
	{
		current.ClientTime = reader.ReadFloat();
		current.HasCriticalData = reader.ReadBool();
		if (reader.ReadBool())
		{
			current.RTT = reader.ReadUInt16();
		}
		current.SyncSkills = reader.ReadBool();
		if (reader.ReadBool())
		{
			current.IsVoiceMuffled = reader.ReadBool();
		}
		if (reader.ReadBool())
		{
			current.IsUnderRoof = reader.ReadBool();
		}
		current.IsExtraPrecisionMovement = reader.ReadBool();
		if (current.IsExtraPrecisionMovement)
		{
			GClass1370.Read(FloatQuantizerStruct_1, reader, out current.DeltaTimeFromLastCriticalPacket);
		}
		else
		{
			GClass1370.Read(FloatQuantizerStruct, reader, out current.DeltaTimeFromLastCriticalPacket);
		}
		MovementInfoPacketStruct.DeserializeDiffUsing(reader, ref current.MovementInfoPacket, prevFrame.MovementInfoPacket, current.IsExtraPrecisionMovement);
		GStruct234.Serialize(reader, ref current.HandsChangePacket, prevFrame.HandsChangePacket);
		int num = (int)reader.ReadBits(8);
		if (num > 0)
		{
			current.InventoryCommandPackets = new List<GStruct252>(num + 1);
			for (int i = 0; i < num; i++)
			{
				GStruct252 item = default(GStruct252);
				item.Deserialize(reader);
				current.InventoryCommandPackets.Add(item);
			}
		}
		if (reader.ReadBool())
		{
			reader.Serialize(ref current.HandsTypePacket);
			if (current.HandsTypePacket == EHandsTypePacket.Firearm)
			{
				GStruct243.DeserializeDiffUsing(reader, ref current.FirearmPacket, prevFrame.FirearmPacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.Grenade)
			{
				GClass3000.Serialize(reader, ref current.GrenadePacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.Knife)
			{
				GClass2990.Serialize(reader, ref current.KnifePacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.EmptyHand)
			{
				GClass2992.Serialize(reader, ref current.EmptyHandPacket);
			}
			else if (current.HandsTypePacket == EHandsTypePacket.UsableItem)
			{
				GClass2991.Serialize(reader, ref current.UsableItemPacket);
			}
		}
		GClass2997.Serialize(reader, ref current.PhraseCommandPacket);
		reader.Serialize(ref current.VoipState);
		reader.Serialize(ref current.IsTalked);
		GStruct268.DeserializeDiffUsing(reader, ref current.ViewPacket);
		if (reader.ReadBool())
		{
			GClass2084.DeserializePacket(reader, ref current.CancelApplyingItemPacket);
			GClass2084.DeserializePacket(reader, ref current.StopSearchContentPacket);
			GClass2084.DeserializePacket(reader, ref current.HelmetLightPacket);
		}
		if (reader.ReadBool())
		{
			GClass2084.DeserializePacket(reader, ref current.DevelopHealPacket);
			GClass2084.DeserializePacket(reader, ref current.DevelopTeleportPacket);
			GClass2084.DeserializePacket(reader, ref current.DevelopContainerHidePacket);
			GClass2084.DeserializePacket(reader, ref current.DevelopEnableServerHitDebuggingPacket);
			GClass2084.DeserializePacket(reader, ref current.DevelopKillMePacket);
			GClass2084.DeserializePacket(reader, ref current.DevelopKillAllAIs);
			GClass2084.DeserializePacket(reader, ref current.DevelopSpawnAI);
			GClass2084.DeserializePacket(reader, ref current.DevelopStartEvent);
			GClass2084.DeserializePacket(reader, ref current.DevelopUnlockAllDoors);
			GClass2084.DeserializePacket(reader, ref current.DevelopSnapshotAllPlayers);
			GClass2084.DeserializePacket(reader, ref current.DevelopResetDiscardLimits);
			GClass2084.DeserializePacket(reader, ref current.DevelopSetEncodedRadioTransmitter);
			GClass2084.DeserializePacket(reader, ref current.DevelopSetActiveLighthouseTraderZoneDebug);
			GClass2084.DeserializePacket(reader, ref current.DevelopAirdrop);
			GClass2084.DeserializePacket(reader, ref current.DevelopSetBufferZoneAccess);
			GClass2084.DeserializePacket(reader, ref current.DevelopResetBufferZoneUsageTime);
			GClass2084.DeserializePacket(reader, ref current.DevelopLighthouseKeeperServicesDebug);
			GClass2084.DeserializePacket(reader, ref current.DevelopBtrSupportServiceDebug);
		}
		return true;
	}

	public void Set(MovementInfoPacketStruct movementInfoPacket)
	{
		MovementInfoPacket = movementInfoPacket;
	}

	public void AddInventoryCommand(GStruct252 inventoryCommandPacket)
	{
		if (InventoryCommandPackets == null)
		{
			InventoryCommandPackets = new List<GStruct252>(3);
		}
		InventoryCommandPackets.Add(inventoryCommandPacket);
	}

	public void AddInventoryCommand(GStruct252 inventoryCommandPacket, Vector3 origin, Vector3 direction)
	{
		AddInventoryCommand(inventoryCommandPacket);
		if (!LootRayInfo.HasValue)
		{
			LootRayInfo = new GStruct185
			{
				origin = origin,
				direction = direction
			};
		}
	}

	public void AddInventoryCommand(Vector3 origin, Vector3 direction)
	{
		if (!LootRayInfo.HasValue)
		{
			LootRayInfo = new GStruct185
			{
				origin = origin,
				direction = direction
			};
		}
	}

	public void Set(GStruct243 firearmPacket)
	{
		if (HandsTypePacket != EHandsTypePacket.None && HandsTypePacket != EHandsTypePacket.Firearm)
		{
			Debug.LogErrorFormat("Expected None or FirearmPacket, actual == {0}", HandsTypePacket);
		}
		HandsTypePacket = EHandsTypePacket.Firearm;
		FirearmPacket = firearmPacket;
	}

	public void Set(GrenadePacketStruct grenadePacket)
	{
		if (HandsTypePacket != EHandsTypePacket.None && HandsTypePacket != EHandsTypePacket.Grenade)
		{
			Debug.LogErrorFormat("Expected None or Grenade, actual == {0}", HandsTypePacket);
		}
		HandsTypePacket = EHandsTypePacket.Grenade;
		GrenadePacket = grenadePacket;
	}

	public void Set(GStruct380 knifePacket)
	{
		if (HandsTypePacket != EHandsTypePacket.None && HandsTypePacket != EHandsTypePacket.Knife)
		{
			Debug.LogErrorFormat("Expected None or Knife, actual == {0}", HandsTypePacket);
		}
		HandsTypePacket = EHandsTypePacket.Knife;
		KnifePacket = knifePacket;
	}

	public void Set(GStruct371 emptyHandPacket)
	{
		if (HandsTypePacket != EHandsTypePacket.None && HandsTypePacket != EHandsTypePacket.EmptyHand)
		{
			Debug.LogErrorFormat("Expected None or EmptyHand, actual == {0}", HandsTypePacket);
		}
		HandsTypePacket = EHandsTypePacket.EmptyHand;
		EmptyHandPacket = emptyHandPacket;
	}

	public void Set(GStruct391 usableItemPacket)
	{
		if (HandsTypePacket != EHandsTypePacket.None && HandsTypePacket != EHandsTypePacket.UsableItem)
		{
			Debug.LogErrorFormat("Expected None or UsableItem, actual == {0}", HandsTypePacket);
		}
		HandsTypePacket = EHandsTypePacket.UsableItem;
		UsableItemPacket = usableItemPacket;
	}

	public void Set(GStruct234 handsChangePacket)
	{
		HandsChangePacket = handsChangePacket;
	}

	public void SetPhraseCommand(EPhraseTrigger phraseCommand, int netPhraseId)
	{
		if (!NetworkPlayer.LocalPhrases.Contains(phraseCommand))
		{
			PhraseCommandPacket = new GStruct383
			{
				HasPhrase = true,
				PhraseCommand = phraseCommand,
				PhraseId = netPhraseId
			};
		}
	}

	public void ClearNonStaticData()
	{
		MovementInfoPacketStruct movementInfoPacket = DEFAULT_CLIENT2_SERVER_PACKET.MovementInfoPacket;
		movementInfoPacket.EPlayerState = MovementInfoPacket.EPlayerState;
		movementInfoPacket.AnimatorStateIndex = MovementInfoPacket.AnimatorStateIndex;
		movementInfoPacket.PoseLevel = MovementInfoPacket.PoseLevel;
		movementInfoPacket.CharacterMovementSpeed = MovementInfoPacket.CharacterMovementSpeed;
		movementInfoPacket.Tilt = MovementInfoPacket.Tilt;
		movementInfoPacket.Step = MovementInfoPacket.Step;
		movementInfoPacket.BlindFire = MovementInfoPacket.BlindFire;
		movementInfoPacket.HeadRotation = MovementInfoPacket.HeadRotation;
		movementInfoPacket.Stamina = MovementInfoPacket.Stamina;
		movementInfoPacket.DiscreteDirection = MovementInfoPacket.DiscreteDirection;
		SyncSkills = false;
		MovementInfoPacket = movementInfoPacket;
		HandsChangePacket = DEFAULT_CLIENT2_SERVER_PACKET.HandsChangePacket;
		InventoryCommandPackets = DEFAULT_CLIENT2_SERVER_PACKET.InventoryCommandPackets;
		HandsTypePacket = DEFAULT_CLIENT2_SERVER_PACKET.HandsTypePacket;
		FirearmPacket = DEFAULT_CLIENT2_SERVER_PACKET.FirearmPacket;
		GrenadePacket = DEFAULT_CLIENT2_SERVER_PACKET.GrenadePacket;
		KnifePacket = DEFAULT_CLIENT2_SERVER_PACKET.KnifePacket;
		PhraseCommandPacket = DEFAULT_CLIENT2_SERVER_PACKET.PhraseCommandPacket;
		EmptyHandPacket = DEFAULT_CLIENT2_SERVER_PACKET.EmptyHandPacket;
		UsableItemPacket = DEFAULT_CLIENT2_SERVER_PACKET.UsableItemPacket;
		CancelApplyingItemPacket = null;
		StopSearchContentPacket = null;
		CutsceneEndedPacket = null;
		LootRayInfo = null;
		HelmetLightPacket = null;
		DevelopTeleportPacket = null;
		DevelopKillAllAIs = null;
		DevelopSpawnAI = null;
		DevelopStartEvent = null;
		DevelopSetDamageCoeffPacket = null;
		DevelopContainerHidePacket = null;
		DevelopEnableServerHitDebuggingPacket = null;
		DevelopKillMePacket = null;
		DevelopUnlockAllDoors = null;
		DevelopSnapshotAllPlayers = null;
		DevelopResetDiscardLimits = null;
		DevelopSetEncodedRadioTransmitter = null;
		DevelopSetActiveLighthouseTraderZoneDebug = null;
		DevelopAirdrop = null;
		DevelopSetBufferZoneAccess = null;
		DevelopResetBufferZoneUsageTime = null;
	}
}
