using System;
using EFT;
using UnityEngine;

public class GClass111 : GClass109
{
	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public const float Float_12 = 1f;

	public GClass111(BotOwner bot, int priority)
		: base(bot, priority)
	{
		BotOwner_0.Memory.OnGoalEnemyChanged += method_19;
	}

	public void method_19(BotOwner obj)
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			Bool_5 = false;
		}
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time < Float_11 + 1f)
		{
			if (goalEnemy.CanShoot)
			{
				return method_15("shtFar1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "gteFar1");
		}
		if (!Bool_5 && !BotOwner_0.Memory.IsUnderFire)
		{
			if (method_20())
			{
				return method_16(goalEnemy, "sf3");
			}
			if (goalEnemy.CanShoot && goalEnemy.IsVisible)
			{
				if (BotOwner_0.Memory.IsInCover)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "sfc");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sfp");
			}
			return method_13();
		}
		if (goalEnemy.Distance < 15f)
		{
			return method_16(goalEnemy, "sf1");
		}
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "sf");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "s2f");
	}

	public bool method_20()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (method_17())
		{
			return false;
		}
		if (goalEnemy.Distance > 5f && Time.time - BotOwner_0.Memory.LastTimeHit < 1f)
		{
			return false;
		}
		if (goalEnemy.Distance < 15f && goalEnemy.CanShoot)
		{
			return goalEnemy.IsVisible;
		}
		return false;
	}

	public override void ManualUpdate()
	{
		if (Bool_5 && Float_10 < Time.time && !method_20())
		{
			Bool_5 = false;
		}
		base.ManualUpdate();
	}

	public void UpdateLastFarShotTime()
	{
		Float_11 = Time.time;
		if (BotOwner_0.Brain.BaseBrain.CurLayerInfo == this)
		{
			BotOwner_0.Brain.BaseBrain.CalcActionNextFrame();
		}
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return BotOwner_0.Boss.Followers.Count == 0;
		}
		return false;
	}

	public override string Name()
	{
		return "KlnSolo";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.IsUnderFire)
		{
			Float_10 = Time.time + 20f;
			Bool_5 = true;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			if (goalEnemy.Distance < 15f)
			{
				Float_10 = Time.time + 20f;
				Bool_5 = true;
				return new AICoreActionEndStruct("cls1");
			}
			return new AICoreActionEndStruct("vsb");
		}
		if (goalEnemy.Distance < 15f)
		{
			Float_10 = Time.time + 20f;
			Bool_5 = true;
			return new AICoreActionEndStruct("cls2");
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
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

	public override void Dispose()
	{
		BotOwner_0.Memory.OnGoalEnemyChanged -= method_19;
		base.Dispose();
	}
}
