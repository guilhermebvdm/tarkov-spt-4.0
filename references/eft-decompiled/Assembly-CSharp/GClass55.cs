using System;
using EFT;
using UnityEngine;

public abstract class GClass55 : GClass50
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_3;

	public GClass55(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && (!CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) || (Bool_4 && CustomNavigationPoint_0.Special.HasFlag(ECoverPointSpecial.forFollowers)) || CustomNavigationPoint_0.IsSpotted))
		{
			CustomNavigationPoint_0 = null;
		}
		if (CustomNavigationPoint_0 != null)
		{
			return CustomNavigationPoint_0;
		}
		if ((Vector3_0 - BotOwner_0.Position).magnitude < 50f)
		{
			data.CenterPos = BotOwner_0.Position;
		}
		else
		{
			data.CenterPos = Vector3_0;
		}
		if (Bool_4)
		{
			data.Special = ECoverPointSpecial.forFollowers;
		}
		data.SearchType = CoverSearchType.distToToCenter;
		CustomNavigationPoint customNavigationPoint = base.FindPoint(data, p, checkCurrent);
		if (customNavigationPoint == null)
		{
			data.CheckShootHide = ECheckSHootHide.hide;
			data.ArrayType = PointsArrayType.covers;
			customNavigationPoint = base.FindPoint(data, p, checkCurrent);
		}
		return customNavigationPoint;
	}

	public override AICoreActionEndStruct EndRunAwayGrenade()
	{
		return base.EndRunAwayGrenade();
	}

	public void method_15()
	{
		if (Float_3 < Time.time)
		{
			if (CustomNavigationPoint_0 != null && ((!CustomNavigationPoint_0.CanIShootToEnemy && BotOwner_0.Memory.HaveEnemy) || !CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) || CustomNavigationPoint_0.IsSpotted))
			{
				CustomNavigationPoint_0 = null;
			}
			CoverSearchData data = method_16();
			Float_3 = 1f + Time.time;
			CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		}
	}

	public CoverSearchData method_16()
	{
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverSearchData coverSearchData = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 5625f, 0f, CoverSearchType.distToToCenter, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(-1f));
		if (Bool_4)
		{
			coverSearchData.Special = ECoverPointSpecial.forFollowers;
		}
		return coverSearchData;
	}
}
