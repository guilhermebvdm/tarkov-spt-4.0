using System;
using System.Collections.Generic;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class PatrolExfiltrationPointsData : GClass429
{
	[NonSerialized]
	public const float MAX_FAR_SDIST = 90000f;

	[NonSerialized]
	public float NextCheck;

	[NonSerialized]
	public float DropPointTime;

	[NonSerialized]
	public float LastTimeToPoint;

	[NonSerialized]
	public float TimeDropVisited;

	[NonSerialized]
	public float StayAtPointPeriod;

	[NonSerialized]
	public AIExfiltrationPoint CachedExfiltrationPoint_1;

	[NonSerialized]
	public bool AtPointNow;

	public AIExfiltrationPoint CachedExfiltrationPoint => CachedExfiltrationPoint_1;

	public PatrolExfiltrationPointsData(BotOwner owner)
		: base(owner)
	{
	}

	public bool HaveActions()
	{
		if (CachedExfiltrationPoint_1 == null)
		{
			UpdateFindNextExfiltrationPoint();
		}
		return CachedExfiltrationPoint_1 != null;
	}

	public void UpdateFindNextExfiltrationPoint()
	{
		if (NextCheck > Time.time)
		{
			return;
		}
		NextCheck = Time.time + GClass856.Random(5f, 6f);
		AtPointNow = false;
		List<AIExfiltrationPoint> exfiltrationPoints = BotOwner_0.BotsController.CoversData.Patrols.ExfiltrationPoints;
		float num = float.MaxValue;
		foreach (AIExfiltrationPoint item in exfiltrationPoints)
		{
			if (item.CorePointInGame != null && item.CorePointInGame.ConnectionGroupId != BotOwner_0.StartCorePoint.ConnectionGroupId)
			{
				continue;
			}
			if (item.ExfiltrationPoint == null)
			{
				for (int i = 0; i < ExfiltrationControllerClass.Instance.ExfiltrationPoints.Length; i++)
				{
					ExfiltrationPoint exfiltrationPoint = ExfiltrationControllerClass.Instance.ExfiltrationPoints[i];
					if (exfiltrationPoint.Settings.Name == item.Name)
					{
						item.LinkExfiltration(exfiltrationPoint);
						break;
					}
				}
			}
			if (item.ExfiltrationPoint == null || item.ExfiltrationPoint.Status == EExfiltrationStatus.NotPresent)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < item.ExfiltrationPoint.Requirements.Length; j++)
			{
				if (item.ExfiltrationPoint.Requirements[j].Requirement != ERequirementState.None)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				float num2 = GClass856.SqrDistance(BotOwner_0.Position, item.Position);
				if (num2 < num)
				{
					num = num2;
					CachedExfiltrationPoint_1 = item;
				}
			}
		}
	}

	public void ComeToExfiltrationPoint()
	{
		if (!AtPointNow)
		{
			LastTimeToPoint = Time.time;
			AtPointNow = true;
			StayAtPointPeriod = GClass856.Random(5f, 15f);
		}
	}
}
