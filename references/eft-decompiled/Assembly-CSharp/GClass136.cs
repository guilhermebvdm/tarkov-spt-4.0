using System;
using EFT;
using UnityEngine;

public class GClass136 : GClass135
{
	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public float Float_7;

	[NonSerialized]
	public float Float_8;

	[NonSerialized]
	public const float Float_9 = 5f;

	public GClass136(BotOwner bot, int priority, bool tryFinGreenFirst, CoverLevel minGreenPointCoverLevel = CoverLevel.Lay, bool usePeriodForceReched = false)
		: base(bot, priority, tryFinGreenFirst, minGreenPointCoverLevel, usePeriodForceReched)
	{
		Float_7 = GClass856.Random(0f, 10f);
	}

	public override bool ShallUseNow()
	{
		BotsPatrolGeneratorGameEvent botPatrolGeneratorGameEvent = BotOwner_0.BotsController.EventsController.BotPatrolGeneratorGameEvent;
		int num;
		if ((!BotOwner_0.Memory.HaveEnemy || Time.time - BotOwner_0.Memory.GoalEnemy.PersonalLastSeenTime >= 20f) && !method_12(20f) && botPatrolGeneratorGameEvent.IsActive)
		{
			EventObject.EState state = botPatrolGeneratorGameEvent.GeneratorData.State;
			num = ((state == EventObject.EState.Running || state == EventObject.EState.Broken) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		if (botPatrolGeneratorGameEvent.ConnectionGroupId > 0 && BotOwner_0.StartCorePoint.ConnectionGroupId != botPatrolGeneratorGameEvent.ConnectionGroupId)
		{
			return false;
		}
		if (flag && Time.time - Float_6 > 5f)
		{
			Float_6 = Time.time;
			if (BotOwner_0.StandBy.StandByType != BotStandByType.active)
			{
				BotOwner_0.StandBy.Activate();
			}
			if (BotOwner_0.Memory.CurCustomCoverPoint != null && botPatrolGeneratorGameEvent.GeneratorData != null && !smethod_0(BotOwner_0.Memory.CurCustomCoverPoint.Position, botPatrolGeneratorGameEvent.GeneratorData.transform.position, 30))
			{
				BotOwner_0.Memory.CurCustomCoverPoint.Spotted(5f);
			}
		}
		return method_17(flag);
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			method_16();
			if (BotOwner_0.Memory.CurCustomCoverPoint != CustomNavigationPoint_0 && CustomNavigationPoint_0 != null)
			{
				BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
				if (BotOwner_0.CanSprintPlayer)
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "!=");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "CntSprint");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "no better");
		}
		if (CustomNavigationPoint_0 != null)
		{
			BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(CustomNavigationPoint_0);
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
	}

	public override string Name()
	{
		return "GeneratorDef";
	}

	public override void OnActivate()
	{
		SetCorePosition(BotOwner_0.BotsController.EventsController.BotPatrolGeneratorGameEvent.GeneratorData.transform.position);
	}

	public bool method_17(bool shallUse)
	{
		if (shallUse)
		{
			if (Float_8 == 0f)
			{
				Float_8 = Time.time;
			}
			if (Time.time - Float_8 > Float_7)
			{
				return true;
			}
		}
		else
		{
			Float_6 = 0f;
		}
		return false;
	}

	public static bool smethod_0(Vector3 left, Vector3 right, int radius)
	{
		return (left - right).sqrMagnitude < (float)(radius * radius);
	}
}
