using System;
using EFT;
using UnityEngine;

public class GClass56 : GClass55
{
	public const float CAN_SPRESS_DELTA_SEEN = 20f;

	public const float DIST_BOSS_CAN_ATTACK = 15f;

	public const float SUPPRESS_PERIOD = 10f;

	[NonSerialized]
	public const float Float_4 = 5f;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	public GClass56(BotOwner bot, int priority)
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
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMovingWithSuppress, "toCvr");
		}
		if (method_19())
		{
			Float_8 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "supp");
		}
		HoldFor(10f);
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdAnw");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "BossBoarFight";
	}

	public override void OnActivate()
	{
		BotOwner_0.ShootData.OnTriggerPressed += method_18;
		BotOwner_0.BotPersonalStats.OnHitTarget += method_17;
		base.OnActivate();
	}

	public override void ManualUpdate()
	{
		if (!(Float_7 < Time.time))
		{
			if (Float_5 - Float_6 > 5f && BotOwner_0.Memory.IsInCover && Time.time - BotOwner_0.Memory.ComeToCoverTime > 5f)
			{
				Float_7 = Time.time + 10f;
				BotOwner_0.Memory.Spotted(byHit: false);
			}
			base.ManualUpdate();
		}
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		if (Time.time - Float_8 > 4f)
		{
			return new AICoreActionEndStruct("stopSp");
		}
		return base.EndSuppressFire();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_6(out var _))
		{
			return new AICoreActionEndStruct("cst");
		}
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy && BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Id != CustomNavigationPoint_0.Id && Time.time - BotOwner_0.Memory.ComeToCoverTime > 3f && method_20())
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
			return new AICoreActionEndStruct("betterCover");
		}
		if (method_19())
		{
			return new AICoreActionEndStruct("canSupp");
		}
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (BotOwner_0.Memory.HaveEnemy)
		{
			if (Time.time - BotOwner_0.Memory.ComeToCoverTime > 3f && method_20())
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				return new AICoreActionEndStruct("haveDelta");
			}
			if (BotOwner_0.Memory.GoalEnemy.IsVisible && BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("haveEnemy");
			}
		}
		method_15();
		return AICoreActionEndStruct_1;
	}

	public void method_17(IPlayer arg1, DamageInfoStruct arg2, EBodyPart arg3)
	{
		Float_5 = Time.time;
	}

	public void method_18()
	{
		Float_6 = Time.time;
	}

	public bool method_19()
	{
		if (Time.time - BotOwner_0.Memory.GoalEnemy.TimeLastSeen > 20f)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.HaveSeenPersonal && Time.time - Float_8 > 10f)
		{
			return goalEnemy.Distance < 100f;
		}
		return false;
	}

	public bool method_20()
	{
		float num = Time.time - BotOwner_0.Memory.GoalEnemy.TimeLastSeen;
		if (BotOwner_0.Memory.GoalEnemy.Distance < 15f && num > 3f && num < 15f)
		{
			return true;
		}
		return Time.time - BotOwner_0.Memory.LastTimeHit < 10f;
	}

	public override void Dispose()
	{
		BotOwner_0.ShootData.OnTriggerPressed -= method_18;
		BotOwner_0.BotPersonalStats.OnHitTarget -= method_17;
		base.Dispose();
	}
}
