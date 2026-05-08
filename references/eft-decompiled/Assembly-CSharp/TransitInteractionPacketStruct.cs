using JsonType;

public struct TransitInteractionPacketStruct
{
	public bool hasInteraction;

	public int pointId;

	public string keyId;

	public EDateTime time;

	public bool Equals(TransitInteractionPacketStruct other)
	{
		return pointId == other.pointId;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is TransitInteractionPacketStruct other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return pointId.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(hasInteraction);
		if (hasInteraction)
		{
			writer.Write(pointId);
			writer.Write(keyId);
			writer.Write(time);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			hasInteraction = true;
			pointId = reader.ReadInt32();
			keyId = reader.ReadString(1600u);
			time = reader.ReadEnum<EDateTime>();
		}
	}
}
