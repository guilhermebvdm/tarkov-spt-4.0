using EFT;

public class GClass139 : BaseLogicLayerSimpleAbstractClass
{
	public GClass139(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		switch (BotOwner_0.BotRequestController.CurRequest.BotRequestType)
		{
		case BotRequestType.goToPoint:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToPoint, "goToPoint");
		default:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "Error");
		case BotRequestType.doorOpen:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.doorOpen, "doorOpen");
		case BotRequestType.getInCover:
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdPositio");
			}
			if (!BotOwner_0.CanSprintPlayer)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "go");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run");
		case BotRequestType.wait:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdPositio");
		case BotRequestType.followMe:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.followMeRequest, "flwMRP");
		}
	}

	public override string Name()
	{
		if (BotOwner_0.BotRequestController.CurRequest != null)
		{
			return "PcReq:" + BotOwner_0.BotRequestController.CurRequest.BotRequestType;
		}
		return "PeacecReqNull";
	}

	public override bool ShallUseNow()
	{
		return method_13();
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.suppressionFire)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSuppressGrenade()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.throwGrenade)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunAndThrowGrenadeFromPlace()
	{
		if (BotOwner_0.BotRequestController.HaveActivatedRequests() && BotOwner_0.BotRequestController.CurRequest is GClass599)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.BotRequestController.CurRequest == null)
		{
			return AICoreActionEndStruct;
		}
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest.BotRequestType != BotRequestType.hold && curRequest.BotRequestType != BotRequestType.hide)
		{
			return BotOwner_0.BotRequestController.CurRequest.EndHoldPosition();
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.goToPoint)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.followMe)
		{
			return new AICoreActionEndStruct("followMeR");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.attackClose)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDoorOpenRequest()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.doorOpen)
		{
			return AICoreActionEndStruct_1;
		}
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
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.followMe)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		BotRequest curRequest2 = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest2 != null && curRequest2.BotRequestType == BotRequestType.followMe)
		{
			return new AICoreActionEndStruct("followMe1");
		}
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndRunToStationary()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		return base.EndRunToStationary();
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		return base.EndSimplePatrol();
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		return base.EndAlternativePatrol();
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		return base.EndAttackMoving();
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		return AICoreActionEndStruct;
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
}
