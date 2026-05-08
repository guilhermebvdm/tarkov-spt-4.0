using System;
using Interpolation;
using UnityEngine;

public abstract class GClass1302<T, U> : GClass1301<T, GClass1302<T, U>, U>
{
	[NonSerialized]
	public GInterface120<T> Ginterface120_0;

	[NonSerialized]
	public GClass826<GStruct131<T>> Gclass826_0;

	public GClass1302(GInterface120<T> interpolationStrategy, int queueSize)
	{
		Gclass826_0 = new GClass826<GStruct131<T>>(queueSize);
		Ginterface120_0 = interpolationStrategy;
	}

	public override int GetValuesCount()
	{
		return Gclass826_0.Size;
	}

	public int GetReservedSize()
	{
		return Gclass826_0.ReservedSize;
	}

	public override float GetLeftTime(float currentTime)
	{
		if (GetValuesCount() == 0)
		{
			return 0f;
		}
		return Gclass826_0.PeekBack().TimeInfo.TimeStamp - currentTime;
	}

	public GStruct131<T> PeekLastValue()
	{
		return Gclass826_0.PeekBack();
	}

	public void OverrideValue(int index, T value)
	{
		GStruct131<T> gStruct = Gclass826_0[index];
		Gclass826_0[index] = new GStruct131<T>(gStruct.TimeInfo, value);
	}

	public override void RemoveValues(int count)
	{
		Gclass826_0.RemoveValues(count);
	}

	public override int Interpolate(GStruct130 start, GStruct130 end, GClass1304<GStruct127<T>> result)
	{
		float timeStamp = start.TimeBound.TimeStamp;
		float timeStamp2 = end.TimeBound.TimeStamp;
		GStruct131<T> gStruct = Gclass826_0[start.Index];
		if (timeStamp2 < gStruct.TimeInfo.TimeStamp)
		{
			Debug.LogErrorFormat("Wrong time interpolation, less than range, time {0}, minTIme {1}", timeStamp2, gStruct.TimeInfo);
			result[0] = new GStruct127<T>
			{
				DeltaTime = 0f,
				Frame = gStruct.Value
			};
			return 1;
		}
		GStruct131<T> gStruct2 = Gclass826_0[end.Index];
		if (end.TimeBound.TimeStamp > timeStamp2)
		{
			Debug.LogErrorFormat("Wrong time interpolation, more than range, time {0}, maxTime {1}", timeStamp2, gStruct2.TimeInfo);
			result[0] = new GStruct127<T>
			{
				DeltaTime = 0f,
				Frame = gStruct2.Value
			};
			return 1;
		}
		int num = end.Index - start.Index;
		for (int i = 0; i < num; i++)
		{
			GStruct131<T> gStruct3 = Gclass826_0[start.Index + i];
			GStruct131<T> gStruct4 = Gclass826_0[start.Index + i + 1];
			TimeRangeInfoStruct startTimeInfo = new TimeRangeInfoStruct(gStruct3.TimeInfo.TimeStamp, start.TimeBound.BoundType);
			TimeRangeInfoStruct endTimeInfo = new TimeRangeInfoStruct(gStruct4.TimeInfo.TimeStamp, end.TimeBound.BoundType);
			float num2 = (timeStamp2 - gStruct3.TimeInfo.TimeStamp) / (gStruct4.TimeInfo.TimeStamp - gStruct3.TimeInfo.TimeStamp);
			float deltaTime = Mathf.Min(timeStamp2, endTimeInfo.TimeStamp) - Mathf.Max(timeStamp, startTimeInfo.TimeStamp);
			num2 = Mathf.Clamp(float.IsNaN(num2) ? 1f : num2, 0f, 1f);
			T frame = Ginterface120_0.Interpolate(in startTimeInfo, in endTimeInfo, in gStruct3.Value, in gStruct4.Value, num2);
			result[i] = new GStruct127<T>
			{
				DeltaTime = deltaTime,
				Frame = frame
			};
		}
		return num;
	}

	public override T InterpolateThroughRange(in GStruct130 start, in GStruct130 end)
	{
		TimeRangeInfoStruct timeBound = end.TimeBound;
		GStruct131<T> gStruct = Gclass826_0[start.Index];
		float timeStamp = timeBound.TimeStamp;
		TimeRangeInfoStruct timeInfo = gStruct.TimeInfo;
		float timeStamp2 = timeInfo.TimeStamp;
		if (timeStamp < timeStamp2)
		{
			Debug.LogErrorFormat("Wrong time interpolation, less than range, time {0}, minTIme {1}", timeStamp, timeInfo);
			return gStruct.Value;
		}
		GStruct131<T> gStruct2 = Gclass826_0[end.Index];
		TimeRangeInfoStruct timeInfo2 = gStruct2.TimeInfo;
		float timeStamp3 = timeInfo2.TimeStamp;
		if (timeStamp > timeStamp3)
		{
			Debug.LogErrorFormat("Wrong time interpolation, more than range, time {0}, maxTime {1}", timeStamp, timeInfo2);
			return gStruct2.Value;
		}
		TimeRangeInfoStruct endTimeInfo = new TimeRangeInfoStruct(timeStamp3, end.TimeBound.BoundType);
		TimeRangeInfoStruct startTimeInfo = new TimeRangeInfoStruct(timeStamp2, start.TimeBound.BoundType);
		float relativeTime = (timeStamp - timeStamp2) / (timeStamp3 - timeStamp2);
		return Ginterface120_0.Interpolate(in startTimeInfo, in endTimeInfo, in gStruct.Value, in gStruct2.Value, relativeTime);
	}

	public override void InternalAdd(float time, in T value)
	{
		if (Gclass826_0.Size + 1 > Gclass826_0.ReservedSize)
		{
			Gclass826_0.RemoveValues(1);
		}
		Gclass826_0.Enqueue(new GStruct131<T>(new TimeRangeInfoStruct(time, EBoundType.Equals), value));
	}

	public override void Clear()
	{
		Gclass826_0.Clear();
	}

	public override GStruct131<T> GetTimeStampedValue(int index)
	{
		return Gclass826_0[index];
	}

	public override float GetTimeStamp(int index)
	{
		return GetTimeStampedValue(index).TimeInfo.TimeStamp;
	}

	public override void Dispose()
	{
		Gclass826_0.Dispose();
	}
}
