public struct FloatQuantizerStruct
{
	public readonly float Min;

	public readonly float Max;

	public readonly float Resolution;

	public readonly int BitsRequired;

	public readonly float Delta;

	public readonly int MaxIntegerValue;

	public readonly bool CheckBounds;

	public FloatQuantizerStruct(float min, float max, float resolution, bool checkBounds = false)
	{
		Min = min;
		Max = max;
		Resolution = resolution;
		CheckBounds = checkBounds;
		GClass1365.CalculateDataForQuantizing(Min, Max, Resolution, out Delta, out MaxIntegerValue, out BitsRequired);
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "Min", Min, "Max", Max, "Resolution", Resolution, "CheckBounds", CheckBounds);
	}
}
