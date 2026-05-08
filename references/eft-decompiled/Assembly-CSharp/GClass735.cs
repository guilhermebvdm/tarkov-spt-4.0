using System;
using UnityEngine;

public class GClass735<T> : IDisposable where T : IComparable<T>
{
	[NonSerialized]
	public LoggerClass LoggerClass;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public T Gparam_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public T Gparam_1;

	public GClass735(LoggerClass logger, string logType, string logTypeValue, T badValue)
	{
		LoggerClass = logger;
		String_0 = logType;
		String_1 = logTypeValue;
		Gparam_0 = badValue;
	}

	public void Check(T value)
	{
		T gparam_ = Gparam_0;
		if (value.CompareTo(gparam_) >= 0)
		{
			if (Int_0 == 0)
			{
				Float_0 = Time.time;
			}
			Int_0++;
			T gparam_2 = Gparam_1;
			if (value.CompareTo(gparam_2) > 0)
			{
				Gparam_1 = value;
			}
		}
		else
		{
			method_0();
		}
	}

	public void method_0()
	{
		if (Int_0 > 0)
		{
			LoggerClass.LogWarnFields(String_0, String_1, "maxBadValue", Gparam_1, "count", Int_0, "durationTime", (Time.time - Float_0).ToString("F2"), "currentFrameNumber", Time.frameCount);
			Reset();
		}
	}

	public void Reset()
	{
		Int_0 = 0;
		Float_0 = 0f;
		Gparam_1 = default(T);
	}

	public void Dispose()
	{
		method_0();
	}
}
