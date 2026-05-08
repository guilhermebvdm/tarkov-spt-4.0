using System;
using EFT;
using UnityEngine;

public class GClass126 : GClass125
{
	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public const float Float_5 = 200f;

	public GClass126(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		global::AICoreActionResultStruct<BotLogicDecision, GClass26>? aICoreActionResultStruct = InFightLogic();
		if (aICoreActionResultStruct.HasValue)
		{
			method_14();
			return aICoreActionResultStruct.Value;
		}
		if (method_15())
		{
			method_14();
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ht09");
		}
		if (BotOwner_0.Memory.GoalEnemy.IsVisible)
		{
			method_14();
			return method_16("uk87");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			HoldFor(10f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hf10");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "atk53");
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
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
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("CLOSEANDVIS");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_16(string info)
	{
		if (BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, info);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, info);
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!visible");
		}
		if (goalEnemy.VisibleType == EEnemyPartVisibleType.Visible)
		{
			return AICoreActionEndStruct_1;
		}
		if (Time.time - goalEnemy.LastChangeVisionTypeTime < 1f)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("!enemy.V");
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - BotOwner_0.Memory.LastTimeHit < 40f)
		{
			return true;
		}
		if (Float_4 > Time.time && Time.time - goalEnemy.PersonalLastSeenTime < 40f)
		{
			Float_4 = Time.time + 10f;
			return true;
		}
		if (goalEnemy.Distance < 200f && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			Float_4 = Time.time + 10f;
			return true;
		}
		if (BotOwner_0.MinesData.HaveAtCache())
		{
			return false;
		}
		return Float_4 > Time.time;
	}

	public override string Name()
	{
		return "PrtFight";
	}
}
