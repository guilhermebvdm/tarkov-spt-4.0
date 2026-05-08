using BitPacking;

public struct GStruct245
{
	public bool Reload;

	public string MagId;

	public byte[] LocationDescription;

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Reload);
		if (Reload)
		{
			writer.WriteLimitedString(MagId, ' ', 'z', BitPackingTag.ReloadMagPacketMagId, 1600u);
			writer.WriteBytesAndSize(LocationDescription ?? new byte[0]);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		Reload = reader.ReadBool();
		if (Reload)
		{
			MagId = reader.ReadLimitedString(' ', 'z', 1600u);
			LocationDescription = reader.ReadBytesAlloc(1600u);
		}
	}

	public static void DeserializeDiffUsing(IDataReader reader, ref GStruct245 reloadMagPacket, GStruct245 reloadMagPacket1)
	{
		reloadMagPacket.Deserialize(reader);
	}

	public static void SerializeDiffUsing(GInterface131 writer, GStruct245 reloadMagPacket, GStruct245 reloadMagPacket1)
	{
		reloadMagPacket.Serialize(writer);
	}

	public override string ToString()
	{
		return $"Reload: {Reload}, MagId: {MagId}, LocationDescription: {LocationDescription}";
	}
}
