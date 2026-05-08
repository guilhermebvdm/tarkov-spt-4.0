public readonly struct GStruct131<T>(TimeRangeInfoStruct timeInfo, T value)
{
	public readonly TimeRangeInfoStruct TimeInfo = timeInfo;

	public readonly T Value = value;

	public override string ToString()
	{
		return $"Time: {TimeInfo}, Value: {Value}";
	}
}
