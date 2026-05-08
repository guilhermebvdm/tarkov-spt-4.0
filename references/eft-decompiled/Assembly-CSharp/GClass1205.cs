using System;
using System.Collections.Generic;
using System.Linq;

public class GClass1205 : GInterface112
{
	[NonSerialized]
	public HashSet<ushort> HashSet_0;

	public int Count => HashSet_0.Count;

	public GClass1205()
	{
		HashSet_0 = new HashSet<ushort>();
	}

	public GClass1205(List<ushort> indices, List<int> pixels)
		: this()
	{
		HashSet_0.UnionWith(indices);
	}

	public GClass1205(GStruct109 result)
		: this()
	{
		HashSet_0.UnionWith(result.Indices);
	}

	public void Clear()
	{
		HashSet_0.Clear();
	}

	public bool ContainsIndex(ushort index)
	{
		return HashSet_0.Contains(index);
	}

	public int GetNumPixels(ushort index)
	{
		return 0;
	}

	public ushort[] ToIndexArray()
	{
		return HashSet_0.ToArray();
	}

	public List<KeyValuePair<ushort, int>> ToSamplesArray()
	{
		List<KeyValuePair<ushort, int>> list = new List<KeyValuePair<ushort, int>>();
		foreach (ushort item in HashSet_0)
		{
			list.Add(new KeyValuePair<ushort, int>(item, 0));
		}
		return list;
	}

	public void UnionWith(GStruct109 result)
	{
		HashSet_0.UnionWith(result.Indices);
	}
}
