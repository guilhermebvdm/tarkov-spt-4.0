using System;
using EFT;
using UnityEngine;

public class GClass98 : BaseLogicLayerSimpleAbstractClass
{
	public enum EZombieMode
	{
		Slow = 1,
		Fast,
		Shooting
	}

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public const float Float_4 = 4f;

	[NonSerialized]
	public EZombieMode EzombieMode_0 = EZombieMode.Fast;

	[NonSerialized]
	public float Float_5 = 1f;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public const float Float_10 = 10f;

	public const float MIN_TALK_DELAY = 1f;

	public const float MAX_TALK_DELAY = 3f;

	[NonSerialized]
	public float Float_11;

	public EZombieMode CurrentZombieMode => EzombieMode_0;

	public GClass98(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (Float_9 > 0f)
		{
			float float_ = Float_9;
			Float_9 = 0f;
			HoldFor(float_);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "wf");
		}
		Float_6 = GClass856.Random(3f, 5f);
		Float_5 = GClass856.Random(2f, 4f);
		return EzombieMode_0 switch
		{
			EZombieMode.Slow => method_13(), 
			EZombieMode.Fast => method_14(), 
			EZombieMode.Shooting => method_15(), 
			_ => method_13(), 
		};
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_13()
	{
		method_16();
		if (BotOwner_0.Memory.GoalEnemy.Distance < 4f)
		{
			if (BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
			{
				BotOwner_0.WeaponManager.Selector.ChangeToMelee();
			}
			Float_7 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "atk1");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemyZigZag, "gzz1");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_14()
	{
		method_16();
		if (BotOwner_0.Memory.GoalEnemy.Distance < 4f)
		{
			if (BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
			{
				BotOwner_0.WeaponManager.Selector.ChangeToMelee();
			}
			Float_7 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.oneMeleeAttack, "atk2");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemyZigZag, "rzz2");
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + GClass856.Random(1f, 3f);
			BotOwner_0.BotTalk.DropNextSayPeriod();
			BotOwner_0.BotTalk.Say(EPhraseTrigger.OnEnemyConversation, sayImmediately: true);
		}
	}

	public override AICoreActionEndStruct EndGoToEnemyZigZag()
	{
		method_16();
		if (EzombieMode_0 == EZombieMode.Shooting)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
			{
				return AICoreActionEndStruct_1;
			}
			return new AICoreActionEndStruct("gh3k");
		}
		if (BotOwner_0.Memory.GoalEnemy.Distance < 2.8f)
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_15()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy.CanShoot)
		{
			method_16();
			if (BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
			{
				BotOwner_0.WeaponManager.Selector.TryChangeToMain();
			}
			if (goalEnemy.IsVisible && goalEnemy.Distance < 15f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "jh3p");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "sht3");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemyZigZag, "gzz3");
	}

	public void SetZombieMode(EZombieMode mode)
	{
		EzombieMode_0 = mode;
	}

	public void method_16()
	{
		if (BotOwner_0.Memory.LastEnemyTimeSeen > Time.time - 1f && Time.time > Float_8 + 10f)
		{
			BotOwner_0.GameEventsData.HalloweenGameEvent.ReportSeenEnemy(BotOwner_0.Memory.GoalEnemy.Person);
			Float_8 = Time.time;
		}
	}

	public override AICoreActionEndStruct EndRunToEnemyZigZag()
	{
		method_16();
		if (EzombieMode_0 == EZombieMode.Shooting)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
			{
				return AICoreActionEndStruct_1;
			}
			return new AICoreActionEndStruct("errzz");
		}
		if (BotOwner_0.Memory.GoalEnemy.Distance < 2.8f)
		{
			return AICoreActionEndStruct;
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
		method_16();
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("!VanS");
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		method_16();
		if (Float_6 > 0f && Time.time - Float_7 > Float_6)
		{
			if (Float_5 > 0f)
			{
				Float_9 = GClass856.Random(1f, Float_5);
			}
			return new AICoreActionEndStruct("rndTimer");
		}
		if (BotOwner_0.Memory.LastEnemyTimeSeen > Time.time - 1f && Time.time > Float_8 + 10f)
		{
			BotOwner_0.GameEventsData.HalloweenGameEvent.ReportSeenEnemy(BotOwner_0.Memory.GoalEnemy.Person);
			Float_8 = Time.time;
		}
		return base.EndOneMeleeAttack();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (!Bool_2)
		{
			return AICoreActionEndStruct;
		}
		if (Float_2 > Time.time)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		method_16();
		if (BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public void Call()
	{
		Float_11 = Time.time;
	}

	public bool method_17()
	{
		if (BotOwner_0.Memory.GoalEnemy != null)
		{
			if (!(BotOwner_0.Memory.LastEnemyTimeSeen > Time.time - 20f))
			{
				return Float_11 > Time.time - 20f;
			}
			return true;
		}
		return false;
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveEnemy)
		{
			return method_17();
		}
		return false;
	}

	public override string Name()
	{
		return EzombieMode_0 switch
		{
			EZombieMode.Slow => "ZombieSlow", 
			EZombieMode.Fast => "ZombieFast", 
			EZombieMode.Shooting => "ZombieShooting", 
			_ => "Zombie[?]", 
		};
	}
}
