public struct GStruct56(float min, float max)
{
	public float min = min;

	public float max = max;

	public static GStruct56 operator +(GStruct56 c1, GStruct56 c2)
	{
		return new GStruct56(c1.min + c2.min, c1.max + c2.max);
	}

	public static GStruct56 operator -(GStruct56 c1, GStruct56 c2)
	{
		return new GStruct56(c1.min - c2.min, c1.max - c2.max);
	}
}
