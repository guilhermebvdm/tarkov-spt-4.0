using System;
using Interpolation;

public abstract class GClass1301<T, U, V> : IDisposable where U : GClass1301<T, U, V>
{
	public bool IsEmpty => GetValuesCount() < 2;

	public abstract void Clear();

	public void Add(float time, T value)
	{
		InternalAdd(time, in value);
	}

	public abstract void InternalAdd(float time, in T value1);

	public abstract GStruct131<T> GetTimeStampedValue(int index);

	public abstract float GetTimeStamp(int index);

	public abstract int Interpolate(GStruct130 start, GStruct130 end, GClass1304<GStruct127<T>> result);

	public abstract T InterpolateThroughRange(in GStruct130 start, in GStruct130 end);

	public abstract int GetValuesCount();

	public abstract float GetLeftTime(float currentTime);

	public abstract V GetValueShortData(int index);

	public abstract void RemoveValues(int count);

	public void RemoveDataUntil(in TimeRangeInfoStruct startRangeTime)
	{
		method_0(in startRangeTime, in startRangeTime, out var resultStartBound, out var _);
		RemoveValues(resultStartBound.Index);
	}

	public bool GetInterpolatorSpan(in TimeRangeInfoStruct start, in TimeRangeInfoStruct end, out GStruct128<T, U, V> interpolatorSpan)
	{
		if (start.TimeStamp >= end.TimeStamp)
		{
			interpolatorSpan = GStruct128<T, U, V>.NOT_INITIALIZED_SPAN;
			return false;
		}
		GStruct130 resultStartBound;
		GStruct130 resultEndBound;
		bool num = method_0(in start, in end, out resultStartBound, out resultEndBound);
		if (num)
		{
			interpolatorSpan = new GStruct128<T, U, V>(resultStartBound, resultEndBound, this);
			return num;
		}
		interpolatorSpan = GStruct128<T, U, V>.NOT_INITIALIZED_SPAN;
		return num;
	}

	public bool method_0(in TimeRangeInfoStruct start, in TimeRangeInfoStruct end, out GStruct130 resultStartBound, out GStruct130 resultEndBound)
	{
		int valuesCount = GetValuesCount();
		float timeStamp = GetTimeStamp(0);
		float timeStamp2 = GetTimeStamp(valuesCount - 1);
		resultStartBound = GClass1310.NOT_VALID_BOUND;
		resultEndBound = GClass1310.NOT_VALID_BOUND;
		bool result = false;
		if (!(start.TimeStamp > end.TimeStamp) && !(timeStamp > end.TimeStamp) && !(end.TimeStamp > timeStamp2) && !(timeStamp > start.TimeStamp) && !(start.TimeStamp > timeStamp2))
		{
			bool num = start.TimeStamp - timeStamp < timeStamp2 - end.TimeStamp;
			int num2 = ((!num) ? (valuesCount - 1) : 0);
			int num3 = (num ? 1 : (-1));
			num2 -= num3;
			TimeRangeInfoStruct leftSideValue = (num ? start : GClass1310.NOT_VALID_TIME_BOUND);
			TimeRangeInfoStruct rightSideValue = (num ? GClass1310.NOT_VALID_TIME_BOUND : end);
			method_1(num2, num3, rightSideValue, leftSideValue, ref resultStartBound, ref resultEndBound, incrementIndexInCaseOfLess: false);
			leftSideValue = (num ? end : GClass1310.NOT_VALID_TIME_BOUND);
			rightSideValue = (num ? GClass1310.NOT_VALID_TIME_BOUND : start);
			method_1(num2, num3, rightSideValue, leftSideValue, ref resultEndBound, ref resultStartBound, incrementIndexInCaseOfLess: true);
			result = true;
		}
		return result;
	}

	public void method_1(int index, int increment, TimeRangeInfoStruct rightSideValue, TimeRangeInfoStruct leftSideValue, ref GStruct130 resultStartIndex, ref GStruct130 resultEndIndex, bool incrementIndexInCaseOfLess)
	{
		int num = GetValuesCount() + 10;
		bool flag = false;
		while (!flag && --num > 0)
		{
			int num2 = index + increment;
			if (increment > 0)
			{
				rightSideValue = new TimeRangeInfoStruct(GetTimeStamp(num2), EBoundType.GreaterOrEqual);
			}
			else
			{
				leftSideValue = new TimeRangeInfoStruct(GetTimeStamp(num2), EBoundType.GreaterOrEqual);
			}
			flag = leftSideValue.TimeStamp <= rightSideValue.TimeStamp;
			if (leftSideValue.TimeStamp > rightSideValue.TimeStamp)
			{
				index = num2;
			}
			else if (leftSideValue.TimeStamp < rightSideValue.TimeStamp)
			{
				if (incrementIndexInCaseOfLess)
				{
					index = num2;
				}
				flag = true;
			}
			else
			{
				index = num2;
				flag = true;
			}
		}
		if (increment > 0)
		{
			resultStartIndex = new GStruct130(index, leftSideValue);
		}
		else
		{
			resultEndIndex = new GStruct130(index, rightSideValue);
		}
	}

	public abstract void Dispose();

	public GClass1301()
	{
	}
}
