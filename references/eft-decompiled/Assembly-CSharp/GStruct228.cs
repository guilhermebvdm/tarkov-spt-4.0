public struct GStruct228
{
	public bool Interact;

	public string LootId;

	public uint CallbackId;

	public bool Equals(GStruct228 other)
	{
		if (Interact == other.Interact && string.Equals(LootId, other.LootId))
		{
			return CallbackId == other.CallbackId;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct228)
		{
			return Equals((GStruct228)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((Interact.GetHashCode() * 397) ^ ((LootId != null) ? LootId.GetHashCode() : 0)) * 397) ^ (int)CallbackId;
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Interact);
		if (Interact)
		{
			writer.Write(LootId);
			writer.Write(CallbackId);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		Interact = reader.ReadBool();
		if (Interact)
		{
			LootId = reader.ReadString(1600u);
			CallbackId = reader.ReadUInt32();
		}
	}
}
