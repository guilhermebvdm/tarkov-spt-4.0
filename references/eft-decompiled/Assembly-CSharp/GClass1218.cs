using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GClass1218 : GInterface112
{
	[NonSerialized]
	public Dictionary<ushort, int> Dictionary_0;

	public int Count => Dictionary_0.Count;

	public GClass1218()
	{
		Dictionary_0 = new Dictionary<ushort, int>();
	}

	public GClass1218(List<ushort> indices, List<int> pixels)
		: this()
	{
		for (int i = 0; i < indices.Count; i++)
		{
			Dictionary_0[indices[i]] = pixels[i];
		}
	}

	public bool ContainsIndex(ushort index)
	{
		return Dictionary_0.ContainsKey(index);
	}

	public int GetNumPixels(ushort index)
	{
		int value = 0;
		Dictionary_0.TryGetValue(index, out value);
		return value;
	}

	public GClass1218(GStruct109 result)
		: this()
	{
		for (int i = 0; i < result.Indices.Count; i++)
		{
			Dictionary_0[result.Indices[i]] = result.Pixels[i];
		}
	}

	public void UnionWith(GStruct109 result)
	{
		for (int i = 0; i < result.Indices.Count; i++)
		{
			ushort key = result.Indices[i];
			if (Dictionary_0.ContainsKey(key))
			{
				Dictionary_0[key] = Mathf.Max(Dictionary_0[key], result.Pixels[i]);
			}
			else
			{
				Dictionary_0[key] = result.Pixels[i];
			}
		}
	}

	public ushort[] ToIndexArray()
	{
		return Dictionary_0.Keys.ToArray();
	}

	public List<KeyValuePair<ushort, int>> ToSamplesArray()
	{
		List<KeyValuePair<ushort, int>> list = new List<KeyValuePair<ushort, int>>();
		foreach (KeyValuePair<ushort, int> item in Dictionary_0)
		{
			list.Add(item);
		}
		return list;
	}

	public void Clear()
	{
		Dictionary_0.Clear();
	}
}
