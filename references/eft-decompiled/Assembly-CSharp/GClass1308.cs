using System;

public class GClass1308<T> : GInterface120<T>
{
	public delegate T GDelegate43(in T startValue, in T endValue, float relativeTime);

	[NonSerialized]
	public GDelegate43 Gdelegate43_0;

	public GClass1308(GDelegate43 linearInterpolationFunction)
	{
		Gdelegate43_0 = linearInterpolationFunction;
	}

	public T Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in T startValue, in T endValue, float relativeTime)
	{
		return Gdelegate43_0(in startValue, in endValue, relativeTime);
	}

	public T Interpolate_1(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in T startValue, in T endValue, float relativeTime)
	{
		return Interpolate(in startTimeInfo, in endTimeInfo, in startValue, in endValue, relativeTime);
	}

	T GInterface120<T>.Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in T startValue, in T endValue, float relativeTime)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Interpolate_1
		return this.Interpolate_1(in startTimeInfo, in endTimeInfo, in startValue, in endValue, relativeTime);
	}
}
