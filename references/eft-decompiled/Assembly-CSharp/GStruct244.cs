public struct GStruct244
{
	public bool HasNewValue;

	public bool LeftStanceState;

	public static void Serialize(ISerializer stream, ref GStruct244 packet)
	{
		stream.Serialize(ref packet.HasNewValue);
		if (packet.HasNewValue)
		{
			stream.Serialize(ref packet.LeftStanceState);
		}
	}
}
