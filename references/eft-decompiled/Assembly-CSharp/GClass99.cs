using System;
using EFT;
using UnityEngine;

public class GClass99 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public CustomNavigationPoint CustomNavigationPoint_0;

	[NonSerialized]
	public float Float_3;

	public GClass99(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		BotLogicDecision botLogicDecision = BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0);
		if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "spot");
		}
		if (botLogicDecision == BotLogicDecision.holdPosition)
		{
			BotOwner_0.GameEventsData.HalloweenGameEvent.TryRecalcPatrolParams();
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(botLogicDecision, "ptrs");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (CustomNavigationPoint_0 != null && !CustomNavigationPoint_0.IsSpotted)
		{
			return CustomNavigationPoint_0;
		}
		CustomNavigationPoint_0 = BotOwner_0.BotsGroup.CoverPointMaster.GetFreeClosePoint(BotOwner_0.GameEventsData.HalloweenGameEvent.FindPointCenterPosition, BotOwner_0.Covers, -1f);
		return CustomNavigationPoint_0;
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + GClass856.Random(1f, 3f);
			BotOwner_0.BotTalk.DropNextSayPeriod();
			BotOwner_0.BotTalk.Say(EPhraseTrigger.OnMutter, sayImmediately: true);
		}
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.IsInCover && Time.time - BotOwner_0.Memory.ComeToCoverTime > BotOwner_0.GameEventsData.HalloweenGameEvent.HoldPeriod)
		{
			if (BotOwner_0.Memory.CurCustomCoverPoint != null)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				CustomNavigationPoint_0 = null;
			}
			return new AICoreActionEndStruct("cvrStp");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("nht53");
		}
		if (!BotOwner_0.Mover.HasPathAndNoComplete && Time.time - BotOwner_0.Mover.LastEndPathTime > BotOwner_0.GameEventsData.HalloweenGameEvent.HoldPeriod)
		{
			return new AICoreActionEndStruct("dfk54");
		}
		return AICoreActionEndStruct_1;
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override string Name()
	{
		return "InfectedPatrol";
	}
}
