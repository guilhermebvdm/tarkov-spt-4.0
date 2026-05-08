using System;
using System.Runtime.CompilerServices;
using EFT;

public abstract class BaseLogicLayerAbstractClass : global::AICoreLayerClass<BotLogicDecision>
{
	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public AICoreActionEndStruct AICoreActionEndStruct = new AICoreActionEndStruct("Base logic");

	[NonSerialized]
	public AICoreActionEndStruct AICoreActionEndStruct_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public sealed override int Priority
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public static BotLogicDecision HoldOrCover(BotOwner owner)
	{
		if (owner.Memory.IsInCover)
		{
			return BotLogicDecision.holdPosition;
		}
		return BotLogicDecision.goToCoverPoint;
	}

	public static BotLogicDecision HoldOrCoverRun(BotOwner owner)
	{
		if (owner.Memory.IsInCover)
		{
			return BotLogicDecision.holdPosition;
		}
		return BotLogicDecision.runToCover;
	}

	public BaseLogicLayerAbstractClass(BotOwner bot, int priority)
	{
		Int_0 = priority;
		BotOwner_0 = bot;
	}

	public override void DecisionChanged(global::AICoreActionResultStruct<BotLogicDecision, GClass26>? prevDecision, global::AICoreActionResultStruct<BotLogicDecision, GClass26> nextDecision)
	{
		base.DecisionChanged(prevDecision, nextDecision);
		BotLogicDecision action = nextDecision.Action;
		if ((uint)(action - 30) > 1u && action != BotLogicDecision.debugStationary && BotOwner_0.WeaponManager.Stationary.Taken)
		{
			BotOwner_0.WeaponManager.Stationary.DropCurWeapon(withDelay: false);
		}
	}

