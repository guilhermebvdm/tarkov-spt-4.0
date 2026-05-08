public struct GStruct240
{
	public bool ChangeLauncherRange;

	public int RangeValue;

	public int LastRangeValue;

	public static void Serialize(ISerializer stream, ref GStruct240 packet)
	{
		stream.Serialize(ref packet.ChangeLauncherRange);
		if (packet.ChangeLauncherRange)
		{
			stream.Serialize(ref packet.RangeValue);
			stream.Serialize(ref packet.LastRangeValue);
		}
	}
}
