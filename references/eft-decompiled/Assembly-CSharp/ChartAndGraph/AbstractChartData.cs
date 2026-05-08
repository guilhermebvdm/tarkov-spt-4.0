using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ChartAndGraph;

[Serializable]
public abstract class AbstractChartData
{
	public class Slider
	{
		public string category;

		public string group;

		public double from;

		public double to;

		public float startTime;

		public float totalTime;

		public float timeScale = 1f;

		public AnimationCurve curve;

		public bool UpdateSlider(AbstractChartData data)
		{
			float num = Time.time - startTime;
			num *= timeScale;
			if (num > totalTime)
			{
				data.SetValueInternal(category, group, to);
				return true;
			}
			float num2 = num / totalTime;
			if (curve != null)
			{
				num2 = curve.Evaluate(num2);
			}
			double value = from * (1.0 - (double)num2) + to * (double)num2;
			data.SetValueInternal(category, group, value);
			return false;
		}
	}

	[CompilerGenerated]
	public class Class1060
	{
		public string group;

		public bool method_0(Slider x)
		{
			return x.group == group;
		}
	}

	[CompilerGenerated]
	public class Class1061
	{
		public string category;

		public bool method_0(Slider x)
		{
			return x.category == category;
		}
	}

	[CompilerGenerated]
	public class Class1062
	{
		public string category;

		public string group;

		public bool method_0(Slider x)
		{
			if (x.category == category)
			{
				return x.group == group;
			}
			return false;
		}
	}

	[NonSerialized]
	public List<Slider> MSliders = new List<Slider>();

	public void RemoveSliderForGroup(string group)
	{
		MSliders.RemoveAll((Slider x) => x.group == group);
	}

	public void RemoveSliderForCategory(string category)
	{
		MSliders.RemoveAll((Slider x) => x.category == category);
	}

	public void RemoveSlider(string category, string group)
	{
		MSliders.RemoveAll((Slider x) => x.category == category && x.group == group);
	}

	public bool method_0(Slider s)
	{
		return s.UpdateSlider(this);
	}

	public void UpdateSliders()
	{
		MSliders.RemoveAll(method_0);
	}

	public abstract void SetValueInternal(string column, string row, double value);

	public AbstractChartData()
	{
	}
}
