using System;
using System.Runtime.CompilerServices;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public abstract class GClass103 : GClass102
{
	[NonSerialized]
	public const float Float_3 = 6f;

	[NonSerialized]
	public const float Float_4 = 10000f;

	[NonSerialized]
	public const float Float_5 = 9f;

	[NonSerialized]
	public const float Float_6 = 10f;

	[NonSerialized]
	public const float Float_7 = 2f;

	[NonSerialized]
	public const float Float_8 = 2f;

	public bool _haveGoodCover;

	public bool IAmReady;

	public bool IsCurPosGood;

	[NonSerialized]
	public float Float_9;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public float Float_10;

	[NonSerialized]
	public float Float_11;

	[NonSerialized]
	public float Float_12;

	public GClass103([NotNull] BotOwner owner, int priority)
		: base(owner, priority)
	{
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		CustomNavigationPoint customNavigationPoint = base.FindPoint(data, p, checkCurrent);
		_haveGoodCover = customNavigationPoint.CanIShootToEnemy;
		BotOwner_0.Memory.BotCurrentCoverInfo.SetCover(customNavigationPoint);
		return customNavigationPoint;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		if (ShallAttack())
		{
			if (BotOwner_0.Memory.IsInCover && ((BotOwner_0.LookSensor.EnoughDistToShoot(out var _) && BotOwner_0.Memory.CurCustomCoverPoint.CanShootToTargetCast(BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.DELTA_SEEN_FROM_COVE_LAST_POS)) || method_16()))
			{
				if (!_haveGoodCover)
				{
					_haveGoodCover = true;
					base.GInterface6_0?.SetHaveGoodCover(BotOwner_0, _haveGoodCover);
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.shootFromCover, "shootFromCo");
			}
			if (!BotOwner_0.Memory.IsInCover && BotOwner_0.CanSprintPlayer && BotOwner_0.Settings.FileSettings.Core.CanRun && BotOwner_0.BotRun.ShallRunAnyway())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "RunAnyway");
			}
		}
		if (method_3())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.dogFight, "StartDogFig");
		}
		if (Nullable_0.HasValue)
		{
			BotLogicDecision value = Nullable_0.Value;
			Nullable_0 = null;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(value, "nextLogic");
		}
		if (ShallAttack())
		{
			return method_21();
		}
		method_18();
		if (BotOwner_0.Memory.IsInCover && IsCurPosGood)
		{
			IAmReady = true;
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "CurPosGood");
		}
		IAmReady = false;
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "CurPosGood");
	}

	public override void OnActivate()
	{
		BotOwner_0.GetPlayer.ActiveHealthController.DoPainKiller();
	}

	public bool IsCoverGoodForRun([CanBeNull] CustomNavigationPoint targetCustomNavigPoint)
	{
		if (targetCustomNavigPoint == null)
		{
			return false;
		}
		if (_haveGoodCover)
		{
			return false;
		}
		return true;
	}

	public void SetCorePosition(Vector3 corePos)
	{
		Vector3_1 = corePos;
	}

	public override AICoreActionEndStruct EndShootFromCover()
	{
		if (BotOwner_0.Memory.IsInCover && ((BotOwner_0.LookSensor.EnoughDistToShoot(out var _) && BotOwner_0.Memory.CurCustomCoverPoint.CanShootToTargetCast(BotOwner_0, BotOwner_0.Settings.FileSettings.Cover.DELTA_SEEN_FROM_COVE_LAST_POS)) || method_16()))
		{
			return default(AICoreActionEndStruct);
		}
		return new AICoreActionEndStruct("cause1");
	}

	public abstract Vector3 GetTargetToLook();

	public override bool CanSearchEnemy()
	{
		return false;
	}

	public override AICoreActionEndStruct EndShootFromPlace()
	{
		AICoreActionEndStruct result = base.EndShootFromPlace();
		if (result.Value)
		{
			return result;
		}
		if (Float_12 < Time.time)
		{
			Float_12 = Time.time + 1f;
			if (BotOwner_0.BotLay.CanShootPos(BotOwner_0.Memory.GoalEnemy, withCheckShoot: true, withFriendlyFire: false))
			{
				return new AICoreActionEndStruct("nextLayTime");
			}
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.GoalEnemy == null)
		{
			return new AICoreActionEndStruct("Enemynull");
		}
		if (!_haveGoodCover && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("!haveGoodCo");
		}
		if (Time.time - BotOwner_0.Memory.GoalEnemy.TimeLastSeenReal > BotOwner_0.Settings.FileSettings.Boss.KOJANIY_TAKE_CARE_ABOULT_ENEMY_DELTA && BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("KOJANIYDELT");
		}
		method_22();
		return base.EndRunToCover();
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && (!goalEnemy.IsVisible || !goalEnemy.CanShoot))
		{
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("!InCover");
			}
			if (Bool_2)
			{
				if (Float_2 < Time.time)
				{
					Bool_2 = false;
					return new AICoreActionEndStruct("endHoldEnab");
				}
				return AICoreActionEndStruct_1;
			}
			if (method_14())
			{
				return new AICoreActionEndStruct("SomeeSeeEne");
			}
			return AICoreActionEndStruct_1;
		}
		return new AICoreActionEndStruct("VisibleCanS");
	}

	public override AICoreActionEndStruct EndLayNode()
	{
		AICoreActionEndStruct result = base.EndLayNode();
		if (!BotOwner_0.BotLay.CheckDistGood())
		{
			return new AICoreActionEndStruct("CheckDistGo");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && !goalEnemy.IsVisible)
		{
			return new AICoreActionEndStruct("!IsVisible");
		}
		return result;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		AICoreActionEndStruct result = base.EndGoToCoverPoint();
		if (result.Value)
		{
			return result;
		}
		if (ShallAttack() && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new AICoreActionEndStruct("Attack");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndRunToEnemy()
	{
		if (!method_24())
		{
			return new AICoreActionEndStruct("EnemyCloseL");
		}
		if (!method_14())
		{
			return new AICoreActionEndStruct("!SomeeSeeEn");
		}
		if (Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal > 9f)
		{
			return new AICoreActionEndStruct("RUNTOENEMYD");
		}
		return base.EndRunToEnemy();
	}

	public override AICoreActionEndStruct EndGoToEnemy()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (base.GInterface6_0.IsNearLoot(goalEnemy))
		{
			if (goalEnemy.IsVisible && goalEnemy.CanShoot)
			{
				return new AICoreActionEndStruct("VCS2");
			}
			return AICoreActionEndStruct_1;
		}
		if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal > 9f)
		{
			return new AICoreActionEndStruct("RUNTOENEMYD");
		}
		if (!base.GInterface6_0.EnoughtHaveGoodCovers)
		{
			return new AICoreActionEndStruct("!EnoughtHav");
		}
		return base.EndGoToEnemy();
	}

	public abstract bool IsEverybodyRun();

	public override AICoreActionEndStruct EndAttackMoving()
	{
		if (Time.time - BotOwner_0.ShootData.LastTriggerPressd > 9f)
		{
			return new AICoreActionEndStruct("MAXATTACKMO");
		}
		if (method_3())
		{
			return new AICoreActionEndStruct("StartD");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public bool method_16()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null && Time.time - goalEnemy.PersonalLastShootTime < 5f)
		{
			return true;
		}
		return false;
	}

	public ShootPointClass method_17()
	{
		Vector3 point = ((BotOwner_0.Memory.GoalEnemy == null) ? GetTargetToLook() : (Nullable_1.HasValue ? ((Time.time - Float_9 < 3f) ? Nullable_1.Value : (BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true)?.Point ?? Nullable_1.Value)) : (BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true)?.Point ?? BotOwner_0.Position)));
		return new ShootPointClass(point);
	}

	public void method_18()
	{
		if (Float_11 < Time.time && !method_16())
		{
			Float_11 = Time.time + 1.2f;
			BotOwner_0.BotAttackManager.TryPointGetting(Vector3_1, CoverShootType.shoot, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, CoverSearchType.distToToCenter, method_17(), delegate(CustomNavigationPoint point)
			{
				IsCurPosGood = BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint == point;
				Float_11 = -1f;
			}, null, checkCurrent: false);
		}
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_19()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (GClass369.CanShootToTarget(BotOwner_0.CurrentEnemyTargetPosition(sensPosition: true), BotOwner_0.Memory.CurCustomCoverPoint, BotOwner_0.LookSensor.Mask))
			{
				return method_23();
			}
			if (Time.time - BotOwner_0.Memory.ComeToCoverTime >= 6f)
			{
				return method_25("1");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(6f), "HOLDINCOVER");
		}
		if (method_14())
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				if (Time.time - BotOwner_0.Memory.ComeToCoverTime >= 6f)
				{
					return method_25("2");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(6f), "SomeeSeeEne");
			}
			return method_25("3");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.holdPosition, "InCover");
		}
		if (Time.time - BotOwner_0.Memory.GoalEnemy.GroupInfo.EnemyLastSeenTimeReal < 2f)
		{
			return method_25("4");
		}
		if (!(Time.time - BotOwner_0.Memory.GoalEnemy.TimeLastSeen > BotOwner_0.Settings.FileSettings.Boss.KOJANIY_WANNA_GO_TO_CLOSEST_COVER) && (method_14() || method_24()))
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(10f), "HoldFor10fL");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "run Last");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "KOJANIYWANN");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_20()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			if (method_14())
			{
				if (Time.time - BotOwner_0.Memory.ComeToCoverTime >= 6f)
				{
					return method_25("5");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(6f), "ManyEnemies");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(5f), "ManyEnemies");
		}
		if (!_haveGoodCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToCoverPoint, "!haveGoodCo");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "haveGoodCov");
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_21()
	{
		if (base.GInterface6_0.TooManyEnemies())
		{
			return method_20();
		}
		return method_19();
	}

	public void method_22()
	{
		if (Float_10 < Time.time)
		{
			Float_10 = Time.time + 3f;
			Vector3? closeFriendCover = BotOwner_0.Covers.ClosestFriendCoverPoint();
			CoverSearchData data = new CoverSearchData(Vector3_1, BotOwner_0.CoverSearchInfo, CoverShootType.shoot, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, 0f, CoverSearchType.distToToCenter, method_17(), closeFriendCover, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(BotOwner_0.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			BotOwner_0.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: true);
		}
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_23()
	{
		if (GClass369.CanShoot(BotOwner_0, BotOwner_0.Memory.GoalEnemy) && BotOwner_0.Memory.GoalEnemy.CanShoot)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_26(), "canSHootChe");
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (Time.time - goalEnemy.GroupInfo.EnemyLastSeenTimeReal < 10f)
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				if (Time.time - BotOwner_0.Memory.ComeToCoverTime > 20f && method_14())
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "SomeeSeeEne");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(2f), "SomeeSeeEne");
			}
			if (method_24() && method_14())
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToEnemy, "EnemyCloseL");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "EnemyCloseL");
		}
		if (base.GInterface6_0.IsNearLoot(goalEnemy))
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.goToEnemy, "nearLoot");
		}
		if (BotOwner_0.Memory.IsInCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(10f), "IsInCover");
		}
		if (_haveGoodCover)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "haveGoodCov");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(HoldFor(10f), "RunIfCantSh");
	}

	public bool method_24()
	{
		if (BotOwner_0.Memory.GoalEnemy != null && (base.GInterface6_0.LootPosition - BotOwner_0.Memory.GoalEnemy.EnemyLastPositionReal).sqrMagnitude < 10000f)
		{
			return true;
		}
		return false;
	}

	public global::AICoreActionResultStruct<BotLogicDecision, GClass26> method_25(string info)
	{
		method_22();
		if (!_haveGoodCover)
		{
			return method_23();
		}
		if (IsEverybodyRun())
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.attackMoving, "isEveryBody");
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "isEveryBody");
	}

	public BotLogicDecision method_26()
	{
		if (BotOwner_0.BotLay.CanShootPos(BotOwner_0.Memory.GoalEnemy, withCheckShoot: true, withFriendlyFire: false))
		{
			return BotLogicDecision.lay;
		}
		return BotLogicDecision.shootFromPlace;
	}

	[CompilerGenerated]
	public void method_27(CustomNavigationPoint point)
	{
		IsCurPosGood = BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint == point;
		Float_11 = -1f;
	}
}
