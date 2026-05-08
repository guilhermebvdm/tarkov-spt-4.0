using EFT;

public class GClass83 : BaseLogicLayerSimpleAbstractClass
{
	public GClass83(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.WarnData.WarnPlayerRequest != null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.warnPlayer, "warnPlayer");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "Error");
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSuppressGrenade()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunAndThrowGrenadeFromPlace()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDoorOpenRequest()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndWarnPlayer()
	{
		if (BotOwner_0.WarnData.WarnPlayerRequest != null)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFollowMeRequest()
	{
		return AICoreActionEndStruct;
	}

	public override string Name()
	{
		return "ExURequest";
	}

	public bool method_13()
	{
		if (BotOwner_0.BotRequestController.CurRequest == null)
		{
			return false;
		}
		if (BotOwner_0.BotRequestController.CurRequest.CanProceed())
		{
			return true;
		}
		if (BotOwner_0.BotRequestController.CurRequest.EndIfCantExecute)
		{
			BotOwner_0.BotRequestController.CurRequest.Dispose();
		}
		return false;
	}

	public override bool ShallUseNow()
	{
		if (method_13())
		{
			return !BotOwner_0.Memory.HaveEnemy;
		}
		return false;
	}
}
