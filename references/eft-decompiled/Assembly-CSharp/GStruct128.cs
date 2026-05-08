using System;
using Interpolation;

public readonly struct GStruct128<T, U, V>(GStruct130 start, GStruct130 end, GClass1301<T, U, V> interpolator) where U : GClass1301<T, U, V>
{
	public static readonly GStruct128<T, U, V> NOT_INITIALIZED_SPAN = new GStruct128<T, U, V>(GClass1310.NOT_VALID_BOUND, GClass1310.NOT_VALID_BOUND, null);

	public readonly GStruct130 Start = start;

	public readonly GStruct130 End = end;

	[NonSerialized]
	public GClass1301<T, U, V> Gclass1301_0 = interpolator;

	public int GetValuesCount()
	{
		return End.Index - Start.Index + 1;
	}

	public float GetDeltaTime()
	{
		return End.TimeBound.TimeStamp - Start.TimeBound.TimeStamp;
	}

	public override string ToString()
	{
		float timeStamp = Gclass1301_0.GetTimeStamp(Start.Index);
		float timeStamp2 = Gclass1301_0.GetTimeStamp(End.Index);
		return string.Format("Index[{0}:{1}] [t:{2}:{3}][dt:{6:F7}] [fdiff:{4:F7}:{5:F7}] [ft:{7:F7}:{8:F7}]", Start.Index, End.Index, Start.TimeBound, End.TimeBound, Start.TimeBound.TimeStamp - timeStamp, End.TimeBound.TimeStamp - timeStamp2, GetDeltaTime(), timeStamp, timeStamp2);
	}

	public bool Equals(GStruct128<T, U, V> other)
	{
		if (Start.Equals(other.Start) && End.Equals(other.End))
		{
			return object.Equals(Gclass1301_0, other.Gclass1301_0);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is GStruct128<T, U, V>)
		{
			return Equals((GStruct128<T, U, V>)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((Start.GetHashCode() * 397) ^ End.GetHashCode()) * 397) ^ ((Gclass1301_0 != null) ? Gclass1301_0.GetHashCode() : 0);
	}

	public float GetTimeStamp(int index)
	{
		return Gclass1301_0.GetTimeStamp(Start.Index + index);
	}

	public V GetValuesShortData(int index)
	{
		return Gclass1301_0.GetValueShortData(Start.Index + index);
	}

	public GStruct128<T, U, V> GetSubSpan(in GStruct130 start, in GStruct130 end)
	{
		return new GStruct128<T, U, V>(start, end, Gclass1301_0);
	}

	public int Interpolate(GClass1304<GStruct127<T>> result)
	{
		return Gclass1301_0.Interpolate(Start, End, result);
	}

	public T InterpolateThroughRange()
	{
		return Gclass1301_0.InterpolateThroughRange(in Start, in End);
	}

	public GStruct128<T, U, V> GetSubSpanFromHeadUntil(in GStruct130 headBound)
	{
		if (End.Index == headBound.Index)
		{
			float timeStamp = End.TimeBound.TimeStamp;
			if (timeStamp.Equals(headBound.TimeBound.TimeStamp) && (End.TimeBound.BoundType & headBound.TimeBound.BoundType) > EBoundType.Unknown)
			{
				return NOT_INITIALIZED_SPAN;
			}
		}
		return new GStruct128<T, U, V>(headBound, End, Gclass1301_0);
	}
}
