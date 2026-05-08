using Audio.SpatialSystem.SpatialAudioCalculator;

public struct GStruct102
{
	public float pathLength;

	public EPathType pathType;

	public static GStruct102 NoPath => new GStruct102
	{
		pathLength = float.MaxValue,
		pathType = EPathType.Obstructed
	};
}
