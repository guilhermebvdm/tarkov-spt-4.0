using System;
using EFT;
using UnityEngine;

public class GClass149(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[NonSerialized]
	public GInterface7 Ginterface7_0;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = 60f;

	public void SetBossData(GInterface7 bossData)
	{
		Ginterface7_0 = bossData;
	}

	public override void OnActivate()
	{
		if (BotOwner_0.Boss.IamBoss)
		{
			SetBossData(BotOwner_0.Boss.BossLogic as GInterface7);
		}
		base.OnActivate();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Medecine.FirstAid.Have2Do)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "FirstAid");
			}
			if (BotOwner_0.Medecine.Stimulators.Using)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "FirstAid");
			}
			if (Float_3 < Time.time)
			{
				Float_3 = Time.time + GClass856.GreateRandom(Float_4);
				if (BotOwner_0.Medecine.Stimulators.HaveSmt && Ginterface7_0 != null && Ginterface7_0.CanStartUseStimulator())
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.healStimulators, "nextPOsible");
				}
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "HoldOrCover");
	}

	public override bool ShallUseNow()
	{
		if (Ginterface7_0 != null)
		{
			return BotOwner_0.Memory.HaveGoal;
		}
		return false;
	}

	public override string Name()
	{
		return "SanitarGoal";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHoldTime");
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
				return new AICoreActionEndStruct("ENEMYCLOSE");
			}
		}
		return AICoreActionEndStruct_1;
	}
}
