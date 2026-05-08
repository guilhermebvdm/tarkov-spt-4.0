using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class GClass67 : GClass65
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_2;

	[NonSerialized]
	public float Float_8;

	public bool Boolean_0
	{
		get
		{
			if (base.GClass435_0 != null && base.GClass435_0.FightAtZone)
			{
				EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
				if (goalEnemy != null && goalEnemy.Person.AIData.PlaceInfo == base.GClass435_0.CorePlace)
				{
					return true;
				}
			}
			return false;
		}
	}

	public GClass67([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (base.GClass435_0.FightAtZone)
		{
			return method_33();
		}
		return method_34();
	}

	public override void OnActivate()
	{
		BotOwner_0.ShootData.OnTriggerPressed += method_32;
		base.OnActivate();
	}

	public override string Name()
	{
		return "GluhAssKilla";
	}

	public bool ShallGoNearBoss()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.HaveSeen || Time.time - goalEnemy.PersonalLastSeenTime <= 10f))
		{
			return false;
		}
		return true;
	}

	public override bool ShallUseNow()
	{
		if (base.ShallUseNow() && base.GClass435_0 != null)
		{
			return base.GClass435_0.FightAtZone;
		}
		return false;
	}

	public override void ManualUpdate()
	{
		method_37();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = false;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.IsVisible)
		{
			flag = Time.time - goalEnemy.TimeLastSeen > 4f;
		}
		if (method_3() || BotOwner_0.Memory.IsInCover || flag)
		{
			return new AICoreActionEndStruct("DogFightAtt");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		if (base.GClass435_0 != null && !base.GClass435_0.BossShallAttack && BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCover");
		}
		if (CanSearchEnemy())
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("last");
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (base.GClass435_0 != null && base.GClass435_0.BossShallAttack)
		{
			return new AICoreActionEndStruct("BossAt");
		}
		if (Boolean_0)
		{
			return new AICoreActionEndStruct("EndPeaceAtH");
		}
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (base.GClass435_0 == null)
		{
			return base.EndHoldPosition();
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			if (Bool_2)
			{
				if (Float_2 < Time.time)
				{
					Bool_2 = false;
					return new AICoreActionEndStruct("endHoldEnab");
				}
				return AICoreActionEndStruct_1;
			}
			if (base.GClass435_0.AssaultFollowersShallAttack && Time.time - goalEnemy.TimeLastSeen < BotOwner_0.Settings.FileSettings.Boss.GLUHAR_TIME_TO_ASSAULT)
			{
				return new AICoreActionEndStruct("AssaultFoll");
			}
			if (Boolean_0)
			{
				return new AICoreActionEndStruct("EndPeaceAtH");
			}
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("IsInCover");
			}
			return AICoreActionEndStruct_1;
		}
		Bool_2 = false;
		return new AICoreActionEndStruct("VisibleCanS");
	}

	public void method_32()
	{
		if (base.GClass435_0 != null)
		{
			base.GClass435_0.AssaultFollowersShallAttack = true;
		}
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_33()
	{
		method_35();
		if (CustomNavigationPoint_2 != null && method_36())
		{
			if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.CurCustomCoverPoint.Id == CustomNavigationPoint_2.Id)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "IsEnemyAtMyPlace");
			}
			if (CustomNavigationPoint_2.CanIShootToEnemy)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "CanIShootEn");
			}
		}
		if (base.GClass435_0 != null && (base.GClass435_0.SecurityFightAtHomeEnought || base.GClass435_0.SecurityCount == 0))
		{
			return base.GetDecision();
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootZone");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "FightAtZone");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_34()
	{
		if (base.GClass435_0 != null && !base.GClass435_0.AssaultFollowersShallAttack && BotOwner_0.BotFollower.BossToFollow != null)
		{
			if (ShallGoNearBoss())
			{
				if ((BotOwner_0.BotFollower.BossToFollow.Position - BotOwner_0.Position).sqrMagnitude > 100f)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "sDistBoss>1");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "sDistBoss>1");
			}
			return base.GetDecision();
		}
		return base.GetDecision();
	}

	public void method_35()
	{
		if (!(Float_8 < Time.time))
		{
			return;
		}
		ShootPointClass shoot2point = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		CoverSearchData data = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, 900f, 0f, CoverSearchType.distToToCenter, shoot2point, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f), PointsArrayType.byShootType, useSelfFindPoint: false, base.GClass435_0.CorePlace.AreaId);
		Float_8 = Time.time + 3f;
		CustomNavigationPoint coverPointMain = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: false);
		if (coverPointMain == CustomNavigationPoint_2)
		{
			return;
		}
		if (coverPointMain != null && coverPointMain.CanIShootToEnemy)
		{
			CustomNavigationPoint_2 = coverPointMain;
			CustomNavigationPoint_2.SetOwner(BotOwner_0);
			return;
		}
		if (CustomNavigationPoint_2 != null)
		{
			CustomNavigationPoint_2.SetFree();
		}
		CustomNavigationPoint_2 = null;
	}

	public bool method_36()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			return goalEnemy.Person.AIData.PlaceInfo != base.GClass435_0.CorePlace;
		}
		return false;
	}

	public void method_37()
	{
		if (base.GClass435_0 != null && !base.GClass435_0.AssaultFollowersShallAttack)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && goalEnemy.IsVisible)
			{
				base.GClass435_0.AssaultFollowersShallAttack = true;
			}
		}
	}

	public override void Dispose()
	{
		BotOwner_0.ShootData.OnTriggerPressed -= method_32;
		base.Dispose();
	}
}
