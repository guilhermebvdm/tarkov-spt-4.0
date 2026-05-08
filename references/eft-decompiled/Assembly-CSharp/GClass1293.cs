using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class GClass1293
{
	public static double Timescale(double drift, double catchupSpeed, double slowdownSpeed, double catchupNegativeThreshold, double catchupPositiveThreshold)
	{
		if (drift > catchupPositiveThreshold)
		{
			return 1.0 + catchupSpeed;
		}
		if (drift < catchupNegativeThreshold)
		{
			return 1.0 - slowdownSpeed;
		}
		return 1.0;
	}

	public static double DynamicAdjustment(double sendInterval, double jitterStandardDeviation, double dynamicAdjustmentTolerance)
	{
		return (sendInterval + jitterStandardDeviation) / sendInterval + dynamicAdjustmentTolerance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool InsertIfNotExists<T>(SortedList<double, T> buffer, T snapshot) where T : GInterface119
	{
		int count = buffer.Count;
		buffer[snapshot.remoteTime] = snapshot;
		return buffer.Count > count;
	}

	public static void InsertAndAdjust<T>(SortedList<double, T> buffer, T snapshot, ref double localTimeline, ref double localTimescale, float sendInterval, double bufferTime, double catchupSpeed, double slowdownSpeed, ref GStruct124 driftEma, float catchupNegativeThreshold, float catchupPositiveThreshold, ref GStruct124 deliveryTimeEma) where T : GInterface119
	{
		if (buffer.Count == 0)
		{
			localTimeline = snapshot.remoteTime - bufferTime;
		}
		if (InsertIfNotExists(buffer, snapshot))
		{
			if (buffer.Count >= 2)
			{
				double localTime = buffer.Values[buffer.Count - 2].localTime;
				double newValue = buffer.Values[buffer.Count - 1].localTime - localTime;
				deliveryTimeEma.Add(newValue);
			}
			double newValue2 = snapshot.remoteTime - localTimeline;
			driftEma.Add(newValue2);
			double drift = driftEma.Value - bufferTime;
			double catchupNegativeThreshold2 = sendInterval * catchupNegativeThreshold;
			double catchupPositiveThreshold2 = sendInterval * catchupPositiveThreshold;
			localTimescale = Timescale(drift, catchupSpeed, slowdownSpeed, catchupNegativeThreshold2, catchupPositiveThreshold2);
		}
	}

	public static void Sample<T>(SortedList<double, T> buffer, double localTimeline, out int from, out int to, out double t) where T : GInterface119
	{
		from = -1;
		to = -1;
		t = 0.0;
		int num = 0;
		T val;
		T val2;
		while (true)
		{
			if (num < buffer.Count - 1)
			{
				val = buffer.Values[num];
				val2 = buffer.Values[num + 1];
				if (localTimeline >= val.remoteTime && !(localTimeline > val2.remoteTime))
				{
					break;
				}
				num++;
				continue;
			}
			if (buffer.Values[0].remoteTime > localTimeline)
			{
				int num2 = 0;
				to = 0;
				from = 0;
				t = 0.0;
			}
			else
			{
				from = (to = buffer.Count - 1);
				t = 0.0;
			}
			return;
		}
		from = num;
		to = num + 1;
		t = GClass1299.InverseLerp(val.remoteTime, val2.remoteTime, localTimeline);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void StepTime(double deltaTime, ref double localTimeline, double localTimescale)
	{
		localTimeline += deltaTime * localTimescale;
	}

	public static void StepInterpolation<T>(SortedList<double, T> buffer, double localTimeline, out T fromSnapshot, out T toSnapshot, out double t) where T : GInterface119
	{
		Sample(buffer, localTimeline, out var from, out var to, out t);
		fromSnapshot = buffer.Values[from];
		toSnapshot = buffer.Values[to];
		GClass1292.RemoveRange(buffer, from);
	}

	public static void Step<T>(SortedList<double, T> buffer, double deltaTime, ref double localTimeline, double localTimescale, out T fromSnapshot, out T toSnapshot, out double t) where T : GInterface119
	{
		StepTime(deltaTime, ref localTimeline, localTimescale);
		StepInterpolation(buffer, localTimeline, out fromSnapshot, out toSnapshot, out t);
	}
}
