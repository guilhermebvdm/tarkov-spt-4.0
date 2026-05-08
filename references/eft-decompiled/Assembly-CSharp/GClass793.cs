using System;
using BitPacking;
using EFT.SynchronizableObjects;

public abstract class GClass793
{
	[NonSerialized]
	public static GClass1378 Gclass1378_0;

	public static void Serialize(this ISerializer stream, ref AirplaneDataPacketStruct packet)
	{
		stream.Serialize(ref packet.Develop);
		if (packet.Develop)
		{
			Serialize(stream, ref packet.PacketData.DevelopDataPacket);
			return;
		}
		stream.Serialize(ref packet.ObjectType);
		switch (packet.ObjectType)
		{
		case SynchronizableObjectType.AirDrop:
			Serialize(stream, ref packet.PacketData.AirdropDataPacket);
			break;
		case SynchronizableObjectType.AirPlane:
			Serialize(stream, ref packet.PacketData.AirplaneDataPacket);
			break;
		case SynchronizableObjectType.Tripwire:
			Serialize(stream, ref packet.PacketData.TripwireDataPacket);
			break;
		}
		stream.SerializeLimitedInt32(ref packet.ObjectId, 0, 127, BitPackingTag.SyncPacketObjectId);
		stream.Serialize(ref packet.Position);
		stream.Serialize(ref packet.Rotation);
		stream.Serialize(ref packet.ObjectType);
		stream.Serialize(ref packet.Outdated);
		stream.Serialize(ref packet.IsStatic);
		stream.Serialize(ref packet.IsActive);
	}

	public static void Serialize(this ISerializer stream, ref AirdropDataPacketStruct packet)
	{
		stream.Serialize(ref packet.SignalFire);
		stream.Serialize(ref packet.FallingStage);
		stream.Serialize(ref packet.AirdropType);
		stream.SerializeLimitedInt32(ref packet.UniqueId, 0, 15, BitPackingTag.Airdrop);
	}

	public static void Serialize(this ISerializer stream, ref GStruct43 packet)
	{
		stream.SerializeLimitedInt32(ref packet.AirplanePercent, 0, 100, BitPackingTag.Airplane);
	}

	public static void Serialize(this ISerializer stream, ref GStruct44 packet)
	{
		stream.SerializeLimitedInt32(ref packet.NextAirdropTimeRemaining, 0, 8191, BitPackingTag.DevelopSynchronizableData);
		stream.Serialize(ref packet.NextAirdropTimeByFlare);
	}

	public static void Serialize(this ISerializer stream, ref GStruct45 packet)
	{
		stream.Serialize(ref packet.State);
		stream.Serialize(ref packet.GrenadeTemplateId, 1600u);
		stream.Serialize(ref packet.GrenadeItemId, 1600u);
		stream.Serialize(ref packet.PlayerId, 1600u);
		stream.Serialize(ref packet.FromPosition);
		stream.Serialize(ref packet.ToPosition);
	}
}
