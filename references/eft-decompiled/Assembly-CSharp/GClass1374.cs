public abstract class GClass1374
{
	public static void Serialize(this ISerializer stream, ref float value, GStruct136 settings)
	{
		stream.SerializeLimitedFloat(ref value, settings.Min, settings.Max, settings.Resolution, settings.Tag);
	}

	public static void Serialize(this ISerializer stream, ref int value, GStruct137 settings)
	{
		stream.SerializeLimitedInt32(ref value, settings.Min, settings.Max, settings.Tag);
	}
}
