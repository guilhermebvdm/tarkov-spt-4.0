using EFT;

public struct InteractPacketStruct
{
	public bool hasInteraction;

	public int objectId;

	public EventObject.EInteraction interaction;

	public bool Equals(InteractPacketStruct other)
	{
		if (objectId == other.objectId)
		{
			return interaction == other.interaction;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is InteractPacketStruct other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((int)interaction * 397) ^ objectId.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(hasInteraction);
		if (hasInteraction)
		{
			writer.Write(objectId);
			writer.Write(interaction);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		if (reader.ReadBool())
		{
			hasInteraction = true;
			objectId = reader.ReadInt32();
			interaction = reader.ReadEnum<EventObject.EInteraction>();
		}
	}
}
