using System;
using Interpolation;
using UnityEngine;

public abstract class GClass1305
{
	public enum ERangeType
	{
		Start,
		End,
		Between
	}

	public static TValue DiscreteInterpolationFunction<TValue>(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in TValue startValue, in TValue endValue, float relativeTime)
	{
		return GetDiscreteBounds(in startTimeInfo, in endTimeInfo, relativeTime) switch
		{
			ERangeType.End => endValue, 
			ERangeType.Start => startValue, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static ERangeType GetDiscreteBounds(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, float relativeTime)
	{
		if ((endTimeInfo.BoundType & EBoundType.GreaterOrEqual) > EBoundType.Unknown && relativeTime >= 1f - Mathf.Epsilon)
		{
			return ERangeType.End;
		}
		return ERangeType.Start;
	}

	public static TValue ImpulseInterpolationFunction<TValue>(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, in TValue startValue, in TValue endValue, float relativeTime, TValue valueBetweenIndexes)
	{
		return GetImpulseBounds(in startTimeInfo, in endTimeInfo, relativeTime) switch
		{
			ERangeType.Start => startValue, 
			ERangeType.End => endValue, 
			ERangeType.Between => valueBetweenIndexes, 
			_ => valueBetweenIndexes, 
		};
	}

	public static ERangeType GetImpulseBounds(in TimeRangeInfoStruct startTimeInfo, in TimeRangeInfoStruct endTimeInfo, float relativeTime)
	{
		if ((startTimeInfo.BoundType & EBoundType.Equals) > EBoundType.Unknown && relativeTime <= Mathf.Epsilon)
		{
			return ERangeType.Start;
		}
		if ((endTimeInfo.BoundType & EBoundType.Equals) > EBoundType.Unknown && relativeTime >= 1f - Mathf.Epsilon)
		{
			return ERangeType.End;
		}
		return ERangeType.Between;
	}
}
