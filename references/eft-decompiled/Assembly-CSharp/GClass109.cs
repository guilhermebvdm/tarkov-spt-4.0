using System;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

public abstract class GClass109 : GClass108
{
	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public GClass459 Gclass459_0;

	public const float DIST_TO_ATTACK_CLOSE = 15f;

	public const float DIST_TO_GET_HIT_NO_ATTACK = 5f;

	public const int MAX_ENEMIES_TO_MELEE = 1;

	[NonSerialized]
	public bool Bool_4;

	public GClass109(BotOwner bot, int priority)
		: base(bot, priority)
	{
		Gclass459_0 = GClass459.Create(bot);
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_16(EnemyInfo enemy, string info)
	{
		if (Float_8 < Time.time && !method_17())
		{
			Float_9 = Time.time;
			Gclass459_0.SetLastStartAttack();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, info);
		}
		if (enemy.CanShoot)
		{
			return method_15(info);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, info);
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (Float_7 < Time.time)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && goalEnemy.Distance < 10f)
			{
				if (goalEnemy.Person.HealthController is ActiveHealthController activeHealthController)
				{
					activeHealthController.AddMisfireEffect(5f);
				}
				Float_7 = Time.time + 4f;
			}
		}
		AICoreActionEndStruct result = method_18();
		if (result.Value)
		{
			Float_8 = Time.time + 15f;
		}
		return result;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_14())
		{
			Float_4 = Time.time;
			return new AICoreActionEndStruct("CvrNtFnd");
		}
		return base.EndRunToCover();
	}

	public bool method_17()
	{
		int num = 0;
		foreach (EnemyInfo value in BotOwner_0.EnemiesController.EnemyInfos.Values)
		{
			if (value.Distance < 30f && Time.time - value.GroupInfo.EnemyLastSeenTimeSense < 20f)
			{
				num++;
			}
		}
		if (num > 1)
		{
			return true;
		}
		return false;
	}

	public AICoreActionEndStruct method_18()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemy null");
		}
		if (goalEnemy.Distance > 5f && Time.time - BotOwner_0.Memory.LastTimeHit < 1f)
		{
			Bool_4 = false;
			return new AICoreActionEndStruct("getHit");
		}
		if (Float_6 < Time.time)
		{
			Float_6 = Time.time + 0.5f;
			foreach (BotOwner follower in BotOwner_0.Boss.Followers)
			{
				if (follower.Memory.HaveEnemy && follower.Memory.GoalEnemy.Person.Id == goalEnemy.Person.Id)
				{
					follower.ShootData.BlockFor(0.6f);
				}
			}
		}
		if (Bool_4 && Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 2f)
		{
			Bool_4 = false;
			return new AICoreActionEndStruct("no vision gr");
		}
		return Gclass459_0.EndOneMeleeAttack();
	}
}
