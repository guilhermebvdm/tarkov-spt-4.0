using System;
using System.Collections.Generic;
using System.Diagnostics;

public abstract class GClass21
{
	[NonSerialized]
	public static GClass17 Gclass17_0;

	[NonSerialized]
	public static HashSet<string> HashSet_0;

	static GClass21()
	{
		HashSet_0 = new HashSet<string>();
		Init(60);
	}

	public static void Init(int avgCount)
	{
		Gclass17_0 = new GClass17(avgCount);
	}

	[Conditional("UNITY_EDITOR")]
	public static void StartDiff(string name, double value)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void FinishDiff(string name, double value)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void OnFrameEnd()
	{
	}

	public static string AVGStats()
	{
		return Gclass17_0.AVGStats();
	}

	public static string LastValues()
	{
		return Gclass17_0.LastValues();
	}
}
