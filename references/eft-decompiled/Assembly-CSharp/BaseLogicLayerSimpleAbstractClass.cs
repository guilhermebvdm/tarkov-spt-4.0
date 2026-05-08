using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseLogicLayerSimpleAbstractClass : BaseLogicLayerAbstractClass
{
	[NonSerialized]
	public const float Float_0 = 5f;

	[NonSerialized]
	public const float Float_1 = 6f;

	[NonSerialized]
	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> Gstruct8_0;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public BotLogicDecision? Nullable_0;

	[NonSerialized]
	public bool Bool_3;

	public static BotLogicDecision TryMoveToEnemy(BotOwner bot, BotLogicDecision runDecision = BotLogicDecision.runToEnemy)
	{
		Vector3 currPosition = bot.Memory.GoalEnemy.CurrPosition;
		NavMeshPath navMeshPath = new NavMeshPath();
		if (NavMesh.CalculatePath(bot.Position, currPosition, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			return runDecision;
		}
		float maxDistance = 10f;
		if (NavMesh.SamplePosition(currPosition, out var hit, maxDistance, -1))
		{
			navMeshPath = new NavMeshPath();
			if (NavMesh.CalculatePath(bot.Position, hit.position, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				return runDecision;
			}
		}
		if (Time.time > bot.Memory.nextTryMoveToEnemyLogTime)
		{
			bot.Memory.nextTryMoveToEnemyLogTime = Time.time + 3f;
		}
		return BaseLogicLayerAbstractClass.HoldOrCover(bot);
	}

	public static bool CheckMedsToStop(BotOwner bot)
	{
		if (bot.Memory.HaveEnemy)
		{
			bot.EnemyLookData.DoCheck();
			EnemyInfo goalEnemy = bot.Memory.GoalEnemy;
			if (goalEnemy.Distance < 10f)
			{
				return true;
			}
			if (goalEnemy.Distance < 30f && (bot.EnemyLookData.IsEnemyLookAtMeForPeriod(2f) || goalEnemy.IsVisible))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPointInsideDangerZone(BotOwner bot, Vector3 point)
	{
		BotZoneDangerAreas zoneDangerAreas = bot.BotsGroup.BotZone.ZoneDangerAreas;
		if (zoneDangerAreas.ActiveAreas.Count == 0)
		{
			return false;
		}
		foreach (AIDangerArea activeArea in zoneDangerAreas.ActiveAreas)
		{
			if (activeArea.IsPointInside(point))
			{
				return true;
			}
		}
		return false;
	}

	public BaseLogicLayerSimpleAbstractClass(BotOwner bot, int priority)
		: base(bot, priority)
	{
	}

	public bool method_3()
	{
		return BotOwner_0.DogFight.DogFightState != BotDogFightStatus.none;
	}

	public virtual CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		return method_11(data, p, checkCurrent);
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26>? InFightLogic()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_4())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromPlace, "ShootImmediately");
		}
		if (method_6(out var cause))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, cause);
		}
		if (BotOwner_0.NearDoorData.RecentlyClosedDoorCheckTime + 0.3f < Time.time && BotOwner_0.BotsGroup.EnemyLastSeenTimeReal + 7f >= Time.time && method_10(goalEnemy))
		{
			BotOwner_0.Memory.Spotted(byHit: false);
		}
		return null;
	}

	public void CalcActionNextFrame(BotLogicDecision? nextLogic = null)
	{
		Nullable_0 = nextLogic;
		Bool_1 = true;
	}

	public bool method_4()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool flag = ((goalEnemy != null && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Shoot.SHOOT_IMMEDIATELY_DIST) || BotOwner_0.BotsGroup.AnyBodyShootImmediately) && goalEnemy.CanShoot && Time.time - goalEnemy.AddTime < 5f;
		bool isActive = BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive;
		BotOwner_0.BotsGroup.AnyBodyShootImmediately = flag || isActive;
		return BotOwner_0.BotsGroup.AnyBodyShootImmediately;
	}

	public bool method_5(EnemyInfo info)
	{
		if (info == null)
		{
			return false;
		}
		Vector3 end = info.EnemyLastPositionReal + Vector3.up * 1.6f;
		if (!Physics.Linecast(BotOwner_0.WeaponRoot.position, end, out var _, LayerMaskClass.HighPolyWithTerrainMask))
		{
			return true;
		}
		return false;
	}

	public bool method_6(out string cause)
	{
		if (!BotOwner_0.Memory.IsInCover)
		{
			cause = "IsInCover";
			return false;
		}
		if (!BotOwner_0.LookSensor.EnoughDistToShoot(out var _))
		{
			cause = "EnoughDistToShoot";
			return false;
		}
		if (!BotOwner_0.Memory.CurCustomCoverPoint.CanShootToTargetCast(BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.DELTA_SEEN_FROM_COVE_LAST_POS))
		{
			cause = "CanShootToTargetCast";
			return false;
		}
		if (BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
		{
			cause = "EndSho";
			return false;
		}
		cause = "allFine";
		return true;
	}

	public bool method_7()
	{
		if (Bool_2)
		{
			if (Float_2 < Time.time)
			{
				Bool_2 = false;
				return true;
			}
			return false;
		}
		return false;
	}

	public virtual BotLogicDecision HoldFor(float sec)
	{
		if (sec > 0f)
		{
			Float_2 = Time.time + sec;
			Bool_2 = true;
		}
		return BotLogicDecision.holdPosition;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_8(float holdperiod = 0f, string pref = null)
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (BotOwner_0.Memory.GoalEnemy != null && BotOwner_0.Memory.GoalEnemy.CanShoot)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, pref + "ASH");
			}
			if (holdperiod > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(holdperiod), pref + "ASH");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, pref + "ASH");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, pref + "ASH");
	}

	public bool method_9()
	{
		return (float)BotOwner_0.WeaponManager.Reload.BulletCount / (float)BotOwner_0.WeaponManager.Reload.MaxBulletCount < BotOwner_0.Settings.FileSettings.Boss.PERCENT_BULLET_TO_RELOAD;
	}

	public bool method_10(EnemyInfo enemy)
	{
		NavMeshDoorLink nearestDoor = BotOwner_0.NearDoorData.GetNearestDoor();
		if (nearestDoor == null)
		{
			return false;
		}
		Vector3 position = BotOwner_0.Transform.position;
		Vector3 currPosition = enemy.CurrPosition;
		GClass365 gClass = new GClass365(position, currPosition);
		Vector3 vector = nearestDoor.SegmentOpen.b - nearestDoor.SegmentOpen.a;
		Vector3 a = nearestDoor.SegmentOpen.a - vector * 0.1f;
		Vector3 b = nearestDoor.SegmentOpen.b + vector * 0.1f;
		return GClass369.GetCrossPoint(gClass.a, gClass.b, a, b).HasValue;
	}

	public CustomNavigationPoint method_11(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		if (checkCurrent && BotOwner_0.BotsGroup.CoverPointMaster.IsCurrentPointGood(data.SearchType, data, out var point))
		{
			return point;
		}
		return p(data);
	}

	public override AICoreActionEndStruct EndRunAwayGrenade()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSimplePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToPoint()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			if (BotOwner_0.GoToSomePointData.IsCome())
			{
				return new AICoreActionEndStruct("Come");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("Enemy");
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (!BotOwner_0.BotLay.IsLay && BotOwner_0.BotLay.CanProne && BotOwner_0.BotLay.CanLayByPeriod())
		{
			return new AICoreActionEndStruct("!.BotL");
		}
		if (goalEnemy != null && !goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!CanShoot");
		}
		if (BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
		{
			return new AICoreActionEndStruct("StationaryW");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndAlternativePatrol()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFollowerPatrolItem()
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

	public override AICoreActionEndStruct EndPeaceHardAim()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndPeaceLook()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndEatDrink()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndFriendlyTilt()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDeadBody()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndLeaveMap()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.DogFight.ShallStartCauseHavePlace())
		{
			return new AICoreActionEndStruct("StartH");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (goalEnemy == null)
		{
			return new AICoreActionEndStruct("enemynull");
		}
		if (!goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!enemy.CanS");
		}
		if (goalEnemy.Distance < 1f)
		{
			return new AICoreActionEndStruct("enemy.Dista");
		}
		if (BotOwner_0.WeaponManager.Reload.Reloading)
		{
			return new AICoreActionEndStruct(".Reloa");
		}
		if (BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
		{
			return new AICoreActionEndStruct("EndSho");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGrenadeSuicide()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndAxeTarget()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndHealAnotherTarget()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndStimulators()
	{
		if (!BotOwner_0.Medecine.Stimulators.Using)
		{
			return new AICoreActionEndStruct("end stim");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndSuppressStationary()
	{
		if (!BotOwner_0.WeaponManager.Stationary.IsClose())
		{
			return new AICoreActionEndStruct("not close");
		}
		StationaryWeaponLink curLink = BotOwner_0.WeaponManager.Stationary.CurLink;
		if (curLink == null)
		{
			return new AICoreActionEndStruct("linkisnull");
		}
		if (!curLink.HaveAmmo())
		{
			return new AICoreActionEndStruct("no ammo");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToStationary()
	{
		if (BotOwner_0.WeaponManager.Stationary.CurLink == null)
		{
			return new AICoreActionEndStruct("no link");
		}
		if (BotOwner_0.WeaponManager.Stationary.IsClose())
		{
			return AICoreActionEndStruct;
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndPanicSitting()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndRunToEnemyZigZag()
	{
		return EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new AICoreActionEndStruct("enemy null");
		}
		Bool_3 = BotOwner_0.WeaponManager.Melee.ShallEndRun;
		if (!Bool_3 && Time.time - BotOwner_0.WeaponManager.Melee.LastTimeEnemyHit < 1f)
		{
			return new AICoreActionEndStruct("deltaLastHi");
		}
		if (Bool_3)
		{
			return new AICoreActionEndStruct("lastCanRunE");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("CanSV");
	}

	public override AICoreActionEndStruct EndSuppressFire()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot) && !BotOwner_0.SuppressShoot.Complete)
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("CanShoot");
	}

	public override AICoreActionEndStruct EndStandBy()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndTurnAway()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndDogFight()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if ((goalEnemy == null || goalEnemy.Distance > BotOwner_0.Settings.FileSettings.Mind.DOG_FIGHT_OUT) && !BotOwner_0.WeaponManager.Reload.Reloading && !BotOwner_0.Memory.BotCurrentCoverInfo.UseDogFight(BotOwner_0.Settings.FileSettings.Cover.DOG_FIGHT_AFTER_LEAVE))
		{
			return new AICoreActionEndStruct("DogFightEnd");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (method_6(out var cause))
		{
			return default(AICoreActionEndStruct);
		}
		return new AICoreActionEndStruct(cause);
	}

	public override AICoreActionEndStruct EndSearch()
	{
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndSuppressGrenade()
	{
		if (Time.time - BotOwner_0.Brain.Agent.LastPeriod > 6f)
		{
			return AICoreActionEndStruct;
		}
		if (BotOwner_0.WeaponManager.Grenades.ThrowindNow)
		{
			return AICoreActionEndStruct_1;
		}
		if (!BotOwner_0.SuppressGrenade.Complete)
		{
			return AICoreActionEndStruct_1;
		}
		return AICoreActionEndStruct;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("VisibleCanS");
		}
		return AICoreActionEndStruct_1;
	}

	public virtual bool CanSearchEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		if (method_12(10f))
		{
			return false;
		}
		if (!goalEnemy.IsVisible && !goalEnemy.CanShoot && goalEnemy.CanISearch)
		{
			if (BotOwner_0.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack) && BotOwner_0.Memory.LastEnemyVisionOld(LocalBotSettingsProviderClass.Core.COVER_SECONDS_AFTER_LOSE_VISION))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool method_12(float period)
	{
		return Time.time - BotOwner_0.Memory.LastTimeHit < period;
	}

	public override AICoreActionEndStruct EndHeal()
	{
		if (!BotOwner_0.Medecine.Using)
		{
			return new AICoreActionEndStruct("1");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!method_3() && goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("DogFightCan");
	}

	public override AICoreActionEndStruct EndAttackMoving()
	{
		if (method_3())
		{
			return new AICoreActionEndStruct("dog");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("inCvr");
		}
		if (BotOwner_0.WeaponManager.Stationary.ShallEndShootFromCurrent())
		{
			return new AICoreActionEndStruct("stationary");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("atCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_7())
		{
			return new AICoreActionEndStruct("EndHol");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		if (goalEnemy == null)
		{
			if (CanSearchEnemy())
			{
				return new AICoreActionEndStruct("CanSearchEn");
			}
		}
		else
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("CanShoot");
			}
			if (goalEnemy.IsVisible && goalEnemy.Distance < BotOwner_0.Settings.FileSettings.Cover.END_HOLD_IF_ENEMY_CLOSE_AND_VISIBLE)
			{
				return new AICoreActionEndStruct("CLOSEANDVIS");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCoverZigZag()
	{
		return EndRunToCover();
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("InCover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			return new AICoreActionEndStruct("CanSprintPl");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			return new AICoreActionEndStruct("IsSpotted");
		}
		return AICoreActionEndStruct_1;
	}
}
