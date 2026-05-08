using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotDangerArea(BotOwner owner) : GClass429(owner)
{
	[NonSerialized]
	public const float HORIZONTAL_POSIBLE_OFFSET = 3f;

	[NonSerialized]
	public float NextCheckAssaultPosibility;

	[NonSerialized]
	public Dictionary<int, List<CustomNavigationPoint>> BlockedCovers = new Dictionary<int, List<CustomNavigationPoint>>();

	public void Activate()
	{
		if (BotOwner_0.Settings.FileSettings.Cover.USE_DANGER_AREAS)
		{
			BotOwner_0.BotsGroup.BotZone.ZoneDangerAreas.OnDangerAreaCreated += method_4;
			BotOwner_0.BotsGroup.BotZone.ZoneDangerAreas.OnDangerAreaRemoved += method_2;
			method_0(BotOwner_0.BotsGroup.BotZone.ZoneDangerAreas.ActiveAreas);
		}
	}

	public void method_0(HashSet<AIDangerArea> activeAreas)
	{
		foreach (AIDangerArea activeArea in activeAreas)
		{
			method_1(activeArea);
		}
	}

	public void method_1(AIDangerArea aiDangerArea)
	{
		Vector3 point = aiDangerArea.Point;
		List<CustomNavigationPoint> list = new List<CustomNavigationPoint>();
		BlockedCovers.Add(aiDangerArea.Id, list);
		if (BotOwner_0.Covers == null)
		{
			return;
		}
		List<CustomNavigationPoint> closePoints = BotOwner_0.BotsGroup.CoverPointMaster.GetClosePoints(point, BotOwner_0, aiDangerArea.Rad);
		for (int i = 0; i < closePoints.Count; i++)
		{
			CustomNavigationPoint customNavigationPoint = closePoints[i];
			if (Mathf.Abs((customNavigationPoint.Position - point).y) < 3f)
			{
				customNavigationPoint.Block();
				list.Add(customNavigationPoint);
			}
		}
	}

	public void method_2(AIDangerArea obj)
	{
		method_3(obj);
	}

	public void method_3(AIDangerArea aiDangerArea)
	{
		foreach (CustomNavigationPoint item in BlockedCovers[aiDangerArea.Id])
		{
			item.Unblock();
		}
		BlockedCovers.Remove(aiDangerArea.Id);
	}

	public void method_4(AIDangerArea obj)
	{
		method_1(obj);
	}

	public void Dispose()
	{
		if (BotOwner_0.BotsGroup != null)
		{
			BotOwner_0.BotsGroup.BotZone.ZoneDangerAreas.OnDangerAreaCreated -= method_4;
			BotOwner_0.BotsGroup.BotZone.ZoneDangerAreas.OnDangerAreaRemoved -= method_2;
		}
	}
}
