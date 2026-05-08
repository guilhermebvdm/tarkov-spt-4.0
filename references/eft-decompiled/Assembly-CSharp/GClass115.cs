using System;
using EFT;
using UnityEngine;

public class GClass115 : GClass108
{
	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public const bool Bool_5 = true;

	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_7;

	public GClass115(BotOwner bot, int priority, float distToAttack)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass441>(bot);
		Gclass453_0.FindBoss();
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal > 5f)
		{
			return false;
		}
		if (!Bool_4)
		{
			return true;
		}
		if (Gclass453_0.BossLogic != null && Gclass453_0.BossLogic.AssaultCanKill)
		{
			return true;
		}
		return false;
	}

	public void method_16()
	{
		if (Float_7 > Time.time)
		{
			return;
		}
		float num = Time.time - BotOwner_0.Memory.ComeToCoverTime;
		if (!BotOwner_0.Memory.IsInCover || !(num < 2f))
		{
			CoverSearchData coverSearchData = method_17();
			coverSearchData.UseSelfFindPoint = false;
			Float_7 = 3f + Time.time;
			CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(coverSearchData, checkCurrent: false);
			if (CustomNavigationPoint_0 != null && (!CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) || CustomNavigationPoint_0.IsSpotted))
			{
				CustomNavigationPoint_0 = null;
			}
		}
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id) && !CustomNavigationPoint_0.IsSpotted)
		{
			return CustomNavigationPoint_0;
		}
		return base.FindPoint(data, p, checkCurrent);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (goalEnemy.IsVisible && goalEnemy.VisibleType == EEnemyPartVisibleType.Visible)
		{
			return method_15("jklu1");
		}
		if (goalEnemy.Distance < 18f && Float_6 < Time.time)
		{
			Float_6 = Time.time + 40f;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "atk");
		}
		method_16();
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			if (GClass856.IsTrue100(50f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMovingWithSuppress, "atwsMTE1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "atMTE2");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerSimpleAbstractClass.TryMoveToEnemy(BotOwner_0, BotLogicDecision.goToEnemy), "slowAtck");
	}

	public override AICoreActionEndStruct EndAttackMovingWithSuppress()
	{
		return base.EndAttackMovingWithSuppress();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemyIsNull");
		}
		if (goalEnemy.Distance < 1f && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("toClose");
		}
		return AICoreActionEndStruct_1;
	}

	public CoverSearchData method_17()
	{
		int num = 75;
		ShootPointClass shootPointClass = BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true);
		Vector3 centerPos = ((shootPointClass != null) ? ((shootPointClass.Point + BotOwner_0.Position) * 0.5f) : BotOwner_0.Position);
		int num2 = num * num;
		return new CoverSearchData(centerPos, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, num2, 0f, CoverSearchType.distToBot, shootPointClass, null, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(-1f))
		{
			PlaceInfo = Gclass453_0.BossLogic.AreaId
		};
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		return base.EndSuppressFire();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("shootNow");
		}
		if (BotOwner_0.Memory.IsInCover && Time.time - BotOwner_0.Memory.ComeToCoverTime < 1.5f)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("LN!");
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		BotOwner_0.BotLight.Stroboscope.EnableFor(1f);
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCvr");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("atCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		return base.EndDogFight();
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		BotOwner_0.BotLight.Stroboscope.EnableFor(1f);
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!enemy.CanS");
		}
		return AICoreActionEndStruct_1;
	}

	public void SetProtect(bool protectBoss)
	{
		Bool_4 = protectBoss;
	}

	public override void Dispose()
	{
		Gclass453_0.Dispose();
		base.Dispose();
	}

	public override string Name()
	{
		return "KlnForceAtk";
	}
}
