using System;
using System.Collections.Generic;
using BitPacking;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.MovingPlatforms;

public abstract class GClass1712
{
	public const int LOW_PRIORITY_LIMIT = 5;

	[NonSerialized]
	public const int Int_0 = 32;

	[NonSerialized]
	public const int Int_1 = 32;

	[NonSerialized]
	public const int Int_2 = 64;

	[NonSerialized]
	public const int Int_3 = 32;

	public static void UpdatePriority(ref GStruct164 packet, GStruct164 previousPacket)
	{
		if (!packet.InteractiveObjectsStatusPacket.HasValue && !packet.UpdateExfiltrationPointPacket.HasValue && !packet.LampChangeStatePacket.HasValue && !packet.WindowHitPacket.HasValue && !smethod_0(packet.GrenadeSyncPackets) && !smethod_1(packet.CorpseSyncPackets) && !smethod_2(packet.LootSyncPackets) && packet.ArtilleryShellSyncPackets.Count <= 0 && packet.BorderZonePackets.Count <= 0)
		{
			packet.PriorityCounter = previousPacket.PriorityCounter + 1;
			if (packet.PriorityCounter >= 5)
			{
				packet.PriorityCounter = 0;
			}
		}
		else
		{
			packet.PriorityCounter = 0;
		}
	}

