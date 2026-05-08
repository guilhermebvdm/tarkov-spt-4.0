using System;
using EFT;
using JetBrains.Annotations;
using UnityEngine;

public class BotAttackManager
{
	[NonSerialized]
	public const float UPDATE_TIME = 1f;

	[NonSerialized]
	public BotOwner Owner;

	[NonSerialized]
	public float UpdatePointTimer;

	[field: NonSerialized]
	public float LastFailSearch { get; set; }

	public BotAttackManager(BotOwner owner)
	{
		Owner = owner;
	}

	public void TryPointGetting(bool withShoot, CoverSearchType searchType, ShootPointClass shoot2point, float deistStartSearchSqr, Action<CustomNavigationPoint> OnFind, Action OnFail = null, bool checkCurrent = true)
	{
		CoverShootType coverShootType = ((!withShoot) ? CoverShootType.hide : CoverShootType.shoot);
		TryPointGetting(Owner.Transform.position, coverShootType, deistStartSearchSqr, searchType, shoot2point, OnFind, OnFail, checkCurrent);
	}

	public void TryPointGetting(CoverShootType coverShootType, CoverSearchType searchType, ShootPointClass shoot2point, float deistStartSearchSqr, Action<CustomNavigationPoint> OnFind, Action OnFail = null, bool checkCurrent = true, bool canChange = true)
	{
		TryPointGetting(Owner.Transform.position, coverShootType, deistStartSearchSqr, searchType, shoot2point, OnFind, OnFail, checkCurrent, recalcNoTimer: false, canChange);
	}

	public void TryPointGetting(Vector3 centerPos, bool withShoot, float distStart2SerachSqr, CoverSearchType searchType, ShootPointClass shoot2point, Action<CustomNavigationPoint> OnFind, Action OnFail = null, bool checkCurrent = true)
	{
		CoverShootType coverShootType = ((!withShoot) ? CoverShootType.hide : CoverShootType.shoot);
		TryPointGetting(centerPos, coverShootType, distStart2SerachSqr, searchType, shoot2point, OnFind, OnFail, checkCurrent);
	}

	public void TryPointGetting(CoverSearchData searchData, Action<CustomNavigationPoint> OnFind, Action OnFail)
	{
		if (UpdatePointTimer < Time.time || !Owner.Memory.BotCurrentCoverInfo.HaveCover)
		{
			UpdatePointTimer = Time.time + 1f;
			CustomNavigationPoint coverPointMain = Owner.BotsGroup.CoverPointMaster.GetCoverPointMain(searchData, checkCurrent: true);
			if (coverPointMain != null)
			{
				OnFind?.Invoke(coverPointMain);
			}
			else
			{
				OnFail?.Invoke();
			}
		}
	}

