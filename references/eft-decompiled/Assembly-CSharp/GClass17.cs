using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public class GClass17
{
	public class GClass18
	{
		[NonSerialized]
		[CompilerGenerated]
		public string String_0;

		[NonSerialized]
		public double Double_0;

		[NonSerialized]
		public double[] Double_1;

		[NonSerialized]
		public int Int_0;

		public string Name
		{
			[CompilerGenerated]
			get
			{
				return String_0;
			}
			[CompilerGenerated]
			set
			{
				String_0 = value;
			}
		}

		public GClass18(string name, int measureForAVG)
		{
			Name = name;
			Double_1 = new double[measureForAVG];
		}

		public void Add(double value)
		{
			Int_0 = (Int_0 + 1) % Double_1.Length;
			Double_1[Int_0] = value;
		}

		public void StartDiff(double value)
		{
			Double_0 = value;
		}

		public void FinishDiff(double value)
		{
			Add(value - Double_0);
		}

		public double AVG()
		{
			double num = 0.0;
			if (Double_1.Length == 0)
			{
				return num;
			}
			double[] double_ = Double_1;
			foreach (double num2 in double_)
			{
				num += num2;
			}
			return num / (double)Double_1.Length;
		}

		public double LastValue()
		{
			return Double_1[Int_0];
		}
	}

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Dictionary<string, GClass18> Dictionary_0 = new Dictionary<string, GClass18>();

	public GClass17(int measureForAVG)
	{
		Int_0 = measureForAVG;
	}

	public void StartDiff(string name, double value)
	{
		method_0(name).StartDiff(value);
	}

	public void FinishDiff(string name, double value)
	{
		method_0(name).FinishDiff(value);
	}

	public GClass18 method_0(string name)
	{
		if (!Dictionary_0.TryGetValue(name, out var value))
		{
			value = new GClass18(name, Int_0);
			Dictionary_0[name] = value;
		}
		return value;
	}

	public string AVGStats()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GClass18 value in Dictionary_0.Values)
		{
			stringBuilder.AppendFormat("{0}: {1}\n", value.Name, value.AVG());
		}
		return stringBuilder.ToString();
	}

	public string LastValues()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (GClass18 value in Dictionary_0.Values)
		{
			stringBuilder.AppendFormat("{0}: {1}\n", value.Name, value.LastValue());
		}
		return stringBuilder.ToString();
	}

	public Dictionary<string, GClass18> GetCounters()
	{
		return Dictionary_0;
	}

	public void Clear()
	{
		Dictionary_0.Clear();
	}
}
