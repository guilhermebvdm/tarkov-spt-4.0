using System;
using System.Collections.Generic;
using EFT;

public class GClass1502
{
	[NonSerialized]
	public const int Int_0 = 16;

	[NonSerialized]
	public List<IEffect> List_0 = new List<IEffect>();

	[NonSerialized]
	public List<Type> List_1 = new List<Type>();

	[NonSerialized]
	public GClass1493 Gclass1493_0;

	public GClass1502()
	{
		method_0();
		Gclass1493_0 = new GClass1493();
		Gclass1493_0.SetTargetCount(16);
	}

	public void AddHealthEffect(IEffect healthEffect)
	{
		if (List_1.Contains(healthEffect.Type))
		{
			if (List_0.Count == 0)
			{
				Gclass1493_0.ResetCounter();
			}
			if (!List_0.Contains(healthEffect))
			{
				List_0.Add(healthEffect);
				Gclass1493_0.SetTargetCount(16 * List_0.Count);
			}
		}
	}

	public void RemoveHealthEffect(IEffect healthEffect)
	{
		if (List_1.Contains(healthEffect.Type))
		{
			List_0.Remove(healthEffect);
			if (List_0.Count == 0)
			{
				Gclass1493_0.ResetCounter();
			}
			else
			{
				Gclass1493_0.SetTargetCount(16 * List_0.Count);
			}
		}
	}

	public bool TryReachThreshold(EDamageType damageType)
	{
		if (GClass3051.IsSelfInflicted(damageType))
		{
			return Gclass1493_0.TryReachThreshold();
		}
		return false;
	}

	public void method_0()
	{
		List_1.Add(typeof(GInterface377));
		List_1.Add(typeof(GInterface346));
		List_1.Add(typeof(GInterface340));
		List_1.Add(typeof(GInterface339));
		List_1.Add(typeof(GInterface343));
		List_1.Add(typeof(GInterface344));
		List_1.Add(typeof(GInterface345));
		List_1.Add(typeof(GInterface348));
		List_1.Add(typeof(GInterface347));
	}
}
