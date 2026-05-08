public struct GStruct107
{
	public float reflectionScore;

	public float diffractionScore;

	public float transmissionScore;

	public static GStruct107 Obstructed => new GStruct107
	{
		reflectionScore = 1f,
		diffractionScore = 1f,
		transmissionScore = 1f
	};

	public static GStruct107 Clear => new GStruct107
	{
		reflectionScore = 0f,
		diffractionScore = 0f,
		transmissionScore = 0f
	};
}
