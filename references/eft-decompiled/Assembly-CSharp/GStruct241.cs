using BitPacking;

public struct GStruct241
{
	public bool ChangeSightMode;

	public GStruct242[] SightModeStatuses;

	public static void Serialize(ISerializer stream, ref GStruct241 packet)
	{
		stream.Serialize(ref packet.ChangeSightMode);
		if (packet.ChangeSightMode)
		{
			int value = ((stream.StreamMode == EBitStreamMode.Writing) ? packet.SightModeStatuses.Length : 0);
			stream.SerializeLimitedInt32(ref value, 0, 7, BitPackingTag.SightsModePacketLength);
			if (stream.StreamMode == EBitStreamMode.Reading)
			{
				packet.SightModeStatuses = new GStruct242[value];
			}
			for (int i = 0; i < value; i++)
			{
				GStruct242.Serialize(stream, ref packet.SightModeStatuses[i]);
			}
		}
	}
}
