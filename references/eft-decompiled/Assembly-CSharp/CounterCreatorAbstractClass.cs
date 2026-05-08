using System;
using System.Diagnostics;
using UnityEngine;

public abstract class CounterCreatorAbstractClass
{
	public struct GStruct3(string counterName) : IDisposable
	{
		[NonSerialized]
		public string String_0 = counterName;

		public void Dispose()
		{
		}
	}

	[NonSerialized]
	public static GClass19 Gclass19_0;

	[NonSerialized]
	public static Stopwatch Stopwatch_0;

	[NonSerialized]
	public static GClass19.GClass20 Gclass20_0;

	[NonSerialized]
	public static Action<string> Action_0;

	static CounterCreatorAbstractClass()
	{
		Gclass19_0 = new GClass19();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ENABLE_DEBUG_TIME_MEASURER")]
	public static void Start(string name)
	{
		double value = Stopwatch_0.ElapsedMillisecondsAsDouble();
		Gclass20_0 = Gclass19_0.StartDiff(name, value, Gclass20_0);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ENABLE_DEBUG_TIME_MEASURER")]
	public static void Finish(string name)
	{
		double value = Stopwatch_0.ElapsedMillisecondsAsDouble();
		Gclass19_0.FinishDiff(name, value);
		Gclass20_0 = Gclass20_0?.Parent;
	}

	public static GStruct3 StartWithToken(string name)
	{
		return new GStruct3(name);
	}

	public static string GetReport(string title, bool fullName = true)
	{
		return "Time samples of " + title + ":\n" + Gclass19_0.GetReport(fullName);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ENABLE_DEBUG_TIME_MEASURER")]
	public static void PrintReport(string title, bool fullName = true)
	{
		string report = GetReport(title, fullName);
		if (Action_0 != null)
		{
			Action_0(report);
		}
		else
		{
			UnityEngine.Debug.Log(report);
		}
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ENABLE_DEBUG_TIME_MEASURER")]
	public static void Clear()
	{
		Gclass19_0.Clear();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("ENABLE_DEBUG_TIME_MEASURER")]
	public static void SetPrintDelegate(Action<string> printDelegate)
	{
		Action_0 = printDelegate;
	}
}
