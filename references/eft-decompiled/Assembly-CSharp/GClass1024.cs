using System;
using System.Collections;
using System.Collections.Generic;
using MultiFlare;

public class GClass1024 : IEnumerable<GClass1027>, IEnumerable
{
	[NonSerialized]
	public Dictionary<FlareType, GClass1027> Dictionary_0 = new Dictionary<FlareType, GClass1027>();

	[NonSerialized]
	public List<GClass1027> List_0 = new List<GClass1027>();

	[NonSerialized]
	public List<GClass1027> List_1 = new List<GClass1027>();

	public GClass1027 this[FlareType flareType] => Dictionary_0[flareType];

	public IReadOnlyList<GClass1027> DefaultBatches => List_0;

	public IReadOnlyList<GClass1027> InstantBatches => List_1;

	public void Register(FlareType flareType, GClass1027 batch)
	{
		Dictionary_0[flareType] = batch;
		if (batch.IsInstant)
		{
			List_1.Add(batch);
		}
		else
		{
			List_0.Add(batch);
		}
	}

	public IEnumerator<GClass1027> GetEnumerator()
	{
		foreach (GClass1027 value in Dictionary_0.Values)
		{
			yield return value;
		}
	}

	public void Clear()
	{
		Dictionary_0.Clear();
		List_1.Clear();
		List_0.Clear();
	}

	public IEnumerator GetEnumerator_1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}
}
