using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

public class GClass1520
{
	[Serializable]
	[CompilerGenerated]
	public class Class1020
	{
		public static readonly Class1020 class1020_0 = new Class1020();

		public static Func<string, string> func_0;

		public static Func<string, float> func_1;

		public string method_0(string x)
		{
			return x;
		}

		public float method_1(string x)
		{
			return 0f;
		}
	}

	[NonSerialized]
	public Dictionary<string, float> Dictionary_0;

	[NonSerialized]
	public IProgress<float> Iprogress_0;

	[NonSerialized]
	public Stopwatch Stopwatch_0;

	[NonSerialized]
	public long Long_0;

	public GClass1520(IProgress<float> progress, IEnumerable<string> keys)
	{
		Iprogress_0 = progress;
		Dictionary_0 = keys.ToDictionary((string x) => x, (string x) => 0f);
		Stopwatch_0 = Stopwatch.StartNew();
	}

	public void Report(string key, float value)
	{
		Long_0 = Stopwatch_0.ElapsedTicks;
		Dictionary_0[key] = value;
		float value2 = Dictionary_0.Values.Sum() / (float)Dictionary_0.Count;
		Iprogress_0.Report(value2);
	}

	public long GetTicksSinceUpdate()
	{
		return Stopwatch_0.ElapsedTicks - Long_0;
	}

	public TimeSpan GetTimeSinceUpdate()
	{
		return TimeSpan.FromTicks(GetTicksSinceUpdate());
	}
}
