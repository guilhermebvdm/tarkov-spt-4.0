public struct GStruct224
{
	public bool HasCriticalData;

	public string ItemId;

	public bool Remove;

	public float RemoveSpeed;

	public bool Equals(GStruct224 other)
	{
		if (object.Equals(ItemId, other.ItemId) && object.Equals(Remove, other.Remove))
		{
			return object.Equals(RemoveSpeed, other.RemoveSpeed);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ItemId.GetHashCode() ^ Remove.GetHashCode() ^ RemoveSpeed.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(ItemId);
		writer.Write(Remove);
		writer.Write(RemoveSpeed);
	}

	public void Deserialize(IDataReader reader)
	{
		ItemId = reader.ReadString(1600u);
		Remove = reader.ReadBool();
		RemoveSpeed = reader.ReadFloat();
	}
}
