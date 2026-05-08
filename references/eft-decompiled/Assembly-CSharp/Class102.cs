using System;
using EFT;
using UnityEngine;

public class Class102(BotOwner bot, int priority) : BaseLogicLayerSimpleAbstractClass(bot, priority)
{
	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4 = 20f;

	public override string Name()
	{
		return "InfectedWait";
	}

	public override void ManualUpdate()
	{
		base.ManualUpdate();
		if (Float_3 < Time.time)
		{
			Float_3 = Time.time + GClass856.Random(1f, 3f);
			BotOwner_0.BotTalk.Say(EPhraseTrigger.OnEnemyConversation);
		}
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (Bool_4)
		{
			Vector3 vector = BotOwner_0.Memory.GoalEnemy.CurrPosition + GClass856.RandomHorizontal(5f, 9f);
			CustomNavigationPoint customNavigationPoint = BotOwner_0.Covers.FindClosestPoint(vector, 1f, vector, noRestrictions: true);
			if (customNavigationPoint != null)
			{
				Bool_4 = !Bool_4;
				BotOwner_0.GoToSomePointData.SetPoint(customNavigationPoint.BasePosition);
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "36nma");
			}
		}
		Bool_4 = !Bool_4;
		BotOwner_0.GetPlayer.TriggerZombieLost();
		HoldFor(GClass856.Random(3f, 8f));
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "y46y3");
	}

	public override bool ShallUseNow()
	{
		return false;
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_12(10f))
		{
			return new AICoreActionEndStruct("hit");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			if (Time.time - BotOwner_0.Memory.ComeToCoverTime > 10f)
			{
				BotOwner_0.Memory.Spotted(byHit: false, null, 5f);
				return new AICoreActionEndStruct("ct1");
			}
			return AICoreActionEndStruct_1;
		}
		if (Bool_2)
		{
			if (Float_2 < Time.time)
			{
				Bool_2 = false;
				return AICoreActionEndStruct;
			}
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override void Dispose()
	{
		base.Dispose();
	}
}
