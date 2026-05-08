using BitPacking;
using EFT.InventoryLogic;

public struct GStruct236
{
	public bool ChangeFireMode;

	public Weapon.EFireMode FireMode;

	public static void DeserializeDiffUsing(IDataReader reader, ref GStruct236 fireModePacket)
	{
		fireModePacket.ChangeFireMode = reader.ReadBool();
		if (fireModePacket.ChangeFireMode)
		{
			fireModePacket.FireMode = (Weapon.EFireMode)reader.ReadLimitedInt32(0, 4);
		}
	}

	public static void SerializeDiffUsing(GInterface131 writer, GStruct236 fireModePacket)
	{
		writer.Write(fireModePacket.ChangeFireMode);
		if (fireModePacket.ChangeFireMode)
		{
			writer.WriteLimitedInt32((int)fireModePacket.FireMode, 0, 4, BitPackingTag.HandsChangePacket13);
		}
	}

	public override string ToString()
	{
		return $"ChangeFireMode: {ChangeFireMode}, FireMode: {FireMode}";
	}
}
