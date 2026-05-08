using System;
using EFT;
using UnityEngine;

public class GClass58 : GClass55
{
	[NonSerialized]
	public GClass430 Gclass430_0;

	public GClass58(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			return aICoreActionResultStruct.Value;
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "cdg");
		}
		StationaryWeaponLink stationaryWeaponLink = BotOwner_0.WeaponManager.Stationary.CheckWantTakeStationary(BotOwner_0.Settings.FileSettings.Cover.STATIONARY_WEAPON_MAX_DIST_TO_USE);
		bool flag = true;
		if (stationaryWeaponLink != null && !stationaryWeaponLink.CanTakeByPrevDeath(9999f))
		{
			flag = false;
			if (Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.LastShootTime < 3f || (BotOwner_0.Brain.LastDecision.HasValue && BotOwner_0.Brain.LastDecision.Value == BotLogicDecision.runToStationary))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			stationaryWeaponLink = null;
		}
		if (stationaryWeaponLink != null)
		{
			BotLogicDecision? currentDecision = BotOwner_0.WeaponManager.Stationary.GetCurrentDecision();
			if (currentDecision.HasValue)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(currentDecision.Value, "stationaryW");
			}
		}
		if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			if (Gclass430_0.IsHitted)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfp");
			}
			if (BotOwner_0.BewareGrenade.SawGrenadeSoFar(5f))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "aftGr");
			}
		}
		method_15();
		if (!BotOwner_0.Memory.IsInCover)
		{
			bool flag2 = false;
			if (CustomNavigationPoint_0 != null)
			{
				if (!CustomNavigationPoint_0.CanIShootToEnemy)
				{
					flag2 = true;
				}
				else if ((CustomNavigationPoint_0.Position - BotOwner_0.Position).magnitude > 20f)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "doRun");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "toCvr");
		}
		if (Gclass430_0.IsHitted && BotOwner_0.Memory.HaveEnemy)
		{
			if (BotOwner_0.Memory.GoalEnemy.IsVisible && BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfp");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "rfp");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdAnw");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "FBoarFght";
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.CanShoot && BotOwner_0.BewareGrenade.SawGrenadeSoFar(5f))
		{
			return new AICoreActionEndStruct("saw grenade");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		return AICoreActionEndStruct_1;
	}

	public override void OnActivate()
	{
		if (Gclass430_0 == null)
		{
			if (BotOwner_0.BotFollower.HaveBoss)
			{
				method_17(BotOwner_0.BotFollower.BossToFollow);
			}
			else
			{
				BotOwner_0.BotFollower.OnBossFinded += method_17;
			}
		}
		base.OnActivate();
	}

	public override AICoreActionEndStruct EndRunToStationary()
	{
		if (BotOwner_0.Memory.HaveEnemy && !BotOwner_0.WeaponManager.Stationary.IsEnemyAtSector(BotOwner_0.WeaponManager.Stationary.CurLink, BotOwner_0.Memory.GoalEnemy.CurrPosition))
		{
			return new AICoreActionEndStruct("notAtSector");
		}
		return base.EndRunToStationary();
	}

	public override AICoreActionEndStruct EndSuppressStationary()
	{
		return new AICoreActionEndStruct("justEnd");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_6(out var _))
		{
			return new AICoreActionEndStruct("cst");
		}
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsFreeById(BotOwner_0.Id))
		{
			CustomNavigationPoint_0 = null;
		}
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy && BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Id != CustomNavigationPoint_0.Id && Time.time - BotOwner_0.Memory.ComeToCoverTime > 3f && Gclass430_0 != null && Gclass430_0.CanStartMoveToAttackPoint(BotOwner_0))
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			Gclass430_0?.StartMoveToAttackPoint(BotOwner_0.Id);
			return new AICoreActionEndStruct("betterCover");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("notInCover");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		if (Gclass430_0.IsHitted)
		{
			return new AICoreActionEndStruct("bossHit");
		}
		method_15();
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return base.EndAttackMoving();
	}

	public void method_17(IBossToFollow obj)
	{
		BotOwner_0.BotFollower.OnBossFinded -= method_17;
		Gclass430_0 = BotOwner_0.BotFollower.BossToFollow.GetBossLogic() as GClass430;
	}

	public override void Dispose()
	{
		BotOwner_0.BotFollower.OnBossFinded -= method_17;
		base.Dispose();
	}
}
