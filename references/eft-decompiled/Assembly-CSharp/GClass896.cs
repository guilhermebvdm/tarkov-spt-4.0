using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass896
{
	[NonSerialized]
	public Dictionary<string, List<GClass895>> Dictionary_0 = new Dictionary<string, List<GClass895>>();

	[NonSerialized]
	public GClass944<GClass895> Gclass944_0 = new GClass944<GClass895>(10, 5);

	public void PrecacheGag(string chokeKey)
	{
		if (!Dictionary_0.ContainsKey(chokeKey))
		{
			Dictionary_0.Add(chokeKey, new List<GClass895>(10));
		}
	}

	public void InitGag(Vector3 position, float clipLength, float rolloff, float chokeTime, Vector2 gagRadius, string chokeKey = "")
	{
		GClass895 gClass = Gclass944_0.GetObject();
		gClass.EndTime = AudioSettings.dspTime + (double)(clipLength * chokeTime);
		gClass.Radius = gagRadius;
		gClass.Position = position;
		gClass.Rolloff = rolloff;
		Dictionary_0[chokeKey].Add(gClass);
	}

	public bool CheckGagsLimit(Vector3 position, Vector3 listenerPos, float sqrDistance, string chokeKey = "")
	{
		if (Dictionary_0.TryGetValue(chokeKey, out var value))
		{
			for (int num = value.Count - 1; num > -1; num--)
			{
				GClass895 item = value[num];
				if (!value[num].IsActive)
				{
					Gclass944_0.PutObject(item);
					value.RemoveAt(num);
				}
				else if (value[num].IsChoked(position, listenerPos, sqrDistance))
				{
					return false;
				}
			}
		}
		else
		{
			Dictionary_0.Add(chokeKey, new List<GClass895>(10));
		}
		return true;
	}
}
