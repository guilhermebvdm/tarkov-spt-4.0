using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class GClass951
{
	public class Class598<T>
	{
		public float Average;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public List<KeyValuePair<float, T>> List_0;

		[NonSerialized]
		public float Float_1;

		public List<int> Items = new List<int>();

		public float LeftMost => Average - Float_0;

		public float RightMost => Average + Float_0;

		public Class598(List<KeyValuePair<float, T>> values, float targetDistance)
		{
			List_0 = values;
			Float_1 = targetDistance;
		}

		public void SpaceOut()
		{
			if (Items.Count >= 1)
			{
				Average = Items.Average((int index) => List_0[index].Key);
				Float_0 = (float)(Items.Count - 1) * Float_1 * 0.5f;
			}
		}

		public void Merge(Class598<T> group, float period)
		{
			float num = Average * (float)Items.Count + group.Items.Sum((int index) => List_0[index].Key);
			float num2 = group.Average - Average;
			if (Math.Abs(num2) >= period * 0.5f)
			{
				if (num2 > 0f)
				{
					num -= (float)group.Items.Count * period;
					foreach (int item in group.Items)
					{
						KeyValuePair<float, T> keyValuePair = List_0[item];
						keyValuePair = new KeyValuePair<float, T>(keyValuePair.Key - period, keyValuePair.Value);
						List_0[item] = keyValuePair;
					}
				}
				else
				{
					num += (float)group.Items.Count * period;
					foreach (int item2 in group.Items)
					{
						KeyValuePair<float, T> keyValuePair2 = List_0[item2];
						keyValuePair2 = new KeyValuePair<float, T>(keyValuePair2.Key + period, keyValuePair2.Value);
						List_0[item2] = keyValuePair2;
					}
				}
			}
			Items.AddRange(group.Items);
			Average = num / (float)Items.Count;
			Float_0 = (float)(Items.Count - 1) * Float_1 * 0.5f;
		}

		public void ApplySpacing()
		{
			float num = LeftMost;
			Items.Sort((int l, int r) => List_0[l].Key.CompareTo(List_0[r].Key));
			foreach (int item in Items)
			{
				List_0[item] = new KeyValuePair<float, T>(num, List_0[item].Value);
				num += Float_1;
			}
		}

		[CompilerGenerated]
		public float method_0(int index)
		{
			return List_0[index].Key;
		}

		[CompilerGenerated]
		public float method_1(int index)
		{
			return List_0[index].Key;
		}

		[CompilerGenerated]
		public int method_2(int l, int r)
		{
			return List_0[l].Key.CompareTo(List_0[r].Key);
		}
	}

	[CompilerGenerated]
	public class Class599<T>
	{
		public List<KeyValuePair<float, T>> items;

		public float targetDistance;

		public Class598<T> method_0(KeyValuePair<float, T> item, int i)
		{
			return new Class598<T>(items, targetDistance)
			{
				Items = new List<int> { i }
			};
		}
	}

	public static bool smethod_0<T>(Class598<T> group1, Class598<T> group2, float targetDistance, float period)
	{
		float num = group1.LeftMost;
		float num2 = group1.RightMost;
		float num3 = group2.LeftMost;
		float num4 = group2.RightMost;
		float num5 = group2.Average - group1.Average;
		if (Math.Abs(num5) >= period * 0.5f)
		{
			if (num5 > 0f)
			{
				num += period;
				num2 += period;
			}
			else
			{
				num3 += period;
				num4 += period;
			}
		}
		if (!(num < num3))
		{
			return num < num4 + targetDistance;
		}
		return num2 + targetDistance > num3;
	}

	public static void SpaceOut<T>(List<KeyValuePair<float, T>> items, float targetDistance, float period)
	{
		List<Class598<T>> list = items.Select((KeyValuePair<float, T> item, int i) => new Class598<T>(items, targetDistance)
		{
			Items = new List<int> { i }
		}).ToList();
		bool flag;
		do
		{
			flag = false;
			foreach (Class598<T> item in list)
			{
				item.SpaceOut();
			}
			if (list.Count <= 1)
			{
				continue;
			}
			Class598<T> @class = list[0];
			for (int num = 1; num <= list.Count; num++)
			{
				if (list.Count <= 1)
				{
					break;
				}
				int index = ((num < list.Count) ? num : 0);
				Class598<T> class2 = list[index];
				if (smethod_0(@class, class2, targetDistance, period))
				{
					flag = true;
					@class.Merge(class2, period);
					list.RemoveAt(index);
					num--;
				}
				else
				{
					@class = class2;
				}
			}
		}
		while (flag && list.Count > 1);
		foreach (Class598<T> item2 in list)
		{
			item2.ApplySpacing();
		}
	}
}
