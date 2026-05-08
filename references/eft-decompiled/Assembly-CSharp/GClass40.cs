using System;
using EFT;
using UnityEngine;

public class GClass40 : GClass38
{
	[NonSerialized]
	public float Float_4;

	public bool Boolean_0
	{
		get
		{
			if (BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return BotOwner_0.Memory.GoalEnemy.IsVisible;
			}
			return false;
		}
	}

	public GClass40(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		Gclass25_0.Update();
		if (Bool_4)
		{
			return AICoreActionEndStruct_1;
		}
		return base.EndHoldPosition();
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		Gclass25_0.Update();
		if (Bool_4)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "toofar");
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "StartD");
		}
		if (Boolean_0)
		{
			if (BotOwner_0.BotLay.CanShootPos(BotOwner_0.Memory.GoalEnemy, withCheckShoot: true, withFriendlyFire: false))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "inCov");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "inPlc");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "inCov");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "MarksmanEnemy";
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		return base.EndDogFight();
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("StartDF");
		}
		if (!Boolean_0)
		{
			return new AICoreActionEndStruct("noVision");
		}
		if (method_12(5f))
		{
			return new AICoreActionEndStruct("getHit");
		}
		if (Float_4 < Time.time)
		{
			Float_4 = Time.time + 3f;
			if (BotOwner_0.BotLay.CanShootPos(BotOwner_0.Memory.GoalEnemy, withCheckShoot: true, withFriendlyFire: false))
			{
				return new AICoreActionEndStruct("canLay");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		if (BotOwner_0.Memory.GoalEnemy.Distance < 5f)
		{
			return new AICoreActionEndStruct("tooClose");
		}
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("StartDF");
		}
		if (method_12(5f) && !BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("getHitLay");
		}
		return AICoreActionEndStruct_1;
	}
}
