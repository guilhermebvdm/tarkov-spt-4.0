using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass48 : BaseLogicLayerSimpleAbstractClass
{
	[NonSerialized]
	public float Float_3;

	public float LastUseAvoidLayer => Float_3;

	public GClass48(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (BotOwner_0.FlashGrenade.IsFlashed)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.flashed, "Flashed");
		}
		if (BotOwner_0.BotTurnAwayLight.IsActive)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.turnAwayLight, "BotTurnAway");
		}
		if (BotOwner_0.ArtilleryDangerPlace.ShallRunAway())
		{
			if (GClass856.IsTrue100(BotOwner_0.Settings.FileSettings.Lay.SHALL_LAY_PROBABILTY_WHEN_ARTILLERY))
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.lay, "ArtLay");
			}
			if (method_13())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ArtInCover");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runAwayArtillery, "ArtRun");
		}
		if (method_16())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "InCover");
		}
		if (BotOwner_0.BewareGrenade.ShallRunAway())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runAwayGrenade, "BewareGrena");
		}
		if (BotOwner_0.BewareBTR.ShallRunAway())
		{
			if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "BwrBTRShot");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "BewareBTR");
		}
		if (method_15())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "PeaceSmoke");
		}
		if (BotOwner_0.BewarePlantedMine.CanDeactivate())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.deactivateMine, "deactM");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "HoldOrCover");
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		CoverSearchData arg = new CoverSearchData(BotOwner_0.Position, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 2500f, 0f, CoverSearchType.distToBot, null, null, null, ECheckSHootHide.nothing, new CoverSearchDefenceDataClass(0f), PointsArrayType.allWithBush, useSelfFindPoint: false);
		CustomNavigationPoint customNavigationPoint = p(arg);
		if (customNavigationPoint == null || (customNavigationPoint.Position - BotOwner_0.Position).sqrMagnitude < LocalBotSettingsProviderClass.Core.DELTA_GRENADE_BEWARE_DIST_SQRT)
		{
			if (customNavigationPoint != null && BotOwner_0.BewareGrenade.GrenadeDangerPoint != null && customNavigationPoint.IsGoodForGrenade(BotOwner_0.BewareGrenade.GrenadeDangerPoint, BotOwner_0))
			{
				return customNavigationPoint;
			}
			customNavigationPoint = method_14();
		}
		return customNavigationPoint;
	}

	public override string Name()
	{
		return "AvoidDanger";
	}

	public override bool ShallUseNow()
	{
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return false;
		}
		int num;
		if (!BotOwner_0.ArtilleryDangerPlace.ShallRunAway() && !BotOwner_0.BewareGrenade.ShallRunAway() && !BotOwner_0.BewareBTR.ShallRunAway() && !BotOwner_0.BotTurnAwayLight.IsActive && !BotOwner_0.FlashGrenade.IsFlashed && !method_15())
		{
			num = (BotOwner_0.BewarePlantedMine.CanDeactivate() ? 1 : 0);
			if (num == 0)
			{
				goto IL_009c;
			}
		}
		else
		{
			num = 1;
		}
		Float_3 = Time.time;
		goto IL_009c;
		IL_009c:
		return (byte)num != 0;
	}

	public override AICoreActionEndStruct EndRunAwayArtillery()
	{
		if (method_13())
		{
			return AICoreActionEndStruct;
		}
		if (BotOwner_0.ArtilleryDangerPlace.ShallRunAway())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public bool method_13()
	{
		if (BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.IsInCover && BotOwner_0.BewareArtillery.IsCoverSafe(BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint))
		{
			return BotOwner_0.Memory.GoalEnemy.CanShoot;
		}
		return false;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		if (!BotOwner_0.ArtilleryDangerPlace.ShallRunAway() && !BotOwner_0.BewareGrenade.ShallRunAway())
		{
			if (BotOwner_0.Memory.IsInCover && BotOwner_0.SmokeGrenade.IsInSmoke)
			{
				BotOwner_0.Memory.CurCustomCoverPoint.Spotted(10f);
			}
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndTurnAway()
	{
		if (BotOwner_0.BotTurnAwayLight.IsActive)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("lightisoff");
	}

	public override AICoreActionEndStruct EndRunAwayGrenade()
	{
		if (method_16())
		{
			return AICoreActionEndStruct;
		}
		if (BotOwner_0.BewareGrenade.ShallRunAway())
		{
			return AICoreActionEndStruct_1;
		}
		if (BotOwner_0.ArtilleryDangerPlace.ShallRunAway())
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("noDanager");
	}

	public CustomNavigationPoint method_14()
	{
		return BotOwner_0.Covers.GetClosestPoint(BotOwner_0.Position, delegate(GroupPoint point)
		{
			if (point.IsSpotted)
			{
				return false;
			}
			if (!point.IsFreeById(BotOwner_0.Id))
			{
				return false;
			}
			if (BotOwner_0.BewareGrenade.GrenadeDangerPoint == null)
			{
				return false;
			}
			return !((point.Position - BotOwner_0.BewareGrenade.GrenadeDangerPoint.DangerPoint).sqrMagnitude < LocalBotSettingsProviderClass.Core.DELTA_GRENADE_BEWARE_DIST_SQRT);
		});
	}

	public bool method_15()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return BotOwner_0.SmokeGrenade.IsInSmoke;
		}
		return false;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndStandBy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndKhorovodChristmasEvent()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDoGiftChristmasEvent()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToStationary()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndPanicSitting()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToEnemyZigZag()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRepairMalfunction()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		if (BotOwner_0.ArtilleryDangerPlace.IsActive)
		{
			return AICoreActionEndStruct_1;
		}
		return base.EndLayNode();
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (method_16())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (BotOwner_0.BewareGrenade.ShallRunAway() && method_18() && BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return AICoreActionEndStruct_1;
		}
		if (method_16())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		if (!method_16() && !method_17())
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSuppressGrenade()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunAndThrowGrenadeFromPlace()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHealAnotherTarget()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAxeTarget()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndStimulators()
	{
		if (method_16())
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromStationary()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSuppressStationary()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGrenadeSuicide()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndLeaveMap()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDeadBody()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFriendlyTilt()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndEatDrink()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndWatchSecondWeapon()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToCoverZigZag()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFollowerPatrolItem()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndTakeItem()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDropItem()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGestus()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndPeaceful()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndPeaceHardAim()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndPeaceLook()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (BotOwner_0.BewareGrenade.ShallRunAway() && method_18() && BotOwner_0.Memory.HaveEnemy && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return AICoreActionEndStruct;
		}
		if (BotOwner_0.BewareGrenade.ShallRunAway() && method_16())
		{
			return AICoreActionEndStruct_1;
		}
		if (BotOwner_0.ArtilleryDangerPlace.IsActive)
		{
			method_13();
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct;
	}

	public bool method_16()
	{
		if (BotOwner_0.Memory.IsInCover && BotOwner_0.Memory.CurCustomCoverPoint.CoverLevel == CoverLevel.Stay && BotOwner_0.Memory.CurCustomCoverPoint.IsGoodForGrenade(BotOwner_0.BewareGrenade.GrenadeDangerPoint, BotOwner_0))
		{
			return !BotOwner_0.BewareBTR.ShallRunAway();
		}
		return false;
	}

	public bool method_17()
	{
		if (BotOwner_0.Memory.IsInCover && BotOwner_0.BewareArtillery.IsCoverSafe(BotOwner_0.Memory.CurCustomCoverPoint))
		{
			return !BotOwner_0.BewareBTR.ShallRunAway();
		}
		return false;
	}

	public bool method_18()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return BotOwner_0.BewareBTR.ShallRunAway();
		}
		return false;
	}

	public override AICoreActionEndStruct EndFlashed()
	{
		if (BotOwner_0.FlashGrenade.IsFlashed)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootToSmoke()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDoorOpenRequest()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndWarnPlayer()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFollowMeRequest()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugMove()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRun()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugDrop()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugTake()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugGestus()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugLay()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugGrenade()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugMelee()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugMeleeChange()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRotateHead()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRotate()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRunToPoint()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugRunToCloseCover()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugShoot()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugStationary()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDebugWeaponChange()
	{
		return AICoreActionEndStruct;
	}

	[CompilerGenerated]
	public bool method_19(GroupPoint point)
	{
		if (point.IsSpotted)
		{
			return false;
		}
		if (!point.IsFreeById(BotOwner_0.Id))
		{
			return false;
		}
		if (BotOwner_0.BewareGrenade.GrenadeDangerPoint == null)
		{
			return false;
		}
		if ((point.Position - BotOwner_0.BewareGrenade.GrenadeDangerPoint.DangerPoint).sqrMagnitude < LocalBotSettingsProviderClass.Core.DELTA_GRENADE_BEWARE_DIST_SQRT)
		{
			return false;
		}
		return true;
	}
}