	public void TryPointGetting(Vector3 centerPos, CoverShootType coverShootType, float distStart2SerachSqr, CoverSearchType searchType, ShootPointClass shoot2point, Action<CustomNavigationPoint> OnFind, Action OnFail, bool checkCurrent = true, bool recalcNoTimer = false, bool useFightLogic = true, int? placeInfo = null)
	{
		if (UpdatePointTimer < Time.time || !Owner.Memory.BotCurrentCoverInfo.HaveCover || recalcNoTimer)
		{
			UpdatePointTimer = Time.time + 1f;
			Vector3? closeFriendCover = Owner.Covers.ClosestFriendCoverPoint();
			CoverSearchData data = new CoverSearchData(centerPos, Owner.CoverSearchInfo, coverShootType, distStart2SerachSqr, 0f, searchType, shoot2point, closeFriendCover, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(Owner.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL), PointsArrayType.byShootType, useFightLogic, placeInfo);
			CustomNavigationPoint coverPointMain = Owner.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: true);
			if (coverPointMain != null)
			{
				OnFind?.Invoke(coverPointMain);
				return;
			}
			LastFailSearch = Time.time;
			OnFail?.Invoke();
		}
	}

	public void PointAndPathDetecting()
	{
		Owner.GoToPoint(Owner.Memory.CurCustomCoverPoint);
	}

	public void UpdateNextTick()
	{
		UpdatePointTimer = Time.time - 1f;
	}

	[CanBeNull]
	public CustomNavigationPoint PointToGo()
	{
		bool num = Owner.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack);
		bool flag = Owner.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect);
		ShootPointClass shootPointClass = null;
		bool num2 = num || flag;
		CoverShootType coverShootType = ((!num2) ? CoverShootType.hide : CoverShootType.shoot);
		if (num2)
		{
			if (Owner.Memory.GoalEnemy == null)
			{
				coverShootType = CoverShootType.hide;
			}
			else if (Time.time - Owner.BotsGroup.EnemyLastSeenTimeReal > Owner.Settings.FileSettings.Mind.FIND_COVER_TO_GET_POSITION_WITH_SHOOT)
			{
				coverShootType = CoverShootType.hide;
			}
		}
		if (coverShootType == CoverShootType.shoot)
		{
			shootPointClass = Owner.CurrentEnemyTargetPosition(flag);
			if (shootPointClass == null)
			{
				coverShootType = CoverShootType.hide;
			}
		}
		CoverSearchType searchType = Owner.Tactic.SubTactic.SearchTypeForCover(coverShootType);
		if (Owner.Medecine.FirstAid.Have2Do)
		{
			searchType = CoverSearchType.distToBot;
		}
		Vector3? closeFriendCover = Owner.Covers.ClosestFriendCoverPoint();
		float sTART_DIST_TO_COV = LocalBotSettingsProviderClass.Core.START_DIST_TO_COV;
		if (Owner.Memory.GoalEnemy != null)
		{
			Vector3 position = Owner.Transform.position;
			Vector3 enemyLastPosition = Owner.Memory.GoalEnemy.EnemyLastPosition;
			CoverSearchData data = new CoverSearchData((coverShootType != CoverShootType.hide) ? ((position + enemyLastPosition) / 2f) : position, Owner.CoverSearchInfo, coverShootType, sTART_DIST_TO_COV, 0f, searchType, shootPointClass, closeFriendCover, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(Owner.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
			return Owner.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: true);
		}
		Vector3 centerPos = Owner.Position;
		if (Owner.BotFollower.NeedToProtectBoss && Owner.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Protect))
		{
			centerPos = Owner.BotFollower.BossToFollow.PositionOrTargetCover;
		}
		CoverSearchData data2 = new CoverSearchData(centerPos, Owner.CoverSearchInfo, coverShootType, sTART_DIST_TO_COV, 0f, searchType, shootPointClass, closeFriendCover, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(Owner.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
		return Owner.BotsGroup.CoverPointMaster.GetCoverPointMain(data2, checkCurrent: true);
	}

	[CanBeNull]
	public CustomNavigationPoint CheckDestinations()
	{
		bool num = Owner.Tactic.IsCurTactic(BotsGroup.BotCurrentTactic.Attack);
		CoverShootType shootType = CoverShootType.hide;
		ShootPointClass shootPointClass = Owner.CurrentEnemyTargetPosition(sensPosition: true);
		if (num)
		{
			shootType = ((shootPointClass == null) ? CoverShootType.hide : CoverShootType.shoot);
		}
		Vector3? closeFriendCover = Owner.Covers.ClosestFriendCoverPoint();
		CoverSearchData data = new CoverSearchData(Owner.Position, Owner.CoverSearchInfo, shootType, LocalBotSettingsProviderClass.Core.START_DIST_TO_COV, 0f, CoverSearchType.shoot_toCover_toBot_Distances, shootPointClass, closeFriendCover, null, ECheckSHootHide.shootAndHide, new CoverSearchDefenceDataClass(Owner.Settings.FileSettings.Cover.MIN_DEFENCE_LEVEL));
		return Owner.BotsGroup.CoverPointMaster.GetCoverPointMain(data, checkCurrent: true);
	}
}
