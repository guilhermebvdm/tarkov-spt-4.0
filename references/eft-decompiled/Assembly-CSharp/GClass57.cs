using System;
using EFT;
using UnityEngine;

public class GClass57 : GClass55
{
	[NonSerialized]
	public float Float_4;

	public bool Boolean_0
	{
		get
		{
			if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return BotOwner_0.Memory.GoalEnemy.IsVisible;
			}
			return false;
		}
	}

	public GClass57(BotOwner bot, int priority)
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
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "StartD");
		}
		if (Boolean_0)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "inPlc");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			HoldFor(5f);
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hld");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override bool ShallUseNow()
	{
		return BotOwner_0.Memory.HaveEnemy;
	}

	public override string Name()
	{
		return "BoarSnEn";
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
		if (method_7())
		{
			return new AICoreActionEndStruct("time");
		}
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
		if (CustomNavigationPoint_0 != null && CustomNavigationPoint_0.CanIShootToEnemy && BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint.Id != CustomNavigationPoint_0.Id && Time.time - BotOwner_0.Memory.ComeToCoverTime > 3f)
		{
			BotOwner_0.Memory.Spotted(byHit: false);
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
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
		if (Boolean_0)
		{
			return new AICoreActionEndStruct("CanShoot");
		}
		method_15();
		return AICoreActionEndStruct_1;
	}
}
