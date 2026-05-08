using BitPacking;

public struct GStruct268
{
	public float WeaponOverlap;

	public static void DeserializeDiffUsing(IDataReader reader, ref GStruct268 current)
	{
		current.WeaponOverlap = reader.ReadLimitedFloat(0f, 1f, 0.2f);
	}

	public static void SerializeDiffUsing(GInterface131 writer, ref GStruct268 current)
	{
		writer.WriteLimitedFloat(current.WeaponOverlap, 0f, 1f, 0.2f, BitPackingTag.WeaponOverlap);
	}
}
