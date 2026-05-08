public struct GStruct221
{
	public bool Successful;

	public string ItemId;

	public string ZoneId;

	public bool Equals(GStruct221 other)
	{
		if (string.Equals(ItemId, other.ItemId))
		{
			return string.Equals(ZoneId, other.ZoneId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct221)
		{
			return Equals((GStruct221)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (ZoneId.GetHashCode() * 1973) ^ ItemId.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Successful);
		if (Successful)
		{
			writer.Write(ItemId);
			writer.Write(ZoneId);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		Successful = reader.ReadBool();
		if (Successful)
		{
			ItemId = reader.ReadString(1600u);
			ZoneId = reader.ReadString(1600u);
		}
	}
}
