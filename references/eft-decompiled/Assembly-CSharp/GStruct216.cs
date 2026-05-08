using BitPacking;

public struct GStruct216
{
	public bool HasInteraction;

	public int Id;

	public bool Equals(GStruct216 other)
	{
		return object.Equals(Id, other.Id);
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(HasInteraction);
		if (HasInteraction)
		{
			writer.WriteLimitedInt32(Id, 0, 512, BitPackingTag.InteractWithTripwire);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			HasInteraction = true;
			Id = reader.ReadLimitedInt32(0, 512);
		}
	}
}
