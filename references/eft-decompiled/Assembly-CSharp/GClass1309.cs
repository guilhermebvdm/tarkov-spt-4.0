using UnityEngine;

public class GClass1309 : GInterface120<Vector2>
{
	public Vector2 Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in Vector2 startValue, in Vector2 endValue, float relativeTime)
	{
		return new Vector2(Mathf.LerpAngle(startValue.x, endValue.x, relativeTime), Mathf.LerpAngle(startValue.y, endValue.y, relativeTime));
	}

	public Vector2 Interpolate_1(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in Vector2 startValue, in Vector2 endValue, float relativeTime)
	{
		return Interpolate(in startTimeInfo, in endTimeInfo, in startValue, in endValue, relativeTime);
	}

	Vector2 GInterface120<Vector2>.Interpolate(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in Vector2 startValue, in Vector2 endValue, float relativeTime)
	{
		//ILSpy generated this explicit interface implementation from .override directive in Interpolate_1
		return this.Interpolate_1(in startTimeInfo, in endTimeInfo, in startValue, in endValue, relativeTime);
	}
}
