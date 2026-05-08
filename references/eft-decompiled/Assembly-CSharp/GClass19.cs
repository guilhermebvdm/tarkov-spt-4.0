using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public class GClass19
{
	public class GClass20
	{
		public string Name;

		public double Result;

		public GClass20 Parent;

		[NonSerialized]
		public double Double_0;

		public bool IsInProgress;

		public bool IsValidResultValue;

		public void Init(string name)
		{
			Name = name;
			Reset();
		}

		public void StartDiff(double value, GClass20 parent = null)
		{
			Double_0 = value;
			Parent = parent;
			IsInProgress = true;
			IsValidResultValue = false;
		}

		public void FinishDiff(double value)
		{
			Result = value - Double_0;
			IsInProgress = false;
			IsValidResultValue = true;
		}

		public void Reset()
		{
			Double_0 = 0.0;
			Result = 0.0;
			Parent = null;
			IsInProgress = false;
			IsValidResultValue = false;
		}

		public string GetFullName()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Name);
			GClass20 gClass = this;
			while (gClass.Parent != null)
			{
				gClass = gClass.Parent;
				stringBuilder.Insert(0, gClass.Name + ".");
			}
			return stringBuilder.ToString();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class98
	{
		public static readonly Class98 class98_0 = new Class98();

		public static Func<GClass20> func_0;

		public GClass20 method_0()
		{
			return new GClass20();
		}
	}

	[NonSerialized]
	public GClass835<GClass20> Gclass835_0 = new GClass835<GClass20>(1000, () => new GClass20());

	[NonSerialized]
	public Dictionary<string, GClass20> Dictionary_0 = new Dictionary<string, GClass20>();

	[NonSerialized]
	public StringBuilder StringBuilder_0 = new StringBuilder();

	public GClass20 StartDiff(string name, double value, GClass20 parent = null)
	{
		GClass20 counter = GetCounter(name);
		counter.StartDiff(value, parent);
		return counter;
	}

	public void FinishDiff(string name, double value)
	{
		GetCounter(name).FinishDiff(value);
	}

	public GClass20 GetCounter(string name)
	{
		if (!Dictionary_0.TryGetValue(name, out var value))
		{
			value = Gclass835_0.Withdraw();
			value.Init(name);
			Dictionary_0[name] = value;
		}
		return value;
	}

	public string GetReport(bool fullName = true)
	{
		StringBuilder_0.Clear();
		foreach (GClass20 value in Dictionary_0.Values)
		{
			if (value.IsValidResultValue)
			{
				StringBuilder_0.AppendFormat("{0}: {1:0.##}\n", fullName ? value.GetFullName() : value.Name, value.Result);
			}
		}
		return StringBuilder_0.ToString();
	}

	public void Reset()
	{
		Dictionary<string, GClass20>.Enumerator enumerator = Dictionary_0.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Reset();
		}
		enumerator.Dispose();
	}

	public void Clear()
	{
		Dictionary<string, GClass20>.Enumerator enumerator = Dictionary_0.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Gclass835_0.Return(enumerator.Current.Value);
		}
		enumerator.Dispose();
		Dictionary_0.Clear();
	}

	public Dictionary<string, GClass20> GetCounters()
	{
		return Dictionary_0;
	}
}