	public override AICoreActionEndStruct ShallEndCurrentDecision(global::AICoreActionResultStruct<BotLogicDecision, GClass26> curDecision)
	{
		if (Bool_1)
		{
			Bool_1 = false;
			return new AICoreActionEndStruct("nextFrameDr");
		}
		switch (curDecision.Action)
		{
		default:
			return new AICoreActionEndStruct("any way");
		case BotLogicDecision.doorOpen:
			return EndDoorOpenRequest();
		case BotLogicDecision.warnPlayer:
			return EndWarnPlayer();
		case BotLogicDecision.shootToSmoke:
			return EndShootToSmoke();
		case BotLogicDecision.holdPosition:
			return EndHoldPosition();
		case BotLogicDecision.runToCover:
		{
			AICoreActionEndStruct result = EndRunToCover();
			if (result.Value)
			{
				BotOwner_0.BotRun.EndMove();
			}
			return result;
		}
		case BotLogicDecision.attackMoving:
			return EndAttackMoving();
		case BotLogicDecision.attackMovingWithSuppress:
			return EndAttackMovingWithSuppress();
		case BotLogicDecision.shootFromPlace:
			return EndShootFromPlace();
		case BotLogicDecision.goToEnemy:
			return EndGoToEnemy();
		case BotLogicDecision.heal:
			return EndHeal();
		case BotLogicDecision.goToCoverPoint:
			return EndGoToCoverPoint();
		case BotLogicDecision.repairMalfunction:
			return EndRepairMalfunction();
		case BotLogicDecision.goToCoverPointTactical:
			return EndGoToCoverPointTactical();
		case BotLogicDecision.goToPointTactical:
			return EndGoSomePointTactical();
		case BotLogicDecision.lay:
			return EndLayNode();
		case BotLogicDecision.search:
			return EndSearch();
		case BotLogicDecision.shootFromCover:
			return EndShootFromCover();
		case BotLogicDecision.dogFight:
			return EndDogFight();
		case BotLogicDecision.turnAwayLight:
			return EndTurnAway();
		case BotLogicDecision.standBy:
			return EndStandBy();
		case BotLogicDecision.suppressFire:
			return EndSuppressFire();
		case BotLogicDecision.suppressGrenade:
			return EndSuppressGrenade();
		case BotLogicDecision.throwGrenadeFromPlace:
			return EndThrowGrenadeFromPlace();
		case BotLogicDecision.runAndThrowGrenadeFromPlace:
			return EndRunAndThrowGrenadeFromPlace();
		case BotLogicDecision.runToEnemy:
			return EndRunToEnemy();
		case BotLogicDecision.runToEnemyZigZag:
			return EndRunToEnemyZigZag();
		case BotLogicDecision.goToEnemyZigZag:
			return EndGoToEnemyZigZag();
		case BotLogicDecision.goToPoint:
			return EndGoToPoint();
		case BotLogicDecision.panicSitting:
			return EndPanicSitting();
		case BotLogicDecision.runToStationary:
			return EndRunToStationary();
		case BotLogicDecision.shootFromStationary:
			return EndShootFromStationary();
		case BotLogicDecision.suppressStationary:
			return EndSuppressStationary();
		case BotLogicDecision.healStimulators:
			return EndStimulators();
		case BotLogicDecision.axeTarget:
			return EndAxeTarget();
		case BotLogicDecision.healAnotherTarget:
			return EndHealAnotherTarget();
		case BotLogicDecision.oneMeleeAttack:
			return EndOneMeleeAttack();
		case BotLogicDecision.grenadeSuicide:
			return EndGrenadeSuicide();
		case BotLogicDecision.leaveMap:
			return EndLeaveMap();
		case BotLogicDecision.deadBody:
			return EndDeadBody();
		case BotLogicDecision.friendlyTilt:
			return EndFriendlyTilt();
		case BotLogicDecision.eatDrink:
			return EndEatDrink();
		case BotLogicDecision.watchSecondWeapon:
			return EndWatchSecondWeapon();
		case BotLogicDecision.peaceHardAim:
			return EndPeaceHardAim();
		case BotLogicDecision.peaceLook:
			return EndPeaceLook();
		case BotLogicDecision.gesture:
			return EndGestus();
		case BotLogicDecision.peaceful:
			return EndPeaceful();
		case BotLogicDecision.botDropItem:
			return EndDropItem();
		case BotLogicDecision.botTakeItem:
			return EndTakeItem();
		case BotLogicDecision.followerPatrol:
			return EndFollowerPatrolItem();
		case BotLogicDecision.alternativePatrol:
			return EndAlternativePatrol();
		case BotLogicDecision.simplePatrol:
			return EndSimplePatrol();
		case BotLogicDecision.runAwayGrenade:
			return EndRunAwayGrenade();
		case BotLogicDecision.runAwayArtillery:
			return EndRunAwayArtillery();
		case BotLogicDecision.runAwayBTR:
			return EndRunAwayBTR();
		case BotLogicDecision.followMeRequest:
			return EndFollowMeRequest();
		case BotLogicDecision.runToCoverZigZag:
			return EndRunToCoverZigZag();
		case BotLogicDecision.flashed:
			return EndFlashed();
		case BotLogicDecision.teleportToCover:
			return method_1();
		case BotLogicDecision.crawl:
			return EndCrawlNode();
		case BotLogicDecision.moveStealthy:
			return EndMoveStealthy();
		case BotLogicDecision.plantMine:
			return EndPlantMine();
		case BotLogicDecision.attackMovingFlank:
			return EndAttackMovingFlank();
		case BotLogicDecision.deactivateMine:
			return EndDeactivateMine();
		case BotLogicDecision.goToLootPointNode:
			return EndGoToLootPointNode();
		case BotLogicDecision.goToExfiltrationPointNode:
			return EndGoToExfiltrationPointNode();
		case BotLogicDecision.khorovodChristmasEvent:
			return EndKhorovodChristmasEvent();
		case BotLogicDecision.doGiftChristmasEvent:
			return EndDoGiftChristmasEvent();
		case BotLogicDecision.summon:
			return method_2();
		case BotLogicDecision.followPlayer:
			return EndPlayerFollow();
		case BotLogicDecision.debugMove:
			return EndDebugMove();
		case BotLogicDecision.debugRun:
			return EndDebugRun();
		case BotLogicDecision.debugDrop:
			return EndDebugDrop();
		case BotLogicDecision.debugTake:
			return EndDebugTake();
		case BotLogicDecision.debugGestus:
			return EndDebugGestus();
		case BotLogicDecision.debugMeleeChange:
			return EndDebugMeleeChange();
		case BotLogicDecision.debugGrenade:
			return EndDebugGrenade();
		case BotLogicDecision.debugLay:
			return EndDebugLay();
		case BotLogicDecision.debugMelee:
			return EndDebugMelee();
		case BotLogicDecision.debugShuttle:
			return EndDebugShuttle();
		case BotLogicDecision.debugTacticalShuttle:
			return EndDebugTaticalShuttle();
		case BotLogicDecision.debugRotateHead:
			return EndDebugRotateHead();
		case BotLogicDecision.debugRotate:
			return EndDebugRotateLay();
		case BotLogicDecision.debugRotateLay:
			return EndDebugRotate();
		case BotLogicDecision.debugRunToPoint:
			return EndDebugRunToPoint();
		case BotLogicDecision.debugShoot:
			return EndDebugShoot();
		case BotLogicDecision.debugWeaponChange:
			return EndDebugWeaponChange();
		case BotLogicDecision.debugStationary:
		case BotLogicDecision.debugStationaryInstantTake:
			return EndDebugStationary();
		case BotLogicDecision.debugRunToCloseCover:
			return EndDebugRunToCloseCover();
		case BotLogicDecision.debugZigZagRunNode:
			return EndDebugZigZagRunNode();
		case BotLogicDecision.debugMeds:
			return EndMedsNode();
		case BotLogicDecision.debugtacticalMove:
			return EndDebugTacticalMove();
		case BotLogicDecision.debugToggleLauncher:
			return method_0();
		}
	}

