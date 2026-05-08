using JetBrains.Annotations;

public readonly struct GStruct130(int index, TimeRangeInfoStruct timeBound)
{
	public readonly TimeRangeInfoStruct TimeBound = timeBound;

	public readonly int Index = index;

	[Pure]
	public bool Equals(GStruct130 other)
	{
		if (Index == other.Index)
		{
			return TimeBound.Equals(other.TimeBound);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct130)
		{
			return Equals((GStruct130)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Index * 397) ^ TimeBound.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("Id:{1} ts:{0}, ", TimeBound, Index);
	}
}
