public struct GStruct229
{
	public bool hasCriticalData;

	public EInteractionStatus interactionStatus;

	public bool isSuccess;

	public bool hasMultiTool;

	public void Serialize(GInterface131 writer)
	{
		writer.Write(hasCriticalData);
		if (hasCriticalData)
		{
			writer.Write((byte)interactionStatus);
			writer.Write(isSuccess);
			writer.Write(hasMultiTool);
		}
	}

	public void Deserialize(IDataReader reader)
	{
		hasCriticalData = reader.ReadBool();
		if (hasCriticalData)
		{
			interactionStatus = (EInteractionStatus)reader.ReadByte();
			isSuccess = reader.ReadBool();
			hasMultiTool = reader.ReadBool();
		}
	}
}
