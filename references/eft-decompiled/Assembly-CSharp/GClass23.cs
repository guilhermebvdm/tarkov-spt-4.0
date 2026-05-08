using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

public class GClass23
{
	public readonly struct GStruct4(GClass23 simpleProfiler, GClass19.GClass20 counter) : IDisposable
	{
		[NonSerialized]
		public GClass23 Gclass23_0 = simpleProfiler;

		[NonSerialized]
		public GClass19.GClass20 Gclass20_0 = counter;

		public void Dispose()
		{
			Gclass23_0.method_2(Gclass20_0);
		}
	}

	public struct GStruct5(string name, double value)
	{
		public string name = name;

		public double value = value;
	}

	[NonSerialized]
	public Stopwatch Stopwatch_0 = new Stopwatch();

	[NonSerialized]
	public StringBuilder StringBuilder_0 = new StringBuilder();

	[NonSerialized]
	public GClass19 Gclass19_0 = new GClass19();

	[NonSerialized]
	public GClass19.GClass20 Gclass20_0;

	[NonSerialized]
	public bool Bool_0;

	public GClass23()
	{
		Stopwatch_0.Start();
	}

	public static double smethod_0(double ticks)
	{
		return ticks / (double)Stopwatch.Frequency * 1000.0;
	}

	public double method_0()
	{
		return Stopwatch_0.ElapsedTicks;
	}

	public GClass19.GClass20 method_1(string name)
	{
		return Gclass20_0 = Gclass19_0.StartDiff(name, method_0(), Gclass20_0);
	}

	public void method_2(GClass19.GClass20 counter)
	{
		counter.FinishDiff(method_0());
		if (Gclass20_0 == counter)
		{
			Gclass20_0 = Gclass20_0.Parent;
		}
		else
		{
			UnityEngine.Debug.LogWarningFormat("Finishing {0} but current counter is {1}", counter.GetFullName(), Gclass20_0?.GetFullName());
			Gclass20_0 = counter.Parent;
		}
		Bool_0 = true;
	}

	public void BeginSample(string name)
	{
		method_1(name);
	}

	public GStruct4 BeginSampleWithToken(string name)
	{
		GClass19.GClass20 counter = method_1(name);
		return new GStruct4(this, counter);
	}

	public void EndSample(string name)
	{
		GClass19.GClass20 counter = Gclass19_0.GetCounter(name);
		method_2(counter);
	}

	public void EndSample()
	{
		if (Gclass20_0 != null)
		{
			method_2(Gclass20_0);
		}
		else
		{
			UnityEngine.Debug.LogWarning("No current counter to finish");
		}
	}

	public void Reset()
	{
		Gclass19_0.Reset();
		Gclass20_0 = null;
		Bool_0 = false;
	}

	public void Clear()
	{
		Gclass19_0.Clear();
		Gclass20_0 = null;
	}

	public string GetStringReport()
	{
		StringBuilder_0.Clear();
		foreach (GClass19.GClass20 value in Gclass19_0.GetCounters().Values)
		{
			if (value.IsValidResultValue)
			{
				double num = smethod_0(value.Result);
				StringBuilder_0.AppendFormat("{0}: {1:0.##}\n", value.GetFullName(), num);
			}
		}
		return StringBuilder_0.ToString();
	}

	public void PrintReport()
	{
		UnityEngine.Debug.Log(GetStringReport());
	}

	[CanBeNull]
	public List<GStruct5> GetSamples(bool fullSampleName)
	{
		if (!Bool_0)
		{
			return null;
		}
		List<GStruct5> list = new List<GStruct5>();
		Dictionary<string, GClass19.GClass20>.Enumerator enumerator = Gclass19_0.GetCounters().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GClass19.GClass20 value = enumerator.Current.Value;
			if (value.IsValidResultValue)
			{
				double value2 = smethod_0(value.Result);
				list.Add(new GStruct5(fullSampleName ? value.GetFullName() : value.Name, value2));
			}
		}
		enumerator.Dispose();
		return list;
	}
}
