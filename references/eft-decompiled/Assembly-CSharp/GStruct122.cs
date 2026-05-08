public struct GStruct122(byte x, byte y, byte z)
{
	public byte x = x;

	public byte y = y;

	public byte z = z;

	public void Serialize(EFTWriterClass writer)
	{
		writer.WriteByte(x);
		writer.WriteByte(y);
		writer.WriteByte(z);
	}

	public void Deserialize(EFTReaderClass reader)
	{
		x = reader.ReadByte();
		y = reader.ReadByte();
		z = reader.ReadByte();
	}
}
