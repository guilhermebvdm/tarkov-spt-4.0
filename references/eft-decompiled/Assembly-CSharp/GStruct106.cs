public struct GStruct106
{
	public float RemainingEnergy;

	public float TotalThickness;

	public bool PathPotentiallyValid;

	public static GStruct106 NoPath => new GStruct106
	{
		RemainingEnergy = 0f,
		TotalThickness = float.MaxValue,
		PathPotentiallyValid = false
	};

	public static GStruct106 ClearPath => new GStruct106
	{
		RemainingEnergy = 1f,
		TotalThickness = 0f,
		PathPotentiallyValid = true
	};
}
