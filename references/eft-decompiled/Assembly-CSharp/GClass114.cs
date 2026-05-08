using System;
using EFT;
using UnityEngine;

public class GClass114 : GClass108
{
	[NonSerialized]
	public GClass453<GClass441> Gclass453_0;

	[NonSerialized]
	public const bool Bool_4 = true;

	[NonSerialized]
	public const float Float_6 = 6f;

	public GClass114(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass453_0 = new GClass453<GClass441>(bot);
		Gclass453_0.FindBoss();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			return method_15("gtyr2");
		}
		if ((BotOwner_0.Medecine.FirstAid.Have2Do || BotOwner_0.Medecine.SurgicalKit.HaveWork) && BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "first aid");
		}
		return method_13();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (Time.time - BotOwner_0.Brain.Agent.LastPeriod > 1f && (!BotOwner_0.Mover.HasPathAndNoComplete || BotOwner_0.Mover.DistDestination < 0.2f))
		{
			Float_5 = Time.time + 5f;
			return new AICoreActionEndStruct("shnp");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
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

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		data.PlaceInfo = Gclass453_0.BossLogic.AreaId;
		data.CheckShootHide = ECheckSHootHide.hide;
		return base.FindPoint(data, p, checkCurrent);
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (!Gclass453_0.HaveLogic())
		{
			return false;
		}
		if (!Gclass453_0.BossLogic.AreaId.HasValue)
		{
			return false;
		}
		IAIData aIData = BotOwner_0.Memory.GoalEnemy.Person.AIData;
		if (!(aIData.PlaceInfo == null) && aIData.PlaceInfo.AreaId == Gclass453_0.BossLogic.AreaId.Value)
		{
			if (Time.time - aIData.LastResetPlaceInfo < 6f)
			{
				return true;
			}
			return false;
		}
		if (Time.time - aIData.LastResetPlaceInfo > 6f)
		{
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy != null)
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("CanShoot");
			}
			if (goalEnemy.IsVisible)
			{
				return new AICoreActionEndStruct("CLOSEANDVIS");
			}
			if ((BotOwner_0.Medecine.FirstAid.Have2Do || BotOwner_0.Medecine.SurgicalKit.HaveWork) && BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("ShallHeal");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public override string Name()
	{
		return "Kln_NIMH";
	}
}
