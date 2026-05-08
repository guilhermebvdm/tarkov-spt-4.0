public class GClass1306<T> : GInterface120<T>
{
	public T Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in T startValue, in T endValue, float relativeTime)
	{
		return GClass1305.DiscreteInterpolationFunction(in startTimeInfo, in endTimeInfo, in startValue, in endValue, relativeTime);
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
