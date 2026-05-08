public interface GInterface120<T>
{
	T Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in T startValue, in T endValue, float relativeTime);
}
