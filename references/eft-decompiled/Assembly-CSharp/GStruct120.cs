public struct GStruct120(byte x, byte y)
{
	public byte x = x;

	public byte y = y;

	public void Serialize(EFTWriterClass writer)
	{
		writer.WriteByte(x);
		writer.WriteByte(y);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		x = reader.ReadByte();
		y = reader.ReadByte();
	}
}
