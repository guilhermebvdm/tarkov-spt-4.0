public struct GStruct226
{
	public bool HasCriticalData;

	public bool Inventory;

	public bool IsOperation;

	public bool Equals(GStruct226 other)
	{
		if (object.Equals(Inventory, other.Inventory))
		{
			return object.Equals(IsOperation, other.IsOperation);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Inventory.GetHashCode() ^ IsOperation.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Inventory);
		writer.Write(IsOperation);
	}

	public void Deserialize(IDataReader reader)
	{
		Inventory = reader.ReadBool();
		IsOperation = reader.ReadBool();
	}
}
