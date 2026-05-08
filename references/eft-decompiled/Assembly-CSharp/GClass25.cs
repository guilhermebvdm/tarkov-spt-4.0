using System;
using UnityEngine;

public class GClass25
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public Action Action_0;

	public GClass25(float period, Action callback, bool withStep = false)
	{
		Float_1 = period;
		Action_0 = callback;
		if (withStep)
		{
			Float_0 = Time.time + Float_1;
		}
	}

	public void Update()
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + Float_1;
			Action_0();
		}
	}
}
