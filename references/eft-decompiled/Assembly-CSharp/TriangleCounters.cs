using System;
using System.Collections.Generic;

[Serializable]
public class TriangleCounters
{
	public List<TriangleCounter> Counters = new List<TriangleCounter>();

	public int Count => Counters.Count;

	public void Clear()
	{
		Counters = new List<TriangleCounter>();
	}
}
