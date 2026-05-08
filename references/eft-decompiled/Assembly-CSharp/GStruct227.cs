public struct GStruct227
{
	public bool Breach;

	public string Id;

	public bool Equals(GStruct227 other)
	{
		if (Breach == other.Breach)
		{
			return string.Equals(Id, other.Id);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct227)
		{
			return Equals((GStruct227)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Breach.GetHashCode() * 397) ^ ((Id != null) ? Id.GetHashCode() : 0);
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Breach);
		if (Breach)
		{
			writer.Write(Id);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		Breach = reader.ReadBool();
		if (Breach)
		{
			Id = reader.ReadString(1600u);
		}
	}
}
