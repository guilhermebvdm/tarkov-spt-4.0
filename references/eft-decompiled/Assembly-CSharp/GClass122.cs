using System;
using EFT;
using UnityEngine;

public abstract class GClass122 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public GClass48 Gclass48_0;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_3 = 20f;

	[NonSerialized]
	public float Float_4 = 400f;

	[NonSerialized]
	public float Float_5 = 50f;

	[NonSerialized]
	public float Float_6 = -9999f;

	[NonSerialized]
	public bool Bool_5;

	public GClass122(BotOwner bot, int priority, GClass48 avoidDangerLayer)
		: base(bot, priority)
	{
		Gclass48_0 = avoidDangerLayer;
	}

	public override AICoreActionEndStruct EndPlantMine()
	{
		BotOwner_0.MinesData.ManualUpdate();
		if (BotOwner_0.MinesData.Planting)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_13(string info, CustomNavigationPoint target)
	{
		Bool_5 = true;
		if (method_14())
		{
			Float_6 = Time.time;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, info);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.moveStealthy, info);
	}

	public override AICoreActionEndStruct EndGoToCoverPointTactical()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoSomePointTactical()
	{
		if (BotOwner_0.GoToSomePointData.IsCome())
		{
			return new AICoreActionEndStruct("Come");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_14()
	{
		CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
		if (curCustomCoverPoint != null && Time.time - BotOwner_0.MinesData.LastPlantTime > Float_5)
		{
			if (Time.time - Float_6 < Float_3)
			{
				return false;
			}
			if ((curCustomCoverPoint.Position - BotOwner_0.Position).sqrMagnitude > Float_4)
			{
				return true;
			}
		}
		return false;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public bool method_15()
	{
		return Time.time - Gclass48_0.LastUseAvoidLayer > 20f;
	}
}
