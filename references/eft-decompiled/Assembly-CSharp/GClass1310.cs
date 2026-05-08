using Interpolation;

public abstract class GClass1310
{
	public const int NOT_VALID_INT = int.MinValue;

	public const float NOT_VALID_FLOAT = float.NaN;

	public static readonly TimeRangeInfoStruct NOT_VALID_TIME_BOUND = new TimeRangeInfoStruct(float.NaN, EBoundType.Unknown);

	public static readonly GStruct130 NOT_VALID_BOUND = new GStruct130(int.MinValue, NOT_VALID_TIME_BOUND);

	public static bool IsValid(this TimeRangeInfoStruct timeBound)
	{
		return !timeBound.Equals(NOT_VALID_TIME_BOUND);
	}

	public static bool IsValid(this GStruct130 spanBound)
	{
		return !spanBound.Equals(NOT_VALID_BOUND);
	}

	public static bool IsValid<TValue, TInterpolator, TValueShortData>(this GStruct128<TValue, TInterpolator, TValueShortData> interpolatorSpan) where TInterpolator : GClass1301<TValue, TInterpolator, TValueShortData>
	{
		if (IsValid(interpolatorSpan.Start))
		{
			return IsValid(interpolatorSpan.End);
		}
		return false;
	}
}
