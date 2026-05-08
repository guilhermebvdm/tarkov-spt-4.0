using System;
using BitPacking;

public struct GStruct252
{
	[NonSerialized]
	public const int Int_0 = 2047;

	public byte[] CommandBytes;

	public uint CallbackId;

	public int InventoryHashSum;

	public void Serialize(GInterface131 writer)
	{
		if (CommandBytes != null && CommandBytes.Length != 0)
		{
			writer.Write(value: true);
			writer.WriteBytesAndSize(CommandBytes);
			writer.WriteLimitedInt32((int)CallbackId, 0, 2047, BitPackingTag.HandsChangePacket21);
			writer.Write(InventoryHashSum);
		}
		else
		{
			writer.Write(value: false);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			CommandBytes = reader.ReadBytesAlloc(1600u);
			CallbackId = (uint)reader.ReadLimitedInt32(0, 2047);
			InventoryHashSum = reader.ReadInt32();
		}
	}
}
