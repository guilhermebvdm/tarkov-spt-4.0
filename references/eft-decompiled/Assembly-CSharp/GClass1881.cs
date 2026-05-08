using System;
using System.Collections.Generic;

public class GClass1881<T> : Dictionary<float, T>
{
	[NonSerialized]
	public float Float_0;

	public GClass1881(params KeyValuePair<T, float>[] items)
	{
		float num = 0f;
		for (int i = 0; i < items.Length; i++)
		{
			KeyValuePair<T, float> keyValuePair = items[i];
			if (keyValuePair.Value > 0f)
			{
				num += keyValuePair.Value;
				Add(num, keyValuePair.Key);
			}
		}
		Float_0 = num;
	}

	public T Random()
	{
		float num = GClass856.Random(0f, Float_0);
		foreach (float key in base.Keys)
		{
			if (!(key < num))
			{
				return base[key];
			}
		}
		return default(T);
	}

	public override string ToString()
	{
		string text = "";
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<float, T> current = enumerator.Current;
			text = text + "{K:" + current.Key + "  v:" + current.Value.ToString() + "}";
		}
		return text;
	}
}
