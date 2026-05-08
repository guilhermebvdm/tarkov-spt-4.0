using System;
using EFT;
using UnityEngine;

public class GClass144 : GClass142
{
	[NonSerialized]
	public GClass453<GClass435> Gclass453_0;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_2;

	[NonSerialized]
	public float Float_11;

	public GClass144(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass435>(bot);
	}

	public override string Name()
	{
		return "SecurityGluhar";
	}

	public override void OnActivate()
	{
		Gclass453_0.FindBoss();
		base.OnActivate();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (Gclass453_0 != null && !Gclass453_0.BossLogic.BossShallAttack)
		{
			if (Gclass453_0.BossLogic.SecurityWannaAttack)
			{
				return base.GetDecision();
			}
			if (Gclass453_0.BossLogic.FightAtZone)
			{
				return method_20();
			}
			return base.GetDecision();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "BossAt");
	}

	public bool FightAtHome()
	{
		if (Gclass453_0.BossLogic.SecurityFightAtHomeEnought)
		{
			return CustomNavigationPoint_2 != null;
		}
		return false;
	}

	public void CalcInnerCover()
	{
		if (!(Float_11 < Time.time) || Gclass453_0 == null)
		{
			return;
		}
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 position = BotOwner_0.Position;
		int num = 900;
		CoverSearchData data = new CoverSearchData(placeInfo: (Gclass453_0.BossLogic.CorePlace != null) ? Gclass453_0.BossLogic.CorePlace.AreaId : (-1), centerPos: position, bot: BotOwner_0.CoverSearchInfo, shootType: CoverShootType.shoot, maxDistSqr: num, minDistSqr: 0f, searchType: CoverSearchType.distToToCenter, shoot2point: shoot2point, closeFriendCover: null, pointToBeClose: null, shootHide: ECheckSHootHide.shootAndHide, coverSearchDefenceData: new CoverSearchDefenceDataClass(0f), arrayType: PointsArrayType.byShootType, useSelfFindPoint: false);
		Float_11 = Time.time + 3f;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		if (coverPointMain == CustomNavigationPoint_2)
		{
			return;
		}
		if (coverPointMain != null && coverPointMain.CanIShootToEnemy)
		{
			CustomNavigationPoint_2 = coverPointMain;
			CustomNavigationPoint_2.SetOwner(BotOwner_0);
		}
		else
		{
			if (CustomNavigationPoint_2 != null)
			{
				CustomNavigationPoint_2.SetFree();
			}
			CustomNavigationPoint_2 = null;
		}
		Gclass453_0.BossLogic.SetHaveCover(BotOwner_0.Id, CustomNavigationPoint_2 != null);
	}

	public override bool ProtectWantKill()
	{
		Gclass453_0.FindBoss();
		return Gclass453_0.BossLogic.SecurityWannaAttack;
	}

	public override bool ProtectCareKill()
	{
		return ProtectWantKill();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (Gclass453_0 != null && Gclass453_0.BossLogic.FightAtZone && FightAtHome())
		{
			return new AICoreActionEndStruct("FightAtHome");
		}
		return base.EndHoldPosition();
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("null");
		}
		if (goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("IsVisible");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_20()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "enemynull");
		}
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (FightAtHome())
		{
			if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.CurCustomCoverPoint == CustomNavigationPoint_2)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "FightAtHome");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "FightAtHome");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "FightAtZone");
	}

	public override void Dispose()
	{
		Gclass453_0.Dispose();
		base.Dispose();
	}
}
