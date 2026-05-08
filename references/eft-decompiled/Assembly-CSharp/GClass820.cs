using System.Collections.Generic;
using UnityEngine;

public abstract class GClass820<T>
{
	public struct GStruct49<U, V>(U first, V second)
	{
		public U Item1 = first;

		public V Item2 = second;
	}

	public static T GenerateDrop(List<GStruct49<float, T>> items)
	{
		if (items.Count == 0)
		{
			return default(T);
		}
		float num = 0f;
		foreach (GStruct49<float, T> item in items)
		{
			num += item.Item1;
		}
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		foreach (GStruct49<float, T> item2 in items)
		{
			num3 += item2.Item1;
			if (!(num2 >= num3))
			{
				return item2.Item2;
			}
		}
		return items[items.Count - 1].Item2;
	}

	public static T GenerateDrop(List<GStruct49<float, T>> items, float random)
	{
		if (items.Count == 0)
		{
			return default(T);
		}
		float num = 0f;
		foreach (GStruct49<float, T> item in items)
		{
			num += item.Item1;
		}
		float num2 = 0f;
		foreach (GStruct49<float, T> item2 in items)
		{
			num2 += item2.Item1;
			if (!(random >= num2))
			{
				return item2.Item2;
			}
		}
		return items[items.Count - 1].Item2;
	}
}
