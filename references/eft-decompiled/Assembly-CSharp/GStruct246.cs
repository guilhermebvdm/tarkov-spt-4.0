using EFT;

public struct GStruct246
{
	public bool Reload;

	public MongoID MagId;

	public void method_0(GInterface131 writer)
	{
		writer.Write(Reload);
		if (Reload)
		{
			GClass2064.Serialize(writer, ref MagId);
		}
	}

	public void method_1(IDataReader reader)
	{
		Reload = reader.ReadBool();
		if (Reload)
		{
			GClass2064.Serialize(reader, ref MagId);
		}
	}

	public static void DeserializeDiffUsing(IDataReader reader, ref GStruct246 quickReloadMagPacket, GStruct246 quickReloadMag)
	{
		quickReloadMagPacket.method_1(reader);
	}

	public static void SerializeDiffUsing(GInterface131 writer, GStruct246 quickReloadMagPacket, GStruct246 quickReloadMag)
	{
		quickReloadMagPacket.method_0(writer);
	}

	public override string ToString()
	{
		return $"Reload: {Reload}, MagId: {MagId}";
	}
}
