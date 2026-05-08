public struct GStruct121(short x, short y)
{
	public short x = x;

	public short y = y;

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteShort(writer, x);
		GClass1290.WriteShort(writer, y);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		x = GClass1285.ReadShort(reader);
		y = GClass1285.ReadShort(reader);
	}
}