	public AICoreActionEndStruct method_0()
	{
		return AICoreActionEndStruct;
	}

	public AICoreActionEndStruct method_1()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndAttackMovingWithSuppress()
	{
		return EndAttackMoving();
	}

	public virtual AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndStandBy()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoToLootPointNode()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoToExfiltrationPointNode()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndKhorovodChristmasEvent()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndSummon()
	{
		return AICoreActionEndStruct;
	}

	public AICoreActionEndStruct method_2()
	{
		AICoreActionEndStruct result = EndSummon();
		if (result.Value)
		{
			BotOwner_0.GetPlayer.SetAnimatorLayerWeight(17, 0);
		}
		return result;
	}

	public virtual AICoreActionEndStruct EndDoGiftChristmasEvent()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndTurnAway()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDogFight()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunToStationary()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndPanicSitting()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunToEnemyZigZag()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndSuppressFire()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoToCoverPoint()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndPlayerFollow()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRepairMalfunction()
	{
		if (BotOwner_0.WeaponManager.Malfunctions.HaveMalfunction())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndPlantMine()
	{
		if (BotOwner_0.MinesData.Planting)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndMoveStealthy()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndAttackMovingFlank()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndCrawlNode()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndLayNode()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndShootFromCover()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndHeal()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndSimplePatrol()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndShootFromPlace()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunToCover()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndSuppressGrenade()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunAndThrowGrenadeFromPlace()
	{
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndThrowGrenadeFromPlace()
	{
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndOneMeleeAttack()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndHealAnotherTarget()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndAxeTarget()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndStimulators()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndShootFromStationary()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndSuppressStationary()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGrenadeSuicide()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoSomePointTactical()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoToCoverPointTactical()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndLeaveMap()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDeadBody()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndFriendlyTilt()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndEatDrink()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndWatchSecondWeapon()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunToCoverZigZag()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunAwayGrenade()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunAwayBTR()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDeactivateMine()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndFollowerPatrolItem()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndAlternativePatrol()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndTakeItem()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDropItem()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGestus()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndPeaceful()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndPeaceHardAim()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndPeaceLook()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoToEnemyZigZag()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndGoToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndHoldPosition()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndFlashed()
	{
		if (BotOwner_0.FlashGrenade.IsFlashed)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndShootToSmoke()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDoorOpenRequest()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndWarnPlayer()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndFollowMeRequest()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugMove()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugRun()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugDrop()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugTake()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugGestus()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugLay()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugGrenade()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugMelee()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugTaticalShuttle()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugShuttle()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugMeleeChange()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugRotateHead()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugRotate()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugRotateLay()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugRunToPoint()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugRunToCloseCover()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugZigZagRunNode()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndMedsNode()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugShoot()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugStationary()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugWeaponChange()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndDebugTacticalMove()
	{
		return AICoreActionEndStruct;
	}

	public virtual AICoreActionEndStruct EndRunAwayArtillery()
	{
		return AICoreActionEndStruct;
	}
}
