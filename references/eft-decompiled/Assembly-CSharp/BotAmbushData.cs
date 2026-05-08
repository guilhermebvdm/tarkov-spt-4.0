using System;
using EFT;
using UnityEngine;

public class BotAmbushData : GClass429
{
	[NonSerialized]
	public const int MAX_AMBUSH_DANGER_LEVEL = 1;

	[NonSerialized]
	public CustomNavigationPoint CoverInMiddle;

	public BotAmbushData(BotOwner owner)
		: base(owner)
	{
	}

	public bool TryGetAmbushPoint(out CustomNavigationPoint ambushPoint)
	{
		if (CoverInMiddle != null && CoverInMiddle.IsSpotted)
		{
			CoverInMiddle = null;
			ambushPoint = null;
			return false;
		}
		if (CoverInMiddle != null)
		{
			ambushPoint = CoverInMiddle;
			return true;
		}
		ambushPoint = null;
		return false;
	}

	public void SetCoverAtMiddle(CustomNavigationPoint coverInMiddle)
	{
		CoverInMiddle = coverInMiddle;
	}

	public CoverSearchData GetCoverSearchData()
	{
		Vector3 position = BotOwner_0.Transform.position;
		Vector3 vector = BotOwner_0.Position;
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			vector = BotOwner_0.Memory.GoalEnemy.CurrPosition;
		}
		else if (BotOwner_0.Memory.GoalTarget.GoalTarget != null)
		{
			vector = BotOwner_0.Memory.GoalTarget.GoalTarget.Position;
		}
		Vector3 centerPos = (BotOwner_0.Position + vector) / 2f;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 4f, 0f, CoverSearchType.distToToCenter, null, null, position, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL, 999))
		{
			MinSDistToCarePos = 0f,
			CenterPos = centerPos,
			searchLabel = "Ambush",
			shootType = CoverShootType.hide,
			CheckShootHide = ECheckSHootHide.shootAndHide,
			coverSearchDefenceData = 
			{
				minDefenceLevel = BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL,
				maxDangerLevel = 1
			}
		};
	}

	public bool HaveCover()
	{
		return CoverInMiddle != null;
	}
}
