using System;
using BitPacking;

public struct GStruct234
{
	public enum EOperationType
	{
		None,
		Drop,
		FastDrop,
		CreateEmptyHands,
		CreateFirearm,
		CreateGrenade,
		CreateMeds,
		CreateKnife,
		CreateQuickGrenadeThrow,
		CreateQuickKnifeKick,
		CreateQuickUseItem,
		CreateUsableItem,
		DropAndCreateEmptyHands,
		DropAndCreateFirearm,
		DropAndCreateGrenade,
		DropAndCreateMeds,
		DropAndCreateKnife,
		DropAndCreateQuickGrenadeThrow,
		DropAndCreateQuickKnifeKick,
		DropAndCreateQuickUseItem,
		DropAndCreateUsableItem
	}

	[NonSerialized]
	public const int Int_0 = 2047;

	public EOperationType OperationType;

	public uint CallbackId;

	public string ItemId;

	public GStruct382<EBodyPart> MedsBodyParts;

	public float MedsAmount;

	public int AnimationVariant;

	public static void Serialize(ISerializer stream, ref GStruct234 handsChangePacket, GStruct234 prevFrame)
	{
		bool value = handsChangePacket.Equals(prevFrame);
		stream.Serialize(ref value);
		if (value)
		{
			handsChangePacket = prevFrame;
			return;
		}
		stream.Serialize(ref handsChangePacket.OperationType);
		stream.SerializeLimitedString(ref handsChangePacket.ItemId, ' ', 'z', BitPackingTag.HandsChangePacketItemId, 1600u);
		int value2 = (int)handsChangePacket.CallbackId;
		stream.SerializeLimitedInt32(ref value2, 0, 2047, BitPackingTag.HandsChangePacket0);
		handsChangePacket.CallbackId = (uint)value2;
		if (handsChangePacket.OperationType == EOperationType.CreateMeds)
		{
			bool flag;
			if (flag = stream.StreamMode == EBitStreamMode.Reading)
			{
				handsChangePacket.MedsBodyParts = default(GStruct382<EBodyPart>);
			}
			int value3 = handsChangePacket.MedsBodyParts.Length;
			stream.Serialize(ref value3);
			for (int i = 0; i < value3; i++)
			{
				EBodyPart value4 = EBodyPart.Head;
				if (!flag)
				{
					value4 = handsChangePacket.MedsBodyParts[i];
				}
				stream.Serialize(ref value4);
				if (flag)
				{
					handsChangePacket.MedsBodyParts.Add(value4);
				}
			}
			stream.Serialize(ref handsChangePacket.MedsAmount);
		}
		stream.SerializeLimitedInt32(ref handsChangePacket.AnimationVariant, -1, 3, BitPackingTag.HandsChangePacket1);
	}

	public bool Equals(GStruct234 other)
	{
		if (OperationType == other.OperationType && CallbackId == other.CallbackId && ItemId == other.ItemId && MedsBodyParts.Equals(other.MedsBodyParts) && MedsAmount.Equals(other.MedsAmount))
		{
			return AnimationVariant == other.AnimationVariant;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is GStruct234 other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)(((((((((uint)((int)OperationType * 397) ^ CallbackId) * 397) ^ (uint)((ItemId != null) ? ItemId.GetHashCode() : 0)) * 397) ^ (uint)MedsBodyParts.GetHashCode()) * 397) ^ (uint)MedsAmount.GetHashCode()) * 397) ^ AnimationVariant;
	}
}
