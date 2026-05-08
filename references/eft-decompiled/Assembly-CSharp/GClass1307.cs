using System;

public class GClass1307<T> : GInterface120<T>
{
	[NonSerialized]
	public T Gparam_0;

	public T ValueBetweenImpulses => Gparam_0;

	public GClass1307(T valueBetweenImpulses)
	{
		Gparam_0 = valueBetweenImpulses;
	}

	public T Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in T startValue, in T endValue, float relativeTime)
	{
		return GClass1305.ImpulseInterpolationFunction(in startTimeInfo, in endTimeInfo, in startValue, in endValue, relativeTime, Gparam_0);
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
