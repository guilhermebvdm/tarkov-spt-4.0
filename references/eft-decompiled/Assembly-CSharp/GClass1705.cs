public class GClass1705 : GInterface382
{
	public int errorCode;

	public void Deserialize(EFTReaderClass reader)
	{
		errorCode = GClass1285.ReadUShort(reader);
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteUShort(writer, (ushort)errorCode);
	}
}
