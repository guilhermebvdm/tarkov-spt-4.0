using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class ArtilleryDangerPlace : GClass429
{
	[Serializable]
	[CompilerGenerated]
	public class Class277
	{
		public static readonly Class277 class277_0 = new Class277();

		public static Func<CustomNavigationPoint, bool> func_0;

		public bool method_0(CustomNavigationPoint coverPoint)
		{
			return coverPoint.EnvironmentType == EnvironmentType.Outdoor;
		}
	}

	public Dictionary<string, List<CustomNavigationPoint>> ActiveZonesSpottedCovers = new Dictionary<string, List<CustomNavigationPoint>>();

	[NonSerialized]
	public float LastCheckTime;

	[NonSerialized]
	public List<string> ZonesToRemove = new List<string>();

	[NonSerialized]
	public float MaxActiveZoneRadius;

	public bool IsActive => ActiveZonesSpottedCovers.Count != 0;

	public float MaxDangerRadius => MaxActiveZoneRadius;

	public ArtilleryDangerPlace(BotOwner owner)
		: base(owner)
	{
	}

	public bool ShallEnd()
	{
		return !IsActive;
	}

	public bool ShallRunAway()
	{
		return IsActive;
	}

	public void ManualUpdate()
	{
		if (BotOwner_0.BotsController.ArtilleryZonesController.ActiveZonesOnMap.Count != 0 && !(Time.time <= LastCheckTime))
		{
			LastCheckTime = Time.time + GClass856.Random(1f, 2f);
			method_0();
			method_2();
			method_3();
		}
	}

	public void Dispose()
	{
		ActiveZonesSpottedCovers.Clear();
		ZonesToRemove.Clear();
	}

	public void OnArtilleryEnd(string zoneId)
	{
		ZonesToRemove.Add(zoneId);
		ActiveZonesSpottedCovers.Remove(zoneId);
	}

	public (string zoneId, GClass1875 zoneData) GetClosestZone()
	{
		if (ActiveZonesSpottedCovers.Count == 0)
		{
			return (zoneId: string.Empty, zoneData: null);
		}
		if (ActiveZonesSpottedCovers.Count == 1)
		{
			KeyValuePair<string, List<CustomNavigationPoint>> keyValuePair = ActiveZonesSpottedCovers.First();
			return (zoneId: keyValuePair.Key, zoneData: BotOwner_0.BotsController.ArtilleryZonesController.GetZoneById(keyValuePair.Key));
		}
		float num = float.MaxValue;
		string item = string.Empty;
		GClass1875 item2 = null;
		foreach (KeyValuePair<string, List<CustomNavigationPoint>> activeZonesSpottedCover in ActiveZonesSpottedCovers)
		{
			if (BotOwner_0.BotsController.ArtilleryZonesController.TryGetZoneById(activeZonesSpottedCover.Key, out var zoneData))
			{
				float num2 = GClass856.SqrDistance(BotOwner_0.Position, zoneData.Position);
				if (num2 < num)
				{
					num = num2;
					item = activeZonesSpottedCover.Key;
					item2 = zoneData;
				}
			}
		}
		return (zoneId: item, zoneData: item2);
	}

	public void method_0()
	{
		foreach (KeyValuePair<string, GClass1875> item in BotOwner_0.BotsController.ArtilleryZonesController.ActiveZonesOnMap)
		{
			if (!ActiveZonesSpottedCovers.ContainsKey(item.Key) && GClass856.SqrDistance(BotOwner_0.Position, item.Value.Position) < item.Value.RadiusSqr)
			{
				method_1(item.Key, item.Value);
			}
		}
	}

	public void method_1(string zoneId, GClass1875 zoneData)
	{
		BotOwner_0.BotTalk.Say(EPhraseTrigger.GetInCover, sayImmediately: true);
		ActiveZonesSpottedCovers[zoneId] = (from coverPoint in BotOwner_0.Covers.GetClosePoints(BotOwner_0.Position, zoneData.Radius)
			where coverPoint.EnvironmentType == EnvironmentType.Outdoor
			select coverPoint).ToList();
		foreach (CustomNavigationPoint item in ActiveZonesSpottedCovers[zoneId])
		{
			item.Spotted(2f);
		}
		method_4();
	}

	public void method_2()
	{
		foreach (KeyValuePair<string, List<CustomNavigationPoint>> activeZonesSpottedCover in ActiveZonesSpottedCovers)
		{
			foreach (CustomNavigationPoint item in activeZonesSpottedCover.Value)
			{
				item.Spotted(2f);
			}
		}
	}

	public void method_3()
	{
		if (ZonesToRemove.Count != 0)
		{
			for (int num = ZonesToRemove.Count - 1; num >= 0; num--)
			{
				ActiveZonesSpottedCovers.Remove(ZonesToRemove[num]);
				ZonesToRemove.RemoveAt(num);
			}
			method_4();
		}
	}

	public void method_4()
	{
		float num = 0f;
		foreach (KeyValuePair<string, List<CustomNavigationPoint>> activeZonesSpottedCover in ActiveZonesSpottedCovers)
		{
			if (BotOwner_0.BotsController.ArtilleryZonesController.TryGetZoneById(activeZonesSpottedCover.Key, out var zoneData) && zoneData.Radius > num)
			{
				num = zoneData.Radius;
			}
		}
		MaxActiveZoneRadius = num;
	}

	public Vector3 GetAveragePositionOfAllZonesCenters()
	{
		if (ActiveZonesSpottedCovers.Count == 0)
		{
			return Vector3.zero;
		}
		int num = 0;
		Vector3 zero = Vector3.zero;
		foreach (KeyValuePair<string, List<CustomNavigationPoint>> activeZonesSpottedCover in ActiveZonesSpottedCovers)
		{
			if (BotOwner_0.BotsController.ArtilleryZonesController.TryGetZoneById(activeZonesSpottedCover.Key, out var zoneData))
			{
				zero += zoneData.Position;
				num++;
			}
		}
		return zero / num;
	}
}
