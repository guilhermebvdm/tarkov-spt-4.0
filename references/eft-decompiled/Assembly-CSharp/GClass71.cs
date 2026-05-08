using System;
using EFT;

public class GClass71 : BaseLogicLayerSimpleAbstractClass
{
	public DebugBotDesition Desition;

	public GClass71(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		return Desition switch
		{
			DebugBotDesition.go => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugMove, "dGo"), 
			DebugBotDesition.run => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugRun, "dRun"), 
			DebugBotDesition.shoot => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugShoot, "dSPlace"), 
			DebugBotDesition.grenade => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugGrenade, "dGrenade"), 
			DebugBotDesition.gestus => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugGestus, "dGestus"), 
			DebugBotDesition.lay => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugLay, "dLay"), 
			DebugBotDesition.shootFromPlace => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "dShoot"), 
			DebugBotDesition.flashed => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.flashed, "dFlashed"), 
			DebugBotDesition.weaponChange => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugWeaponChange, "dChange"), 
			DebugBotDesition.rotate => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugRotate, "dRotate"), 
			DebugBotDesition.rotateLay => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugRotateLay, "dLayRotate"), 
			DebugBotDesition.rotateHead => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugRotateHead, "dHead"), 
			DebugBotDesition.shuttleMove => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugShuttle, "Shuttle"), 
			DebugBotDesition.runToTarget => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugRunToPoint, "ToPoint"), 
			DebugBotDesition.runToCloseCover => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugRunToCloseCover, "ToCloseCover"), 
			DebugBotDesition.useStationary => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugStationary, "Stationary"), 
			DebugBotDesition.botTakeItem => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugTake, "Take"), 
			DebugBotDesition.botDropItem => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugDrop, "Drop"), 
			DebugBotDesition.botGetMelee => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugMeleeChange, "MeleeChange"), 
			DebugBotDesition.oneMeleeAtack => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugMelee, "Melee"), 
			DebugBotDesition.debugZigZagRunNode => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugZigZagRunNode, "debugZigZagRunNode"), 
			DebugBotDesition.useStationaryInsta => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugStationaryInstantTake, "Stationary Insta"), 
			DebugBotDesition.debugMeds => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugMeds, "debugMeds"), 
			DebugBotDesition.debugTacticalMove => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugtacticalMove, "debugTMove"), 
			DebugBotDesition.shuttleTacticalMove => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugTacticalShuttle, "ShuttleT"), 
			DebugBotDesition.toggleLauncher => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.debugToggleLauncher, "debugToggleLauncher"), 
			_ => new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "error"), 
		};
	}

	public override string Name()
	{
		return "Debug";
	}

	public override bool ShallUseNow()
	{
		return true;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		return base.FindPoint(data, p, checkCurrent);
	}

	public override AICoreActionEndStruct EndDebugMove()
	{
		if (Desition == DebugBotDesition.go)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRun()
	{
		if (Desition == DebugBotDesition.run)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugDrop()
	{
		if (Desition == DebugBotDesition.botDropItem)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugTake()
	{
		if (Desition == DebugBotDesition.botTakeItem)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugGestus()
	{
		if (Desition == DebugBotDesition.gestus)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugLay()
	{
		if (Desition == DebugBotDesition.lay)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugGrenade()
	{
		if (Desition == DebugBotDesition.grenade)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugMelee()
	{
		if (Desition == DebugBotDesition.oneMeleeAtack)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugShuttle()
	{
		if (Desition == DebugBotDesition.shuttleMove)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugTaticalShuttle()
	{
		if (Desition == DebugBotDesition.debugTacticalMove)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugMeleeChange()
	{
		if (Desition == DebugBotDesition.botGetMelee)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRotateHead()
	{
		if (Desition == DebugBotDesition.rotateHead)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRotate()
	{
		if (Desition == DebugBotDesition.rotate)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRotateLay()
	{
		if (Desition == DebugBotDesition.rotateLay)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRunToPoint()
	{
		if (Desition == DebugBotDesition.runToTarget)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRunToCloseCover()
	{
		if (Desition == DebugBotDesition.runToCloseCover)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugZigZagRunNode()
	{
		if (Desition == DebugBotDesition.debugZigZagRunNode)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndMedsNode()
	{
		if (Desition == DebugBotDesition.debugMeds)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugShoot()
	{
		if (Desition == DebugBotDesition.shoot)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugStationary()
	{
		if (Desition != DebugBotDesition.useStationary && Desition != DebugBotDesition.useStationaryInsta)
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndDebugWeaponChange()
	{
		if (Desition == DebugBotDesition.weaponChange)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugTacticalMove()
	{
		if (Desition == DebugBotDesition.debugTacticalMove)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}
}