	public static bool smethod_0(this IReadOnlyList<GrenadeDataPacketStruct> list)
	{
		if (list != null && list.Count != 0)
		{
			int num = 0;
			while (true)
			{
				if (num < list.Count)
				{
					if (list[num].Done)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool smethod_1(this IReadOnlyList<RagdollPacketStruct> list)
	{
		if (list != null && list.Count != 0)
		{
			int num = 0;
			while (true)
			{
				if (num < list.Count)
				{
					if (list[num].Done)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool smethod_2(this IReadOnlyList<LootSyncStruct> list)
	{
		if (list != null && list.Count != 0)
		{
			int num = 0;
			while (true)
			{
				if (num < list.Count)
				{
					if (list[num].Done)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static void SerializePacket(this GInterface131 writer, ref GStruct164 packet, GameWorld gameWorld)
	{
		writer.Write(packet.RemoteTime);
		smethod_3(writer, ref packet.InteractiveObjectsStatusPacket);
		smethod_5(writer, ref packet.UpdateExfiltrationPointPacket);
		smethod_7(writer, ref packet.LampChangeStatePacket);
		smethod_9(writer, ref packet.WindowHitPacket);
		smethod_11(writer, packet.LootSyncPackets);
		smethod_13(writer, packet.CorpseSyncPackets);
		smethod_15(writer, packet.GrenadeSyncPackets);
		SerializePackets(writer, packet.ArtilleryShellSyncPackets);
		smethod_18(writer, packet.PlatformSyncPackets, gameWorld.PlatformAdapters);
		smethod_20(writer, packet.BorderZonePackets, gameWorld.BorderZones);
		smethod_22(writer, packet.SynchronizableObjectPackets);
	}

	public static GStruct164 DeserializeGameWorldPacket(this IDataReader reader)
	{
		GStruct164 result = GStruct164.Create();
		result.RemoteTime = reader.ReadFloat();
		result.InteractiveObjectsStatusPacket = smethod_4(reader);
		result.UpdateExfiltrationPointPacket = smethod_6(reader);
		result.LampChangeStatePacket = smethod_8(reader);
		result.WindowHitPacket = smethod_10(reader);
		smethod_12(reader, result.LootSyncPackets);
		smethod_14(reader, result.CorpseSyncPackets);
		smethod_16(reader, result.GrenadeSyncPackets);
		DeserializeArtillerySyncPackets(reader, result.ArtilleryShellSyncPackets);
		smethod_19(reader, result.PlatformSyncPackets);
		smethod_21(reader, result.BorderZonePackets);
		smethod_22(reader, result.SynchronizableObjectPackets);
		return result;
	}

	public static void smethod_3(this GInterface131 writer, ref GStruct165? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			ArraySegment<byte> segment = packet.GetValueOrDefault().Segment;
			writer.Write((short)segment.Count);
			writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
		}
	}

	public static GStruct165? smethod_4(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		short num = reader.ReadInt16();
		GClass1369.CheckMaxSize(num);
		byte[] array = new byte[num];
		reader.ReadBytes(array, 0, num);
		return new GStruct165
		{
			Segment = new ArraySegment<byte>(array)
		};
	}

	public static void smethod_5(this GInterface131 writer, ref GStruct166? packet)
	{
		writer.Write(packet.HasValue);
		if (!packet.HasValue)
		{
			return;
		}
		GStruct166 valueOrDefault = packet.GetValueOrDefault();
		writer.Write(valueOrDefault.PointName);
		writer.Write((byte)valueOrDefault.Command);
		writer.Write((byte)valueOrDefault.QueuedPlayers.Count);
		foreach (string queuedPlayer in valueOrDefault.QueuedPlayers)
		{
			writer.Write(queuedPlayer);
		}
		writer.Write(valueOrDefault.ItemTransferred);
		GClass2084.Serialize(writer, valueOrDefault.Nested, smethod_5);
	}

	public static GStruct166? smethod_6(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		string pointName = reader.ReadString(1600u);
		EExfiltrationStatus command = (EExfiltrationStatus)reader.ReadByte();
		byte b = reader.ReadByte();
		List<string> list = new List<string>();
		for (int i = 0; i < b; i++)
		{
			list.Add(reader.ReadString(1600u));
		}
		bool itemTransferred = reader.ReadBool();
		GInterface217<GStruct166> nested = GClass2084.Deserialize(reader, smethod_6);
		return new GStruct166
		{
			PointName = pointName,
			Command = command,
			QueuedPlayers = list,
			ItemTransferred = itemTransferred,
			Nested = nested
		};
	}

	public static void smethod_7(this GInterface131 writer, ref GStruct167? packet)
	{
		writer.Write(packet.HasValue);
		if (packet.HasValue)
		{
			GStruct167 valueOrDefault = packet.GetValueOrDefault();
			writer.Write(valueOrDefault.NetId);
			writer.Write((byte)valueOrDefault.State);
			GClass2084.Serialize(writer, valueOrDefault.Nested, smethod_7);
		}
	}

	public static GStruct167? smethod_8(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct167
		{
			NetId = reader.ReadInt32(),
			State = (Turnable.EState)reader.ReadByte(),
			Nested = GClass2084.Deserialize(reader, smethod_8)
		};
	}

	public static void smethod_9(this GInterface131 writer, ref GStruct168? packet)
	{
		bool value = packet.HasValue;
		writer.Serialize(ref value);
		if (value)
		{
			GStruct168 valueOrDefault = packet.GetValueOrDefault();
			writer.Serialize(ref valueOrDefault.NetId);
			writer.Serialize(ref valueOrDefault.HitPosition);
			GClass2084.Serialize(writer, valueOrDefault.Nested, smethod_9);
		}
	}

	public static GStruct168? smethod_10(this IDataReader reader)
	{
		if (!reader.ReadBool())
		{
			return null;
		}
		return new GStruct168
		{
			NetId = reader.ReadInt32(),
			HitPosition = reader.ReadVector3(),
			Nested = GClass2084.Deserialize(reader, smethod_10)
		};
	}

	public static void smethod_11(this GInterface131 writer, List<LootSyncStruct> packets)
	{
		bool flag = packets.Count > 0;
		writer.Write(flag);
		if (flag)
		{
			int num = packets.Count;
			if (num > 64)
			{
				num = 64;
			}
			writer.WriteLimitedInt32(num, 1, 64, BitPackingTag.GameWorldPacketExtension1);
			for (int i = 0; i < num; i++)
			{
				LootSyncStruct lootSyncStruct = packets[i];
				writer.Write(lootSyncStruct.Id);
				lootSyncStruct.Serialize(writer);
			}
		}
	}

	public static void smethod_12(this IDataReader reader, List<LootSyncStruct> packets)
	{
		if (reader.ReadBool())
		{
			int num = reader.ReadLimitedInt32(1, 64);
			for (int i = 0; i < num; i++)
			{
				LootSyncStruct item = default(LootSyncStruct);
				item.Id = reader.ReadInt32();
				item.Deserialize(reader);
				packets.Add(item);
			}
		}
	}

	public static void smethod_13(this GInterface131 writer, List<RagdollPacketStruct> packets)
	{
		bool flag = packets.Count > 0;
		writer.Write(flag);
		if (!flag)
		{
			return;
		}
		writer.WriteLimitedInt32(packets.Count, 1, 32, BitPackingTag.GameWorldPacketExtension2);
		foreach (RagdollPacketStruct packet in packets)
		{
			writer.Write(packet.Id);
			packet.Serialize(writer);
		}
	}

	public static void smethod_14(this IDataReader reader, List<RagdollPacketStruct> packets)
	{
		if (reader.ReadBool())
		{
			int num = reader.ReadLimitedInt32(1, 32);
			for (int i = 0; i < num; i++)
			{
				RagdollPacketStruct item = default(RagdollPacketStruct);
				item.Id = reader.ReadInt32();
				item.Deserialize(reader);
				packets.Add(item);
			}
		}
	}

	public static void smethod_15(this GInterface131 writer, List<GrenadeDataPacketStruct> packets)
	{
		bool flag = packets.Count > 0;
		writer.Write(flag);
		if (flag)
		{
			int num = packets.Count;
			if (num > 32)
			{
				num = 32;
			}
			writer.WriteLimitedInt32(num, 1, 32, BitPackingTag.GameWorldPacketExtension3);
			for (int i = 0; i < num; i++)
			{
				GrenadeDataPacketStruct grenadeDataPacketStruct = packets[i];
				writer.Write(grenadeDataPacketStruct.Id);
				grenadeDataPacketStruct.Serialize(writer);
			}
		}
	}

	public static void smethod_16(this IDataReader reader, List<GrenadeDataPacketStruct> packets)
	{
		if (reader.ReadBool())
		{
			int num = reader.ReadLimitedInt32(1, 32);
			for (int i = 0; i < num; i++)
			{
				GrenadeDataPacketStruct item = default(GrenadeDataPacketStruct);
				item.Id = reader.ReadInt32();
				item.Deserialize(reader);
				packets.Add(item);
			}
		}
	}

	public static void SerializePackets(this GInterface131 writer, List<ArtilleryPacketStruct> packets)
	{
		bool flag = packets.Count > 0;
		writer.Write(flag);
		if (flag)
		{
			int num = packets.Count;
			if (num > 32)
			{
				num = 32;
			}
			writer.WriteLimitedInt32(num, 1, 32, BitPackingTag.GameWorldPacketExtension3);
			for (int i = 0; i < num; i++)
			{
				ArtilleryPacketStruct artilleryPacketStruct = packets[i];
				writer.Write(artilleryPacketStruct.id);
				artilleryPacketStruct.Serialize(writer);
			}
		}
	}

	public static void DeserializeArtillerySyncPackets(this IDataReader reader, List<ArtilleryPacketStruct> packets)
	{
		if (reader.ReadBool())
		{
			int num = reader.ReadLimitedInt32(1, 32);
			for (int i = 0; i < num; i++)
			{
				ArtilleryPacketStruct item = default(ArtilleryPacketStruct);
				item.id = reader.ReadInt32();
				item.Deserialize(reader);
				packets.Add(item);
			}
		}
	}

	public static bool smethod_17(List<ArtilleryPacketStruct> packets, int id, out ArtilleryPacketStruct found)
	{
		int num = 0;
		ArtilleryPacketStruct artilleryPacketStruct;
		while (true)
		{
			if (num < packets.Count)
			{
				artilleryPacketStruct = packets[num];
				if (artilleryPacketStruct.id == id)
				{
					break;
				}
				num++;
				continue;
			}
			found = default(ArtilleryPacketStruct);
			return false;
		}
		found = artilleryPacketStruct;
		return true;
	}

	public static void smethod_18(this GInterface131 writer, List<GStruct173> packets, MovingPlatform.GClass3596[] platformAdapters)
	{
		if (packets.Count > 0)
		{
			writer.Write(value: true);
			writer.WriteLimitedInt32(packets.Count, 0, platformAdapters.Length, BitPackingTag.GameWorldPacketExtension4);
			{
				foreach (GStruct173 packet in packets)
				{
					writer.Write(packet.Id);
					writer.Write(packet.Position);
				}
				return;
			}
		}
		writer.Write(value: false);
	}

	public static void smethod_19(this IDataReader reader, List<GStruct173> packets)
	{
		if (reader.ReadBool())
		{
			int num = reader.ReadLimitedInt32(0, Singleton<GameWorld>.Instance.PlatformAdapters.Length);
			for (int i = 0; i < num; i++)
			{
				packets.Add(new GStruct173
				{
					Id = reader.ReadByte(),
					Position = reader.ReadFloat()
				});
			}
		}
	}

	public static void smethod_20(this GInterface131 writer, List<GStruct174> packets, BorderZone[] borderZones)
	{
		if (packets.Count > 0)
		{
			writer.Write(value: true);
			writer.WriteLimitedInt32(packets.Count, 0, borderZones.Length, BitPackingTag.GameWorldPacketExtension4);
			{
				foreach (GStruct174 packet in packets)
				{
					writer.WriteLimitedInt32(packet.Id, 0, borderZones.Length, BitPackingTag.BorderZonePacketPacketId);
					writer.Write(packet.PlayerId);
					writer.Write(packet.WillHit);
				}
				return;
			}
		}
		writer.Write(value: false);
	}

	public static void smethod_21(this IDataReader reader, List<GStruct174> packets)
	{
		if (reader.ReadBool())
		{
			int num = reader.ReadLimitedInt32(0, Singleton<GameWorld>.Instance.BorderZones.Length);
			for (int i = 0; i < num; i++)
			{
				packets.Add(new GStruct174
				{
					Id = reader.ReadLimitedInt32(0, Singleton<GameWorld>.Instance.BorderZones.Length),
					PlayerId = reader.ReadInt32(),
					WillHit = reader.ReadBool()
				});
			}
		}
	}

	public static void smethod_22(this ISerializer stream, List<AirplaneDataPacketStruct> packets)
	{
		int value = packets?.Count ?? 0;
		bool value2 = value > 0;
		stream.Serialize(ref value2);
		if (!value2)
		{
			return;
		}
		stream.SerializeLimitedInt32(ref value, 1, 64, BitPackingTag.GameWorldPacketSyncObjectsPacketsCount);
		if (stream.StreamMode == EBitStreamMode.Writing)
		{
			for (int i = 0; i < value; i++)
			{
				AirplaneDataPacketStruct packet = packets[i];
				GClass793.Serialize(stream, ref packet);
			}
			return;
		}
		for (int j = 0; j < value; j++)
		{
			AirplaneDataPacketStruct packet2 = default(AirplaneDataPacketStruct);
			GClass793.Serialize(stream, ref packet2);
			packets.Add(packet2);
		}
	}
}
