using System;
using EFT;
using UnityEngine;

public abstract class GClass125 : BaseLogicLayerSimpleAbstractClass
{
	public const float LOW_KARMA = 0.5f;

	[NonSerialized]
	public float Float_3;

	public GClass125(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public void method_13()
	{
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + GClass856.Random(15f, 60f);
			BotOwner_0.BotTalk.Say(EPhraseTrigger.Provocation, sayImmediately: true);
		}
	}

	public void method_14()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && goalEnemy.Person.Profile.KarmaValue < 0.5f)
		{
			BotOwner_0.BotTalk.TrySay(EPhraseTrigger.LowKarmaAttack, withGroupDelay: true);
		}
	}

	public bool method_15()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - BotOwner_0.Memory.UnderFireTime < 5f && !BotOwner_0.Memory.IsInCover && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return true;
		}
		return false;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (method_15())
		{
			return new AICoreActionEndStruct("underFire");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}
}
