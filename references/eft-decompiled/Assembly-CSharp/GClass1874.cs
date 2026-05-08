using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass1874
{
	[CompilerGenerated]
	public class Class1154
	{
		public Vector3 position;

		public float radius;

		public bool method_0(BotOwner owner)
		{
			return GClass856.SqrDistance(owner.Position, position) < radius * radius;
		}
	}

	public static readonly GClass1875 DefaultZoneData = new GClass1875(Vector3.zero, 0f, 0f);

	public Dictionary<string, GClass1875> ActiveZonesOnMap = new Dictionary<string, GClass1875>();

	[NonSerialized]
	public BotsController BotsController_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public List<string> List_0 = new List<string>();

	[NonSerialized]
	public string String_0;

	public GClass1874(BotsController botsController)
	{
		BotsController_0 = botsController;
		String_0 = "BotArtilleryZonesController";
	}

	public void Activate()
	{
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnArtilleryStart += method_1;
			instance.OnArtilleryEnd += method_2;
			instance.OnArtilleryStartWithTimer += method_3;
		}
	}

	public void ManualUpdate()
	{
		if (ActiveZonesOnMap.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, GClass1875> item in ActiveZonesOnMap)
		{
			if (item.Value.StartTime > 0f && Time.time > item.Value.StartTime + item.Value.Duration)
			{
				method_5(item.Key);
			}
		}
		method_0();
	}

	public void Dispose()
	{
		BotEventHandler instance = Singleton<BotEventHandler>.Instance;
		if (instance != null)
		{
			instance.OnArtilleryStart -= method_1;
			instance.OnArtilleryEnd -= method_2;
			instance.OnArtilleryStartWithTimer -= method_3;
		}
	}

	public GClass1875 GetZoneById(string zoneId)
	{
		if (!ActiveZonesOnMap.TryGetValue(zoneId, out var value))
		{
			return DefaultZoneData;
		}
		return value;
	}

	public bool TryGetZoneById(string zoneId, out GClass1875 zoneData)
	{
		return ActiveZonesOnMap.TryGetValue(zoneId, out zoneData);
	}

	public void method_0()
	{
		if (List_0.Count != 0)
		{
			for (int num = List_0.Count - 1; num >= 0; num--)
			{
				ActiveZonesOnMap.Remove(List_0[num]);
				List_0.RemoveAt(num);
			}
		}
	}

	public void method_1(string zoneId, Vector3 position, float radius)
	{
		if (!string.IsNullOrEmpty(zoneId))
		{
			method_4(zoneId, position, radius);
		}
	}

	public void method_2(string zoneId, Vector3 position, float radius)
	{
		if (!string.IsNullOrEmpty(zoneId))
		{
			method_5(zoneId);
		}
	}

	public void method_3(Vector3 position, float radius, float duration)
	{
		if (Time.time - Float_0 < 1f)
		{
			foreach (KeyValuePair<string, GClass1875> item in ActiveZonesOnMap)
			{
				if (!(GClass856.SqrDistance(item.Value.Position, position) >= 1f))
				{
					return;
				}
			}
		}
		if (BotsController_0.Bots.BotOwners.Any((BotOwner owner) => GClass856.SqrDistance(owner.Position, position) < radius * radius))
		{
			method_4("_legacy_temporal_zone_" + UnityEngine.Random.Range(0, int.MaxValue), position, radius, Time.time, duration);
		}
	}

	public void method_4(string zoneId, Vector3 position, float radius, float startTime = -1f, float duration = -1f)
	{
		if (ActiveZonesOnMap.TryGetValue(zoneId, out var value))
		{
			value.Position = position;
			value.Radius = radius;
			value.RadiusSqr = radius * radius;
			value.StartTime = startTime;
			value.Duration = duration;
		}
		else
		{
			ActiveZonesOnMap[zoneId] = new GClass1875(position, radius, radius * radius, startTime, duration);
		}
	}

	public void method_5(string zoneId)
	{
		foreach (BotOwner botOwner in BotsController_0.Bots.BotOwners)
		{
			botOwner.ArtilleryDangerPlace.OnArtilleryEnd(zoneId);
		}
		List_0.Add(zoneId);
	}
}
