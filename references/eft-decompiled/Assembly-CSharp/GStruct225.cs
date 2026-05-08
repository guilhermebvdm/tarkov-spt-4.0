public struct GStruct225
{
	public bool HasCriticalData;

	public EInteraction Interaction;

	public bool Equals(GStruct225 other)
	{
		return object.Equals(Interaction, other.Interaction);
	}

	public override int GetHashCode()
	{
		return Interaction.GetHashCode();
	}

	public void Serialize(GInterface131 writer)
	{
		writer.Write(Interaction);
	}

	public void Deserialize(IDataReader reader)
	{
		Interaction = reader.ReadEnum<EInteraction>();
	}
}
