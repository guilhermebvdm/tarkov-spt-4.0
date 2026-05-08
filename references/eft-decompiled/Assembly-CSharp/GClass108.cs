using System;
using EFT;
using UnityEngine;

public abstract class GClass108 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public float Float_3 = 5f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public float Float_5;

	public GClass108(BotOwner bot, int priority)
		: base(bot, priority)
	{
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

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_13()
	{
		if (Time.time < Float_4 + Float_3)
		{
			EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
			if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "noCoverSht");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "noCoverHld");
		}
		if (Float_5 > Time.time)
		{
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "blU");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "horr5");
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

	public bool method_14()
	{
		if (BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint != null)
		{
			return BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.IsSpotted;
		}
		return true;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_15(string info)
	{
		if (BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, info);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, info);
	}
}
