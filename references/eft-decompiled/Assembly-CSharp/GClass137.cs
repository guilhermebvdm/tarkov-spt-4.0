using System;
using EFT;
using UnityEngine;

public class GClass137 : GClass135
{
	[NonSerialized]
	public float Float_6 = 90f;

	[NonSerialized]
	public float Float_7 = 5f;

	[NonSerialized]
	public float Float_8 = 10f;

	public GClass137(BotOwner bot, int priority, bool tryFinGreenFirst, CoverLevel minGreenPointCoverLevel = CoverLevel.Lay, bool usePeriodForceReched = false)
		: base(bot, priority, tryFinGreenFirst, minGreenPointCoverLevel, usePeriodForceReched)
	{
		method_17();
	}

	public void method_17()
	{
		Float_6 = GClass856.Random(Float_7, Float_8);
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (Time.time - BotOwner_0.Memory.ComeToCoverTime > Float_6)
			{
				BotOwner_0.Memory.Spotted(byHit: false);
				return AICoreActionEndStruct;
			}
			return AICoreActionEndStruct_1;
		}
		if (Time.time - BotOwner_0.Mover.LastEndPathTime > Float_6)
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override CustomNavigationPoint GetPoint()
	{
		Vector3 pos = Vector3_0 + GClass856.RandomHorizontal(20f, 50f);
		return BotOwner_0.BotsGroup.CoverPointMaster.GetFreeClosePoint(pos, BotOwner_0.Covers, -1f);
	}

	public override string Name()
	{
		return "InfStayAtPos";
	}
}
