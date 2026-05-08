using System;
using EFT;

public class GClass100 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public Func<bool> Func_0;

	public GClass100(BotOwner bot, int priority, Func<bool> shallUse)
		: base(bot, priority)
	{
		Func_0 = shallUse;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.Medecine.FirstAid.Have2Do && BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.heal, "heal");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "HorC");
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.Memory.HaveGoal)
		{
			return Func_0();
		}
		return false;
	}

	public override string Name()
	{
		return "HoldOrCoverT";
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("NotInCover");
	}

	public override AICoreActionEndStruct EndMoveStealthy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("InCover");
	}
}
