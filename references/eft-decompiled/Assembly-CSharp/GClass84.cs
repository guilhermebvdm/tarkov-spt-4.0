using System;
using EFT;

public class GClass84 : BaseLogicLayerSimpleAbstractClass
{
	public GClass84(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		switch (BotOwner_0.BotRequestController.CurRequest.BotRequestType)
		{
		case BotRequestType.suppressionFire:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.suppressFire, "suppressFir");
		case BotRequestType.followMe:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.followMeRequest, "flwMRF");
		case BotRequestType.attackClose:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "attackClose");
		case BotRequestType.hold:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "holdPositio");
		case BotRequestType.throwGrenade:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.throwGrenadeFromPlace, "throwGrenadeRequest");
		case BotRequestType.throwGrenadeFromPlace:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runAndThrowGrenadeFromPlace, "runAndThrowGrenadeFromPlace");
		case BotRequestType.hide:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "hide");
		default:
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "Error");
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
		}
	}

	public override string Name()
	{
		if (BotOwner_0.BotRequestController.CurRequest != null)
		{
			return "FiReq:" + BotOwner_0.BotRequestController.CurRequest.BotRequestType;
		}
		return "FightReqNull";
	}

	public override bool ShallUseNow()
	{
		return method_13();
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (BotOwner_0.BotRequestController.CurRequest != null && BotOwner_0.BotRequestController.CurRequest.FindPoint(data, p, checkCurrent, out var customNavigationPoint))
		{
			return customNavigationPoint;
		}
		return base.FindPoint(data, p, checkCurrent);
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
			return new AICoreActionEndStruct("followMe");
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

	public override AICoreActionEndStruct EndDogFight()
	{
		return AICoreActionEndStruct;
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
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
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

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.BotRequestController.CurRequest == null)
		{
			return AICoreActionEndStruct;
		}
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.followMe)
		{
			return new AICoreActionEndStruct("followMe2");
		}
		return BotOwner_0.BotRequestController.CurRequest.EndHoldPosition();
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		BotRequest curRequest2 = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest2 != null && curRequest2.BotRequestType == BotRequestType.followMe)
		{
			return new AICoreActionEndStruct("followMe3");
		}
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		BotRequest curRequest = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest != null && curRequest.BotRequestType == BotRequestType.wait)
		{
			return new AICoreActionEndStruct("wait");
		}
		BotRequest curRequest2 = BotOwner_0.BotRequestController.CurRequest;
		if (curRequest2 != null && curRequest2.BotRequestType == BotRequestType.attackClose)
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

	public override AICoreActionEndStruct EndThrowGrenadeFromPlace()
	{
		if (BotOwner_0.BotRequestController.HaveActivatedRequests() && BotOwner_0.BotRequestController.CurRequest is GClass599)
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

	public bool method_13()
	{
		if (BotOwner_0.BotRequestController.CurRequest == null)
		{
			return false;
		}
		if (BotOwner_0.BotRequestController.CurRequest.RequestMode == EBotRequestMode.Peace)
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
