using Interpolation;
using JetBrains.Annotations;

public readonly struct TimeRangeInfoStruct(float timeStamp, EBoundType boundType)
{
	public readonly float TimeStamp = timeStamp;

	public readonly EBoundType BoundType = boundType;

	[Pure]
	public bool Equals(TimeRangeInfoStruct other)
	{
		float timeStamp = TimeStamp;
		if (timeStamp.Equals(other.TimeStamp))
		{
			return BoundType == other.BoundType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is TimeRangeInfoStruct)
		{
			return Equals((TimeRangeInfoStruct)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		float timeStamp = TimeStamp;
		return (timeStamp.GetHashCode() * 397) ^ (int)BoundType;
	}

	public override string ToString()
	{
		return string.Format("T:[{1} {0:F7}]", TimeStamp, BoundType);
	}

	public static int Compare(TimeRangeInfoStruct lv, TimeRangeInfoStruct rv)
	{
		if (lv.TimeStamp < rv.TimeStamp)
		{
			return -1;
		}
		if (lv.TimeStamp > rv.TimeStamp)
		{
			return 1;
		}
		if (lv.BoundType == rv.BoundType)
		{
			return 0;
		}
		if (lv.BoundType < rv.BoundType)
		{
			return -1;
		}
		return 1;
	}
}
