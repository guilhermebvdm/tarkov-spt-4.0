using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass153 : GClass150
{
	public const float SDIST_TREE_CLOSE = 900f;

	public const float SDIST_TREE_CLOSE_LONG = 2025f;

	public const float UNDER_FIRE_DELTA = 30f;

	[NonSerialized]
	public const float Float_3 = 15f;

	[NonSerialized]
	public const float Float_4 = 10f;

	[NonSerialized]
	public const float Float_5 = 50f;

	[NonSerialized]
	public const float Float_6 = 50f;

	public const float COVER_MIN_MAX_DIST_ENEMY_OUT = 25f;

	public const float COVER_MIN_MAX_DIST_ENEMY_IN = 10f;

	[NonSerialized]
	public const float Float_7 = 10f;

	[NonSerialized]
	public const float Float_8 = 4f;

	[NonSerialized]
	public const float Float_9 = 7f;

	[NonSerialized]
	public const float Float_10 = 8f;

	[NonSerialized]
	public float Float_11 = 8f;

	[NonSerialized]
	public const float Float_12 = 25f;

	[NonSerialized]
	public const float Float_13 = 3f;

	[NonSerialized]
	public const float Float_14 = 10f;

	[NonSerialized]
	public const float Float_15 = 2f;

	[NonSerialized]
	public const float Float_16 = 9f;

	[NonSerialized]
	public const float Float_17 = 40f;

	[NonSerialized]
	public const float Float_18 = 8f;

	[NonSerialized]
	public const float Float_19 = 15f;

	[NonSerialized]
	public float Float_20;

	[NonSerialized]
	public GClass448 Gclass448_0;

	[NonSerialized]
	public float Float_21;

	[NonSerialized]
	public float Float_22;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public GClass360 Gclass360_0;

	[NonSerialized]
	public float Float_23;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public float Float_24;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public float Float_25;

	[NonSerialized]
	public bool Bool_6;

	[NonSerialized]
	public float Float_26 = 50f;

	[NonSerialized]
	public string String_0 = "MeleeS_IN";

	[NonSerialized]
	public const float Float_27 = 9f;

	[NonSerialized]
	public const float Float_28 = 15f;

	[NonSerialized]
	public float Float_29 = 9f;

	[NonSerialized]
	public GClass25 Gclass25_0;

	public float Single_0 => BotOwner_0.Settings.FileSettings.Boss.SECTANT_INDOOR_DIST_NOT_TO_ATTACK;

	public GClass153(BotOwner bot, int priority, GClass360 warrior)
		: base(bot, priority)
	{
		Gclass360_0 = warrior;
		Gclass25_0 = new GClass25(3f, method_13);
	}

	public void method_13()
	{
		BotOwner_0.WeaponManager.Selector.ChangeToMelee();
	}

	public override bool ShallUseNow()
	{
		if (!BotOwner_0.Memory.HaveEnemy)
		{
			return false;
		}
		if (!BotOwner_0.WeaponManager.Melee.HaveMelee)
		{
			return false;
		}
		if (BotOwner_0.WeaponManager.IsMelee && BotOwner_0.WeaponManager.Selector.IsWeaponReady)
		{
			return true;
		}
		if (BotOwner_0.WeaponManager.Selector.CanChangeToMeleeWeapons)
		{
			Gclass25_0.Update();
			return true;
		}
		return false;
	}

	public override string Name()
	{
		return String_0;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		BotOwner_0.Memory.OnBulletNear += method_16;
		BotOwner_0.WeaponManager.Melee.OnEnemyHitted += method_14;
	}

	public void SetInside(bool isIndoorZone)
	{
		Bool_6 = isIndoorZone;
		String_0 = (Bool_6 ? "MeleeS_IN" : "MeleeS_OUT");
		Float_29 = (Bool_6 ? 15f : 9f);
		Float_11 = (Bool_6 ? 7f : 8f);
		if (Bool_6)
		{
			Float_26 = 0f;
		}
		else
		{
			Float_26 = 50f;
		}
	}

	public void SetRavangeMode()
	{
		Float_25 = Time.time + 20f;
	}

	public override global::AICoreActionResultStruct<BotLogicDecision, GClass26> GetDecision()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		bool bool_ = Bool_5;
		Bool_5 = false;
		if (goalEnemy == null)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCover(BotOwner_0), "enemynull");
		}
		if (Time.time - BotOwner_0.WeaponManager.Melee.LastTimeEnemyHit < 2f)
		{
			Gclass448_0.StartRunAway();
		}
		if (Float_25 > Time.time && goalEnemy.Distance < 40f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_21(), "REVANGEMELE");
		}
		if (Time.time - Float_20 < 10f && goalEnemy.Distance < 15f)
		{
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_21(), "BULLETDISTA");
		}
		if (bool_)
		{
			if (GClass856.IsTrue100(50f))
			{
				BotOwner_0.Memory.Spotted(byHit: false, null, 10f);
				if (GClass856.IsTrue100(Float_26))
				{
					return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCoverZigZag, "zigZag shallRun");
				}
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BotLogicDecision.runToCover, "minorCover");
			}
			return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_21(anyway: true), "endMinorPer");
		}
		if (goalEnemy.Distance < 15f)
		{
			if (Vector3.Dot(goalEnemy.Direction, goalEnemy.Person.LookDirection) > 0f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_21(), "dot");
			}
			if (goalEnemy.Distance < 8f)
			{
				return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(method_21(), "TRULYDISTAT");
			}
		}
		return new global::AICoreActionResultStruct<BotLogicDecision, GClass26>(BaseLogicLayerAbstractClass.HoldOrCoverRun(BotOwner_0), "HoldOrCover");
	}

	public void SetBoss(GClass448 sectantPriest)
	{
		Gclass448_0 = sectantPriest;
	}

	public void SetCorePosition(Vector3 corePoint)
	{
		Debug.DrawRay(corePoint, Vector3.up * 15f, Color.cyan, 3f);
		Debug.DrawLine(corePoint, BotOwner_0.Position + Vector3.up, Color.cyan, 3f);
		Vector3_0 = corePoint;
	}

	public override CustomNavigationPoint FindPoint(CoverSearchData data, Func<CoverSearchData, CustomNavigationPoint> p, bool checkCurrent)
	{
		GClass150.UpdatePlaceInfo(BotOwner_0, data);
		bool flag = Time.time < Float_24;
		float num = Time.time - BotOwner_0.Memory.UnderFireTime;
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		float num2 = ((!flag) ? 4f : (Bool_6 ? 10f : 25f));
		Vector3? closeFriendCover = BotOwner_0.Covers.ClosestFriendCoverPoint(Vector3_0);
		float minDistToFriend = (Bool_6 ? 9f : (data.Bot.Settings.Cover.CHECK_CLOSEST_FRIEND_DIST * data.Bot.Settings.Cover.CHECK_CLOSEST_FRIEND_DIST));
		CoverPointMaster.GStruct10? gStruct = null;
		if (closeFriendCover.HasValue)
		{
			gStruct = new CoverPointMaster.GStruct10
			{
				MinDistToFriend = minDistToFriend,
				CoverPos = closeFriendCover.Value
			};
		}
		int? placeId = GClass150.GetPlaceId(BotOwner_0);
		if (num > 30f && !Bool_6)
		{
			float sDist;
			CustomNavigationPoint customNavigationPoint = ((goalEnemy == null) ? BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(Vector3_0, BotOwner_0, gStruct, placeId, out sDist) : BotOwner_0.BotsGroup.CoverPointMaster.ClosestGreenPoint(Vector3_0, BotOwner_0, num2, goalEnemy.CurrPosition, gStruct, out sDist));
			if (customNavigationPoint != null && sDist < (flag ? 2025f : 900f))
			{
				return customNavigationPoint;
			}
		}
		if (goalEnemy == null)
		{
			CustomNavigationPoint curCustomCoverPoint = BotOwner_0.Memory.CurCustomCoverPoint;
			if (curCustomCoverPoint != null && curCustomCoverPoint.IsGoodInsideBuilding && (!placeId.HasValue || curCustomCoverPoint.PlaceId == placeId.Value))
			{
				return curCustomCoverPoint;
			}
			CustomNavigationPoint customNavigationPoint2 = BotOwner_0.Covers.FindHidePoint(Vector3_0, 0f, gStruct, onlyWithInsideCover: true, placeId);
			if (customNavigationPoint2 != null)
			{
				return customNavigationPoint2;
			}
			customNavigationPoint2 = BotOwner_0.Covers.FindHidePoint(Vector3_0, 0f, gStruct, onlyWithInsideCover: false, placeId);
			if (customNavigationPoint2 != null)
			{
				return customNavigationPoint2;
			}
		}
		Vector3 vector = ((goalEnemy == null) ? Vector3_0 : BotOwner_0.Position);
		CoverSearchType searchType = ((goalEnemy != null) ? CoverSearchType.distToBot : CoverSearchType.closerToSelectedPoint);
		CoverSearchData coverSearchData = new CoverSearchData(vector, BotOwner_0.CoverSearchInfo, CoverShootType.hide, 16f, 0f, searchType, null, closeFriendCover, vector, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(0f));
		coverSearchData.CenterPos = vector;
		if (Bool_6)
		{
			coverSearchData.UseAngCastToCover = true;
			coverSearchData.UseLineCastToCover = true;
		}
		coverSearchData.MinSDistToCarePos = num2 * num2;
		return base.FindPoint(coverSearchData, p, checkCurrent);
	}

	public bool CanRunToEnemyWithShortPath()
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return false;
		}
		if (Float_23 < Time.time)
		{
			Float_23 = Time.time + 1f;
			NavMeshPath navMeshPath = new NavMeshPath();
			NavMesh.CalculatePath(BotOwner_0.Position, goalEnemy.CurrPosition, -1, navMeshPath);
			if (navMeshPath.status == NavMeshPathStatus.PathComplete)
			{
				float num = GClass371.CalculatePathLength(navMeshPath);
				Bool_4 = num < Float_11 * 1.5f;
				if (Bool_6 && !goalEnemy.IsVisible && num > Single_0)
				{
					return false;
				}
			}
			else
			{
				Bool_4 = false;
			}
		}
		return Bool_4;
	}

	public bool IsCoverGoodForRun(CustomNavigationPoint targetCustomNavigPoint)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy == null)
		{
			return true;
		}
		if (!((targetCustomNavigPoint.Position - goalEnemy.CurrPosition).sqrMagnitude > 900f) && goalEnemy.IsVisible)
		{
			return false;
		}
		return true;
	}

	public override AICoreActionEndStruct EndRunToCover()
	{
		if (BotOwner_0.Memory.IsInCover)
		{
			BotOwner_0.BotRun.EndMove();
			return new AICoreActionEndStruct("in cover");
		}
		if (!BotOwner_0.CanSprintPlayer)
		{
			BotOwner_0.BotRun.EndMove();
			return new AICoreActionEndStruct("can'tsprint");
		}
		if (BotOwner_0.Memory.CurCustomCoverPoint != null && BotOwner_0.Memory.CurCustomCoverPoint.IsSpotted)
		{
			BotOwner_0.BotRun.EndMove();
			return new AICoreActionEndStruct("coverspotte");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndHoldPosition()
	{
		if (method_20())
		{
			if (!BotOwner_0.Memory.IsInCover)
			{
				return new AICoreActionEndStruct("!IsInCover");
			}
			return AICoreActionEndStruct_1;
		}
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (goalEnemy != null)
		{
			if (Float_25 > Time.time && goalEnemy.Distance < 40f)
			{
				return new AICoreActionEndStruct("REVANGEMELE");
			}
			if (Time.time - Float_20 < 10f)
			{
				return new AICoreActionEndStruct("bulletNearD");
			}
			BotOwner_0.EnemyLookData.DoCheck();
			if (BotOwner_0.EnemyLookData.IsEnemyLookAtMeForPeriod(2f))
			{
				Gclass448_0.AllFollowersDoAttack();
				return new AICoreActionEndStruct("LOOKTOSECTA");
			}
			if (goalEnemy.Distance < 15f && method_17())
			{
				if (Vector3.Dot(goalEnemy.Direction, goalEnemy.Person.LookDirection) > 0f)
				{
					if (CanRunToEnemyWithShortPath())
					{
						return new AICoreActionEndStruct("RunEnemyPat");
					}
					return AICoreActionEndStruct_1;
				}
				if (goalEnemy.Distance < 8f)
				{
					if (CanRunToEnemyWithShortPath())
					{
						return new AICoreActionEndStruct("EnemyShortP");
					}
					return AICoreActionEndStruct_1;
				}
			}
		}
		if (!BotOwner_0.Memory.IsInCover)
		{
			return new AICoreActionEndStruct("!IsInCover");
		}
		return AICoreActionEndStruct_1;
	}

	public override AICoreActionEndStruct EndGoToCoverPoint()
	{
		return base.EndGoToCoverPoint();
	}

	public override AICoreActionEndStruct EndOneMeleeAttack()
	{
		if (Time.time - Float_22 > 9f)
		{
			Gclass360_0.SetAttackWithDelay(0f);
			return new AICoreActionEndStruct("MAXKNIFEATT");
		}
		return base.EndOneMeleeAttack();
	}

	public void method_14(BotOwner arg1, Player trg)
	{
	}

	public void method_15()
	{
		Float_24 = Time.time + 5f;
		Bool_5 = true;
	}

	public void method_16(BotOwner bot, IPlayer source)
	{
		Float_20 = Time.time;
	}

	public bool method_17()
	{
		float num = Time.time - BotOwner_0.WeaponManager.Melee.LastTimeEnemyHit;
		if (BotOwner_0.Memory.GoalEnemy.Distance < 3f && num > 0.5f)
		{
			return true;
		}
		if (num < 10f)
		{
			return false;
		}
		if (method_18() && BotOwner_0.Memory.GoalEnemy.Distance < 15f)
		{
			return true;
		}
		return Gclass448_0.CanHit(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.GoalEnemy.Person.ProfileId));
	}

	public bool method_18()
	{
		CustomNavigationPoint covPoint = BotOwner_0.Memory.BotCurrentCoverInfo.CovPoint;
		if (covPoint == null)
		{
			return true;
		}
		if (covPoint.CoverType == CoverType.Foliage)
		{
			return false;
		}
		if (method_19(covPoint))
		{
			return false;
		}
		return true;
	}

	public bool method_19(CustomNavigationPoint pointOfSearch)
	{
		bool useAng = !Bool_6;
		HashSet<Vector3> positionsIMustCare = BotOwner_0.Covers.CarePositions();
		return pointOfSearch.CanIHide(positionsIMustCare, 0f, useRaycast: true, useAng);
	}

	public bool method_20()
	{
		if (BotOwner_0.Memory.GoalEnemy.Person.AIData.IsInside && !BotOwner_0.AIData.IsInside)
		{
			return true;
		}
		return false;
	}

	public BotLogicDecision method_21(bool anyway = false)
	{
		EnemyInfo goalEnemy = BotOwner_0.Memory.GoalEnemy;
		if (!anyway && !method_17())
		{
			if (BotOwner_0.Memory.IsInCover && IsCoverGoodForRun(BotOwner_0.Memory.CurCustomCoverPoint))
			{
				return BotLogicDecision.holdPosition;
			}
			if (goalEnemy != null && goalEnemy.Distance < 10f && GClass856.IsTrue100(50f))
			{
				return BotLogicDecision.runToCoverZigZag;
			}
			return BotLogicDecision.runToCover;
		}
		if (method_20())
		{
			if (BotOwner_0.Memory.IsInCover)
			{
				return BotLogicDecision.holdPosition;
			}
			return BotLogicDecision.runToCover;
		}
		Float_22 = Time.time;
		if (BotOwner_0.Memory.IsInCover)
		{
			BotOwner_0.Memory.Spotted(byHit: false, null, 10f);
		}
		method_15();
		if (Gclass448_0 != null)
		{
			Gclass448_0.StartAttackWithKnife(BotOwner_0, BotOwner_0.Memory.GoalEnemy.CurrPosition);
			Gclass448_0.StartHitPLayer(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(BotOwner_0.Memory.GoalEnemy.Person.ProfileId));
		}
		return BotLogicDecision.oneMeleeAttack;
	}

	public override void Dispose()
	{
		BotOwner_0.WeaponManager.Melee.OnEnemyHitted -= method_14;
		BotOwner_0.Memory.OnBulletNear -= method_16;
		base.Dispose();
	}
}
