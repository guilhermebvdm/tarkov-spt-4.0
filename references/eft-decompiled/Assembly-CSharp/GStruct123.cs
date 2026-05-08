public struct GStruct123(short x, short y, short z)
{
	public short x = x;

	public short y = y;

	public short z = z;

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteShort(writer, x);
		GClass1290.WriteShort(writer, y);
		GClass1290.WriteShort(writer, z);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		x = GClass1285.ReadShort(reader);
		y = GClass1285.ReadShort(reader);
		z = GClass1285.ReadShort(reader);
	}
}
